using System;
using System.Globalization;
using System.Windows.Data;

namespace SimpleDnsCrypt.Converter
{
    /// <summary>
    ///     Boolean to color converter.
    /// </summary>
    public class EnabledToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var enabled = (bool) value;
            if (enabled)
            {
                //gray
                return "#FFA0A0A0";
            }
            //green
            return "#FF8ab329";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}