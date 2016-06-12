using ColossalFramework;
using ICities;
using UnityEngine;

namespace RushHour.Zones
{
    public class NewZoneManager : DemandExtensionBase
    {
        /*
        ** Notes:
        ** finalHomeOrWorkCount - Jobs/Houses available
        ** finalEmptyCount - Empty jobs/houses
        ** aliveCount - Taken jobs/houses
        ** buildingCount - Number of buildings (which contain many jobs/houses)
        ** I think player data is buildings the player has placed which contain jobs/housing
        */

        private static readonly float _commercialToResidentialInfluence = 20f;
        private static readonly float _activeJobInfluence = 80f;

        private static readonly float _maxJobsToResidentialDifference = 10f;
        private static readonly float _maxEmptyJobs = 2.5f;

        public override int OnCalculateResidentialDemand(int originalDemand)
        {
            int finalDemand = originalDemand;

            if (Experiments.ExperimentsToggle.ImprovedResidentialDemand)
            {
                DistrictManager _districtManager = Singleton<DistrictManager>.instance;
                DistrictPrivateData _residentialData = _districtManager.m_districts.m_buffer[0].m_residentialData;
                DistrictPrivateData _industrialData = _districtManager.m_districts.m_buffer[0].m_industrialData;
                DistrictPrivateData _commercialData = _districtManager.m_districts.m_buffer[0].m_commercialData;
                DistrictPrivateData _playerData = _districtManager.m_districts.m_buffer[0].m_playerData;
                DistrictEducationData _uneducatedData = _districtManager.m_districts.m_buffer[0].m_educated0Data;
                DistrictEducationData _primarySchoolEducatedData = _districtManager.m_districts.m_buffer[0].m_educated1Data;
                DistrictEducationData _highSchoolEducatedData = _districtManager.m_districts.m_buffer[0].m_educated2Data;
                DistrictEducationData _universityEducatedData = _districtManager.m_districts.m_buffer[0].m_educated3Data;

                uint _totalHousePositions = _residentialData.m_finalHomeOrWorkCount;
                uint _emptyHousePositions = _residentialData.m_finalEmptyCount;
                uint _homelessPeople = _uneducatedData.m_finalHomeless + _primarySchoolEducatedData.m_finalHomeless + _highSchoolEducatedData.m_finalHomeless + _universityEducatedData.m_finalHomeless;
                uint _emptyWorkplacePositions = _commercialData.m_finalEmptyCount + _industrialData.m_finalEmptyCount + _playerData.m_finalEmptyCount;
                uint _totalWorkplacePositions = _commercialData.m_finalHomeOrWorkCount + _industrialData.m_finalHomeOrWorkCount + _playerData.m_finalHomeOrWorkCount;

                if (_emptyHousePositions != 0 && _totalHousePositions != 0 && _emptyWorkplacePositions != 0 && _totalWorkplacePositions != 0)
                {
                    float _emptyJobPercentage = ((float)_emptyWorkplacePositions / (float)_totalWorkplacePositions) * 100f;
                    float _emptyHomesPercentage = ((float)_emptyHousePositions / (float)_totalHousePositions) * 100f;

                    CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.Log("Empty job percentage: " + _emptyJobPercentage);
                    CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.Log("Empty homes percentage: " + _emptyHomesPercentage);
                    CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.Log("Empty houses: " + _emptyHousePositions);
                    CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.Log("Homeless: " + _homelessPeople);

                    finalDemand = (Mathf.RoundToInt(_emptyJobPercentage * 8f) + (int)_homelessPeople) /*- Mathf.RoundToInt(_emptyHomesPercentage)*/;

                    CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.Log("Final residential demand: " + finalDemand);
                }
            }
            else
            {
                CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.Log("Using legacy residential demand");
            }

            return finalDemand;
        }

