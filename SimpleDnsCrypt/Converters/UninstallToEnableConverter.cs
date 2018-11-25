using System;
using System.Globalization;
using System.Windows.Data;

namespace SimpleDnsCrypt.Converters
{
	public class UninstallToEnableConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			var isUninstalling = (bool)values[0];
			var isServiceInstalled = (bool)values[1];
			return !isUninstalling && isServiceInstalled;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
