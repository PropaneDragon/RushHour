using RushHour.Events.Unique;

namespace RushHour.Containers
{
    /// <summary>
    /// Option storage
    /// </summary>
    internal class LabelOptionItem
    {
        /// <summary>
        /// A unique identifier for finding the option later
        /// </summary>
        public XmlEvent linkedEvent = null;

        /// <summary>
        /// The readable string to be printed out
        /// </summary>
        public string readableLabel = "";
    }
}
