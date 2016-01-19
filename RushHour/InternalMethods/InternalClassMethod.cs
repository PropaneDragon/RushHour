using System.Reflection;
using UnityEngine;

namespace RushHour.InternalMethods
{
    public class InternalClassMethod<T>
    {
        private MethodInfo _methodInfo = null;
        private object _methodObject = null;

        public InternalClassMethod(object objectToCallForMethod, MethodInfo methodToCall)
        {
            _methodInfo = methodToCall;
            _methodObject = objectToCallForMethod;
        }

        public T Invoke(params object[] parameters)
        {
            T returnObject = default(T);

            if (_methodInfo != null && _methodObject != null)
            {
                returnObject = (T)_methodInfo.Invoke(_methodObject, parameters);
            }
            else
            {
                Debug.LogError("Method Hook: Can't call method as it or the object it belongs to is null!");
            }

            return returnObject;
        }
    }
}
