using ColossalFramework.UI;
using ICities;
using System;
using CimTools.V1.Utilities;

namespace RushHour.UI
{
    public class TimeOfDayVarianceSlider : OptionsItemBase
    {
        public float min = 0.0f;

        public float max = 23.9f;

        public float step = 0.25f;

        public float value
        {
            get
            {
                return (float)m_value;
            }
            set
            {
                m_value = value;
            }
        }

        private const float one_over_twelve = 0.08333333333333333f; // This is just 1/12 because * is (usually) faster than /

        public override void Create(UIHelperBase helper)
        {
            UISlider slider = helper.AddSlider(this.uniqueName, this.min, this.max, this.step, this.value, IgnoredFunction) as UISlider;
            slider.enabled = this.enabled;
            slider.name = this.uniqueName;
            slider.tooltip = this.value.ToString();
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
            float displayedValue = this.value; // Wrap military time into civilian time
            int hours = (int)(displayedValue);
            int minutes = (int)((displayedValue % 1f) * 60f);
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
            slider.tooltip = strings;

            try
            {
                slider.tooltipBox.Show();
                slider.RefreshTooltip();
            }
            catch
            {
                //This is just here because it'll error out when the game fist starts otherwise as the tooltip doesn't exist.
            }
        }
    }
}
