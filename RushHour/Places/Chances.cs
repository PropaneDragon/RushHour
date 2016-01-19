using ColossalFramework;
using UnityEngine;

namespace RushHour.Places
{
    public static class Chances
    {
        private static float m_minSchoolHour = 6.5f, m_startSchoolHour = 8f, m_endSchoolHour = 15f, m_maxSchoolHour = 16f;
        private static float m_minWorkHour = 7.5f, m_startWorkHour = 9f, m_endWorkHour = 17f, m_maxWorkHour = 20f;

        public static uint GoOutAtNight(int age)
        {
            uint chance = 0;

            switch (Citizen.GetAgeGroup(age))
            {
                case Citizen.AgeGroup.Child:
                case Citizen.AgeGroup.Senior:
                    chance = 0;
                    break;
                case Citizen.AgeGroup.Teen:
                    chance = 10;
                    break;
                case Citizen.AgeGroup.Young:
                    chance = 5;
                    break;
                case Citizen.AgeGroup.Adult:
                    chance = 2;
                    break;
            }

            return chance;
        }

        public static bool ShouldGoToWork(ref Citizen person)
        {
            bool shouldWork = false;

            SimulationManager _simulation = Singleton<SimulationManager>.instance;
            Citizen.AgeGroup ageGroup = Citizen.GetAgeGroup(person.Age);

            float currentHour = _simulation.m_currentDayTimeHour;

            switch (ageGroup)
            {
                case Citizen.AgeGroup.Child:
                case Citizen.AgeGroup.Teen:
                    if (currentHour > m_minSchoolHour && currentHour < m_startSchoolHour)
                    {
                        uint startEarlyPercent = 5;

                        shouldWork = _simulation.m_randomizer.UInt32(100) < startEarlyPercent;
                    }
                    else if (currentHour >= m_minSchoolHour && currentHour < m_endSchoolHour)
                    {
                        shouldWork = true;
                    }
                    break;

                case Citizen.AgeGroup.Young:
                case Citizen.AgeGroup.Adult:
                    if (currentHour > m_minWorkHour && currentHour < m_startWorkHour)
                    {
                        uint startEarlyPercent = 3;

                        shouldWork = _simulation.m_randomizer.UInt32(100) < startEarlyPercent;
                    }
                    else if (currentHour >= m_minWorkHour && currentHour < m_endWorkHour)
                    {
                        shouldWork = true;
                    }
                    break;
            }

            Debug.Log("Should go to work: " + shouldWork + " (time: " + currentHour);

            return shouldWork;
        }

        public static bool ShouldReturnFromWork(ref Citizen person)
        {
            bool returnFromWork = false;

            SimulationManager _simulation = Singleton<SimulationManager>.instance;
            Citizen.AgeGroup ageGroup = Citizen.GetAgeGroup(person.Age);

            float currentHour = _simulation.m_currentDayTimeHour;

            switch (ageGroup)
            {
                case Citizen.AgeGroup.Child:
                case Citizen.AgeGroup.Teen:
                    if (currentHour >= m_endSchoolHour && currentHour < m_maxSchoolHour)
                    {
                        uint leaveOnTimePercent = 20;

                        returnFromWork = _simulation.m_randomizer.UInt32(100) < leaveOnTimePercent;
                    }
                    else if (currentHour > m_maxSchoolHour || currentHour < m_minSchoolHour)
                    {
                        returnFromWork = true;
                    }
                    break;

                case Citizen.AgeGroup.Young:
                case Citizen.AgeGroup.Adult:
                    if (currentHour >= m_endWorkHour && currentHour < m_maxWorkHour)
                    {
                        uint leaveOnTimePercent = 20;

                        returnFromWork = _simulation.m_randomizer.UInt32(100) < leaveOnTimePercent;
                    }
                    else if (currentHour > m_maxWorkHour || currentHour < m_minWorkHour)
                    {
                        returnFromWork = true;
                    }
                    break;
                default:
                    returnFromWork = true;
                    break;
            }

            return returnFromWork;
        }

        public static bool ShouldGoFindEntertainment(ref Citizen person)
        {
            bool goFindEntertainment = false;

            SimulationManager _simulation = Singleton<SimulationManager>.instance;
            Citizen.AgeGroup ageGroup = Citizen.GetAgeGroup(person.Age);
            
            float currentHour = _simulation.m_currentDayTimeHour;
            switch (ageGroup)
            {
                case Citizen.AgeGroup.Child:
                case Citizen.AgeGroup.Teen:
                case Citizen.AgeGroup.Senior:
                    if (currentHour < 15 && currentHour > 7)
                    {
                        uint wantEntertainmentPercent = 3;

                        goFindEntertainment = _simulation.m_randomizer.UInt32(100) < wantEntertainmentPercent;
                    }
                    break;

                case Citizen.AgeGroup.Young:
                case Citizen.AgeGroup.Adult:
                    if (currentHour < 18 && currentHour > 7)
                    {
                        uint wantEntertainmentPercent = 4;

                        goFindEntertainment = _simulation.m_randomizer.UInt32(100) < wantEntertainmentPercent;
                    }
                    else
                    {
                        uint goingOutAtNightPercent = GoOutAtNight(person.Age);

                        goFindEntertainment = _simulation.m_randomizer.UInt32(300) < goingOutAtNightPercent;
                    }
                    break;
            }

            return goFindEntertainment;
        }

        public static bool CanStayOut(ref Citizen person)
        {
            bool canStayOut = false;

            SimulationManager _simulation = Singleton<SimulationManager>.instance;
            Citizen.AgeGroup ageGroup = Citizen.GetAgeGroup(person.Age);

            float currentHour = _simulation.m_currentDayTimeHour;
            switch (ageGroup)
            {
                case Citizen.AgeGroup.Child:
                case Citizen.AgeGroup.Teen:
                case Citizen.AgeGroup.Senior:
                    if (currentHour < 15 && currentHour > 7)
                    {
                        canStayOut = true;
                    }
                    break;

                case Citizen.AgeGroup.Young:
                case Citizen.AgeGroup.Adult:
                    if ((currentHour < 18 && currentHour > 7) || GoOutAtNight(person.Age) != 0)
                    {
                        canStayOut = true;
                    }
                    break;
            }

            return canStayOut;
        }
    }
}
