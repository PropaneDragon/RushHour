using ColossalFramework;
using ColossalFramework.Plugins;
using ColossalFramework.Steamworks;
using ICities;
using RushHourLoader.Redirection;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace RushHourLoader
{
    public class RushHourMod : IUserMod
    {
        public static readonly ulong _cimToolsId = 676481286ul;
        public static readonly ulong _rushHourId = 605590542ul;

        public string Name => "Rush Hour";
        public string Description => "Improves AI so citizens and tourists act more realistically.";

        private static Dictionary<MethodInfo, RedirectCallsState> redirects;

        public void OnEnabled()
        {
            Debug.Log("Rush Hour Enabled.");
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, "Rush Hour Enabled.");

            Redirect();

            Singleton<LoadingManager>.Ensure();
            Singleton<LoadingManager>.instance.m_introLoaded += OnIntroLoaded;
        }

        public void OnDisabled()
        {
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            PublishedFileId cimToolsWorkshop = new PublishedFileId(_cimToolsId);
            RushHourActivator activator = new RushHourActivator(helper, cimToolsWorkshop);

            SettingsHandler.SetUp(helper);
        }

        private void OnIntroLoaded()
        {
            CheckMod();
        }

        private void CheckMod()
        {
            PluginManager pluginManager = Singleton<PluginManager>.instance;
            PublishedFileId cimToolsWorkshop = new PublishedFileId(_cimToolsId);
            RushHourActivator activator = new RushHourActivator(null, cimToolsWorkshop);

            bool foundPlugin = false;

            foreach (PluginManager.PluginInfo pluginInfo in pluginManager.GetPluginsInfo())
            {
                if (pluginInfo.publishedFileID.AsUInt64 == _cimToolsId)
                {
                    foundPlugin = true;
                    break;
                }
            }

            if (foundPlugin)
            {
                //helper.AddButton("This mod requires a subscription to Cim Tools!", delegate { Steam.ActivateGameOverlayToWorkshopItem(cimToolsWorkshop); });
                Debug.Log("Rush Hour: Found a subscription to Cim Tools, activating.");
                activator.ActivateRushHour();
                Debug.Log("Rush Hour: Activated.");
            }
            else
            {
                //helper.AddButton("This mod requires a subscription to Cim Tools!", delegate { Steam.ActivateGameOverlayToWorkshopItem(cimToolsWorkshop); });
                Debug.LogWarning("Rush Hour: Cim Tools couldn't be found. Requesting subscription");

                if (LoadingExtension._activationPopUpGameObject == null)
                {
                    LoadingExtension._activationPopUpGameObject = new GameObject("RushHourActivationPopUp");
                    LoadingExtension._activationPopUp = LoadingExtension._activationPopUpGameObject.AddComponent<SubscribePopUp>();
                }

                if (LoadingExtension._activationPopUp != null)
                {
                    LoadingExtension._activationPopUp.Show();
                    LoadingExtension._activationPopUp.activator = activator;
                    LoadingExtension._activationPopUp.description = "Hey! Sorry to spring this on you, but to make my life easier I now need you to subscribe to <color#C6F47F>Cim Tools</color> for <color#C6F47F>Rush Hour</color> to work. " +
                                                   "If you've just subscribed to Rush Hour then you've probably forgotten to also subscribe to CimTools. Luckily you can do that here." +
                                                   "\n\nThis is just a small utility I made to help in developing Rush Hour (and other mods), but without it the mod won't work :(." +
                                                   "\n\nIf you select <color#C6F47F>\"Subscribe\"</color> you will automatically be subscribed and there's nothing more you'll need to do, but if you don't want to that's fine too.";
                    LoadingExtension._activationPopUp.relativePosition = new Vector3((Screen.width / 2f) - (LoadingExtension._activationPopUp.width / 2f), (Screen.height / 2f) - (LoadingExtension._activationPopUp.height / 2f));
                }
            }
        }        

        public static void Redirect()
        {
            redirects = new Dictionary<MethodInfo, RedirectCallsState>();
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                redirects.AddRange(RedirectionUtil.RedirectType(type));
            }
        }

        private static void RevertRedirect()
        {
            if (redirects == null)
            {
                return;
            }
            foreach (var kvp in redirects)
            {
                RedirectionHelper.RevertRedirect(kvp.Key, kvp.Value);
            }
            redirects.Clear();
        }
    }
}
