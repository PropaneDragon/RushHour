using System.Collections.Generic;
using System.Reflection;
using ICities;
using RushHour.Redirection;
using UnityEngine;
using RushHour.UI;

namespace RushHour
{
    public class LoadingExtension : LoadingExtensionBase
    {
        private static Dictionary<MethodInfo, RedirectCallsState> redirects;
        private GameObject _dateTimeGameObject = null;
        private DateTimeBar _dateTimeBar = null;

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);

            if (mode != LoadMode.LoadGame && mode != LoadMode.NewGame)
            {
                return;
            }

            _dateTimeGameObject = new GameObject("DateTimeBar");
            _dateTimeBar = _dateTimeGameObject.AddComponent<DateTimeBar>();
            _dateTimeBar.Initialise();

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