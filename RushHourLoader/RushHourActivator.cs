using ColossalFramework;
using ColossalFramework.Plugins;
using ColossalFramework.Steamworks;
using ICities;
using System.IO;
using System.Reflection;
using System.Timers;
using UnityEngine;
using System;

namespace RushHourLoader
{
    public delegate void CreateOptionsPanelEventHandler(UIHelperBase helper);

    public class RushHourActivator
    {
        private UIHelperBase _settingsHelper;
        private Timer _workshopStatusUpdateTimer = new Timer(10);
        private PublishedFileId _cimToolsWorkshop;

        public event CreateOptionsPanelEventHandler OnRequestOptionCreation;

        public RushHourActivator(UIHelperBase settingsHelper, PublishedFileId cimToolsWorkshop)
        {
            _settingsHelper = settingsHelper;
            _cimToolsWorkshop = cimToolsWorkshop;

            _workshopStatusUpdateTimer.AutoReset = true;
            _workshopStatusUpdateTimer.Elapsed += _workshopStatusUpdateTimer_Elapsed;
        }

        private void _workshopStatusUpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            float progress = Steam.workshop.GetSubscribedItemProgress(_cimToolsWorkshop);

            if (progress > 0f)
            {
                Debug.Log("Rush Hour: Subscribing to Cim Tools (" + (progress * 100f).ToString() + "%)");

                if (progress >= 1f)
                {
                    _workshopStatusUpdateTimer.Stop();
                    _workshopStatusUpdateTimer.Elapsed -= _workshopStatusUpdateTimer_Elapsed;
                    ActivateRushHour();
                }
            }
        }

        internal void CreateSettings(UIHelperBase helper)
        {
            throw new NotImplementedException();
        }

        public void SubscribeToCimTools()
        {
            bool subscribed = Steam.workshop.Subscribe(_cimToolsWorkshop);

            _workshopStatusUpdateTimer.Enabled = true;
            _workshopStatusUpdateTimer.Start();
        }

        public void ActivateRushHour()
        {
            Debug.Log("Rush Hour: Enabling main mod dll...");

            foreach (PublishedFileId id in Steam.workshop.GetSubscribedItems())
            {
                if (id.AsUInt64 == RushHourMod._rushHourId)
                {
                    string subscribedItemPath = Steam.workshop.GetSubscribedItemPath(id);

                    if (subscribedItemPath != null)
                    {
                        string unloadedFilePath = subscribedItemPath + Path.DirectorySeparatorChar + "RushHour.unloaded";
                        string loadedFilePath = subscribedItemPath + Path.DirectorySeparatorChar + "zRushHour.dll";

                        if (File.Exists(unloadedFilePath))
                        {
                            Assembly currentAssembly = null;
                            Assembly oldAssembly = null;

                            if (File.Exists(loadedFilePath))
                            {
                                Debug.Log("Rush Hour: Loading assemblies to check versions");
                                currentAssembly = Assembly.ReflectionOnlyLoadFrom(unloadedFilePath);
                                oldAssembly = Assembly.ReflectionOnlyLoadFrom(loadedFilePath);
                            }

                            if (currentAssembly != null && oldAssembly != null)
                            {
                                if(oldAssembly.GetName().Version == currentAssembly.GetName().Version)
                                {
                                    Debug.Log("Version numbers of both assemblies are equal. Leaving as they are.");
                                    TriggerMod();
                                }
                                else
                                {
                                    Debug.LogWarning("Version numbers of both assemblies are not equal. Copying and reloading.");
                                    CopyAndReload(loadedFilePath, unloadedFilePath, id);
                                }
                            }
                            else
                            {
                                CopyAndReload(loadedFilePath, unloadedFilePath, id);
                            }
                        }
                    }
                }
            }
        }

        private void TriggerMod()
        {
            PluginManager pluginManager = Singleton<PluginManager>.instance;

            foreach (PluginManager.PluginInfo pluginInfo in pluginManager.GetPluginsInfo())
            {
                if (pluginInfo.publishedFileID.AsUInt64 == RushHourMod._rushHourId)
                {
                    ModExtensionHandler[] extensions = pluginInfo.GetInstances<ModExtensionHandler>();

                    if(extensions.Length > 0)
                    {
                        foreach(ModExtensionHandler extension in extensions)
                        {
                            Debug.Log("Triggering " + extension.GetType().Assembly.GetName());
                            extension.OnEnabled();
                        }
                    }
                    else
                    {
                        Debug.LogError("No extensions found! This shouldn't happen if Rush Hour has been enabled correctly.");
                    }
                }
            }
        }

        /*public void CreateSettings()
        {
            if (_settingsHelper != null)
            {
                PluginManager pluginManager = Singleton<PluginManager>.instance;

                foreach (PluginManager.PluginInfo pluginInfo in pluginManager.GetPluginsInfo())
                {
                    if (pluginInfo.publishedFileID.AsUInt64 == RushHourMod._rushHourId)
                    {
                        if (pluginInfo.assemblyCount < 3)
                        {
                            _settingsHelper.AddButton("This mod requires a subscription to Cim Tools!", delegate { Steam.ActivateGameOverlayToWorkshopItem(_cimToolsWorkshop); });
                        }
                        break;
                    }
                }

                if (OnRequestOptionCreation != null)
                {
                    OnRequestOptionCreation(_settingsHelper);
                }
            }
        }*/

        private void CopyAndReload(string loadedFilePath, string unloadedFilePath, PublishedFileId id)
        {
            if (File.Exists(loadedFilePath))
            {
                Debug.Log("Rush Hour: Deleting existing file at " + loadedFilePath);
                File.Delete(loadedFilePath);
            }

            Debug.Log("Rush Hour: Copying " + unloadedFilePath + " to " + loadedFilePath);
            File.Copy(unloadedFilePath, loadedFilePath);

            if (File.Exists(loadedFilePath))
            {
                Debug.Log("Rush Hour: Reloading mods...");
                Reload(id);
            }
        }

        private void Reload(PublishedFileId id)
        {
            if (LoadingExtension._activationPopUp != null)
            {
                LoadingExtension._activationPopUp.Hide();
                LoadingExtension._activationPopUp.Invalidate();
                LoadingExtension._activationPopUp = null;
            }

            if (LoadingExtension._activationPopUpGameObject != null)
            {
                LoadingExtension._activationPopUpGameObject.SetActive(false);
                LoadingExtension._activationPopUpGameObject = null;
            }

            Debug.Log("Rush Hour: Enabled main mod dll. Reloading mod.");

            PluginManager thisPluginManager = Singleton<PluginManager>.instance;
            PluginManagerForwarder.RemoveWorkshopPlugin(thisPluginManager, id);
            PluginManagerForwarder.LoadWorkshopPlugin(thisPluginManager, id);
            TriggerMod();
        }
    }
}
