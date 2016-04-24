using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using RushHour.Containers;
using UnityEngine;

namespace RushHour.Events.Unique
{
    public class XmlEvent : CityEvent
    {
        protected int m_capacity = 0;
        protected double m_eventLength = 0;
        protected string m_userEventName = "";
        protected CityEventXmlChances m_eventChances = null;
        protected CityEventXmlCosts m_eventCosts = null;
        protected CityEventXmlIncentive[] m_eventIncentives = null;

        public XmlEvent(CityEventXmlContainer xmlContainer)
        {
            m_eventInitialisedMessages = xmlContainer._initialisedMessages.ToList();
            m_eventStartedMessages = xmlContainer._beginMessages.ToList();
            m_eventEndedMessages = xmlContainer._endedMessages.ToList();

            m_eventData.m_eventName = "XMLEvent-" + xmlContainer._name;
            m_eventData.m_canBeWatchedOnTV = xmlContainer._canBeWatchedOnTV;
            m_eventData.m_entryCost = xmlContainer._costs._entry;

            m_capacity = xmlContainer._eventCapacity;
            m_eventLength = xmlContainer._eventLength;
            m_eventChances = xmlContainer._chances;
            m_eventCosts = xmlContainer._costs;
            m_eventIncentives = xmlContainer._incentives;
            m_userEventName = xmlContainer._userEventName;

            SetUpIncentives(m_eventIncentives);
        }

        protected void SetUpIncentives(CityEventXmlIncentive[] incentives)
        {
            if (incentives != null)
            {
                m_eventData.m_incentives = new CityEventDataIncentives[incentives.Length];

                for (int index = 0; index < incentives.Length; ++index)
                {
                    CityEventXmlIncentive incentive = incentives[index];
                    CityEventDataIncentives dataIncentive = new CityEventDataIncentives()
                    {
                        itemCount = 0,
                        name = incentive._name,
                        returnCost = incentive._returnCost
                    };

                    m_eventData.m_incentives[index] = dataIncentive;
                }
            }
        }

        public string GetReadableName()
        {
            return m_userEventName;
        }

        public CityEventXmlCosts GetCosts()
        {
            return m_eventCosts;
        }

        public List<CityEventXmlIncentive> GetIncentives()
        {
            return m_eventIncentives.ToList();
        }

        public void SetEntryCost(int cost)
        {
            m_eventData.m_entryCost = cost;
        }

