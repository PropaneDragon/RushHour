using RushHour.Events.Unique;

namespace RushHour.Events
{
    public class CityEventBuildings
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

        public CityEvent GetEventForBuilding(ref Building thisBuilding)
        {
            CityEvent buildingEvent = null;

            switch(thisBuilding.Info.name)
            {
                case "Stadium":
                    buildingEvent = new FootballGame();
                    break;
                /*case "Modern Art Museum":
                    buildingEvents.Add(CityEventOld.CityEventType.ArtExhibit);
                    break;
                case "Grand Mall":
                    buildingEvents.Add(CityEventOld.CityEventType.ShopOpening);
                    break;
                case "Library":
                    buildingEvents.Add(CityEventOld.CityEventType.BookSigning);
                    break;*/
            }

            return buildingEvent;
        }
    }
}
