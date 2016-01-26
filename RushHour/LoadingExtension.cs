using System.Collections.Generic;
using System.Reflection;
using ICities;
using RushHour.BuildingHandlers;
using RushHour.Redirection;
using RushHour.ResidentHandlers;
using RushHour.TouristHandlers;
using UnityEngine;
using ColossalFramework.UI;

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

            UIView view = UIView.GetAView();
            UIPanel _uiPanel = UIView.Find<UIPanel>("InfoPanel");

            if(_uiPanel != null)
            {
                UIPanel _panelTime = _uiPanel.Find<UIPanel>("PanelTime");

                if(_panelTime != null)
                {
                    UISprite _dayProgressSrite = _panelTime.Find<UISprite>("Sprite");

                    if(_dayProgressSrite != null)
                    {
                        UISprite _newDayProgressSprite = _panelTime.AddUIComponent<UISprite>();
                        _newDayProgressSprite.name = "NewSprite";
                        _newDayProgressSprite.relativePosition = _dayProgressSrite.relativePosition;
                        _newDayProgressSprite.spriteName = _dayProgressSrite.spriteName;
                        _newDayProgressSprite.size = _dayProgressSrite.size;
                        _newDayProgressSprite.atlas = _dayProgressSrite.atlas;
                        _newDayProgressSprite.fillAmount = 0.5f;
                        _newDayProgressSprite.fillDirection = UIFillDirection.Horizontal;
                        _newDayProgressSprite.color = new Color32(255, 0, 255, 255);

                        _dayProgressSrite.Hide();
                    }
                    else
                    {
                        Debug.LogWarning("Didn't replace sprite.");
                    }
                }
                else
                {
                    Debug.LogWarning("Didn't replace sprite.");
                }
            }
            else
            {
                Debug.LogWarning("Didn't replace sprite.");
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