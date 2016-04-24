using ColossalFramework;
using RushHour.Events;
using UnityEngine;

namespace RushHour.Places
{
    public static class Chances
    {
        //School based hours
        public static float m_minSchoolHour = 7.66667f, m_startSchoolHour = 8f, m_endSchoolHour = 14.83333f, m_maxSchoolHour = 15.083333f;

        //Work based hours
        public static float m_minWorkHour = 7f, m_startWorkHour = 9f, m_endWorkHour = 16.75f, m_maxWorkHour = 17.5f;

        //Lunch based hours
        public static float m_lunchBegin = 11.9f, m_lunchEnd = 12.3f;

        //Durations
        public static float m_minSchoolDuration = 1f, m_minWorkDuration = 1f, m_workTravelTime = 2f;

        //Hours to attempt to go to school, if not already at school. Don't want them travelling only to go home straight away
        public static float m_maxSchoolAttemptHour
        {
            get
            {
                return m_endSchoolHour - m_minSchoolDuration;
            }
        }
        //Hours to attempt to go to work, if not already at work. Don't want them travelling only to go home straight away
        public static float m_maxWorkAttemptHour
        {
            get
            {
                return m_endWorkHour - m_minWorkDuration;
            }
        }

        /// <summary>
        /// Is today a work day?
        /// </summary>
        /// <returns>Whether today is a work day</returns>
        public static bool WorkDay()
        {
            return !CityEventManager.instance.IsWeekend();
        }

        /// <summary>
        /// Has the person got to go work today?
        /// </summary>
        /// <param name="person">The citizen to check against.</param>
        /// <returns>Whether the person has a work day today</returns>
        public static bool WorkDay(ref Citizen person)
        {
            return person.m_workBuilding != 0 && WorkDay();
        }

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
        /// Is it a lunch hour?
        /// </summary>
        /// <returns>Whether it's lunch time</returns>
        public static bool LunchHour()
        {
            float currentTime = Singleton<SimulationManager>.instance.m_currentDayTimeHour;

            return !CityEventManager.instance.IsWeekend() && currentTime >= m_lunchBegin && currentTime < m_lunchEnd;
        }

        public static bool HoursSinceLunchHour(float hours)
        {
            float currentTime = Singleton<SimulationManager>.instance.m_currentDayTimeHour;

            return !CityEventManager.instance.IsWeekend() && currentTime < m_lunchEnd + hours;
        }

        /// <summary>
        /// Should the age group bother going out when it's dark
        /// </summary>
        /// <param name="age">Age to check</param>
        /// <returns></returns>
        public static uint GoOutAtNight(int age)
        {
            float currentTime = Singleton<SimulationManager>.instance.m_currentDayTimeHour;
            uint chance = 0u;
            uint weekendMultiplier = CityEventManager.instance.IsStillWeekend(12) ? (uint)Mathf.RoundToInt(currentTime > 12f ? 6f : Mathf.Clamp(-((currentTime * 1.5f) - 6f), 0f, 6f)) : 1u;

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
                    chance = 8 * weekendMultiplier;
                    break;
                case Citizen.AgeGroup.Adult:
                    chance = 2 * weekendMultiplier;
                    break;
            }

            return chance;
        }

        /// <summary>
        /// Should the age group bother going out when it's light out
        /// </summary>
        /// <param name="age">Age to check</param>
        /// <returns></returns>
        public static uint GoOutThroughDay(int age)
        {
            uint chance = 0u;
            uint weekendMultiplier = CityEventManager.instance.IsStillWeekend(12) ? 4u : 1u;

            switch (Citizen.GetAgeGroup(age))
            {
                case Citizen.AgeGroup.Child:
                    chance = 60;
                    break;
                case Citizen.AgeGroup.Teen:
                    chance = 13 * weekendMultiplier;
                    break;
                case Citizen.AgeGroup.Young:
                    chance = 12 * weekendMultiplier;
                    break;
                case Citizen.AgeGroup.Adult:
                    chance = 10 * weekendMultiplier;
                    break;
                case Citizen.AgeGroup.Senior:
                    chance = 6;
                    break;
            }

            return chance;
        }

