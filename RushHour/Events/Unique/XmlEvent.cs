using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RushHour.Events.Unique
{
    public class XmlEvent : CityEvent
    {
        internal int m_capacity = 0;
        internal double m_eventLength = 0;
        internal CityEventXmlChances m_eventChances = null;

        public XmlEvent(CityEventXmlContainer xmlContainer)
        {
            m_eventInitialisedMessages = xmlContainer._initialisedMessages.ToList();
            m_eventStartedMessages = xmlContainer._beginMessages.ToList();
            m_eventEndedMessages = xmlContainer._endedMessages.ToList();

            m_eventData.m_eventName = "XMLEvent-" + xmlContainer._name;

            m_capacity = xmlContainer._eventCapacity;
            m_eventLength = xmlContainer._eventLength;
            m_eventChances = xmlContainer._chances;
        }

        public override bool CitizenCanGo(uint citizenID, ref Citizen person)
        {
            bool canGo = false;

            if(m_eventChances != null)
            {
                canGo = true;

                Citizen.Wealth _citizenWealth = person.WealthLevel;
                Citizen.Education _citizenEducation = person.EducationLevel;
                Citizen.Gender _citizenGender = Citizen.GetGender(citizenID);
                Citizen.Happiness _citizenHappiness = Citizen.GetHappinessLevel(Citizen.GetHappiness(person.m_health, person.m_wellbeing));
                Citizen.Wellbeing _citizenWellbeing = Citizen.GetWellbeingLevel(_citizenEducation, person.m_wellbeing);
                Citizen.AgeGroup _citizenAgeGroup = Citizen.GetAgeGroup(person.Age);

                int randomPercentage = Singleton<SimulationManager>.instance.m_randomizer.Int32(100U);

                switch(_citizenWealth)
                {
                    case Citizen.Wealth.Low:
                        canGo = canGo && randomPercentage < m_eventChances._lowWealth;
                        break;
                    case Citizen.Wealth.Medium:
                        canGo = canGo && randomPercentage < m_eventChances._mediumWealth;
                        break;
                    case Citizen.Wealth.High:
                        canGo = canGo && randomPercentage < m_eventChances._highWealth;
                        break;
                }

                randomPercentage = Singleton<SimulationManager>.instance.m_randomizer.Int32(100U);

                switch (_citizenEducation)
                {
                    case Citizen.Education.Uneducated:
                        canGo = canGo && randomPercentage < m_eventChances._uneducated;
                        break;
                    case Citizen.Education.OneSchool:
                        canGo = canGo && randomPercentage < m_eventChances._oneSchool;
                        break;
                    case Citizen.Education.TwoSchools:
                        canGo = canGo && randomPercentage < m_eventChances._twoSchools;
                        break;
                    case Citizen.Education.ThreeSchools:
                        canGo = canGo && randomPercentage < m_eventChances._threeSchools;
                        break;
                }

                randomPercentage = Singleton<SimulationManager>.instance.m_randomizer.Int32(100U);

                switch (_citizenGender)
                {
                    case Citizen.Gender.Female:
                        canGo = canGo && randomPercentage < m_eventChances._females;
                        break;
                    case Citizen.Gender.Male:
                        canGo = canGo && randomPercentage < m_eventChances._males;
                        break;
                }

                randomPercentage = Singleton<SimulationManager>.instance.m_randomizer.Int32(100U);

                switch (_citizenHappiness)
                {
                    case Citizen.Happiness.Bad:
                        canGo = canGo && randomPercentage < m_eventChances._badHappiness;
                        break;
                    case Citizen.Happiness.Poor:
                        canGo = canGo && randomPercentage < m_eventChances._poorHappiness;
                        break;
                    case Citizen.Happiness.Good:
                        canGo = canGo && randomPercentage < m_eventChances._goodHappiness;
                        break;
                    case Citizen.Happiness.Excellent:
                        canGo = canGo && randomPercentage < m_eventChances._excellentHappiness;
                        break;
                    case Citizen.Happiness.Suberb:
                        canGo = canGo && randomPercentage < m_eventChances._superbHappiness;
                        break;
                }

                randomPercentage = Singleton<SimulationManager>.instance.m_randomizer.Int32(100U);

                switch (_citizenWellbeing)
                {
                    case Citizen.Wellbeing.VeryUnhappy:
                        canGo = canGo && randomPercentage < m_eventChances._veryUnhappyWellbeing;
                        break;
                    case Citizen.Wellbeing.Unhappy:
                        canGo = canGo && randomPercentage < m_eventChances._unhappyWellbeing;
                        break;
                    case Citizen.Wellbeing.Satisfied:
                        canGo = canGo && randomPercentage < m_eventChances._satisfiedWellbeing;
                        break;
                    case Citizen.Wellbeing.Happy:
                        canGo = canGo && randomPercentage < m_eventChances._happyWellbeing;
                        break;
                    case Citizen.Wellbeing.VeryHappy:
                        canGo = canGo && randomPercentage < m_eventChances._veryHappyWellbeing;
                        break;
                }

                randomPercentage = Singleton<SimulationManager>.instance.m_randomizer.Int32(100U);

                switch (_citizenAgeGroup)
                {
                    case Citizen.AgeGroup.Child:
                        canGo = canGo && randomPercentage < m_eventChances._children;
                        break;
                    case Citizen.AgeGroup.Teen:
                        canGo = canGo && randomPercentage < m_eventChances._teens;
                        break;
                    case Citizen.AgeGroup.Young:
                        canGo = canGo && randomPercentage < m_eventChances._youngAdults;
                        break;
                    case Citizen.AgeGroup.Adult:
                        canGo = canGo && randomPercentage < m_eventChances._adults;
                        break;
                    case Citizen.AgeGroup.Senior:
                        canGo = canGo && randomPercentage < m_eventChances._seniors;
                        break;
                }

                if (m_eventData.m_registeredCitizens < GetCapacity())
                {
                    CimTools.CimToolsHandler.CimToolBase.DetailedLogger.Log(
                        "Citizen " + citizenID + " registered " + canGo + " for " + m_eventData.m_eventName + "\n\t" +
                        _citizenWealth.ToString() + ", " + _citizenEducation.ToString() + ", " + _citizenGender.ToString() + ", " +
                        _citizenHappiness.ToString() + ", " + _citizenWellbeing.ToString() + ", " + _citizenAgeGroup.ToString());
                }
            }
            
            return canGo;
        }

        public override int GetCapacity()
        {
            return Math.Min(m_capacity, 9000);
        }

        public override double GetEventLength()
        {
            return m_eventLength;
        }
    }
}
