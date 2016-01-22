using System.Runtime.CompilerServices;
using ColossalFramework.Math;
using RushHour.Places;
using RushHour.Redirection;
using UnityEngine;

namespace RushHour.BuildingHandlers
{
    [TargetType(typeof(PrivateBuildingAI))]
    public static class NewPrivateBuildingAI
    {
        [RedirectMethod]
        public static int HandleWorkers(PrivateBuildingAI thisAI, ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, ref int aliveWorkerCount, ref int totalWorkerCount, ref int workPlaceCount)
        {
            //Not messed with this code too much yet. Still requires cleaning up.

            int b = 0;
            int level0, level1, level2, level3;

            GetWorkBehaviour(thisAI, buildingID, ref buildingData, ref behaviour, ref aliveWorkerCount, ref totalWorkerCount);
            thisAI.CalculateWorkplaceCount(new Randomizer((int)buildingID), buildingData.Width, buildingData.Length, out level0, out level1, out level2, out level3);
            workPlaceCount = level0 + level1 + level2 + level3;

            if ((int)buildingData.m_fireIntensity == 0)
            {
                HandleWorkPlaces(thisAI, buildingID, ref buildingData, level0, level1, level2, level3, ref behaviour, aliveWorkerCount, totalWorkerCount);
                if (aliveWorkerCount != 0 && workPlaceCount != 0)
                {
                    int num = (behaviour.m_efficiencyAccumulation + aliveWorkerCount - 1) / aliveWorkerCount;
                    b = 2 * num - 200 * num / ((100 * aliveWorkerCount + workPlaceCount - 1) / workPlaceCount + 100);
                }
            }

            Notification.Problem problems1 = Notification.RemoveProblems(buildingData.m_problems, Notification.Problem.NoWorkers | Notification.Problem.NoEducatedWorkers);
            int num1 = (level3 * 300 + level2 * 200 + level1 * 100) / (workPlaceCount + 1);
            int num2 = (behaviour.m_educated3Count * 300 + behaviour.m_educated2Count * 200 + behaviour.m_educated1Count * 100) / (aliveWorkerCount + 1);

            if (Chances.WorkHour())
            {
                if (aliveWorkerCount < workPlaceCount >> 1)
                {
                    buildingData.m_workerProblemTimer = (byte)Mathf.Min((int)byte.MaxValue, (int)buildingData.m_workerProblemTimer + 1);
                    if ((int)buildingData.m_workerProblemTimer >= 128)
                        problems1 = Notification.AddProblems(problems1, Notification.Problem.NoWorkers | Notification.Problem.MajorProblem);
                    else if ((int)buildingData.m_workerProblemTimer >= 64)
                        problems1 = Notification.AddProblems(problems1, Notification.Problem.NoWorkers);
                }
                else if (num2 < num1 - 50)
                {
                    buildingData.m_workerProblemTimer = (byte)Mathf.Min((int)byte.MaxValue, (int)buildingData.m_workerProblemTimer + 1);
                    if ((int)buildingData.m_workerProblemTimer >= 128)
                        problems1 = Notification.AddProblems(problems1, Notification.Problem.NoEducatedWorkers | Notification.Problem.MajorProblem);
                    else if ((int)buildingData.m_workerProblemTimer >= 64)
                        problems1 = Notification.AddProblems(problems1, Notification.Problem.NoEducatedWorkers);
                }
                else
                    buildingData.m_workerProblemTimer = (byte)0;
            }

            buildingData.m_problems = problems1;
            return Mathf.Max(1, b);
        }

        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void SimulationStepActive(PrivateBuildingAI baseAI, ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
        {
            Debug.LogWarning("SimulationStepActive is not overridden!");
        }

        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void GetWorkBehaviour(PrivateBuildingAI thisAI, ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, ref int aliveCount, ref int totalCount)
        {
            Debug.LogWarning("GetWorkBehaviour is not overridden!");
        }

        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void HandleWorkPlaces(PrivateBuildingAI thisAI, ushort buildingID, ref Building data, int workPlaces0, int workPlaces1, int workPlaces2, int workPlaces3, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount)
        {
            Debug.LogWarning("HandleWorkPlaces is not overridden!");
        }
    }
}
