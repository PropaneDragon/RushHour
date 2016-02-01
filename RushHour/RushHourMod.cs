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
                new OptionsCheckbox() { readableName = "Enable weekends", value = false, uniqueName = "Weekends", enabled = false },
                new OptionsCheckbox() { readableName = "Use modified date bar", value = true, uniqueName = "CityTimeDateBar" },
                new OptionsCheckbox() { readableName = "Ghost mode", value = false, uniqueName = "GhostMode", enabled = false },
                new OptionsCheckbox() { readableName = "EXPERIMENTAL: Force random events immediately", value = false, uniqueName = "ForceRandomEvents" },
                new OptionsCheckbox() { readableName = "EXPERIMENTAL: Use improved commercial demand", value = false, uniqueName = "UseImprovedCommercial" },
                new HoursSlider() { readableName = "School Start Time", value = Chances.m_startSchoolHour, min = 0f, max = 23.9f, step = .25f, uniqueName = "SchoolStartTime" },
                new HoursSlider() { readableName = "School Start Time Variance", value = Chances.m_minSchoolHour, min = 0f, max = 23.9f, step = .25f, uniqueName = "SchoolStartTimeVariance" },
                new HoursSlider() { readableName = "School End Time", value = Chances.m_endSchoolHour, min = 0f, max = 23.9f, step = .25f, uniqueName = "SchoolEndTime" },
                new HoursSlider() { readableName = "School End Time Variance", value = Chances.m_maxSchoolHour, min = 0f, max = 23.9f, step = .25f, uniqueName = "SchoolEndTimeVariance" }
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
            safelyGetValue("SchoolStartTime", ref Chances.m_startSchoolHour, "IngameOptions");
            safelyGetValue("SchoolStartTimeVariance", ref Chances.m_minSchoolHour, "IngameOptions");
            safelyGetValue("SchoolEndTime", ref Chances.m_endSchoolHour, "IngameOptions");
            safelyGetValue("SchoolEndTimeVariance", ref Chances.m_maxSchoolHour, "IngameOptions");
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
            return err;
        }
    }
}
