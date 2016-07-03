using System.Windows.Controls;

namespace EventEditor.UI
{
    /// <summary>
    /// Interaction logic for ContentPlacemarkBlock.xaml
    /// </summary>
    public partial class ContentPlacemarkBlock : UserControl
    {
        private string[] _scrollableLabels = null;
        private string _label = null;

        public string[] AlternativeLabels
        {
            get
            {
                return _scrollableLabels;
            }
            set
            {
                _scrollableLabels = value;
            }
        }

        public ContentPlacemarkBlock(string label)
        {
            InitializeComponent();

            _label = label;
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            label.Content = _label;
        }
    }
}
