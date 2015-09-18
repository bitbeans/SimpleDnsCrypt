using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using MahApps.Metro.Converters;

namespace SimpleDnsCrypt.Converter
{
    /// <summary>
    ///     Converter for WPF-visiblity to boolean.
    /// </summary>
    [ValueConversion(typeof (bool), typeof (Visibility))]
    public sealed class VisibilityConverter : MarkupConverter
    {
        /// <summary>
        ///     converts from visiblity to boolean
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        protected override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visibility = (bool) value;
            return visibility ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        ///     converts from visibility to boolean
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        protected override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visibility = (Visibility) value;
            return (visibility == Visibility.Visible);
        }
    }
}