using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SimpleDnsCrypt.Converters
{
	/// <summary>
	///     Boolean to visibility converter.
	/// </summary>
	public class BoolToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			try
			{
				return (bool)value ? Visibility.Visible : Visibility.Hidden;
			}
			catch
			{
				return Visibility.Visible;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
