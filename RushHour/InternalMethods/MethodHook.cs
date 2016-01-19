using CitiesSkylinesDetour;
using System;
using System.Reflection;
using UnityEngine;

namespace RushHour.InternalMethods
{
    public static class MethodHook
    {
        public static InternalClassMethod<Returns> GetClassMethod<Returns>(object objectWithMethod, string methodName)
        {
            InternalClassMethod<Returns> returnMethod = null;
            MethodInfo foundMethod = objectWithMethod.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if(foundMethod != null)
            {
                returnMethod = new InternalClassMethod<Returns>(objectWithMethod, foundMethod);
            }
            else
            {
                Debug.LogError("Method Hook: Can't hook into method " + methodName + " inside the " + objectWithMethod.GetType().Name + " class!");
            }

            return returnMethod;
        }

        public static bool Patch(Type getMethodFrom, Type putMethodIn, string methodName, Type[] types)
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

        public static bool Patch(Type getMethodFrom, Type putMethodIn, string methodName)
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
