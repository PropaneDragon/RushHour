using ICities;
using RushHour.Options;

namespace RushHour
{
    public class RushHourMod : IUserMod
    {
        public string Name => "Rush Hour";
        public string Description => "Improves AI so citizens and tourists act more realistically.";

        public void OnSettingsUI(UIHelperBase helper)
        {
            OptionHandler.SetUpOptions(helper);
        }
    }
}
