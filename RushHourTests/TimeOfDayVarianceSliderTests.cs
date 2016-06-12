using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RushHour.UI;

namespace RushHourTests
{
    [TestClass]
    public class TimeOfDayVarianceSliderTests
    {
        /*[TestMethod]
        public void TestGetAndSetValue()
        {
            // Commented lines throw System.Security.SecurityException: ECall methods must be packaged into a system module.
            // because we use UnityEngine underneath :( - I think we need to rewrite components for better testability with
            // less reliance on Unity stuff where possible, or using DI so we can pass in a stub to use.
            //UIHelperBase _base = new UIHelperBaseStub();
            TimeOfDayVarianceSlider _slider = new TimeOfDayVarianceSlider();

            //_slider.Create(_base);

            _slider.value = 5;
            Assert.AreEqual(5f, _slider.value);
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void TestGetValueThrowsIfNotSet()
        {
            TimeOfDayVarianceSlider _slider = new TimeOfDayVarianceSlider();
            Assert.AreEqual("12", _slider.value);
        }

        [TestMethod]
        public void TestToStringReturnsClassName()
        {
            TimeOfDayVarianceSlider _slider = new TimeOfDayVarianceSlider();
            Assert.AreEqual("RushHour.UI.TimeOfDayVarianceSlider", _slider.ToString());
        }

        [TestMethod]
        public void TestGetTimeFromFloatingValueDetectsEarlyMorning()
        {
            PrivateObject privateHelperObject = new PrivateObject(typeof(TimeOfDayVarianceSlider));
            float newValue = 3f;
            string expectedValue = "3 hours";

            string actualValue = (string)privateHelperObject.Invoke("getVarianceTimeFromFloatingValue", newValue);
            Assert.AreEqual(expectedValue, actualValue);
        }

        [TestMethod]
        public void TestGetTimeFromFloatingValueDetectsExtremelyEarlyMorning()
        {
            PrivateObject privateHelperObject = new PrivateObject(typeof(TimeOfDayVarianceSlider));
            float newValue = 0.17f;
            string expectedValue = "10 minutes";

            string actualValue = (string)privateHelperObject.Invoke("getVarianceTimeFromFloatingValue", newValue);
            Assert.AreEqual(expectedValue, actualValue);
        }
        
        [TestMethod]
        public void TestGetTimeFromFloatingValueDoesNotWrapGreaterThanHours()
        {
            PrivateObject privateHelperObject = new PrivateObject(typeof(TimeOfDayVarianceSlider));
            float newValue = 123.999f;
            string expectedValue = "123 hours and 59 minutes";

            string actualValue = (string)privateHelperObject.Invoke("getVarianceTimeFromFloatingValue", newValue);
            Assert.AreEqual(expectedValue, actualValue);
        }

        [TestMethod]
        public void TestGetTimeFromFloatingValueRounds()
        {
            PrivateObject privateHelperObject = new PrivateObject(typeof(TimeOfDayVarianceSlider));
            float newValue = 12.49999f;
            string expectedValue = "12 hours and 29 minutes";

            string actualValue = (string)privateHelperObject.Invoke("getVarianceTimeFromFloatingValue", newValue);
            Assert.AreEqual(expectedValue, actualValue);

            newValue = 12.49999999999999f;
            expectedValue = "12 hours and 30 minutes";

            actualValue = (string)privateHelperObject.Invoke("getVarianceTimeFromFloatingValue", newValue);
            Assert.AreEqual(expectedValue, actualValue);

            newValue = 12.500000f;
            expectedValue = "12 hours and 30 minutes";

            actualValue = (string)privateHelperObject.Invoke("getVarianceTimeFromFloatingValue", newValue);
            Assert.AreEqual(expectedValue, actualValue);

            newValue = 12.500111f;
            expectedValue = "12 hours and 30 minutes";

            actualValue = (string)privateHelperObject.Invoke("getVarianceTimeFromFloatingValue", newValue);
            Assert.AreEqual(expectedValue, actualValue);

            newValue = 12.511111f;
            expectedValue = "12 hours and 30 minutes";

            actualValue = (string)privateHelperObject.Invoke("getVarianceTimeFromFloatingValue", newValue);
            Assert.AreEqual(expectedValue, actualValue);

            newValue = 12.516f;
            expectedValue = "12 hours and 30 minutes";

            actualValue = (string)privateHelperObject.Invoke("getVarianceTimeFromFloatingValue", newValue);
            Assert.AreEqual(expectedValue, actualValue);

            newValue = 12.517f;
            expectedValue = "12 hours and 31 minutes";

            actualValue = (string)privateHelperObject.Invoke("getVarianceTimeFromFloatingValue", newValue);
            Assert.AreEqual(expectedValue, actualValue);
        }*/
    }
}
