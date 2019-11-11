using SimpleDnsCrypt.Models;
using System;
using System.Globalization;
using System.Windows.Data;

namespace SimpleDnsCrypt.Converters
{
	public class RouteStateToColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value != null)
			{
				var routeState = (RouteState)value;
				switch (routeState)
				{
					case RouteState.Empty:
						//gray
						return "#D8D8D8";
					case RouteState.Invalid:
						//red
						return "#CCC1170F";
					case RouteState.Valid:
						//green
						return "#CC60A917";
				}
			}
			//gray
			return "#D8D8D8";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
