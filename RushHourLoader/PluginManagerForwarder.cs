using ColossalFramework.Plugins;
using ColossalFramework.Steamworks;
using RushHourLoader.Redirection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace RushHourLoader
{
    [TargetType(typeof(PluginManager))]
    class PluginManagerForwarder
    {
        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void LoadWorkshopPlugin(PluginManager thisManager, PublishedFileId id)
        {
            Debug.LogWarning("LoadWorkshopPlugin is not overridden!");
        }

        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void RemoveWorkshopPlugin(PluginManager thisManager, PublishedFileId id)
        {
            Debug.LogWarning("RemoveWorkshopPlugin is not overridden!");
        }
    }
}
