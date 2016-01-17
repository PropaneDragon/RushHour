using ColossalFramework;
using UnityEngine;

namespace RushHour
{
    class NewCitizenManager
    {
        public static void SimulationStepImpl(int subStep)
        {
            CitizenManager _citizenManager = Singleton<CitizenManager>.instance;
            if (subStep != 0)
            {
                int num1 = (int)Singleton<SimulationManager>.instance.m_currentFrameIndex & 4095;
                int num2 = num1 * 256;
                int num3 = (num1 + 1) * 256 - 1;
                for (int index = num2; index <= num3; ++index)
                {
                    if ((_citizenManager.m_citizens.m_buffer[index].m_flags & Citizen.Flags.Created) != Citizen.Flags.None)
                    {
                        CitizenInfo citizenInfo = _citizenManager.m_citizens.m_buffer[index].GetCitizenInfo((uint)index);
                        if (citizenInfo == null)
                            _citizenManager.ReleaseCitizen((uint)index);
                        else
                            NewResidentAI.SimulationStep((uint)index, ref _citizenManager.m_citizens.m_buffer[index]);
                    }
                }
                if (num1 == 4095)
                {
                    _citizenManager.m_finalOldestOriginalResident = _citizenManager.m_tempOldestOriginalResident;
                    _citizenManager.m_tempOldestOriginalResident = 0;
                }
            }
            if (subStep != 0)
            {
                int num1 = (int)Singleton<SimulationManager>.instance.m_currentFrameIndex & 4095;
                int num2 = num1 * 128;
                int num3 = (num1 + 1) * 128 - 1;
                for (int index = num2; index <= num3; ++index)
                {
                    if ((_citizenManager.m_units.m_buffer[index].m_flags & CitizenUnit.Flags.Created) != CitizenUnit.Flags.None)
                        _citizenManager.m_units.m_buffer[index].SimulationStep((uint)index);
                }
            }
            if (subStep == 0)
                return;
            SimulationManager instance = Singleton<SimulationManager>.instance;
            Vector3 physicsLodRefPos = instance.m_simulationView.m_position + instance.m_simulationView.m_direction * 200f;
            int num4 = (int)Singleton<SimulationManager>.instance.m_currentFrameIndex & 15;
            int num5 = num4 * 4096;
            int num6 = (num4 + 1) * 4096 - 1;
            for (int index = num5; index <= num6; ++index)
            {
                if ((_citizenManager.m_instances.m_buffer[index].m_flags & CitizenInstance.Flags.Created) != CitizenInstance.Flags.None)
                    _citizenManager.m_instances.m_buffer[index].Info.m_citizenAI.SimulationStep((ushort)index, ref _citizenManager.m_instances.m_buffer[index], physicsLodRefPos);
            }
        }
    }
}
