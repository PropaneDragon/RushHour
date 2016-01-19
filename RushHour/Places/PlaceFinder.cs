using ColossalFramework;
using UnityEngine;

namespace RushHour.Places
{
    public static class PlaceFinder
    {
        public static bool FindHospital(uint citizenID, ushort sourceBuilding, TransferManager.TransferReason reason)
        {
            if (reason == TransferManager.TransferReason.Dead)
            {
                if (Singleton<UnlockManager>.instance.Unlocked(UnlockManager.Feature.DeathCare))
                    return true;
                
                Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
                return false;
            }
            if (Singleton<UnlockManager>.instance.Unlocked(ItemClass.Service.HealthCare))
            {
                Singleton<TransferManager>.instance.AddOutgoingOffer(reason, new TransferManager.TransferOffer()
                {
                    Priority = 6,
                    Citizen = citizenID,
                    Position = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)sourceBuilding].m_position,
                    Amount = 1,
                    Active = Singleton<SimulationManager>.instance.m_randomizer.Int32(2U) == 0
                });
                return true;
            }
            Singleton<CitizenManager>.instance.ReleaseCitizen(citizenID);
            return false;
        }

        /// <summary>
        /// Finds a place to visit for the specified reason
        /// </summary>
        /// <param name="citizenID">Citizen to move</param>
        /// <param name="sourceBuilding">Building to move citizen from</param>
        /// <param name="reason">Reason for moving</param>
        public static void FindVisitPlace(uint citizenID, ushort sourceBuilding, TransferManager.TransferReason reason)
        {
            Singleton<TransferManager>.instance.AddIncomingOffer(reason, new TransferManager.TransferOffer()
            {
                Priority = Singleton<SimulationManager>.instance.m_randomizer.Int32(8U),
                Citizen = citizenID,
                Position = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)sourceBuilding].m_position,
                Amount = 1,
                Active = true
            });
        }

        public static TransferManager.TransferReason GetShoppingReason()
        {
            switch (Singleton<SimulationManager>.instance.m_randomizer.Int32(8U))
            {
                case 0:
                    return TransferManager.TransferReason.Shopping;
                case 1:
                    return TransferManager.TransferReason.ShoppingB;
                case 2:
                    return TransferManager.TransferReason.ShoppingC;
                case 3:
                    return TransferManager.TransferReason.ShoppingD;
                case 4:
                    return TransferManager.TransferReason.ShoppingE;
                case 5:
                    return TransferManager.TransferReason.ShoppingF;
                case 6:
                    return TransferManager.TransferReason.ShoppingG;
                case 7:
                    return TransferManager.TransferReason.ShoppingH;
                default:
                    return TransferManager.TransferReason.Shopping;
            }
        }

        public static TransferManager.TransferReason GetEntertainmentReason()
        {
            switch (Singleton<SimulationManager>.instance.m_randomizer.Int32(4U))
            {
                case 0:
                    return TransferManager.TransferReason.Entertainment;
                case 1:
                    return TransferManager.TransferReason.EntertainmentB;
                case 2:
                    return TransferManager.TransferReason.EntertainmentC;
                case 3:
                    return TransferManager.TransferReason.EntertainmentD;
                default:
                    return TransferManager.TransferReason.Entertainment;
            }
        }
    }
}
