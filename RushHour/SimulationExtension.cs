using ColossalFramework;
using ColossalFramework.IO;
using RushHour.Events;
using UnityEngine;

namespace RushHour
{
    public class SimulationExtension : ISimulationManager
    {
        private int step = 0;

        public void GetData(FastList<IDataContainer> data)
        {
        }

        public string GetName()
        {
            return "Rush Hour";
        }

        public ThreadProfiler GetSimulationProfiler()
        {
            return null;
        }

        public void LateUpdateData(SimulationManager.UpdateMode mode)
        {
        }

        public void SimulationStep(int subStep)
        {
            CityEventManager.instance.Update();

            if (Experiments.ExperimentsToggle.SlowTimeProgression)
            {
                SimulationManager _simulation = Singleton<SimulationManager>.instance;

                if (_simulation.m_enableDayNight)
                {
                    if (!_simulation.SimulationPaused && !_simulation.ForcedSimulationPaused)
                    {
                        float timeMultiplier;
                        if (!float.TryParse(Experiments.ExperimentsToggle.TimeMultiplier, out timeMultiplier))
                        {
                            timeMultiplier = 0.25f;
                        }

                        if (timeMultiplier >= 1f)
                        {
                            _simulation.m_dayTimeOffsetFrames = (_simulation.m_dayTimeOffsetFrames + (uint)Mathf.RoundToInt(timeMultiplier)) % SimulationManager.DAYTIME_FRAMES;
                        }
                        else
                        {
                            if (step < Mathf.RoundToInt(1f / timeMultiplier))
                            {
                                ++step;
                                _simulation.m_dayTimeOffsetFrames = (_simulation.m_dayTimeOffsetFrames - 1u) % SimulationManager.DAYTIME_FRAMES;
                            }
                            else
                            {
                                step = 0;
                            }
                        }
                    }
                }
            }
        }

        public void UpdateData(SimulationManager.UpdateMode mode)
        {
        }
    }
}
