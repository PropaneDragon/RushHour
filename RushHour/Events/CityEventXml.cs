using System.Xml.Serialization;

namespace RushHour.Events
{
    [XmlRoot("EventContainer", IsNullable = false)]
    public class CityEventXml
    {
        [XmlArray("Events", IsNullable = false)]
        [XmlArrayItem("Event", IsNullable = false)]
        public CityEventXmlContainer[] _containedEvents = { new CityEventXmlContainer() };
    }
    
    public class CityEventXmlContainer
    {
        [XmlAttribute("EventName")]
        public string _name = "";

        [XmlAttribute("BuildingName")]
        public string _eventBuildingClassName = "";

        [XmlAttribute("Capacity")]
        public int _eventCapacity = 1000;

        [XmlAttribute("LengthInHours")]
        public double _eventLength = 1.5;

        [XmlArray("InitialisedMessages", IsNullable = false)]
        [XmlArrayItem("Message", IsNullable = false)]
        public string[] _initialisedMessages =
        {
            "Variable {0} displays the number of days until the event, eg \"An event in {0}!\" would display as \"An event in 1 day!\", or \"An event in less than a day!\" ingame. Place this where you need it.",
            "Add messages here!"
        };

        [XmlArray("BeginMessages", IsNullable = false)]
        [XmlArrayItem("Message", IsNullable = false)]
        public string[] _beginMessages =
        {
            "Add messages here!"
        };

        [XmlArray("EndMessages", IsNullable = false)]
        [XmlArrayItem("Message", IsNullable = false)]
        public string[] _endedMessages =
        {
            "Add messages here!"
        };

        [XmlElement("ChanceOfAttendingPercentage", IsNullable = false)]
        public CityEventXmlChances _chances = new CityEventXmlChances();
    }

    public class CityEventXmlChances
    {
        [XmlElement("Males", IsNullable = false)]
        public int _males = 0;

        [XmlElement("Females", IsNullable = false)]
        public int _females = 0;

        [XmlElement("Children", IsNullable = false)]
        public int _children = 0;

        [XmlElement("Teens", IsNullable = false)]
        public int _teens = 0;

        [XmlElement("YoungAdults", IsNullable = false)]
        public int _youngAdults = 0;

        [XmlElement("Adults", IsNullable = false)]
        public int _adults = 0;

        [XmlElement("Seniors", IsNullable = false)]
        public int _seniors = 0;

        [XmlElement("LowWealth", IsNullable = false)]
        public int _lowWealth = 0;

        [XmlElement("MediumWealth", IsNullable = false)]
        public int _mediumWealth = 0;

        [XmlElement("HighWealth", IsNullable = false)]
        public int _highWealth = 0;

        [XmlElement("Uneducated", IsNullable = false)]
        public int _uneducated = 0;

        [XmlElement("OneSchool", IsNullable = false)]
        public int _oneSchool = 0;

        [XmlElement("TwoSchools", IsNullable = false)]
        public int _twoSchools = 0;

        [XmlElement("ThreeSchools", IsNullable = false)]
        public int _threeSchools = 0;

        [XmlElement("BadHappiness", IsNullable = false)]
        public int _badHappiness = 0;

        [XmlElement("PoorHappiness", IsNullable = false)]
        public int _poorHappiness = 0;

        [XmlElement("GoodHappiness", IsNullable = false)]
        public int _goodHappiness = 0;

        [XmlElement("ExcellentHappiness", IsNullable = false)]
        public int _excellentHappiness = 0;

        [XmlElement("SuperbHappiness", IsNullable = false)]
        public int _superbHappiness = 0;

        [XmlElement("VeryUnhappyWellbeing", IsNullable = false)]
        public int _veryUnhappyWellbeing = 0;

        [XmlElement("UnhappyWellbeing", IsNullable = false)]
        public int _unhappyWellbeing = 0;

        [XmlElement("SatisfiedWellbeing", IsNullable = false)]
        public int _satisfiedWellbeing = 0;

        [XmlElement("HappyWellbeing", IsNullable = false)]
        public int _happyWellbeing = 0;

        [XmlElement("VeryHappyWellbeing", IsNullable = false)]
        public int _veryHappyWellbeing = 0;
    }
}
