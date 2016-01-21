using CitiesSkylinesDetour;
using System;
using System.Reflection;
using UnityEngine;

namespace RushHour.InternalMethods
{
    public static class MethodHook
    {
        /// <summary>
        /// Patches into a method from one class, and redirects to another.
        /// </summary>
        /// <param name="getMethodFrom">The class to get the method from.</param>
        /// <param name="putMethodIn">The class to redirect calls to.</param>
        /// <param name="methodName">The method name to redirect.</param>
        /// <param name="types">Specific types this method takes, if there are multiple of the same name.</param>
        /// <returns></returns>
        public static bool PatchFromCities(Type getMethodFrom, Type putMethodIn, string methodName, bool isStatic = false, Type[] types = null)
        {
            bool succeeded = false;

            if (getMethodFrom != null && putMethodIn != null)
            {
                MethodInfo citiesMethod = null;

                if(types != null)
                {
                    citiesMethod = getMethodFrom.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | (isStatic ? BindingFlags.Static : BindingFlags.Instance), null, types, null);
                }
                else
                {
                    citiesMethod = getMethodFrom.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | (isStatic ? BindingFlags.Static : BindingFlags.Instance));
                }

                MethodInfo ourMethod = putMethodIn.GetMethod(methodName);

                succeeded = Patch(citiesMethod, ourMethod, methodName);
            }
            else
            {
                Debug.LogError("Couldn't patch into the methods for " + methodName + " as the class is null!");
            }

            return succeeded;
        }

        public static bool PatchIntoCities(Type getMethodFrom, Type putMethodIn, string methodName, bool isStatic = false, Type[] types = null)
        {
            bool succeeded = false;

            if (getMethodFrom != null && putMethodIn != null)
            {
                MethodInfo ourMethod = getMethodFrom.GetMethod(methodName);

                MethodInfo citiesMethod = null;

                if (types != null)
                {
                    citiesMethod = putMethodIn.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | (isStatic ? BindingFlags.Static : BindingFlags.Instance), null, types, null);
                }
                else
                {
                    citiesMethod = putMethodIn.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | (isStatic ? BindingFlags.Static : BindingFlags.Instance));
                }

                succeeded = Patch(ourMethod, citiesMethod, methodName);
            }
            else
            {
                Debug.LogError("Couldn't patch into the methods for " + methodName + " as the class is null!");
            }

            return succeeded;
        }

        /// <summary>
        /// Patches into a method from one class, and redirects to another.
        /// </summary>
        /// <param name="methodFrom">The method to redirect.</param>
        /// <param name="methodTo">The method that takes the redirection.</param>
        /// <param name="methodName">The name of the method.</param>
        /// <returns></returns>
        public static bool Patch(MethodInfo methodFrom, MethodInfo methodTo, string methodName)
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
