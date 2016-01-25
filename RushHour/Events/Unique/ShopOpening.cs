using System.Collections.Generic;

namespace RushHour.Events.Unique
{
    class ShopOpening : CityEvent
    {
        public ShopOpening()
        {
            m_eventInitialisedMessages = new List<string>()
            {
                "Yet another shop opening in {0}? How many more can they fit in there? #event",
                "Finally! They're opening another shop! #event"
            };

            m_eventStartedMessages = new List<string>()
            {
                "This #shop's pretty huge! It's going to take years to get around here. #event",
                "Well, that was #underwhelming... #event"
            };

            m_eventEndedMessages = new List<string>()
            {
                "Got sooo much stuff... #event",
                "Ok... How am I going to get all this stuff home? Could someone pick me up? #shops #event"
            };
        }

        public override bool CitizenCanGo(uint citizenID, ref Citizen person)
        {
            Citizen.Education _citizenEducation = person.EducationLevel;
            Citizen.AgeGroup _citizenAge = Citizen.GetAgeGroup(person.Age);
            Citizen.Happiness _citizenHappiness = Citizen.GetHappinessLevel(Citizen.GetHappiness(person.m_health, person.m_wellbeing));
            Citizen.Wellbeing _citizenWellbeing = Citizen.GetWellbeingLevel(_citizenEducation, person.m_wellbeing);

            return _citizenAge >= Citizen.AgeGroup.Teen &&
                    _citizenHappiness >= Citizen.Happiness.Good &&
                    _citizenWellbeing >= Citizen.Wellbeing.Unhappy;
        }

        public override int GetCapacity()
        {
            return 200;
        }

        public override double GetEventLength()
        {
            return 0.5D;
        }
    }
}
