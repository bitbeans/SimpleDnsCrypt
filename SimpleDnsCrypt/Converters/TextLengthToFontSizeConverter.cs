using System;
using System.Globalization;
using System.Windows.Data;

namespace SimpleDnsCrypt.Converters
{
	/// <summary>
	///     Text length to font size converter.
	/// </summary>
	public class TextLengthToFontSizeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var text = (string)value;
			if (text.Length > 10 && text.Length < 15)
			{
				return 14;
				//return 12;
			}
			if (text.Length >= 15)
			{
				return 10;
			}
			return 14;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
