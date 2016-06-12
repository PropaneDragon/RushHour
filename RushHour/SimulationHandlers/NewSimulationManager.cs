using RushHour.Events;
using RushHour.Redirection;
using System;

namespace RushHour.SimulationHandlers
{
    [TargetType(typeof(SimulationManager))]
    internal class NewSimulationManager
    {
        [RedirectMethod]
        public static DateTime FrameToTime(SimulationManager thisManager, uint frame)
        {
            uint offsetFrame = frame - thisManager.m_referenceFrameIndex;
            float timeMultiplier;

            if (!float.TryParse(Experiments.ExperimentsToggle.TimeMultiplier, out timeMultiplier))
            {
                timeMultiplier = 0.25f;
            }

            float hoursOffset = offsetFrame * (SimulationManager.DAYTIME_FRAME_TO_HOUR * timeMultiplier);

            return CityEventManager.CITY_TIME.AddHours(hoursOffset);
        }
    }
}
