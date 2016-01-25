using ColossalFramework;

namespace RushHour.Events.Unique
{
    class ComicExpo : CityEvent
    {
        public override bool CitizenCanGo(uint citizenID, ref Citizen person)
        {
            Citizen.AgeGroup _citizenAge = Citizen.GetAgeGroup(person.Age);
            Citizen.Gender _citizenGender = Citizen.GetGender(citizenID);

            int percentage = Singleton<SimulationManager>.instance.m_randomizer.Int32(100);

            return (_citizenGender == Citizen.Gender.Male || (_citizenGender == Citizen.Gender.Female && percentage < 20)) &&
                    _citizenAge == Citizen.AgeGroup.Young || _citizenAge == Citizen.AgeGroup.Teen;
        }

        public override int GetCapacity()
        {
            return 4000;
        }
    }
}
