using System;

namespace SimpleDnsCrypt.Models
{
	public class AddressBlockLogLine : LogLine
	{
		public DateTime Time { get; set; }
		public string Host { get; set; }
		public string QName { get; set; }
		public string Message { get; set; }

		public AddressBlockLogLine(string line)
		{
			try
			{

			}
			catch (Exception)
			{
			}
		}
	}
}