using ColossalFramework.UI;
using UnityEngine;

namespace RushHourLoader
{
    public class SubscribePopUp : UIPanel
    {
        private UILabel _informationLabel;
        private UIButton _subscribeButton;
        private UIButton _closeButton;
        private RushHourActivator _activator;

        public string description
        {
            set
            {
                if (_informationLabel == null)
                    return;

                _informationLabel.text = value;
                _informationLabel.Update();
                PerformLayout();
            }
            get
            {
                if (_informationLabel != null)
                    return _informationLabel.text;

                return "";
            }
        }

        public RushHourActivator activator
        {
            set { _activator = value; }
            get { return _activator; }
        }

        public override void Awake()
        {
            base.Awake();

            Initialise();
        }

        public override void Start()
        {
            base.Start();

            Initialise();
            
            backgroundSprite = "CitizenBackground";

            _informationLabel.relativePosition = new Vector3(0, 0);
            _informationLabel.autoSize = false;
            _informationLabel.width = width;
            _informationLabel.padding = new RectOffset(20, 20, 50, 10);
            _informationLabel.autoHeight = true;
            _informationLabel.processMarkup = true;
            _informationLabel.wordWrap = true;
            _informationLabel.textScale = 0.8f;

            _subscribeButton.textScale = 0.8f;
            _subscribeButton.eventClicked += _subscribeButton_eventClicked;

            _closeButton.textScale = 0.8f;
            _closeButton.eventClicked += _closeButton_eventClicked;

            PerformLayout();
        }

        private void Initialise()
        {
            UIView view = UIView.GetAView();            
            transform.parent = view.transform;

            UIHelper helper = new UIHelper(this);

            width = 300;
            height = 200;
            isInteractive = true;
            enabled = true;

            if (_informationLabel == null)
                _informationLabel = AddUIComponent<UILabel>();

            if (_subscribeButton == null)
                _subscribeButton = helper.AddButton("Subscribe", delegate { }) as UIButton;

            if (_closeButton == null)
                _closeButton = helper.AddButton("Close", delegate { }) as UIButton;
        }

        public override void PerformLayout()
        {
            base.PerformLayout();

            if (_informationLabel != null)
            {
                _subscribeButton.relativePosition = new Vector3(width - _subscribeButton.width - _informationLabel.padding.right, _informationLabel.relativePosition.y + _informationLabel.height);
                _closeButton.relativePosition = new Vector3(_subscribeButton.relativePosition.x - _closeButton.width - 5f, _informationLabel.relativePosition.y + _informationLabel.height);
                height = _subscribeButton.relativePosition.y + _subscribeButton.height + 15f;
            }
        }

        private void _subscribeButton_eventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            _activator.SubscribeToCimTools();
        }

        private void _closeButton_eventClicked(UIComponent component, UIMouseEventParameter eventParam)
        {
            Hide();
        }
    }
}
