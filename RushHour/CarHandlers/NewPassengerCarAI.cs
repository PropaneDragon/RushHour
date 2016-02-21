using ColossalFramework;
using RushHour.Redirection;
using UnityEngine;
using ColossalFramework.Math;
using System.Runtime.CompilerServices;

namespace RushHour.CarHandlers
{
    [TargetType(typeof(PassengerCarAI))]
    public class NewPassengerCarAI
    {
        [RedirectMethod]
        public static bool FindParkingSpace(ushort homeID, Vector3 refPos, Vector3 searchDir, ushort segment, float width, float length, out Vector3 parkPos, out Quaternion parkRot, out float parkOffset)
        {
            bool foundASpace = false;
            //Vector3 searchMagnitude = refPos + searchDir * 16f;

            parkOffset = -1f;
            parkPos = Vector3.zero;
            parkRot = new Quaternion();

            if (Singleton<SimulationManager>.instance.m_randomizer.Int32(3U) == 0)
            {
                if (false)//FindParkingSpaceRoadSide(0, segment, refPos, width - 0.2f, length, out parkPos, out parkRot, out parkOffset))
                {
                    foundASpace = true;
                }
                else if (FindParkingSpaceBuilding(homeID, 0, refPos, width, length, 16f, out parkPos, out parkRot))
                {
                    parkOffset = -1f;
                    foundASpace = true;
                }
                else
                {
                    parkOffset = -1f;
                }
            }
            else
            {
                if (FindParkingSpaceBuilding(homeID, 0, refPos, width, length, 16f, out parkPos, out parkRot))
                {
                    parkOffset = -1f;
                    foundASpace = true;
                }
                else if (false)//FindParkingSpaceRoadSide(0, segment, refPos, width - 0.2f, length, out parkPos, out parkRot, out parkOffset))
                {
                    foundASpace = true;
                }
                else
                {
                    parkOffset = -1f;
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

            int minimumGridX = Mathf.Max((int)((minimumDistanceX - 72.0) / 64.0 + 135.0), 0);
            int minimumGridZ = Mathf.Max((int)((minimumDistanceZ - 72.0) / 64.0 + 135.0), 0);
            int maximumGridX = Mathf.Min((int)((maximumDistanceX + 72.0) / 64.0 + 135.0), 269);
            int maximumGridZ = Mathf.Min((int)((maximumDistanceZ + 72.0) / 64.0 + 135.0), 269);

            BuildingManager instance = Singleton<BuildingManager>.instance;
            bool foundASpace = false;

            for (int currentGridZ = minimumGridZ; currentGridZ <= maximumGridZ; ++currentGridZ)
            {
                for (int currentGridX = minimumGridX; currentGridX <= maximumGridX; ++currentGridX)
                {
                    ushort buildingID = instance.m_buildingGrid[currentGridZ * 270 + currentGridX];
                    int loopCount = 0;
                    Building currentBuilding = instance.m_buildings.m_buffer[buildingID];

                    //Go through every building in this grid segment and find a parking space.
                    while (buildingID != 0)
                    {
                        if (FindParkingSpaceBuilding(homeID, ignoreParked, buildingID, ref currentBuilding, refPos, width, length, ref maxDistance, ref parkPos, ref parkRot))
                        {
                            foundASpace = true;
                            buildingID = 0;
                            break;
                        }

                        buildingID = currentBuilding.m_nextGridBuilding;

                        if (++loopCount >= 49152)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                            break;
                        }
                    }

                    if (foundASpace)
                    {
                        break;
                    }
                }

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

        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool FindParkingSpaceProp(ushort ignoreParked, PropInfo info, Vector3 position, float angle, bool fixedHeight, Vector3 refPos, float width, float length, ref float maxDistance, ref Vector3 parkPos, ref Quaternion parkRot)
        {
            Debug.LogWarning("FindParkingSpaceProp is not overridden!");
            return false;
        }

        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool CheckOverlap(ushort ignoreParked, Segment3 segment)
        {
            Debug.LogWarning("CheckOverlap is not overridden!");
            return false;
        }

        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool CheckOverlap(ushort ignoreParked, ref Bezier3 bezier, float offset, float length, out float minPos, out float maxPos)
        {
            Debug.LogWarning("CheckOverlap is not overridden!");
            minPos = 0;
            maxPos = 0;
            return false;
        }

        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static ushort CheckOverlap(ushort ignoreParked, ref Bezier3 bezier, Vector3 pos, Vector3 dir, float offset, float length, ushort otherID, ref VehicleParked otherData, ref bool overlap, ref float minPos, ref float maxPos)
        {
            Debug.LogWarning("CheckOverlap is not overridden!");
            minPos = 0;
            maxPos = 0;
            return 0;
        }

        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static ushort CheckOverlap(ushort ignoreParked, Segment3 segment, ushort otherID, ref VehicleParked otherData, ref bool overlap)
        {
            Debug.LogWarning("CheckOverlap is not overridden!");
            return 0;
        }
    }
}
