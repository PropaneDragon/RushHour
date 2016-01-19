using ColossalFramework;
using UnityEngine;

namespace RushHour.BuildingHandlers
{
    public static class NewCommonBuildingAI
    {
        public static void SimulationStepActive(CommonBuildingAI buildingAI, ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            SimulationManager _simulationManager = Singleton<SimulationManager>.instance;
            Notification.Problem problems1 = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem.Garbage);

            if((_simulationManager.m_currentDayTimeHour < 10 || _simulationManager.m_currentDayTimeHour > 17) && buildingData.m_outgoingProblemTimer > 1)
            {
                CommercialBuildingAI commercialAI = buildingAI as CommercialBuildingAI;

                if (commercialAI != null)
                {
                    buildingData.m_outgoingProblemTimer -= 1;
                }
            }

            if ((int)buildingData.m_garbageBuffer >= 2000)
            {
                int num = (int)buildingData.m_garbageBuffer / 1000;
                if (_simulationManager.m_randomizer.Int32(5U) == 0)
                    Singleton<NaturalResourceManager>.instance.TryDumpResource(NaturalResourceManager.Resource.Pollution, num, num, buildingData.m_position, 0.0f);
                if (num >= 3)
                {
                    if (Singleton<UnlockManager>.instance.Unlocked(ItemClass.Service.Garbage))
                    {
                        problems1 = num < 6 ? Notification.AddProblems(problems1, Notification.Problem.Garbage) : Notification.AddProblems(problems1, Notification.Problem.Garbage | Notification.Problem.MajorProblem);
                        GuideController guideController = Singleton<GuideManager>.instance.m_properties;
                        if (guideController != null)
                            Singleton<GuideManager>.instance.m_serviceNeeded[ItemClass.GetPublicServiceIndex(ItemClass.Service.Garbage)].Activate(guideController.m_serviceNeeded, ItemClass.Service.Garbage);
                    }
                    else
                        buildingData.m_garbageBuffer = (ushort)2000;
                }
            }
            buildingData.m_problems = problems1;
            float radius = (float)(buildingData.Width + buildingData.Length) * 2.5f;
            if ((int)buildingData.m_crimeBuffer != 0)
                Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.CrimeRate, (int)buildingData.m_crimeBuffer, buildingData.m_position, radius);
            int fireHazard;
            int fireSize;
            int fireTolerance;
            buildingAI.GetFireParameters(buildingID, ref buildingData, out fireHazard, out fireSize, out fireTolerance);
            if (fireHazard != 0)
            {
                DistrictManager instance = Singleton<DistrictManager>.instance;
                byte district = instance.GetDistrict(buildingData.m_position);
                if ((instance.m_districts.m_buffer[(int)district].m_servicePolicies & DistrictPolicies.Services.SmokeDetectors) != DistrictPolicies.Services.None)
                    fireHazard = fireHazard * 75 / 100;
            }
            int num1 = 100 - (10 + fireTolerance) * 50000 / ((100 + fireHazard) * (100 + fireSize));
            if (num1 <= 0)
                return;
            Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.FireHazard, num1 * buildingData.Width * buildingData.Length, buildingData.m_position, radius);
        }
    }
}
