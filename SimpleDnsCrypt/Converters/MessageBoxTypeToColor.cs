using SimpleDnsCrypt.Models;
using System;
using System.Globalization;
using System.Windows.Data;

namespace SimpleDnsCrypt.Converters
{
	/// <summary>
	///     Enum to color converter.
	/// </summary>
	[ValueConversion(typeof(BoxType), typeof(string))]
	public class MessageBoxTypeToColor : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			switch ((BoxType)value)
			{
				case BoxType.Error:
					//red
					return "#CCC1170F";
				case BoxType.Warning:
					//orange
					return "#CCEA6A12";
				case BoxType.Default:
					//green
					return "#CC60A917";
				default:
					return null;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
