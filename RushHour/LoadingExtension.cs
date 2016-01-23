using System.Collections.Generic;
using System.Reflection;
using ICities;
using RushHour.BuildingHandlers;
using RushHour.Redirection;
using RushHour.ResidentHandlers;
using RushHour.TouristHandlers;
using UnityEngine;

namespace RushHour
{
    public class LoadingExtension : LoadingExtensionBase
    {

        private static Dictionary<MethodInfo, RedirectCallsState> redirects;

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);

            if (mode != LoadMode.LoadGame && mode != LoadMode.NewGame)
            {
                return;
            }

            Redirect();
        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();
            RevertRedirect();
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