        public override int OnCalculateWorkplaceDemand(int originalDemand)
        {
            int finalDemand = originalDemand;

            if (Experiments.ExperimentsToggle.ImprovedDemand)
            {
                DistrictManager _districtManager = Singleton<DistrictManager>.instance;
                DistrictPrivateData _commercialData = _districtManager.m_districts.m_buffer[0].m_commercialData;
                DistrictPrivateData _residentialData = _districtManager.m_districts.m_buffer[0].m_residentialData;
                DistrictPrivateData _industrialData = _districtManager.m_districts.m_buffer[0].m_industrialData;
                DistrictPrivateData _playerData = _districtManager.m_districts.m_buffer[0].m_playerData;
                uint _emptyWorkplacePositions = _industrialData.m_finalEmptyCount;
                uint _totalResidentialPositions = _residentialData.m_finalHomeOrWorkCount;
                uint _totalWorkplacePositions = _industrialData.m_finalHomeOrWorkCount;
                uint _totalJobPositions = _commercialData.m_finalHomeOrWorkCount + _industrialData.m_finalHomeOrWorkCount + _playerData.m_finalHomeOrWorkCount;

                if (_totalWorkplacePositions != 0 && _totalResidentialPositions != 0)
                {
                    float _emptyJobPercentage = ((float)_emptyWorkplacePositions / (float)_totalWorkplacePositions) * 100f;
                    float _adjustedEmptyJobPercentage = (Mathf.Clamp(_emptyJobPercentage, 0, _maxEmptyJobs) / _maxEmptyJobs) * _activeJobInfluence;
                    int _activeJobPercentage = (int)_activeJobInfluence - (int)Mathf.Round(_adjustedEmptyJobPercentage);

                    float _workplaceToResidentialPercentage = 100f - (((float)_totalJobPositions / (float)_totalResidentialPositions) * 100f);
                    float _adjustedWorkplaceToResidentialPercentage = (Mathf.Clamp(_workplaceToResidentialPercentage, -_maxJobsToResidentialDifference, _maxJobsToResidentialDifference) / _maxJobsToResidentialDifference) * _commercialToResidentialInfluence;

                    CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.Log("Commercial/Industrial/Player - Residential percentage: " + _workplaceToResidentialPercentage);
                    CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.Log("Empty industrial job positions: " + _emptyJobPercentage);

                    finalDemand = Mathf.Clamp(_activeJobPercentage * 2, (int)-_activeJobInfluence, (int)_activeJobInfluence);
                    finalDemand += Mathf.Clamp((int)_adjustedWorkplaceToResidentialPercentage * 2, (int)-_commercialToResidentialInfluence, (int)_commercialToResidentialInfluence);

                    CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.Log("Final industrial demand: " + finalDemand);
                }
            }
            else
            {
                CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.Log("Using legacy industrial demand");
            }

            return finalDemand;
        }

        public override int OnCalculateCommercialDemand(int originalDemand)
        {
            int finalDemand = originalDemand;

            if (Experiments.ExperimentsToggle.ImprovedDemand)
            {
                DistrictManager _districtManager = Singleton<DistrictManager>.instance;
                DistrictPrivateData _commercialData = _districtManager.m_districts.m_buffer[0].m_commercialData;
                DistrictPrivateData _residentialData = _districtManager.m_districts.m_buffer[0].m_residentialData;
                DistrictPrivateData _industrialData = _districtManager.m_districts.m_buffer[0].m_industrialData;
                DistrictPrivateData _playerData = _districtManager.m_districts.m_buffer[0].m_playerData;
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
                    float _adjustedCommercialToResidentialPercentage = (Mathf.Clamp(_commercialToResidentialPercentage, -_maxJobsToResidentialDifference, _maxJobsToResidentialDifference) / _maxJobsToResidentialDifference) * _commercialToResidentialInfluence;

                    CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.Log("Commercial/Industrial/Player - Residential percentage: " + _commercialToResidentialPercentage);
                    CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.Log("Empty commercial job positions: " + _emptyJobPercentage);

                    finalDemand = Mathf.Clamp(_activeJobPercentage * 2, (int)-_activeJobInfluence, (int)_activeJobInfluence);
                    finalDemand += Mathf.Clamp((int)_adjustedCommercialToResidentialPercentage, (int)-_commercialToResidentialInfluence, (int)_commercialToResidentialInfluence);

                    CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.Log("Final commercial demand: " + finalDemand);
                }
            }
            else
            {
                CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.Log("Using legacy commercial demand");
            }

            return finalDemand;
        }
    }
}
