using ColossalFramework;
using ColossalFramework.UI;
using RushHour.Containers;
using RushHour.Events;
using RushHour.Events.Unique;
using RushHour.Redirection;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace RushHour.UI
{
    [TargetType(typeof(BuildingWorldInfoPanel))]
    internal class NewBuildingWorldInfoPanel
    {
        protected static InstanceID? lastInstanceID = null;
        protected static bool translationSetUp = false;
        protected static float originalNameWidth = -1;
        protected static UITextField m_NameField = null;
        protected static UserEventCreationWindow eventCreationWindow = null;

        [RedirectMethod]
        public static void OnSetTarget(BuildingWorldInfoPanel thisPanel)
        {
            FieldInfo m_TimeInfo = typeof(BuildingWorldInfoPanel).GetField("m_Time", BindingFlags.NonPublic | BindingFlags.Instance);
            float? m_Time = m_TimeInfo.GetValue(thisPanel) as float?;

            if(m_Time != null)
            {
                m_NameField = thisPanel.Find<UITextField>("BuildingName");

                if(m_NameField != null)
                {
                    if(originalNameWidth == -1)
                    {
                        originalNameWidth = m_NameField.width;
                    }

                    m_NameField.text = GetName(thisPanel);
                    m_Time = 0.0f;

                    CityServiceWorldInfoPanel servicePanel = thisPanel as CityServiceWorldInfoPanel;

                    if(servicePanel != null)
                    {
                        CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.Log("Adding event UI to service panel.");
                        AddEventUI(servicePanel);

                        if (!translationSetUp)
                        {
                            translationSetUp = true;

                            CimToolsHandler.CimToolsHandler.CimToolBase.Translation.OnLanguageChanged += delegate (string languageIdentifier)
                            {
                                UIButton createEventButton = servicePanel.Find<UIButton>("CreateEventButton");

                                createEventButton.tooltip = CimToolsHandler.CimToolsHandler.CimToolBase.Translation.GetTranslation("Event_CreateUserEvent");
                                createEventButton.RefreshTooltip();
                            };
                        }
                    }
                }
                else
                {
                    Debug.LogError("Couldn't set the m_NameField parameter of the BuildingWorldInfoPanel");
                }
            }
            else
            {
                Debug.LogError("Couldn't set the m_Time parameter of the BuildingWorldInfoPanel");
            }
        }

        [RedirectReverse]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetName(BuildingWorldInfoPanel thisPanel)
        {
            Debug.LogWarning("GetName is not overridden!");
            return "";
        }

        public static void AddEventUI(CityServiceWorldInfoPanel cityServicePanel)
        {
            UIMultiStateButton locationButton = cityServicePanel.Find<UIMultiStateButton>("LocationMarker");
            UIButton createEventButton = cityServicePanel.Find<UIButton>("CreateEventButton");
            UIFastList eventSelection = cityServicePanel.Find<UIFastList>("EventSelectionList");
            UserEventCreationWindow eventCreationWindow = cityServicePanel.Find<UserEventCreationWindow>("EventCreator");

            FieldInfo m_InstanceIDInfo = typeof(CityServiceWorldInfoPanel).GetField("m_InstanceID", BindingFlags.NonPublic | BindingFlags.Instance);
            InstanceID? m_InstanceID = m_InstanceIDInfo.GetValue(cityServicePanel) as InstanceID?;

            lastInstanceID = m_InstanceID;

            BuildCreationWindow(cityServicePanel.component);

            if (eventSelection != null)
            {
                eventSelection.Hide();
            }

            if(eventCreationWindow != null)
            {
                eventCreationWindow.Hide();
            }

            if (createEventButton == null)
            {
                createEventButton = cityServicePanel.component.AddUIComponent<UIButton>();
                createEventButton.name = "CreateEventButton";
                createEventButton.atlas = CimToolsHandler.CimToolsHandler.CimToolBase.SpriteUtilities.GetAtlas("Ingame");
                createEventButton.normalFgSprite = "InfoIconLevel";
                createEventButton.disabledFgSprite = "InfoIconLevelDisabled";
                createEventButton.focusedFgSprite = "InfoIconLevelFocused";
                createEventButton.hoveredFgSprite = "InfoIconLevelHovered";
                createEventButton.pressedFgSprite = "InfoIconLevelPressed";
                createEventButton.width = locationButton.width;
                createEventButton.height = locationButton.height;
                createEventButton.position = locationButton.position - new Vector3(createEventButton.width - 5f, 0);
                createEventButton.eventClicked += CreateEventButton_eventClicked;

                BuildDropdownList(createEventButton);
            }

            if(m_InstanceID != null)
            {
                BuildingManager _buildingManager = Singleton<BuildingManager>.instance;
                Building _currentBuilding = _buildingManager.m_buildings.m_buffer[lastInstanceID.Value.Building];

                if (CityEventBuildings.instance.BuildingHasUserEvents(ref _currentBuilding))
                {
                    createEventButton.Show();
                    createEventButton.Enable();
                    m_NameField.width = originalNameWidth - 45f;
                }
                else
                {
                    if (CityEventBuildings.instance.BuildingHasEvents(ref _currentBuilding))
                    {
                        createEventButton.Show();
                        createEventButton.Disable();
                        m_NameField.width = originalNameWidth - 45f;
                    }
                    else
                    {
                        createEventButton.Hide();
                        m_NameField.width = originalNameWidth;
                    }
                }
            }
        }

        private static void BuildCreationWindow(UIComponent parent)
        {
            if (eventCreationWindow == null)
            {
                eventCreationWindow = parent.AddUIComponent<UserEventCreationWindow>();
                eventCreationWindow.name = "EventCreator";
                CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.Log("Creating a new UserEventCreationWindow");
                eventCreationWindow.Hide();
            }
        }

        private static void CreateEventButton_eventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            UIFastList eventSelection = component.parent.Find<UIFastList>("EventSelectionList");
            ushort buildingID = lastInstanceID.Value.Building;

            if (lastInstanceID != null && buildingID != 0)
            {
                BuildingManager _buildingManager = Singleton<BuildingManager>.instance;
                Building _currentBuilding = _buildingManager.m_buildings.m_buffer[buildingID];

                if ((_currentBuilding.m_flags & Building.Flags.Active) != Building.Flags.None)
                {
                    List<CityEvent> userEvents = CityEventBuildings.instance.GetUserEventsForBuilding(ref _currentBuilding);

                    BuildDropdownList(component);

                    if (eventSelection.isVisible)
                    {
                        eventSelection.Hide();
                    }
                    else
                    {
                        eventSelection.selectedIndex = -1;
                        eventSelection.Show();
                        eventSelection.rowsData.Clear();

                        foreach (CityEvent userEvent in userEvents)
                        {
                            XmlEvent xmlUserEvent = userEvent as XmlEvent;

                            if (xmlUserEvent != null)
                            {
                                xmlUserEvent.SetUp(ref buildingID);
                                LabelOptionItem eventToInsert = new LabelOptionItem() { linkedEvent = xmlUserEvent, readableLabel = xmlUserEvent.GetReadableName() };
                                eventSelection.rowsData.Add(eventToInsert);

                                CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.Log(xmlUserEvent.GetReadableName());
                            }
                        }

                        eventSelection.DisplayAt(0);
                    }
                }
            }
        }

        private static void BuildDropdownList(UIComponent component)
        {
            UIFastList eventSelection = component.parent.Find<UIFastList>("EventSelectionList");

            if (eventSelection == null)
            {
                eventSelection = UIFastList.Create<UIFastListLabel>(component.parent);
                eventSelection.name = "EventSelectionList";
                eventSelection.backgroundSprite = "UnlockingPanel";
                eventSelection.size = new Vector2(120, 60);
                eventSelection.canSelect = true;
                eventSelection.relativePosition = component.relativePosition + new Vector3(0, component.height);
                eventSelection.rowHeight = 20f;
                eventSelection.selectedIndex = -1;
                eventSelection.eventClicked += EventSelection_eventClicked;
                eventSelection.eventSelectedIndexChanged += EventSelection_eventSelectedIndexChanged;
                eventSelection.Hide();
            }
        }

        private static void UpdateEventSelection(UIComponent component)
        {
            UIFastList list = component as UIFastList;

            if (list != null)
            {
                LabelOptionItem selectedOption = list.selectedItem as LabelOptionItem;

                if (selectedOption != null && eventCreationWindow != null)
                {
                    eventCreationWindow.Show();
                    eventCreationWindow.SetUp(selectedOption, lastInstanceID.Value.Building);
                    eventCreationWindow.relativePosition = list.relativePosition + new Vector3(-(list.width / 2f), list.height);

                    CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.Log("Selected " + list.selectedIndex);
                }
                else
                {
                    CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.LogError("Couldn't find the option that has been selected for an event!");
                }
            }
            else
            {
                CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.LogError("Couldn't find the list that the selection was made on!");
            }
        }

        private static void EventSelection_eventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.Log("Clicked");
            UpdateEventSelection(component);
        }

        private static void EventSelection_eventSelectedIndexChanged(UIComponent component, int value)
        {
            CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.Log("IndexChanged");
            UpdateEventSelection(component);
        }
    }
}
