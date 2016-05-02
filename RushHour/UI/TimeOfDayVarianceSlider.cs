using ColossalFramework.UI;
using ICities;
using System;
using CimTools.v2.Utilities;

namespace RushHour.UI
{
    internal class TimeOfDayVarianceSlider : OptionsItemBase
    {
        public float min = 0.0f;

        public float max = 23.9f;

        public float step = 0.25f;

        public float value
        {
            get
            {
                float? convertedValue = Convert.ChangeType(m_value, typeof(float)) as float?;
                return convertedValue.HasValue ? convertedValue.Value : 0f;
            }
            set
            {
                m_value = value;
            }
        }

        private const float one_over_twelve = 0.08333333333333333f; // This is just 1/12 because * is (usually) faster than /

        public override UIComponent Create(UIHelperBase helper)
        {
            UISlider slider = helper.AddSlider(this.uniqueName, this.min, this.max, this.step, this.value, IgnoredFunction) as UISlider;
            slider.enabled = this.enabled;
            slider.name = this.uniqueName;
            slider.tooltip = this.getVarianceTimeFromFloatingValue(this.value);
            slider.width = 500f;

            component = slider;

            UIPanel sliderParent = slider.parent as UIPanel;
            if(sliderParent != null)
            {
                UILabel label = sliderParent.Find<UILabel>("Label");

                if (label != null)
                {
                    label.width = 500f;
                }
            }

            slider.eventValueChanged += Slider_eventValueChanged;
            Slider_eventValueChanged(slider, slider.value);

            component = slider;
            return slider;
        }

        public override void Translate(Translation translation)
        {
            UISlider uiObject = component as UISlider;
            UIPanel sliderParent = uiObject.parent as UIPanel;
            
            if (sliderParent != null)
            {
                UILabel label = sliderParent.Find<UILabel>("Label");

                if (label != null)
                {
                    label.text = translation.GetTranslation("Option_" + (translationIdentifier == "" ? uniqueName : translationIdentifier));
                }
            }
        }

        private void Slider_eventValueChanged(UIComponent component, float value)
        {
            UISlider slider = (UISlider)component;

            this.value = value;
            slider.tooltip = getVarianceTimeFromFloatingValue(this.value);

            try
            {
                slider.RefreshTooltip();
            }
            catch
            {
                //If there's any initial problems.
            }
        }

        private string getVarianceTimeFromFloatingValue(float value)
        {
            int hours = (int)(value);
            int minutes = (int)((value % 1f) * 60f);
            string minutesString = string.Format("{0:00}", minutes);

            string strings = "";
            if (hours != 0)
            {
                strings += string.Format("{0} hour", hours.ToString());
                if (hours > 1)
                {
                    strings += "s"; // Pluralize
                }
            }
            if (minutes != 0)
            {
                if (strings != "")
                {
                    strings += " and ";
                }
                strings += string.Format("{0} minute", minutesString);
                if (minutes > 1)
                {
                    strings += "s"; // Pluralize
                }
            }
            if (strings == "")
            {
                strings = "No Variance";
            }

            return strings;
        }
    }
}
