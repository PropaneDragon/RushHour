using System.Collections.Generic;

namespace RushHour.Events.Unique
{
    class TheaterEvent : CityEvent
    {
        public TheaterEvent()
        {
            m_eventInitialisedMessages = new List<string>()
            {
                "Anyone want to go to the #theater with me in {0}? I've got some free time. #event"
            };

            m_eventStartedMessages = new List<string>()
            {
                "Wow, this really is a theater of wonders! #event"
            };

            m_eventEndedMessages = new List<string>()
            {
                "That was awesome! #event"
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
                    _citizenWellbeing >= Citizen.Wellbeing.Unhappy;
        }

        public override int GetCapacity()
        {
            return 700;
        }

        public override double GetEventLength()
        {
            return 2.3D;
        }
    }
}
