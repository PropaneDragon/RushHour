using CitiesSkylinesDetour;
using ICities;
using RushHour.BuildingHandlers;
using RushHour.InternalMethods;
using RushHour.ResidentHandlers;
using RushHour.ResidentHandlers;
using System;
using System.Reflection;
using UnityEngine;

namespace RushHour
{
    public class RushHourMod : IUserMod
    {
        public string Name
        {
            get
            {
                Patch();
                return "Rush Hour";
            }
        }

        public string Description
        {
            get
            {
                return "Improves citizen AI so they act more realistically.";
            }
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            
        }

        private void Patch()
        {
            bool patchSuccess = true;

            Debug.Log("Patching up Rush Hour");

            patchSuccess = MethodHook.Patch(typeof(ResidentAI), typeof(NewResidentAI), "UpdateLocation") && patchSuccess;
            patchSuccess = MethodHook.Patch(typeof(CommercialBuildingAI), typeof(NewCommonBuildingAI), "SimulationStepActive") && patchSuccess;

            if (patchSuccess)
            {
                Debug.Log("Patched!");
            }
        }
    }
}
