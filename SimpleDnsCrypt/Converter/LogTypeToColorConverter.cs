using System;
using System.Globalization;
using System.Windows.Data;
using SimpleDnsCrypt.Models;

namespace SimpleDnsCrypt.Converter
{
	public class LogTypeToColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var logLineType = (LogLineType)value;
			switch (logLineType)
			{
				case LogLineType.A:
					return "#FF8ab329";
				case LogLineType.Unknown:
					return "#FF8ab329";
				case LogLineType.NS:
					return "#FF8ab329";
				case LogLineType.CNAME:
					return "#FF8ab329";
				case LogLineType.SOA:
					return "#FF8ab329"; ;
				case LogLineType.WKS:
					return "#FF8ab329";
				case LogLineType.PTR:
					return "#FF2a3b68";
				case LogLineType.MX:
					return "#FF8ab329";
				case LogLineType.TXT:
					return "#FF8ab329";
				case LogLineType.AAAA:
					return "#FFB32929";
				case LogLineType.SRV:
					return "#FF8ab329";
				case LogLineType.ANY:
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
