using CimTools.V1.Panels;
using ColossalFramework;
using ColossalFramework.UI;
using RushHour.CimTools;
using RushHour.Events;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RushHour.UI
{
    public class DateTimeBar : MonoBehaviour
    {
        private UpdatePanel _updatePanel = null;
        private UISprite _oldDayProgressSprite = null;
        private UISprite _newDayProgressSprite = null;
        private UILabel _newDayProgressLabel = null;
        private DateTime _lastTime = DateTime.MinValue;
        private List<UISprite> _eventBars = new List<UISprite>();

        public void Initialise()
        {
            UIPanel _uiPanel = UIView.Find<UIPanel>("InfoPanel");
            UIPanel _panelTime = _uiPanel == null ? null : _uiPanel.Find<UIPanel>("PanelTime");
            _oldDayProgressSprite = _panelTime == null ? null : _panelTime.Find<UISprite>("Sprite");
            UILabel _dateLabel = _oldDayProgressSprite == null ? null : _oldDayProgressSprite.Find<UILabel>("Time");

            if (_uiPanel != null && _panelTime != null && _oldDayProgressSprite != null && _dateLabel != null)
            {
                _newDayProgressSprite = _panelTime.AddUIComponent<UISprite>();
                _newDayProgressSprite.name = "NewSprite";
                _newDayProgressSprite.relativePosition = _oldDayProgressSprite.relativePosition;
                _newDayProgressSprite.atlas = _oldDayProgressSprite.atlas;
                _newDayProgressSprite.spriteName = _oldDayProgressSprite.spriteName;
                _newDayProgressSprite.size = _oldDayProgressSprite.size;
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

                _oldDayProgressSprite.Hide();

                InitialiseUpdatePanel(_newDayProgressSprite);
                Update();
            }
            else
            {
                Debug.LogWarning("Didn't replace sprite.");
            }

            Debug.Log("Rush Hour: DateTimeBar initialised");
        }

        private void InitialiseUpdatePanel(UIComponent parent)
        {
            _updatePanel = parent.AddUIComponent<UpdatePanel>();
            _updatePanel.SetPositionSpeakyPoint(new Vector2(parent.position.x, parent.position.y) + new Vector2(parent.size.x, 0));
            _updatePanel.Initialise(CimToolsHandler.CimToolBase);
        }

        private void Update()
        {
            bool useThisDateBar = true;

            CimToolsHandler.CimToolBase.ModOptions.GetOptionValue("CityTimeDateBar", ref useThisDateBar);

            if (useThisDateBar)
            {
                if(_oldDayProgressSprite != null)
                    _oldDayProgressSprite.Hide();

                if (_newDayProgressSprite != null)
                    _newDayProgressSprite.Show();
            }
            else
            {
                if (_oldDayProgressSprite != null)
                    _oldDayProgressSprite.Show();

                if(_newDayProgressSprite != null)
                    _newDayProgressSprite.Hide();
            }

            if (_newDayProgressSprite != null && _newDayProgressLabel != null)
            {
                SimulationManager _simulationManager = Singleton<SimulationManager>.instance;
                DateTime _date = CityEventManager.CITY_TIME;
                Color32 _barColour = new Color32(199, 254, 115, 255);
                double currentHour = _date.TimeOfDay.TotalHours;

                if (_simulationManager.SimulationPaused)
                {
                    _barColour = new Color32(254, 115, 115, 255);
                }
                else if (CityEventManager.instance.EventTakingPlace())
                {
                    _barColour = new Color32(254, 230, 115, 255);
                }

                _newDayProgressSprite.fillAmount = (float)currentHour / 24F;
                _newDayProgressSprite.color = _barColour;

                _newDayProgressLabel.text = string.Format("{0} {1}/{2}/{3}", _date.DayOfWeek.ToString(), _date.Day, _date.Month, _date.Year);

                if (CityEventManager.CITY_TIME - _lastTime >= new TimeSpan(0, 1, 0))
                {
                    _lastTime = CityEventManager.CITY_TIME;
                    FastList<CityEvent> eventsInDay = CityEventManager.instance.EventsThatStartWithin(24D, true);

                    for (int index = 0; index < _eventBars.Count; ++index)
                    {
                        UISprite _childSprite = _eventBars[index];

                        Destroy(_childSprite);
                        _childSprite = null;
                    }

                    _eventBars.Clear();

                    for (int index = 0; index < eventsInDay.m_size; ++index)
                    {
                        CityEvent _event = eventsInDay.m_buffer[index];

                        double startHour = _event.m_eventData.m_eventStartTime.TimeOfDay.TotalHours;
                        double endHour = _event.m_eventData.m_eventFinishTime.TimeOfDay.TotalHours;

                        CreateEvent(_event.ToString(), startHour, endHour, new Color32(254, 230, 115, 255));
                    }
                }
            }
        }

        private void CreateEvent(string eventName, double startHour, double endHour, Color32 colour)
        {
            double startPercent = startHour / 24D;
            double endPercent = endHour / 24D;

            if(endPercent < startPercent)
            {
                CreateEventBlock(eventName, startPercent, 1D, colour);
                CreateEventBlock(eventName, 0D, endPercent, colour);
            }
            else
            {
                CreateEventBlock(eventName, startPercent, endPercent, colour);
            }
        }

        private void CreateEventBlock(string eventName, double startPercent, double endPercent, Color32 colour)
        {
            int padding = 2;

            float startPosition = (float)(_newDayProgressSprite.width * startPercent);
            float endPosition = (float)(_newDayProgressSprite.width * endPercent);
            int endWidth = (int)Mathf.Round(endPosition - startPosition);

            UISprite eventSprite = _newDayProgressSprite.AddUIComponent<UISprite>();
            eventSprite.name = eventName;
            eventSprite.relativePosition = new Vector3(startPosition, padding);
            eventSprite.atlas = _newDayProgressSprite.atlas;
            eventSprite.spriteName = _newDayProgressSprite.spriteName;
            eventSprite.height = _newDayProgressSprite.height - (padding * 2);
            eventSprite.width = endWidth;
            eventSprite.fillDirection = UIFillDirection.Horizontal;
            eventSprite.color = colour;
            eventSprite.fillAmount = 1F;

            _eventBars.Add(eventSprite);
        }
    }
}
