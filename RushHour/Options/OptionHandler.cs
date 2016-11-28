using CimToolsRushHour.v2.Utilities;
using ColossalFramework.UI;
using ICities;
using RushHour.Places;
using RushHour.UI;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace RushHour.Options
{
    internal static class OptionHandler
    {
        private static Dictionary<string, List<OptionsItemBase>> allOptions = new Dictionary<string, List<OptionsItemBase>>
        {
            {
                "General", new List<OptionsItemBase>
                {
                    new OptionsCheckbox() { value = false, uniqueName = "GhostMode" },
                    new OptionsCheckbox() { value = true, uniqueName = "CityTimeDateBar" },
                    new OptionsCheckbox() { value = true, uniqueName = "AvailableInScenarios" },
                    new OptionsCheckbox() { value = true, uniqueName = "BetterParking" },
                    new OptionsSlider() { value = 100f, max = 500f, min = 16f, step = 1f, uniqueName = "ParkingSearchRadius" },
                    new OptionsSpace() { spacing = 20 },
                    new OptionsCheckbox() { value = true, uniqueName = "UseImprovedCommercial1", translationIdentifier = "UseImprovedCommercial" },
                    new OptionsCheckbox() { value = true, uniqueName = "UseImprovedResidential" },
                    new OptionsDropdown() { value = "English", uniqueName = "Language", options = CimTools.CimToolsHandler.CimToolBase.Translation.AvailableLanguagesReadable().ToArray() }
                }
            },
            {
                "School", new List<OptionsItemBase>
                {
                    new TimeOfDaySlider() { value = Chances.m_minSchoolHour, min = 5f, max = 11f, step = 0.0833333334f, uniqueName = "SchoolStartTimeVariance2", translationIdentifier = "SchoolStartTimeVariance" },
                    new TimeOfDaySlider() { value = Chances.m_startSchoolHour, min = 5f, max = 11f, step = 0.0833333334f, uniqueName = "SchoolStartTime2", translationIdentifier = "SchoolStartTime" },
                    new TimeOfDaySlider() { value = Chances.m_endSchoolHour, min = 13f, max = 18f, step = 0.0833333334f, uniqueName = "SchoolEndTime2", translationIdentifier = "SchoolEndTime" },
                    new TimeOfDaySlider() { value = Chances.m_maxSchoolHour, min = 13f, max = 18f, step = 0.0833333334f, uniqueName = "SchoolEndTimeVariance2", translationIdentifier = "SchoolEndTimeVariance" },

                    new TimeOfDayVarianceSlider() { value = Chances.m_minSchoolDuration, min = 0.5f, max = 4f, uniqueName = "SchoolDurationMinimum2", translationIdentifier = "SchoolDurationMinimum" }
                }
            },
            {
                "Work", new List<OptionsItemBase>
                {
                    new TimeOfDaySlider() { value = Chances.m_minWorkHour, min = 5f, max = 12f, step = 0.0833333334f, uniqueName = "WorkStartTimeVariance2", translationIdentifier = "WorkStartTimeVariance" },
                    new TimeOfDaySlider() { value = Chances.m_startWorkHour, min = 5f, max = 12f, step = 0.0833333334f, uniqueName = "WorkStartTime2", translationIdentifier = "WorkStartTime" },
                    new TimeOfDaySlider() { value = Chances.m_endWorkHour, min = 14f, max = 18f, step = 0.0833333334f, uniqueName = "WorkEndTime2", translationIdentifier = "WorkEndTime" },
                    new TimeOfDaySlider() { value = Chances.m_maxWorkHour, min = 14f, max = 18f, step = 0.0833333334f, uniqueName = "WorkEndTimeVariance2", translationIdentifier = "WorkEndTimeVariance" },

                    new TimeOfDayVarianceSlider() { value = Chances.m_minWorkDuration, min = 0.5f, max = 4f, uniqueName = "WorkDurationMinimum2", translationIdentifier = "WorkDurationMinimum" }
                }
            },
            {
                "Citizens", new List<OptionsItemBase>
                {
                    new OptionsCheckbox() { value = true, uniqueName = "Weekends1", translationIdentifier = "Weekends" }, //Weekends1 because I needed to override the old value. Silly me
                    new OptionsCheckbox() { value = true, uniqueName = "LunchRush" },
                    new OptionsCheckbox() { value = true, uniqueName = "SearchLocally" },

                    new OptionsSlider() { value = 10f, uniqueName = "LocalSearchChance", min = 1f, max = 100f, step = 1f }
                }
            },
            {
                "Events", new List<OptionsItemBase>
                {
                    new OptionsCheckbox() { value = true, uniqueName = "RandomEvents" },
                    new OptionsCheckbox() { value = false, uniqueName = "TeamColourOnBar" }
                    //new OptionsCheckbox() { value = false, uniqueName = "DisableGameEvents", enabled = false }
                }
            },
            {
                "Time", new List<OptionsItemBase>
                {
                    new OptionsCheckbox() { value = true, uniqueName = "TwentyFourHourClock" },
                    new OptionsCheckbox() { value = true, uniqueName = "SlowTimeProgression" },
                    new OptionsDropdown() { value = "0.25", uniqueName = "SlowTimeProgressionSpeed", options = new string[]{ "0.125", "0.25", "0.33", "0.5", "1", "2", "4", "8", "16" } },
                    new OptionsDropdown() { value = "0.25", uniqueName = "SlowTimeProgressionSpeedNight", options = new string[]{ "0.125", "0.25", "0.33", "0.5", "1", "2", "4", "8", "16" } },
                    new TimeOfDaySlider() { value = SimulationManager.SUNRISE_HOUR, uniqueName = "SunriseHour", min = 3f, max = 9f, step = 0.0833333334f, },
                    new TimeOfDaySlider() { value = SimulationManager.SUNSET_HOUR, uniqueName = "SunsetHour", min = 18f, max = 23.99999f, step = 0.0833333334f, },
                    new OptionsDropdown() { value = "dd/MM/yyyy", uniqueName = "DateFormat", options = new string[]{ "dd/MM/yyyy", "dd/MM/yy", "MM/dd/yy", "MM/dd/yyyy", "yyyy/MM/dd", "yy/MM/dd", "dd.MM.yyyy", "dd.MM.yy", "dd-MM-yyyy", "dd-MM-yy"} },
                }
            },
            {
                "Experimental", new List<OptionsItemBase>
                {
                    new OptionsCheckbox() { value = false, uniqueName = "ForceRandomEvents" },
                    new OptionsCheckbox() { value = true, uniqueName = "FixInactiveBuildings" },
                    new OptionsCheckbox() { value = false, uniqueName = "ClearEvents" },
                    new OptionsCheckbox() { value = false, uniqueName = "EditEvents" },
                    new OptionsSpace() { spacing = 20 },
                    new OptionsCheckbox() { value = false, uniqueName = "PrintMonuments" },
                    new OptionsCheckbox() { value = false, uniqueName = "ForceXMLEnabled" }
                }
            }
        };

        public static void SetUpOptions(UIHelperBase helper)
        {
            UIHelper actualHelper = helper as UIHelper;
            UIComponent container = actualHelper.self as UIComponent;

            UITabstrip tabStrip = container.AddUIComponent<UITabstrip>();
            tabStrip.relativePosition = new Vector3(0, 0);
            tabStrip.size = new Vector2(container.width - 20, 40);

            UITabContainer tabContainer = container.AddUIComponent<UITabContainer>();
            tabContainer.relativePosition = new Vector3(0, 40);
            tabContainer.size = new Vector2(container.width - 20, container.height - tabStrip.height - 20);
            tabStrip.tabPages = tabContainer;

            int currentIndex = 0;
            foreach (KeyValuePair<string, List<OptionsItemBase>> optionGroup in allOptions)
            {
                UIButton settingsButton = AddOptionTab(tabStrip, optionGroup.Key);
                settingsButton.textPadding = new RectOffset(10, 10, 10, 10);
                settingsButton.autoSize = true;
                settingsButton.tooltip = optionGroup.Key;
                tabStrip.selectedIndex = currentIndex;
                TranslateTab(settingsButton, optionGroup.Key);

                CimTools.CimToolsHandler.CimToolBase.Translation.OnLanguageChanged += delegate (string languageIdentifier)
                {
                    TranslateTab(settingsButton, optionGroup.Key);
                };

                UIPanel currentPanel = tabStrip.tabContainer.components[currentIndex++] as UIPanel;
                currentPanel.autoLayout = true;
                currentPanel.autoLayoutDirection = LayoutDirection.Vertical;
                currentPanel.autoLayoutPadding.top = 5;
                currentPanel.autoLayoutPadding.left = 10;
                currentPanel.autoLayoutPadding.right = 10;

                UIHelper panelHelper = new UIHelper(currentPanel);

                CimTools.CimToolsHandler.CimToolBase.ModPanelOptions.CreateOptions(panelHelper, optionGroup.Value, optionGroup.Key, optionGroup.Key);
            }

            loadSettingsFromSaveFile();

            CimTools.CimToolsHandler.CimToolBase.ModPanelOptions.OnOptionPanelSaved += new OptionPanelSavedEventHandler(loadSettingsFromSaveFile);
        }

        private static UIButton AddOptionTab(UITabstrip tabStrip, string caption)
        {
            UIButton tabButton = tabStrip.AddTab(caption);

            tabButton.normalBgSprite = "SubBarButtonBase";
            tabButton.disabledBgSprite = "SubBarButtonBaseDisabled";
            tabButton.focusedBgSprite = "SubBarButtonBaseFocused";
            tabButton.hoveredBgSprite = "SubBarButtonBaseHovered";
            tabButton.pressedBgSprite = "SubBarButtonBasePressed";

            tabButton.textPadding = new RectOffset(10, 10, 10, 10);
            tabButton.autoSize = true;
            tabButton.tooltip = caption;

            return tabButton;
        }

        private static void TranslateTab(UIButton tab, string translationKey)
        {
            if (CimTools.CimToolsHandler.CimToolBase.Translation.HasTranslation("OptionGroup_" + translationKey))
            {
                if (tab != null && translationKey != null && translationKey != "")
                {
                    tab.text = CimTools.CimToolsHandler.CimToolBase.Translation.GetTranslation("OptionGroup_" + translationKey);
                }
                else
                {
                    CimTools.CimToolsHandler.CimToolBase.DetailedLogger.LogWarning("Couldn't translate tab because important bits were null");
                }
            }
        }

        private static void loadSettingsFromSaveFile()
        {
            CimTools.CimToolsHandler.CimToolBase.NamedLogger.Log("Rush Hour: Safely loading saved data.");

            bool legacy = false;

            if(!CimTools.CimToolsHandler.CimToolBase.ModPanelOptions.LoadOptions())
            {
                CimTools.CimToolsHandler.CimToolBase.NamedLogger.Log("Rush Hour: Loading data from legacy XML file.");
                CimToolsRushHour.Legacy.File.ExportOptionBase.OptionError error = CimTools.CimToolsHandler.LegacyCimToolBase.XMLFileOptions.Load();
                legacy = error == CimToolsRushHour.Legacy.File.ExportOptionBase.OptionError.NoError;

                if(legacy == false)
                {
                    CimTools.CimToolsHandler.CimToolBase.NamedLogger.LogError("Couldn't load up legacy data. " + error.ToString());
                }
            }
            else
            {
                CimTools.CimToolsHandler.CimToolBase.NamedLogger.Log("Rush Hour: Loading data from normal XML file.");
            }

            safelyGetValue("RandomEvents", ref Experiments.ExperimentsToggle.EnableRandomEvents, legacy);
            safelyGetValue("TeamColourOnBar", ref Experiments.ExperimentsToggle.TeamColourOnBar, false);
            safelyGetValue("DisableGameEvents", ref Experiments.ExperimentsToggle.DisableIngameEvents, false);
            safelyGetValue("AvailableInScenarios", ref Experiments.ExperimentsToggle.EnableInScenarios, false);

            safelyGetValue("ForceRandomEvents", ref Experiments.ExperimentsToggle.ForceEventToHappen, legacy);
            safelyGetValue("UseImprovedCommercial1", ref Experiments.ExperimentsToggle.ImprovedDemand, legacy);
            safelyGetValue("UseImprovedResidential", ref Experiments.ExperimentsToggle.ImprovedResidentialDemand, legacy);
            safelyGetValue("GhostMode", ref Experiments.ExperimentsToggle.GhostMode, legacy);
            safelyGetValue("Weekends1", ref Experiments.ExperimentsToggle.EnableWeekends, legacy);
            safelyGetValue("BetterParking", ref Experiments.ExperimentsToggle.ImprovedParkingAI, legacy);
            safelyGetValue("ParkingSearchRadius", ref Experiments.ExperimentsToggle.ParkingSearchRadius, legacy);
            safelyGetValue("LunchRush", ref Experiments.ExperimentsToggle.SimulateLunchTimeRushHour, legacy);
            safelyGetValue("SearchLocally", ref Experiments.ExperimentsToggle.AllowLocalBuildingSearch, false);
            safelyGetValue("LocalSearchChance", ref Experiments.ExperimentsToggle.LocalBuildingPercentage, false);

            safelyGetValue("TwentyFourHourClock", ref Experiments.ExperimentsToggle.NormalClock, legacy);
            safelyGetValue("SlowTimeProgression", ref Experiments.ExperimentsToggle.SlowTimeProgression, legacy);
            safelyGetValue("SlowTimeProgressionSpeed", ref Experiments.ExperimentsToggle.TimeMultiplier, legacy);
            safelyGetValue("SlowTimeProgressionSpeedNight", ref Experiments.ExperimentsToggle.TimeMultiplierNight, legacy);
            safelyGetValue("SunriseHour", ref SimulationManager.SUNRISE_HOUR, legacy);
            safelyGetValue("SunsetHour", ref SimulationManager.SUNSET_HOUR, legacy);
            safelyGetValue("DateFormat", ref Experiments.ExperimentsToggle.DateFormat, legacy);

            safelyGetValue("SchoolStartTime2", ref Chances.m_startSchoolHour, legacy);
            safelyGetValue("SchoolStartTimeVariance2", ref Chances.m_minSchoolHour, legacy);
            safelyGetValue("SchoolEndTime2", ref Chances.m_endSchoolHour, legacy);
            safelyGetValue("SchoolEndTimeVariance2", ref Chances.m_maxSchoolHour, legacy);
            safelyGetValue("SchoolDurationMinimum2", ref Chances.m_minSchoolDuration, legacy);

            safelyGetValue("WorkStartTime2", ref Chances.m_startWorkHour, legacy);
            safelyGetValue("WorkStartTimeVariance2", ref Chances.m_minWorkHour, legacy);
            safelyGetValue("WorkEndTime2", ref Chances.m_endWorkHour, legacy);
            safelyGetValue("WorkEndTimeVariance2", ref Chances.m_maxWorkHour, legacy);
            safelyGetValue("WorkDurationMinimum2", ref Chances.m_minWorkDuration, legacy);

            safelyGetValue("PrintMonuments", ref Experiments.ExperimentsToggle.PrintAllMonuments, legacy);
            safelyGetValue("ForceXMLEnabled", ref Experiments.ExperimentsToggle.AllowForcedXMLEvents, legacy);
            safelyGetValue("FixInactiveBuildings", ref Experiments.ExperimentsToggle.AllowActiveCommercialFix, legacy);

            string language = "English";
            safelyGetValue("Language", ref language, legacy);

            List<string> validLanguages = CimTools.CimToolsHandler.CimToolBase.Translation.GetLanguageIDsFromName(language);

            if (validLanguages.Count > 0)
            {
                CimTools.CimToolsHandler.CimToolBase.Translation.TranslateTo(validLanguages[0]);

                if (validLanguages.Count > 1)
                {
                    CimTools.CimToolsHandler.CimToolBase.DetailedLogger.LogWarning("Language " + language + " has more than one unique ID associated with it. Picked the first one.");
                }
            }
            else
            {
                CimTools.CimToolsHandler.CimToolBase.DetailedLogger.LogError("Could not switch to language " + language + ", as there are no valid languages with that name!");
            }

            bool loadEventEditor = false;
            safelyGetValue("EditEvents", ref loadEventEditor, legacy);

            if(loadEventEditor)
            {
                try
                {
                    string modPath = CimTools.CimToolsHandler.CimToolBase.Path.GetModPath();

                    if(modPath != null && modPath != "")
                    {
                        Process.Start(modPath + Path.DirectorySeparatorChar + "EventEditor.exe");
                        CimTools.CimToolsHandler.CimToolBase.ModPanelOptions.SetOptionValue("EditEvents", false);
                    }
                    else
                    {
                        UnityEngine.Debug.LogError("Rush Hour: Couldn't find the event editor!");
                    }
                }
                catch
                {
                    UnityEngine.Debug.LogError("Rush Hour: Couldn't load the event editor!");
                }
            }

            CimTools.CimToolsHandler.CimToolBase.Translation.RefreshLanguages();
        }

        /// <summary>
        /// <para>Call [[CimToolsRushHour.CimToolsHandler.CimToolBase.XMLFileOptions.Data.GetValue]], reporting any errors to [[DebugOutputPanel]].</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns>Whether the value was retrieved</returns>
        private static bool safelyGetValue<T>(string name, ref T value, bool legacy)
        {
            bool success = false;

            if (legacy)
            {
                success = CimTools.CimToolsHandler.LegacyCimToolBase.XMLFileOptions.Data.GetValue(name, ref value, "IngameOptions", true) == CimToolsRushHour.Legacy.File.ExportOptionBase.OptionError.NoError;
                CimTools.CimToolsHandler.CimToolBase.ModPanelOptions.SetOptionValue(name, value);
                CimTools.CimToolsHandler.CimToolBase.ModPanelOptions.SaveOptions();
            }
            else
            {
                success = CimTools.CimToolsHandler.CimToolBase.ModPanelOptions.GetOptionValue(name, ref value);
            }

            if (!success)
            {
                CimTools.CimToolsHandler.CimToolBase.DetailedLogger.LogError(string.Format("An error occurred trying to fetch '{0}'.", name));
            }
            else
            {
                CimTools.CimToolsHandler.CimToolBase.DetailedLogger.Log("Option \"" + name + "\" is " + value);
            }

            return success;
        }
    }
}
