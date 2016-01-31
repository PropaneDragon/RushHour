using System;
using ColossalFramework;
using System.Collections.Generic;

namespace RushHour.Events.Unique
{
    class ElectronicExpo : CityEvent
    {
        public ElectronicExpo()
        {
            m_eventInitialisedMessages = new List<string>()
            {
                "An electronic expo?! In {0}?! Tickets please! #event"
            };

            m_eventStartedMessages = new List<string>()
            {
                "I wonder how much all these #electronics cost... #event"
            };

            m_eventEndedMessages = new List<string>()
            {
                "Awwww, the event's over already? I was enjoying myself :( #event",
                "Got myself some awesome stuff! A new #chirper phone? Yes please! #event"
            };

            m_eventData.m_eventName = GetType().FullName;
        }

        public override bool CitizenCanGo(uint citizenID, ref Citizen person)
        {
            Citizen.Wealth _citizenWealth = person.WealthLevel;
            Citizen.Education _citizenEducation = person.EducationLevel;
            Citizen.AgeGroup _citizenAge = Citizen.GetAgeGroup(person.Age);
            Citizen.Gender _citizenGender = Citizen.GetGender(citizenID);
            Citizen.Happiness _citizenHappiness = Citizen.GetHappinessLevel(Citizen.GetHappiness(person.m_health, person.m_wellbeing));
            Citizen.Wellbeing _citizenWellbeing = Citizen.GetWellbeingLevel(_citizenEducation, person.m_wellbeing);

            return _citizenWealth >= Citizen.Wealth.Medium &&
                    _citizenEducation >= Citizen.Education.TwoSchools &&
                    _citizenAge >= Citizen.AgeGroup.Teen &&
                    _citizenHappiness >= Citizen.Happiness.Good &&
                    _citizenWellbeing > Citizen.Wellbeing.Unhappy;
        }

        public override int GetCapacity()
        {
            return 2000;
        }

        public override double GetEventLength()
        {
            return 2.5D;
        }
    }
}
