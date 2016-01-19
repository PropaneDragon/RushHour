using ColossalFramework;
using RushHour.Places;
using RushHour.ResidentHandlers;
using System.Reflection;

namespace RushHour.CitizenHandlers
{
    public static class CitizenLocationHandler
    {
        public static bool ProcessHome(uint citizenID, ref Citizen person)
        {
            CitizenManager _citizenManager = Singleton<CitizenManager>.instance;
            SimulationManager _simulation = Singleton<SimulationManager>.instance;

            if ((person.m_flags & Citizen.Flags.MovingIn) != Citizen.Flags.None)
            {
                _citizenManager.ReleaseCitizen(citizenID);
                return false;
            }

            if (person.Dead)
            {
                if (person.m_homeBuilding == 0)
                {
                    _citizenManager.ReleaseCitizen(citizenID);
                    return false;
                }

                if (person.m_workBuilding != 0)
                {
                    person.SetWorkplace(citizenID, 0, 0U);
                }

                if (person.m_visitBuilding != 0)
                {
                    person.SetVisitplace(citizenID, 0, 0U);
                }

                if (person.m_vehicle == 0 && !PlaceFinder.FindHospital(citizenID, person.m_homeBuilding, TransferManager.TransferReason.Dead))
                {
                    return false;
                }

                return true;
            }

            if (person.Arrested)
            {
                person.Arrested = false;

                return true;
            }

            if (person.Sick)
            {
                if (person.m_homeBuilding != 0 && person.m_vehicle == 0 && !PlaceFinder.FindHospital(citizenID, person.m_homeBuilding, TransferManager.TransferReason.Sick))
                {
                    return false;
                }

                return true;
            }

            if ((person.m_flags & Citizen.Flags.NeedGoods) != Citizen.Flags.None) //Wants to go shopping
            {
                if (person.m_homeBuilding != 0 && person.m_instance == 0 && person.m_vehicle == 0) //Person isn't already out and about
                {
                    if (_simulation.m_isNightTime)
                    {
                        uint chance = _simulation.m_randomizer.UInt32(1000);

                        if (chance < Chances.GoOutAtNight(person.Age))
                        {
                            PlaceFinder.FindVisitPlace(citizenID, person.m_homeBuilding, PlaceFinder.GetShoppingReason());
                        }
                    }
                    else
                    {
                        PlaceFinder.FindVisitPlace(citizenID, person.m_homeBuilding, PlaceFinder.GetShoppingReason());
                    }
                }

                return true;
            }

            if (person.m_homeBuilding != 0 && person.m_instance != 0 && person.m_vehicle == 0 || NewResidentAI.DoRandomMove()) //If the person is already out and about, or can move (based on entities already visible)
            {
                if (person.m_workBuilding != 0 && !_simulation.m_isNightTime)
                {
                    if (Chances.ShouldGoToWork(ref person))
                    {
                        NewResidentAI.StartMoving(citizenID, ref person, person.m_homeBuilding, person.m_workBuilding);
                        return true;
                    }
                }
                else
                {
                    if(Chances.ShouldGoFindEntertainment(ref person))
                    {
                        PlaceFinder.FindVisitPlace(citizenID, person.m_homeBuilding, PlaceFinder.GetEntertainmentReason());
                        return true;
                    }
                }

                return true;
            }

            return true;
        }

