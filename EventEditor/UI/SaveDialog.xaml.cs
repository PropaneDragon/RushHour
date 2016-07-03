using RushHour.Events;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Xml.Serialization;
using System.Diagnostics;

namespace EventEditor.UI
{
    /// <summary>
    /// Interaction logic for SaveDialog.xaml
    /// </summary>
    public partial class SaveDialog : Window
    {
        private List<XmlEditorPanel> _editors = null;
        private List<XmlEditorPanel> _editorsToSave = new List<XmlEditorPanel>();

        private string _directory = null;

        public SaveDialog(List<XmlEditorPanel> editors, string directory)
        {
            InitializeComponent();

            _editors = editors;
            _directory = directory;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _xmlContainerList.Items.Clear();
            ApplyAllXmlPanels();

            if (_editors != null)
            {
                foreach (XmlEditorPanel editor in _editors)
                {
                    if (editor._container != null)
                    {
                        CityEventXmlContainer _container = editor._container;

                        CheckBox _checkBox = new CheckBox();
                        _checkBox.Content = _container._userEventName == "" ? _container._name : _container._userEventName;
                        _checkBox.Checked += (object sender2, RoutedEventArgs e2) =>
                        {
                            if(!_editorsToSave.Contains(editor))
                            {
                                _editorsToSave.Add(editor);
                                CheckToEnableSave();
                            }
                        };

                        _checkBox.Unchecked += (object sender2, RoutedEventArgs e2) =>
                        {
                            if (_editorsToSave.Contains(editor))
                            {
                                _editorsToSave.Remove(editor);
                                CheckToEnableSave();
                            }
                        };

                        _xmlContainerList.Items.Add(_checkBox);
                    }
                }
            }
        }

        private void _saveButton_Click(object sender, RoutedEventArgs e)
        {
            if(_directory != null && _fileName.Text != "")
            {
                bool overallSaveSucceeded = true;

                LoadingWindow savingProgress = new LoadingWindow("Saving...");
                savingProgress.Owner = this;
                savingProgress.Show();

                overallSaveSucceeded = overallSaveSucceeded && ApplyAllXmlPanels();
                overallSaveSucceeded = overallSaveSucceeded && SaveAllSelectedXmlData();

                savingProgress.Close();
                savingProgress = null;

                Close();

                if (overallSaveSucceeded)
                {
                    MessageBox.Show(this, "Saved successfully.", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(this, "Save failed.", "Failed to save", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void _fileName_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckToEnableSave();
        }

        private void CheckToEnableSave()
        {
            if (_editorsToSave.Count > 0 && _fileName.Text != "")
            {
                _saveButton.IsEnabled = true;
            }
            else
            {
                _saveButton.IsEnabled = false;
            }
        }

        private bool ApplyAllXmlPanels()
        {
            bool overallSaveSucceeded = true;

            if (_editors != null && _editors.Count > 0)
            {
                foreach (XmlEditorPanel editor in _editors)
                {
                    if (editor._container != null)
                    {
                        overallSaveSucceeded = overallSaveSucceeded && editor.ApplyXmlEvent();
                    }
                    else
                    {
                        overallSaveSucceeded = false;
                        break;
                    }
                }
            }
            else
            {
                overallSaveSucceeded = false;
            }

            return overallSaveSucceeded;
        }

        private bool SaveAllSelectedXmlData()
        {
            CityEventXml mainEventContainer = new CityEventXml();
            bool overallSaveSucceeded = true;

            if (_editorsToSave != null && _editorsToSave.Count > 0)
            {
                List<CityEventXmlContainer> containerList = new List<CityEventXmlContainer>();

                foreach (XmlEditorPanel editor in _editorsToSave)
                {
                    if (editor._container != null)
                    {
                        containerList.Add(editor._container);
                    }
                    else
                    {
                        overallSaveSucceeded = false;
                        break;
                    }
                }

                mainEventContainer._containedEvents = containerList.ToArray();
            }
            else
            {
                overallSaveSucceeded = false;
            }

            if (overallSaveSucceeded)
            {
                try
                {
                    string _eventsPath = _directory + Path.DirectorySeparatorChar + "RushHour Events";
                    string _nameWithoutExtension = Path.GetFileNameWithoutExtension(_fileName.Text);

                    if (_nameWithoutExtension != null && _nameWithoutExtension != "")
                    {
                        string _filePath = _eventsPath + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(_fileName.Text) + ".xml";
                        bool allowSave = true;

                        if(File.Exists(_filePath))
                        {
                            MessageBoxResult choice = MessageBox.Show(this, "This file already exists. Do you want to replace it?", "File already exists", MessageBoxButton.YesNo, MessageBoxImage.Question);

                            if(choice == MessageBoxResult.No)
                            {
                                allowSave = false;
                            }
                        }

                        if (allowSave)
                        {
                            if (!Directory.Exists(_eventsPath))
                            {
                                Directory.CreateDirectory(_eventsPath);
                            }

                            XmlSerializer _xmlSerialiser = new XmlSerializer(typeof(CityEventXml));
                            _xmlSerialiser.Serialize(new FileStream(_filePath, FileMode.Create), mainEventContainer);

                            Process.Start(_eventsPath);
                        }
                        else
                        {
                            overallSaveSucceeded = false;
                        }
                    }
                    else
                    {
                        overallSaveSucceeded = false;
                    }
                }
                catch
                {
                    MessageBox.Show(this, "Couldn't save out to the selected file. Is it read only?", "Failed to write data", MessageBoxButton.OK, MessageBoxImage.Error);
                    overallSaveSucceeded = false;
                }
            }

            return overallSaveSucceeded;
        }
    }
}
