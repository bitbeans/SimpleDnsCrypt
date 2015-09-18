using System;
using System.Globalization;
using System.Windows.Data;

namespace SimpleDnsCrypt.Converter
{
    /// <summary>
    ///     Boolean to icon converter.
    /// </summary>
    public class BoolToStateIconConverter : IValueConverter
    {
        public object TrueIcon { get; set; }
        public object FalseIcon { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var state = (bool) value;
            return state ? TrueIcon : FalseIcon;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}