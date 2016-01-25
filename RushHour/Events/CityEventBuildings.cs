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

            switch(thisBuilding.Info.name)
            {
                case "Stadium":
                    buildingEvent = new FootballGame();
                    break;
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
            }

            return buildingEvent;
        }
    }
}
