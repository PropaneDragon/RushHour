using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml.Serialization;
using RushHour.Events;
using System.IO;

namespace RushHourTests
{
    [TestClass]
    public class RushHourTest
    {
        [TestMethod]
        public void ExportXML()
        {
            XmlSerializer _serialiser = new XmlSerializer(typeof(CityEventXml));
            TextWriter _xmlWriter = new StreamWriter("exportedXML.xml");
            
            _serialiser.Serialize(_xmlWriter, new CityEventXml());
        }
    }
}
