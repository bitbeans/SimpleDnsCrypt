using System;
using System.Globalization;
using System.Threading;
using System.Windows.Data;

namespace SimpleDnsCrypt.Converters
{
	public class LocalDateConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null) return $"({DateTime.Now.ToString("g", Thread.CurrentThread.CurrentCulture)})";
			var date = (DateTime)value;
			return $"({date.ToString("g", Thread.CurrentThread.CurrentCulture)})";
		}

		public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
