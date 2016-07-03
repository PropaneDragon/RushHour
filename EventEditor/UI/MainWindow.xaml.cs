using EventEditor.Delegates;
using EventEditor.Utils;
using Microsoft.Win32;
using RushHour.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace EventEditor.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ItemInfoDelegate _itemInfo = null;
        private string _itemPath = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string[] _arguments = Environment.GetCommandLineArgs();

            if (_arguments.Length > 1)
            {
                LoadInformation(_arguments[_arguments.Length - 1]); //Load info for the last argument.
            }
            else
            {
                string steamFolder = FindSteamFolder();

                OpenFileDialog _openFile = new OpenFileDialog();
                _openFile.DefaultExt = ".crp";
                _openFile.CheckFileExists = true;
                _openFile.CheckPathExists = true;
                _openFile.Filter = "Model files|*.crp";
                _openFile.Title = "Navigate to a model file to create an event at";
                _openFile.InitialDirectory = steamFolder;

                bool? result = _openFile.ShowDialog();

                if (result.HasValue && result.Value)
                {
                    string path = _openFile.FileName;

                    if(File.Exists(path))
                    {
                        path = Path.GetDirectoryName(path);

                        LoadInformation(path);
                    }
                }
                else
                {
                    Close();
                }
            }
        }

        private string FindSteamFolder()
        {
            string steamFolder = "";
            DriveInfo[] drives = DriveInfo.GetDrives();

            foreach(DriveInfo drive in drives)
            {
                if(drive.DriveType == DriveType.Fixed && drive.IsReady)
                {
                    char separator = Path.DirectorySeparatorChar;
                    string steamDir = drive.Name + "Program Files (x86)" + separator + "Steam";

                    if(Directory.Exists(steamDir))
                    {
                        steamFolder = steamFolder == "" ? steamDir : steamFolder;

                        string workshopDir = steamDir + separator + "steamapps" + separator + "workshop" + separator + "content" + separator + Constants.CITIES_GAME_ID.ToString();

                        if(Directory.Exists(workshopDir))
                        {
                            steamFolder = workshopDir;
                        }
                    }
                }
            }

            return steamFolder;
        }

        private void LoadInformation(string path)
        {
            if(Directory.Exists(path))
            {
                _itemPath = path;

                string _itemIdString = Path.GetFileName(_itemPath);
                ulong _itemId = 0ul;

                ulong.TryParse(_itemIdString, out _itemId);

                if (_itemId != 0ul)
                {
                    _itemInfo = new ItemInfoDelegate(this, _itemId);
                    _itemInfo.DownloadCompleted += _itemInfo_DownloadCompleted;
                    _itemInfo.DownloadFailed += _itemInfo_DownloadFailed;
                    _itemInfo.DownloadData();
                }
            }
            else
            {
                MessageBox.Show(this, "Couldn't load information for item at " + path, "Failed to load information", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void _itemInfo_DownloadFailed()
        {
            MessageBoxResult result = MessageBox.Show(this, "Couldn't download the item details from the Steam servers. Retry?", "Failed to load information", MessageBoxButton.YesNo, MessageBoxImage.Error);

            if (result == MessageBoxResult.Yes)
            {
                LoadInformation(_itemPath);
            }
            else
            {
                Close();
            }
        }

        private void _itemInfo_DownloadCompleted()
        {
            LoadingWindow loadingImage = new LoadingWindow("Loading image...");
            loadingImage.Owner = this;
            loadingImage.Show();

            if(_itemInfo.ImageUri != null)
            {
                BitmapImage _bitmap = new BitmapImage(_itemInfo.ImageUri);
                _steamImage.Source = _bitmap;
            }

            _titleLabel.Text = _itemInfo.ItemTitle;

            FindEvents();

            loadingImage.Close();
            loadingImage = null;
        }

        private void _createEventButton_Click(object sender, RoutedEventArgs e)
        {
            NewTab(Constants.EMPTY_EVENT_TAB_NAME);
        }

        private void _saveButton_Click(object sender, RoutedEventArgs e)
        {            
            List<XmlEditorPanel> _editors = GetAllXmlEditors();

            SaveDialog saveDialog = new SaveDialog(_editors, _itemPath);
            saveDialog.Owner = this;
            saveDialog.ShowDialog();
        }

        private void FindEvents()
        {
            if(_itemPath != null)
            {
                string _eventsPath = _itemPath + Path.DirectorySeparatorChar + "RushHour Events";

                if(Directory.Exists(_eventsPath))
                {
                    string[] files = Directory.GetFiles(_eventsPath);

                    foreach(string file in files)
                    {
                        try
                        {
                            XmlSerializer _xmlSerialiser = new XmlSerializer(typeof(CityEventXml));
                            CityEventXml xmlEvent = _xmlSerialiser.Deserialize(new FileStream(file, FileMode.Open)) as CityEventXml;

                            if (xmlEvent != null)
                            {
                                foreach (CityEventXmlContainer xmlContainer in xmlEvent._containedEvents)
                                {
                                    string tabName = xmlContainer._userEventName == "" ? (xmlContainer._name == "" ? Constants.EMPTY_EVENT_TAB_NAME : xmlContainer._name) : xmlContainer._userEventName;

                                    XmlEditorPanel panel = new XmlEditorPanel(xmlContainer, _itemPath);
                                    TabItem createdTab = NewTab(tabName, panel);

                                    panel.ParentTab = createdTab;
                                }
                            }
                        }
                        catch
                        {
                            MessageBox.Show(this, "Couldn't load up the event data.", "Failed to load", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }

            if(_eventTabs.Items.Count <= 0)
            {
                NewTab(Constants.EMPTY_EVENT_TAB_NAME);
            }
        }

        private TabItem NewTab(string title, object content = null)
        {
            TabItem tab = new TabItem();
            tab.Header = title;
            tab.Margin = new Thickness(0d);
            tab.MaxWidth = 100;
            tab.Opacity = 0.9;

            MenuItem _deleteMenuItem = new MenuItem();
            _deleteMenuItem.Header = "Delete this event";
            _deleteMenuItem.Click += (object sender, RoutedEventArgs e) =>
            {
                RemoveTab(tab);
            };

            ContextMenu _contextMenu = new ContextMenu();
            _contextMenu.Items.Add(_deleteMenuItem);

            tab.ContextMenu = _contextMenu;

            if (content == null)
            {
                content = new XmlEditorPanel(new CityEventXmlContainer(), _itemPath, tab);
            }

            tab.Content = content;

            _eventTabs.Items.Add(tab);
            _eventTabs.SelectedItem = tab;

            return tab;
        }

        private void RemoveTab(TabItem tab)
        {
            MessageBoxResult _result = MessageBox.Show("Are you sure you want to delete this event?", "Delete event?", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (_result == MessageBoxResult.Yes)
            {
                _eventTabs.Items.Remove(tab);
                tab = null;
            }
        }

        private List<XmlEditorPanel> GetAllXmlEditors()
        {
            List<XmlEditorPanel> returnList = new List<XmlEditorPanel>();

            foreach (TabItem tab in _eventTabs.Items)
            {
                if (tab != null)
                {
                    XmlEditorPanel xmlEditor = tab.Content as XmlEditorPanel;

                    if(xmlEditor != null)
                    {
                        returnList.Add(xmlEditor);
                    }
                }
            }

            return returnList;
        }

        private List<CityEventXmlContainer> GetAllXmlContainers()
        {
            List<CityEventXmlContainer> returnList = new List<CityEventXmlContainer>();
            List<XmlEditorPanel> xmlEditors = GetAllXmlEditors();

            foreach(XmlEditorPanel xmlEditor in xmlEditors)
            {
                CityEventXmlContainer foundXmlContainer = xmlEditor._container;

                if(foundXmlContainer != null)
                {
                    returnList.Add(foundXmlContainer);
                }
            }

            return returnList;
        }
    }
}
