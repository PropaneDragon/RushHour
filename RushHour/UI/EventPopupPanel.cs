using ColossalFramework.UI;
using UnityEngine;
using CimToolsRushHour.v2.Elements;

namespace RushHour.UI
{
    internal class EventPopupPanel : UIPanel
    {
        protected UITitleBar _titleBar = null;
        protected UILabel _informationLabel = null;
        protected Transform _cameraTransform = null;

        public string title
        {
            set
            {
                if (_titleBar != null)
                    _titleBar.title = value;
            }
            get
            {
                if (_titleBar != null)
                    return _titleBar.title;

                return "";
            }
        }

        public string description
        {
            set
            {
                if (_informationLabel == null || value == null)
                    return;

                _informationLabel.text = value;
            }
            get
            {
                if (_informationLabel != null)
                    return _informationLabel.text;

                return "";
            }
        }

        public InstanceID worldInstance = new InstanceID();

        public override void Awake()
        {
            base.Awake();

            Initialise();
        }

        public override void Start()
        {
            base.Start();

            Initialise();

            atlas = CimTools.CimToolsHandler.CimToolBase.SpriteUtilities.GetAtlas("Ingame");
            backgroundSprite = "InfoBubble";

            _titleBar.name = "TitleBar";
            _titleBar.relativePosition = new Vector3(0, 0);
            _titleBar.width = width;

            _informationLabel.relativePosition = new Vector3(0, _titleBar.height);
            _informationLabel.text = description;
            _informationLabel.autoHeight = true;
            _informationLabel.processMarkup = true;
            _informationLabel.textAlignment = UIHorizontalAlignment.Center;
            _informationLabel.verticalAlignment = UIVerticalAlignment.Middle;

            /*_informationLabel.relativePosition = new Vector3(0, _titleBar.height);
            _informationLabel.width = width;
            _informationLabel.padding = new RectOffset(5, 5, 5, 5);
            _informationLabel.autoHeight = true;
            _informationLabel.processMarkup = true;
            _informationLabel.wordWrap = true;
            _informationLabel.textScale = 0.7f;*/

            _cameraTransform = Camera.main.transform;

            PerformLayout();
        }

        private void Initialise()
        {
            width = 300;
            height = 200;
            isInteractive = true;
            enabled = true;

            if (_titleBar == null)
                _titleBar = AddUIComponent<UITitleBar>();

            if (_informationLabel == null)
                _informationLabel = AddUIComponent<UILabel>();

            _titleBar.Initialise(CimTools.CimToolsHandler.CimToolBase);
        }

        public override void PerformLayout()
        {
            base.PerformLayout();

            if (_informationLabel != null)
            {
                height = _informationLabel.relativePosition.y + _informationLabel.height + 15f;
            }

            pivot = UIPivotPoint.BottomLeft;
        }

        public override void Update()
        {
            if(_cameraTransform)
            {
                Vector3 position;
                Quaternion rotation;
                Vector3 size;

                if (InstanceManager.GetPosition(worldInstance, out position, out rotation, out size))
                {
                    position.y += size.y * 0.8f;
                }
                else
                {
                    position = Vector3.zero;
                    Hide();
                }

                UIView view = GetUIView();
                Vector3 screenPos = Camera.main.WorldToScreenPoint(position) * Mathf.Sign(Vector3.Dot(position - _cameraTransform.position, _cameraTransform.forward)); ;
                Vector3 scaledPos = screenPos / view.inputScale;
                Vector3 upperLeftTransform = UIPivotExtensions.UpperLeftToTransform(pivot, size, arbitraryPivotOffset);
                Vector3 guiPoint = view.ScreenPointToGUI(scaledPos) + new Vector2(upperLeftTransform.x, upperLeftTransform.y);

                relativePosition = guiPoint;
            }

            base.Update();
        }
    }
}