        /// <summary>
        /// Returns whether the citizen should go to work or not.
        /// </summary>
        /// <param name="person">The citizen to check</param>
        /// <returns>Whether they should set off for work</returns>
        public static bool ShouldGoToWork(ref Citizen person, bool ignoreMinimumDuration = false)
        {
            bool shouldWork = false;

            if (!CityEventManager.instance.IsWeekend() && person.m_workBuilding != 0)
            {
                SimulationManager _simulation = Singleton<SimulationManager>.instance;
                Citizen.AgeGroup ageGroup = Citizen.GetAgeGroup(person.Age);

                float currentHour = _simulation.m_currentDayTimeHour;

                switch (ageGroup)
                {
                    case Citizen.AgeGroup.Child:
                    case Citizen.AgeGroup.Teen:
                        if (currentHour > m_minSchoolHour - m_workTravelTime && currentHour < m_startSchoolHour - m_workTravelTime)
                        {
                            uint startEarlyPercent = 40;

                            shouldWork = _simulation.m_randomizer.UInt32(100) < startEarlyPercent;
                        }
                        else if (currentHour >= m_startSchoolHour - m_workTravelTime && currentHour < (ignoreMinimumDuration ? m_endSchoolHour : m_maxSchoolAttemptHour))
                        {
                            shouldWork = true;
                        }
                        break;

                    case Citizen.AgeGroup.Young:
                    case Citizen.AgeGroup.Adult:
                        if (currentHour > m_minWorkHour - m_workTravelTime && currentHour < m_startWorkHour - m_workTravelTime)
                        {
                            uint startEarlyPercent = 60;

                            shouldWork = _simulation.m_randomizer.UInt32(100) < startEarlyPercent;
                        }
                    	else if (currentHour >= m_startWorkHour - m_workTravelTime && currentHour < (ignoreMinimumDuration ? m_endWorkHour : m_maxWorkAttemptHour))
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

            if (!CityEventManager.instance.IsWeekend())
            {
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
            }
            else
            {
                returnFromWork = true;
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
            float minHour = 7f, maxHour = 16f;

            switch (ageGroup)
            {
                case Citizen.AgeGroup.Child:
                    minHour = 7f;
                    maxHour = 16f;
                    break;

                case Citizen.AgeGroup.Teen:
                    minHour = 10f;
                    maxHour = 20f;
                    break;
                case Citizen.AgeGroup.Young:
                case Citizen.AgeGroup.Adult:
                    minHour = 7f;
                    maxHour = 20f;
                    break;

                case Citizen.AgeGroup.Senior:
                    minHour = 6f;
                    maxHour = 16f;
                    break;
            }

            if (currentHour > minHour && currentHour < maxHour)
            {
                uint wantEntertainmentPercent = GoOutThroughDay(person.Age);

                goFindEntertainment = _simulation.m_randomizer.UInt32(100) < wantEntertainmentPercent;
            }
            else
            {
                uint goingOutAtNightPercent = GoOutAtNight(person.Age);

                goFindEntertainment = _simulation.m_randomizer.UInt32(100) < goingOutAtNightPercent;
            }

            return goFindEntertainment;
        }

        /// <summary>
        /// Determines whether we can go to lunch
        /// </summary>
        /// <returns>Whether it's lunch time</returns>
        public static bool ShouldGoToLunch(ref Citizen person)
        {
            if (Experiments.ExperimentsToggle.SimulateLunchTimeRushHour)
            {
                SimulationManager _simulation = Singleton<SimulationManager>.instance;
                Citizen.AgeGroup ageGroup = Citizen.GetAgeGroup(person.Age);
                float currentHour = _simulation.m_currentDayTimeHour;

                if(ageGroup > Citizen.AgeGroup.Child && currentHour > m_lunchBegin && currentHour < m_lunchEnd)
                {
                    uint lunchChance = _simulation.m_randomizer.UInt32(100u);
                    return lunchChance < 60u;
                }
            }

            return false;
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
