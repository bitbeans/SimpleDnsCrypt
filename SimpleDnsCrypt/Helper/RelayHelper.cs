using Caliburn.Micro;
using DnsCrypt.Models;
using SimpleDnsCrypt.Config;
using System;
using System.Collections.Generic;
using System.IO;

namespace SimpleDnsCrypt.Helper
{
	public static class RelayHelper
	{
		private static readonly ILog Log = LogManagerHelper.Factory();

		public static List<StampFileEntry> GetRelays()
		{
			var relays = new List<StampFileEntry>();
			var relayFile = Path.Combine(Directory.GetCurrentDirectory(), Global.DnsCryptProxyFolder, "relays.md");
			try
			{
				if (File.Exists(relayFile))
				{
					relays = DnsCrypt.Stamps.StampTools.ReadStampFileEntries(relayFile);
				}
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}
			return relays;
		}
	}
}
