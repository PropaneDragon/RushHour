using System.Collections.Generic;

namespace RushHour.Events.Unique
{
    class BookSigning : CityEvent
    {
        public BookSigning()
        {
            m_eventInitialisedMessages = new List<string>()
            {
                "Yes! My favourite author is in town doing book signings in {0}. Count me there. #event",
                "Ooo, a book signing #event? Shame I've never heard of the author. It's on in {0} if anyone's interested.",
                "If anyone wants to get their #book signed, the author's in town in {0} for a few hours. #event"
            };

            m_eventStartedMessages = new List<string>()
            {
                "Next in queue to get my book signed! Can't wait. #event",
                "Just got my book signed! How do I sell things online again? #event"
            };

            m_eventEndedMessages = new List<string>()
            {
                "Well, that was fun while it lasted. #event",
                "Didn't even manage to get my #book signed. Too many people :( #event"
            };
        }

        public override bool CitizenCanGo(uint citizenID, ref Citizen person)
        {
            Citizen.Wealth _citizenWealth = person.WealthLevel;
            Citizen.Education _citizenEducation = person.EducationLevel;
            Citizen.Wellbeing _citizenWellbeing = Citizen.GetWellbeingLevel(_citizenEducation, person.m_wellbeing);

            return  _citizenWealth > Citizen.Wealth.Low &&
                    _citizenEducation > Citizen.Education.Uneducated &&
                    _citizenWellbeing > Citizen.Wellbeing.VeryUnhappy;
        }

        public override int GetCapacity()
        {
            return 300;
        }

        public override double GetEventLength()
        {
            return 0.9D;
        }
    }
}