        public override bool CitizenCanGo(uint citizenID, ref Citizen person)
        {
            bool canGo = false;

            if (m_eventChances != null && m_eventData.m_registeredCitizens < GetCapacity())
            {
                canGo = true;

                Citizen.Wealth _citizenWealth = person.WealthLevel;
                Citizen.Education _citizenEducation = person.EducationLevel;
                Citizen.Gender _citizenGender = Citizen.GetGender(citizenID);
                Citizen.Happiness _citizenHappiness = Citizen.GetHappinessLevel(Citizen.GetHappiness(person.m_health, person.m_wellbeing));
                Citizen.Wellbeing _citizenWellbeing = Citizen.GetWellbeingLevel(_citizenEducation, person.m_wellbeing);
                Citizen.AgeGroup _citizenAgeGroup = Citizen.GetAgeGroup(person.Age);

                float percentageChange = GetAdjustedChancePercentage();
                int randomPercentage = Singleton<SimulationManager>.instance.m_randomizer.Int32(100U);

                switch (_citizenWealth)
                {
                    case Citizen.Wealth.Low:
                        canGo = canGo && randomPercentage < Adjust(m_eventChances._lowWealth, percentageChange);
                        break;
                    case Citizen.Wealth.Medium:
                        canGo = canGo && randomPercentage < Adjust(m_eventChances._mediumWealth, percentageChange);
                        break;
                    case Citizen.Wealth.High:
                        canGo = canGo && randomPercentage < Adjust(m_eventChances._highWealth, percentageChange);
                        break;
                }

                randomPercentage = Singleton<SimulationManager>.instance.m_randomizer.Int32(100U);

                switch (_citizenEducation)
                {
                    case Citizen.Education.Uneducated:
                        canGo = canGo && randomPercentage < Adjust(m_eventChances._uneducated, percentageChange);
                        break;
                    case Citizen.Education.OneSchool:
                        canGo = canGo && randomPercentage < Adjust(m_eventChances._oneSchool, percentageChange);
                        break;
                    case Citizen.Education.TwoSchools:
                        canGo = canGo && randomPercentage < Adjust(m_eventChances._twoSchools, percentageChange);
                        break;
                    case Citizen.Education.ThreeSchools:
                        canGo = canGo && randomPercentage < Adjust(m_eventChances._threeSchools, percentageChange);
                        break;
                }

                randomPercentage = Singleton<SimulationManager>.instance.m_randomizer.Int32(100U);

                switch (_citizenGender)
                {
                    case Citizen.Gender.Female:
                        canGo = canGo && randomPercentage < Adjust(m_eventChances._females, percentageChange);
                        break;
                    case Citizen.Gender.Male:
                        canGo = canGo && randomPercentage < Adjust(m_eventChances._males, percentageChange);
                        break;
                }

                randomPercentage = Singleton<SimulationManager>.instance.m_randomizer.Int32(100U);

                switch (_citizenHappiness)
                {
                    case Citizen.Happiness.Bad:
                        canGo = canGo && randomPercentage < Adjust(m_eventChances._badHappiness, percentageChange);
                        break;
                    case Citizen.Happiness.Poor:
                        canGo = canGo && randomPercentage < Adjust(m_eventChances._poorHappiness, percentageChange);
                        break;
                    case Citizen.Happiness.Good:
                        canGo = canGo && randomPercentage < Adjust(m_eventChances._goodHappiness, percentageChange);
                        break;
                    case Citizen.Happiness.Excellent:
                        canGo = canGo && randomPercentage < Adjust(m_eventChances._excellentHappiness, percentageChange);
                        break;
                    case Citizen.Happiness.Suberb:
                        canGo = canGo && randomPercentage < Adjust(m_eventChances._superbHappiness, percentageChange);
                        break;
                }

                randomPercentage = Singleton<SimulationManager>.instance.m_randomizer.Int32(100U);

                switch (_citizenWellbeing)
                {
                    case Citizen.Wellbeing.VeryUnhappy:
                        canGo = canGo && randomPercentage < Adjust(m_eventChances._veryUnhappyWellbeing, percentageChange);
                        break;
                    case Citizen.Wellbeing.Unhappy:
                        canGo = canGo && randomPercentage < Adjust(m_eventChances._unhappyWellbeing, percentageChange);
                        break;
                    case Citizen.Wellbeing.Satisfied:
                        canGo = canGo && randomPercentage < Adjust(m_eventChances._satisfiedWellbeing, percentageChange);
                        break;
                    case Citizen.Wellbeing.Happy:
                        canGo = canGo && randomPercentage < Adjust(m_eventChances._happyWellbeing, percentageChange);
                        break;
                    case Citizen.Wellbeing.VeryHappy:
                        canGo = canGo && randomPercentage < Adjust(m_eventChances._veryHappyWellbeing, percentageChange);
                        break;
                }

                randomPercentage = Singleton<SimulationManager>.instance.m_randomizer.Int32(100U);

                switch (_citizenAgeGroup)
                {
                    case Citizen.AgeGroup.Child:
                        canGo = canGo && randomPercentage < Adjust(m_eventChances._children, percentageChange);
                        break;
                    case Citizen.AgeGroup.Teen:
                        canGo = canGo && randomPercentage < Adjust(m_eventChances._teens, percentageChange);
                        break;
                    case Citizen.AgeGroup.Young:
                        canGo = canGo && randomPercentage < Adjust(m_eventChances._youngAdults, percentageChange);
                        break;
                    case Citizen.AgeGroup.Adult:
                        canGo = canGo && randomPercentage < Adjust(m_eventChances._adults, percentageChange);
                        break;
                    case Citizen.AgeGroup.Senior:
                        canGo = canGo && randomPercentage < Adjust(m_eventChances._seniors, percentageChange);
                        break;
                }
                
                CimTools.CimToolsHandler.CimToolBase.DetailedLogger.Log(
                    (canGo ? "[Can Go]" : "[Ignoring]") +
                    " Citizen " + citizenID + " for " + m_eventData.m_eventName + "\n\t" +
                    _citizenWealth.ToString() + ", " + _citizenEducation.ToString() + ", " + _citizenGender.ToString() + ", " +
                    _citizenHappiness.ToString() + ", " + _citizenWellbeing.ToString() + ", " + _citizenAgeGroup.ToString());
            }
            
            return canGo;
        }

