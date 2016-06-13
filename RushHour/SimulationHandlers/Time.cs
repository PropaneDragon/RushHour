using RushHour.Experiments;

namespace RushHour.SimulationHandlers
{
    internal class Time
    {
        public static double SpeedMultiplier(double value)
        {
            double speed = 0.25d;
            double.TryParse(ExperimentsToggle.TimeMultiplier, out speed);

            if(ExperimentsToggle.SlowTimeProgression)
            {
                return value * speed;
            }

            return value;
        }

        public static float SpeedMultiplier(float value)
        {
            float speed = 0.25f;
            float.TryParse(ExperimentsToggle.TimeMultiplier, out speed);

            if (ExperimentsToggle.SlowTimeProgression)
            {
                return value * speed;
            }

            return value;
        }
    }
}
