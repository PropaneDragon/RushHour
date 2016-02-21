using ColossalFramework;
using RushHour.Redirection;
using UnityEngine;
using ColossalFramework.Math;

namespace RushHour.CarHandlers
{
    [TargetType(typeof(PassengerCarAI))]
    public class NewPassengerCarAI
    {
        [RedirectMethod]
        public static bool FindParkingSpace(PassengerCarAI thisAI, ushort homeID, Vector3 refPos, Vector3 searchDir, ushort segment, float width, float length, out Vector3 parkPos, out Quaternion parkRot, out float parkOffset)
        {
            bool foundASpace = false;
            //Vector3 searchMagnitude = refPos + searchDir * 16f;

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

                    if(foundASpace)
                    {
                        break;
                    }
                }

                if(foundASpace)
                {
                    break;
                }
            }

            return foundASpace;
        }
        
        private static bool FindParkingSpaceBuilding(ushort homeID, ushort ignoreParked, ushort buildingID, ref Building building, Vector3 refPos, float width, float length, ref float maxDistance, ref Vector3 parkPos, ref Quaternion parkRot)
        {
            int buildingWidth = building.Width;
            int buildingLength = building.Length;

            float area = Mathf.Sqrt((buildingWidth * buildingWidth + buildingLength * buildingLength)) * 8f;

            if ((double)VectorUtils.LengthXZ(building.m_position - refPos) >= (double)maxDistance + (double)area || (building.m_flags & Building.Flags.BurnedDown) != Building.Flags.None)
            {
                return false;
            }

            BuildingInfo info = building.Info;
            Matrix4x4 matrix4x4 = new Matrix4x4();
            bool flag1 = false;
            bool flag2 = false;
            if (info.m_class.m_service == ItemClass.Service.Residential && (int)buildingID != (int)homeID || info.m_props == null)
                return flag2;
            for (int index = 0; index < info.m_props.Length; ++index)
            {
                BuildingInfo.Prop prop = info.m_props[index];
                Randomizer r = new Randomizer((int)buildingID << 6 | prop.m_index);
                if (r.Int32(100U) < prop.m_probability && buildingLength >= prop.m_requiredLength)
                {
                    PropInfo propInfo = prop.m_finalProp;
                    if (propInfo != null)
                    {
                        PropInfo variation = propInfo.GetVariation(ref r);
                        if (variation.m_parkingSpaces != null && variation.m_parkingSpaces.Length != 0)
                        {
                            if (!flag1)
                            {
                                flag1 = true;
                                Vector3 pos = Building.CalculateMeshPosition(info, building.m_position, building.m_angle, building.Length);
                                Quaternion q = Quaternion.AngleAxis(building.m_angle * 57.29578f, Vector3.down);
                                matrix4x4.SetTRS(pos, q, Vector3.one);
                            }
                            Vector3 position = matrix4x4.MultiplyPoint(prop.m_position);
                            if (FindParkingSpaceProp(ignoreParked, variation, position, building.m_angle + prop.m_radAngle, prop.m_fixedHeight, refPos, width, length, ref maxDistance, ref parkPos, ref parkRot))
                                flag2 = true;
                        }
                    }
                }
            }
            return flag2;
        }
        
        private static bool FindParkingSpaceProp(ushort ignoreParked, PropInfo info, Vector3 position, float angle, bool fixedHeight, Vector3 refPos, float width, float length, ref float maxDistance, ref Vector3 parkPos, ref Quaternion parkRot)
        {
            bool flag = false;
            Matrix4x4 matrix4x4 = new Matrix4x4();
            Quaternion q = Quaternion.AngleAxis(angle * 57.29578f, Vector3.down);
            matrix4x4.SetTRS(position, q, Vector3.one);
            for (int index = 0; index < info.m_parkingSpaces.Length; ++index)
            {
                Vector3 a = matrix4x4.MultiplyPoint(info.m_parkingSpaces[index].m_position);
                float num1 = Vector3.Distance(a, refPos);
                if ((double)num1 < (double)maxDistance)
                {
                    float num2 = (float)(((double)info.m_parkingSpaces[index].m_size.z - (double)length) * 0.5);
                    Vector3 forward = matrix4x4.MultiplyVector(info.m_parkingSpaces[index].m_direction);
                    Vector3 vector3_1 = a + forward * num2;
                    if (fixedHeight)
                    {
                        Vector3 vector3_2 = forward * (float)((double)length * 0.5 - 1.0);
                        Segment3 segment = new Segment3(vector3_1 + vector3_2, vector3_1 - vector3_2);
                        if (!CheckOverlap(ignoreParked, segment))
                        {
                            parkPos = vector3_1;
                            parkRot = Quaternion.LookRotation(forward);
                            maxDistance = num1;
                            flag = true;
                        }
                    }
                    else
                    {
                        Vector3 worldPos1 = vector3_1 + new Vector3((float)((double)forward.x * (double)length * 0.25 + (double)forward.z * (double)width * 0.400000005960464), 0.0f, (float)((double)forward.z * (double)length * 0.25 - (double)forward.x * (double)width * 0.400000005960464));
                        Vector3 worldPos2 = vector3_1 + new Vector3((float)((double)forward.x * (double)length * 0.25 - (double)forward.z * (double)width * 0.400000005960464), 0.0f, (float)((double)forward.z * (double)length * 0.25 + (double)forward.x * (double)width * 0.400000005960464));
                        Vector3 worldPos3 = vector3_1 - new Vector3((float)((double)forward.x * (double)length * 0.25 - (double)forward.z * (double)width * 0.400000005960464), 0.0f, (float)((double)forward.z * (double)length * 0.25 + (double)forward.x * (double)width * 0.400000005960464));
                        Vector3 worldPos4 = vector3_1 - new Vector3((float)((double)forward.x * (double)length * 0.25 + (double)forward.z * (double)width * 0.400000005960464), 0.0f, (float)((double)forward.z * (double)length * 0.25 - (double)forward.x * (double)width * 0.400000005960464));
                        worldPos1.y = Singleton<TerrainManager>.instance.SampleDetailHeight(worldPos1);
                        worldPos2.y = Singleton<TerrainManager>.instance.SampleDetailHeight(worldPos2);
                        worldPos3.y = Singleton<TerrainManager>.instance.SampleDetailHeight(worldPos3);
                        worldPos4.y = Singleton<TerrainManager>.instance.SampleDetailHeight(worldPos4);
                        vector3_1.y = (float)(((double)worldPos1.y + (double)worldPos2.y + (double)worldPos3.y + (double)worldPos4.y) * 0.25);
                        Vector3 normalized = (worldPos1 + worldPos2 - worldPos3 - worldPos4).normalized;
                        Vector3 vector3_2 = normalized * (float)((double)length * 0.5 - 1.0);
                        Segment3 segment = new Segment3(vector3_1 + vector3_2, vector3_1 - vector3_2);
                        if (!CheckOverlap(ignoreParked, segment))
                        {
                            Vector3 rhs = worldPos1 + worldPos3 - worldPos2 - worldPos4;
                            parkPos = vector3_1;
                            parkRot = Quaternion.LookRotation(normalized, Vector3.Cross(normalized, rhs));
                            maxDistance = num1;
                            flag = true;
                        }
                    }
                }
            }
            return flag;
        }
        
        private static bool CheckOverlap(ushort ignoreParked, Segment3 segment)
        {
            VehicleManager instance = Singleton<VehicleManager>.instance;
            Vector3 vector3_1 = segment.Min();
            Vector3 vector3_2 = segment.Max();
            int num1 = Mathf.Max((int)(((double)vector3_1.x - 10.0) / 32.0 + 270.0), 0);
            int num2 = Mathf.Max((int)(((double)vector3_1.z - 10.0) / 32.0 + 270.0), 0);
            int num3 = Mathf.Min((int)(((double)vector3_2.x + 10.0) / 32.0 + 270.0), 539);
            int num4 = Mathf.Min((int)(((double)vector3_2.z + 10.0) / 32.0 + 270.0), 539);
            bool overlap = false;
            for (int index1 = num2; index1 <= num4; ++index1)
            {
                for (int index2 = num1; index2 <= num3; ++index2)
                {
                    ushort otherID = instance.m_parkedGrid[index1 * 540 + index2];
                    int num5 = 0;
                    while ((int)otherID != 0)
                    {
                        otherID = CheckOverlap(ignoreParked, segment, otherID, ref instance.m_parkedVehicles.m_buffer[(int)otherID], ref overlap);
                        if (++num5 > 32768)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            return overlap;
        }
        
        private static bool FindParkingSpaceRoadSide(ushort ignoreParked, ushort requireSegment, Vector3 refPos, float width, float length, out Vector3 parkPos, out Quaternion parkRot, out float parkOffset)
        {
            parkPos = Vector3.zero;
            parkRot = Quaternion.identity;
            parkOffset = 0.0f;
            PathUnit.Position pathPos;

            if (PathManager.FindPathPosition(refPos, ItemClass.Service.Road, NetInfo.LaneType.Parking, VehicleInfo.VehicleType.Car, false, false, 32f, out pathPos) && ((int)requireSegment == 0 || (int)pathPos.m_segment == (int)requireSegment))
            {
                NetManager _netManager = Singleton<NetManager>.instance;
                NetInfo _netInfo = _netManager.m_segments.m_buffer[(int)pathPos.m_segment].Info;

                uint laneId = PathManager.GetLaneID(pathPos);
                uint laneCount = _netManager.m_segments.m_buffer[pathPos.m_segment].m_lanes;

                for (int currentLane = 0; currentLane < _netInfo.m_lanes.Length && laneCount != 0; ++currentLane)
                {
                    if ((_netManager.m_lanes.m_buffer[laneCount].m_flags & 768) != 0 && _netInfo.m_lanes[pathPos.m_lane].m_position >= 0.0 == _netInfo.m_lanes[currentLane].m_position >= 0.0)
                    {
                        return false;
                    }

                    laneCount = _netManager.m_lanes.m_buffer[laneCount].m_nextLane;
                }

                bool invert = (_netManager.m_segments.m_buffer[pathPos.m_segment].m_flags & NetSegment.Flags.Invert) != NetSegment.Flags.None;
                bool backwards = (_netInfo.m_lanes[pathPos.m_lane].m_finalDirection & NetInfo.Direction.Forward) == NetInfo.Direction.None;
                bool positionBehind = _netInfo.m_lanes[pathPos.m_lane].m_position < 0.0;
                float offset = pathPos.m_offset * 0.003921569f;
                float minPos1, maxPos1;

                if (CheckOverlap(ignoreParked, ref _netManager.m_lanes.m_buffer[laneId].m_bezier, offset, length, out minPos1, out maxPos1))
                {
                    offset = -1f;

                    for (int index = 0; index < 6; ++index)
                    {
                        float minPos2, maxPos2;

                        if (maxPos1 <= 1.0)
                        {
                            if (CheckOverlap(ignoreParked, ref _netManager.m_lanes.m_buffer[laneId].m_bezier, maxPos1, length, out minPos2, out maxPos2))
                            {
                                maxPos1 = maxPos2;
                            }
                            else
                            {
                                offset = maxPos1;
                                break;
                            }
                        }
                        if (minPos1 >= 0.0)
                        {
                            if (CheckOverlap(ignoreParked, ref _netManager.m_lanes.m_buffer[laneId].m_bezier, minPos1, length, out minPos2, out maxPos2))
                            {
                                minPos1 = minPos2;
                            }
                            else
                            {
                                offset = minPos1;
                                break;
                            }
                        }
                    }
                }

                if (offset >= 0.0)
                {
                    Vector3 position, direction;
                    _netManager.m_lanes.m_buffer[laneId].CalculatePositionAndDirection(offset, out position, out direction);

                    float halfWidth = (float)((_netInfo.m_lanes[pathPos.m_lane].m_width - (double)width) * 0.5);
                    direction.Normalize();

                    if (invert != positionBehind)
                    {
                        parkPos.x = position.x - direction.z * halfWidth;
                        parkPos.y = position.y;
                        parkPos.z = position.z + direction.x * halfWidth;
                    }
                    else
                    {
                        parkPos.x = position.x + direction.z * halfWidth;
                        parkPos.y = position.y;
                        parkPos.z = position.z - direction.x * halfWidth;
                    }

                    parkRot = invert == backwards ? Quaternion.LookRotation(direction) : Quaternion.LookRotation(-direction);
                    parkOffset = offset;
                    return true;
                }
            }

            return false;
        }

        private static bool CheckOverlap(ushort ignoreParked, ref Bezier3 bezier, float offset, float length, out float minPos, out float maxPos)
        {
            VehicleManager instance = Singleton<VehicleManager>.instance;
            float t1 = bezier.Travel(offset, length * -0.5f);
            float t2 = bezier.Travel(offset, length * 0.5f);
            bool overlap = false;
            minPos = offset;
            maxPos = offset;
            if ((double)t1 < 1.0 / 1000.0)
            {
                overlap = true;
                t1 = 0.0f;
                minPos = -1f;
                maxPos = Mathf.Max(maxPos, bezier.Travel(0.0f, (float)((double)length * 0.5 + 0.5)));
            }
            if ((double)t2 > 0.999000012874603)
            {
                overlap = true;
                t2 = 1f;
                maxPos = 2f;
                minPos = Mathf.Min(minPos, bezier.Travel(1f, (float)((double)length * -0.5 - 0.5)));
            }
            Vector3 pos = bezier.Position(offset);
            Vector3 dir = bezier.Tangent(offset);
            Vector3 lhs = bezier.Position(t1);
            Vector3 rhs = bezier.Position(t2);
            Vector3 vector3_1 = Vector3.Min(lhs, rhs);
            Vector3 vector3_2 = Vector3.Max(lhs, rhs);
            int num1 = Mathf.Max((int)(((double)vector3_1.x - 10.0) / 32.0 + 270.0), 0);
            int num2 = Mathf.Max((int)(((double)vector3_1.z - 10.0) / 32.0 + 270.0), 0);
            int num3 = Mathf.Min((int)(((double)vector3_2.x + 10.0) / 32.0 + 270.0), 539);
            int num4 = Mathf.Min((int)(((double)vector3_2.z + 10.0) / 32.0 + 270.0), 539);
            for (int index1 = num2; index1 <= num4; ++index1)
            {
                for (int index2 = num1; index2 <= num3; ++index2)
                {
                    ushort otherID = instance.m_parkedGrid[index1 * 540 + index2];
                    int num5 = 0;
                    while ((int)otherID != 0)
                    {
                        otherID = CheckOverlap(ignoreParked, ref bezier, pos, dir, offset, length, otherID, ref instance.m_parkedVehicles.m_buffer[(int)otherID], ref overlap, ref minPos, ref maxPos);
                        if (++num5 > 32768)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + System.Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            return overlap;
        }

        private static ushort CheckOverlap(ushort ignoreParked, ref Bezier3 bezier, Vector3 pos, Vector3 dir, float offset, float length, ushort otherID, ref VehicleParked otherData, ref bool overlap, ref float minPos, ref float maxPos)
        {
            if ((int)otherID != (int)ignoreParked)
            {
                VehicleInfo info = otherData.Info;
                Vector3 lhs = otherData.m_position - pos;
                float num1 = (float)(((double)length + (double)info.m_generatedInfo.m_size.z) * 0.5 + 1.0);
                float magnitude = lhs.magnitude;
                if ((double)magnitude < (double)num1 - 0.5)
                {
                    overlap = true;
                    float distance;
                    float num2;
                    if ((double)Vector3.Dot(lhs, dir) >= 0.0)
                    {
                        distance = num1 + magnitude;
                        num2 = num1 - magnitude;
                    }
                    else
                    {
                        distance = num1 - magnitude;
                        num2 = num1 + magnitude;
                    }
                    maxPos = Mathf.Max(maxPos, bezier.Travel(offset, distance));
                    minPos = Mathf.Min(minPos, bezier.Travel(offset, -num2));
                }
            }
            return otherData.m_nextGridParked;
        }

        private static ushort CheckOverlap(ushort ignoreParked, Segment3 segment, ushort otherID, ref VehicleParked otherData, ref bool overlap)
        {
            if ((int)otherID != (int)ignoreParked)
            {
                VehicleInfo info = otherData.Info;
                Vector3 vector3 = otherData.m_rotation * new Vector3(0.0f, 0.0f, (float)((double)info.m_generatedInfo.m_size.z * 0.5 - 1.0));
                Segment3 segment1 = new Segment3(otherData.m_position + vector3, otherData.m_position - vector3);
                float u;
                float v;
                if ((double)segment.DistanceSqr(segment1, out u, out v) < 1.0)
                    overlap = true;
            }
            return otherData.m_nextGridParked;
        }
    }
}
