using ColossalFramework;

namespace RushHour.Events.Unique
{
    class Memorial : CityEvent
    {
        public override bool CitizenCanGo(uint citizenID, ref Citizen person)
        {
            Citizen.AgeGroup _citizenAge = Citizen.GetAgeGroup(person.Age);

            return _citizenAge >= Citizen.AgeGroup.Adult;
        }

        public override int GetCapacity()
        {
            return 800;
        }
    }
}
