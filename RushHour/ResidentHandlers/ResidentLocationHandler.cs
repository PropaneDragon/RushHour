using ColossalFramework;
using RushHour.Experiments;
using RushHour.Events;
using RushHour.Places;
using UnityEngine;

namespace RushHour.ResidentHandlers
{
    public static class ResidentLocationHandler
    {
        private enum BuildingType { Home, Work, Visit };
        private static double _startMovingToEventTime = 3D, _maxMoveToEventTime = 1.9D;

        public static bool ProcessHome(ref ResidentAI thisAI, uint citizenID, ref Citizen person)
        {
            CitizenManager _citizenManager = Singleton<CitizenManager>.instance;
            SimulationManager _simulation = Singleton<SimulationManager>.instance;

            if ((person.m_flags & Citizen.Flags.MovingIn) != Citizen.Flags.None)
            {
                _citizenManager.ReleaseCitizen(citizenID);
            }
            else if (ProcessGenerics(ref thisAI, citizenID, ref person))
            {
                if ((person.m_flags & Citizen.Flags.NeedGoods) != Citizen.Flags.None) //Wants to go shopping
                {
                    if (person.m_homeBuilding != 0 && person.m_instance == 0 && person.m_vehicle == 0) //Person isn't already out and about
                    {
                        if (_simulation.m_isNightTime)
                        {
                            uint chance = _simulation.m_randomizer.UInt32(1000);

                            if (chance < Chances.GoOutAtNight(person.Age) && NewResidentAI.DoRandomMove(thisAI))
                            {
                                NewResidentAI.FindVisitPlace(thisAI, citizenID, person.m_homeBuilding, NewResidentAI.GetShoppingReason(thisAI));
                                return true;
                            }
                        }
                        else if(NewResidentAI.DoRandomMove(thisAI))
                        {
                            uint chance = _simulation.m_randomizer.UInt32(100);

                            if (chance < 10)
                            {
                                NewResidentAI.FindVisitPlace(thisAI, citizenID, person.m_homeBuilding, NewResidentAI.GetShoppingReason(thisAI));
                                return true;
                            }
                        }
                    }
                }
                else if (person.m_homeBuilding != 0 && person.m_instance != 0 && person.m_vehicle == 0 || NewResidentAI.DoRandomMove(thisAI)) //If the person is already out and about, or can move (based on entities already visible)
                {
                    int eventId = CityEventManager.instance.EventStartsWithin(citizenID, ref person, _startMovingToEventTime);

                    if (eventId != -1)
                    {
                        CityEvent _cityEvent = CityEventManager.instance.m_nextEvents[eventId];

                        if (_cityEvent.EventStartsWithin(_startMovingToEventTime) && !_cityEvent.EventStartsWithin(_maxMoveToEventTime))
                        {
                            if(_cityEvent.Register())
                            {
                                NewResidentAI.StartMoving(thisAI, citizenID, ref person, person.m_homeBuilding, _cityEvent.m_eventData.m_eventBuilding);
                                person.SetVisitplace(citizenID, _cityEvent.m_eventData.m_eventBuilding, 0U);
                                person.m_visitBuilding = _cityEvent.m_eventData.m_eventBuilding;

                                return true;
                            }
                        }
                    }
                    else
                    {
                        if (person.m_workBuilding != 0 && !_simulation.m_isNightTime && !Chances.ShouldReturnFromWork(ref person))
                        {
                            if (Chances.ShouldGoToWork(ref person))
                            {
                                NewResidentAI.StartMoving(thisAI, citizenID, ref person, person.m_homeBuilding, person.m_workBuilding);
                                return true;
                            }
                        }
                        else
                        {
                            if (Chances.ShouldGoFindEntertainment(ref person))
                            {
                                NewResidentAI.FindVisitPlace(thisAI, citizenID, person.m_homeBuilding, NewResidentAI.GetEntertainmentReason(thisAI));
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        public static bool ProcessWork(ref ResidentAI thisAI, uint citizenID, ref Citizen person)
        {
            CitizenManager _citizenManager = Singleton<CitizenManager>.instance;

            if (ProcessGenerics(ref thisAI, citizenID, ref person))
            {
                if (person.m_instance != 0 || NewResidentAI.DoRandomMove(thisAI)) //If the person is already out and about, or can move (based on entities already visible)
                {
                    if (Chances.ShouldReturnFromWork(ref person))
                    {
                        int eventId = CityEventManager.instance.EventStartsWithin(citizenID, ref person, _startMovingToEventTime);

                        if (eventId != -1)
                        {
                            CityEvent _cityEvent = CityEventManager.instance.m_nextEvents[eventId];

                            if (_cityEvent.EventStartsWithin(_startMovingToEventTime) && !_cityEvent.EventStartsWithin(_maxMoveToEventTime))
                            {
                                if (_cityEvent.Register())
                                {
                                    NewResidentAI.StartMoving(thisAI, citizenID, ref person, person.m_workBuilding, _cityEvent.m_eventData.m_eventBuilding);
                                    person.SetVisitplace(citizenID, _cityEvent.m_eventData.m_eventBuilding, 0U);
                                    person.m_visitBuilding = _cityEvent.m_eventData.m_eventBuilding;

                                    return true;
                                }
                            }
                        }
                        else
                        {
                            if (Chances.ShouldGoFindEntertainment(ref person))
                            {
                                NewResidentAI.FindVisitPlace(thisAI, citizenID, person.m_workBuilding, NewResidentAI.GetEntertainmentReason(thisAI));

                                return true;
                            }
                            else
                            {
                                if ((person.m_flags & Citizen.Flags.NeedGoods) != Citizen.Flags.None) //Wants to go shopping
                                {
                                    if (person.m_workBuilding != 0 && person.m_instance == 0 && person.m_vehicle == 0) //Person isn't already out and about
                                    {
                                        NewResidentAI.FindVisitPlace(thisAI, citizenID, person.m_homeBuilding, NewResidentAI.GetShoppingReason(thisAI));
                                    }

                                    return true;
                                }
                                else
                                {
                                    NewResidentAI.StartMoving(thisAI, citizenID, ref person, person.m_workBuilding, person.m_homeBuilding);

                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        public static bool ProcessVisit(ref ResidentAI thisAI, uint citizenID, ref Citizen person)
        {
            if (ProcessGenerics(ref thisAI, citizenID, ref person))
            {
                ItemClass.Service service = ItemClass.Service.None;

                if (person.m_visitBuilding != 0)
                {
                    service = Singleton<BuildingManager>.instance.m_buildings.m_buffer[person.m_visitBuilding].Info.m_class.m_service;
                }

                if (service == ItemClass.Service.PoliceDepartment || service == ItemClass.Service.HealthCare)
                {
                    if (person.m_homeBuilding != 0 && person.m_instance == 0 && person.m_vehicle == 0)
                    {
                        NewResidentAI.StartMoving(thisAI, citizenID, ref person, person.m_visitBuilding, person.m_homeBuilding);
                        person.SetVisitplace(citizenID, 0, 0U);

                        return true;
                    }
                }
                else if(!CityEventManager.instance.EventTakingPlace(person.m_visitBuilding) && !CityEventManager.instance.EventStartsWithin(person.m_visitBuilding, 2D))
                {
                    int eventId = CityEventManager.instance.EventStartsWithin(citizenID, ref person, _startMovingToEventTime);

                    if (eventId != -1)
                    {
                        CityEvent _cityEvent = CityEventManager.instance.m_nextEvents[eventId];

                        if (_cityEvent.EventStartsWithin(_startMovingToEventTime) && !_cityEvent.EventStartsWithin(_maxMoveToEventTime))
                        {
                            if (_cityEvent.Register())
                            {
                                NewResidentAI.StartMoving(thisAI, citizenID, ref person, person.m_visitBuilding, _cityEvent.m_eventData.m_eventBuilding);
                                person.SetVisitplace(citizenID, _cityEvent.m_eventData.m_eventBuilding, 0U);
                                person.m_visitBuilding = _cityEvent.m_eventData.m_eventBuilding;

                                return true;
                            }
                        }
                    }
                    else if ((person.m_flags & Citizen.Flags.NeedGoods) != Citizen.Flags.None)
                    {
                        if (person.m_visitBuilding == 0)
                        {
                            person.CurrentLocation = Citizen.Location.Home;
                        }
                        else
                        {
                            BuildingManager instance = Singleton<BuildingManager>.instance;
                            BuildingInfo info = instance.m_buildings.m_buffer[person.m_visitBuilding].Info;

                            int amountDelta = -100;
                            info.m_buildingAI.ModifyMaterialBuffer(person.m_visitBuilding, ref instance.m_buildings.m_buffer[person.m_visitBuilding], TransferManager.TransferReason.Shopping, ref amountDelta);
                        }

                        return true;
                    }
                    else if ((person.m_instance != 0 || NewResidentAI.DoRandomMove(thisAI)) && person.m_homeBuilding != 0 && person.m_vehicle == 0)
                    {
                        uint shouldStayPercent = 2;

                        SimulationManager _simulation = Singleton<SimulationManager>.instance;

                        if (Chances.CanStayOut(ref person) && _simulation.m_randomizer.UInt32(100) < shouldStayPercent)
                        {
                            if (Chances.ShouldGoFindEntertainment(ref person))
                            {
                                NewResidentAI.FindVisitPlace(thisAI, citizenID, person.m_homeBuilding, NewResidentAI.GetEntertainmentReason(thisAI));
                                return true;
                            }
                        }
                        else
                        {
                            NewResidentAI.StartMoving(thisAI, citizenID, ref person, person.m_visitBuilding, person.m_homeBuilding);
                            person.SetVisitplace(citizenID, 0, 0U);
                            return true;
                        }
                    }
                }
            }
            
            return false;
        }

        internal static bool ProcessMoving(ref ResidentAI resident, uint citizenID, ref Citizen data)
        {
            if (data.Dead)
            {
                if (data.m_vehicle == 0)
                {
                    Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
                    return false;
                }

                if (data.m_homeBuilding != 0)
                {
                    data.SetHome(citizenID, 0, 0U);
                }

                if (data.m_workBuilding != 0)
                {
                    data.SetWorkplace(citizenID, 0, 0U);
                }

                if (data.m_visitBuilding != 0)
                {
                    data.SetVisitplace(citizenID, 0, 0U);
                    return true;
                }

                return true;
            }

            if (data.m_vehicle == 0 && data.m_instance == 0) //If they've stopped moving for whatever reason
            {
                if (data.m_visitBuilding != 0)
                {
                    data.SetVisitplace(citizenID, 0, 0U);
                }

                data.CurrentLocation = Citizen.Location.Home;
                data.Arrested = false;

                return true;
            }

            return true;
        }

        /// <summary>
        /// Generic stuff that can be taken care of as a group.
        /// </summary>
        /// <param name="thisAI"></param>
        /// <param name="citizenID"></param>
        /// <param name="person"></param>
        /// <returns>Whether to continue or whether this has taken care of the resident.</returns>
        private static bool ProcessGenerics(ref ResidentAI thisAI, uint citizenID, ref Citizen person)
        {
            bool everythingOk = false;
            ushort residingBuilding = 0;

            CitizenManager _citizenManager = Singleton<CitizenManager>.instance;
            Citizen.Location _currentLocation = person.CurrentLocation;

            switch (_currentLocation)
            {
                case Citizen.Location.Home:
                    residingBuilding = person.m_homeBuilding;
                    break;
                case Citizen.Location.Work:
                    residingBuilding = person.m_workBuilding;
                    break;
                case Citizen.Location.Visit:
                    residingBuilding = person.m_visitBuilding;
                    break;
            }

            Building visitBuilding = Singleton<BuildingManager>.instance.m_buildings.m_buffer[person.m_visitBuilding];
            bool inHealthcare = _currentLocation == Citizen.Location.Visit && residingBuilding != 0 && visitBuilding.Info.m_class.m_service == ItemClass.Service.HealthCare;

            if (person.Dead)
            {
                if (residingBuilding == 0)
                {
                    _citizenManager.ReleaseCitizen(citizenID);
                }
                else
                {
                    if ((_currentLocation == Citizen.Location.Work || _currentLocation == Citizen.Location.Visit) && person.m_homeBuilding != 0)
                    {
                        person.SetWorkplace(citizenID, 0, 0U);
                    }

                    if ((_currentLocation == Citizen.Location.Home || _currentLocation == Citizen.Location.Visit) && person.m_workBuilding != 0)
                    {
                        person.SetWorkplace(citizenID, 0, 0U);
                    }

                    if ((_currentLocation == Citizen.Location.Home || _currentLocation == Citizen.Location.Work) && person.m_visitBuilding != 0)
                    {
                        person.SetVisitplace(citizenID, 0, 0U);
                    }

                    if (ExperimentsToggle.ImprovedDeathcare)
                    {
                        if (person.m_vehicle == 0 && !inHealthcare)
                        {
                            NewResidentAI.FindHospital(thisAI, citizenID, residingBuilding, TransferManager.TransferReason.Sick);
                        }
                    }
                    else
                    {
                        if (person.m_vehicle == 0 && !inHealthcare)
                        {
                            NewResidentAI.FindHospital(thisAI, citizenID, residingBuilding, TransferManager.TransferReason.Dead);
                        }
                    }
                }
            }
            else if (person.Arrested)
            {
                if (_currentLocation == Citizen.Location.Visit && residingBuilding == 0)
                {
                    person.Arrested = false;
                }
                else
                {
                    person.Arrested = false;
                }
            }
            else if (person.Sick)
            {
                if (residingBuilding != 0)
                {
                    if (person.m_vehicle == 0 && !inHealthcare)
                    {
                        NewResidentAI.FindHospital(thisAI, citizenID, residingBuilding, TransferManager.TransferReason.Sick);
                    }
                }
                else
                {
                    person.CurrentLocation = Citizen.Location.Home;
                }
            }
            else if(residingBuilding == 0)
            {
                person.CurrentLocation = Citizen.Location.Home;
            }
            else
            {
                everythingOk = true;
            }

            return everythingOk;
        }
    }
}
