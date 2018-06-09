using System;

namespace SimpleDnsCrypt.Models
{
	public abstract class LogLine
	{
		public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
		{
			// Unix timestamp is seconds past epoch
			var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
			return dtDateTime;
		}
	}

	public enum QueryLogReturnCode
	{
		PASS,
		FORWARD,
		DROP,
		REJECT,
	    SYNTH,
	    PARSE_ERROR,
		NXDOMAIN,
		RESPONSE_ERROR,
		SERVER_ERROR
	}

	public class QueryLogLine : LogLine
	{
		public DateTime Date { get; set; }
		public string Address { get; set; }
		public string Remote { get; set; }
		public QueryLogLineType Type { get; set; }
		public QueryLogReturnCode ReturnCode { get; set; }

		public QueryLogLine(string line)
		{
			try
			{
				//this only works with the ltsv log format: 
				//time:1516734518	host:::1	message:stackoverflow.com	type:A
				var stringSeparators = new[] { "\t" };
				var parts = line.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length != 5) return;
				if (parts[0].StartsWith("time:"))
				{
					Date = UnixTimeStampToDateTime(Convert.ToDouble(parts[0].Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1]));
				}
				if (parts[1].StartsWith("host:"))
				{
					Address = parts[1].Split(new[] {':'}, 2)[1];
				}
				if (parts[2].StartsWith("message:"))
				{
					Remote = parts[2].Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
				}
				if (parts[3].StartsWith("type:"))
				{
					if (Enum.TryParse(parts[3].Split(new[] {":"}, StringSplitOptions.RemoveEmptyEntries)[1].Trim(),
						out QueryLogLineType queryLogLineType))
					{
						Type = queryLogLineType;
					}
				}
				if (parts[4].StartsWith("return:"))
				{
					if (Enum.TryParse(parts[4].Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim(),
						out QueryLogReturnCode queryLogReturnCode))
					{
						ReturnCode = queryLogReturnCode;
					}
				}
				else
				{
					Type = QueryLogLineType.Unknown;
				}
			}
			catch (Exception)
			{
			}
		}
	}
}
