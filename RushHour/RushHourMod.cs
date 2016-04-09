using CimTools.V1.Utilities;
using ICities;
using System.Collections.Generic;
using RushHour.Places;
using CimTools.V1.File;
using ColossalFramework.Plugins;
using RushHour.UI;
using ColossalFramework.UI;
using UnityEngine;

namespace RushHour
{
    public class RushHourMod : IUserMod
    {
        private Dictionary<string, List<OptionsItemBase>> allOptions = new Dictionary<string, List<OptionsItemBase>>
        {
            {
                "General", new List<OptionsItemBase>
                {
                    new OptionsCheckbox() { readableName = "Enable weekends", value = true, uniqueName = "Weekends1", enabled = true }, //Weekends1 because I needed to override the old value. Silly me
                    new OptionsCheckbox() { readableName = "Allow workers to go out for lunch", value = true, uniqueName = "LunchRush" },
                    new OptionsCheckbox() { readableName = "Use modified date bar", value = true, uniqueName = "CityTimeDateBar" },
                    new OptionsCheckbox() { readableName = "Increase use of parking spaces", value = true, uniqueName = "BetterParking" },
                    new OptionsSlider() { readableName = "Parking search radius", value = 100f, max = 500f, min = 16f, step = 1f, uniqueName = "ParkingSearchRadius" },
                    new OptionsSpace() { spacing = 20 },
                    new OptionsCheckbox() { readableName = "Improved commercial, industrial and office demand", value = true, uniqueName = "UseImprovedCommercial1" },
                    new OptionsCheckbox() { readableName = "Improved residential demand", value = true, uniqueName = "UseImprovedResidential" },
                    new OptionsCheckbox() { readableName = "Ghost mode (coming soon)", value = false, uniqueName = "GhostMode", enabled = false },
                    new OptionsCheckbox() { readableName = "24 hour clock", value = true, uniqueName = "24HourClock" },
                    new OptionsDropdown() { readableName = "Date format", value = "dd/MM/yyyy", uniqueName = "DateFormat", options = new string[]{ "dd/MM/yyyy", "MM/dd/yyyy", "yyyy/MM/dd" } },
                }
            },
            {
                "School", new List<OptionsItemBase>
                {
                    new TimeOfDaySlider() { readableName = "School earliest start time", value = Chances.m_minSchoolHour, min = 5f, max = 11f, step = 0.0833333334f, uniqueName = "SchoolStartTimeVariance2" },
                    new TimeOfDaySlider() { readableName = "School latest start time", value = Chances.m_startSchoolHour, min = 5f, max = 11f, step = 0.0833333334f, uniqueName = "SchoolStartTime2" },
                    new TimeOfDaySlider() { readableName = "School earliest end time", value = Chances.m_endSchoolHour, min = 13f, max = 18f, step = 0.0833333334f, uniqueName = "SchoolEndTime2" },
                    new TimeOfDaySlider() { readableName = "School latest end time", value = Chances.m_maxSchoolHour, min = 13f, max = 18f, step = 0.0833333334f, uniqueName = "SchoolEndTimeVariance2" },

                    new TimeOfDayVarianceSlider() { readableName = "Don't travel to school if under this many hours left", value = Chances.m_minSchoolDuration, min = 0.5f, max = 4f, uniqueName = "SchoolDurationMinimum2" },
                }
            },
            {
                "Work", new List<OptionsItemBase>
                {
                    new TimeOfDaySlider() { readableName = "Work earliest start time", value = Chances.m_minWorkHour, min = 5f, max = 12f, step = 0.0833333334f, uniqueName = "WorkStartTimeVariance2" },
                    new TimeOfDaySlider() { readableName = "Work latest start time", value = Chances.m_startWorkHour, min = 5f, max = 12f, step = 0.0833333334f, uniqueName = "WorkStartTime2" },
                    new TimeOfDaySlider() { readableName = "Work earliest end time", value = Chances.m_endWorkHour, min = 14f, max = 18f, step = 0.0833333334f, uniqueName = "WorkEndTime2" },
                    new TimeOfDaySlider() { readableName = "Work latest end time", value = Chances.m_maxWorkHour, min = 14f, max = 18f, step = 0.0833333334f, uniqueName = "WorkEndTimeVariance2" },
                    
                    new TimeOfDayVarianceSlider() { readableName = "Don't travel to work if under this many hours left", value = Chances.m_minWorkDuration, min = 0.5f, max = 4f, uniqueName = "WorkDurationMinimum2" }
                }
            },
            {
                "Events", new List<OptionsItemBase>
                {
                    new OptionsCheckbox() { readableName = "Enable random events", value = true, uniqueName = "RandomEvents" },
                    new OptionsCheckbox() { readableName = "More coming soon!", value = false, enabled = false },
                    /*new OptionsSlider() { readableName = "Maximum events at once", value = 1f, max = 5f, min = 1f, step = 1f, uniqueName = "MaximumEventsAtOnce" },
                    new TimeOfDayVarianceSlider() { readableName = "Minimum hours between events", value = 24f, max = 144f, min = 0f, step = 1f, uniqueName = "MinHoursBetweenEvents" },
                    new TimeOfDayVarianceSlider() { readableName = "Maximum hours between events", value = 48f, max = 144f, min = 24f, step = 1f, uniqueName = "MaxHoursBetweenEvents" }*/
                }
            },
            {
                "Experimental", new List<OptionsItemBase>
                {
                    new OptionsCheckbox() { readableName = "EXPERIMENTAL: Better time progression (like Time Warp)", value = true, uniqueName = "SlowTimeProgression" },
                    new OptionsDropdown() { readableName = "EXPERIMENTAL: Time progression speed (default 0.25)", value = "0.25", uniqueName = "SlowTimeProgressionSpeed", options = new string[]{ "0.125", "0.25", "0.33", "0.5", "2", "4", "8", "16" } },
                    new OptionsCheckbox() { readableName = "EXPERIMENTAL: No cooldown timer on random events", value = false, uniqueName = "ForceRandomEvents" },
                    new OptionsCheckbox() { readableName = "EXPERIMENTAL: Allow commercial fix for inactive buildings", value = true, uniqueName = "FixInactiveBuildings" },
                    new OptionsSpace() { spacing = 20 },
                    new OptionsCheckbox() { readableName = "DEVELOPER: Print all monuments in your city to the console", value = false, uniqueName = "PrintMonuments" },
                    new OptionsCheckbox() { readableName = "DEVELOPER: Enable \"Force\" XML parameter to immediately create a chosen event", value = false, uniqueName = "ForceXMLEnabled" },
                }
            }
        };

