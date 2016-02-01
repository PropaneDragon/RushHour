using ColossalFramework;
using RushHour.CimTools;
using RushHour.Experiments;
using RushHour.Message;
using System;
using UnityEngine;
using CimTools.V1.File;
using System.Collections.Generic;

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

        public List<CityEvent> m_nextEvents = new List<CityEvent>();

        public CityEventManager()
        {
            bool loaded = true;
            int year = 0, month = 0, day = 0;

            loaded = loaded && CimToolsHandler.CimToolBase.SaveFileOptions.Data.GetValue("CityTimeYear", ref year) == ExportOptionBase.OptionError.NoError;
            loaded = loaded && CimToolsHandler.CimToolBase.SaveFileOptions.Data.GetValue("CityTimeMonth", ref month) == ExportOptionBase.OptionError.NoError;
            loaded = loaded && CimToolsHandler.CimToolBase.SaveFileOptions.Data.GetValue("CityTimeDay", ref day) == ExportOptionBase.OptionError.NoError;

            if (loaded)
            {
                m_baseTime = new DateTime(year, month, day);
                CITY_TIME = new DateTime(year, month, day);
            }
            else
            {
                Debug.LogWarning("Rush hour: Couldn't extract date from save file.");

                CimToolsHandler.CimToolBase.SaveFileOptions.Data.SetValue("CityTimeYear", m_baseTime.Year);
                CimToolsHandler.CimToolBase.SaveFileOptions.Data.SetValue("CityTimeMonth", m_baseTime.Month);
                CimToolsHandler.CimToolBase.SaveFileOptions.Data.SetValue("CityTimeDay", m_baseTime.Day);
            }
        }

        public void Update()
        {
            SimulationManager _simulationManager = Singleton<SimulationManager>.instance;
            float currentHour = _simulationManager.m_currentDayTimeHour;

            if(currentHour < 1D && m_lastDayTimeHour > 23D)
            {
                m_baseTime = m_baseTime.AddDays(1D);

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
                    foreach (ushort monumentId in monuments.m_buffer)
                    {
                        Building monument = _buildingManager.m_buildings.m_buffer[monumentId];
                        Debug.Log(monument.Info.name);
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

                if (thisEvent.CitizenCanGo(citizenID, ref person) && (thisEvent.EventStartsWithin(hours) || (countStarted && thisEvent.m_eventData.m_eventStarted)))
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