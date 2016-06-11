using ColossalFramework;
using ColossalFramework.Plugins;
using ColossalFramework.Steamworks;
using ICities;
using System.IO;
using System.Reflection;
using System.Timers;
using UnityEngine;
using System;
using ColossalFramework.UI;

namespace RushHourLoader
{
    public delegate void CreateOptionsPanelEventHandler(UIHelperBase helper);

    public class RushHourActivator
    {
        private Timer _workshopStatusUpdateTimer = new Timer(10);
        private PublishedFileId _cimToolsWorkshop;

        public event CreateOptionsPanelEventHandler OnRequestOptionCreation;

        public RushHourActivator(PublishedFileId cimToolsWorkshop)
        {
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

        public void SubscribeToCimTools()
        {
            bool subscribed = Steam.workshop.Subscribe(_cimToolsWorkshop);

            _workshopStatusUpdateTimer.Enabled = true;
            _workshopStatusUpdateTimer.Start();
        }

        public void ActivateRushHour()
        {
            Debug.Log("Rush Hour: Internally activating Rush Hour");
            PluginManager pluginManager = Singleton<PluginManager>.instance;

            foreach (PublishedFileId id in Steam.workshop.GetSubscribedItems())
            {
                if (id.AsUInt64 == RushHourMod._rushHourId)
                {
                    string subscribedItemPath = Steam.workshop.GetSubscribedItemPath(id);

                    if (subscribedItemPath != null)
                    {
                        string rushHourDllPath = subscribedItemPath + Path.DirectorySeparatorChar + "RushHour.unloaded";

                        if (File.Exists(rushHourDllPath))
                        {
                            Debug.Log("Rush Hour: Searching for previously loaded Rush Hour plugin information");

                            PluginManager.PluginInfo rushHourPluginInfo = null;
                            PluginManager.PluginInfo pluginInfo = new PluginManager.PluginInfo("", false, new PublishedFileId(10));

                            Assembly rushHourAssembly = Assembly.Load(File.ReadAllBytes(rushHourDllPath));

                            foreach (PluginManager.PluginInfo info in pluginManager.GetPluginsInfo())
                            {
                                if (info.modPath == subscribedItemPath)
                                {
                                    Debug.Log("Rush Hour: Found plugin information");

                                    rushHourPluginInfo = info;
                                    break;
                                }
                            }

                            if(rushHourAssembly != null)
                            {
                                Debug.Log("Rush Hour: Loaded private assemblies");
                                pluginInfo.AddAssembly(rushHourAssembly);

                                PluginManager.userModType = typeof(PrivatePlugin);

                                if (pluginInfo.userModInstance != null)
                                {
                                    MethodInfo method = pluginInfo.userModInstance.GetType().GetMethod("OnEnabled", BindingFlags.Instance | BindingFlags.Public);

                                    if (method != null)
                                    {
                                        try
                                        {
                                            method.Invoke(pluginInfo.userModInstance, null);

                                            if(rushHourPluginInfo != null)
                                            {
                                                rushHourPluginInfo.AddAssembly(rushHourAssembly);
                                                MulticastDelegate pluginChanged = (MulticastDelegate)pluginManager.GetType().GetField("eventPluginsChanged", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(pluginManager);
                                                pluginChanged.DynamicInvoke(new object[] { });
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Debug.LogException(ex);
                                            UIView.ForwardException(new Exception("Rush Hour: Couldn't load private assemblies due to an error", ex));
                                        }
                                        finally
                                        {
                                            PluginManager.userModType = typeof(IUserMod);
                                        }
                                    }
                                }

                                PluginManager.userModType = typeof(IUserMod);
                            }
                            else
                            {
                                Debug.LogError("Failed to load internal RushHour unloaded dll");
                            }
                        }
                    }
                }
            }
        }
    }
}
