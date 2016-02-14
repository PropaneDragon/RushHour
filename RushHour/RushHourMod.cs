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
                new OptionsCheckbox() { readableName = "Ghost mode", value = false, uniqueName = "GhostMode", enabled = false },

                new TimeOfDaySlider() { readableName = "Earliest school start time", value = Chances.m_minSchoolHour,uniqueName = "SchoolStartTimeVariance1" },
                new TimeOfDaySlider() { readableName = "Latest school start time", value = Chances.m_startSchoolHour, uniqueName = "SchoolStartTime1" },
                new TimeOfDaySlider() { readableName = "Earliest school end time", value = Chances.m_endSchoolHour, uniqueName = "SchoolEndTime1" },
                new TimeOfDaySlider() { readableName = "Latest end school time", value = Chances.m_maxSchoolHour, uniqueName = "SchoolEndTimeVariance1" },

                new TimeOfDaySlider() { readableName = "Earliest work start time", value = Chances.m_minWorkHour,uniqueName = "WorkStartTimeVariance1" },
                new TimeOfDaySlider() { readableName = "Work Start Time", value = Chances.m_startWorkHour, uniqueName = "WorkStartTime1" },
                new TimeOfDaySlider() { readableName = "Work End Time", value = Chances.m_endWorkHour, uniqueName = "WorkEndTime1" },
                new TimeOfDaySlider() { readableName = "Latest end work time", value = Chances.m_maxWorkHour, uniqueName = "WorkEndTimeVariance1" },
                
                new TimeOfDayVarianceSlider() { readableName = "Shortest School Duration", value = Chances.m_minSchoolDuration,uniqueName = "SchoolDurationMinimum1" },
                new TimeOfDayVarianceSlider() { readableName = "Shortest Work Duration", value = Chances.m_minWorkDuration, uniqueName = "WorkDurationMinimum1" },

                new OptionsCheckbox() { readableName = "EXPERIMENTAL: Better time progression", value = false, uniqueName = "SlowTimeProgression" },
                new OptionsCheckbox() { readableName = "EXPERIMENTAL: Force random events immediately after the last", value = false, uniqueName = "ForceRandomEvents" },
                new OptionsCheckbox() { readableName = "EXPERIMENTAL: Use improved commercial demand", value = false, uniqueName = "UseImprovedCommercial" },
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
            safelyGetValue("UseImprovedCommercial", ref Experiments.ExperimentsToggle.ImprovedCommercialDemand, "IngameOptions");
            safelyGetValue("Weekends1", ref Experiments.ExperimentsToggle.EnableWeekends, "IngameOptions");
            safelyGetValue("SlowTimeProgression", ref Experiments.ExperimentsToggle.SlowTimeProgression, "IngameOptions");

            safelyGetValue("SchoolStartTime1", ref Chances.m_startSchoolHour, "IngameOptions");
            safelyGetValue("SchoolStartTimeVariance1", ref Chances.m_minSchoolHour, "IngameOptions");
            safelyGetValue("SchoolEndTime1", ref Chances.m_endSchoolHour, "IngameOptions");
            safelyGetValue("SchoolEndTimeVariance1", ref Chances.m_maxSchoolHour, "IngameOptions");
            safelyGetValue("SchoolDurationMinimum1", ref Chances.m_minSchoolDuration, "IngameOptions");

            safelyGetValue("WorkStartTime1", ref Chances.m_startWorkHour, "IngameOptions");
            safelyGetValue("WorkStartTimeVariance1", ref Chances.m_minWorkHour, "IngameOptions");
            safelyGetValue("WorkEndTime1", ref Chances.m_endWorkHour, "IngameOptions");
            safelyGetValue("WorkEndTimeVariance1", ref Chances.m_maxWorkHour, "IngameOptions");
            safelyGetValue("WorkDurationMinimum1", ref Chances.m_minWorkDuration, "IngameOptions");

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
