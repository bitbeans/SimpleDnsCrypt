using SimpleDnsCrypt.Models;
using System;
using System.Globalization;
using System.Windows.Data;

namespace SimpleDnsCrypt.Converters
{
	public class QueryLogReturnCodeToColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var logLineReturnCode = (QueryLogReturnCode)value;
			switch (logLineReturnCode)
			{
				case QueryLogReturnCode.PASS:
					return "#FF8ab329";
				case QueryLogReturnCode.FORWARD:
					return "#FF8ab329";
				case QueryLogReturnCode.DROP:
					return "#FFB32929";
				case QueryLogReturnCode.REJECT:
					return "#FFB32929";
				case QueryLogReturnCode.SYNTH:
					return "#FF8ab329";
				case QueryLogReturnCode.PARSE_ERROR:
					return "#FFB32929";
				case QueryLogReturnCode.NXDOMAIN:
					return "#FFB36729";
				case QueryLogReturnCode.RESPONSE_ERROR:
					return "#FFB32929";
				case QueryLogReturnCode.SERVER_ERROR:
					return "#FFB32929";
				case QueryLogReturnCode.CLOAK:
					return "#FF2a3b68";
				default:
					return "#FFB32929";
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
