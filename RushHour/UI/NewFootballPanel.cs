using ColossalFramework;
using ColossalFramework.UI;
using RushHour.Redirection;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace RushHour.UI
{
    /*[TargetType(typeof(FootballPanel))]
    internal class NewFootballPanel
    {
        [RedirectMethod]
        public static void UpdateBindings(FootballPanel thisPanel)
        {
            NewBuildingWorldInfoPanel.UpdateBindings(thisPanel);

            FieldInfo m_InstanceIDInfo = typeof(FootballPanel).GetField("m_InstanceID", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo m_upkeepInfo = typeof(FootballPanel).GetField("m_upkeep", BindingFlags.NonPublic | BindingFlags.Instance);

            InstanceID? m_InstanceID = m_InstanceIDInfo.GetValue(thisPanel) as InstanceID?;
            UILabel m_upkeep = m_upkeepInfo.GetValue(thisPanel) as UILabel;

            if (m_InstanceID != null && m_upkeep != null)
            {
                if (!Singleton<BuildingManager>.exists || m_InstanceID.Value.Type != InstanceType.Building || m_InstanceID.Value.Building == 0)
                    return;

                ushort building = m_InstanceID.Value.Building;
                BuildingManager instance = Singleton<BuildingManager>.instance;

                m_upkeep.text = LocaleFormatter.FormatUpkeep(instance.m_buildings.m_buffer[building].Info.m_buildingAI.GetResourceRate(building, ref instance.m_buildings.m_buffer[building], EconomyManager.Resource.Maintenance), false);

                RefreshMatchInfo(thisPanel);
                RushHourUpdateStartTime(thisPanel);
            }
        }

        public static void RushHourUpdateStartTime(FootballPanel thisPanel)
        {

        }

        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void RefreshMatchInfo(BuildingWorldInfoPanel thisPanel)
        {
            Debug.LogWarning("RefreshMatchInfo is not overridden!");
        }
    }*/
}
