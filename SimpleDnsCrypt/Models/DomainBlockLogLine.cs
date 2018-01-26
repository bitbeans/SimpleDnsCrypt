using System;

namespace SimpleDnsCrypt.Models
{
	public class DomainBlockLogLine : LogLine
	{
		public DateTime Time { get; set; }
		public string Host { get; set; }
		public string QName { get; set; }
		public string Message { get; set; }

		public DomainBlockLogLine(string line)
		{
			try
			{
				//this only works with the ltsv log format: 
				//time:1516794292	host:127.0.0.1	qname:stats.g.doubleclick.net	message:stats.*
				var stringSeparators = new[] { "\t" };
				var parts = line.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length != 4) return;
				if (parts[0].StartsWith("time:"))
				{
					Time = UnixTimeStampToDateTime(Convert.ToDouble(parts[0].Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1]));
				}
				if (parts[1].StartsWith("host:"))
				{
					Host = parts[1].Split(new[] { ':' }, 2)[1];
				}
				if (parts[2].StartsWith("qname:"))
				{
					QName = parts[2].Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
				}
				if (parts[3].StartsWith("message:"))
				{
					Message = parts[3].Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
				}
			}
			catch (Exception)
			{
			}
		}
	}
}
