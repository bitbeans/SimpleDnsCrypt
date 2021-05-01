using SimpleDnsCrypt.Models;
using System;
using System.Globalization;
using System.Windows.Data;

namespace SimpleDnsCrypt.Converters
{
	public class QueryLogTypeToColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var logLineType = (QueryLogLineType)value;
			switch (logLineType)
			{
				case QueryLogLineType.A:
					return "#FF8ab329";
				case QueryLogLineType.Unknown:
					return "#FF8ab329";
				case QueryLogLineType.NS:
					return "#FF8ab329";
				case QueryLogLineType.CNAME:
					return "#FF8ab329";
				case QueryLogLineType.SOA:
					return "#FF8ab329";
				case QueryLogLineType.WKS:
					return "#FF8ab329";
				case QueryLogLineType.PTR:
					return "#FF2a3b68";
				case QueryLogLineType.MX:
					return "#FF8ab329";
				case QueryLogLineType.TXT:
					return "#FF8ab329";
				case QueryLogLineType.AAAA:
					return "#FFB32929";
				case QueryLogLineType.SRV:
					return "#FF8ab329";
				case QueryLogLineType.ANY:
					return "#FF8ab329";
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
