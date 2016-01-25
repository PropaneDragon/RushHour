using ColossalFramework;
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

            switch (thisBuilding.Info.name)
            {
                case "Modern Art Museum":
                    buildingEvent = new ArtExhibit();
                    break;
                case "Grand Mall":
                    buildingEvent = new ShopOpening();
                    break;
                case "Library":
                    buildingEvent = new BookSigning();
                    break;
                case "ExpoCenter":
                    switch (Singleton<SimulationManager>.instance.m_randomizer.Int32(5))
                    {
                        case 0:
                            buildingEvent = new BusinessExpo();
                            break;
                        case 1:
                            buildingEvent = new CaravanExpo();
                            break;
                        case 2:
                            buildingEvent = new ComicExpo();
                            break;
                        case 3:
                            buildingEvent = new ElectronicExpo();
                            break;
                        case 4:
                            buildingEvent = new GameExpo();
                            break;
                    }
                    break;
                case "Stadium":
                    buildingEvent = new FootballGame();
                    break;
                case "Opera House":
                    buildingEvent = new OperaEvent();
                    break;
                case "Posh Mall":
                    buildingEvent = new ShopOpening();
                    break;
                case "Observatory":
                    //coming soon
                    break;
                case "Official Park":
                    //buildingEvent = new Memorial();
                    break;
                case "Theater of Wonders":
                    buildingEvent = new TheaterEvent();
                    break;
                case "Trash Mall":
                    buildingEvent = new ShopOpening();
                    break;
                case "SeaWorld":
                    buildingEvent = new AquariumEvent();
                    break;
            }

            return buildingEvent;
        }
    }
}
