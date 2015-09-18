using System;
using System.Globalization;
using System.Windows.Data;

namespace SimpleDnsCrypt.Converter
{
    /// <summary>
    ///     Boolean to color converter.
    /// </summary>
    public class InUseToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isInUse = (bool) value;
            if (isInUse)
            {
                //green
                return "#FF8ab329";
            }
            //gray
            return "#FFA0A0A0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}