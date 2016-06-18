using ColossalFramework;
using RushHour.Experiments;
using RushHour.Events;
using RushHour.Places;
using RushHour.SimulationHandlers;

namespace RushHour.CitizenHandlers
{
    public static class ResidentLocationHandler
    {
        private enum BuildingType { Home, Work, Visit };
        private static double _startMovingToEventTime = 12D, _maxMoveToEventTime = 7.6D;

        /// <summary>
        /// When a citizen is allowed to start moving to an event
        /// </summary>
        public static double StartMovingToEventTime
        {
            get
            {
                return Time.SpeedMultiplier(_startMovingToEventTime);
            }
        }

        /// <summary>
        /// The maximum time a citizen is allowed to start moving to an event
        /// </summary>
        public static double MaxMoveToEventTime
        {
            get
            {
                return Time.SpeedMultiplier(_maxMoveToEventTime);
            }
        }

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
                if ((person.m_flags & Citizen.Flags.NeedGoods) != Citizen.Flags.None && (!Chances.WorkDay(ref person) || _simulation.m_currentDayTimeHour > Chances.m_startWorkHour)) //Wants to go shopping
                {
                    if (person.m_homeBuilding != 0 && person.m_instance == 0 && person.m_vehicle == 0) //Person isn't already out and about
                    {
                        if (_simulation.m_isNightTime)
                        {
                            uint chance = _simulation.m_randomizer.UInt32(1000);

                            if (chance < Chances.GoOutAtNight(person.Age) && NewResidentAI.DoRandomMove(thisAI))
                            {
                                //Only go locally to find a shop at night
                                FindCloseVisitPlace(ref thisAI, citizenID, ref person, person.m_homeBuilding, 1000f);
                                return true;
                            }
                        }
                        else if(NewResidentAI.DoRandomMove(thisAI))
                        {
                            uint chance = _simulation.m_randomizer.UInt32(100);

                            if (chance < 10)
                            {
                                uint localChance = _simulation.m_randomizer.UInt32(100);
                                ushort localVisitPlace = 0;

                                if (ExperimentsToggle.AllowLocalBuildingSearch && localChance < ExperimentsToggle.LocalBuildingPercentage)
                                {
                                    CimTools.CimToolsHandler.CimToolBase.DetailedLogger.Log("Citizen " + citizenID + " trying to find a local commercial building.");
                                    localVisitPlace = FindCloseVisitPlace(ref thisAI, citizenID, ref person, person.m_homeBuilding, 1000f);
                                }

                                if (localVisitPlace == 0)
                                {
                                    CimTools.CimToolsHandler.CimToolBase.DetailedLogger.Log("Citizen " + citizenID + " going to a random commercial building.");
                                    NewResidentAI.FindVisitPlace(thisAI, citizenID, person.m_homeBuilding, NewResidentAI.GetShoppingReason(thisAI));
                                }
                                return true;
                            }
                        }
                    }
                }
                else if (person.m_homeBuilding != 0 && person.m_instance != 0 && person.m_vehicle == 0 || NewResidentAI.DoRandomMove(thisAI)) //If the person is already out and about, or can move (based on entities already visible)
                {
                    int eventId = CityEventManager.instance.EventStartsWithin(citizenID, ref person, StartMovingToEventTime);

                    if (eventId != -1)
                    {
                        CityEvent _cityEvent = CityEventManager.instance.m_nextEvents[eventId];

                        if (_cityEvent.EventStartsWithin(StartMovingToEventTime) && !_cityEvent.EventStartsWithin(MaxMoveToEventTime))
                        {
                            if((person.m_instance != 0 || NewResidentAI.DoRandomMove(thisAI)) && _cityEvent.Register(citizenID, ref person))
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
                                if (_simulation.m_isNightTime)
                                {
                                    FindLeisure(ref thisAI, citizenID, ref person, person.m_homeBuilding);
                                }
                                else
                                {
                                    NewResidentAI.FindVisitPlace(thisAI, citizenID, person.m_homeBuilding, NewResidentAI.GetEntertainmentReason(thisAI));
                                }

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
            BuildingManager _buildingManager = Singleton<BuildingManager>.instance;
            SimulationManager _simulation = Singleton<SimulationManager>.instance;

            if (ProcessGenerics(ref thisAI, citizenID, ref person))
            {
                if (person.m_instance != 0 || NewResidentAI.DoRandomMove(thisAI)) //If the person is already out and about, or can move (based on entities already visible)
                {
                    if (Chances.ShouldReturnFromWork(ref person))
                    {
                        int eventId = CityEventManager.instance.EventStartsWithin(citizenID, ref person, StartMovingToEventTime);

                        if (eventId != -1)
                        {
                            CityEvent _cityEvent = CityEventManager.instance.m_nextEvents[eventId];

                            if (_cityEvent.EventStartsWithin(StartMovingToEventTime) && !_cityEvent.EventStartsWithin(MaxMoveToEventTime))
                            {
                                if ((person.m_instance != 0 || NewResidentAI.DoRandomMove(thisAI)) && _cityEvent.Register(citizenID, ref person))
                                {
                                    NewResidentAI.StartMoving(thisAI, citizenID, ref person, person.m_workBuilding, _cityEvent.m_eventData.m_eventBuilding);
                                    person.SetVisitplace(citizenID, _cityEvent.m_eventData.m_eventBuilding, 0U);
                                    person.m_visitBuilding = _cityEvent.m_eventData.m_eventBuilding;
                                }
                            }
                        }
                        else
                        {
                            if (Chances.ShouldGoFindEntertainment(ref person))
                            {
                                uint localChance = _simulation.m_randomizer.UInt32(100);
                                ushort localVisitPlace = 0;

                                if (ExperimentsToggle.AllowLocalBuildingSearch && localChance < ExperimentsToggle.LocalBuildingPercentage)
                                {
                                    CimTools.CimToolsHandler.CimToolBase.DetailedLogger.Log("Citizen " + citizenID + " trying to find a local commercial building.");
                                    localVisitPlace = FindCloseVisitPlace(ref thisAI, citizenID, ref person, person.m_workBuilding, 1000f);
                                }

                                if (localVisitPlace == 0)
                                {
                                    CimTools.CimToolsHandler.CimToolBase.DetailedLogger.Log("Citizen " + citizenID + " going to a random commercial building.");
                                    NewResidentAI.FindVisitPlace(thisAI, citizenID, person.m_workBuilding, NewResidentAI.GetEntertainmentReason(thisAI));
                                }

                                return true;
                            }
                            else
                            {
                                if ((person.m_flags & Citizen.Flags.NeedGoods) != Citizen.Flags.None) //Wants to go shopping
                                {
                                    if (person.m_workBuilding != 0 && person.m_instance == 0 && person.m_vehicle == 0) //Person isn't already out and about
                                    {
                                        uint localChance = _simulation.m_randomizer.UInt32(100);
                                        ushort localVisitPlace = 0;

                                        if (ExperimentsToggle.AllowLocalBuildingSearch && localChance < ExperimentsToggle.LocalBuildingPercentage)
                                        {
                                            CimTools.CimToolsHandler.CimToolBase.DetailedLogger.Log("Citizen " + citizenID + " trying to find a local commercial building.");
                                            localVisitPlace = FindCloseVisitPlace(ref thisAI, citizenID, ref person, person.m_workBuilding, 1000f);
                                        }

                                        if (localVisitPlace == 0)
                                        {
                                            CimTools.CimToolsHandler.CimToolBase.DetailedLogger.Log("Citizen " + citizenID + " going to a random commercial building.");
                                            NewResidentAI.FindVisitPlace(thisAI, citizenID, person.m_workBuilding, NewResidentAI.GetShoppingReason(thisAI));
                                        }
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
                    else if(Chances.ShouldGoToLunch(ref person) && person.m_workBuilding != 0)
                    {
                        //Try find somewhere close to eat
                        ushort foundLunchPlace = FindCloseVisitPlace(ref thisAI, citizenID, ref person, person.m_workBuilding, 200f);
                        
                        if (foundLunchPlace != 0)
                        {
                            CimTools.CimToolsHandler.CimToolBase.DetailedLogger.Log("Citizen " + citizenID + " is heading out to eat for lunch at " + CityEventManager.CITY_TIME.ToShortTimeString() + ".");
                            return true;
                        }
                        else
                        {
                            CimTools.CimToolsHandler.CimToolBase.DetailedLogger.Log("Citizen " + citizenID + " wanted to head out for lunch, but there were no buildings close enough.");
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
                SimulationManager _simulation = Singleton<SimulationManager>.instance;
                WeatherManager _weatherManager = Singleton<WeatherManager>.instance;
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
                else if (!GameEventHelpers.EventTakingPlace(person.m_visitBuilding) && !CityEventManager.instance.EventTakingPlace(person.m_visitBuilding) && !CityEventManager.instance.EventStartsWithin(person.m_visitBuilding, 2D))
                {
                    int eventId = CityEventManager.instance.EventStartsWithin(citizenID, ref person, StartMovingToEventTime);
                    bool eventOn = false;

                    if (eventId != -1)
                    {
                        CityEvent _cityEvent = CityEventManager.instance.m_nextEvents[eventId];

                        if (_cityEvent.EventStartsWithin(StartMovingToEventTime) && !_cityEvent.EventStartsWithin(MaxMoveToEventTime))
                        {
                            if ((person.m_instance != 0 || NewResidentAI.DoRandomMove(thisAI)) && _cityEvent.Register(citizenID, ref person))
                            {
                                NewResidentAI.StartMoving(thisAI, citizenID, ref person, person.m_visitBuilding, _cityEvent.m_eventData.m_eventBuilding);
                                person.SetVisitplace(citizenID, _cityEvent.m_eventData.m_eventBuilding, 0U);
                                person.m_visitBuilding = _cityEvent.m_eventData.m_eventBuilding;
                                eventOn = true;
                            }
                        }
                    }

                    if (!eventOn)
                    {
                        if (person.m_instance != 0 && _weatherManager.m_currentRain > 0 && _simulation.m_randomizer.Int32(0, 10) <= (_weatherManager.m_currentRain * 10))
                        {
                            //It's raining, we're outside, and we need to go somewhere dry!
                            if (person.m_workBuilding != 0 && !_simulation.m_isNightTime && !Chances.ShouldReturnFromWork(ref person))
                            {
                                NewResidentAI.StartMoving(thisAI, citizenID, ref person, person.m_visitBuilding, person.m_workBuilding);
                                CimTools.CimToolsHandler.CimToolBase.DetailedLogger.Log("Rain! Citizen " + citizenID + " is getting wet, and has decided to go back to work.");
                            }
                            else
                            {
                                NewResidentAI.StartMoving(thisAI, citizenID, ref person, person.m_visitBuilding, person.m_homeBuilding);
                                CimTools.CimToolsHandler.CimToolBase.DetailedLogger.Log("Rain! Citizen " + citizenID + " is getting wet, and has decided to go home.");
                            }

                            person.SetVisitplace(citizenID, 0, 0U);

                        }
                        else if (person.m_workBuilding != 0 && (Chances.ShouldGoToWork(ref person) || ((Chances.LunchHour() || Chances.HoursSinceLunchHour(1.5f)) && Chances.ShouldGoToWork(ref person, true))))
                        {
                            NewResidentAI.StartMoving(thisAI, citizenID, ref person, person.m_visitBuilding, person.m_workBuilding);
                            person.SetVisitplace(citizenID, 0, 0U);

                            CimTools.CimToolsHandler.CimToolBase.DetailedLogger.Log("Citizen " + citizenID + " was out and about but should've been at work. Going there now.");
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

                            if (Chances.CanStayOut(ref person) && _simulation.m_randomizer.UInt32(100) < shouldStayPercent)
                            {
                                if (Chances.ShouldGoFindEntertainment(ref person))
                                {
                                    if (_simulation.m_isNightTime)
                                    {
                                        FindLeisure(ref thisAI, citizenID, ref person, person.m_visitBuilding);
                                    }
                                    else
                                    {
                                        NewResidentAI.FindVisitPlace(thisAI, citizenID, person.m_visitBuilding, NewResidentAI.GetEntertainmentReason(thisAI));
                                    }

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

        private static ushort FindLeisure(ref ResidentAI thisAI, uint citizenID, ref Citizen person, ushort buildingID)
        {
            ushort foundLeisure = 0;

            if ((person.m_instance != 0 || NewResidentAI.DoRandomMove(thisAI)))
            {
                BuildingManager _buildingManager = Singleton<BuildingManager>.instance;
                SimulationManager _simulationManager = Singleton<SimulationManager>.instance;
                Building _currentBuilding = _buildingManager.m_buildings.m_buffer[buildingID];

                foundLeisure = _buildingManager.FindBuilding(_currentBuilding.m_position, 1000f, ItemClass.Service.Commercial, ItemClass.SubService.CommercialLeisure, Building.Flags.Created | Building.Flags.Active, Building.Flags.Deleted);

                if (foundLeisure != 0)
                {
                    if (_simulationManager.m_randomizer.Int32(0, 10) > 2)
                    {
                        thisAI.StartMoving(citizenID, ref person, buildingID, foundLeisure);
                        person.SetVisitplace(citizenID, foundLeisure, 0U);
                        person.m_visitBuilding = foundLeisure;
                        CimTools.CimToolsHandler.CimToolBase.DetailedLogger.Log("Citizen " + citizenID + " found leisure.");
                    }
                    else
                    {
                        CimTools.CimToolsHandler.CimToolBase.DetailedLogger.Log("Citizen " + citizenID + " found leisure, but chose not to bother.");
                        NewResidentAI.FindVisitPlace(thisAI, citizenID, buildingID, NewResidentAI.GetEntertainmentReason(thisAI));
                    }
                }
                else
                {
                    CimTools.CimToolsHandler.CimToolBase.DetailedLogger.Log("Citizen " + citizenID + " couldn't find leisure.");
                    NewResidentAI.FindVisitPlace(thisAI, citizenID, buildingID, NewResidentAI.GetEntertainmentReason(thisAI));
                }
            }

            return foundLeisure;
        }

        private static ushort FindCloseVisitPlace(ref ResidentAI thisAI, uint citizenID, ref Citizen person, ushort buildingID, float distance)
        {
            ushort foundBuilding = 0;

            if ((person.m_instance != 0 || NewResidentAI.DoRandomMove(thisAI)))
            {
                BuildingManager _buildingManager = Singleton<BuildingManager>.instance;
                Building _currentBuilding = _buildingManager.m_buildings.m_buffer[buildingID];

                foundBuilding = _buildingManager.FindBuilding(_currentBuilding.m_position, distance, ItemClass.Service.Commercial, ItemClass.SubService.None, Building.Flags.Created | Building.Flags.Active, Building.Flags.Deleted);

                if(foundBuilding != 0)
                {
                    CimTools.CimToolsHandler.CimToolBase.DetailedLogger.Log("Citizen " + citizenID + " going to a local commercial building.");
                    thisAI.StartMoving(citizenID, ref person, buildingID, foundBuilding);
                    person.SetVisitplace(citizenID, foundBuilding, 0U);
                    person.m_visitBuilding = foundBuilding;
                }
            }

            return foundBuilding;
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