        public static bool ProcessWork(uint citizenID, ref Citizen person)
        {
            CitizenManager _citizenManager = Singleton<CitizenManager>.instance;

            if (person.Dead)
            {
                if (person.m_workBuilding == 0)
                {
                    _citizenManager.ReleaseCitizen(citizenID);
                    return false;
                }

                if (person.m_homeBuilding != 0)
                {
                    person.SetHome(citizenID, 0, 0U);
                }

                if (person.m_visitBuilding != 0)
                {
                    person.SetVisitplace(citizenID, 0, 0U);
                }

                if (person.m_vehicle == 0 && !PlaceFinder.FindHospital(citizenID, person.m_workBuilding, TransferManager.TransferReason.Dead))
                {
                    return false;
                }

                return true;
            }

            if (person.Arrested)
            {
                person.Arrested = false;

                return true;
            }

            if (person.Sick)
            {
                if (person.m_workBuilding == 0)
                {
                    person.CurrentLocation = Citizen.Location.Home;

                    return true;
                }

                if (person.m_vehicle == 0 && !PlaceFinder.FindHospital(citizenID, person.m_workBuilding, TransferManager.TransferReason.Sick))
                {
                    return false;
                }

                return true;
            }

            if (person.m_workBuilding == 0)
            {
                person.CurrentLocation = Citizen.Location.Home;
                return true;
            }

            if (person.m_instance != 0 || NewResidentAI.DoRandomMove()) //If the person is already out and about, or can move (based on entities already visible)
            {
                if (Chances.ShouldReturnFromWork(ref person))
                {
                    if (Chances.ShouldGoFindEntertainment(ref person))
                    {
                        PlaceFinder.FindVisitPlace(citizenID, person.m_workBuilding, PlaceFinder.GetEntertainmentReason());
                        return true;
                    }
                    else
                    {
                        if ((person.m_flags & Citizen.Flags.NeedGoods) != Citizen.Flags.None) //Wants to go shopping
                        {
                            if (person.m_workBuilding != 0 && person.m_instance == 0 && person.m_vehicle == 0) //Person isn't already out and about
                            {
                                PlaceFinder.FindVisitPlace(citizenID, person.m_homeBuilding, PlaceFinder.GetShoppingReason());
                            }

                            return true;
                        }
                        else
                        {
                            NewResidentAI.StartMoving(citizenID, ref person, person.m_workBuilding, person.m_homeBuilding);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static bool ProcessVisit(uint citizenID, ref Citizen person)
        {
            if (person.Dead)
            {
                if (person.m_visitBuilding == 0)
                {
                    Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
                    return false;
                }

                if (person.m_homeBuilding != 0)
                {
                    person.SetHome(citizenID, (ushort)0, 0U);
                }

                if (person.m_workBuilding != 0)
                {
                    person.SetWorkplace(citizenID, (ushort)0, 0U);
                }

                if (person.m_vehicle == 0 && Singleton<BuildingManager>.instance.m_buildings.m_buffer[person.m_visitBuilding].Info.m_class.m_service != ItemClass.Service.HealthCare && !PlaceFinder.FindHospital(citizenID, person.m_visitBuilding, TransferManager.TransferReason.Dead))
                {
                    return false;
                }

                return true;
            }

            if (person.Arrested)
            {
                if (person.m_visitBuilding == 0)
                {
                    person.Arrested = false;
                    return true;
                }

                return true;
            }

            if (person.Sick)
            {
                if (person.m_visitBuilding == 0)
                {
                    person.CurrentLocation = Citizen.Location.Home;
                    return true;
                }

                if (person.m_vehicle == 0 && Singleton<BuildingManager>.instance.m_buildings.m_buffer[person.m_visitBuilding].Info.m_class.m_service != ItemClass.Service.HealthCare && !PlaceFinder.FindHospital(citizenID, person.m_visitBuilding, TransferManager.TransferReason.Sick))
                {
                    return false;
                }

                return true;
            }

            ItemClass.Service service = ItemClass.Service.None;

            if (person.m_visitBuilding != 0)
            {
                service = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)person.m_visitBuilding].Info.m_class.m_service;
            }

            if (service == ItemClass.Service.PoliceDepartment || service == ItemClass.Service.HealthCare)
            {
                if (person.m_homeBuilding != 0 && person.m_instance == 0 && person.m_vehicle == 0)
                {
                    NewResidentAI.StartMoving(citizenID, ref person, person.m_visitBuilding, person.m_homeBuilding);
                    person.SetVisitplace(citizenID, 0, 0U);

                    return true;
                }

                return true;
            }

            if ((person.m_flags & Citizen.Flags.NeedGoods) != Citizen.Flags.None)
            {
                if (person.m_visitBuilding == 0)
                {
                    person.CurrentLocation = Citizen.Location.Home;
                    return true;
                }

                BuildingManager instance = Singleton<BuildingManager>.instance;
                BuildingInfo info = instance.m_buildings.m_buffer[(int)person.m_visitBuilding].Info;

                int amountDelta = -100;
                info.m_buildingAI.ModifyMaterialBuffer(person.m_visitBuilding, ref instance.m_buildings.m_buffer[person.m_visitBuilding], TransferManager.TransferReason.Shopping, ref amountDelta);

                return true;
            }

            if (person.m_visitBuilding == 0)
            {
                person.CurrentLocation = Citizen.Location.Home;
                return true;
            }

            if ((person.m_instance != 0 || NewResidentAI.DoRandomMove()) && person.m_homeBuilding != 0 && person.m_instance == 0 && person.m_vehicle == 0)
            {
                uint shouldStayPercent = 20;

                SimulationManager _simulation = Singleton<SimulationManager>.instance;
                if (Chances.CanStayOut(ref person) && _simulation.m_randomizer.UInt32(100) < shouldStayPercent)
                {
                    if (Chances.ShouldGoFindEntertainment(ref person))
                    {
                        PlaceFinder.FindVisitPlace(citizenID, person.m_homeBuilding, PlaceFinder.GetEntertainmentReason());
                        return true;
                    }
                }

                NewResidentAI.StartMoving(citizenID, ref person, person.m_visitBuilding, person.m_homeBuilding);
                person.SetVisitplace(citizenID, 0, 0U);

                return true;
            }
            
            return true;
        }

        internal static bool ProcessMoving(uint citizenID, ref Citizen data)
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
    }
}
