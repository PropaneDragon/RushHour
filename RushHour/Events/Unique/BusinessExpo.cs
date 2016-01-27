using System.Collections.Generic;

namespace RushHour.Events.Unique
{
    class BusinessExpo : CityEvent
    {
        public BusinessExpo()
        {
            m_eventInitialisedMessages = new List<string>()
            {
                "Anyone else going to the #business expo in {0}? I need someone to give me a #lift #event",
                "If anyone's going to the #business expo in {0}, you better get tickets quick as there's only " + GetCapacity() + " being sold. #event"
            };

            m_eventStartedMessages = new List<string>()
            {
                "Talk about a lot of stalls! I didn't even realise you could fit this many people into the Expo Center. #event",
                "The #business expo has begun! #event",
                "Can't wait to mess with those #curved TVs! #letmein #event"
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

            return  _citizenWealth == Citizen.Wealth.High &&
                    _citizenEducation >= Citizen.Education.TwoSchools &&
                    _citizenAge >= Citizen.AgeGroup.Young &&
                    _citizenHappiness >= Citizen.Happiness.Good &&
                    _citizenWellbeing > Citizen.Wellbeing.Unhappy;
        }

        public override int GetCapacity()
        {
            return 1500;
        }

        public override double GetEventLength()
        {
            return 2D;
        }
    }
}
