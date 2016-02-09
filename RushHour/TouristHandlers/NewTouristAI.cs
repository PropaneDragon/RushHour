using System.Runtime.CompilerServices;
using ColossalFramework;
using RushHour.Redirection;
using UnityEngine;
using RushHour.Events;
using RushHour.ResidentHandlers;
using RushHour.Experiments;

namespace RushHour.TouristHandlers
{
    [TargetType(typeof(TouristAI))]
    public class NewTouristAI
    {
        [RedirectMethod]
        public static void UpdateLocation(TouristAI thisAI, uint citizenID, ref Citizen person)
        {
            if (person.m_homeBuilding == 0 && person.m_workBuilding == 0 && (person.m_visitBuilding == 0 && person.m_instance == 0))
            {
                Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
            }
            else
            {
                switch (person.CurrentLocation)
                {
                    case Citizen.Location.Home:
                        Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
                        break;

                    case Citizen.Location.Work:
                        Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
                        break;

                    case Citizen.Location.Visit:
                        if (person.Dead || person.Sick || (int)person.m_visitBuilding == 0)
                        {
                            Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
                            break;
                        }

                        SimulationManager _simulationManager = Singleton<SimulationManager>.instance;
                        BuildingManager _buildingManager = Singleton<BuildingManager>.instance;
                        Building _currentBuilding = _buildingManager.m_buildings.m_buffer[person.m_visitBuilding];
                        BuildingInfo _buildingInfo = _currentBuilding.Info;

                        float time = _simulationManager.m_currentDayTimeHour;
                        bool visitingHours = time > _simulationManager.m_randomizer.Int32(6, 8) && time < _simulationManager.m_randomizer.Int32(18, 23);
                        int reduceAmount = -100;

                        if (!CityEventManager.instance.EventTakingPlace(person.m_visitBuilding) && !CityEventManager.instance.EventStartsWithin(person.m_visitBuilding, 2D))
                        {
                            int eventId = CityEventManager.instance.EventStartsWithin(citizenID, ref person, ResidentLocationHandler._startMovingToEventTime);

                            if (eventId != -1)
                            {
                                CityEvent _cityEvent = CityEventManager.instance.m_nextEvents[eventId];

                                if (_cityEvent.EventStartsWithin(ResidentLocationHandler._startMovingToEventTime) && !_cityEvent.EventStartsWithin(ResidentLocationHandler._maxMoveToEventTime))
                                {
                                    if ((person.m_instance != 0 || DoRandomMove(thisAI)) && _cityEvent.Register())
                                    {
                                        StartMoving(thisAI, citizenID, ref person, person.m_visitBuilding, _cityEvent.m_eventData.m_eventBuilding);
                                        person.SetVisitplace(citizenID, _cityEvent.m_eventData.m_eventBuilding, 0U);
                                        person.m_visitBuilding = _cityEvent.m_eventData.m_eventBuilding;
                                    }
                                }
                            }
                            else if (visitingHours)
                            {
                                int chance = _simulationManager.m_randomizer.Int32(10U);

                                if (chance == 0 && (person.m_instance != 0 || DoRandomMove(thisAI)))
                                {
                                    FindVisitPlace(thisAI, citizenID, person.m_visitBuilding, GetLeavingReason(thisAI, citizenID, ref person));
                                }
                                else if (chance > 7)
                                {
                                    break;
                                }
                                else if (chance > 5)
                                {
                                    if (person.m_instance != 0 || DoRandomMove(thisAI))
                                    {
                                        FindVisitPlace(thisAI, citizenID, person.m_visitBuilding, GetShoppingReason(thisAI));
                                    }

                                    _buildingInfo.m_buildingAI.ModifyMaterialBuffer(person.m_visitBuilding, ref _buildingManager.m_buildings.m_buffer[person.m_visitBuilding], TransferManager.TransferReason.Shopping, ref reduceAmount);
                                    AddTouristVisit(thisAI, citizenID, person.m_visitBuilding);
                                }
                                else if (chance > 3)
                                {
                                    if (person.m_instance != 0 || DoRandomMove(thisAI))
                                    {
                                        FindVisitPlace(thisAI, citizenID, person.m_visitBuilding, GetEntertainmentReason(thisAI));
                                    }
                                }
                                else
                                {
                                    _buildingInfo.m_buildingAI.ModifyMaterialBuffer(person.m_visitBuilding, ref _buildingManager.m_buildings.m_buffer[person.m_visitBuilding], TransferManager.TransferReason.Shopping, ref reduceAmount);
                                    AddTouristVisit(thisAI, citizenID, person.m_visitBuilding);
                                }
                            }
                            else if (_buildingInfo.m_class.m_subService != ItemClass.SubService.CommercialTourist) //Not in a hotel
                            {
                                //Try find a hotel
                                ushort foundHotel = _buildingManager.FindBuilding(_currentBuilding.m_position, 200f, ItemClass.Service.Commercial, ItemClass.SubService.CommercialTourist, Building.Flags.Created | Building.Flags.Active, Building.Flags.Deleted);

                                if (foundHotel != 0 && (person.m_instance != 0 || DoRandomMove(thisAI)) && _simulationManager.m_randomizer.Int32(0, 10) > 7)
                                {
                                    thisAI.StartMoving(citizenID, ref person, person.m_visitBuilding, foundHotel);
                                    person.SetVisitplace(citizenID, foundHotel, 0U);
                                    person.m_visitBuilding = foundHotel;
                                    AddTouristVisit(thisAI, citizenID, foundHotel);
                                    CimTools.CimToolsHandler.CimToolBase.DetailedLogger.Log("Tourist found hotel.");
                                }
                                else
                                {
                                    FindVisitPlace(thisAI, citizenID, person.m_visitBuilding, GetLeavingReason(thisAI, citizenID, ref person));
                                }
                            }
                        }

                        break;

                    case Citizen.Location.Moving:
                        if (person.Dead || person.Sick)
                        {
                            Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
                            break;
                        }
                        if ((int)person.m_vehicle != 0 || (int)person.m_instance != 0)
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

        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool StartMoving(TouristAI thisAI, uint citizenID, ref Citizen data, ushort sourceBuilding, ushort targetBuilding)
        {
            Debug.LogWarning("StartMoving is not overridden!");
            return false;
        }
    }
}
