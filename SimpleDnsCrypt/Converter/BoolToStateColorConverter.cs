using System;
using System.Globalization;
using System.Windows.Data;

namespace SimpleDnsCrypt.Converter
{
    /// <summary>
    ///     Boolean to color converter.
    /// </summary>
    public class BoolToStateColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isTrue = (bool) value;
            if (isTrue)
            {
                // green
                return "#FF8ab329";
            }
            // red
            return "#FFB32929";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}