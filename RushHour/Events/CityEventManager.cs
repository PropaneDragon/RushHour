using ColossalFramework;
using RushHour.CimTools;
using RushHour.Experiments;
using RushHour.Message;
using System;
using UnityEngine;
using CimTools.V1.File;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace RushHour.Events
{
    public class CityEventManager
    {
        private static CityEventManager m_instance = null;
        private static float m_lastDayTimeHour = 0F;
        private static DateTime m_baseTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
        private static DateTime m_nextEventCheck = DateTime.Now.AddDays(-10);
        
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

        public List<CityEventXml> m_xmlEvents = new List<CityEventXml>();
        public List<CityEvent> m_nextEvents = new List<CityEvent>();

        public CityEventManager()
        {
            CITY_TIME = m_baseTime;

            LoadEvents();
        }

        public void UpdateTime(int year, int month, int day)
        {
            m_baseTime = new DateTime(year, month, day);
            CITY_TIME = new DateTime(year, month, day);
        }

        private void LoadEvents()
        {
            string modPath = CimToolsHandler.CimToolBase.Path.GetModPath();

            if (modPath != null && modPath != "" && Directory.Exists(modPath))
            {
                DirectoryInfo parentPath = Directory.GetParent(modPath);
                string searchDirectory = parentPath.FullName;

                string[] allModDirectories = Directory.GetDirectories(searchDirectory);

                foreach (string modDirectory in allModDirectories)
                {
                    if (Directory.Exists(modDirectory))
                    {
                        string rushHourDirectory = modDirectory + "/RushHour Events";

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
                                            CimToolsHandler.CimToolBase.DetailedLogger.Log("Successfully found and loaded " + foundEventFile);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        CimToolsHandler.CimToolBase.DetailedLogger.LogError(ex.Message + "\n" + ex.StackTrace);
                                    }
                                }
                            }
                        }
                        else
                        {
                            CimToolsHandler.CimToolBase.DetailedLogger.Log("No events directory in " + rushHourDirectory);
                        }
                    }
                    else
                    {
                        CimToolsHandler.CimToolBase.DetailedLogger.LogWarning("Directory " + modDirectory + " doesn't exist.");
                    }
                }
            }
            else
            {
                CimToolsHandler.CimToolBase.DetailedLogger.LogWarning("Could not find mod path at " + modPath ?? "null");
            }
        }

        public void Update()
        {
            SimulationManager _simulationManager = Singleton<SimulationManager>.instance;
            float currentHour = _simulationManager.m_currentDayTimeHour;

            if(currentHour < 1D && m_lastDayTimeHour > 23D)
            {
                m_baseTime = m_baseTime.AddDays(1D);
                CimToolsHandler.CimToolBase.DetailedLogger.Log("New day " + m_baseTime.ToShortDateString() + " " + m_baseTime.ToShortTimeString());

                CimToolsHandler.CimToolBase.SaveFileOptions.Data.SetValue("CityTimeYear", m_baseTime.Year);
                CimToolsHandler.CimToolBase.SaveFileOptions.Data.SetValue("CityTimeMonth", m_baseTime.Month);
                CimToolsHandler.CimToolBase.SaveFileOptions.Data.SetValue("CityTimeDay", m_baseTime.Day);

                Debug.Log("Current date: " + m_baseTime.ToLongTimeString() + ", " + m_baseTime.ToShortDateString());
            }

            m_lastDayTimeHour = currentHour;

            CITY_TIME = m_baseTime.AddHours(currentHour);

            CheckEventStartDate();
        }

        private void CheckEventStartDate()
        {
            if(ExperimentsToggle.EnableRandomEvents && m_nextEvents.Count == 0 && m_nextEventCheck < CITY_TIME) //Can be changed later for more events at the same time
            {
                SimulationManager _simulationManager = Singleton<SimulationManager>.instance;
                BuildingManager _buildingManager = Singleton<BuildingManager>.instance;

                FastList<ushort> monuments = _buildingManager.GetServiceBuildings(ItemClass.Service.Monument);

                if (ExperimentsToggle.PrintAllMonuments)
                {
                    Debug.Log("Available monuments:");
                    foreach (ushort monumentId in monuments.m_buffer)
                    {
                        Building monument = _buildingManager.m_buildings.m_buffer[monumentId];
                        Debug.Log(monument.Info.name);
                        CimToolsHandler.CimToolBase.DetailedLogger.Log(monument.Info.name);
                    }
                }

                if (monuments.m_size > 0)
                {
                    ushort randomMonumentId = monuments.m_buffer[_simulationManager.m_randomizer.UInt32((uint)monuments.m_size)];

                    if (randomMonumentId < _buildingManager.m_buildings.m_size)
                    {
                        Building monument = _buildingManager.m_buildings.m_buffer[randomMonumentId];
                        CityEvent foundEvent = CityEventBuildings.instance.GetEventForBuilding(ref monument);

                        if (foundEvent != null)
                        {
                            foundEvent.SetUp(ref randomMonumentId);
                            m_nextEvents.Add(foundEvent);

                            MessageManager _messageManager = Singleton<MessageManager>.instance;
                            _messageManager.QueueMessage(new CitizenCustomMessage(_messageManager.GetRandomResidentID(), foundEvent.GetCitizenMessageInitialised()));

                            CimToolsHandler.CimToolBase.DetailedLogger.Log("Event created at " + monument.Info.name + " for " + foundEvent.m_eventData.m_eventStartTime.ToShortDateString() + ". Current date: " + CITY_TIME.ToShortDateString());

                            Debug.Log("Event starting at " + foundEvent.m_eventData.m_eventStartTime.ToLongTimeString() + ", " + foundEvent.m_eventData.m_eventStartTime.ToShortDateString());
                            Debug.Log("Event building is " + monument.Info.name);
                            Debug.Log("Current date: " + CITY_TIME.ToLongTimeString() + ", " + CITY_TIME.ToShortDateString());
                        }
                        else
                        {
                            Debug.Log("No event scheduled just yet. Checking again soon.");
                        }
                    }
                }

                if (!ExperimentsToggle.ForceEventToHappen)
                {
                    m_nextEventCheck = CITY_TIME.AddHours(3);
                }
            }
            else
            {
                for(int index = 0; index < m_nextEvents.Count; ++index)
                {
                    if (m_nextEvents[index].m_eventData.m_eventEnded && (CITY_TIME - m_nextEvents[index].m_eventData.m_eventFinishTime).TotalHours > 4D)
                    {
                        m_nextEvents.RemoveAt(index);
                        --index;

                        Debug.Log("Event finished");
                    }
                    else
                    {
                        m_nextEvents[index].Update();
                    }
                }
            }
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

        public int EventStartsWithin(uint citizenID, ref Citizen person, double hours, bool countStarted = false)
        {
            int foundEventIndex = -1;

            for(int index = 0; index < m_nextEvents.Count; ++index)
            {
                CityEvent thisEvent = m_nextEvents[index];

                if ((thisEvent.EventStartsWithin(hours) && thisEvent.CitizenCanGo(citizenID, ref person) || (countStarted && thisEvent.m_eventData.m_eventStarted)))
                {
                    foundEventIndex = index;
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
                    return m_nextEvents[index].m_eventData.m_eventStarted;
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
    }
}