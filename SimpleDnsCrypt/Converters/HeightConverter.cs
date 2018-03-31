using System;
using System.Globalization;
using System.Windows.Data;

namespace SimpleDnsCrypt.Converters
{
	public class HeightConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			try
			{
				var actualHeight = (double)value;
				if (actualHeight > 700)
				{
					switch ((string)parameter)
					{
						case "Resolvers":
							return actualHeight - 400;
						case "QueryLog":
							return actualHeight - 200;
						case "DomainBlockLog":
							return actualHeight - 200;
						default:
							return actualHeight - 200;
					}
				}
				return actualHeight;
			}
			catch
			{
				return 0;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
