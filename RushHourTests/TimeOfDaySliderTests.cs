using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RushHour.UI;
using CimTools.V1.Utilities;
using ICities;
using RushHourTests.MockClasses;

namespace RushHourTests
{
    [TestClass]
    public class TimeOfDaySliderTests
    {
        [TestMethod]
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
    }

}
