using System;
using System.Globalization;
using System.Windows.Data;

namespace SimpleDnsCrypt.Converter
{
    /// <summary>
    ///     Reverse boolean converter.
    /// </summary>
    public class ReverseBoolToEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool) value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}