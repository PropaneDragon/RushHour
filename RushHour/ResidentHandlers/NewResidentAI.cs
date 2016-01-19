using ColossalFramework;
using System;
using UnityEngine;

namespace RushHour.ResidentHandlers
{
    public static class NewResidentAI
    {
        public static void UpdateLocation(ResidentAI resident, uint citizenID, ref Citizen data)
        {
            try
            {
                CitizenManager _citizenManager = Singleton<CitizenManager>.instance;

                if (data.m_homeBuilding == 00 && data.m_workBuilding == 0 && data.m_visitBuilding == 0 && data.m_instance == 0 && data.m_vehicle == 0)
                {
                    _citizenManager.ReleaseCitizen(citizenID);
                }
                else
                {
                    switch (data.CurrentLocation)
                    {
                        case Citizen.Location.Home:
                            if (!ResidentLocationHandler.ProcessHome(ref resident, citizenID, ref data))
                            {
                                return;
                            }
                            break;
                        case Citizen.Location.Work:
                            if (!ResidentLocationHandler.ProcessWork(ref resident, citizenID, ref data))
                            {
                                return;
                            }
                            break;
                        case Citizen.Location.Visit:
                            if (!ResidentLocationHandler.ProcessVisit(ref resident, citizenID, ref data))
                            {
                                return;
                            }
                            break;
                        case Citizen.Location.Moving:
                            if (!ResidentLocationHandler.ProcessMoving(ref resident, citizenID, ref data))
                            {
                                return;
                            }
                            break;
                    }

                    data.m_flags &= ~Citizen.Flags.NeedGoods;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Error on " + citizenID);
                Debug.LogException(ex);
            }
        }
    }
}
