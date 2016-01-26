using ColossalFramework;
using RushHour.Message;
using System;
using System.Collections.Generic;
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

        internal List<string> m_eventInitialisedMessages = null;
        internal List<string> m_eventStartedMessages = null;
        internal List<string> m_eventEndedMessages = null;
        
        public abstract bool CitizenCanGo(uint citizenID, ref Citizen person);
        public abstract int GetCapacity();
        public abstract double GetEventLength();

        public virtual string GetCitizenMessageInitialised()
        {
            string chosenMessage = "";

            if(m_eventInitialisedMessages.Count > 0)
            {
                int days = (m_eventStartTime - CityEventManager.CITY_TIME).Days;
                int randomIndex = Singleton<SimulationManager>.instance.m_randomizer.Int32(1, m_eventInitialisedMessages.Count) - 1;

                string dayString = days < 1 ? "less than a day" : days + " day" + (days > 1 ? "s" : "");

                chosenMessage = string.Format(m_eventInitialisedMessages[randomIndex], dayString);
            }

            return chosenMessage;
        }

        public virtual string GetCitizenMessageStarted()
        {
            string chosenMessage = "";

            if (m_eventStartedMessages.Count > 0)
            {
                int randomIndex = Singleton<SimulationManager>.instance.m_randomizer.Int32(1, m_eventStartedMessages.Count) - 1;
                chosenMessage = m_eventStartedMessages[randomIndex];
            }

            return chosenMessage;
        }

        public virtual string GetCitizenMessageEnded()
        {
            string chosenMessage = "";

            if (m_eventEndedMessages.Count > 0)
            {
                int randomIndex = Singleton<SimulationManager>.instance.m_randomizer.Int32(1, m_eventEndedMessages.Count) - 1;
                chosenMessage = m_eventEndedMessages[randomIndex];
            }

            return chosenMessage;
        }

        public void Update()
        {
            if (CityEventManager.CITY_TIME > m_eventStartTime && !m_eventStarted && !m_eventEnded)
            {
                SimulationManager _simulationManager = Singleton<SimulationManager>.instance;

                m_eventFinishTime = m_eventStartTime.AddHours(GetEventLength());
                m_eventStarted = true;

                MessageManager _messageManager = Singleton<MessageManager>.instance;
                _messageManager.QueueMessage(new CitizenCustomMessage(_messageManager.GetRandomResidentID(), GetCitizenMessageStarted()));

                Debug.Log("Event started!");
                Debug.Log("Current date: " + CityEventManager.CITY_TIME.ToLongTimeString() + ", " + CityEventManager.CITY_TIME.ToShortDateString());
            }
            else if (m_eventStarted && CityEventManager.CITY_TIME > m_eventFinishTime)
            {
                m_eventEnded = true;
                m_eventStarted = false;

                MessageManager _messageManager = Singleton<MessageManager>.instance;
                _messageManager.QueueMessage(new CitizenCustomMessage(_messageManager.GetRandomResidentID(), GetCitizenMessageEnded()));

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
