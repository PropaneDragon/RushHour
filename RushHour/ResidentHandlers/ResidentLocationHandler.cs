using ColossalFramework;
using RushHour.Experiments;
using RushHour.Places;

namespace RushHour.ResidentHandlers
{
    public static class ResidentLocationHandler
    {
        public static bool ProcessHome(ref ResidentAI thisAI, uint citizenID, ref Citizen person)
        {
            CitizenManager _citizenManager = Singleton<CitizenManager>.instance;
            SimulationManager _simulation = Singleton<SimulationManager>.instance;

            if ((person.m_flags & Citizen.Flags.MovingIn) != Citizen.Flags.None)
            {
                _citizenManager.ReleaseCitizen(citizenID);
                return false;
            }
            else if (person.Dead)
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

                if (ExperimentsToggle.ExperimentDeathcare())
                {
                    if (person.m_vehicle == 0 && !NewResidentAI.FindHospital(thisAI, citizenID, person.m_homeBuilding, TransferManager.TransferReason.Sick))
                    {
                        return false;
                    }
                }
                else
                {
                    if (person.m_vehicle == 0 && !NewResidentAI.FindHospital(thisAI, citizenID, person.m_homeBuilding, TransferManager.TransferReason.Dead))
                    {
                        return false;
                    }
                }
            }
            else if (person.Arrested)
            {
                person.Arrested = false;
            }
            else if (person.Sick)
            {
                if (person.m_homeBuilding != 0 && person.m_vehicle == 0 && !NewResidentAI.FindHospital(thisAI, citizenID, person.m_homeBuilding, TransferManager.TransferReason.Sick))
                {
                    return false;
                }
            }
            else if ((person.m_flags & Citizen.Flags.NeedGoods) != Citizen.Flags.None) //Wants to go shopping
            {
                if (person.m_homeBuilding != 0 && person.m_instance == 0 && person.m_vehicle == 0) //Person isn't already out and about
                {
                    if (_simulation.m_isNightTime)
                    {
                        uint chance = _simulation.m_randomizer.UInt32(1000);

                        if (chance < Chances.GoOutAtNight(person.Age))
                        {
                            NewResidentAI.FindVisitPlace(thisAI, citizenID, person.m_homeBuilding, NewResidentAI.GetShoppingReason(thisAI));
                        }
                    }
                    else
                    {
                        NewResidentAI.FindVisitPlace(thisAI, citizenID, person.m_homeBuilding, NewResidentAI.GetShoppingReason(thisAI));
                    }
                }
            }
            else if (person.m_homeBuilding != 0 && person.m_instance != 0 && person.m_vehicle == 0 || NewResidentAI.DoRandomMove(thisAI)) //If the person is already out and about, or can move (based on entities already visible)
            {
                if (person.m_workBuilding != 0 && !_simulation.m_isNightTime && !Chances.ShouldReturnFromWork(ref person))
                {
                    if (Chances.ShouldGoToWork(ref person))
                    {
                        NewResidentAI.StartMoving(thisAI, citizenID, ref person, person.m_homeBuilding, person.m_workBuilding);
                    }
                }
                else
                {
                    if(Chances.ShouldGoFindEntertainment(ref person))
                    {
                        NewResidentAI.FindVisitPlace(thisAI, citizenID, person.m_homeBuilding, NewResidentAI.GetEntertainmentReason(thisAI));
                    }
                }
            }

            return true;
        }

        public static bool ProcessWork(ref ResidentAI thisAI, uint citizenID, ref Citizen person)
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

                if (ExperimentsToggle.ExperimentDeathcare())
                {
                    if (person.m_vehicle == 0 && !NewResidentAI.FindHospital(thisAI, citizenID, person.m_workBuilding, TransferManager.TransferReason.Sick))
                    {
                        return false;
                    }
                }
                else
                {
                    if (person.m_vehicle == 0 && !NewResidentAI.FindHospital(thisAI, citizenID, person.m_workBuilding, TransferManager.TransferReason.Dead))
                    {
                        return false;
                    }
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

                if (person.m_vehicle == 0 && !NewResidentAI.FindHospital(thisAI, citizenID, person.m_workBuilding, TransferManager.TransferReason.Sick))
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

            if (person.m_instance != 0 || NewResidentAI.DoRandomMove(thisAI)) //If the person is already out and about, or can move (based on entities already visible)
            {
                if (Chances.ShouldReturnFromWork(ref person))
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

            return false;
        }

        public static bool ProcessVisit(ref ResidentAI thisAI, uint citizenID, ref Citizen person)
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

                if (ExperimentsToggle.ExperimentDeathcare())
                {
                    if (person.m_vehicle == 0)
                    {
                        /*Attempt at making death more realistic. When people die they should now go to hospital, then get
                        picked up from there by hearse.*/
                        if (Singleton<BuildingManager>.instance.m_buildings.m_buffer[person.m_visitBuilding].Info.m_class.m_service == ItemClass.Service.HealthCare)
                        {
                            if (!NewResidentAI.FindHospital(thisAI, citizenID, person.m_visitBuilding, TransferManager.TransferReason.Dead))
                            {
                                return false;
                            }
                        }
                        else if (!NewResidentAI.FindHospital(thisAI, citizenID, person.m_visitBuilding, TransferManager.TransferReason.Sick))
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    if (person.m_vehicle == 0 && Singleton<BuildingManager>.instance.m_buildings.m_buffer[person.m_visitBuilding].Info.m_class.m_service != ItemClass.Service.HealthCare && !NewResidentAI.FindHospital(thisAI, citizenID, person.m_visitBuilding, TransferManager.TransferReason.Dead))
                    {
                        return false;
                    }
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

                if (person.m_vehicle == 0 && Singleton<BuildingManager>.instance.m_buildings.m_buffer[person.m_visitBuilding].Info.m_class.m_service != ItemClass.Service.HealthCare && !NewResidentAI.FindHospital(thisAI, citizenID, person.m_visitBuilding, TransferManager.TransferReason.Sick))
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
                    NewResidentAI.StartMoving(thisAI, citizenID, ref person, person.m_visitBuilding, person.m_homeBuilding);
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
                BuildingInfo info = instance.m_buildings.m_buffer[person.m_visitBuilding].Info;

                int amountDelta = -100;
                info.m_buildingAI.ModifyMaterialBuffer(person.m_visitBuilding, ref instance.m_buildings.m_buffer[person.m_visitBuilding], TransferManager.TransferReason.Shopping, ref amountDelta);

                return true;
            }

            if (person.m_visitBuilding == 0)
            {
                person.CurrentLocation = Citizen.Location.Home;
                return true;
            }

            if ((person.m_instance != 0 || NewResidentAI.DoRandomMove(thisAI)) && person.m_homeBuilding != 0 && person.m_instance == 0 && person.m_vehicle == 0)
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

                    return true;
                }

                NewResidentAI.StartMoving(thisAI, citizenID, ref person, person.m_visitBuilding, person.m_homeBuilding);
                person.SetVisitplace(citizenID, 0, 0U);

                return true;
            }
            
            return true;
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
    }
}
