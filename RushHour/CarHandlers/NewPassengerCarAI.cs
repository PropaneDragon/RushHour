using ColossalFramework;
using RushHour.Redirection;
using UnityEngine;
using System.Runtime.CompilerServices;

namespace RushHour.CarHandlers
{
    [TargetType(typeof(PassengerCarAI))]
    internal class NewPassengerCarAI
    {
        [RedirectMethod]
        public static bool FindParkingSpace(ushort homeID, Vector3 refPos, Vector3 searchDir, ushort segment, float width, float length, out Vector3 parkPos, out Quaternion parkRot, out float parkOffset)
        {
            bool foundASpace = false;
            float searchRadius = Experiments.ExperimentsToggle.ParkingSearchRadius;
            uint chanceOfParkingOffRoad = 80u;
            Vector3 searchMagnitude = refPos + searchDir * 16f;

            if(!Experiments.ExperimentsToggle.ImprovedParkingAI)
            {
                searchRadius = 16f;
                chanceOfParkingOffRoad = 3u;
            }

            //Make it really unlikely they'll choose roadside parking as an option (because there's less available). It was 3 before, which is a bit silly.
            if (Singleton<SimulationManager>.instance.m_randomizer.Int32(chanceOfParkingOffRoad) == 0)
            {
                if (FindParkingSpaceRoadSide(0, segment, refPos, width - 0.2f, length, out parkPos, out parkRot, out parkOffset))
                {
                    foundASpace = true;
                }
                else if (FindParkingSpaceBuilding(homeID, 0, refPos, width, length, searchRadius, out parkPos, out parkRot))
                {
                    parkOffset = -1f;
                    foundASpace = true;
                }
            }
            else
            {
                //Searching a 100 radius, rather than 16. Means they'll actually attempt to use parking instead of just finding a few spots and resorting to pocket cars.
                //This also means they'll drive through buildings and roads to get to their space, but meh.
                if (FindParkingSpaceBuilding(homeID, 0, refPos, width, length, searchRadius, out parkPos, out parkRot))
                {
                    parkOffset = -1f;
                    foundASpace = true;
                }
                else if (FindParkingSpaceRoadSide(0, segment, refPos, width - 0.2f, length, out parkPos, out parkRot, out parkOffset))
                {
                    foundASpace = true;
                }
            }

            return foundASpace;
        }

        private static bool FindParkingSpaceBuilding(ushort homeID, ushort ignoreParked, Vector3 refPos, float width, float length, float maxDistance, out Vector3 parkPos, out Quaternion parkRot)
        {
            parkPos = Vector3.zero;
            parkRot = Quaternion.identity;

            float minimumDistanceX = refPos.x - maxDistance;
            float minimumDistanceZ = refPos.z - maxDistance;
            float maximumDistanceX = refPos.x + maxDistance;
            float maximumDistanceZ = refPos.z + maxDistance;

            int minimumGridX = Mathf.Max((int)((minimumDistanceX - 72.0d) / 64.0d + 135.0d), 0);
            int minimumGridZ = Mathf.Max((int)((minimumDistanceZ - 72.0d) / 64.0d + 135.0d), 0);
            int maximumGridX = Mathf.Min((int)((maximumDistanceX + 72.0d) / 64.0d + 135.0d), 269);
            int maximumGridZ = Mathf.Min((int)((maximumDistanceZ + 72.0d) / 64.0d + 135.0d), 269);

            BuildingManager _buildingManager = Singleton<BuildingManager>.instance;
            bool foundASpace = false;

            for (int currentGridZ = minimumGridZ; currentGridZ <= maximumGridZ; ++currentGridZ)
            {
                for (int currentGridX = minimumGridX; currentGridX <= maximumGridX; ++currentGridX)
                {
                    ushort buildingID = _buildingManager.m_buildingGrid[currentGridZ * 270 + currentGridX];
                    int loopCount = 0;

                    //Go through every building in this grid segment and find a parking space.
                    while (buildingID != 0)
                    {
                        if (FindParkingSpaceBuilding(homeID, ignoreParked, buildingID, ref _buildingManager.m_buildings.m_buffer[buildingID], refPos, width, length, ref maxDistance, ref parkPos, ref parkRot))
                        {
                            //CO missed adding a break here, so it'd just keep searching regardless
                            foundASpace = true;
                            buildingID = 0;
                            break;
                        }

                        buildingID = _buildingManager.m_buildings.m_buffer[buildingID].m_nextGridBuilding;

                        if (++loopCount >= 49152)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                            break;
                        }
                    }

                    //They also forgot to check here
                    if (foundASpace)
                    {
                        break;
                    }
                }

                //And here. I think I sped up searching for spaces by quite a bit.
                if (foundASpace)
                {
                    break;
                }
            }

            return foundASpace;
        }

        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool FindParkingSpaceBuilding(ushort homeID, ushort ignoreParked, ushort buildingID, ref Building building, Vector3 refPos, float width, float length, ref float maxDistance, ref Vector3 parkPos, ref Quaternion parkRot)
        {
            Debug.LogWarning("FindParkingSpaceBuilding is not overridden!");
            return false;
        }

        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool FindParkingSpaceRoadSide(ushort ignoreParked, ushort requireSegment, Vector3 refPos, float width, float length, out Vector3 parkPos, out Quaternion parkRot, out float parkOffset)
        {
            Debug.LogWarning("FindParkingSpaceRoadSide is not overridden!");
            parkPos = Vector3.zero;
            parkRot = new Quaternion();
            parkOffset = 0;
            return false;
        }
    }
}
