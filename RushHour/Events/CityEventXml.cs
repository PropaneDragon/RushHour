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

        [XmlAttribute("UserEventName")]
        public string _userEventName = "";

        [XmlAttribute("Capacity")]
        public int _eventCapacity = 1000;

        [XmlAttribute("LengthInHours")]
        public double _eventLength = 1.5;

        [XmlAttribute("Force")]
        public bool _force = false;

        [XmlAttribute("SupportsRandomEvents")]
        public bool _supportsRandomEvents = true;

        [XmlAttribute("SupportsUserEvents")]
        public bool _supportUserEvents = false;

        [XmlAttribute("CanBeWatchedOnTV")]
        public bool _canBeWatchedOnTV = false;

        [XmlArray("InitialisedMessages", IsNullable = false)]
        [XmlArrayItem("Message", IsNullable = false)]
        public string[] _initialisedMessages =
        {
            "Variable {0} displays the number of days until the event, eg \"An event in {0}!\" would display as \"An event in 1 day!\", or \"An event in less than a day!\" in game. Place this where you need it.",
            "Add messages here! Remember to delete these ones!"
        };

        [XmlArray("BeginMessages", IsNullable = false)]
        [XmlArrayItem("Message", IsNullable = false)]
        public string[] _beginMessages =
        {
            "Add messages here! Remember to delete these ones!"
        };

        [XmlArray("EndMessages", IsNullable = false)]
        [XmlArrayItem("Message", IsNullable = false)]
        public string[] _endedMessages =
        {
            "Add messages here! Remember to delete these ones!"
        };

        [XmlElement("ChanceOfAttendingPercentage", IsNullable = false)]
        public CityEventXmlChances _chances = new CityEventXmlChances();

        [XmlElement("Costs", IsNullable = false)]
        public CityEventXmlCosts _costs = new CityEventXmlCosts();

        [XmlArray("Incentives", IsNullable = false)]
        [XmlArrayItem("Incentive", IsNullable = false)]
        public CityEventXmlIncentive[] _incentives = null;
    }

    public class CityEventXmlChances
    {
        [XmlElement("Males", IsNullable = false)]
        public int _males = 100;

        [XmlElement("Females", IsNullable = false)]
        public int _females = 100;

        [XmlElement("Children", IsNullable = false)]
        public int _children = 100;

        [XmlElement("Teens", IsNullable = false)]
        public int _teens = 100;

        [XmlElement("YoungAdults", IsNullable = false)]
        public int _youngAdults = 100;

        [XmlElement("Adults", IsNullable = false)]
        public int _adults = 100;

        [XmlElement("Seniors", IsNullable = false)]
        public int _seniors = 100;

        [XmlElement("LowWealth", IsNullable = false)]
        public int _lowWealth = 60;

        [XmlElement("MediumWealth", IsNullable = false)]
        public int _mediumWealth = 100;

        [XmlElement("HighWealth", IsNullable = false)]
        public int _highWealth = 100;

        [XmlElement("Uneducated", IsNullable = false)]
        public int _uneducated = 100;

        [XmlElement("OneSchool", IsNullable = false)]
        public int _oneSchool = 100;

        [XmlElement("TwoSchools", IsNullable = false)]
        public int _twoSchools = 100;

        [XmlElement("ThreeSchools", IsNullable = false)]
        public int _threeSchools = 100;

        [XmlElement("BadHappiness", IsNullable = false)]
        public int _badHappiness = 40;

        [XmlElement("PoorHappiness", IsNullable = false)]
        public int _poorHappiness = 60;

        [XmlElement("GoodHappiness", IsNullable = false)]
        public int _goodHappiness = 100;

        [XmlElement("ExcellentHappiness", IsNullable = false)]
        public int _excellentHappiness = 100;

        [XmlElement("SuperbHappiness", IsNullable = false)]
        public int _superbHappiness = 100;

        [XmlElement("VeryUnhappyWellbeing", IsNullable = false)]
        public int _veryUnhappyWellbeing = 20;

        [XmlElement("UnhappyWellbeing", IsNullable = false)]
        public int _unhappyWellbeing = 50;

        [XmlElement("SatisfiedWellbeing", IsNullable = false)]
        public int _satisfiedWellbeing = 100;

        [XmlElement("HappyWellbeing", IsNullable = false)]
        public int _happyWellbeing = 100;

        [XmlElement("VeryHappyWellbeing", IsNullable = false)]
        public int _veryHappyWellbeing = 100;
    }

    public class CityEventXmlCosts
    {
        [XmlElement("Creation", IsNullable = false)]
        public float _creation = 100;

        [XmlElement("PerHead", IsNullable = false)]
        public float _perHead = 5;

        [XmlElement("AdvertisingSigns", IsNullable = false)]
        public float _advertisingSigns = 20000;

        [XmlElement("AdvertisingTV", IsNullable = false)]
        public float _advertisingTV = 5000;

        [XmlElement("EntryCost", IsNullable = false)]
        public float _entry = 10;
    }

    public class CityEventXmlIncentive
    {
        [XmlAttribute("Name")]
        public string _name = "";

        [XmlAttribute("Cost")]
        public float _cost = 3;

        [XmlAttribute("ReturnCost")]
        public float _returnCost = 10;

        [XmlAttribute("ActiveWhenRandomEvent")]
        public bool _activeWhenRandomEvent = false;

        [XmlElement("Description", IsNullable = false)]
        public string _description = "";

        [XmlElement("PositiveEffect", IsNullable = false)]
        public int _positiveEffect = 10;

        [XmlElement("NegativeEffect", IsNullable = false)]
        public int _negativeEffect = 10;
    }
}
