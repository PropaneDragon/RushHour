
using ColossalFramework;

namespace RushHour.Events.Unique
{
    class BookSigning : CityEvent
    {
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
    }
}