        public string Name => "Rush Hour";
        public string Description => "Improves AI so citizens and tourists act more realistically.";

        public void OnSettingsUI(UIHelperBase helper)
        {
            UIHelper actualHelper = helper as UIHelper;
            UIComponent container = actualHelper.self as UIComponent;

            //Find the tab button in the KeyMappingPanel, so we can copy it
            UIButton tabTemplate = GameObject.Find("KeyMappingTabStrip").GetComponentInChildren<UIButton>();

            UITabstrip tabStrip = container.AddUIComponent<UITabstrip>();
            tabStrip.relativePosition = new Vector3(0, 0);
            tabStrip.size = new Vector2(container.width - 20, 40);

            UITabContainer tabContainer = container.AddUIComponent<UITabContainer>();
            tabContainer.relativePosition = new Vector3(0, 40);
            tabContainer.size = new Vector2(container.width - 20, container.height - tabStrip.height - 20);
            tabStrip.tabPages = tabContainer;

            loadSettingsFromSaveFile();

            int currentIndex = 0;
            foreach(KeyValuePair<string, List<OptionsItemBase>> optionGroup in allOptions)
            {
                UIButton settingsButton = tabStrip.AddTab(optionGroup.Key, tabTemplate, true);
                tabStrip.selectedIndex = currentIndex;

                UIPanel currentPanel = tabStrip.tabContainer.components[currentIndex++] as UIPanel;
                currentPanel.autoLayout = true;
                currentPanel.autoLayoutDirection = LayoutDirection.Vertical;
                currentPanel.autoLayoutPadding.top = 5;
                currentPanel.autoLayoutPadding.left = 10;
                currentPanel.autoLayoutPadding.right = 10;

                UIHelper panelHelper = new UIHelper(currentPanel);

                CimTools.CimToolsHandler.CimToolBase.ModOptions.CreateOptions(panelHelper, optionGroup.Value, optionGroup.Key);
            }

            CimTools.CimToolsHandler.CimToolBase.ModOptions.OnOptionPanelSaved += new OptionPanelSaved(loadSettingsFromSaveFile);
        }

