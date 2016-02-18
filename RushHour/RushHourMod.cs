using CimTools.V1.Utilities;
using ICities;
using System.Collections.Generic;
using RushHour.Places;
using CimTools.V1.File;
using ColossalFramework.Plugins;
using RushHour.UI;

namespace RushHour
{
    public class RushHourMod : IUserMod
    {
        public string Name => "Rush Hour";

        public string Description => "Improves AI so citizens and tourists act more realistically.";

        public void OnSettingsUI(UIHelperBase helper)
        {
            List<OptionsItemBase> options = new List<OptionsItemBase>
            {
                new OptionsCheckbox() { readableName = "Enable random events", value = true, uniqueName = "RandomEvents" },
                new OptionsCheckbox() { readableName = "Enable weekends", value = true, uniqueName = "Weekends1", enabled = true }, //Weekends1 because I needed to override the old value. Silly me
                new OptionsCheckbox() { readableName = "Use modified date bar", value = true, uniqueName = "CityTimeDateBar" },
                new OptionsCheckbox() { readableName = "Improved commercial demand", value = true, uniqueName = "UseImprovedCommercial1" },
                new OptionsCheckbox() { readableName = "Ghost mode (coming soon)", value = false, uniqueName = "GhostMode", enabled = false },

                new TimeOfDaySlider() { readableName = "School earliest start time", value = Chances.m_minSchoolHour, min = 5f, max = 11f, uniqueName = "SchoolStartTimeVariance2" },
                new TimeOfDaySlider() { readableName = "School latest start time", value = Chances.m_startSchoolHour, min = 5f, max = 11f,  uniqueName = "SchoolStartTime2" },
                new TimeOfDaySlider() { readableName = "School earliest end time", value = Chances.m_endSchoolHour, min = 13f, max = 18f,  uniqueName = "SchoolEndTime2" },
                new TimeOfDaySlider() { readableName = "School latest end time", value = Chances.m_maxSchoolHour, min = 13f, max = 18f,  uniqueName = "SchoolEndTimeVariance2" },

                new TimeOfDaySlider() { readableName = "Work earliest start time", value = Chances.m_minWorkHour, min = 5f, max = 12f,  uniqueName = "WorkStartTimeVariance2" },
                new TimeOfDaySlider() { readableName = "Work latest start time", value = Chances.m_startWorkHour, min = 5f, max = 12f,  uniqueName = "WorkStartTime2" },
                new TimeOfDaySlider() { readableName = "Work earliest end time", value = Chances.m_endWorkHour, min = 14f, max = 18f,  uniqueName = "WorkEndTime2" },
                new TimeOfDaySlider() { readableName = "Work latest end time", value = Chances.m_maxWorkHour, min = 14f, max = 18f,  uniqueName = "WorkEndTimeVariance2" },

                new TimeOfDayVarianceSlider() { readableName = "Don't travel to school if under this many hours left", value = Chances.m_minSchoolDuration, min = 0.5f, max = 4f, uniqueName = "SchoolDurationMinimum2" },
                new TimeOfDayVarianceSlider() { readableName = "Don't travel to work if under this many hours left", value = Chances.m_minWorkDuration, min = 0.5f, max = 4f, uniqueName = "WorkDurationMinimum2" },

                new OptionsCheckbox() { readableName = "EXPERIMENTAL: Better time progression (like Time Warp)", value = true, uniqueName = "SlowTimeProgression" },
                new OptionsCheckbox() { readableName = "EXPERIMENTAL: No cooldown timer on random events", value = false, uniqueName = "ForceRandomEvents" },
                new OptionsCheckbox() { readableName = "DEVELOPER: Print all monuments in your city to the console", value = false, uniqueName = "PrintMonuments" }
            };

            loadSettingsFromSaveFile();

            CimTools.CimToolsHandler.CimToolBase.ModOptions.CreateOptions(helper, options, "Rush Hour Options");
            CimTools.CimToolsHandler.CimToolBase.ModOptions.OnOptionPanelSaved += new OptionPanelSaved(loadSettingsFromSaveFile);
        }

        private void loadSettingsFromSaveFile()
        {
            safelyGetValue("RandomEvents", ref Experiments.ExperimentsToggle.EnableRandomEvents, "IngameOptions");
            safelyGetValue("ForceRandomEvents", ref Experiments.ExperimentsToggle.ForceEventToHappen, "IngameOptions");
            safelyGetValue("UseImprovedCommercial1", ref Experiments.ExperimentsToggle.ImprovedCommercialDemand, "IngameOptions");
            safelyGetValue("Weekends1", ref Experiments.ExperimentsToggle.EnableWeekends, "IngameOptions");
            safelyGetValue("SlowTimeProgression", ref Experiments.ExperimentsToggle.SlowTimeProgression, "IngameOptions");

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
                DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, string.Format("An error occurred trying to fetch '{0}': {1}.", name, errType));
            }
            else
            {
                CimTools.CimToolsHandler.CimToolBase.DetailedLogger.Log("Option \"" + name + "\" is " + value);
            }

            return err;
        }
    }
}
