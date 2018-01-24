using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SimpleDnsCrypt.Converters
{
	public class IntegerBoolToVisibilityConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			var integerValue = (int)values[0];
			var boolValue = (bool)values[1];
			if (integerValue == 0 && boolValue)
			{
				return Visibility.Visible;
			}
			return Visibility.Collapsed;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
