using System;
using System.ComponentModel;
using System.Windows;

namespace EventEditor.Utils
{
    internal class SafelyConvert
    {
        public static bool SafelyParseWithError<From, To>(From value, ref To newValue, string objectLabel) where From : IConvertible where To : IConvertible
        {
            bool converted = SafelyParse(value, ref newValue);

            if (!converted)
            {
                MessageBox.Show(string.Format("The {0} couldn't be saved, as it is invalid. Make sure it's correct and try again.", objectLabel), "Invalid value", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return converted;
        }

        public static bool SafelyParse<From, To>(From value, ref To newValue) where From : IConvertible where To : IConvertible
        {
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(To));

            if (converter != null && converter.CanConvertFrom(typeof(From)))
            {
                try
                {
                    newValue = (To)converter.ConvertFrom(value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }
    }
}
