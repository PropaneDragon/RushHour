using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using RushHour.Containers;
using RushHour.Events;
using RushHour.Events.Unique;
using RushHour.Localisation;
using System.Collections.Generic;
using UnityEngine;
using ICities;
using System;

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
        protected UILabel _totalIncomeLabel = null;
        protected UILabel _incomeLabel = null;
        protected UIFastList _incentiveList = null;
        protected UIButton _createButton = null;
        protected UISlider _startTimeSlider = null;
        protected UISlider _startDaySlider = null;

        protected float totalCost = 0f;
        protected float maxIncome = 0f;

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
            _totalIncomeLabel = _totalPanel.AddUIComponent<UILabel>();
            _costLabel = _totalPanel.AddUIComponent<UILabel>();
            _incomeLabel = _totalPanel.AddUIComponent<UILabel>();
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

            _startDaySlider = _helper.AddSlider("Days", 0, 7, 1, 1, delegate (float value)
            {
                CalculateTotal();
            }) as UISlider;

            TimeOfDaySlider startTimeSliderOptions = new TimeOfDaySlider() { uniqueName = "Time", value = 0 };
            _startTimeSlider = startTimeSliderOptions.Create(_helper) as UISlider;
            _startTimeSlider.eventValueChanged += delegate (UIComponent component, float value)
            {
                CalculateTotal();
            };

            _createButton = _helper.AddButton("Create", new OnButtonClicked(CreateEvent)) as UIButton;

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
            sliderLabel.textScale = 0.8f;
            sliderLabel.width = _ticketSlider.width;

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
            _totalAmountLabel.width = 110;
            _totalAmountLabel.height = _totalPanel.height / 2f;
            _totalAmountLabel.name = "TotalLabel";
            _totalAmountLabel.padding = new RectOffset(4, 4, 4, 4);
            _totalAmountLabel.textAlignment = UIHorizontalAlignment.Left;
            _totalAmountLabel.verticalAlignment = UIVerticalAlignment.Middle;
            _totalAmountLabel.textColor = new Color32(255, 100, 100, 255);
            _totalAmountLabel.color = new Color32(91, 97, 106, 255);
            _totalAmountLabel.textScale = 0.7f;

            _totalIncomeLabel.relativePosition = _totalAmountLabel.relativePosition + new Vector3(0, _totalAmountLabel.height);
            _totalIncomeLabel.autoSize = false;
            _totalIncomeLabel.autoHeight = false;
            _totalIncomeLabel.width = 110;
            _totalIncomeLabel.height = _totalPanel.height / 2f;
            _totalIncomeLabel.name = "TotalIncome";
            _totalIncomeLabel.padding = new RectOffset(4, 4, 4, 4);
            _totalIncomeLabel.textAlignment = UIHorizontalAlignment.Left;
            _totalIncomeLabel.verticalAlignment = UIVerticalAlignment.Middle;
            _totalIncomeLabel.textColor = new Color32(206, 248, 0, 255);
            _totalIncomeLabel.color = new Color32(91, 97, 106, 255);
            _totalIncomeLabel.textScale = 0.7f;

            _costLabel.relativePosition = _totalAmountLabel.relativePosition + new Vector3(_totalAmountLabel.width, 4);
            _costLabel.autoSize = false;
            _costLabel.autoHeight = false;
            _costLabel.width = _totalPanel.width - _totalAmountLabel.width - 4;
            _costLabel.height = _totalAmountLabel.height - 8;
            _costLabel.atlas = CimTools.CimToolsHandler.CimToolBase.SpriteUtilities.GetAtlas("Ingame");
            _costLabel.backgroundSprite = "TextFieldPanel";
            _costLabel.name = "Cost";
            _costLabel.padding = new RectOffset(4, 4, 2, 2);
            _costLabel.textAlignment = UIHorizontalAlignment.Right;
            _costLabel.verticalAlignment = UIVerticalAlignment.Middle;
            _costLabel.textColor = new Color32(238, 95, 0, 255);
            _costLabel.color = new Color32(45, 52, 61, 255);
            _costLabel.textScale = 0.7f;

            _incomeLabel.relativePosition = _totalIncomeLabel.relativePosition + new Vector3(_totalIncomeLabel.width, 4);
            _incomeLabel.autoSize = false;
            _incomeLabel.autoHeight = false;
            _incomeLabel.width = _totalPanel.width - _totalIncomeLabel.width - 4;
            _incomeLabel.height = _totalIncomeLabel.height - 8;
            _incomeLabel.atlas = CimTools.CimToolsHandler.CimToolBase.SpriteUtilities.GetAtlas("Ingame");
            _incomeLabel.backgroundSprite = "TextFieldPanel";
            _incomeLabel.name = "Income";
            _incomeLabel.padding = new RectOffset(4, 4, 2, 2);
            _incomeLabel.textAlignment = UIHorizontalAlignment.Right;
            _incomeLabel.verticalAlignment = UIVerticalAlignment.Middle;
            _incomeLabel.textColor = new Color32(151, 238, 0, 255);
            _incomeLabel.color = new Color32(45, 52, 61, 255);
            _incomeLabel.textScale = 0.7f;

            _incentiveList.canSelect = false;
            _incentiveList.relativePosition = sliderPanel.relativePosition + new Vector3(0, sliderPanel.height + 10);
            _incentiveList.width = width - 20;
            _incentiveList.name = "IncentiveSelectionList";
            _incentiveList.backgroundSprite = "UnlockingPanel";
            _incentiveList.rowHeight = 76f;

            sliderPanel = _startTimeSlider.parent as UIPanel;
            sliderLabel = sliderPanel.Find<UILabel>("Label");
            sliderLabel.textScale = 0.8f;

            sliderPanel = _startDaySlider.parent as UIPanel;
            sliderLabel = sliderPanel.Find<UILabel>("Label");
            sliderLabel.textScale = 0.8f;

            _createButton.textScale = 0.9f;
            _createButton.anchor = UIAnchorStyle.Right | UIAnchorStyle.Bottom;
            _createButton.height = sliderPanel.height;

            _incentiveList.height = height - _incentiveList.relativePosition.y - 20 - _createButton.height;

            Translation_OnLanguageChanged("Manually Called!");
            LayOut();
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

                CimTools.CimToolsHandler.CimToolBase.DetailedLogger.Log("Event capacity is " + _linkedEvent.GetCapacity().ToString() + ".");
                CimTools.CimToolsHandler.CimToolBase.DetailedLogger.Log("Successfully set up the UserEventCreationWindow.");
            }
            else
            {
                CimTools.CimToolsHandler.CimToolBase.DetailedLogger.LogError("Linked event was invalid, or the building was 0!");
            }
        }

        private void LayOut()
        {
            float availableWidth = width - _createButton.width;

            _createButton.relativePosition = new Vector3(width - _createButton.width - 10, height - _createButton.height - 10);

            _startTimeSlider.width = (availableWidth / 2f) - 10;

            UIPanel startTimeSliderPanel = _startTimeSlider.parent as UIPanel;
            startTimeSliderPanel.width = _startTimeSlider.width;
            startTimeSliderPanel.relativePosition = new Vector3(10, _createButton.relativePosition.y - 7f);

            UILabel sliderLabel = startTimeSliderPanel.Find<UILabel>("Label");
            sliderLabel.width = _startTimeSlider.width;

            _startDaySlider.width = (availableWidth / 2f) - 35;

            UIPanel startDaySliderPanel = _startDaySlider.parent as UIPanel;
            startDaySliderPanel.width = _startDaySlider.width;
            startDaySliderPanel.relativePosition = new Vector3(startTimeSliderPanel.relativePosition.x + startTimeSliderPanel.width + 5, _createButton.relativePosition.y - 7f);

            sliderLabel = startTimeSliderPanel.Find<UILabel>("Label");
            sliderLabel.width = _startDaySlider.width;
        }

        public void CalculateTotal()
        {
            if (_linkedEvent != null && _ticketSlider != null)
            {
                EconomyManager economyManager = Singleton<EconomyManager>.instance;
                CityEventXmlCosts costs = _linkedEvent.GetCosts();

                totalCost = 0f;
                maxIncome = 0f;

                totalCost += costs._creation;
                totalCost += _ticketSlider.value * costs._perHead;

                maxIncome += _ticketSlider.value * costs._entry;

                if(_incentiveList != null)
                {
                    FastList<object> optionItems = _incentiveList.rowsData;

                    foreach(IncentiveOptionItem optionItemObject in optionItems)
                    {
                        totalCost += optionItemObject.cost * optionItemObject.sliderValue;
                        maxIncome += optionItemObject.returnCost * optionItemObject.sliderValue;
                    }
                }

                if (_costLabel != null)
                {
                    _costLabel.text = totalCost.ToString(Settings.moneyFormat, LocaleManager.cultureInfo);
                    _incomeLabel.text = maxIncome.ToString(Settings.moneyFormat, LocaleManager.cultureInfo);
                }

                if(_ticketSlider != null)
                {
                    UIPanel sliderPanel = _ticketSlider.parent as UIPanel;
                    UILabel sliderLabel = sliderPanel.Find<UILabel>("Label");
                    sliderLabel.text = string.Format(LocalisationStrings.EVENT_TICKETCOUNT, _ticketSlider.value);
                }

                if (_startTimeSlider != null)
                {
                    UIPanel sliderPanel = _startTimeSlider.parent as UIPanel;
                    UILabel sliderLabel = sliderPanel.Find<UILabel>("Label");
                    sliderLabel.text = string.Format(LocalisationStrings.EVENT_STARTTIMESLIDER, TimeOfDaySlider.getTimeFromFloatingValue(_startTimeSlider.value));
                }

                if (_startDaySlider != null)
                {
                    UIPanel sliderPanel = _startDaySlider.parent as UIPanel;
                    UILabel sliderLabel = sliderPanel.Find<UILabel>("Label");
                    sliderLabel.text = string.Format(LocalisationStrings.EVENT_STARTDAYSLIDER, _startDaySlider.value);
                }

                if (_createButton != null)
                {
                    int adjustedCost = Mathf.RoundToInt(totalCost * 100f);
                    if (economyManager.PeekResource(EconomyManager.Resource.Construction, adjustedCost) != adjustedCost)
                    {
                        _createButton.Disable();
                    }
                    else
                    {
                        _createButton.Enable();
                    }
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
                _totalAmountLabel.tooltip = LocalisationStrings.EVENTTOOLTIP_TOTALCOST;
                _totalIncomeLabel.text = LocalisationStrings.EVENT_TOTALINCOME;
                _totalIncomeLabel.tooltip = LocalisationStrings.EVENTTOOLTIP_TOTALINCOME;
                _createButton.text = LocalisationStrings.EVENT_CREATE;
                _createButton.tooltip = LocalisationStrings.EVENTTOOLTIP_CREATE;

                UIPanel sliderPanel = _startTimeSlider.parent as UIPanel;
                UILabel sliderLabel = sliderPanel.Find<UILabel>("Label");
                sliderLabel.tooltip = LocalisationStrings.EVENTTOOLTIP_TICKETCOUNT;

                _totalAmountLabel.Update();
                _totalIncomeLabel.Update();
                _createButton.Update();

                CalculateTotal();
                LayOut();
            }
        }

        private void CreateEvent()
        {
            if (_linkedEvent != null)
            {
                FastList<object> optionItems = _incentiveList.rowsData;
                List<IncentiveOptionItem> optionItemList = new List<IncentiveOptionItem>();

                foreach (IncentiveOptionItem optionItemObject in optionItems)
                {
                    if (optionItemObject != null)
                    {
                        optionItemList.Add(optionItemObject);
                    }
                }

                DateTime startTime = new DateTime(CityEventManager.CITY_TIME.Year, CityEventManager.CITY_TIME.Month, CityEventManager.CITY_TIME.Day);

                if (_startTimeSlider.value <= CityEventManager.CITY_TIME.TimeOfDay.TotalHours)
                {
                    startTime = startTime.AddDays(1d);
                }

                startTime = startTime.AddDays(_startDaySlider.value);
                startTime = startTime.AddHours(_startTimeSlider.value);

                if (_linkedEvent.CreateUserEvent(Mathf.RoundToInt(_ticketSlider.value), _linkedEvent.m_eventData.m_entryCost, optionItemList, startTime))
                {
                    CityEventManager.instance.AddEvent(_linkedEvent);
                    Hide();
                }
                else
                {
                    UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage(LocalisationStrings.EVENT_CREATIONERRORTITLE, LocalisationStrings.EVENT_CREATIONERRORDESCRIPTION, true);
                    CimTools.CimToolsHandler.CimToolBase.DetailedLogger.LogError("Couldn't create user event!");
                }
            }
        }
    }
}
