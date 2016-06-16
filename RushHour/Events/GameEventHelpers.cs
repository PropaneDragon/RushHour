using ColossalFramework;
using System;

namespace RushHour.Events
{
    internal class GameEventHelpers
    {
        public static bool EventStartsWithin(EventData eventData, double hours)
        {
            SimulationManager simulationManager = Singleton<SimulationManager>.instance;

            bool eventStartsSoon = false;
            bool created = (eventData.m_flags & EventData.Flags.Created) != EventData.Flags.None;
            bool started = (eventData.m_flags & EventData.Flags.Active) != EventData.Flags.None;
            bool ended = (eventData.m_flags & EventData.Flags.Completed) != EventData.Flags.None;

            if (created && !started && !ended)
            {
                DateTime startTime = simulationManager.FrameToTime(eventData.m_startFrame);

                TimeSpan difference = startTime - CityEventManager.CITY_TIME;
                eventStartsSoon = difference.TotalHours > 0 && difference.TotalHours <= hours;
            }

            return eventStartsSoon;
        }

        public static bool EventTakingPlace()
        {
            EventManager eventManager = Singleton<EventManager>.instance;

            bool eventTakingPlace = false;

            for(int index = 0; index < eventManager.m_events.m_size; ++index)
            {
                EventData eventData = eventManager.m_events.m_buffer[index];

                bool created = (eventData.m_flags & EventData.Flags.Created) != EventData.Flags.None;
                bool started = (eventData.m_flags & EventData.Flags.Active) != EventData.Flags.None;
                bool ended = (eventData.m_flags & EventData.Flags.Completed) != EventData.Flags.None;

                if (created && started && !ended)
                {
                    eventTakingPlace = true;
                    break;
                }
            }

            return eventTakingPlace;
        }

        public static bool EventTakingPlace(ushort building)
        {
            EventManager eventManager = Singleton<EventManager>.instance;
            BuildingManager buildingManager = Singleton<BuildingManager>.instance;

            ushort eventId = buildingManager.m_buildings.m_buffer[building].m_eventIndex;
            bool eventTakingPlace = false;

            if (eventId != 0)
            {
                eventTakingPlace = (eventManager.m_events.m_buffer[eventId].m_flags & (EventData.Flags.Preparing | EventData.Flags.Active)) != EventData.Flags.None;
            }

            return eventTakingPlace;
        }
    }
}
