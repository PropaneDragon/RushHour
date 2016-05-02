using CimTools.v2.Attributes;

namespace RushHour.Data
{
    [XmlOptions(XmlOptionsAttribute.OptionType.SaveFile)]
    public static class CityTime
    {
        public static int year = 0;
        public static int month = 0;
        public static int day = 0;
    }
}
