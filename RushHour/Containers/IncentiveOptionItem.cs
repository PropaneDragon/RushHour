using RushHour.Events.Unique;

namespace RushHour.Containers
{
    public delegate void OptionItemChanged();
    public delegate void TicketSizeChanged();

    /// <summary>
    /// Option storage
    /// </summary>
    public class IncentiveOptionItem
    {
        public string title = "";
        public string description = "";
        public int positiveEffect = 0;
        public int negativeEffect = 0;
        public float cost = 0;
        public float returnCost = 0;
        public float sliderValue = 0;
        public float ticketCount = 0;

        public event OptionItemChanged OnOptionItemChanged;
        public event TicketSizeChanged OnTicketSizeChanged;

        public void UpdateItemChanged()
        {
            if(OnOptionItemChanged != null)
            {
                OnOptionItemChanged();
            }
        }

        public void UpdateTicketSize()
        {
            if (OnTicketSizeChanged != null)
            {
                OnTicketSizeChanged();
            }
        }
    }
}
