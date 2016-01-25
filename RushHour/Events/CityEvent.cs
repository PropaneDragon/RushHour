using ColossalFramework;
using System;
using UnityEngine;

namespace RushHour.Events
{
    public abstract class CityEvent
    {
        public bool m_eventCreated = false;
        public bool m_eventStarted = false;
        public bool m_eventEnded = false;
        public ushort m_eventBuilding = 0;
        public int m_registeredCitizens = 0;
        public DateTime m_eventStartTime;
        public DateTime m_eventFinishTime;
        
        public abstract bool CitizenCanGo(uint citizenID, ref Citizen person);
        public abstract int GetCapacity();

        public void Update()
        {
            if (CityEventManager.CITY_TIME > m_eventStartTime && !m_eventStarted)
            {
                SimulationManager _simulationManager = Singleton<SimulationManager>.instance;

                m_eventFinishTime = m_eventStartTime.AddHours(_simulationManager.m_randomizer.Int32(1, 3));
                m_eventStarted = true;
            }
            else if (m_eventStarted && CityEventManager.CITY_TIME > m_eventFinishTime)
            {
                m_eventEnded = true;
                m_eventStarted = false;

                Debug.Log("Event finished!");
                Debug.Log("Current date: " + CityEventManager.CITY_TIME.ToLongTimeString() + ", " + CityEventManager.CITY_TIME.ToShortDateString());
            }
        }

        public void SetUp(ref ushort building)
        {
            SimulationManager _simulationManager = Singleton<SimulationManager>.instance;

            int dayOffset = _simulationManager.m_randomizer.Int32(1, 7);
            int startHour = _simulationManager.m_randomizer.Int32(19, 23);

            m_eventBuilding = building;
            m_eventStartTime = new DateTime(CityEventManager.CITY_TIME.Year, CityEventManager.CITY_TIME.Month, CityEventManager.CITY_TIME.Day, startHour, 0, 0).AddDays(dayOffset);
            m_eventCreated = true;
        }

        public bool Register()
        {
            bool registered = false;

            if (m_registeredCitizens < GetCapacity())
            {
                ++m_registeredCitizens;
                registered = true;
            }

            return registered;
        }

        public bool EventStartsWithin(double hours)
        {
            bool eventStartsSoon = false;

            if (m_eventCreated && !m_eventStarted)
            {
                TimeSpan difference = m_eventStartTime - CityEventManager.CITY_TIME;
                eventStartsSoon = difference.TotalHours <= hours;
            }

            return eventStartsSoon;
        }

        public bool EventEndsWithin(double hours)
        {
            bool eventEndsSoon = false;

            if (m_eventCreated && m_eventStarted && !m_eventEnded)
            {
                TimeSpan difference = m_eventFinishTime - CityEventManager.CITY_TIME;
                eventEndsSoon = difference.TotalHours <= hours;
            }

            return eventEndsSoon;
        }
    }
}
