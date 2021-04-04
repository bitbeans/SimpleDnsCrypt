using Caliburn.Micro;
using SimpleDnsCrypt.Helper;
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
		SERVER_ERROR,
		CLOAK
	}

	public class QueryLogLine : LogLine
	{
		private static readonly ILog Log = LogManagerHelper.Factory();
		public DateTime Date { get; set; }
		public string Address { get; set; }
		public string Remote { get; set; }
		public QueryLogLineType Type { get; set; }
		public QueryLogReturnCode ReturnCode { get; set; }
		public bool Cached { get; set; }
		public string CachedText
		{
			get
			{
				if (Cached)
				{
					return "cached";
				}
				else
				{
					return "live";
				}
			}
		}
		public long Duration { get; set; }
		public string DurationText
		{
			get
			{
				return $"{Duration}ms";
			}
		}
		public string Server { get; set; }

		public QueryLogLine(string line)
		{
			try
			{
				//this only works with the ltsv log format: 
				//time:1559589175	host:::1	message:www.test.de	type:AAAA	return:SYNTH	cached:0	duration:0	server:freetsa.org
				var stringSeparators = new[] { "\t" };
				var parts = line.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length != 8) return;
				if (parts[0].StartsWith("time:"))
				{
					Date = UnixTimeStampToDateTime(Convert.ToDouble(parts[0].Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1]));
				}
				if (parts[1].StartsWith("host:"))
				{
					Address = parts[1].Split(new[] { ':' }, 2)[1];
				}
				if (parts[2].StartsWith("message:"))
				{
					Remote = parts[2].Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
				}
				if (parts[3].StartsWith("type:"))
				{
					if (Enum.TryParse(parts[3].Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim(),
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
				if (parts[5].StartsWith("cached:"))
				{
					Cached = Convert.ToBoolean(Convert.ToInt16(parts[5].Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim()));
				}
				if (parts[6].StartsWith("duration:"))
				{
					Duration = Convert.ToInt64(parts[6].Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim());
				}
				if (parts[7].StartsWith("server:"))
				{
					Server = parts[7].Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
				}
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}
		}
	}
}
