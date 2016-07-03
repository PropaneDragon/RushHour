using EventEditor.Utils;
using RushHour.Events;
using System.Windows.Controls;

namespace EventEditor.UI
{
    /// <summary>
    /// Interaction logic for EventXmlUserEventEditor.xaml
    /// </summary>
    public partial class IncentiveEditorPanel : UserControl
    {
        public CityEventXmlIncentive _incentive = null;
        public TabItem ParentTab
        {
            get
            {
                return _parentTab;
            }
            set
            {
                _parentTab = value;
            }
        }

        private TabItem _parentTab = null;

        public IncentiveEditorPanel(CityEventXmlIncentive incentive, TabItem parentTab = null)
        {
            InitializeComponent();

            _incentive = incentive;
            _parentTab = parentTab;

            LoadUserEvent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            LoadUserEvent();
        }

        private void _name_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_parentTab != null)
            {
                if (_name.Text != "")
                {
                    _parentTab.Header = _name.Text;
                }
                else
                {
                    _parentTab.Header = Constants.EMPTY_INCENTIVE_TAB_NAME;
                }
            }
        }

        public void LoadUserEvent()
        {
            if (_incentive != null)
            {
                _name.Text = _incentive._name;
                _description.Text = _incentive._description;
                _costBuy.Text = _incentive._cost.ToString();
                _costSell.Text = _incentive._returnCost.ToString();
                _positiveEffect.Text = _incentive._positiveEffect.ToString();
                _negativeEffect.Text = _incentive._negativeEffect.ToString();
                _activeWhenRandomEvent.IsChecked = _incentive._activeWhenRandomEvent;
            }
        }

        public bool ApplyUserEvent()
        {
            bool success = true;

            _incentive = new CityEventXmlIncentive()
            {
                _name = _name.Text,
                _description = _description.Text,
                _activeWhenRandomEvent = _activeWhenRandomEvent.IsChecked.HasValue ? _activeWhenRandomEvent.IsChecked.Value : false
            };

            success = success && SafelyConvert.SafelyParseWithError(_costBuy.Text, ref _incentive._cost, "cost to buy");
            success = success && SafelyConvert.SafelyParseWithError(_costSell.Text, ref _incentive._returnCost, "cost to sell");
            success = success && SafelyConvert.SafelyParseWithError(_positiveEffect.Text, ref _incentive._positiveEffect, "percentage increase");
            success = success && SafelyConvert.SafelyParseWithError(_negativeEffect.Text, ref _incentive._negativeEffect, "percentage decrease");

            return success;
        }
    }
}
