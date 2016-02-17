using ColossalFramework;
using RushHour.Message;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RushHour.Events
{
    public abstract class CityEvent
    {
        public CityEventData m_eventData = new CityEventData();

        internal List<string> m_eventInitialisedMessages = null;
        internal List<string> m_eventStartedMessages = null;
        internal List<string> m_eventEndedMessages = null;
        
        public abstract bool CitizenCanGo(uint citizenID, ref Citizen person);
        public abstract int GetCapacity();
        public abstract double GetEventLength();

        public void SetUp(ref ushort building)
        {
            SimulationManager _simulationManager = Singleton<SimulationManager>.instance;

            int dayOffset = _simulationManager.m_randomizer.Int32(1, 3);
            int startHour = _simulationManager.m_randomizer.Int32(19, 23);

            if (CityEventManager.instance.IsWeekend(CityEventManager.CITY_TIME.AddDays(dayOffset)))
            {
                startHour = _simulationManager.m_randomizer.Int32(8, 23);
            }

            m_eventData.m_eventBuilding = building;
            m_eventData.m_eventStartTime = new DateTime(CityEventManager.CITY_TIME.Year, CityEventManager.CITY_TIME.Month, CityEventManager.CITY_TIME.Day, startHour, 0, 0).AddDays(dayOffset);
            m_eventData.m_eventFinishTime = m_eventData.m_eventStartTime.AddHours(GetEventLength());
            m_eventData.m_creationDate = DateTime.Now.Ticks.ToString();

            m_eventData.m_eventCreated = true;
        }

        public bool Register()
        {
            bool registered = false;

            if (m_eventData.m_registeredCitizens < GetCapacity())
            {
                ++m_eventData.m_registeredCitizens;
                registered = true;
            }

            return registered;
        }

        public void Update()
        {
            if (CityEventManager.CITY_TIME > m_eventData.m_eventStartTime && !m_eventData.m_eventStarted && !m_eventData.m_eventEnded)
            {
                SimulationManager _simulationManager = Singleton<SimulationManager>.instance;

                m_eventData.m_eventStarted = true;

                string message = GetCitizenMessageStarted();

                if (message != "")
                {
                    MessageManager _messageManager = Singleton<MessageManager>.instance;
                    _messageManager.QueueMessage(new CitizenCustomMessage(_messageManager.GetRandomResidentID(), message));
                }

                CimTools.CimToolsHandler.CimToolBase.DetailedLogger.Log("Event starting at " + Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_eventData.m_eventBuilding].Info.name);
                Debug.Log("Event started!");
                Debug.Log("Current date: " + CityEventManager.CITY_TIME.ToLongTimeString() + ", " + CityEventManager.CITY_TIME.ToShortDateString());
            }
            else if (m_eventData.m_eventStarted && CityEventManager.CITY_TIME > m_eventData.m_eventFinishTime)
            {
                m_eventData.m_eventEnded = true;
                m_eventData.m_eventStarted = false;

                string message = GetCitizenMessageEnded();

                if (message != "")
                {
                    MessageManager _messageManager = Singleton<MessageManager>.instance;
                    _messageManager.QueueMessage(new CitizenCustomMessage(_messageManager.GetRandomResidentID(), message));
                }

                CimTools.CimToolsHandler.CimToolBase.DetailedLogger.Log("Event finished at " + Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_eventData.m_eventBuilding].Info.name);
                Debug.Log("Event finished!");
                Debug.Log("Current date: " + CityEventManager.CITY_TIME.ToLongTimeString() + ", " + CityEventManager.CITY_TIME.ToShortDateString());
            }
        }

        public bool EventStartsWithin(double hours)
        {
            bool eventStartsSoon = false;

            if (m_eventData.m_eventCreated && !m_eventData.m_eventStarted && !m_eventData.m_eventEnded)
            {
                TimeSpan difference = m_eventData.m_eventStartTime - CityEventManager.CITY_TIME;
                eventStartsSoon = difference.TotalHours <= hours;
            }

            return eventStartsSoon;
        }

        public bool EventEndsWithin(double hours)
        {
            bool eventEndsSoon = false;

            if (m_eventData.m_eventCreated && m_eventData.m_eventStarted && !m_eventData.m_eventEnded)
            {
                TimeSpan difference = m_eventData.m_eventFinishTime - CityEventManager.CITY_TIME;
                eventEndsSoon = difference.TotalHours <= hours;
            }

            return eventEndsSoon;
        }

        public virtual string GetCitizenMessageInitialised()
        {
            string chosenMessage = "";

            if (m_eventInitialisedMessages.Count > 0)
            {
                if (m_eventInitialisedMessages.Count < 4)
                {
                    CimTools.CimToolsHandler.CimToolBase.DetailedLogger.LogWarning("Event " + m_eventData.m_eventName + " has less than 4 messages for the initialisation. This could get boring.");
                }

                int days = (m_eventData.m_eventStartTime - CityEventManager.CITY_TIME).Days;
                float eventLength = (float)GetEventLength();
                int roundedEventLength = Mathf.FloorToInt(eventLength);
                float eventLengthDifference = eventLength - roundedEventLength;

                string dayString = days < 1 ? "less than a day" : days + " day" + (days > 1 ? "s" : "");
                string ticketString = GetCapacity() + " tickets";
                string eventLengthString = (eventLengthDifference > 0.1 ? "more than " : "") + roundedEventLength + " hour" + (roundedEventLength > 1 ? "s" : "") + " long";

                int randomIndex = Singleton<SimulationManager>.instance.m_randomizer.Int32(1, m_eventInitialisedMessages.Count) - 1;

                chosenMessage = string.Format(m_eventInitialisedMessages[randomIndex], dayString, ticketString, eventLengthString);
            }

            CimTools.CimToolsHandler.CimToolBase.DetailedLogger.Log("Event chirped initialised \"" + chosenMessage + "\"");
            return chosenMessage;
        }

        public virtual string GetCitizenMessageStarted()
        {
            string chosenMessage = "";

            if (m_eventStartedMessages.Count > 0)
            {
                if (m_eventStartedMessages.Count < 4)
                {
                    CimTools.CimToolsHandler.CimToolBase.DetailedLogger.LogWarning("Event " + m_eventData.m_eventName + " has less than 4 messages for the start. This could get boring.");
                }

                int randomIndex = Singleton<SimulationManager>.instance.m_randomizer.Int32(1, m_eventStartedMessages.Count) - 1;
                chosenMessage = m_eventStartedMessages[randomIndex];
            }

            CimTools.CimToolsHandler.CimToolBase.DetailedLogger.Log("Event chirped started \"" + chosenMessage + "\"");
            return chosenMessage;
        }

        public virtual string GetCitizenMessageEnded()
        {
            string chosenMessage = "";

            if (m_eventEndedMessages.Count > 0)
            {
                if(m_eventEndedMessages.Count < 4)
                {
                    CimTools.CimToolsHandler.CimToolBase.DetailedLogger.LogWarning("Event " + m_eventData.m_eventName + " has less than 4 messages for the end. This could get boring.");
                }

                int randomIndex = Singleton<SimulationManager>.instance.m_randomizer.Int32(1, m_eventEndedMessages.Count) - 1;
                chosenMessage = m_eventEndedMessages[randomIndex];
            }

            CimTools.CimToolsHandler.CimToolBase.DetailedLogger.Log("Event chirped ended \"" + chosenMessage + "\"");
            return chosenMessage;
        }
    }
}
