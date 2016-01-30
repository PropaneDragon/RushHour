using RushHour.Redirection;
using UnityEngine;

namespace RushHour.Zones
{
    [TargetType(typeof(ZoneManager))]
    public static class NewZoneManager
    {
        [RedirectMethod]
        public static int CalculateCommercialDemand(ZoneManager thisZoneManager, ref District districtData)
        {
            DistrictPrivateData _commercialData = districtData.m_commercialData;
            DistrictPrivateData _residentialData = districtData.m_residentialData;

            int commercialCount = (int)_commercialData.m_finalHomeOrWorkCount - (int)_commercialData.m_finalEmptyCount;
            int residentialCount = (int)_residentialData.m_finalHomeOrWorkCount - (int)_residentialData.m_finalEmptyCount;

            float residentialMultiplier = residentialCount * 1.2F;
            float commercialResidentialPercent = ((float)commercialCount / (float)residentialMultiplier) * 100F;

            int demand = (int)Mathf.Ceil(commercialResidentialPercent - 10F);

            thisZoneManager.m_DemandWrapper.OnCalculateCommercialDemand(ref demand);

            Debug.Log(commercialResidentialPercent);

            return Mathf.Clamp(demand, 0, 100);
        }
    }
}
