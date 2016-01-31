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
            serializableDataManager.SaveData(CimToolsHandler.CimToolBase.ModSettings.ModName + "EventData", memoryStream.ToArray());

            memoryStream.Close();
        }

        public override void OnLoadData()
        {
            CimToolsHandler.CimToolBase.SaveFileOptions.OnLoadData(serializableDataManager);

            byte[] deserialisedData = serializableDataManager.LoadData(CimToolsHandler.CimToolBase.ModSettings.ModName + "EventData");

            if (deserialisedData != null)
            {
                MemoryStream memoryStream = new MemoryStream();
                memoryStream.Write(deserialisedData, 0, deserialisedData.Length);
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
                                Debug.Log("Adding event");
                                CityEventManager.instance.m_nextEvents.Add(foundEvent);
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
