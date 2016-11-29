using ColossalFramework;
using System;
using System.Runtime.CompilerServices;
using RushHour.Redirection;
using UnityEngine;

namespace RushHour.CitizenHandlers
{
    [TargetType(typeof(ResidentAI))]
    internal class NewResidentAI
    {
        [RedirectMethod]
        public static void UpdateLocation(ResidentAI resident, uint citizenID, ref Citizen data)
        {
            try
            {
                CitizenManager _citizenManager = Singleton<CitizenManager>.instance;

                if (data.m_homeBuilding == 0 && data.m_workBuilding == 0 && data.m_visitBuilding == 0 && data.m_instance == 0 && data.m_vehicle == 0)
                {
                    _citizenManager.ReleaseCitizen(citizenID);
                }
                else
                {
                    bool goodsSatisfied = false;

                    switch (data.CurrentLocation)
                    {
                        case Citizen.Location.Home:
                            goodsSatisfied = ResidentLocationHandler.ProcessHome(ref resident, citizenID, ref data);
                            break;
                        case Citizen.Location.Work:
                            goodsSatisfied = ResidentLocationHandler.ProcessWork(ref resident, citizenID, ref data);
                            break;
                        case Citizen.Location.Visit:
                            goodsSatisfied = ResidentLocationHandler.ProcessVisit(ref resident, citizenID, ref data);
                            break;
                        case Citizen.Location.Moving:
                            goodsSatisfied = ResidentLocationHandler.ProcessMoving(ref resident, citizenID, ref data);
                            break;
                    }

                    if (goodsSatisfied)
                    {
                        data.m_flags &= ~Citizen.Flags.NeedGoods;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Error on " + citizenID);
                Debug.LogException(ex);
            }
        }

        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool FindHospital(ResidentAI thisAI, uint citizenID, ushort sourceBuilding, TransferManager.TransferReason reason)
        {
            Debug.LogWarning("FindHospital is not overridden!");
            return false;
        }

        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FindVisitPlace(ResidentAI thisAI, uint citizenID, ushort sourceBuilding, TransferManager.TransferReason reason)
        {
            Debug.LogWarning("FindVisitPlace is not overridden!");
        }

        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static TransferManager.TransferReason GetShoppingReason(ResidentAI thisAI)
        {
            Debug.LogWarning("GetShoppingReason is not overridden!");
            return 0;
        }

        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static TransferManager.TransferReason GetEntertainmentReason(ResidentAI thisAI)
        {
            Debug.LogWarning("GetEntertainmentReason is not overridden!");
            return 0;
        }

        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool DoRandomMove(ResidentAI thisAI)
        {
            Debug.LogWarning("DoRandomMove is not overridden!");
            return false;
        }

        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool StartMoving(ResidentAI thisAI, uint citizenID, ref Citizen data, ushort sourceBuilding, ushort targetBuilding)
        {
            Debug.LogWarning("StartMoving is not overridden!");
            return false;
        }

        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void FindEvacuationPlace(ResidentAI thisAI, uint citizenID, ushort sourceBuilding, TransferManager.TransferReason reason)
        {
            Debug.LogWarning("FindEvacuationPlace is not overridden!");
        }

        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static TransferManager.TransferReason GetEvacuationReason(ResidentAI thisAI, ushort sourceBuilding)
        {
            Debug.LogWarning("GetEvacuationReason is not overridden!");
            return TransferManager.TransferReason.None;
        }
    }
}
