using ColossalFramework;

namespace RushHour.Events.Unique
{
    class ShopOpening : CityEvent
    {
        public override bool CitizenCanGo(uint citizenID, ref Citizen person)
        {
            Citizen.Education _citizenEducation = person.EducationLevel;
            Citizen.AgeGroup _citizenAge = Citizen.GetAgeGroup(person.Age);
            Citizen.Happiness _citizenHappiness = Citizen.GetHappinessLevel(Citizen.GetHappiness(person.m_health, person.m_wellbeing));
            Citizen.Wellbeing _citizenWellbeing = Citizen.GetWellbeingLevel(_citizenEducation, person.m_wellbeing);

            return _citizenAge >= Citizen.AgeGroup.Teen &&
                    _citizenHappiness >= Citizen.Happiness.Good &&
                    _citizenWellbeing >= Citizen.Wellbeing.Unhappy;
        }

        public override int GetCapacity()
        {
            return 200;
        }
    }
}
