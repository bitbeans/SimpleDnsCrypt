using System;
using System.Globalization;
using System.Windows.Data;

namespace SimpleDnsCrypt.Converters
{
	public class BoolToForegroundConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if ((bool)value)
			{
				return "#FF8ab329";
			}

			return "#FFFF8C00";
		}

		public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
