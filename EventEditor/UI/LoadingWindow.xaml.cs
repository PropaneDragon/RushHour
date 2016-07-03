using System.Windows;

namespace EventEditor.UI
{
    /// <summary>
    /// Interaction logic for LoadingWindow.xaml
    /// </summary>
    public partial class LoadingWindow : Window
    {
        public double Minimum
        {
            get
            {
                return _progressBar.Minimum;
            }
            set
            {
                _progressBar.Minimum = value;
            }
        }

        public double Maximum
        {
            get
            {
                return _progressBar.Maximum;
            }
            set
            {
                _progressBar.Maximum = value;
            }
        }

        public double Progress
        {
            get
            {
                return _progressBar.Value;
            }
            set
            {
                _progressBar.IsIndeterminate = false;
                _progressBar.Value = value;
            }
        }

        public LoadingWindow(string message)
        {
            InitializeComponent();

            _loadingLabel.Content = message;
        }
    }
}