        protected float GetAdjustedChancePercentage()
        {
            float additionalAmount = 0;

            if(m_eventData.m_incentives != null && m_eventIncentives != null)
            {
                List<CityEventXmlIncentive> xmlIncentives = m_eventIncentives.ToList();

                foreach(CityEventDataIncentives dataIncentive in m_eventData.m_incentives)
                {
                    CityEventXmlIncentive foundIncentive = xmlIncentives.Find(incentive => incentive._name == dataIncentive.name);

                    if(foundIncentive != null)
                    {
                        if(dataIncentive.boughtItems < dataIncentive.itemCount || (!m_eventData.m_userEvent && foundIncentive._activeWhenRandomEvent))
                        {
                            additionalAmount += foundIncentive._positiveEffect;
                        }
                        else
                        {
                            additionalAmount -= foundIncentive._negativeEffect;
                        }
                    }
                }
            }

            CimTools.CimToolsHandler.CimToolBase.DetailedLogger.Log("Adjusting percentage for event. Adjusting by " + additionalAmount.ToString());

            return additionalAmount;
        }

        protected int Adjust(int value, float percentage)
        {
            float decimalValue = value;
            return Mathf.RoundToInt(decimalValue + (decimalValue == 0f || percentage == 0f ? 0 : ((decimalValue * percentage) / 100f)));
        }

        public override float GetCost()
        {
            float finalCost = 0f;

            if (m_eventData != null && m_eventData.m_userEvent)
            {
                finalCost += m_eventCosts._creation;
                finalCost += m_eventCosts._perHead * m_eventData.m_userTickets;

                if (m_eventData.m_incentives != null && m_eventIncentives != null)
                {
                    List<CityEventXmlIncentive> incentiveList = m_eventIncentives.ToList();

                    foreach (CityEventDataIncentives incentive in m_eventData.m_incentives)
                    {
                        CityEventXmlIncentive foundIncentive = incentiveList.Find(thisIncentive => thisIncentive._name == incentive.name);

                        if(foundIncentive != null)
                        {
                            finalCost += incentive.itemCount * foundIncentive._cost;
                        }
                        else
                        {
                            CimTools.CimToolsHandler.CimToolBase.DetailedLogger.LogError("Failed to match event data incentive to XML data incentive.");
                        }
                    }
                }
                else
                {
                    CimTools.CimToolsHandler.CimToolBase.DetailedLogger.LogError("Tried to get the cost of an event that has no incentives!");
                }
            }
            else
            {
                CimTools.CimToolsHandler.CimToolBase.DetailedLogger.LogError("Tried to get the cost of an event that has no data!");
            }

            return finalCost;
        }

        public override float GetExpectedReturn()
        {
            float expectedReturn = 0f;

            if (m_eventData != null && m_eventData.m_userEvent)
            {
                expectedReturn += m_eventData.m_entryCost * m_eventData.m_userTickets;

                if (m_eventData.m_incentives != null && m_eventIncentives != null)
                {
                    List<CityEventXmlIncentive> incentiveList = m_eventIncentives.ToList();

                    foreach (CityEventDataIncentives incentive in m_eventData.m_incentives)
                    {
                        expectedReturn += incentive.itemCount * incentive.returnCost;
                    }
                }
                else
                {
                    CimTools.CimToolsHandler.CimToolBase.DetailedLogger.LogError("Tried to get the return cost of an event that has no incentives!");
                }
            }

            return expectedReturn;
        }

        public override int GetCapacity()
        {
            if (m_eventData.m_userEvent)
            {
                return Math.Min(m_eventData.m_userTickets, Math.Min(m_capacity, 9000));
            }
            else
            {
                return Math.Min(m_capacity, 9000);
            }
        }

        public override double GetEventLength()
        {
            return m_eventLength;
        }

