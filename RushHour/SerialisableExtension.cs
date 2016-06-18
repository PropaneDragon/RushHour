using ICities;
using RushHour.Events;
using RushHour.Experiments;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace RushHour
{
    public class SerialisableExtension : SerializableDataExtensionBase
    {
        public override void OnSaveData()
        {
            if (!ExperimentsToggle.GhostMode)
            {
                SaveCimToolsData();
                SaveEventData();
            }
        }

        public override void OnLoadData()
        {
            if (!ExperimentsToggle.GhostMode)
            {
                LoadCimToolsData();
                LoadEventData();
            }
        }

        private void SaveCimToolsData()
        {
            CimTools.CimToolsHandler.CimToolBase.SaveFileOptions.SaveData(serializableDataManager);
        }

        private void LoadCimToolsData()
        {
            CimTools.CimToolsHandler.CimToolBase.DetailedLogger.Log("Loading up save file data");

            CityEventManager eventManager = CityEventManager.instance;

            CimTools.CimToolsHandler.CimToolBase.SaveFileOptions.LoadData(serializableDataManager);

            bool loaded = Data.CityTime.day != 0 && Data.CityTime.month != 0 && Data.CityTime.year != 0;

            if (loaded)
            {
                CimTools.CimToolsHandler.CimToolBase.DetailedLogger.Log("Time loaded from new data");
                eventManager.UpdateTime();
            }
            else
            {
                CimTools.CimToolsHandler.CimToolBase.DetailedLogger.Log("Time is legacy data");
                LoadLegacyTimeData();
            }
        }

        private void LoadLegacyTimeData()
        {
            CityEventManager eventManager = CityEventManager.instance;

            CimTools.CimToolsHandler.LegacyCimToolBase.SaveFileOptions.LoadData(serializableDataManager);

            CimTools.CimToolsHandler.LegacyCimToolBase.SaveFileOptions.Data.GetValue("CityTimeDay", ref Data.CityTime.day);
            CimTools.CimToolsHandler.LegacyCimToolBase.SaveFileOptions.Data.GetValue("CityTimeMonth", ref Data.CityTime.month);
            CimTools.CimToolsHandler.LegacyCimToolBase.SaveFileOptions.Data.GetValue("CityTimeYear", ref Data.CityTime.year);

            bool loaded = Data.CityTime.day != 0 && Data.CityTime.month != 0 && Data.CityTime.year != 0;

            if (loaded)
            {
                eventManager.UpdateTime();
                CimTools.CimToolsHandler.CimToolBase.DetailedLogger.Log("Loaded legacy date data. Time: " + Data.CityTime.day + "/" + Data.CityTime.month + "/" + Data.CityTime.year);
            }
            else
            {
                Data.CityTime.year = CityEventManager.CITY_TIME.Year;
                Data.CityTime.month = CityEventManager.CITY_TIME.Month;
                Data.CityTime.day = CityEventManager.CITY_TIME.Day;

                CimTools.CimToolsHandler.CimToolBase.DetailedLogger.Log("Initially setting up time: " + Data.CityTime.day + "/" + Data.CityTime.month + "/" + Data.CityTime.year);
            }
        }

        private void SaveEventData()
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream();

            List<CityEventData> _cityEventData = new List<CityEventData>();

            foreach (CityEvent cityEvent in CityEventManager.instance.m_nextEvents)
            {
                _cityEventData.Add(cityEvent.m_eventData);
            }

            binaryFormatter.Serialize(memoryStream, _cityEventData.ToArray());
            memoryStream.Close();

            serializableDataManager.SaveData(CimTools.CimToolsHandler.CimToolBase.ModSettings.ModName + "EventData", memoryStream.ToArray());
        }

        private void LoadEventData()
        {
            CityEventManager eventManager = CityEventManager.instance;

            byte[] deserialisedEventData = serializableDataManager.LoadData(CimTools.CimToolsHandler.CimToolBase.ModSettings.ModName + "EventData");

            if (deserialisedEventData != null)
            {
                MemoryStream memoryStream = new MemoryStream();
                memoryStream.Write(deserialisedEventData, 0, deserialisedEventData.Length);
                memoryStream.Position = 0;

                BinaryFormatter binaryFormatter = new BinaryFormatter();

                try
                {
                    CityEventData[] eventData = binaryFormatter.Deserialize(memoryStream) as CityEventData[];

                    if (eventData != null)
                    {
                        foreach (CityEventData cityEvent in eventData)
                        {
                            CityEvent foundEvent = CityEventBuildings.instance.GetEventFromData(cityEvent);

                            if (foundEvent != null)
                            {
                                CimTools.CimToolsHandler.CimToolBase.DetailedLogger.Log("Found event - starts: " + foundEvent.m_eventData.m_eventStartTime.ToShortDateString() + ", finishes: " + foundEvent.m_eventData.m_eventFinishTime.ToShortDateString() + ". " + foundEvent.m_eventData.m_registeredCitizens + "/" + foundEvent.GetCapacity() + " registered");
                                Debug.Log("Adding event");
                                eventManager.m_nextEvents.Add(foundEvent);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log("Failed to load events");
                    Debug.LogException(ex);
                }
                finally
                {
                    memoryStream.Close();
                }
            }
        }
    }
}
