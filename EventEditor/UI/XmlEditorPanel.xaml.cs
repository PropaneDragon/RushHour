using System.Windows.Controls;
using RushHour.Events;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;
using Microsoft.Win32;
using System.IO;
using System;
using System.ComponentModel;
using EventEditor.Utils;
using System.Text.RegularExpressions;

namespace EventEditor.UI
{
    /// <summary>
    /// Interaction logic for EventXmlEditor.xaml
    /// </summary>
    public partial class XmlEditorPanel : UserControl
    {
        public CityEventXmlContainer _container = null;
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
        private string _workingDirectory = null;

        public XmlEditorPanel(CityEventXmlContainer xmlEvent, string workingDirectory, TabItem parentTab = null)
        {
            InitializeComponent();

            _container = xmlEvent;
            _workingDirectory = workingDirectory;
            _parentTab = parentTab;

            LoadXmlEvent();
        }

        private void _searchBuildingClass_Click(object sender, RoutedEventArgs e)
        {
            bool exit = false;

            while (!exit)
            {
                OpenFileDialog _openFile = new OpenFileDialog();
                _openFile.DefaultExt = ".crp";
                _openFile.CheckFileExists = true;
                _openFile.CheckPathExists = true;
                _openFile.Filter = "Model files|*.crp";
                _openFile.Title = "Navigate to a model file to load";
                _openFile.InitialDirectory = _workingDirectory;

                bool? result = _openFile.ShowDialog();

                if (result.HasValue && result.Value)
                {
                    if (File.Exists(_openFile.FileName))
                    {
                        string foundModelName = RetrieveModelNameFromFile(_openFile.FileName);

                        if (foundModelName != null)
                        {
                            _buildingName.Text = foundModelName;
                            exit = true;
                        }
                        else
                        {
                            MessageBoxResult answer = MessageBox.Show("Couldn't find the model name from the file. Would you like me to make a guess? I can't guarantee it's going to be right, but is it worth a try?", "Can't find the model name.", MessageBoxButton.YesNo, MessageBoxImage.Question);

                            if(answer == MessageBoxResult.Yes)
                            {
                                string fileName = Path.GetFileNameWithoutExtension(_openFile.FileName);
                                string steamId = Path.GetFileName(Path.GetDirectoryName(_openFile.FileName));

                                _buildingName.Text = string.Format("{0}.{1}_Data", steamId, fileName);
                            }

                            exit = true;
                        }
                    }
                    else
                    {
                        MessageBoxResult answer = MessageBox.Show("Sorry, that doesn't appear to be a valid file. Try again?", "Invalid file", MessageBoxButton.YesNo, MessageBoxImage.Question);

                        if (answer != MessageBoxResult.Yes)
                        {
                            exit = true;
                        }
                    }
                }
                else
                {
                    exit = true;
                }
            }
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            LoadXmlEvent();
        }

        private void _menuItem_timeUntilEvent_Click(object sender, RoutedEventArgs e)
        {
            _initialisedMessages.Text = _initialisedMessages.Text.Insert(_initialisedMessages.CaretIndex, " {0} ");
        }

        private void _expandAll_Click(object sender, RoutedEventArgs e)
        {
            List<Expander> expanders = FindChildren<Expander>(_chancesOfAttendingGroup as DependencyObject);

            foreach (Expander expander in expanders)
            {
                expander.IsExpanded = true;
            }
        }

        private void _collapseAll_Click(object sender, RoutedEventArgs e)
        {
            List<Expander> expanders = FindChildren<Expander>(_chancesOfAttendingGroup as DependencyObject);

            foreach (Expander expander in expanders)
            {
                expander.IsExpanded = false;
            }
        }

        private void _playerEvents_Checked(object sender, RoutedEventArgs e)
        {
            _playerEventGroup.IsEnabled = _playerEvents.IsChecked.HasValue ? _playerEvents.IsChecked.Value : false;
        }

