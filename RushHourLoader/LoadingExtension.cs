using ICities;
using UnityEngine;

namespace RushHourLoader
{
    public class LoadingExtension : LoadingExtensionBase
    {
        public static GameObject _activationPopUpGameObject;
        public static SubscribePopUp _activationPopUp;

        public override void OnReleased()
        {
            base.OnReleased();

            if(_activationPopUp != null)
            {
                _activationPopUp.Hide();
                _activationPopUp.Invalidate();
                _activationPopUp = null;
            }

            if(_activationPopUpGameObject != null)
            {
                _activationPopUpGameObject.SetActive(false);
                _activationPopUpGameObject = null;
            }
        }
    }
}