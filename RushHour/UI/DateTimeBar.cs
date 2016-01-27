using ColossalFramework;
using ColossalFramework.UI;
using RushHour.Events;
using System;
using UnityEngine;

namespace RushHour.UI
{
    public class DateTimeBar : MonoBehaviour
    {
        private UISprite _newDayProgressSprite = null;
        private UILabel _newDayProgressLabel = null;

        public void Initialise()
        {
            UIPanel _uiPanel = UIView.Find<UIPanel>("InfoPanel");
            UIPanel _panelTime = _uiPanel == null ? null : _uiPanel.Find<UIPanel>("PanelTime");
            UISprite _dayProgressSrite = _panelTime == null ? null : _panelTime.Find<UISprite>("Sprite");
            UILabel _dateLabel = _dayProgressSrite == null ? null : _dayProgressSrite.Find<UILabel>("Time");

            if (_uiPanel != null && _panelTime != null && _dayProgressSrite != null && _dateLabel != null)
            {
                _newDayProgressSprite = _panelTime.AddUIComponent<UISprite>();
                _newDayProgressSprite.name = "NewSprite";
                _newDayProgressSprite.relativePosition = _dayProgressSrite.relativePosition;
                _newDayProgressSprite.spriteName = _dayProgressSrite.spriteName;
                _newDayProgressSprite.size = _dayProgressSrite.size;
                _newDayProgressSprite.atlas = _dayProgressSrite.atlas;
                _newDayProgressSprite.fillAmount = 0.5f;
                _newDayProgressSprite.fillDirection = UIFillDirection.Horizontal;
                _newDayProgressSprite.color = new Color32(255, 255, 255, 255);

                _newDayProgressLabel = _newDayProgressSprite.AddUIComponent<UILabel>();
                _newDayProgressLabel.name = "NewTime";
                _newDayProgressLabel.autoSize = false;
                _newDayProgressLabel.autoHeight = false;
                _newDayProgressLabel.font = _dateLabel.font;
                _newDayProgressLabel.atlas = _dateLabel.atlas;
                _newDayProgressLabel.color = _dateLabel.color;
                _newDayProgressLabel.textColor = _dateLabel.textColor; 
                _newDayProgressLabel.size = _newDayProgressSprite.size;
                _newDayProgressLabel.width = _newDayProgressSprite.width;
                _newDayProgressLabel.height = _newDayProgressSprite.height;
                _newDayProgressLabel.textAlignment = UIHorizontalAlignment.Center;
                _newDayProgressLabel.verticalAlignment = UIVerticalAlignment.Middle;
                _newDayProgressLabel.relativePosition = new Vector3(0, 0, 0);

                _dayProgressSrite.Hide();
                Update();
            }
            else
            {
                Debug.LogWarning("Didn't replace sprite.");
            }

            Debug.Log("Rush Hour: DateTimeBar initialised");
        }

        private void Update()
        {
            if(_newDayProgressSprite != null && _newDayProgressLabel != null)
            {
                SimulationManager _simulationManager = Singleton<SimulationManager>.instance;
                DateTime _date = CityEventManager.CITY_TIME;
                Color32 _barColour = new Color32(199, 254, 115, 255);
                double currentHour = _date.TimeOfDay.TotalHours;

                if (_simulationManager.SimulationPaused)
                {
                    _barColour = new Color32(254, 115, 115, 255);
                }

                _newDayProgressSprite.fillAmount = (float)currentHour / 24F;
                _newDayProgressSprite.color = _barColour;

                _newDayProgressLabel.text = string.Format("{0} {1}/{2}/{3}", _date.DayOfWeek.ToString(), _date.Day, _date.Month, _date.Year);
            }
        }
    }
}
