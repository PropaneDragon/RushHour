using RushHour.Events;
using RushHour.Redirection;
using System;
using UnityEngine;

namespace RushHour.SimulationHandlers
{
    [TargetType(typeof(SimulationManager))]
    internal class NewSimulationManager
    {
        [RedirectMethod]
        public static DateTime FrameToTime(SimulationManager thisManager, uint frame)
        {
            long offsetFrame = (int)frame - (int)thisManager.m_referenceFrameIndex;

            float hoursOffset = offsetFrame * Time.SpeedMultiplier(SimulationManager.DAYTIME_FRAME_TO_HOUR);
            DateTime offsetTime = CityEventManager.CITY_TIME.AddHours(hoursOffset);

            return offsetTime;
        }
    }
}
