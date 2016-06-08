using UnityEngine;

namespace RushHourLoader
{
    public class ModExtensionHandler
    {
        public virtual void OnEnabled()
        {
            Debug.LogWarning("Base ModExtensionHandler");
        }
    }
}
