using System;
using ColossalFramework;
using System.Collections.Generic;

namespace RushHour.Events.Unique
{
    class GameExpo : CityEvent
    {
        public GameExpo()
        {
            m_eventInitialisedMessages = new List<string>()
            {
                "A #games expo? In my city? In {0}? Whaaaat? #event",
                "Can't wait to see what companies are attending the #games expo in {0}. I wonder if the city building game will be there... #event",
                "Got my tickets for the #game expo in {0}! So excited! #event"
            };

            m_eventStartedMessages = new List<string>()
            {
                "So many #games! I'm so excited! #event",
                "Can't wait to get my hands on some of the #VR equipment! #event",
                "The games are so realistic this year! #event"
            };

            m_eventEndedMessages = new List<string>()
            {
                "Over already? I can't believe it. I hope there's another soon. #event"
            };
        }

        public override bool CitizenCanGo(uint citizenID, ref Citizen person)
        {
            Citizen.Wealth _citizenWealth = person.WealthLevel;
            Citizen.Education _citizenEducation = person.EducationLevel;
            Citizen.AgeGroup _citizenAge = Citizen.GetAgeGroup(person.Age);
            Citizen.Gender _citizenGender = Citizen.GetGender(citizenID);
            Citizen.Happiness _citizenHappiness = Citizen.GetHappinessLevel(Citizen.GetHappiness(person.m_health, person.m_wellbeing));
            Citizen.Wellbeing _citizenWellbeing = Citizen.GetWellbeingLevel(_citizenEducation, person.m_wellbeing);

            int percentage = Singleton<SimulationManager>.instance.m_randomizer.Int32(100);

            return _citizenWealth >= Citizen.Wealth.Medium &&
                    (_citizenGender == Citizen.Gender.Male || (_citizenGender == Citizen.Gender.Female && percentage < 40)) &&
                    _citizenAge >= Citizen.AgeGroup.Young && _citizenAge < Citizen.AgeGroup.Senior &&
                    _citizenHappiness >= Citizen.Happiness.Good &&
                    _citizenWellbeing > Citizen.Wellbeing.Unhappy;
        }

        public override int GetCapacity()
        {
            return 4000;
        }

        public override double GetEventLength()
        {
            return 4D;
        }
    }
}
