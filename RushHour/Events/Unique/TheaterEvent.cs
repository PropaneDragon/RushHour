using ColossalFramework;

namespace RushHour.Events.Unique
{
    class TheaterEvent : CityEvent
    {
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
    }
}
