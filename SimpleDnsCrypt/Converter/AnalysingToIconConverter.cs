using System;
using System.Globalization;

using System.Windows.Data;

namespace SimpleDnsCrypt.Converter
{
	public class AnalysingToIconConverter : IValueConverter
	{
		public object AnalysingIcon { get; set; }
		public object CancelingIcon { get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var state = (bool)value;
			return state ? CancelingIcon : AnalysingIcon;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
