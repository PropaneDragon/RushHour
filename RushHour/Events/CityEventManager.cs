using ColossalFramework;
using RushHour.Experiments;
using RushHour.Message;
using System;
using UnityEngine;

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

        public FastList<CityEvent> m_nextEvents = new FastList<CityEvent>();

        public void Update()
        {
            SimulationManager _simulationManager = Singleton<SimulationManager>.instance;
            float currentHour = _simulationManager.m_currentDayTimeHour;

            if(currentHour < 1D && m_lastDayTimeHour > 23D)
            {
                m_baseTime = m_baseTime.AddDays(1D);
                Debug.Log("Current date: " + m_baseTime.ToLongTimeString() + ", " + m_baseTime.ToShortDateString());
            }

            m_lastDayTimeHour = currentHour;

            CITY_TIME = m_baseTime.AddHours(currentHour);

            CheckEventStartDate();
        }

        private void CheckEventStartDate()
        {
            if(m_nextEvents.m_size == 0 && m_nextEventCheck < CITY_TIME) //Can be changed later for more events at the same time
            {
                SimulationManager _simulationManager = Singleton<SimulationManager>.instance;
                BuildingManager _buildingManager = Singleton<BuildingManager>.instance;

                FastList<ushort> monuments = _buildingManager.GetServiceBuildings(ItemClass.Service.Monument);

                if (ExperimentsToggle.PrintAllMonuments())
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

                            Debug.Log("Event starting at " + foundEvent.m_eventStartTime.ToLongTimeString() + ", " + foundEvent.m_eventStartTime.ToShortDateString());
                            Debug.Log("Event building is " + monument.Info.name);
                            Debug.Log("Current date: " + CITY_TIME.ToLongTimeString() + ", " + CITY_TIME.ToShortDateString());
                        }
                        else
                        {
                            Debug.Log("No event scheduled just yet. Checking again soon.");
                        }
                    }
                }

                if (!ExperimentsToggle.ForceEventToHappen())
                {
                    m_nextEventCheck = CITY_TIME.AddHours(3);
                }
            }
            else
            {
                for(int index = 0; index < m_nextEvents.m_size; ++index)
                {
                    if (m_nextEvents.m_buffer[index].m_eventEnded)
                    {
                        m_nextEvents.RemoveAt(index);
                        --index;

                        Debug.Log("Event finished");
                    }
                    else
                    {
                        m_nextEvents.m_buffer[index].Update();
                    }
                }
            }
        }

        public bool EventStartsWithin(double hours)
        {
            for (int index = 0; index < m_nextEvents.m_size; ++index)
            {
                CityEvent thisEvent = m_nextEvents.m_buffer[index];

                if (thisEvent.EventStartsWithin(hours))
                {
                    return true;
                }
            }

            return false;
        }

        public int EventStartsWithin(uint citizenID, ref Citizen person, double hours)
        {
            int foundEventIndex = -1;

            for(int index = 0; index < m_nextEvents.m_size; ++index)
            {
                CityEvent thisEvent = m_nextEvents.m_buffer[index];

                if (thisEvent.CitizenCanGo(citizenID, ref person) && thisEvent.EventStartsWithin(hours))
                {
                    foundEventIndex = index;
                }
            }

            return foundEventIndex;
        }

        public bool EventTakingPlace()
        {
            for (int index = 0; index < m_nextEvents.m_size; ++index)
            {
                if(m_nextEvents.m_buffer[index].m_eventStarted)
                {
                    return true;
                }
            }

            return false;
        }

        public bool EventTakingPlace(ushort buildingID)
        {
            for(int index = 0; index < m_nextEvents.m_size; ++index)
            {
                if (m_nextEvents.m_buffer[index].m_eventBuilding == buildingID)
                {
                    return m_nextEvents.m_buffer[index].m_eventStarted;
                }
            }

            return false;
        }
    }
}