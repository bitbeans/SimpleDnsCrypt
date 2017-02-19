using System;
using System.Globalization;

namespace SimpleDnsCrypt.Models
{
	public class LogLine
	{
		public LogLine()
		{
		}

		public LogLine(string line)
		{
			try
			{
				//this only works with the DNSCrypt default log format
				var stringSeparators = new[] {"\t"};
				var parts = line.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
				try
				{
					Date = DateTime.ParseExact(parts[0], "MM/dd/yy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None);
				}
				catch (FormatException)
				{
					//not the best solution
					Date = DateTime.Now;
				}

				Address = parts[1];
				Remote = parts[2];
				LogLineType logLineType;
				if (Enum.TryParse(parts[3], out logLineType))
				{
					Type = logLineType;
				}
			}
			catch (Exception)
			{
			}
		}

		public DateTime Date { get; set; }
		public string Address { get; set; }
		public string Remote { get; set; }
		public LogLineType Type { get; set; }
	}
}
