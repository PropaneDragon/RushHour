using ColossalFramework;

namespace RushHour.Events.Unique
{
    class GameExpo : CityEvent
    {
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
    }
}
