using ColossalFramework;
using ColossalFramework.Globalization;
using RushHour.Containers;
using RushHour.Localisation;
using RushHour.Message;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RushHour.Events
{
    internal abstract class CityEvent
    {
        public CityEventData m_eventData = new CityEventData();

        protected List<string> m_eventInitialisedMessages = null;
        protected List<string> m_eventStartedMessages = null;
        protected List<string> m_eventEndedMessages = null;
        
        public abstract bool CitizenCanGo(uint citizenID, ref Citizen person);
        public abstract int GetCapacity();
        public abstract double GetEventLength();

        public void SetUp(ref ushort building)
        {
            SimulationManager _simulationManager = Singleton<SimulationManager>.instance;

            int dayOffset = _simulationManager.m_randomizer.Int32(1, 3);
            int startHour = _simulationManager.m_randomizer.Int32(19, 23);
            int startMinute = _simulationManager.m_randomizer.Int32(0, 3);

            if (CityEventManager.instance.IsWeekend(CityEventManager.CITY_TIME.AddDays(dayOffset)))
            {
                startHour = _simulationManager.m_randomizer.Int32(8, 23);
            }

            m_eventData.m_eventBuilding = building;
            m_eventData.m_eventStartTime = new DateTime(CityEventManager.CITY_TIME.Year, CityEventManager.CITY_TIME.Month, CityEventManager.CITY_TIME.Day, startHour, startMinute * 15, 0).AddDays(dayOffset);
            m_eventData.m_eventFinishTime = m_eventData.m_eventStartTime.AddHours(GetEventLength());
            m_eventData.m_creationDate = DateTime.Now.Ticks.ToString();

            m_eventData.m_eventCreated = true;
        }

        public bool CreateUserEvent(int ticketsAvailable, float entryCost, List<IncentiveOptionItem> incentives, DateTime startTime)
        {
            bool created = false;

            if (!CityEventManager.instance.EventStartsBetween(startTime, startTime.AddHours(GetEventLength()) - startTime))
            {
                CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.Log("Creating user event");

                m_eventData.m_eventStartTime = startTime;
                m_eventData.m_eventFinishTime = startTime.AddHours(GetEventLength());
                m_eventData.m_entryCost = entryCost;
                m_eventData.m_userTickets = ticketsAvailable;
                m_eventData.m_userEvent = true;

                CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.Log("Adding incentives");

                if (m_eventData.m_incentives != null)
                {
                    foreach (CityEventDataIncentives dataIncentive in m_eventData.m_incentives)
                    {
                        CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.Log("Adding incentive " + dataIncentive.name);

                        IncentiveOptionItem foundIncentive = incentives.Find(match => match.title == dataIncentive.name);

                        if (foundIncentive != null)
                        {
                            CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.Log("Setting up incentive " + dataIncentive.name);
                            dataIncentive.itemCount = Mathf.RoundToInt(foundIncentive.sliderValue);
                            dataIncentive.returnCost = foundIncentive.returnCost;
                        }
                        else
                        {
                            CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.LogWarning("Couldn't find the IncentiveOptionItem that matches " + dataIncentive.name);
                        }
                    }

                    TakeInitialAmount();

                    created = true;
                }
                else
                {
                    CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.LogWarning("There are no incentives for " + m_eventData.m_eventName + ". Skipping");
                }
            }
            else
            {
                CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.LogWarning("Event clashes with another event.");
            }

            return created;
        }

        public bool Register(uint citizenID, ref Citizen person)
        {
            bool registered = false;

            if (m_eventData.m_registeredCitizens < GetCapacity())
            {
                registered = CitizenRegistered(citizenID, ref person);

                if(registered)
                {
                    ++m_eventData.m_registeredCitizens;

                    CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.Log("Registered citizen to event (" + m_eventData.m_registeredCitizens + "/" + GetCapacity() + ")");
                }
            }

            return registered;
        }

        public void Update()
        {
            if (CityEventManager.CITY_TIME > m_eventData.m_eventStartTime && !m_eventData.m_eventStarted && !m_eventData.m_eventEnded)
            {
                SimulationManager _simulationManager = Singleton<SimulationManager>.instance;
                Building eventBuilding = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_eventData.m_eventBuilding];
                InstanceID instance = new InstanceID() { Building = m_eventData.m_eventBuilding, Type = InstanceType.Building };

                m_eventData.m_eventStarted = true;

                string message = GetCitizenMessageStarted();

                if (message != "")
                {
                    MessageManager _messageManager = Singleton<MessageManager>.instance;
                    _messageManager.QueueMessage(new CitizenCustomMessage(_messageManager.GetRandomResidentID(), message));
                }

                PopupEventStarted(instance);

                CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.Log("Event starting at " + eventBuilding.Info.name);
                Debug.Log("Event started!");
                Debug.Log("Current date: " + CityEventManager.CITY_TIME.ToLongTimeString() + ", " + CityEventManager.CITY_TIME.ToShortDateString());
            }
            else if (m_eventData.m_eventStarted && CityEventManager.CITY_TIME > m_eventData.m_eventFinishTime)
            {
                Building eventBuilding = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_eventData.m_eventBuilding];
                InstanceID instance = new InstanceID() { Building = m_eventData.m_eventBuilding, Type = InstanceType.Building };

                m_eventData.m_eventEnded = true;
                m_eventData.m_eventStarted = false;

                string message = GetCitizenMessageEnded();

                if (message != "")
                {
                    MessageManager _messageManager = Singleton<MessageManager>.instance;
                    _messageManager.QueueMessage(new CitizenCustomMessage(_messageManager.GetRandomResidentID(), message));
                }

                if ((eventBuilding.m_flags & Building.Flags.Created) != Building.Flags.None)
                {
                    ReturnFinalAmount();
                    PopupEventEnded(instance);
                    CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.Log("Event finished at " + eventBuilding.Info.name);
                }
                else
                {
                    CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.Log("Event finished at a building that no longer exists...");
                }

                Debug.Log("Event finished!");
                Debug.Log("Current date: " + CityEventManager.CITY_TIME.ToLongTimeString() + ", " + CityEventManager.CITY_TIME.ToShortDateString());
            }
        }

        protected void PopupEventStarted(InstanceID eventInstance)
        {
            EventPopupManager.Instance.Show(LocalisationStrings.EVENT_POPUPSTARTEDTITLE, LocalisationStrings.EVENT_POPUPSTARTEDDESCRIPTION, eventInstance);
        }

        protected void PopupEventEnded(InstanceID eventInstance)
        {
            if (m_eventData.m_userEvent)
            {
                string endedDescription = LocalisationStrings.EVENT_POPUPENDEDDESCRIPTION;
                endedDescription += "\n" + LocalisationStrings.EVENT_POPUPENDEDDESCRIPTIONUSERMADE;
                
                if(m_eventData.m_incentives != null)
                {
                    string colourGreenString = "<color#97ee00>{0}</color>";
                    string colourRedString = "<color#ee5f00>{0}</color>";
                    string initialValue = "\n\n" + string.Format(colourRedString, LocalisationStrings.EVENT_TICKETS) + ": " + m_eventData.m_registeredCitizens + "/" + m_eventData.m_userTickets + "\n";
                    string concatenatedIncentives = initialValue;

                    CityEventDataIncentives[] incentives = m_eventData.m_incentives;

                    foreach(CityEventDataIncentives incentive in incentives)
                    {
                        if(concatenatedIncentives != initialValue) { concatenatedIncentives += ", "; }

                        concatenatedIncentives += string.Format(colourGreenString, incentive.name) + ": " + incentive.boughtItems + "/" + incentive.itemCount;
                    }

                    concatenatedIncentives += "\n\n" + string.Format(colourRedString, LocalisationStrings.EVENT_TOTALCOST) + ": " + GetCost().ToString(Settings.moneyFormat, LocaleManager.cultureInfo);
                    concatenatedIncentives += "\n" + string.Format(colourRedString, LocalisationStrings.EVENT_TOTALINCOME) + ": " + GetExpectedReturn().ToString(Settings.moneyFormat, LocaleManager.cultureInfo);
                    concatenatedIncentives += "\n" + string.Format(colourRedString, LocalisationStrings.EVENT_ACTUALINCOME) + ": " + GetActualReturn().ToString(Settings.moneyFormat, LocaleManager.cultureInfo);

                    endedDescription += concatenatedIncentives;
                }

                EventPopupManager.Instance.Show(LocalisationStrings.EVENT_POPUPENDEDTITLE, endedDescription, eventInstance);
            }
            else
            {
                EventPopupManager.Instance.Show(LocalisationStrings.EVENT_POPUPENDEDTITLE, LocalisationStrings.EVENT_POPUPENDEDDESCRIPTION, eventInstance);
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

        public bool EventActiveBetween(DateTime date, TimeSpan length)
        {
            if (m_eventData.m_eventCreated && !m_eventData.m_eventEnded)
            {
                DateTime eventStart = m_eventData.m_eventStartTime;
                DateTime eventEnd = m_eventData.m_eventFinishTime;
                DateTime endDate = date + length;

                return (date >= eventStart && date <= eventEnd) || (endDate >= eventStart && endDate <= eventEnd) || (date <= eventStart && endDate >= eventEnd);
            }

            return false;
        }

        public virtual float GetCost()
        {
            return 0f;
        }

        public virtual float GetExpectedReturn()
        {
            return 0f;
        }

        public virtual float GetActualReturn()
        {
            float actualReturn = 0f;

            if (m_eventData != null && m_eventData.m_userEvent)
            {
                actualReturn += m_eventData.m_entryCost * m_eventData.m_registeredCitizens;

                if (m_eventData.m_incentives != null)
                {
                    foreach (CityEventDataIncentives incentive in m_eventData.m_incentives)
                    {
                        actualReturn += incentive.boughtItems * incentive.returnCost;
                    }
                }
                else
                {
                    CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.LogError("Tried to get the return cost of an event that has no incentives!");
                }
            }

            return actualReturn - GetCost();
        }

        public void TakeInitialAmount()
        {
            if (m_eventData.m_userEvent)
            {
                Building eventBuilding = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_eventData.m_eventBuilding];

                if ((eventBuilding.m_flags & Building.Flags.Created) != Building.Flags.None)
                {
                    Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.Construction, Mathf.RoundToInt(GetCost() * 100f), eventBuilding.Info.m_class);
                    CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.Log("Taking " + GetCost() + " from the player to pay for the event");
                }
                else
                {
                    CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.LogError("Event building has not been created!");
                }
            }
        }

        public void ReturnFinalAmount()
        {
            if (m_eventData.m_userEvent)
            {
                Building eventBuilding = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_eventData.m_eventBuilding];

                if ((eventBuilding.m_flags & Building.Flags.Created) != Building.Flags.None)
                {
                    Singleton<EconomyManager>.instance.AddResource(EconomyManager.Resource.RewardAmount, Mathf.RoundToInt(GetActualReturn() * 100f), eventBuilding.Info.m_class);
                    CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.Log("Returning " + GetActualReturn() + " out of " + GetExpectedReturn() + " to the player for the event completion");
                }
                else
                {
                    CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.LogError("Event building has been destroyed!");
                }
            }
        }

        public virtual string GetCitizenMessageInitialised()
        {
            string chosenMessage = "";

            if (m_eventInitialisedMessages.Count > 0)
            {
                if (m_eventInitialisedMessages.Count < 4)
                {
                    CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.LogWarning("Event " + m_eventData.m_eventName + " has less than 4 messages for the initialisation. This could get boring.");
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

            CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.Log("Event chirped initialised \"" + chosenMessage + "\"");
            return chosenMessage;
        }

        public virtual string GetCitizenMessageStarted()
        {
            string chosenMessage = "";

            if (m_eventStartedMessages.Count > 0)
            {
                if (m_eventStartedMessages.Count < 4)
                {
                    CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.LogWarning("Event " + m_eventData.m_eventName + " has less than 4 messages for the start. This could get boring.");
                }

                int randomIndex = Singleton<SimulationManager>.instance.m_randomizer.Int32(1, m_eventStartedMessages.Count) - 1;
                chosenMessage = m_eventStartedMessages[randomIndex];
            }

            CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.Log("Event chirped started \"" + chosenMessage + "\"");
            return chosenMessage;
        }

        public virtual string GetCitizenMessageEnded()
        {
            string chosenMessage = "";

            if (m_eventEndedMessages.Count > 0)
            {
                if(m_eventEndedMessages.Count < 4)
                {
                    CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.LogWarning("Event " + m_eventData.m_eventName + " has less than 4 messages for the end. This could get boring.");
                }

                int randomIndex = Singleton<SimulationManager>.instance.m_randomizer.Int32(1, m_eventEndedMessages.Count) - 1;
                chosenMessage = m_eventEndedMessages[randomIndex];
            }

            CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.Log("Event chirped ended \"" + chosenMessage + "\"");
            return chosenMessage;
        }

        protected virtual bool CitizenRegistered(uint citizenID, ref Citizen person)
        {
            return true;
        }
    }
}
