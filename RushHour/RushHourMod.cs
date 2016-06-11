using ColossalFramework.Plugins;
using ICities;
using RushHour.Options;
using RushHourLoader;
using System;
using System.Reflection;
using UnityEngine;

namespace RushHour
{
    public class RushHourMod : PrivatePlugin
    {
        public void OnEnabled()
        {
            Debug.Log("Rush Hour: Activating main dll");

            if (SettingsHandler._helper != null)
            {
                OptionHandler.SetUpOptions(SettingsHandler._helper);
            }
            else
            {
                Debug.LogError("Rush Hour: Couldn't create options for Rush Hour");
                DebugOutputPanel.AddMessage(PluginManager.MessageType.Error, "Couldn't create options for Rush Hour");
            }
        }
    }
}
