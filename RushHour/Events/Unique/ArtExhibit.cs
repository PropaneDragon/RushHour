using System;
using ColossalFramework;
using System.Collections.Generic;

namespace RushHour.Events.Unique
{
    class ArtExhibit : CityEvent
    {
        public ArtExhibit()
        {
            m_eventInitialisedMessages = new List<string>()
            {
                "Got my tickets to the #art exhibit in {0}! #first #event",
                "One of my favourite artists is going to be at the #art exhibit in {0}! Can't wait! #event",
                "Anyone want a spare ticket for the #art exhibit in {0}? I've got a few going. #event",
                "I think I just got the last ticket for the #art exhibit in {0}. Feeling #lucky! #event"
            };

            m_eventStartedMessages = new List<string>()
            {
                "And the doors have finally opened! Time to see some lovely #art. #event",
                "Wow, they've certainly packed this place full. I'm never going to get around all of this! #event",
                "I didn't event realise this city enjoyed #art as much as this! #event"
            };
        }

        public override bool CitizenCanGo(uint citizenID, ref Citizen person)
        {
            Citizen.Wealth _citizenWealth = person.WealthLevel;
            Citizen.Education _citizenEducation = person.EducationLevel;
            Citizen.Happiness _citizenHappiness = Citizen.GetHappinessLevel(Citizen.GetHappiness(person.m_health, person.m_wellbeing));
            Citizen.Wellbeing _citizenWellbeing = Citizen.GetWellbeingLevel(_citizenEducation, person.m_wellbeing);

            return _citizenWealth > Citizen.Wealth.Low &&
                    _citizenEducation > Citizen.Education.TwoSchools &&
                    _citizenHappiness >= Citizen.Happiness.Good &&
                    _citizenWellbeing >= Citizen.Wellbeing.Satisfied;
        }

        public override int GetCapacity()
        {
            return 900;
        }

        public override double GetEventLength()
        {
            return Singleton<SimulationManager>.instance.m_randomizer.Int32(5,15) / 10D;
        }
    }
}
