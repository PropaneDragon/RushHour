using ColossalFramework;
using System;

namespace RushHour.Events.Unique
{
    public class FootballGame : CityEvent
    {
        public FootballGame()
        {
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
            return 9000;
        }
    }
}
