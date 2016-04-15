using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml.Serialization;
using RushHour.Events;
using System.IO;

namespace RushHourTests
{
    [TestClass]
    public class EventsTest
    {
        [TestMethod]
        public void ExportXML()
        {
            XmlSerializer _serialiser = new XmlSerializer(typeof(CityEventXml));
            TextWriter _xmlWriter = new StreamWriter("exportedXML.xml");

            _serialiser.Serialize(_xmlWriter, new CityEventXml());
            _xmlWriter.Close();
        }

        [TestMethod]
        public void ParseOldXMLFormat()
        {
            XmlSerializer _serialiser = new XmlSerializer(typeof(CityEventXml));

            string toParse = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                                "<EventContainer xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\n" +
                                "  <Events>\n" +
                                "    <Event EventName=\"\" BuildingName=\"\" Capacity=\"1000\" LengthInHours=\"1.5\" Force=\"false\">\n" +
                                "      <InitialisedMessages>\n" +
                                "        <Message>Variable {0} displays the number of days until the event, eg \"An event in {0}!\" would display as \"An event in 1 day!\", or \"An event in less than a day!\" ingame. Place this where you need it.</Message>\n" +
                                "        <Message>Add messages here!</Message>\n" +
                                "      </InitialisedMessages>\n" +
                                "      <BeginMessages>\n" +
                                "        <Message>Add messages here!</Message>\n" +
                                "      </BeginMessages>\n" +
                                "      <EndMessages>\n" +
                                "        <Message>Add messages here!</Message>\n" +
                                "      </EndMessages>\n" +
                                "      <ChanceOfAttendingPercentage>\n" +
                                "        <Males>0</Males>\n" +
                                "        <Females>0</Females>\n" +
                                "        <Children>0</Children>\n" +
                                "        <Teens>0</Teens>\n" +
                                "        <YoungAdults>0</YoungAdults>\n" +
                                "        <Adults>0</Adults>\n" +
                                "        <Seniors>0</Seniors>\n" +
                                "        <LowWealth>0</LowWealth>\n" +
                                "        <MediumWealth>0</MediumWealth>\n" +
                                "        <HighWealth>0</HighWealth>\n" +
                                "        <Uneducated>0</Uneducated>\n" +
                                "        <OneSchool>0</OneSchool>\n" +
                                "        <TwoSchools>0</TwoSchools>\n" +
                                "        <ThreeSchools>0</ThreeSchools>\n" +
                                "        <BadHappiness>0</BadHappiness>\n" +
                                "        <PoorHappiness>0</PoorHappiness>\n" +
                                "        <GoodHappiness>0</GoodHappiness>\n" +
                                "        <ExcellentHappiness>0</ExcellentHappiness>\n" +
                                "        <SuperbHappiness>0</SuperbHappiness>\n" +
                                "        <VeryUnhappyWellbeing>0</VeryUnhappyWellbeing>\n" +
                                "        <UnhappyWellbeing>0</UnhappyWellbeing>\n" +
                                "        <SatisfiedWellbeing>0</SatisfiedWellbeing>\n" +
                                "        <HappyWellbeing>0</HappyWellbeing>\n" +
                                "        <VeryHappyWellbeing>0</VeryHappyWellbeing>\n" +
                                "      </ChanceOfAttendingPercentage>\n" +
                                "    </Event>\n" +
                                "  </Events>\n" +
                                "</EventContainer>\n";

            StringReader _xmlReader = new StringReader(toParse);
            CityEventXml xmlEvent = _serialiser.Deserialize(_xmlReader) as CityEventXml;

            Assert.IsNotNull(xmlEvent);
            Assert.IsNotNull(xmlEvent._containedEvents[0]);
            Assert.IsFalse(xmlEvent._containedEvents[0]._canBeWatchedOnTV);
            Assert.IsFalse(xmlEvent._containedEvents[0]._supportUserEvents);
        }

