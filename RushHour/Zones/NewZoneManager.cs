using ColossalFramework;
using ICities;
using UnityEngine;

namespace RushHour.Zones
{
    public class NewZoneManager : DemandExtensionBase
    {
        private static readonly float _commercialToResidentialInfluence = 20f;
        private static readonly float _activeJobInfluence = 80f;

        private static readonly float _maxCommercialToResidentialDifference = 10f;
        private static readonly float _maxEmptyJobs = 2.5f;

        public override int OnCalculateCommercialDemand(int originalDemand)
        {
            int finalDemand = originalDemand;

            if (Experiments.ExperimentsToggle.ImprovedCommercialDemand)
            {
                DistrictManager _districtManager = Singleton<DistrictManager>.instance;
                DistrictPrivateData _commercialData = _districtManager.m_districts.m_buffer[0].m_commercialData;
                DistrictPrivateData _residentialData = _districtManager.m_districts.m_buffer[0].m_residentialData;
                DistrictPrivateData _industrialData = _districtManager.m_districts.m_buffer[0].m_industrialData;
                DistrictPrivateData _playerData = _districtManager.m_districts.m_buffer[0].m_playerData;

                //finalHomeOrWorkCount - Jobs/Houses available
                //finalEmptyCount - Empty jobs/houses
                //aliveCount - Taken jobs/houses
                //buildingCount - Number of buildings (which contain many jobs/houses)
                //I think player data is buildings the player has placed which contain jobs/housing.
                uint _emptyCommercialPositions = _commercialData.m_finalEmptyCount;
                uint _totalResidentialPositions = _residentialData.m_finalHomeOrWorkCount;
                uint _totalCommercialPositions = _commercialData.m_finalHomeOrWorkCount;
                uint _totalJobPositions = _commercialData.m_finalHomeOrWorkCount + _industrialData.m_finalHomeOrWorkCount + _playerData.m_finalHomeOrWorkCount;

                if (_totalCommercialPositions != 0 && _totalResidentialPositions != 0)
                {
                    float _emptyJobPercentage = ((float)_emptyCommercialPositions / (float)_totalCommercialPositions) * 100f;
                    float _adjustedEmptyJobPercentage = (Mathf.Clamp(_emptyJobPercentage, 0, _maxEmptyJobs) / _maxEmptyJobs) * _activeJobInfluence;
                    int _activeJobPercentage = (int)_activeJobInfluence - (int)Mathf.Round(_adjustedEmptyJobPercentage);

                    float _commercialToResidentialPercentage = 100f - (((float)_totalJobPositions / (float)_totalResidentialPositions) * 100f);
                    float _adjustedCommercialToResidentialPercentage = (Mathf.Clamp(_commercialToResidentialPercentage, -_maxCommercialToResidentialDifference, _maxCommercialToResidentialDifference) / _maxCommercialToResidentialDifference) * _commercialToResidentialInfluence;

                    finalDemand = Mathf.Clamp(_activeJobPercentage, (int)-_activeJobInfluence, (int)_activeJobInfluence);
                    finalDemand += Mathf.Clamp((int)_adjustedCommercialToResidentialPercentage, (int)-_commercialToResidentialInfluence, (int)_commercialToResidentialInfluence);
                }
                else
                {
                    finalDemand = (int)_totalResidentialPositions;
                }
            }

            return finalDemand;
        }
    }
}