        private void _newIncentiveButton_Click(object sender, RoutedEventArgs e)
        {
            NewIncentiveTab(Constants.EMPTY_INCENTIVE_TAB_NAME);
        }

        public void LoadXmlEvent()
        {
            if (_container != null)
            {
                _eventName.Text = _container._name;
                _readableName.Text = _container._userEventName;
                _buildingName.Text = _container._eventBuildingClassName;
                _capacity.Text = _container._eventCapacity.ToString();
                _length.Text = _container._eventLength.ToString();
                _randomEvents.IsChecked = _container._supportsRandomEvents;
                _playerEvents.IsChecked = _container._supportUserEvents;
                _watchedOnTv.IsChecked = _container._canBeWatchedOnTV;

                SetUpText(_container._initialisedMessages, _initialisedMessages);
                SetUpText(_container._beginMessages, _startedMessages);
                SetUpText(_container._endedMessages, _endedMessages);

                if (_container._chances != null)
                {
                    _males.Value = _container._chances._males;
                    _females.Value = _container._chances._females;

                    _children.Value = _container._chances._children;
                    _teens.Value = _container._chances._teens;
                    _adults.Value = _container._chances._adults;
                    _seniors.Value = _container._chances._seniors;

                    _lowWealth.Value = _container._chances._lowWealth;
                    _mediumWealth.Value = _container._chances._mediumWealth;
                    _highWealth.Value = _container._chances._highWealth;

                    _uneducated.Value = _container._chances._uneducated;
                    _oneSchool.Value = _container._chances._oneSchool;
                    _twoSchools.Value = _container._chances._twoSchools;
                    _threeSchools.Value = _container._chances._threeSchools;

                    _badHappiness.Value = _container._chances._badHappiness;
                    _poorHappiness.Value = _container._chances._poorHappiness;
                    _goodHappiness.Value = _container._chances._goodHappiness;
                    _excellentHappiness.Value = _container._chances._excellentHappiness;
                    _superbHappiness.Value = _container._chances._superbHappiness;

                    _veryUnhappy.Value = _container._chances._veryUnhappyWellbeing;
                    _unhappy.Value = _container._chances._unhappyWellbeing;
                    _satisfied.Value = _container._chances._satisfiedWellbeing;
                    _happy.Value = _container._chances._happyWellbeing;
                    _veryHappy.Value = _container._chances._veryHappyWellbeing;
                }

                if (_container._costs != null)
                {
                    _creationCost.Text = _container._costs._creation.ToString();
                    _perHeadCost.Text = _container._costs._perHead.ToString();
                    _advertSignCost.Text = _container._costs._advertisingSigns.ToString();
                    _advertTVCost.Text = _container._costs._advertisingTV.ToString();
                    _entryCost.Text = _container._costs._entry.ToString();
                }

                if (_container._incentives != null)
                {
                    _incentiveTabs.Items.Clear();

                    foreach (CityEventXmlIncentive incentive in _container._incentives)
                    {
                        IncentiveEditorPanel editor = new IncentiveEditorPanel(incentive);
                        TabItem createdTab = NewIncentiveTab(incentive._name, editor);

                        editor.ParentTab = createdTab;
                    }
                }
            }

            _playerEvents_Checked(this, new RoutedEventArgs());
        }

