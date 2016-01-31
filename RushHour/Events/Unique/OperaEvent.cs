using System;
using ColossalFramework;
using System.Collections.Generic;

namespace RushHour.Events.Unique
{
    class OperaEvent : CityEvent
    {
        public OperaEvent()
        {
            m_eventInitialisedMessages = new List<string>()
            {
                "Got my tickets to the #Opera in {0}. #event"
            };

            m_eventStartedMessages = new List<string>()
            {
                "Phone's off, so I won't be replying any time soon. #opera #event"
            };

            m_eventEndedMessages = new List<string>()
            {
                "That Opera was fantastic. I'd watch that again! #event"
            };

            m_eventData.m_eventName = GetType().FullName;
        }

        public override bool CitizenCanGo(uint citizenID, ref Citizen person)
        {
            Citizen.Wealth _citizenWealth = person.WealthLevel;
            Citizen.Education _citizenEducation = person.EducationLevel;
            Citizen.AgeGroup _citizenAge = Citizen.GetAgeGroup(person.Age);
            Citizen.Happiness _citizenHappiness = Citizen.GetHappinessLevel(Citizen.GetHappiness(person.m_health, person.m_wellbeing));
            Citizen.Wellbeing _citizenWellbeing = Citizen.GetWellbeingLevel(_citizenEducation, person.m_wellbeing);

            return _citizenWealth == Citizen.Wealth.High &&
                    _citizenEducation >= Citizen.Education.TwoSchools &&
                    _citizenAge >= Citizen.AgeGroup.Young &&
                    _citizenHappiness >= Citizen.Happiness.Good &&
                    _citizenWellbeing > Citizen.Wellbeing.Unhappy;
        }

        public override int GetCapacity()
        {
            return 2500;
        }

        public override double GetEventLength()
        {
            return 1.8D;
        }
    }
}
