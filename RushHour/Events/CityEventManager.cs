using ColossalFramework;
using RushHour.Experiments;
using RushHour.Message;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using RushHour.Events.Unique;
using RushHour.Logging;

namespace RushHour.Events
{
    internal class CityEventManager
    {
        private static CityEventManager m_instance = null;
        private static float m_lastDayTimeHour = 0F;
        private static DateTime m_baseTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
        private static DateTime m_nextEventCheck = DateTime.MinValue;
        
        public static DateTime CITY_TIME = m_baseTime;
        public static CityEventManager instance
        {
            get
            {
                if(m_instance == null)
                {
                    m_instance = new CityEventManager();
                }

                return m_instance;
            }
        }

        public CityEventXmlContainer m_forcedEvent = null;
        public List<CityEventXml> m_xmlEvents = new List<CityEventXml>();
        public List<CityEvent> m_nextEvents = new List<CityEvent>();

        private double _eventEndBuffer = 2d; //Time after an event to wait before removing it

        public CityEventManager()
        {
            CITY_TIME = m_baseTime;

            LoadEvents();
        }

        public void UpdateTime()
        {
            m_baseTime = new DateTime(Data.CityTime.year, Data.CityTime.month, Data.CityTime.day);
            CITY_TIME = new DateTime(Data.CityTime.year, Data.CityTime.month, Data.CityTime.day);
        }

