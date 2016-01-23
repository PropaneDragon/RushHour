using ColossalFramework;
using System;
using UnityEngine;

namespace RushHour.Places
{
    public class CityEventManager
    {
        private static CityEventManager m_instance = null;
        private static float m_lastDayTimeHour = 0F;
        private static DateTime m_baseTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);

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

        private DateTime? m_nextEventStartTime = null;
        private DateTime m_eventFinishTime;

        public bool m_eventStarted = false;
        public bool m_eventFinished = true;
        public ushort m_eventBuilding = 0;

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
            if(m_nextEventStartTime == null)
            {
                SimulationManager _simulationManager = Singleton<SimulationManager>.instance;
                BuildingManager _buildingManager = Singleton<BuildingManager>.instance;

                FastList<ushort> monuments = _buildingManager.GetServiceBuildings(ItemClass.Service.Monument);

                if (monuments.m_size > 0)
                {
                    ushort randomMonumentId = monuments.m_buffer[_simulationManager.m_randomizer.UInt32((uint)monuments.m_size)];

                    if (randomMonumentId < _buildingManager.m_buildings.m_size)
                    {
                        Building monument = _buildingManager.m_buildings.m_buffer[randomMonumentId];

                        //monument.Info.name <- check this at some point so we have only the building we want

                        m_eventBuilding = randomMonumentId;

                        int dayOffset = _simulationManager.m_randomizer.Int32(1, 7);
                        int startHour = _simulationManager.m_randomizer.Int32(19, 23);

                        m_nextEventStartTime = new DateTime(CITY_TIME.Year, CITY_TIME.Month, CITY_TIME.Day, startHour, 0, 0).AddDays(dayOffset);
                        m_eventFinished = false;

                        Debug.Log("Event starting at " + m_nextEventStartTime.Value.ToLongTimeString() + ", " + m_nextEventStartTime.Value.ToShortDateString());
                        Debug.Log("Event building is " + monument.Info.name);
                        Debug.Log("Current date: " + CITY_TIME.ToLongTimeString() + ", " + CITY_TIME.ToShortDateString());
                    }
                }
            }
            else if(CITY_TIME > m_nextEventStartTime && !m_eventStarted)
            {
                StartRandomEvent();
            }
            else if(m_eventStarted && CITY_TIME > m_eventFinishTime)
            {
                m_eventFinished = true;
                m_eventStarted = false;
                m_nextEventStartTime = null;

                Debug.Log("Event finished!");
                Debug.Log("Current date: " + CITY_TIME.ToLongTimeString() + ", " + CITY_TIME.ToShortDateString());
            }
        }

        private void StartRandomEvent()
        {
            SimulationManager _simulationManager = Singleton<SimulationManager>.instance;

            m_eventFinishTime = m_nextEventStartTime.Value.AddHours(_simulationManager.m_randomizer.Int32(1, 3));
            m_eventStarted = true;
        }

        public bool EventStartsWithin(double hours)
        {
            bool eventStartsSoon = false;

            if (m_nextEventStartTime != null && !m_eventStarted)
            {
                TimeSpan difference = CITY_TIME - m_nextEventStartTime.Value;
                eventStartsSoon = difference.TotalHours <= hours;
            }

            return eventStartsSoon;
        }

        public bool EventEndsWithin(double hours)
        {
            bool eventEndsSoon = false;

            if(m_eventStarted && !m_eventFinished)
            {
                TimeSpan difference = CITY_TIME - m_eventFinishTime;
                eventEndsSoon = difference.TotalHours <= hours;
            }

            return eventEndsSoon;
        }
    }
}