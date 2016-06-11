using CimTools.v2.Utilities;
using ColossalFramework.UI;
using ICities;
using RushHour.Places;
using RushHour.UI;
using System.Collections.Generic;
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
                    new OptionsCheckbox() { value = true, uniqueName = "Weekends1", enabled = true, translationIdentifier = "Weekends" }, //Weekends1 because I needed to override the old value. Silly me
                    new OptionsCheckbox() { value = true, uniqueName = "LunchRush" },
                    new OptionsCheckbox() { value = true, uniqueName = "CityTimeDateBar" },
                    new OptionsCheckbox() { value = true, uniqueName = "BetterParking" },
                    new OptionsSlider() { value = 100f, max = 500f, min = 16f, step = 1f, uniqueName = "ParkingSearchRadius" },
                    new OptionsSpace() { spacing = 20 },
                    new OptionsCheckbox() { value = true, uniqueName = "UseImprovedCommercial1", translationIdentifier = "UseImprovedCommercial" },
                    new OptionsCheckbox() { value = true, uniqueName = "UseImprovedResidential" },
                    new OptionsCheckbox() { value = false, uniqueName = "GhostMode", enabled = false },
                    new OptionsCheckbox() { value = true, uniqueName = "TwentyFourHourClock" },
                    new OptionsDropdown() { value = "dd/MM/yyyy", uniqueName = "DateFormat", options = new string[]{ "dd/MM/yyyy", "MM/dd/yyyy", "yyyy/MM/dd" } },
                    new OptionsDropdown() { value = "English", uniqueName = "Language", options = CimToolsHandler.CimToolsHandler.CimToolBase.Translation.AvailableLanguagesReadable().ToArray() }
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
                "Events", new List<OptionsItemBase>
                {
                    new OptionsCheckbox() { value = true, uniqueName = "RandomEvents" }
                    /*new OptionsSlider() { value = 1f, max = 5f, min = 1f, step = 1f, uniqueName = "MaximumEventsAtOnce" },
                    new TimeOfDayVarianceSlider() { value = 24f, max = 144f, min = 0f, step = 1f, uniqueName = "MinHoursBetweenEvents" },
                    new TimeOfDayVarianceSlider() { value = 48f, max = 144f, min = 24f, step = 1f, uniqueName = "MaxHoursBetweenEvents" }*/
                }
            },
            {
                "Experimental", new List<OptionsItemBase>
                {
                    new OptionsCheckbox() { value = true, uniqueName = "SlowTimeProgression" },
                    new OptionsDropdown() { value = "0.25", uniqueName = "SlowTimeProgressionSpeed", options = new string[]{ "0.125", "0.25", "0.33", "0.5", "2", "4", "8", "16" } },
                    new OptionsCheckbox() { value = false, uniqueName = "ForceRandomEvents" },
                    new OptionsCheckbox() { value = true, uniqueName = "FixInactiveBuildings" },
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

            //Find the tab button in the KeyMappingPanel, so we can copy it
            UIButton tabTemplate = GameObject.Find("KeyMappingTabStrip").GetComponentInChildren<UIButton>();

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
                UIButton settingsButton = tabStrip.AddTab(optionGroup.Key, tabTemplate, true);
                tabStrip.selectedIndex = currentIndex;
                TranslateTab(settingsButton, optionGroup.Key);

                CimToolsHandler.CimToolsHandler.CimToolBase.Translation.OnLanguageChanged += delegate (string languageIdentifier)
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

                CimToolsHandler.CimToolsHandler.CimToolBase.ModPanelOptions.CreateOptions(panelHelper, optionGroup.Value, optionGroup.Key, optionGroup.Key);
            }

            loadSettingsFromSaveFile();

            CimToolsHandler.CimToolsHandler.CimToolBase.ModPanelOptions.OnOptionPanelSaved += new OptionPanelSavedEventHandler(loadSettingsFromSaveFile);
        }

        private static void TranslateTab(UIButton tab, string translationKey)
        {
            if (CimToolsHandler.CimToolsHandler.CimToolBase.Translation.HasTranslation("OptionGroup_" + translationKey))
            {
                if (tab != null && translationKey != null && translationKey != "")
                {
                    tab.text = CimToolsHandler.CimToolsHandler.CimToolBase.Translation.GetTranslation("OptionGroup_" + translationKey);
                }
                else
                {
                    CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.LogWarning("Couldn't translate tab because important bits were null");
                }
            }
        }

        private static void loadSettingsFromSaveFile()
        {
            CimToolsHandler.CimToolsHandler.CimToolBase.NamedLogger.Log("Rush Hour: Safely loading saved data.");

            bool legacy = false;

            if(!CimToolsHandler.CimToolsHandler.CimToolBase.ModPanelOptions.LoadOptions())
            {
                CimToolsHandler.CimToolsHandler.CimToolBase.NamedLogger.Log("Rush Hour: Loading data from legacy XML file.");
                CimTools.Legacy.File.ExportOptionBase.OptionError error = CimToolsHandler.CimToolsHandler.LegacyCimToolBase.XMLFileOptions.Load();
                legacy = error == CimTools.Legacy.File.ExportOptionBase.OptionError.NoError;

                if(legacy == false)
                {
                    CimToolsHandler.CimToolsHandler.CimToolBase.NamedLogger.LogError("Couldn't load up legacy data. " + error.ToString());
                }
            }
            else
            {
                CimToolsHandler.CimToolsHandler.CimToolBase.NamedLogger.Log("Rush Hour: Loading data from normal XML file.");
            }

            safelyGetValue("RandomEvents", ref Experiments.ExperimentsToggle.EnableRandomEvents, legacy);
            safelyGetValue("ForceRandomEvents", ref Experiments.ExperimentsToggle.ForceEventToHappen, legacy);
            safelyGetValue("UseImprovedCommercial1", ref Experiments.ExperimentsToggle.ImprovedDemand, legacy);
            safelyGetValue("UseImprovedResidential", ref Experiments.ExperimentsToggle.ImprovedResidentialDemand, legacy);
            safelyGetValue("Weekends1", ref Experiments.ExperimentsToggle.EnableWeekends, legacy);
            safelyGetValue("SlowTimeProgression", ref Experiments.ExperimentsToggle.SlowTimeProgression, legacy);
            safelyGetValue("SlowTimeProgressionSpeed", ref Experiments.ExperimentsToggle.TimeMultiplier, legacy);
            safelyGetValue("BetterParking", ref Experiments.ExperimentsToggle.ImprovedParkingAI, legacy);
            safelyGetValue("ParkingSearchRadius", ref Experiments.ExperimentsToggle.ParkingSearchRadius, legacy);
            safelyGetValue("DateFormat", ref Experiments.ExperimentsToggle.DateFormat, legacy);
            safelyGetValue("TwentyFourHourClock", ref Experiments.ExperimentsToggle.NormalClock, legacy);
            safelyGetValue("LunchRush", ref Experiments.ExperimentsToggle.SimulateLunchTimeRushHour, legacy);

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

            List<string> validLanguages = CimToolsHandler.CimToolsHandler.CimToolBase.Translation.GetLanguageIDsFromName(language);

            if (validLanguages.Count > 0)
            {
                CimToolsHandler.CimToolsHandler.CimToolBase.Translation.TranslateTo(validLanguages[0]);

                if (validLanguages.Count > 1)
                {
                    CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.LogWarning("Language " + language + " has more than one unique ID associated with it. Picked the first one.");
                }
            }
            else
            {
                CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.LogError("Could not switch to language " + language + ", as there are no valid languages with that name!");
            }

            CimToolsHandler.CimToolsHandler.CimToolBase.Translation.RefreshLanguages();
        }

        /// <summary>
        /// <para>Call [[CimTools.CimToolsHandler.CimToolBase.XMLFileOptions.Data.GetValue]], reporting any errors to [[DebugOutputPanel]].</para>
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
                success = CimToolsHandler.CimToolsHandler.LegacyCimToolBase.XMLFileOptions.Data.GetValue(name, ref value, "IngameOptions", true) == CimTools.Legacy.File.ExportOptionBase.OptionError.NoError;
                CimToolsHandler.CimToolsHandler.CimToolBase.ModPanelOptions.SetOptionValue(name, value);
                CimToolsHandler.CimToolsHandler.CimToolBase.ModPanelOptions.SaveOptions();
            }
            else
            {
                success = CimToolsHandler.CimToolsHandler.CimToolBase.ModPanelOptions.GetOptionValue(name, ref value);
            }

            if (!success)
            {
                CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.LogError(string.Format("An error occurred trying to fetch '{0}'.", name));
            }
            else
            {
                CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.Log("Option \"" + name + "\" is " + value);
            }

            return success;
        }
    }
}
