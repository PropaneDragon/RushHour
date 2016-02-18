using ColossalFramework.UI;
using ICities;
using System;
using CimTools.V1.Utilities;

namespace RushHour.UI
{
    public class TimeOfDaySlider : OptionsItemBase
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
            UISlider slider = helper.AddSlider(this.readableName, this.min, this.max, this.step, this.value, IgnoredFunction) as UISlider;
            slider.enabled = this.enabled;
            slider.name = this.uniqueName;
            slider.tooltip = this.value.ToString();
            slider.width = 500f;

            UIPanel sliderParent = slider.parent as UIPanel;
            if (sliderParent != null)
            {
                UILabel label = sliderParent.Find<UILabel>("Label");

                if (label != null)
                {
                    label.width = 500f;
                }
            }

            slider.eventValueChanged += delegate (UIComponent component, float newValue)
            {
                this.value = newValue;
                float displayedValue = this.value % 12; // Wrap military time into civilian time
                if ( displayedValue < 1f ) {
                    displayedValue += 12f; // Instead of 0 let's show 12 even for am
                }
                int hours = (int)(displayedValue);
                string minutes = string.Format("{0:00}", (int)((displayedValue % 1f) * 60f));
                string suffix = (this.value * one_over_twelve > 1 ) ? "pm" : "am";

                slider.tooltip = hours.ToString() + ':' + minutes.ToString() + ' ' + suffix;
                slider.RefreshTooltip();
            };
        }
    }
}
