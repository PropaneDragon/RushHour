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
            int commercialCount = (int)districtData.m_commercialData.m_finalHomeOrWorkCount + (int)districtData.m_commercialData.m_finalEmptyCount;
            int residentialCount = (int)districtData.m_residentialData.m_finalHomeOrWorkCount - (int)districtData.m_residentialData.m_finalEmptyCount;
            int visitorHome = (int)districtData.m_visitorData.m_finalHomeOrWorkCount;
            int visitorEmpty = (int)districtData.m_visitorData.m_finalEmptyCount;
            int residentialClamped = Mathf.Clamp(residentialCount, 0, 50);
            int commercialMult = commercialCount * 10 * 16 / 100;
            int residentialMult = residentialCount * 20 / 100;
            int demand = residentialClamped + Mathf.Clamp((residentialMult * 200 - commercialMult * 200) / Mathf.Max(commercialMult, 100), -50, 50) + Mathf.Clamp((visitorHome * 100 - visitorEmpty * 300) / Mathf.Max(visitorHome, 100), -50, 50);
            thisZoneManager.m_DemandWrapper.OnCalculateCommercialDemand(ref demand);

            //Debug.Log("C: " + commercialCount + ", R: " + residentialCount + " V: " + visitorHome + ", " + visitorEmpty + " D: " + demand);

            return Mathf.Clamp(demand, 0, 100);
        }
    }
}
