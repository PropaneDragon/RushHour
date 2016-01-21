using CitiesSkylinesDetour;
using ICities;
using RushHour.BuildingHandlers;
using RushHour.InternalMethods;
using RushHour.ResidentHandlers;
using RushHour.ResidentHandlers;
using RushHour.TouristHandlers;
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

            patchSuccess = MethodHook.PatchFromCities(typeof(ResidentAI), typeof(NewResidentAI), "UpdateLocation") && patchSuccess;
            patchSuccess = MethodHook.PatchFromCities(typeof(CommercialBuildingAI), typeof(NewCommercialBuildingAI), "SimulationStepActive") && patchSuccess;
            patchSuccess = MethodHook.PatchFromCities(typeof(PrivateBuildingAI), typeof(NewPrivateBuildingAI), "HandleWorkers") && patchSuccess;
            patchSuccess = MethodHook.PatchFromCities(typeof(TouristAI), typeof(NewTouristAI), "UpdateLocation") && patchSuccess;

            patchSuccess = MethodHook.PatchIntoCities(typeof(NewResidentAI), typeof(ResidentAI), "FindHospital") && patchSuccess;
            patchSuccess = MethodHook.PatchIntoCities(typeof(NewResidentAI), typeof(ResidentAI), "DoRandomMove") && patchSuccess;
            patchSuccess = MethodHook.PatchIntoCities(typeof(NewResidentAI), typeof(ResidentAI), "StartMoving") && patchSuccess;
            patchSuccess = MethodHook.PatchIntoCities(typeof(NewResidentAI), typeof(ResidentAI), "FindVisitPlace") && patchSuccess;
            patchSuccess = MethodHook.PatchIntoCities(typeof(NewResidentAI), typeof(ResidentAI), "GetEntertainmentReason") && patchSuccess;
            patchSuccess = MethodHook.PatchIntoCities(typeof(NewResidentAI), typeof(ResidentAI), "GetShoppingReason") && patchSuccess;

            patchSuccess = MethodHook.PatchIntoCities(typeof(NewCommercialBuildingAI), typeof(CommercialBuildingAI), "HandleWorkers") && patchSuccess;
            patchSuccess = MethodHook.PatchIntoCities(typeof(NewCommercialBuildingAI), typeof(CommercialBuildingAI), "MaxIncomingLoadSize") && patchSuccess;
            patchSuccess = MethodHook.PatchIntoCities(typeof(NewCommercialBuildingAI), typeof(CommercialBuildingAI), "HandleCommonConsumption") && patchSuccess;
            patchSuccess = MethodHook.PatchIntoCities(typeof(NewCommercialBuildingAI), typeof(CommercialBuildingAI), "GetVisitBehaviour") && patchSuccess;
            patchSuccess = MethodHook.PatchIntoCities(typeof(NewCommercialBuildingAI), typeof(CommercialBuildingAI), "GetAccumulation") && patchSuccess;
            patchSuccess = MethodHook.PatchIntoCities(typeof(NewCommercialBuildingAI), typeof(CommercialBuildingAI), "HandleDead") && patchSuccess;
            patchSuccess = MethodHook.PatchIntoCities(typeof(NewCommercialBuildingAI), typeof(CommercialBuildingAI), "HandleCrime") && patchSuccess;
            patchSuccess = MethodHook.PatchIntoCities(typeof(NewCommercialBuildingAI), typeof(CommercialBuildingAI), "CalculateGuestVehicles") && patchSuccess;
            patchSuccess = MethodHook.PatchIntoCities(typeof(NewCommercialBuildingAI), typeof(CommercialBuildingAI), "CheckBuildingLevel") && patchSuccess;
            patchSuccess = MethodHook.PatchIntoCities(typeof(NewCommercialBuildingAI), typeof(CommercialBuildingAI), "HandleFire") && patchSuccess;
            patchSuccess = MethodHook.PatchIntoCities(typeof(NewCommercialBuildingAI), typeof(CommercialBuildingAI), "GetIncomingTransferReason") && patchSuccess;
            patchSuccess = MethodHook.PatchIntoCities(typeof(NewCommercialBuildingAI), typeof(CommercialBuildingAI), "GetOutgoingTransferReason") && patchSuccess;

            patchSuccess = MethodHook.PatchIntoCities(typeof(NewPrivateBuildingAI), typeof(PrivateBuildingAI), "SimulationStepActive") && patchSuccess;
            patchSuccess = MethodHook.PatchIntoCities(typeof(NewPrivateBuildingAI), typeof(PrivateBuildingAI), "GetWorkBehaviour") && patchSuccess;
            patchSuccess = MethodHook.PatchIntoCities(typeof(NewPrivateBuildingAI), typeof(PrivateBuildingAI), "HandleWorkPlaces") && patchSuccess;

            patchSuccess = MethodHook.PatchIntoCities(typeof(NewTouristAI), typeof(TouristAI), "FindVisitPlace") && patchSuccess;
            patchSuccess = MethodHook.PatchIntoCities(typeof(NewTouristAI), typeof(TouristAI), "GetLeavingReason") && patchSuccess;
            patchSuccess = MethodHook.PatchIntoCities(typeof(NewTouristAI), typeof(TouristAI), "DoRandomMove") && patchSuccess;
            patchSuccess = MethodHook.PatchIntoCities(typeof(NewTouristAI), typeof(TouristAI), "GetShoppingReason") && patchSuccess;
            patchSuccess = MethodHook.PatchIntoCities(typeof(NewTouristAI), typeof(TouristAI), "AddTouristVisit") && patchSuccess;

            if (patchSuccess)
            {
                Debug.Log("Patched!");
            }
        }
    }
}
