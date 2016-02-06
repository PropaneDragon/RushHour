using ColossalFramework;
using ICities;
using UnityEngine;

namespace RushHour.Zones
{
    public class NewZoneManager : DemandExtensionBase
    {
        public override int OnCalculateCommercialDemand(int originalDemand)
        {
            int finalDemand = originalDemand;

            if (Experiments.ExperimentsToggle.ImprovedCommercialDemand)
            {
                DistrictManager _districtManager = Singleton<DistrictManager>.instance;
                DistrictPrivateData _commercialData = _districtManager.m_districts.m_buffer[0].m_commercialData;

                Debug.Log("Abandoned: " + _commercialData.m_finalAbandonedCount);
                Debug.Log("Count: " + _commercialData.m_finalBuildingCount);
                Debug.Log("Empty: " + _commercialData.m_finalEmptyCount);
                Debug.Log("Home or work: " + _commercialData.m_finalHomeOrWorkCount);
                Debug.Log("Alive " + _commercialData.m_finalAliveCount);

                finalDemand = 100 - (int)Mathf.Round((_commercialData.m_finalEmptyCount / _commercialData.m_finalAliveCount) * 100f);
            }

            return finalDemand;
        }
    }
}
