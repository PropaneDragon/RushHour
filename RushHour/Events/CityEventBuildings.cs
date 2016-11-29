using ColossalFramework;
using RushHour.Events.Unique;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace RushHour.Events
{
    internal class CityEventBuildings
    {
        private static CityEventBuildings m_instance = null;

        public static CityEventBuildings instance
        {
            get
            {
                if(m_instance == null)
                {
                    m_instance = new CityEventBuildings();
                }

                return m_instance;
            }
        }

        public CityEvent GetEventFromData(CityEventData data)
        {
            CityEvent dataEvent = null;

            if (data.m_eventName != "")
            {
                if (data.m_eventName.Substring(0, 9) != "XMLEvent-")
                {
                    try
                    {
                        Type foundType = Assembly.GetExecutingAssembly().GetType(data.m_eventName);

                        if (foundType != null)
                        {
                            dataEvent = Activator.CreateInstance(foundType) as CityEvent;

                            if (dataEvent != null)
                            {
                                dataEvent.m_eventData = data;
                                CimTools.CimToolsHandler.CimToolBase.DetailedLogger.Log("Created a normal event: " + data.m_eventName);
                            }
                        }
                    }
                    catch
                    {
                        dataEvent = null;
                    }
                }
                else
                {
                    CimTools.CimToolsHandler.CimToolBase.DetailedLogger.Log("Found an XML event, rerouting.");
                    dataEvent = GetXmlEventFromData(data);
                }
            }

            return dataEvent;
        }

        public CityEvent GetXmlEventFromData(CityEventData data)
        {
            CityEvent dataEvent = null;

            if (data.m_eventName != "")
            {
                foreach(CityEventXml xmlEvent in CityEventManager.instance.m_xmlEvents)
                {
                    foreach(CityEventXmlContainer containedEvent in xmlEvent._containedEvents)
                    {
                        if("XMLEvent-" + containedEvent._name == data.m_eventName)
                        {
                            dataEvent = new XmlEvent(containedEvent);
                            dataEvent.m_eventData = data;
                            CimTools.CimToolsHandler.CimToolBase.DetailedLogger.Log("Created an XML event: " + data.m_eventName);
                            break;
                        }
                    }

                    if(dataEvent != null)
                    {
                        break;
                    }
                }
            }

            return dataEvent;
        }

        public CityEvent GetEventForBuilding(ref Building thisBuilding, List<CityEventXml> xmlEvents)
        {
            CityEvent _buildingEvent = null;
            List<CityEventXmlContainer> _validEvents = new List<CityEventXmlContainer>();
            SimulationManager _simulationManager = Singleton<SimulationManager>.instance;

            foreach (CityEventXml xmlEvent in xmlEvents)
            {
                foreach(CityEventXmlContainer containedEvent in xmlEvent._containedEvents)
                {
                    if(containedEvent._eventBuildingClassName == thisBuilding.Info.name && containedEvent._supportsRandomEvents)
                    {
                        _validEvents.Add(containedEvent);
                    }
                }
            }

            if(_validEvents.Count > 0)
            {
                CityEventXmlContainer pickedEvent = _validEvents[_simulationManager.m_randomizer.Int32((uint)_validEvents.Count)];

                if(pickedEvent != null)
                {
                    _buildingEvent = new XmlEvent(pickedEvent);
                }
            }
            else
            {
                CimTools.CimToolsHandler.CimToolBase.DetailedLogger.Log("Couldn't find any events for " + thisBuilding.Info.name);
            }

            return _buildingEvent;
        }

        public CityEvent GetEventForBuilding(ref Building thisBuilding)
        {
            CityEvent buildingEvent = null;

            if (!Experiments.ExperimentsToggle.UseXMLEvents)
            {
                switch (thisBuilding.Info.name)
                {
                    //No longer supported
                }
            }
            else
            {
                buildingEvent = GetEventForBuilding(ref thisBuilding, CityEventManager.instance.m_xmlEvents);
            }

            return buildingEvent;
        }

        public FastList<ushort> GetPotentialEventBuildings()
        {
            FastList<ushort> returnedList = new FastList<ushort>();
            BuildingManager _buildingManager = Singleton<BuildingManager>.instance;

            AddBuildingGroupToList(ItemClass.Service.Monument, ref returnedList);
            AddBuildingGroupToList(ItemClass.Service.Beautification, ref returnedList);

            return returnedList;
        }

        public void AddBuildingGroupToList(ItemClass.Service service, ref FastList<ushort> list)
        {
            BuildingManager _buildingManager = Singleton<BuildingManager>.instance;

            if (list != null)
            {
                FastList<ushort> buildings = _buildingManager.GetServiceBuildings(ItemClass.Service.Monument);

                for(int index = 0; index < buildings.m_size; ++index)
                {
                    list.Add(buildings.m_buffer[index]);
                }
            }
        }

        public List<CityEvent> GetEventsForBuilding(ref Building building)
        {
            List<CityEvent> returnEvents = new List<CityEvent>();

            foreach (CityEventXml xmlEvent in CityEventManager.instance.m_xmlEvents)
            {
                foreach (CityEventXmlContainer containedEvent in xmlEvent._containedEvents)
                {
                    if (containedEvent._eventBuildingClassName == building.Info.name)
                    {
                        returnEvents.Add(new XmlEvent(containedEvent));
                    }
                }
            }

            return returnEvents;
        }

        public List<CityEvent> GetUserEventsForBuilding(ref Building building)
        {
            List<CityEvent> returnEvents = new List<CityEvent>();

            foreach (CityEventXml xmlEvent in CityEventManager.instance.m_xmlEvents)
            {
                foreach (CityEventXmlContainer containedEvent in xmlEvent._containedEvents)
                {
                    if (containedEvent._supportUserEvents && containedEvent._eventBuildingClassName == building.Info.name)
                    {
                        returnEvents.Add(new XmlEvent(containedEvent));
                    }
                }
            }

            return returnEvents;
        }

        public bool BuildingHasEvents(ref Building building)
        {
            return GetEventsForBuilding(ref building).Count > 0;
        }

        public bool BuildingHasUserEvents(ref Building building)
        {
            return GetUserEventsForBuilding(ref building).Count > 0;
        }
    }
}
