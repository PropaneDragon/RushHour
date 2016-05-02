using ICities;
using RushHour.CimToolsHandler;
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
            CimToolsHandler.CimToolsHandler.CimToolBase.SaveFileOptions.SaveData(serializableDataManager);

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream();

            List<CityEventData> _cityEventData = new List<CityEventData>();

            foreach (CityEvent cityEvent in CityEventManager.instance.m_nextEvents)
            {
                _cityEventData.Add(cityEvent.m_eventData);
            }

            binaryFormatter.Serialize(memoryStream, _cityEventData.ToArray());
            memoryStream.Close();

            serializableDataManager.SaveData(CimToolsHandler.CimToolsHandler.CimToolBase.ModSettings.ModName + "EventData", memoryStream.ToArray());
        }

        public override void OnLoadData()
        {
            CityEventManager _eventManager = CityEventManager.instance;

            CimToolsHandler.CimToolsHandler.CimToolBase.SaveFileOptions.LoadData(serializableDataManager);

            bool loaded = true;

            loaded = Data.CityTime.day != 0 && Data.CityTime.month != 0 && Data.CityTime.year != 0;

            if(loaded)
            {
                _eventManager.UpdateTime();
            }
            else
            {
                Data.CityTime.year = CityEventManager.CITY_TIME.Year;
                Data.CityTime.month = CityEventManager.CITY_TIME.Month;
                Data.CityTime.day = CityEventManager.CITY_TIME.Day;
            }

            byte[] deserialisedEventData = serializableDataManager.LoadData(CimToolsHandler.CimToolsHandler.CimToolBase.ModSettings.ModName + "EventData");

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
                                CimToolsHandler.CimToolsHandler.CimToolBase.DetailedLogger.Log("Found event - starts: " + foundEvent.m_eventData.m_eventStartTime.ToShortDateString() + ", finishes: " + foundEvent.m_eventData.m_eventFinishTime.ToShortDateString() + ". " + foundEvent.m_eventData.m_registeredCitizens + "/" + foundEvent.GetCapacity() + " registered");
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
