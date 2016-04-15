using CimTools.V1.Panels;
using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using RushHour.CimTools;
using RushHour.Events;
using RushHour.Localisation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace RushHour.UI
{
    public class DateTimeBar : ToolsModifierControl
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
                _newDayProgressLabel.isInteractive = false;

                _oldDayProgressSprite.Hide();

                InitialiseUpdatePanel(_newDayProgressSprite);
                Update();
            }
            else
            {
                Debug.LogWarning("Didn't replace sprite.");
            }

            CimToolsHandler.CimToolBase.Translation.OnLanguageChanged += delegate (string languageIdentifier)
            {
                UpdateEventBlocks();
            };

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

                if (_simulationManager.SimulationPaused || _simulationManager.ForcedSimulationPaused)
                {
                    _barColour = new Color32(254, 115, 115, 255);
                }
                else if (CityEventManager.instance.EventTakingPlace())
                {
                    _barColour = new Color32(254, 230, 115, 255);
                }

                _newDayProgressSprite.fillAmount = (float)currentHour / 24F;
                _newDayProgressSprite.color = _barColour;
                _newDayProgressSprite.tooltip = _date.ToString(Experiments.ExperimentsToggle.DateFormat, LocaleManager.cultureInfo);

                _newDayProgressLabel.text = _date.ToString(Experiments.ExperimentsToggle.NormalClock ? "dddd HH:mm" : "dddd hh:mm tt", LocaleManager.cultureInfo);

                if (CityEventManager.CITY_TIME - _lastTime >= new TimeSpan(0, 1, 0))
                {
                    UpdateEventBlocks();
                }

                _newDayProgressLabel.BringToFront();
            }
        }

        private void CreateEvent(CityEventData eventData, Color32 colour)
        {
            double startPercent = eventData.m_eventStartTime.TimeOfDay.TotalHours / 24D;
            double endPercent = eventData.m_eventFinishTime.TimeOfDay.TotalHours / 24D;

            string tooltip = string.Format(LocalisationStrings.DATETIME_EVENTLOCATION, eventData.m_eventStartTime.ToString(Experiments.ExperimentsToggle.NormalClock ? "HH:mm" : "hh:mm tt", LocaleManager.cultureInfo), eventData.m_eventFinishTime.ToString(Experiments.ExperimentsToggle.NormalClock ? "HH:mm" : "hh:mm tt", LocaleManager.cultureInfo), CleanUpName(Singleton<BuildingManager>.instance.m_buildings.m_buffer[eventData.m_eventBuilding].Info.name));

            if(endPercent < startPercent)
            {
                CreateEventBlock(startPercent, 1D, colour, eventData.m_eventBuilding, tooltip);
                CreateEventBlock(0D, endPercent, colour, eventData.m_eventBuilding, tooltip);
            }
            else
            {
                CreateEventBlock(startPercent, endPercent, colour, eventData.m_eventBuilding, tooltip);
            }
        }

        private void CreateEventBlock(double startPercent, double endPercent, Color32 colour, ushort buildingId, string tooltip = "")
        {
            int padding = 2;

            float startPosition = (float)(_newDayProgressSprite.width * startPercent);
            float endPosition = (float)(_newDayProgressSprite.width * endPercent);
            int endWidth = (int)Mathf.Round(endPosition - startPosition);

            UISprite eventSprite = _newDayProgressSprite.AddUIComponent<UISprite>();
            eventSprite.relativePosition = new Vector3(startPosition, padding);
            eventSprite.atlas = _newDayProgressSprite.atlas;
            eventSprite.spriteName = _newDayProgressSprite.spriteName;
            eventSprite.height = _newDayProgressSprite.height - (padding * 2);
            eventSprite.width = endWidth;
            eventSprite.fillDirection = UIFillDirection.Horizontal;
            eventSprite.color = colour;
            eventSprite.fillAmount = 1F;
            eventSprite.tooltip = tooltip;
            eventSprite.eventClicked += ((component, eventHandler) =>
            {
                InstanceID instance = new InstanceID();
                instance.Building = buildingId;

                cameraController.SetTarget(instance, Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingId].m_position, true);
            });

            _eventBars.Add(eventSprite);
        }

        private void ClearEventBlocks()
        {
            for (int index = 0; index < _eventBars.Count; ++index)
            {
                UISprite _childSprite = _eventBars[index];

                Destroy(_childSprite);
                _childSprite = null;
            }

            _eventBars.Clear();
        }

        private void UpdateEventBlocks()
        {
            _lastTime = CityEventManager.CITY_TIME;
            FastList<CityEvent> eventsInDay = CityEventManager.instance.EventsThatStartWithin(24D, true);

            ClearEventBlocks();

            for (int index = 0; index < eventsInDay.m_size; ++index)
            {
                CityEvent _event = eventsInDay.m_buffer[index];

                CreateEvent(_event.m_eventData, new Color32(254, 230, 115, 255));
            }
        }

        /// <summary>
        /// Removes any crap data around the name of workshop items
        /// </summary>
        /// <param name="name">The object name to be printed</param>
        /// <returns>The cleaned up name</returns>
        public static string CleanUpName(string name)
        {
            Regex removeSteamworksData = new Regex("(?:[0-9]*\\.)(.*)(?:_Data.*)");
            Regex addSpacingOnUppercase = new Regex("([a-z]|[0-9])([A-Z])");

            name = removeSteamworksData.Replace(name, "$1");
            name = addSpacingOnUppercase.Replace(name, "$1 $2");

            return name;
        }
    }
}
