using System.Collections.Generic;

namespace RushHour.Events.Unique
{
    class AquariumEvent : CityEvent
    {
        public AquariumEvent()
        {
            m_eventInitialisedMessages = new List<string>()
            {
                "Can't wait to see the fish at the #aquarium in {0}! #event #fishhh",
                "Wait what? The #aquarium has an event on in {0}? Count me in! #event",
                "Excited to see the #dolphin whisperer at the Aquarium #event in {0}",
                "FYI, tickets are almost sold out for the #event at the Aquarium in {0}."
            };

            m_eventStartedMessages = new List<string>()
            {
                "Yay! The #event is starting at the #aquarium! Time to stop #chirping for a while",
                "That's a lot of fish! #aquarium #event",
                "We're going to need a bigger boat... #aquarium #event"
            };

            m_eventEndedMessages = new List<string>()
            {
                "Awwww, the event's over already? I was enjoying myself :( #event",
                "So long and thanks for all the #fish! #event"
            };
        }

        public override bool CitizenCanGo(uint citizenID, ref Citizen person)
        {
            Citizen.Wealth _citizenWealth = person.WealthLevel;
            Citizen.Education _citizenEducation = person.EducationLevel;
            Citizen.Happiness _citizenHappiness = Citizen.GetHappinessLevel(Citizen.GetHappiness(person.m_health, person.m_wellbeing));
            Citizen.Wellbeing _citizenWellbeing = Citizen.GetWellbeingLevel(_citizenEducation, person.m_wellbeing);

            return _citizenWealth > Citizen.Wealth.Low &&
                    _citizenHappiness > Citizen.Happiness.Poor &&
                    _citizenWellbeing > Citizen.Wellbeing.VeryUnhappy;
        }

        public override int GetCapacity()
        {
            return 400;
        }

        public override double GetEventLength()
        {
            return 1D;
        }
    }
}