        private void LoadEvents()
        {
            PrintMonuments();

            string modPath = CimTools.CimToolsHandler.CimToolBase.Path.GetModPath();

            if (modPath != null && modPath != "" && Directory.Exists(modPath))
            {
                DirectoryInfo parentPath = Directory.GetParent(modPath);
                string searchDirectory = parentPath.FullName;

                string[] allModDirectories = Directory.GetDirectories(searchDirectory);

                foreach (string modDirectory in allModDirectories)
                {
                    if (Directory.Exists(modDirectory))
                    {
                        string rushHourDirectory = modDirectory + System.IO.Path.DirectorySeparatorChar + "RushHour Events";

                        if (Directory.Exists(rushHourDirectory))
                        {
                            string[] eventFiles = Directory.GetFiles(rushHourDirectory, "*.xml");

                            foreach (string foundEventFile in eventFiles)
                            {
                                if (File.Exists(foundEventFile))
                                {
                                    try
                                    {
                                        XmlSerializer eventDeserialiser = new XmlSerializer(typeof(CityEventXml));
                                        TextReader eventReader = new StreamReader(foundEventFile);

                                        CityEventXml loadedXmlEvent = eventDeserialiser.Deserialize(eventReader) as CityEventXml;

                                        if (loadedXmlEvent != null)
                                        {
                                            m_xmlEvents.Add(loadedXmlEvent);

                                            if (ExperimentsToggle.AllowForcedXMLEvents)
                                            {
                                                foreach (CityEventXmlContainer individualEvent in loadedXmlEvent._containedEvents)
                                                {
                                                    if (individualEvent._force)
                                                    {
                                                        m_forcedEvent = individualEvent;
                                                        LoggingWrapper.Log("Forcing event " + individualEvent._name);
                                                    }
                                                }
                                            }

                                            LoggingWrapper.Log("Successfully found and loaded " + foundEventFile);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        LoggingWrapper.LogError(ex.Message + "\n" + ex.StackTrace);
                                    }
                                }
                            }
                        }
                        else
                        {
                            LoggingWrapper.Log("No events directory in " + rushHourDirectory);
                        }
                    }
                    else
                    {
                        LoggingWrapper.LogWarning("Directory " + modDirectory + " doesn't exist.");
                    }
                }
            }
            else
            {
                LoggingWrapper.LogWarning("Could not find mod path at " + modPath ?? "null");
            }
        }

        public void Update()
        {
            SimulationManager _simulationManager = Singleton<SimulationManager>.instance;
            float currentHour = _simulationManager.m_currentDayTimeHour;

            if(currentHour < 1D && m_lastDayTimeHour > 23D)
            {
                m_baseTime = m_baseTime.AddDays(1D);
                LoggingWrapper.Log("New day " + m_baseTime.ToShortDateString() + " " + m_baseTime.ToShortTimeString());

                Data.CityTime.year = m_baseTime.Year;
                Data.CityTime.month = m_baseTime.Month;
                Data.CityTime.day = m_baseTime.Day;

                Debug.Log("Current date: " + m_baseTime.ToLongTimeString() + ", " + m_baseTime.ToShortDateString());
            }

            if (currentHour > 23D && m_lastDayTimeHour < 1D)
            {
                LoggingWrapper.LogWarning("Time jumped back, but it was prevented.");
            }
            else
            {
                m_lastDayTimeHour = currentHour;
            }

            CITY_TIME = m_baseTime.AddHours(currentHour);

            if(ExperimentsToggle.AllowForcedXMLEvents)
            {
                m_xmlEvents.Clear();
                LoadEvents();
                ExperimentsToggle.AllowForcedXMLEvents = false;
            }

            UpdateEvents();
        }

        private void UpdateEvents()
        {
            if((ExperimentsToggle.EnableRandomEvents && m_nextEvents.Count < ExperimentsToggle.MaxConcurrentEvents && m_nextEventCheck < CITY_TIME) || m_forcedEvent != null) //Can be changed later for more events at the same time
            {
                CreateEvents();
            }

            ClearDeadEvents();
        }

        public void CreateEvents()
        {
            SimulationManager _simulationManager = Singleton<SimulationManager>.instance;
            BuildingManager _buildingManager = Singleton<BuildingManager>.instance;

            PrintMonuments();

            if (m_forcedEvent != null)
            {
                m_nextEvents.Clear();
            }
            
            FastList<ushort> allBuildings = CityEventBuildings.instance.GetPotentialEventBuildings();

            if (allBuildings.m_size > 0)
            {
                if (m_forcedEvent != null)
                {
                    for (int index = 0; index < allBuildings.m_size; ++index)
                    {
                        ushort buildingId = allBuildings[index];
                        Building monument = _buildingManager.m_buildings.m_buffer[buildingId];

                        if (monument.Info.name == m_forcedEvent._eventBuildingClassName)
                        {
                            CityEvent xmlEvent = new XmlEvent(m_forcedEvent);
                            xmlEvent.SetUp(ref buildingId);
                            xmlEvent.m_eventData.m_eventStartTime = CITY_TIME.AddHours(4d);
                            xmlEvent.m_eventData.m_eventFinishTime = xmlEvent.m_eventData.m_eventStartTime.AddHours(xmlEvent.GetEventLength());

                            AddEvent(xmlEvent);

                            LoggingWrapper.Log("Forced event created at " + monument.Info.name + " for " + xmlEvent.m_eventData.m_eventStartTime.ToShortTimeString() + ". Current date: " + CITY_TIME.ToShortTimeString());
                        }
                        else
                        {
                            LoggingWrapper.Log(monument.Info.name + " != " + m_forcedEvent._eventBuildingClassName);
                        }
                    }
                }
                else
                {
                    for (int count = 0; count < 10; ++count)
                    {
                        ushort randomMonumentId = allBuildings.m_buffer[_simulationManager.m_randomizer.UInt32((uint)allBuildings.m_size)];

                        if (randomMonumentId < _buildingManager.m_buildings.m_size)
                        {
                            Building monument = _buildingManager.m_buildings.m_buffer[randomMonumentId];
                            CityEvent foundEvent = CityEventBuildings.instance.GetEventForBuilding(ref monument);

                            if (foundEvent != null && (monument.m_flags & Building.Flags.Active) != Building.Flags.None)
                            {
                                foundEvent.SetUp(ref randomMonumentId);
                                AddEvent(foundEvent);
                                break;
                            }
                            else
                            {
                                Debug.Log("No event scheduled just yet. Checking again soon.");
                            }
                        }
                    }
                }
            }

            m_forcedEvent = null;

            if (!ExperimentsToggle.ForceEventToHappen)
            {
                m_nextEventCheck = CITY_TIME.AddHours(3);
            }
        }

        public void ClearDeadEvents()
        {
            bool clearAllEvents = false;

            CimTools.CimToolsHandler.CimToolBase.ModPanelOptions.GetOptionValue("ClearEvents", ref clearAllEvents);

            if(clearAllEvents)
            {
                CimTools.CimToolsHandler.CimToolBase.ModPanelOptions.SetOptionValue("ClearEvents", false);
            }

            if (m_nextEvents.Count > 0)
            {
                for (int index = 0; index < m_nextEvents.Count; ++index)
                {
                    bool clearEvent = false || clearAllEvents;
                    CityEvent thisEvent = m_nextEvents[index];

                    if ((thisEvent.m_eventData.m_eventEnded && (CITY_TIME - thisEvent.m_eventData.m_eventFinishTime).TotalHours > _eventEndBuffer))
                    {
                        clearEvent = true;

                        Debug.Log("Event finished");
                        LoggingWrapper.Log("Event finished");
                    }
                    else if (!thisEvent.m_eventData.m_eventStarted && !thisEvent.m_eventData.m_eventEnded && !thisEvent.EventStartsWithin(24 * 7))
                    {
                        clearEvent = true;

                        Debug.LogWarning("Event had more than a week of buffer. Removed.");
                        LoggingWrapper.LogWarning("Event had more than a week of buffer. Removed.");
                    }

                    if(clearEvent)
                    {
                        m_nextEvents.RemoveAt(index);
                        --index;
                    }
                    else
                    {
                        m_nextEvents[index].Update();
                    }
                }
            }
        }

        public void AddEvent(CityEvent eventToAdd)
        {
            if (eventToAdd != null)
            {
                BuildingManager buildingManager = Singleton<BuildingManager>.instance;
                Building monument = buildingManager.m_buildings.m_buffer[eventToAdd.m_eventData.m_eventBuilding];

                if ((monument.m_flags & Building.Flags.Active) != Building.Flags.None && (monument.m_flags & Building.Flags.Created) != Building.Flags.None)
                {
                    m_nextEvents.Add(eventToAdd);

                    string message = eventToAdd.GetCitizenMessageInitialised();

                    if (message != "")
                    {
                        MessageManager _messageManager = Singleton<MessageManager>.instance;
                        _messageManager.QueueMessage(new CitizenCustomMessage(_messageManager.GetRandomResidentID(), message));
                    }

                    LoggingWrapper.Log("Event created at " + monument.Info.name + " for " + eventToAdd.m_eventData.m_eventStartTime.ToShortDateString() + ". Current date: " + CITY_TIME.ToShortDateString());

                    Debug.Log("Event starting at " + eventToAdd.m_eventData.m_eventStartTime.ToLongTimeString() + ", " + eventToAdd.m_eventData.m_eventStartTime.ToShortDateString());
                    Debug.Log("Event building is " + monument.Info.name);
                    Debug.Log("Current date: " + CITY_TIME.ToLongTimeString() + ", " + CITY_TIME.ToShortDateString());
                }
                else
                {
                    LoggingWrapper.LogError("Couldn't create an event, as the building is inactive or not created!");
                }
            }
            else
            {
                LoggingWrapper.LogError("Couldn't create an event, as it was null!");
            }
        }

        public bool EventStartsBetween(DateTime date, TimeSpan length)
        {
            for (int index = 0; index < m_nextEvents.Count; ++index)
            {
                CityEvent thisEvent = m_nextEvents[index];

                if (thisEvent.EventActiveBetween(date, length))
                {
                    return true;
                }
            }

            return false;
        }

        public bool EventStartsWithin(double hours, bool countStarted = false)
        {
            for (int index = 0; index < m_nextEvents.Count; ++index)
            {
                CityEvent thisEvent = m_nextEvents[index];

                if (thisEvent.EventStartsWithin(hours) || (countStarted && thisEvent.m_eventData.m_eventStarted))
                {
                    return true;
                }
            }

            return false;
        }

        public bool EventStartsWithin(ushort buildingID, double hours, bool countStarted = false)
        {
            for (int index = 0; index < m_nextEvents.Count; ++index)
            {
                CityEvent thisEvent = m_nextEvents[index];

                if (thisEvent.m_eventData.m_eventBuilding == buildingID && (thisEvent.EventStartsWithin(hours) || (countStarted && thisEvent.m_eventData.m_eventStarted)))
                {
                    return true;
                }
            }

            return false;
        }

        public FastList<CityEvent> EventsThatStartWithin(double hours, bool countStarted = false)
        {
            FastList<CityEvent> _eventsWithin = new FastList<CityEvent>();

            for (int index = 0; index < m_nextEvents.Count; ++index)
            {
                CityEvent thisEvent = m_nextEvents[index];

                if (thisEvent.EventStartsWithin(hours) || (countStarted && thisEvent.m_eventData.m_eventStarted))
                {
                    _eventsWithin.Add(thisEvent);
                }
            }

            return _eventsWithin;
        }

        public FastList<EventData> GameEventsThatStartWithin(double hours, bool countStarted = false)
        {
            FastList<EventData> _eventsWithin = new FastList<EventData>();
            EventManager _eventManager = Singleton<EventManager>.instance;

            for(int index = 0; index < _eventManager.m_events.m_size; ++index)
            {
                EventData thisEvent = _eventManager.m_events.m_buffer[index];

                if ((thisEvent.m_flags & EventData.Flags.Created) != EventData.Flags.None)
                {
                    if (GameEventHelpers.EventStartsWithin(thisEvent, hours) || (countStarted && (thisEvent.m_flags & EventData.Flags.Active) != EventData.Flags.None))
                    {
                        _eventsWithin.Add(thisEvent);
                    }
                }
            }

            return _eventsWithin;
        }

        public int EventStartsWithin(uint citizenID, ref Citizen person, double hours, bool countStarted = false)
        {
            int foundEventIndex = -1;

            for(int index = 0; index < m_nextEvents.Count; ++index)
            {
                CityEvent thisEvent = m_nextEvents[index];

                if ((thisEvent.EventStartsWithin(hours) && thisEvent.CitizenCanGo(citizenID, ref person)) || (countStarted && thisEvent.m_eventData.m_eventStarted))
                {
                    foundEventIndex = index;
                    break;
                }
            }

            return foundEventIndex;
        }

        public bool EventTakingPlace()
        {
            for (int index = 0; index < m_nextEvents.Count; ++index)
            {
                if(m_nextEvents[index].m_eventData.m_eventStarted)
                {
                    return true;
                }
            }

            return false;
        }

        public bool EventTakingPlace(ushort buildingID)
        {
            for(int index = 0; index < m_nextEvents.Count; ++index)
            {
                if (m_nextEvents[index].m_eventData.m_eventBuilding == buildingID)
                {
                    if(m_nextEvents[index].m_eventData.m_eventStarted)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool EventJustEnded()
        {
            for (int index = 0; index < m_nextEvents.Count; ++index)
            {
                if (m_nextEvents[index].m_eventData.m_eventEnded)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsWeekend()
        {
            return IsWeekend(CITY_TIME);
        }

        public bool IsWeekend(DateTime timeToCheck)
        {
            return ExperimentsToggle.EnableWeekends && (timeToCheck.DayOfWeek == DayOfWeek.Saturday || timeToCheck.DayOfWeek == DayOfWeek.Sunday);
        }

        public bool IsStillWeekend(int hoursOffset)
        {
            return IsWeekend(CITY_TIME + new TimeSpan(hoursOffset, 0, 0));
        }

        public void PrintMonuments()
        {
            BuildingManager _buildingManager = Singleton<BuildingManager>.instance;

            if (_buildingManager != null)
            {
                FastList<ushort> monuments = _buildingManager.GetServiceBuildings(ItemClass.Service.Monument);

                if (ExperimentsToggle.PrintAllMonuments)
                {
                    Debug.Log("Available monuments:");

                    for (int index = 0; index < monuments.m_size; ++index)
                    {
                        Building monument = _buildingManager.m_buildings.m_buffer[monuments.m_buffer[index]];

                        if ((monument.m_flags & Building.Flags.Created) != Building.Flags.None)
                        {
                            Debug.Log(monument.Info.name);
                            LoggingWrapper.Log(monument.Info.name);
                        }
                    }
                }
            }
        }
    }
}