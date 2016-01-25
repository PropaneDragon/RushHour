using System;
using ColossalFramework;
using System.Collections.Generic;

namespace RushHour.Events.Unique
{
    class ScienceEvent : CityEvent
    {
        public ScienceEvent()
        {
            m_eventInitialisedMessages = new List<string>()
            {
                "I wonder what this science event is about in {0}. #event"
            };

            m_eventStartedMessages = new List<string>()
            {
                "Time to do some #SCIENCE! Mwhahaha! #event"
            };
        }

        public override bool CitizenCanGo(uint citizenID, ref Citizen person)
        {
            Citizen.Wealth _citizenWealth = person.WealthLevel;
            Citizen.Education _citizenEducation = person.EducationLevel;
            Citizen.Happiness _citizenHappiness = Citizen.GetHappinessLevel(Citizen.GetHappiness(person.m_health, person.m_wellbeing));
            Citizen.Wellbeing _citizenWellbeing = Citizen.GetWellbeingLevel(_citizenEducation, person.m_wellbeing);

            return _citizenWealth >= Citizen.Wealth.Medium &&
                    _citizenHappiness >= Citizen.Happiness.Good &&
                    _citizenWellbeing > Citizen.Wellbeing.Unhappy;
        }

        public override int GetCapacity()
        {
            return 2000;
        }

        public override double GetEventLength()
        {
            return 2.3D;
        }
    }
}
