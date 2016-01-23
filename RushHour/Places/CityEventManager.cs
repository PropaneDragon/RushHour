using ColossalFramework;
using System;
using UnityEngine;

namespace RushHour.Places
{
    public class CityEventManager
    {
        private static CityEventManager _instance = null;
        private static float _lastDayTimeHour = 0F;

        public static DateTime CITY_TIME = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
        public static CityEventManager instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new CityEventManager();
                }

                return _instance;
            }
        }

        public void Update()
        {
            SimulationManager _simulationManager = Singleton<SimulationManager>.instance;
            float currentHour = _simulationManager.m_currentDayTimeHour;

            float difference = (currentHour >= _lastDayTimeHour ? currentHour : currentHour + 24) - _lastDayTimeHour;
            _lastDayTimeHour = currentHour;

            CITY_TIME = CITY_TIME.AddHours(difference);

            Debug.Log(CITY_TIME.ToLongTimeString() + ", " + CITY_TIME.ToShortDateString());
        }
    }
}