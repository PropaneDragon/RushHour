using ColossalFramework;
using System.Collections.Generic;

namespace RushHour.Events.Unique
{
    public class FootballGame : CityEvent
    {
        public FootballGame()
        {
            m_eventInitialisedMessages = new List<string>()
            {
                "Yeahhh. My favourite team is playing at the #stadium in {0}. Hope I can get some tikcets. #event",
                "Ahhh! Not another game at the #stadium. At least I know when to head out #shopping. {0}... #event",
                "Why is it never the team I want playing at the #stadium? Oh well, maybe next time. #event"
            };

            m_eventStartedMessages = new List<string>()
            {
                "Kickoff! See you in 90 minutes. #stadium #event"
            };

            m_eventEndedMessages = new List<string>()
            {
                "What a match! Can't wait until the next one. #stadium #event",
                "See that ludicrous display? #walkitin #event"
            };

            m_eventData.m_eventName = GetType().FullName;
        }

        public override bool CitizenCanGo(uint citizenID, ref Citizen person)
        {
            Citizen.Wealth _citizenWealth = person.WealthLevel;
            Citizen.Education _citizenEducation = person.EducationLevel;
            Citizen.Gender _citizenGender = Citizen.GetGender(citizenID);
            Citizen.Happiness _citizenHappiness = Citizen.GetHappinessLevel(Citizen.GetHappiness(person.m_health, person.m_wellbeing));
            Citizen.Wellbeing _citizenWellbeing = Citizen.GetWellbeingLevel(_citizenEducation, person.m_wellbeing);

            int percentage = Singleton<SimulationManager>.instance.m_randomizer.Int32(100);

            return  _citizenWealth < Citizen.Wealth.High &&
                    (_citizenEducation < Citizen.Education.ThreeSchools || _citizenEducation == Citizen.Education.ThreeSchools && percentage < 5) &&
                    (_citizenGender == Citizen.Gender.Male || (_citizenGender == Citizen.Gender.Female && percentage < 20)) &&
                    _citizenHappiness > Citizen.Happiness.Bad &&
                    _citizenWellbeing > Citizen.Wellbeing.Unhappy;
        }

        public override int GetCapacity()
        {
            return 6000;
        }

        public override double GetEventLength()
        {
            return 1.5D;
        }
    }
}
