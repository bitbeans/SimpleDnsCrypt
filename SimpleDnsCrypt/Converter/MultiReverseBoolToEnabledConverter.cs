using System;
using System.Globalization;
using System.Windows.Data;

namespace SimpleDnsCrypt.Converter
{
    /// <summary>
    ///     Multi boolean converter.
    /// </summary>
    public class MultiReverseBoolToEnabledConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var isTrueOne = (bool) values[0];
            var isTrueTwo = (bool) values[1];
            if ((isTrueOne) || (isTrueTwo))
            {
                return false;
            }
            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}