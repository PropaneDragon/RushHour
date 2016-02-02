using ColossalFramework.UI;
using ICities;
using System;
using CimTools.V1.Utilities;

namespace RushHour.UI
{
    class TimeOfDayVarianceSlider : OptionsItemBase
    {
        public float min = 0.0f;

        public float max = 23.9f;

        public float step = 0.25f;

        internal object m_value = null;

        public float value
        {
            get
            {
                return (float)this.m_value;
            }
            set
            {
                this.m_value = value;
            }
        }

        internal void IgnoredFunction<T>(T ignored)
        {
        }

        private const float one_over_twelve = 0.08333333333333333f; // This is just 1/12 because * is (usually) faster than /

        public override void Create(UIHelperBase helper)
        {
            UISlider slider = helper.AddSlider(this.readableName, this.min, this.max, this.step, this.value, new OnValueChanged(IgnoredFunction<float>)) as UISlider;
            slider.enabled = this.enabled;
            slider.name = this.uniqueName;
            slider.tooltip = this.value.ToString();
            slider.width = 500f;
            slider.eventValueChanged += delegate (UIComponent component, float newValue)
            {
                this.value = newValue;
                float displayedValue = this.value; // Wrap military time into civilian time
                int hours = (int)(displayedValue);
                int minutes = (int)((displayedValue % 1f) * 60f);
                string minutesString = String.Format("{0:00}", minutes);

                string strings = "";
                if (hours != 0) {
                    strings += String.Format("{0} hour", hours.ToString());
                    if ( hours > 1 ) {
                        strings += "s"; // Pluralize
                    }
                }
                if (minutes != 0) {
                    if (strings != "")
                    {
                        strings += " and ";
                    }
                    strings += String.Format("{0} minute", minutesString);
                    if (minutes > 1)
                    {
                        strings += "s"; // Pluralize
                    }
                }
                if ( strings == "" )
                {
                    strings = "No Variance";
                }
                slider.tooltip = strings;
                slider.RefreshTooltip();
            };
        }
    }
}