        [TestMethod]
        public void ParseXMLEvent()
        {
            XmlSerializer _serialiser = new XmlSerializer(typeof(CityEventXml));

            string toParse =    "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                                "<EventContainer xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\n" +
                                "  <Events>\n" +
                                "    <Event EventName=\"testEvent\" BuildingName=\"testBuilding\" Capacity=\"4321\" LengthInHours=\"12.3\" Force=\"true\" SupportsUserEvents=\"true\" CanBeWatchedOnTV=\"true\">\n" +
                                "      <InitialisedMessages>\n" +
                                "        <Message>Variable {0} displays the number of days until the event, eg \"An event in {0}!\" would display as \"An event in 1 day!\", or \"An event in less than a day!\" ingame. Place this where you need it.</Message>\n" +
                                "        <Message>Add messages here!</Message>\n" +
                                "        <Message>Add messages here!</Message>\n" +
                                "        <Message>Add messages here!</Message>\n" +
                                "      </InitialisedMessages>\n" +
                                "      <BeginMessages>\n" +
                                "        <Message>Add messages here!</Message>\n" +
                                "        <Message>Add messages here!</Message>\n" +
                                "      </BeginMessages>\n" +
                                "      <EndMessages>\n" +
                                "        <Message>Add messages here!</Message>\n" +
                                "        <Message>Add messages here!</Message>\n" +
                                "        <Message>Add messages here!</Message>\n" +
                                "      </EndMessages>\n" +
                                "      <ChanceOfAttendingPercentage>\n" +
                                "        <Males>1</Males>\n" +
                                "        <Females>2</Females>\n" +
                                "        <Children>3</Children>\n" +
                                "        <Teens>4</Teens>\n" +
                                "        <YoungAdults>5</YoungAdults>\n" +
                                "        <Adults>6</Adults>\n" +
                                "        <Seniors>7</Seniors>\n" +
                                "        <LowWealth>8</LowWealth>\n" +
                                "        <MediumWealth>9</MediumWealth>\n" +
                                "        <HighWealth>10</HighWealth>\n" +
                                "        <Uneducated>11</Uneducated>\n" +
                                "        <OneSchool>12</OneSchool>\n" +
                                "        <TwoSchools>13</TwoSchools>\n" +
                                "        <ThreeSchools>14</ThreeSchools>\n" +
                                "        <BadHappiness>15</BadHappiness>\n" +
                                "        <PoorHappiness>16</PoorHappiness>\n" +
                                "        <GoodHappiness>17</GoodHappiness>\n" +
                                "        <ExcellentHappiness>18</ExcellentHappiness>\n" +
                                "        <SuperbHappiness>19</SuperbHappiness>\n" +
                                "        <VeryUnhappyWellbeing>20</VeryUnhappyWellbeing>\n" +
                                "        <UnhappyWellbeing>21</UnhappyWellbeing>\n" +
                                "        <SatisfiedWellbeing>22</SatisfiedWellbeing>\n" +
                                "        <HappyWellbeing>23</HappyWellbeing>\n" +
                                "        <VeryHappyWellbeing>24</VeryHappyWellbeing>\n" +
                                "      </ChanceOfAttendingPercentage>\n" +
                                "      <Costs>\n" +
                                "        <Creation>1</Creation>\n" +
                                "        <PerHead>2</PerHead>\n" +
                                "        <AdvertisingSigns>3</AdvertisingSigns>\n" +
                                "        <AdvertisingTV>4</AdvertisingTV>\n" +
                                "        <EntryCost>5</EntryCost>\n" +
                                "      </Costs>\n" +
                                "      <Incentives>\n" +
                                "        <Incentive Name=\"testIncentive\" Cost=\"123\" ReturnCost=\"456\">\n" +
                                "          <Description>Test description.</Description>\n" +
                                "          <PositiveEffect>3</PositiveEffect>\n" +
                                "          <NegativeEffect>5</NegativeEffect>\n" +
                                "        </Incentive>\n" +
                                "      </Incentives>\n" +
                                "    </Event>\n" +
                                "  </Events>\n" +
                                "</EventContainer>\n";

            StringReader _xmlReader = new StringReader(toParse);
            CityEventXml xmlEvent = _serialiser.Deserialize(_xmlReader) as CityEventXml;

            Assert.IsNotNull(xmlEvent);

            CityEventXmlContainer currentContainer = xmlEvent._containedEvents[0];
            Assert.IsNotNull(currentContainer);
            Assert.AreEqual("testEvent", currentContainer._name);
            Assert.AreEqual("testBuilding", currentContainer._eventBuildingClassName);
            Assert.AreEqual(4321, currentContainer._eventCapacity);
            Assert.AreEqual(12.3, currentContainer._eventLength);
            Assert.IsTrue(currentContainer._force);
            Assert.IsTrue(currentContainer._canBeWatchedOnTV);
            Assert.IsTrue(currentContainer._supportUserEvents);

            Assert.AreEqual(4, currentContainer._initialisedMessages.Length);
            Assert.AreEqual(2, currentContainer._beginMessages.Length);
            Assert.AreEqual(3, currentContainer._endedMessages.Length);

            Assert.AreEqual(1, currentContainer._chances._males);
            Assert.AreEqual(2, currentContainer._chances._females);
            Assert.AreEqual(3, currentContainer._chances._children);
            Assert.AreEqual(4, currentContainer._chances._teens);
            Assert.AreEqual(5, currentContainer._chances._youngAdults);
            Assert.AreEqual(6, currentContainer._chances._adults);
            Assert.AreEqual(7, currentContainer._chances._seniors);
            Assert.AreEqual(8, currentContainer._chances._lowWealth);
            Assert.AreEqual(9, currentContainer._chances._mediumWealth);
            Assert.AreEqual(10, currentContainer._chances._highWealth);
            Assert.AreEqual(11, currentContainer._chances._uneducated);
            Assert.AreEqual(12, currentContainer._chances._oneSchool);
            Assert.AreEqual(13, currentContainer._chances._twoSchools);
            Assert.AreEqual(14, currentContainer._chances._threeSchools);
            Assert.AreEqual(15, currentContainer._chances._badHappiness);
            Assert.AreEqual(16, currentContainer._chances._poorHappiness);
            Assert.AreEqual(17, currentContainer._chances._goodHappiness);
            Assert.AreEqual(18, currentContainer._chances._excellentHappiness);
            Assert.AreEqual(19, currentContainer._chances._superbHappiness);
            Assert.AreEqual(20, currentContainer._chances._veryUnhappyWellbeing);
            Assert.AreEqual(21, currentContainer._chances._unhappyWellbeing);
            Assert.AreEqual(22, currentContainer._chances._satisfiedWellbeing);
            Assert.AreEqual(23, currentContainer._chances._happyWellbeing);
            Assert.AreEqual(24, currentContainer._chances._veryHappyWellbeing);

            Assert.AreEqual(1, currentContainer._costs._creation);
            Assert.AreEqual(2, currentContainer._costs._perHead);
            Assert.AreEqual(3, currentContainer._costs._advertisingSigns);
            Assert.AreEqual(4, currentContainer._costs._advertisingTV);
            Assert.AreEqual(5, currentContainer._costs._entry);

            Assert.IsNotNull(currentContainer._incentives);
            Assert.IsNotNull(currentContainer._incentives[0]);

            CityEventXmlIncentive currentIncentive = currentContainer._incentives[0];

            Assert.AreEqual("testIncentive", currentIncentive._name);
            Assert.AreEqual(123, currentIncentive._cost);
            Assert.AreEqual(456, currentIncentive._returnCost);
            Assert.AreEqual("Test description.", currentIncentive._description);
            Assert.AreEqual(3, currentIncentive._positiveEffect);
            Assert.AreEqual(5, currentIncentive._negativeEffect);
        }
    }
}