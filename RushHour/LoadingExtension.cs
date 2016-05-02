using System.Collections.Generic;
using System.Reflection;
using ICities;
using RushHour.Redirection;
using UnityEngine;
using RushHour.UI;
using RushHour.CimToolsHandler;
using RushHour.Events;

namespace RushHour
{
    public class LoadingExtension : LoadingExtensionBase
    {
        private static Dictionary<MethodInfo, RedirectCallsState> redirects;
        private static bool _redirected = false; //Temporary to solve crashing for now. I think it needs to stop threads from calling it while it's reverting the redirect.
        private static bool _simulationRegistered = false;

        public static GameObject _mainUIGameObject = null;

        private GameObject _dateTimeGameObject = null;
        private DateTimeBar _dateTimeBar = null;
        private SimulationExtension _simulationManager = new SimulationExtension();

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);

            if (mode != LoadMode.LoadGame && mode != LoadMode.NewGame)
            {
                return;
            }

            CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.Log("Loading mod");
            CimToolsHandler.CimToolsHandler.CimToolBase.Changelog.DownloadChangelog();
            CimToolsHandler.CimToolsHandler.CimToolBase.XMLFileOptions.Load();

            if (_dateTimeGameObject == null)
            {
                _dateTimeGameObject = new GameObject("DateTimeBar");
            }

            if(_mainUIGameObject == null)
            {
                _mainUIGameObject = new GameObject("RushHourUI");
                EventPopupManager popupManager = EventPopupManager.Instance;
            }

            if (_dateTimeBar == null)
            {
                _dateTimeBar = _dateTimeGameObject.AddComponent<DateTimeBar>();
                _dateTimeBar.Initialise();
            }

            if (!_simulationRegistered)
            {
                SimulationManager.RegisterSimulationManager(_simulationManager);
                _simulationRegistered = true;
            }
            
            Redirect();
        }

        public override void OnLevelUnloading()
        {
            if (Experiments.ExperimentsToggle.RevertRedirects)
            {
                RevertRedirect();
            }

            if (_dateTimeBar != null)
            {
                _dateTimeBar.CloseEverything();
                _dateTimeBar = null;
            }
            
            _dateTimeGameObject = null;
            _simulationManager = null;
            _mainUIGameObject = null;

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