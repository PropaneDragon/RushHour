using CitiesSkylinesDetour;
using ICities;
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

            patchSuccess = patchSuccess && Patch(typeof(CitizenManager), typeof(NewCitizenManager), "SimulationStepImpl");

            if (patchSuccess)
            {
                Debug.Log("Patched!");
            }
        }

        private bool Patch(Type getMethodFrom, Type putMethodIn, string methodName)
        {
            bool succeeded = false;

            if (getMethodFrom != null && putMethodIn != null)
            {
                MethodInfo citiesMethod = getMethodFrom.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo ourMethod = putMethodIn.GetMethod(methodName);

                if (citiesMethod != null && ourMethod != null)
                {
                    RedirectionHelper.RedirectCalls(citiesMethod, ourMethod);
                    Debug.Log("Rush Hour: Patched up " + methodName);

                    succeeded = true;
                }
                else
                {
                    Debug.LogError("Couldn't patch into the methods for " + methodName + " as one or both of the methods don't exist!");
                }
            }
            else
            {
                Debug.LogError("Couldn't patch into the methods for " + methodName + " as the class is null!");
            }

            return succeeded;
        }
    }
}
