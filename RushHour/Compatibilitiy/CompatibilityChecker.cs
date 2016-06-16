using ColossalFramework;
using ColossalFramework.Plugins;
using ColossalFramework.Steamworks;
using ColossalFramework.UI;
using ICities;
using System.Collections.Generic;

namespace RushHour.Compatibilitiy
{
    internal class CompatibilityChecker
    {
        private static CompatibilityChecker _instance = null;
        public static CompatibilityChecker Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new CompatibilityChecker();
                }

                return _instance;
            }
        }

        private Dictionary<PublishedFileId, string> _incompatibilities = new Dictionary<PublishedFileId, string>()
        {
            {
                new PublishedFileId(524021335ul),
                "The event blip system for the new football events doesn't work with {0}. " +
                "Use the time progression controls in the options for Rush Hour instead. " +
                "Compatibility might be possible in the future, but only if you really need it. Let me know. " +
                "You can still use this mod if you don't mind the dates for events being wrong."
            }
        };

        public Dictionary<PublishedFileId, string> CheckIncompatibilities()
        {
            var userIncompatibilities = new Dictionary<PublishedFileId, string>();

            foreach(PublishedFileId publishedFile in Steam.workshop.GetSubscribedItems())
            {
                if(_incompatibilities.ContainsKey(publishedFile))
                {
                    userIncompatibilities.Add(publishedFile, _incompatibilities[publishedFile]);
                }
            }

            return userIncompatibilities;
        }

        public void DisplayIncompatibleMods()
        {
            Dictionary<PublishedFileId, string> userIncompatibilities = CheckIncompatibilities();

            if(userIncompatibilities.Count > 0)
            {
                PluginManager pluginManager = Singleton<PluginManager>.instance;
                var plugins = pluginManager.GetPluginsInfo();
                string incompatibleList = "";

                foreach(KeyValuePair<PublishedFileId, string> incompatibility in userIncompatibilities)
                {
                    PluginManager.PluginInfo foundPlugin = null;
                    string pluginName = "Unknown mod (" + incompatibility.Key.AsUInt64.ToString() + ")";

                    foreach(PluginManager.PluginInfo plugin in plugins)
                    {
                        if(plugin.publishedFileID == incompatibility.Key)
                        {
                            foundPlugin = plugin;
                            break;
                        }
                    }

                    if (incompatibleList != "")
                    {
                        incompatibleList += "\n\n";
                    }

                    if (foundPlugin != null)
                    {
                        IUserMod[] instances = foundPlugin.GetInstances<IUserMod>();

                        if(instances.Length > 0)
                        {
                            pluginName = instances[0].Name;
                        }
                    }

                    incompatibleList += pluginName + " - " + string.Format(incompatibility.Value, pluginName);
                }

                incompatibleList += "\n\nYou can still run the game with th" + (userIncompatibilities.Count > 1 ? "ese mods" : "is mod") + ", but some things in Rush Hour might not work.";

                UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("Rush Hour has detected incompatible mods", incompatibleList, false);
            }
        }
    }
}
