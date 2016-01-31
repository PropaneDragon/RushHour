using System;
using ColossalFramework;
using System.Collections.Generic;

namespace RushHour.Events.Unique
{
    class CaravanExpo : CityEvent
    {
        public CaravanExpo()
        {
            m_eventInitialisedMessages = new List<string>()
            {
                "Awww, there's only a #caravan expo going on in {0}... I was hoping for something more exciting. #event"
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
                    _citizenAge == Citizen.AgeGroup.Senior &&
                    _citizenHappiness >= Citizen.Happiness.Good &&
                    _citizenWellbeing > Citizen.Wellbeing.Unhappy;
        }

        public override int GetCapacity()
        {
            return 300;
        }

        public override double GetEventLength()
        {
            return 1.5D;
        }
    }
}