        private void loadSettingsFromSaveFile()
        {
            Debug.Log("RushHour: Safely loading saved data.");

            safelyGetValue("RandomEvents", ref Experiments.ExperimentsToggle.EnableRandomEvents, "IngameOptions");
            safelyGetValue("ForceRandomEvents", ref Experiments.ExperimentsToggle.ForceEventToHappen, "IngameOptions");
            safelyGetValue("UseImprovedCommercial1", ref Experiments.ExperimentsToggle.ImprovedDemand, "IngameOptions");
            safelyGetValue("UseImprovedResidential", ref Experiments.ExperimentsToggle.ImprovedResidentialDemand, "IngameOptions");
            safelyGetValue("Weekends1", ref Experiments.ExperimentsToggle.EnableWeekends, "IngameOptions");
            safelyGetValue("SlowTimeProgression", ref Experiments.ExperimentsToggle.SlowTimeProgression, "IngameOptions");
            safelyGetValue("SlowTimeProgressionSpeed", ref Experiments.ExperimentsToggle.TimeMultiplier, "IngameOptions");
            safelyGetValue("BetterParking", ref Experiments.ExperimentsToggle.ImprovedParkingAI, "IngameOptions");
            safelyGetValue("ParkingSearchRadius", ref Experiments.ExperimentsToggle.ParkingSearchRadius, "IngameOptions");
            safelyGetValue("DateFormat", ref Experiments.ExperimentsToggle.DateFormat, "IngameOptions");
            safelyGetValue("24HourClock", ref Experiments.ExperimentsToggle.NormalClock, "IngameOptions");
            safelyGetValue("LunchRush", ref Experiments.ExperimentsToggle.SimulateLunchTimeRushHour, "IngameOptions");

            safelyGetValue("SchoolStartTime2", ref Chances.m_startSchoolHour, "IngameOptions");
            safelyGetValue("SchoolStartTimeVariance2", ref Chances.m_minSchoolHour, "IngameOptions");
            safelyGetValue("SchoolEndTime2", ref Chances.m_endSchoolHour, "IngameOptions");
            safelyGetValue("SchoolEndTimeVariance2", ref Chances.m_maxSchoolHour, "IngameOptions");
            safelyGetValue("SchoolDurationMinimum2", ref Chances.m_minSchoolDuration, "IngameOptions");

            safelyGetValue("WorkStartTime2", ref Chances.m_startWorkHour, "IngameOptions");
            safelyGetValue("WorkStartTimeVariance2", ref Chances.m_minWorkHour, "IngameOptions");
            safelyGetValue("WorkEndTime2", ref Chances.m_endWorkHour, "IngameOptions");
            safelyGetValue("WorkEndTimeVariance2", ref Chances.m_maxWorkHour, "IngameOptions");
            safelyGetValue("WorkDurationMinimum2", ref Chances.m_minWorkDuration, "IngameOptions");

            safelyGetValue("PrintMonuments", ref Experiments.ExperimentsToggle.PrintAllMonuments, "IngameOptions");
            safelyGetValue("ForceXMLEnabled", ref Experiments.ExperimentsToggle.AllowForcedXMLEvents, "IngameOptions");
            safelyGetValue("FixInactiveBuildings", ref Experiments.ExperimentsToggle.AllowActiveCommercialFix, "IngameOptions");
        }

        /// <summary>
        /// <para>Call [[CimTools.CimToolsHandler.CimToolBase.XMLFileOptions.Data.GetValue]], reporting any errors to [[DebugOutputPanel]].</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="groupName"></param>
        /// <param name="strict"></param>
        /// <returns>The [[ExportOptionBase.OptionError]] for further inspection.</returns>
        private ExportOptionBase.OptionError safelyGetValue<T>(string name, ref T value, string groupName = null, bool strict = true)
        {
            ExportOptionBase.OptionError err = CimTools.CimToolsHandler.CimToolBase.XMLFileOptions.Data.GetValue(name, ref value, groupName, strict);

            if (err != ExportOptionBase.OptionError.NoError)
            {
                string errType = ((ExportOptionBase.OptionError)(int)err).ToString();
                CimTools.CimToolsHandler.CimToolBase.DetailedLogger.LogError(string.Format("An error occurred trying to fetch '{0}': {1}.", name, errType));
            }
            else
            {
                CimTools.CimToolsHandler.CimToolBase.DetailedLogger.Log("Option \"" + name + "\" is " + value);
            }

            return err;
        }
    }
}
