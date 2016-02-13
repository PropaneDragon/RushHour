using System.Collections.Generic;
using System.Reflection;
using ICities;
using RushHour.Redirection;
using UnityEngine;
using RushHour.UI;
using RushHour.CimTools;

namespace RushHour
{
    public class LoadingExtension : LoadingExtensionBase
    {
        private static Dictionary<MethodInfo, RedirectCallsState> redirects;
        private static bool _redirected = false; //Temporary to solve crashing for now. I think it needs to stop threads from calling it while it's reverting the redirect.
        private GameObject _dateTimeGameObject = null;
        private DateTimeBar _dateTimeBar = null;

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);

            if (mode != LoadMode.LoadGame && mode != LoadMode.NewGame)
            {
                return;
            }

            CimToolsHandler.CimToolBase.DetailedLogger.Log("Loading mod");
            CimToolsHandler.CimToolBase.Changelog.DownloadChangelog();
            CimToolsHandler.CimToolBase.XMLFileOptions.Load();

            _dateTimeGameObject = new GameObject("DateTimeBar");
            _dateTimeBar = _dateTimeGameObject.AddComponent<DateTimeBar>();
            _dateTimeBar.Initialise();
            
            Redirect();
        }

        public override void OnLevelUnloading()
        {
            if (Experiments.ExperimentsToggle.RevertRedirects)
            {
                RevertRedirect();
            }

            base.OnLevelUnloading();
        }

        public static void Redirect()
        {
            if (!_redirected || Experiments.ExperimentsToggle.RevertRedirects)
            {
                _redirected = true;

                redirects = new Dictionary<MethodInfo, RedirectCallsState>();
                foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
                {
                    redirects.AddRange(RedirectionUtil.RedirectType(type));
                }
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