        public bool ApplyXmlEvent()
        {
            bool success = true;

            _container = new CityEventXmlContainer()
            {
                _name = _eventName.Text,
                _userEventName = _readableName.Text,
                _eventBuildingClassName = _buildingName.Text,

                _supportsRandomEvents = _randomEvents.IsChecked.HasValue ? _randomEvents.IsChecked.Value : false,
                _supportUserEvents = _playerEvents.IsChecked.HasValue ? _playerEvents.IsChecked.Value : false,
                _canBeWatchedOnTV = _watchedOnTv.IsChecked.HasValue ? _watchedOnTv.IsChecked.Value : false,
                _initialisedMessages = TearDownText(_initialisedMessages),
                _beginMessages = TearDownText(_startedMessages),
                _endedMessages = TearDownText(_endedMessages),

                _chances = new CityEventXmlChances()
                {
                    _children = (int)(Math.Round(_children.Value)),
                    _teens = (int)(Math.Round(_teens.Value)),
                    _adults = (int)(Math.Round(_adults.Value)),
                    _seniors = (int)(Math.Round(_seniors.Value)),

                    _lowWealth = (int)(Math.Round(_lowWealth.Value)),
                    _mediumWealth = (int)(Math.Round(_mediumWealth.Value)),
                    _highWealth = (int)(Math.Round(_highWealth.Value)),

                    _uneducated = (int)(Math.Round(_uneducated.Value)),
                    _oneSchool = (int)(Math.Round(_oneSchool.Value)),
                    _twoSchools = (int)(Math.Round(_twoSchools.Value)),
                    _threeSchools = (int)(Math.Round(_threeSchools.Value)),

                    _badHappiness = (int)(Math.Round(_badHappiness.Value)),
                    _poorHappiness = (int)(Math.Round(_poorHappiness.Value)),
                    _goodHappiness = (int)(Math.Round(_goodHappiness.Value)),
                    _excellentHappiness = (int)(Math.Round(_excellentHappiness.Value)),
                    _superbHappiness = (int)(Math.Round(_superbHappiness.Value)),

                    _veryUnhappyWellbeing = (int)(Math.Round(_veryUnhappy.Value)),
                    _unhappyWellbeing = (int)(Math.Round(_unhappy.Value)),
                    _satisfiedWellbeing = (int)(Math.Round(_satisfied.Value)),
                    _happyWellbeing = (int)(Math.Round(_happy.Value)),
                    _veryHappyWellbeing = (int)(Math.Round(_veryHappy.Value)),
                },

                _costs = new CityEventXmlCosts()
                {
                },

                _incentives = new CityEventXmlIncentive[]
                {

                }
            };

            success = success && SafelyConvert.SafelyParseWithError(_capacity.Text, ref _container._eventCapacity, "capacity");
            success = success && SafelyConvert.SafelyParseWithError(_length.Text, ref _container._eventLength, "event length");

            success = success && SafelyConvert.SafelyParseWithError(_creationCost.Text, ref _container._costs._creation, "creation cost");
            success = success && SafelyConvert.SafelyParseWithError(_perHeadCost.Text, ref _container._costs._perHead, "cost per citizen");
            success = success && SafelyConvert.SafelyParseWithError(_advertSignCost.Text, ref _container._costs._advertisingSigns, "advertising sign cost");
            success = success && SafelyConvert.SafelyParseWithError(_advertTVCost.Text, ref _container._costs._advertisingTV, "advertising TV cost");
            success = success && SafelyConvert.SafelyParseWithError(_entryCost.Text, ref _container._costs._entry, "ticket cost");

            List<CityEventXmlIncentive> incentiveList = new List<CityEventXmlIncentive>();

            foreach(TabItem tab in _incentiveTabs.Items)
            {
                IncentiveEditorPanel editor = tab.Content as IncentiveEditorPanel;

                if (editor != null)
                {
                    success = success && editor.ApplyUserEvent();

                    if(success)
                    {
                        incentiveList.Add(editor._incentive);
                    }
                }
            }

            _container._incentives = incentiveList.ToArray();

            return success;
        }

