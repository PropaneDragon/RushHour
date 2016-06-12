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
        }

        private void OnIntroLoaded()
        {
            if (ExperimentsToggle.ShowIncompatibleMods)
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
