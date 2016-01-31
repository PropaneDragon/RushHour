using CimTools.V1.Utilities;
using ICities;
using System.Collections.Generic;

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
                new OptionsCheckbox() { readableName = "EXPERIMENTAL: Use improved commercial demand", value = false, uniqueName = "UseImprovedCommercial" }
            };

            CimTools.CimToolsHandler.CimToolBase.ModOptions.CreateOptions(helper, options, "Rush Hour Options");
            CimTools.CimToolsHandler.CimToolBase.ModOptions.OnOptionPanelSaved += new OptionPanelSaved(delegate
            {
                CimTools.CimToolsHandler.CimToolBase.XMLFileOptions.Data.GetValue("RandomEvents", ref Experiments.ExperimentsToggle.EnableRandomEvents, "IngameOptions");
                CimTools.CimToolsHandler.CimToolBase.XMLFileOptions.Data.GetValue("ForceRandomEvents", ref Experiments.ExperimentsToggle.ForceEventToHappen, "IngameOptions");
                CimTools.CimToolsHandler.CimToolBase.XMLFileOptions.Data.GetValue("UseImprovedCommercial", ref Experiments.ExperimentsToggle.ImprovedCommercialDemand, "IngameOptions");
            });
        }
    }
}
