using ColossalFramework.UI;
using RushHour.Logging;
using RushHour.UI;
using UnityEngine;

namespace RushHour.Events
{
    internal class EventPopupManager
    {
        protected static EventPopupManager _instance = null;
        protected EventPopupPanel _panel = null;

        public static EventPopupManager Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new EventPopupManager();
                    _instance.Initialise();
                }

                return _instance;
            }
        }

        public EventPopupPanel Show(string title, string description, InstanceID instance)
        {
            Initialise();

            _panel.title = title;
            _panel.worldInstance = instance;
            _panel.description = description;
            _panel.PerformLayout();
            _panel.Show();
            _panel.Update();

            LoggingWrapper.Log("Showing event popup: " + title + " - " + description);

            return _panel;
        }

        private void Initialise()
        {
            if (_panel == null)
            {
                UIView view = UIView.GetAView();

                LoggingWrapper.Log("Creating event popup panel");
                _panel = LoadingExtension._mainUIGameObject.AddComponent<EventPopupPanel>();
                _panel.transform.parent = view.transform;
                _panel.Hide();
            }
        }
    }
}
