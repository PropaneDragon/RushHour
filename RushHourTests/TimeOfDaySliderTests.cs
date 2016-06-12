using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RushHour.UI;

namespace RushHourTests
{
    [TestClass]
    public class TimeOfDaySliderTests
    {
        /*[TestMethod]
        public void TestGetAndSetValue()
        {
            // Commented lines throw System.Security.SecurityException: ECall methods must be packaged into a system module.
            // because we use UnityEngine underneath :( - I think we need to rewrite components for better testability with
            // less reliance on Unity stuff where possible, or using DI so we can pass in a stub to use.
            //UIHelperBase _base = new UIHelperBaseStub();
            TimeOfDaySlider _slider = new TimeOfDaySlider();

            //_slider.Create(_base);

            _slider.value = 5;
            Assert.AreEqual(5f, _slider.value);
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void TestGetValueThrowsIfNotSet()
        {
            TimeOfDaySlider _slider = new TimeOfDaySlider();
            Assert.AreEqual("12", _slider.value);
        }

        [TestMethod]
        public void TestToStringReturnsClassName()
        {
            TimeOfDaySlider _slider = new TimeOfDaySlider();
            Assert.AreEqual("RushHour.UI.TimeOfDaySlider", _slider.ToString());
        }

        [TestMethod]
        public void TestGetTimeFromFloatingValueDetectsEarlyMorning()
        {
            PrivateObject privateHelperObject = new PrivateObject(typeof(TimeOfDaySlider));
            float newValue = 3f;
            string expectedValue = "3:00 am";

            string actualValue = (string)privateHelperObject.Invoke("getTimeFromFloatingValue", newValue);
            Assert.AreEqual(expectedValue, actualValue);
        }

        [TestMethod]
        public void TestGetTimeFromFloatingValueDetectsExtremelyEarlyMorning()
        {
            PrivateObject privateHelperObject = new PrivateObject(typeof(TimeOfDaySlider));
            float newValue = 0.17f;
            string expectedValue = "12:10 am";

            string actualValue = (string)privateHelperObject.Invoke("getTimeFromFloatingValue", newValue);
            Assert.AreEqual(expectedValue, actualValue);
        }

        [TestMethod]
        public void TestGetTimeFromFloatingValueDetectsNoonAkaLunchTime()
        {
            PrivateObject privateHelperObject = new PrivateObject(typeof(TimeOfDaySlider));
            float newValue = 12.0f;
            string expectedValue = "12:00 pm";

            string actualValue = (string)privateHelperObject.Invoke("getTimeFromFloatingValue", newValue);
            Assert.AreEqual(expectedValue, actualValue);
        }

        [TestMethod]
        public void TestGetTimeFromFloatingValueDetectsExtremelyLateNight()
        {
            PrivateObject privateHelperObject = new PrivateObject(typeof(TimeOfDaySlider));
            float newValue = 23.8f;
            string expectedValue = "11:47 pm";

            string actualValue = (string)privateHelperObject.Invoke("getTimeFromFloatingValue", newValue);
            Assert.AreEqual(expectedValue, actualValue);
        }

        [TestMethod]
        public void TestGetTimeFromFloatingValueDetectsMostExtremeLateNight()
        {
            PrivateObject privateHelperObject = new PrivateObject(typeof(TimeOfDaySlider));
            float newValue = 23.9f;
            string expectedValue = "11:53 pm";

            string actualValue = (string)privateHelperObject.Invoke("getTimeFromFloatingValue", newValue);
            Assert.AreEqual(expectedValue, actualValue);
            
            newValue = 23.999f;
            expectedValue = "11:59 pm";

            actualValue = (string)privateHelperObject.Invoke("getTimeFromFloatingValue", newValue);
            Assert.AreEqual(expectedValue, actualValue);
        }

        [TestMethod]
        public void TestGetTimeFromFloatingValueRounds()
        {
            PrivateObject privateHelperObject = new PrivateObject(typeof(TimeOfDaySlider));
            float newValue = 12.49999f;
            string expectedValue = "12:29 pm";

            string actualValue = (string)privateHelperObject.Invoke("getTimeFromFloatingValue", newValue);
            Assert.AreEqual(expectedValue, actualValue);

            newValue = 12.49999999999999f;
            expectedValue = "12:30 pm";

            actualValue = (string)privateHelperObject.Invoke("getTimeFromFloatingValue", newValue);
            Assert.AreEqual(expectedValue, actualValue);

            newValue = 12.500000f;
            expectedValue = "12:30 pm";

            actualValue = (string)privateHelperObject.Invoke("getTimeFromFloatingValue", newValue);
            Assert.AreEqual(expectedValue, actualValue);

            newValue = 12.500111f;
            expectedValue = "12:30 pm";

            actualValue = (string)privateHelperObject.Invoke("getTimeFromFloatingValue", newValue);
            Assert.AreEqual(expectedValue, actualValue);

            newValue = 12.511111f;
            expectedValue = "12:30 pm";

            actualValue = (string)privateHelperObject.Invoke("getTimeFromFloatingValue", newValue);
            Assert.AreEqual(expectedValue, actualValue);

            newValue = 12.516f;
            expectedValue = "12:30 pm";

            actualValue = (string)privateHelperObject.Invoke("getTimeFromFloatingValue", newValue);
            Assert.AreEqual(expectedValue, actualValue);

            newValue = 12.517f;
            expectedValue = "12:31 pm";

            actualValue = (string)privateHelperObject.Invoke("getTimeFromFloatingValue", newValue);
            Assert.AreEqual(expectedValue, actualValue);
        }*/
    }
}
