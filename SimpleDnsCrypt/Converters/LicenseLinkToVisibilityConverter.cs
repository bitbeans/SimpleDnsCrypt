using SimpleDnsCrypt.Models;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SimpleDnsCrypt.Converters
{
	public class LicenseLinkToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			try
			{
				return (LicenseLink)value != null ? Visibility.Visible : Visibility.Collapsed;
			}
			catch
			{
				return Visibility.Collapsed;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
