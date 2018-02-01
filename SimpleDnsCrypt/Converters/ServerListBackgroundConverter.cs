using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Data;
using SimpleDnsCrypt.Extensions;

namespace SimpleDnsCrypt.Converters
{
	public class ServerListBackgroundConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if ((bool)value)
			{
				return "#FF8AB329";
			}
			return Color.DarkGray.ToHexString();
		}

		public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}