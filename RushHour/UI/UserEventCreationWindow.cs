using ColossalFramework.Globalization;
using ColossalFramework.UI;
using RushHour.Containers;
using RushHour.Events;
using RushHour.Events.Unique;
using RushHour.Localisation;
using System.Collections.Generic;
using UnityEngine;

namespace RushHour.UI
{
    public class UserEventCreationWindow : UIPanel
    {
        protected XmlEvent _linkedEvent = null;
        protected UIHelper _helper = null;
        protected UITitleBar _titleBar = null;
        protected UISlider _ticketSlider = null;
        protected UIPanel _totalPanel = null;
        protected UILabel _totalAmountLabel = null;
        protected UILabel _costLabel = null;
        protected UIFastList _incentiveList = null;
        protected UIButton _createButton = null;

        public string title
        {
            set
            {
                if(_titleBar)
                {
                    _titleBar.title = value;
                }
            }

            get
            {
                if (_titleBar)
                {
                    return _titleBar.title;
                }

                return "";
            }
        }

        public override void Awake()
        {
            base.Awake();

            _helper = new UIHelper(this);
            _titleBar = AddUIComponent<UITitleBar>();
            _totalPanel = AddUIComponent<UIPanel>();
            _totalAmountLabel = _totalPanel.AddUIComponent<UILabel>();
            _costLabel = _totalPanel.AddUIComponent<UILabel>();
            _incentiveList = UIFastList.Create<UIFastListIncentives>(this);

            _ticketSlider = _helper.AddSlider("Tickets", 100, 9000, 10, 500, delegate (float value)
            {
                if (_incentiveList != null)
                {
                    FastList<object> optionItems = _incentiveList.rowsData;

                    foreach (IncentiveOptionItem optionItemObject in optionItems)
                    {
                        optionItemObject.ticketCount = value;
                        optionItemObject.UpdateTicketSize();
                    }
                }
            }) as UISlider;

            _createButton = _helper.AddButton("Create", delegate ()
            {

            }) as UIButton;

            CimTools.CimToolsHandler.CimToolBase.Translation.OnLanguageChanged += Translation_OnLanguageChanged;
        }

        public override void Start()
        {
            base.Start();

            width = 400;
            height = 400;
            atlas = CimTools.CimToolsHandler.CimToolBase.SpriteUtilities.GetAtlas("Ingame");
            backgroundSprite = "MenuPanel2";

            _titleBar.name = "TitleBar";
            _titleBar.relativePosition = new Vector3(0, 0);
            _titleBar.width = width;

            _ticketSlider.width = width - ((width * 60f) / 100f);
            _ticketSlider.eventValueChanged += TicketSlider_eventValueChanged;

            UIPanel sliderPanel = _ticketSlider.parent as UIPanel;
            sliderPanel.width = _ticketSlider.width;
            sliderPanel.name = "TicketSliderPanel";
            sliderPanel.relativePosition = new Vector3(10, _titleBar.height + 10);

            UILabel sliderLabel = sliderPanel.Find<UILabel>("Label");
            sliderLabel.tooltip = "Increase or decrease the number of tickets available for this venue";
            sliderLabel.textScale = 0.8f;

            _totalPanel.atlas = CimTools.CimToolsHandler.CimToolBase.SpriteUtilities.GetAtlas("Ingame");
            _totalPanel.backgroundSprite = "GenericPanel";
            _totalPanel.color = new Color32(91, 97, 106, 255);
            _totalPanel.relativePosition = sliderPanel.relativePosition + new Vector3(sliderPanel.width + 10, 0);
            _totalPanel.width = width - sliderPanel.width - 30;
            _totalPanel.height = sliderPanel.height;
            _totalPanel.name = "Totals";

            _totalAmountLabel.relativePosition = Vector3.zero;
            _totalAmountLabel.autoSize = false;
            _totalAmountLabel.autoHeight = false;
            _totalAmountLabel.width = 60;
            _totalAmountLabel.height = _totalPanel.height;
            _totalAmountLabel.name = "TotalLabel";
            _totalAmountLabel.padding = new RectOffset(4, 4, 4, 4);
            _totalAmountLabel.textAlignment = UIHorizontalAlignment.Left;
            _totalAmountLabel.verticalAlignment = UIVerticalAlignment.Middle;
            _totalAmountLabel.textColor = new Color32(248, 64, 0, 255);
            _totalAmountLabel.color = new Color32(91, 97, 106, 255);

            _costLabel.relativePosition = _totalAmountLabel.relativePosition + new Vector3(_totalAmountLabel.width + 10, 10);
            _costLabel.autoSize = false;
            _costLabel.autoHeight = false;
            _costLabel.width = _totalPanel.width - _totalAmountLabel.width - 30;
            _costLabel.height = _totalPanel.height - 20;
            _costLabel.atlas = CimTools.CimToolsHandler.CimToolBase.SpriteUtilities.GetAtlas("Ingame");
            _costLabel.backgroundSprite = "TextFieldPanel";
            _costLabel.name = "Cost";
            _costLabel.textAlignment = UIHorizontalAlignment.Right;
            _costLabel.verticalAlignment = UIVerticalAlignment.Middle;
            _costLabel.textColor = new Color32(238, 95, 0, 255);
            _costLabel.color = new Color32(45, 52, 61, 255);

            _incentiveList.canSelect = false;
            _incentiveList.relativePosition = sliderPanel.relativePosition + new Vector3(0, sliderPanel.height + 10);
            _incentiveList.width = width - 20;
            _incentiveList.name = "IncentiveSelectionList";
            _incentiveList.backgroundSprite = "UnlockingPanel";
            _incentiveList.rowHeight = 76f;

            _createButton.width = 70;
            _createButton.height = 40;
            _createButton.relativePosition = new Vector3(width - _createButton.width - 10, height - _createButton.height - 10);
            _createButton.anchor = UIAnchorStyle.Right | UIAnchorStyle.Bottom;
            _createButton.textScale = 0.9f;

            _incentiveList.height = height - _incentiveList.relativePosition.y - 20 - _createButton.height;
        }

