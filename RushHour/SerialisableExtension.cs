using CimTools.V1.File;
using ICities;
using RushHour.CimTools;
using RushHour.Events;
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
            CimToolsHandler.CimToolBase.SaveFileOptions.OnSaveData(serializableDataManager);

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream();

            List<CityEventData> _cityEventData = new List<CityEventData>();

            foreach (CityEvent cityEvent in CityEventManager.instance.m_nextEvents)
            {
                _cityEventData.Add(cityEvent.m_eventData);
            }

            binaryFormatter.Serialize(memoryStream, _cityEventData.ToArray());
            memoryStream.Close();

            serializableDataManager.SaveData(CimToolsHandler.CimToolBase.ModSettings.ModName + "EventData", memoryStream.ToArray());
        }

        public override void OnLoadData()
        {
            CityEventManager _eventManager = CityEventManager.instance;

            CimToolsHandler.CimToolBase.SaveFileOptions.OnLoadData(serializableDataManager);

            bool loaded = true;
            int year = 0, month = 0, day = 0;

            loaded = loaded && CimToolsHandler.CimToolBase.SaveFileOptions.Data.GetValue("CityTimeYear", ref year) == ExportOptionBase.OptionError.NoError;
            loaded = loaded && CimToolsHandler.CimToolBase.SaveFileOptions.Data.GetValue("CityTimeMonth", ref month) == ExportOptionBase.OptionError.NoError;
            loaded = loaded && CimToolsHandler.CimToolBase.SaveFileOptions.Data.GetValue("CityTimeDay", ref day) == ExportOptionBase.OptionError.NoError;

            if(loaded)
            {
                _eventManager.UpdateTime(year, month, day);
            }
            else
            {
                CimToolsHandler.CimToolBase.SaveFileOptions.Data.SetValue("CityTimeYear", CityEventManager.CITY_TIME.Year);
                CimToolsHandler.CimToolBase.SaveFileOptions.Data.SetValue("CityTimeMonth", CityEventManager.CITY_TIME.Month);
                CimToolsHandler.CimToolBase.SaveFileOptions.Data.SetValue("CityTimeDay", CityEventManager.CITY_TIME.Day);
            }

            byte[] deserialisedEventData = serializableDataManager.LoadData(CimToolsHandler.CimToolBase.ModSettings.ModName + "EventData");

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
                        foreach(CityEventData cityEvent in eventData)
                        {
                            CityEvent foundEvent = CityEventBuildings.instance.GetEventFromData(cityEvent);

                            if(foundEvent != null)
                            {
                                CimToolsHandler.CimToolBase.DetailedLogger.Log("Found event - starts: " + foundEvent.m_eventData.m_eventStartTime.ToShortDateString() + ", finishes: " + foundEvent.m_eventData.m_eventFinishTime.ToShortDateString() + ". " + foundEvent.m_eventData.m_registeredCitizens + "/" + foundEvent.GetCapacity() + " registered");
                                Debug.Log("Adding event");
                                _eventManager.m_nextEvents.Add(foundEvent);
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
