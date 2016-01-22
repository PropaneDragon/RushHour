using System.Runtime.CompilerServices;
using ColossalFramework;
using RushHour.Redirection;
using UnityEngine;

namespace RushHour.TouristHandlers
{
    [TargetType(typeof(TouristAI))]
    public class NewTouristAI
    {
        [RedirectMethod]
        public static void UpdateLocation(TouristAI thisAI, uint citizenID, ref Citizen data)
        {
            if (data.m_homeBuilding == 0 && data.m_workBuilding == 0 && (data.m_visitBuilding == 0 && data.m_instance == 0))
            {
                Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
            }
            else
            {
                switch (data.CurrentLocation)
                {
                    case Citizen.Location.Home:
                        Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
                        break;

                    case Citizen.Location.Work:
                        Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
                        break;

                    case Citizen.Location.Visit:
                        if (data.Dead || data.Sick || (int)data.m_visitBuilding == 0)
                        {
                            Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
                            break;
                        }

                        SimulationManager _simulationManager = Singleton<SimulationManager>.instance;
                        BuildingManager _buildingManager = Singleton<BuildingManager>.instance;
                        BuildingInfo _buildingInfo = _buildingManager.m_buildings.m_buffer[data.m_visitBuilding].Info;
                        float time = _simulationManager.m_currentDayTimeHour;
                        bool visitingHours = time > _simulationManager.m_randomizer.Int32(6, 8) && time < _simulationManager.m_randomizer.Int32(18, 23);
                        int reduceAmount = -100;

                        if (visitingHours)
                        {
                            int chance = _simulationManager.m_randomizer.Int32(10U);

                            if (chance == 0)
                            {
                                FindVisitPlace(thisAI, citizenID, data.m_visitBuilding, GetLeavingReason(thisAI, citizenID, ref data));
                            }
                            else if (chance > 7)
                            {
                                break;
                            }
                            else if (chance > 5)
                            {
                                if (data.m_instance != 0 || DoRandomMove(thisAI))
                                {
                                    FindVisitPlace(thisAI, citizenID, data.m_visitBuilding, GetShoppingReason(thisAI));
                                }

                                _buildingInfo.m_buildingAI.ModifyMaterialBuffer(data.m_visitBuilding, ref _buildingManager.m_buildings.m_buffer[data.m_visitBuilding], TransferManager.TransferReason.Shopping, ref reduceAmount);
                                AddTouristVisit(thisAI, citizenID, data.m_visitBuilding);
                            }
                            else if (chance > 3)
                            {
                                if (data.m_instance != 0 || DoRandomMove(thisAI))
                                {
                                    FindVisitPlace(thisAI, citizenID, data.m_visitBuilding, GetEntertainmentReason(thisAI));
                                }
                            }
                            else
                            {
                                _buildingInfo.m_buildingAI.ModifyMaterialBuffer(data.m_visitBuilding, ref _buildingManager.m_buildings.m_buffer[data.m_visitBuilding], TransferManager.TransferReason.Shopping, ref reduceAmount);
                                AddTouristVisit(thisAI, citizenID, data.m_visitBuilding);
                            }
                        }
                        else if(_buildingInfo.m_class.m_subService != ItemClass.SubService.CommercialTourist) //Not in a hotel, so may as well go home.
                        {
                            FindVisitPlace(thisAI, citizenID, data.m_visitBuilding, GetLeavingReason(thisAI, citizenID, ref data));
                        }

                        break;

                    case Citizen.Location.Moving:
                        if (data.Dead || data.Sick)
                        {
                            Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
                            break;
                        }
                        if ((int)data.m_vehicle != 0 || (int)data.m_instance != 0)
                            break;
                        Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
                        break;
                }
            }
        }

        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FindVisitPlace(TouristAI thisAI, uint citizenID, ushort sourceBuilding, TransferManager.TransferReason reason)
        {
            Debug.LogWarning("FindVisitPlace is not overridden!");
        }

        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static TransferManager.TransferReason GetLeavingReason(TouristAI thisAI, uint citizenID, ref Citizen data)
        {
            Debug.LogWarning("GetLeavingReason is not overridden!");
            return 0;
        }

        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool DoRandomMove(TouristAI thisAI)
        {
            Debug.LogWarning("DoRandomMove is not overridden!");
            return false;
        }

        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static TransferManager.TransferReason GetShoppingReason(TouristAI thisAI)
        {
            Debug.LogWarning("GetShoppingReason is not overridden!");
            return 0;
        }

        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static TransferManager.TransferReason GetEntertainmentReason(TouristAI thisAI)
        {
            Debug.LogWarning("GetEntertainmentReason is not overridden!");
            return 0;
        }

        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool AddTouristVisit(TouristAI thisAI, uint citizenID, ushort buildingID)
        {
            Debug.LogWarning("AddTouristVisit is not overridden!");
            return false;
        }
    }
}
