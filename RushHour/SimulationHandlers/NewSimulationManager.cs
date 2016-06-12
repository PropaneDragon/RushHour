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
            float timeMultiplier;

            if (!float.TryParse(Experiments.ExperimentsToggle.TimeMultiplier, out timeMultiplier))
            {
                timeMultiplier = 0.25f;
            }

            float hoursOffset = offsetFrame * (SimulationManager.DAYTIME_FRAME_TO_HOUR * timeMultiplier);
            DateTime offsetTime = CityEventManager.CITY_TIME.AddHours(hoursOffset);

            return offsetTime;
        }
    }
}
