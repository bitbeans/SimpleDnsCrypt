using System;
using System.Globalization;
using System.Threading;
using System.Windows.Data;

namespace SimpleDnsCrypt.Converter
{
	public class LocalDateConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var date = (DateTime)value;
			return string.Format("({0})",date.ToString("g", Thread.CurrentThread.CurrentCulture));
		}

		public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
