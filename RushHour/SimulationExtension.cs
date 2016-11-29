using System;
using ColossalFramework;
using ColossalFramework.IO;
using RushHour.Events;
using RushHour.Experiments;
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

        public void EarlyUpdateData()
        {
        }

        public void LateUpdateData(SimulationManager.UpdateMode mode)
        {
        }

        public void SimulationStep(int subStep)
        {
            if (!ExperimentsToggle.GhostMode)
            {
                CityEventManager.instance.Update();

                if (ExperimentsToggle.SlowTimeProgression)
                {
                    SimulationManager _simulation = Singleton<SimulationManager>.instance;

                    if (_simulation.m_enableDayNight)
                    {
                        if (!_simulation.SimulationPaused && !_simulation.ForcedSimulationPaused)
                        {
                            float timeMultiplier = 0.25f;
                            string currentMultiplier = _simulation.m_isNightTime ? ExperimentsToggle.TimeMultiplierNight : ExperimentsToggle.TimeMultiplier;

                            if (!float.TryParse(currentMultiplier, out timeMultiplier))
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
        }

        public void UpdateData(SimulationManager.UpdateMode mode)
        {
        }
    }
}
