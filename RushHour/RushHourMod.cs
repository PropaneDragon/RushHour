using ColossalFramework;
using ICities;
using RushHour.Compatibilitiy;
using RushHour.Experiments;
using RushHour.Options;
using UnityEngine;

namespace RushHour
{
    public class RushHourMod : IUserMod
    {
        public string Name => "Rush Hour";
        public string Description => "Improves AI so citizens and tourists act more realistically.";

        public void OnEnabled()
        {
            Debug.Log("Rush Hour has been enabled.");
            DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "Rush Hour has been enabled.");

            Singleton<LoadingManager>.Ensure();
            Singleton<LoadingManager>.instance.m_introLoaded += OnIntroLoaded;

            if(ExperimentsToggle.GhostMode)
            {
                Debug.LogWarning("Rush Hour is in ghost mode! Everything will be disabled.");
                DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Warning, "Rush Hour is in ghost mode! Everything will be disabled.");
            }
        }

        private void OnIntroLoaded()
        {
            if (ExperimentsToggle.ShowIncompatibleMods && !ExperimentsToggle.GhostMode)
            {
                CompatibilityChecker.Instance.DisplayIncompatibleMods();
            }
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            OptionHandler.SetUpOptions(helper);
        }
    }
}
