using CitiesSkylinesDetour;
using ICities;
using RushHour.BuildingHandlers;
using RushHour.CitizenHandlers;
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

            patchSuccess = Patch(typeof(ResidentAI), typeof(NewResidentAI), "UpdateLocation") && patchSuccess;
            patchSuccess = Patch(typeof(CommercialBuildingAI), typeof(NewCommonBuildingAI), "SimulationStepActive") && patchSuccess;

            if (patchSuccess)
            {
                Debug.Log("Patched!");
            }
        }

        private bool Patch(Type getMethodFrom, Type putMethodIn, string methodName, Type[] types)
        {
            bool succeeded = false;

            if (getMethodFrom != null && putMethodIn != null)
            {
                MethodInfo citiesMethod = getMethodFrom.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, types, null);
                MethodInfo ourMethod = putMethodIn.GetMethod(methodName);

                succeeded = Patch(citiesMethod, ourMethod, methodName);
            }
            else
            {
                Debug.LogError("Couldn't patch into the methods for " + methodName + " as the class is null!");
            }

            return succeeded;
        }

        private bool Patch(Type getMethodFrom, Type putMethodIn, string methodName)
        {
            bool succeeded = false;

            if (getMethodFrom != null && putMethodIn != null)
            {
                MethodInfo citiesMethod = getMethodFrom.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo ourMethod = putMethodIn.GetMethod(methodName);

                succeeded = Patch(citiesMethod, ourMethod, methodName);
            }
            else
            {
                Debug.LogError("Couldn't patch into the methods for " + methodName + " as the class is null!");
            }

            return succeeded;
        }

        private bool Patch(MethodInfo methodFrom, MethodInfo methodTo, string methodName)
        {
            bool succeeded = false;

            if (methodFrom != null && methodTo != null)
            {
                RedirectionHelper.RedirectCalls(methodFrom, methodTo);
                Debug.Log("Rush Hour: Patched up " + methodName);

                succeeded = true;
            }
            else
            {
                Debug.LogError("Couldn't patch into the methods for " + methodName + " as one or both of the methods don't exist!");
            }

            return succeeded;
        }
    }
}