        protected override bool CitizenRegistered(uint citizenID, ref Citizen person)
        {
            bool canAttend = true;
            float maxSpend = 0f;

            if (m_eventData.m_userEvent)
            {
                SimulationManager simulationManager = Singleton<SimulationManager>.instance;
                Citizen.Wealth wealth = person.WealthLevel;

                switch (wealth)
                {
                    case Citizen.Wealth.Low:
                        maxSpend = 30f + simulationManager.m_randomizer.Int32(60);
                        break;
                    case Citizen.Wealth.Medium:
                        maxSpend = 80f + simulationManager.m_randomizer.Int32(80);
                        break;
                    case Citizen.Wealth.High:
                        maxSpend = 120f + simulationManager.m_randomizer.Int32(320);
                        break;
                }

                if (m_eventCosts != null)
                {
                    maxSpend -= m_eventCosts._entry;

                    canAttend = maxSpend > 0;

                    if (m_eventData.m_incentives != null && m_eventData.m_incentives.Length > 0)
                    {
                        int startFrom = simulationManager.m_randomizer.Int32(0, m_eventData.m_incentives.Length - 1);
                        int index = startFrom;
                        string buying = m_eventData.m_eventName + " ";

                        do
                        {
                            CityEventDataIncentives incentive = m_eventData.m_incentives[index];

                            if (incentive.boughtItems < incentive.itemCount && maxSpend - incentive.returnCost >= 0)
                            {
                                maxSpend -= incentive.returnCost;
                                ++incentive.boughtItems;

                                buying += "[" + incentive.name + " (" + incentive.boughtItems + "/" + incentive.itemCount + ")] ";
                            }

                            if (++index >= m_eventData.m_incentives.Length)
                            {
                                index = 0;
                            }
                        } while (index != startFrom);

                        CimTools.CimToolsHandler.CimToolBase.DetailedLogger.Log(buying);
                    }
                }
            }

            if(!canAttend)
            {
                CimTools.CimToolsHandler.CimToolBase.DetailedLogger.Log("Cim is too poor to attend the event :(");
            }

            return canAttend;
        }

        
        protected void CheckAndAdd<T>(ref List<T> list, ref int highestChance, T itemToAdd, int value)
        {
            if (value > highestChance)
            {
                highestChance = value;
                list.Clear();
                list.Add(itemToAdd);
            }
            else if(value == highestChance)
            {
                list.Add(itemToAdd);
            }
        }

        public List<Citizen.AgeGroup> GetHighestPercentageAgeGroup()
        {
            List<Citizen.AgeGroup> returnGroups = new List<Citizen.AgeGroup>();
            int highestChance = 0;

            CheckAndAdd(ref returnGroups, ref highestChance, Citizen.AgeGroup.Adult, m_eventChances._adults);
            CheckAndAdd(ref returnGroups, ref highestChance, Citizen.AgeGroup.Child, m_eventChances._children);
            CheckAndAdd(ref returnGroups, ref highestChance, Citizen.AgeGroup.Senior, m_eventChances._seniors);
            CheckAndAdd(ref returnGroups, ref highestChance, Citizen.AgeGroup.Teen, m_eventChances._teens);
            CheckAndAdd(ref returnGroups, ref highestChance, Citizen.AgeGroup.Young, m_eventChances._youngAdults);

            return returnGroups;
        }

        public List<Citizen.Wealth> GetHighestPercentageWealth()
        {
            List<Citizen.Wealth> returnGroups = new List<Citizen.Wealth>();
            int highestChance = 0;

            CheckAndAdd(ref returnGroups, ref highestChance, Citizen.Wealth.High, m_eventChances._highWealth);
            CheckAndAdd(ref returnGroups, ref highestChance, Citizen.Wealth.Medium, m_eventChances._mediumWealth);
            CheckAndAdd(ref returnGroups, ref highestChance, Citizen.Wealth.Low, m_eventChances._lowWealth);

            return returnGroups;
        }

        public List<Citizen.Education> GetHighestPercentageEducation()
        {
            List<Citizen.Education> returnGroups = new List<Citizen.Education>();
            int highestChance = 0;

            CheckAndAdd(ref returnGroups, ref highestChance, Citizen.Education.Uneducated, m_eventChances._uneducated);
            CheckAndAdd(ref returnGroups, ref highestChance, Citizen.Education.OneSchool, m_eventChances._oneSchool);
            CheckAndAdd(ref returnGroups, ref highestChance, Citizen.Education.TwoSchools, m_eventChances._twoSchools);
            CheckAndAdd(ref returnGroups, ref highestChance, Citizen.Education.ThreeSchools, m_eventChances._threeSchools);

            return returnGroups;
        }

        public List<Citizen.Gender> GetHighestPercentageGender()
        {
            List<Citizen.Gender> returnGroups = new List<Citizen.Gender>();
            int highestChance = 0;

            CheckAndAdd(ref returnGroups, ref highestChance, Citizen.Gender.Female, m_eventChances._females);
            CheckAndAdd(ref returnGroups, ref highestChance, Citizen.Gender.Male, m_eventChances._males);

            return returnGroups;
        }
    }
}
