namespace RushHour.Experiments
{
    public static class ExperimentsToggle
    {
        /// <summary>
        /// Set this to true to enable the experimental deathcare, which takes into
        /// consideration more realistic behaviour. Hearses pick up from hospitals,
        /// and dead people are transported to hospitals by ambulances. Non functional.
        /// </summary>
        public static bool ImprovedDeathcare = false;     
        
        /// <summary>
        /// Prints all monuments in your city to the console.
        /// </summary>
        public static bool PrintAllMonuments = false;

        /// <summary>
        /// Forces an event to happen as soon as the last one is removed from the
        /// event list. Will queue up another a bit after the last one has ended.
        /// </summary>
        public static bool ForceEventToHappen = false;

        /// <summary>
        /// Improved commercial and industrial demand for Rush Hour. Better than the normal behaviour
        /// hopefully.
        /// </summary>
        public static bool ImprovedDemand = false;

        /// <summary>
        /// Improved residential demand.
        /// </summary>
        public static bool ImprovedResidentialDemand = false;

        /// <summary>
        /// Enable random events to be initialised by the city.
        /// </summary>
        public static bool EnableRandomEvents = true;

        /// <summary>
        /// Enables weekends, where Cims don't go to work.
        /// </summary>
        public static bool EnableWeekends = true;

        /// <summary>
        /// Redirects reverted code. Potentially crashes, so this is here for experimentation
        /// </summary>
        public static bool RevertRedirects = true;

        /// <summary>
        /// Slows time down 4x so rush hour can happen properly
        /// </summary>
        public static bool SlowTimeProgression = false;

        /// <summary>
        /// Uses the new XML events
        /// </summary>
        public static bool UseXMLEvents = true;

        /// <summary>
        /// Improves Cims parking behaviour
        /// </summary>
        public static bool ImprovedParkingAI = true;

        /// <summary>
        /// Allows people to use the Force parameter in their XML event files
        /// </summary>
        public static bool AllowForcedXMLEvents = false;

        /// <summary>
        /// 24 hour clock or 12 hour clock
        /// </summary>
        public static bool NormalClock = true;

        /// <summary>
        /// Whether people should go out at lunch for food
        /// </summary>
        public static bool SimulateLunchTimeRushHour = true;

        /// <summary>
        /// Enable the fix for inactive commercial buildings
        /// </summary>
        public static bool AllowActiveCommercialFix = true;

        /// <summary>
        /// Shows team colours on the date/time bar
        /// </summary>
        public static bool TeamColourOnBar = false;

        /// <summary>
        /// Shows incompatible mods on startup
        /// </summary>
        public static bool ShowIncompatibleMods = true;

        /// <summary>
        /// Display date format
        /// </summary>
        public static string DateFormat = "dd/MM/yyyy";

        /// <summary>
        /// The time scale multiplier
        /// </summary>
        public static string TimeMultiplier = "0.25";

        /// <summary>
        /// The search radius for parking
        /// </summary>
        public static float ParkingSearchRadius = 100f;

        /// <summary>
        /// The time that the day starts
        /// </summary>
        public static float DayTimeStart = 5f;

        /// <summary>
        /// The time that the day ends
        /// </summary>
        public static float DayTimeEnd = 22f;

        /// <summary>
        /// The maximum amount of events to allow to be scheduled at once
        /// </summary>
        public static int MaxConcurrentEvents = 1;
    }
}