        private string RetrieveModelNameFromFile(string file)
        {
            string modelName = null;

            if (file != null)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                string steamId = Path.GetFileName(Path.GetDirectoryName(file));
                string dataName = string.Format("{0}_Data", fileName);

                if(File.Exists(file))
                {
                    string fileData = File.ReadAllText(file);
                    
                    if(fileData.Contains(dataName)) //Simple search. Find the filename in the data, not always correct.
                    {
                        modelName = string.Format("{0}.{1}_Data", steamId, fileName);
                    }
                    else //If we can't find the data from the filename, it's named something else, so we need a longer search.
                    {
                        string steamPreviewSearch = "_SteamPreview";
                        int steamPreviewData = fileData.IndexOf(steamPreviewSearch);

                        if(steamPreviewData != -1)
                        {
                            int startPos = steamPreviewData - 1000;
                            startPos = startPos > 0 ? startPos : 0;

                            string chosenData = fileData.Substring(startPos, (steamPreviewData - startPos) + steamPreviewSearch.Length);

                            Regex nameFinder = new Regex("(?:.*\\0)(.*?)(?:_SteamPreview)");
                            Match foundName = nameFinder.Match(chosenData);

                            if(foundName.Groups.Count > 1)
                            {
                                string estimatedName = foundName.Groups[1].Value;
                                bool foundInFile = false;

                                while(estimatedName.Length > 1 && !foundInFile)
                                {
                                    string estimatedDataName = string.Format("{0}_Data", estimatedName);

                                    if (fileData.IndexOf(estimatedDataName) != -1 && fileData.IndexOf(estimatedDataName) != fileData.LastIndexOf(estimatedDataName))
                                    {
                                        foundInFile = true;
                                        modelName = string.Format("{0}.{1}_Data", steamId, estimatedName);
                                    }

                                    estimatedName = estimatedName.Substring(1, estimatedName.Length - 1);
                                }
                            }
                        }
                    }
                }
            }

            return modelName;
        }

        private void SetUpText(string[] messages, TextBox control)
        {
            control.Text = "";

            if (messages != null && messages.Length > 0)
            {
                foreach (string message in messages)
                {
                    if(control.Text != "")
                    {
                        control.Text += "\n";
                    }

                    control.Text += message;
                }
            }
        }

        private string[] TearDownText(TextBox control)
        {
            string[] returnString = { };

            if(control != null && control.Text != "")
            {
                returnString = control.Text.Split('\n');
            }

            return returnString;
        }

        private TabItem NewIncentiveTab(string title, object content = null)
        {
            TabItem tab = new TabItem();
            tab.Header = title;
            tab.Margin = new Thickness(0d);
            tab.MaxWidth = 85;
            tab.MinWidth = 85;

            MenuItem _deleteMenuItem = new MenuItem();
            _deleteMenuItem.Header = "Delete this incentive";
            _deleteMenuItem.Click += (object sender, RoutedEventArgs e) =>
            {
                RemoveIncentiveTab(tab);
            };

            ContextMenu _contextMenu = new ContextMenu();
            _contextMenu.Items.Add(_deleteMenuItem);

            tab.ContextMenu = _contextMenu;

            if (content == null)
            {
                IncentiveEditorPanel editor = new IncentiveEditorPanel(new CityEventXmlIncentive(), tab);
                content = editor;
            }

            tab.Content = content;

            _incentiveTabs.Items.Add(tab);
            _incentiveTabs.SelectedItem = tab;

            return tab;
        }

        private void RemoveIncentiveTab(TabItem tab)
        {
            MessageBoxResult _result = MessageBox.Show("Are you sure you want to delete this incentive?", "Delete incentive?", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (_result == MessageBoxResult.Yes)
            {
                _incentiveTabs.Items.Remove(tab);
                tab = null;
            }
        }

        private List<T> FindChildren<T>(DependencyObject dependency) where T : DependencyObject
        {
            List<T> returnList = new List<T>();

            if (dependency != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependency); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(dependency, i);

                    if (child != null)
                    {
                        if (child is T)
                        {
                            returnList.Add(child as T);
                        }

                        returnList.AddRange(FindChildren<T>(child));
                    }
                }
            }

            return returnList;
        }

        private void UpdateTabName(object sender, TextChangedEventArgs e)
        {
            if(_parentTab != null)
            {
                string readable = _readableName.Text;
                string id = _eventName.Text;

                if (readable != "")
                {
                    _parentTab.Header = readable;
                }
                else if(id != "")
                {
                    _parentTab.Header = id;
                }
                else
                {
                    _parentTab.Header = Constants.EMPTY_EVENT_TAB_NAME;
                }
            }
        }
    }
}
