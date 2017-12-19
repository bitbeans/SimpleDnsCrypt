using System;
using System.Globalization;
using System.Windows.Data;

namespace SimpleDnsCrypt.Converter
{
	public class UnsavedStateToIconConverter : IValueConverter
	{
		public object SavedIcon { get; set; }
		public object UnsavedIcon { get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var state = (bool)value;
			return state ? UnsavedIcon : SavedIcon;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
