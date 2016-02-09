using ColossalFramework;
using RushHour.Events;
using System;

namespace RushHour.Places
{
    public static class Chances
    {
        //School based hours
        public static float m_minSchoolHour = 6.5f, m_startSchoolHour = 7f, m_endSchoolHour = 14.9f, m_maxSchoolHour = 15.1f;

        //Work based hours
        public static float m_minWorkHour = 6f, m_startWorkHour = 8f, m_endWorkHour = 17f, m_maxWorkHour = 17.5f;

        //Hours to attempt to go to work, if not already at work. Don't want them travelling only to go home straight away
        public static float m_maxSchoolAttemptHour = m_endSchoolHour - 2f, m_maxWorkAttemptHour = m_endWorkHour - 3f;

        /// <summary>
        /// Is it a work hour?
        /// </summary>
        /// <returns>Whether it's a work hour.</returns>
        public static bool WorkHour()
        {
            float currentTime = Singleton<SimulationManager>.instance.m_currentDayTimeHour;

            return !CityEventManager.instance.IsWeekend() && currentTime >= m_startWorkHour && currentTime < m_endWorkHour;
        }

        /// <summary>
        /// Is it a school hour?
        /// </summary>
        /// <returns>Whether it's a school hour</returns>
        public static bool SchoolHour()
        {
            float currentTime = Singleton<SimulationManager>.instance.m_currentDayTimeHour;

            return !CityEventManager.instance.IsWeekend() && currentTime >= m_startSchoolHour && currentTime < m_endSchoolHour;
        }

        /// <summary>
        /// Should the age group bother going out when it's dark
        /// </summary>
        /// <param name="age">Age to check</param>
        /// <returns></returns>
        public static uint GoOutAtNight(int age)
        {
            uint chance = 0u;
            uint weekendMultiplier = CityEventManager.instance.IsWeekend() ? 3u : 1u;

            switch (Citizen.GetAgeGroup(age))
            {
                case Citizen.AgeGroup.Child:
                case Citizen.AgeGroup.Senior:
                    chance = 0;
                    break;
                case Citizen.AgeGroup.Teen:
                    chance = 10 * weekendMultiplier;
                    break;
                case Citizen.AgeGroup.Young:
                    chance = 5 * weekendMultiplier;
                    break;
                case Citizen.AgeGroup.Adult:
                    chance = 2 * weekendMultiplier;
                    break;
            }

            return chance;
        }

        /// <summary>
        /// Returns whether the citizen should go to work or not.
        /// </summary>
        /// <param name="person">The citizen to check</param>
        /// <returns>Whether they should set off for work</returns>
        public static bool ShouldGoToWork(ref Citizen person)
        {
            bool shouldWork = false;

            if (!CityEventManager.instance.IsWeekend())
            {
                SimulationManager _simulation = Singleton<SimulationManager>.instance;
                Citizen.AgeGroup ageGroup = Citizen.GetAgeGroup(person.Age);

                float currentHour = _simulation.m_currentDayTimeHour;

                switch (ageGroup)
                {
                    case Citizen.AgeGroup.Child:
                    case Citizen.AgeGroup.Teen:
                        if (currentHour > m_minSchoolHour && currentHour < m_startSchoolHour)
                        {
                            uint startEarlyPercent = 40;

                            shouldWork = _simulation.m_randomizer.UInt32(100) < startEarlyPercent;
                        }
                        else if (currentHour >= m_startSchoolHour && currentHour < m_maxSchoolAttemptHour)
                        {
                            shouldWork = true;
                        }
                        break;

                    case Citizen.AgeGroup.Young:
                    case Citizen.AgeGroup.Adult:
                        if (currentHour > m_minWorkHour && currentHour < m_startWorkHour)
                        {
                            uint startEarlyPercent = 60;

                            shouldWork = _simulation.m_randomizer.UInt32(100) < startEarlyPercent;
                        }
                    	else if (currentHour >= m_startWorkHour && currentHour < m_maxWorkAttemptHour)
                        {
                            shouldWork = true;
                        }
                        break;
                }
            }

            return shouldWork;
        }

        /// <summary>
        /// Check whether the citizen is done with their day at work
        /// </summary>
        /// <param name="person">The citizen to check</param>
        /// <returns>Whether they can leave work</returns>
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
                        uint leaveOnTimePercent = 80;

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
                        uint leaveOnTimePercent = 50;

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

        /// <summary>
        /// Check whether the citizen wants to go find some entertainment or not
        /// </summary>
        /// <param name="person">The citizen to check</param>
        /// <returns>Whether this citizen would like some entertainment</returns>
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
                    if (currentHour < 16 && currentHour > 7)
                    {
                        uint wantEntertainmentPercent = 3;

                        goFindEntertainment = _simulation.m_randomizer.UInt32(100) < wantEntertainmentPercent;
                    }
                    break;

                case Citizen.AgeGroup.Young:
                case Citizen.AgeGroup.Adult:
                    if (currentHour < 19 && currentHour > 7)
                    {
                        uint wantEntertainmentPercent = 4;

                        goFindEntertainment = _simulation.m_randomizer.UInt32(100) < wantEntertainmentPercent;
                    }
                    else
                    {
                        uint goingOutAtNightPercent = GoOutAtNight(person.Age);

                        goFindEntertainment = _simulation.m_randomizer.UInt32(700) < goingOutAtNightPercent;
                    }
                    break;
            }

            return goFindEntertainment;
        }

        /// <summary>
        /// Checks whether the citizen can stay out, or whether they'd prefer to come home
        /// </summary>
        /// <param name="person">The citizen to check</param>
        /// <returns>Whether the citizen would be able to stay out or not</returns>
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
                    if (currentHour < 16 && currentHour > 7)
                    {
                        canStayOut = true;
                    }
                    break;

                case Citizen.AgeGroup.Young:
                case Citizen.AgeGroup.Adult:
                    if ((currentHour < 19 && currentHour > 7) || GoOutAtNight(person.Age) != 0)
                    {
                        canStayOut = true;
                    }
                    break;
            }

            return canStayOut;
        }
    }
}