        public void SetUp(LabelOptionItem selectedData, ushort buildingID)
        {
            _linkedEvent = selectedData.linkedEvent;

            if(_linkedEvent != null && buildingID != 0 && _ticketSlider != null)
            {
                title = _linkedEvent.GetReadableName();

                _ticketSlider.maxValue = _linkedEvent.GetCapacity();
                _ticketSlider.minValue = Mathf.Min(_linkedEvent.GetCapacity(), 100);
                _ticketSlider.value = _ticketSlider.minValue;

                _incentiveList.rowsData.Clear();

                if (_linkedEvent != null)
                {
                    List<CityEventXmlIncentive> incentives = _linkedEvent.GetIncentives();

                    foreach (CityEventXmlIncentive incentive in incentives)
                    {
                        IncentiveOptionItem optionItem = new IncentiveOptionItem()
                        {
                            cost = incentive._cost,
                            description = incentive._description,
                            negativeEffect = incentive._negativeEffect,
                            positiveEffect = incentive._positiveEffect,
                            returnCost = incentive._returnCost,
                            title = incentive._name,
                            ticketCount = _ticketSlider.value
                        };
                        optionItem.OnOptionItemChanged += OptionItem_OnOptionItemChanged;

                        _incentiveList.rowsData.Add(optionItem);
                        _incentiveList.DisplayAt(0);
                    }

                    CalculateTotal();
                }

                CimTools.CimToolsHandler.CimToolBase.DetailedLogger.Log("Successfully set up the UserEventCreationWindow.");
            }
            else
            {
                CimTools.CimToolsHandler.CimToolBase.DetailedLogger.LogError("Linked event was invalid, or the building was 0!");
            }
        }

        public void CalculateTotal()
        {
            if (_linkedEvent != null && _ticketSlider != null)
            {
                float total = 0f;
                CityEventXmlCosts costs = _linkedEvent.GetCosts();

                total += costs._creation;
                total += _ticketSlider.value * costs._perHead;

                if(_incentiveList != null)
                {
                    FastList<object> optionItems = _incentiveList.rowsData;

                    foreach(IncentiveOptionItem optionItemObject in optionItems)
                    {
                        total += optionItemObject.cost * optionItemObject.sliderValue;
                    }
                }

                if (_costLabel != null)
                {
                    _costLabel.text = total.ToString(Settings.moneyFormat, LocaleManager.cultureInfo);
                }

                if(_ticketSlider != null)
                {
                    UIPanel sliderPanel = _ticketSlider.parent as UIPanel;
                    UILabel sliderLabel = sliderPanel.Find<UILabel>("Label");
                    sliderLabel.text = string.Format(LocalisationStrings.EVENT_TICKETCOUNT, _ticketSlider.value);
                }
            }
        }

        private void OptionItem_OnOptionItemChanged()
        {
            CalculateTotal();
        }

        private void TicketSlider_eventValueChanged(UIComponent component, float value)
        {
            CalculateTotal();
        }

        private void Translation_OnLanguageChanged(string languageIdentifier)
        {
            if (_totalAmountLabel != null)
            {
                _totalAmountLabel.text = LocalisationStrings.EVENT_TOTALCOST;
                _createButton.text = LocalisationStrings.EVENT_CREATE;

                _totalAmountLabel.Update();
                _createButton.Update();

                CalculateTotal();
            }
        }
    }
}
