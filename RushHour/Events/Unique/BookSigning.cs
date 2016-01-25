using System.Collections.Generic;

namespace RushHour.Events.Unique
{
    class BookSigning : CityEvent
    {
        public BookSigning()
        {
            m_eventInitialisedMessages = new List<string>()
            {
                "Yes! My favourite author is in town doing book signings in {0}. Count me there. #event"
            };

            m_eventStartedMessages = new List<string>()
            {
                "Next in queue to get my book signed! Can't wait. #event"
            };

            m_eventEndedMessages = new List<string>()
            {
                "Well, that was fun while it lasted. #event",
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
