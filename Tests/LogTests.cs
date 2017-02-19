using System;
using NUnit.Framework;
using SimpleDnsCrypt.Models;

namespace Tests
{
	[TestFixture]
	public class LogTests
	{
		[Test]
		public void LogLineParserTest()
		{
			const string rawLogLine = "01/21/17 20:06:18\t127.0.0.1\tapp.vsspsext.visualstudio.com\tA";
			var logLine = new LogLine(rawLogLine);
			Assert.AreEqual(logLine.Address, "127.0.0.1");
			Assert.AreEqual(logLine.Date, new DateTime(2017, 1, 21, 20, 6, 18));
			Assert.AreEqual(logLine.Remote, "app.vsspsext.visualstudio.com");
			Assert.AreEqual(logLine.Type, LogLineType.A);
		}
	}
}
