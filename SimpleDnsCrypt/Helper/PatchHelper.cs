namespace SimpleDnsCrypt.Helper
{
	/// <summary>
	/// Class to update the configuration file. 
	/// </summary>
	public static class PatchHelper
	{
		public static bool Patch()
		{
			var version = VersionHelper.PublishVersion;
			if (!DnscryptProxyConfigurationManager.LoadConfiguration()) return false;
			if (version.Equals("0.6.5"))
			{
				//added: netprobe_address = '255.255.255.0:53'
				//changed: netprobe_timeout = 0
				DnscryptProxyConfigurationManager.DnscryptProxyConfiguration.netprobe_address = "255.255.255.0:53";
				DnscryptProxyConfigurationManager.DnscryptProxyConfiguration.netprobe_timeout = 0;
				return DnscryptProxyConfigurationManager.SaveConfiguration();
			}
			if (version.Equals("0.6.6"))
			{
				//changed: netprobe_address = '9.9.9.9:53'
				//changed: netprobe_timeout = 60
				DnscryptProxyConfigurationManager.DnscryptProxyConfiguration.netprobe_address = "9.9.9.9:53";
				DnscryptProxyConfigurationManager.DnscryptProxyConfiguration.netprobe_timeout = 60;
				return DnscryptProxyConfigurationManager.SaveConfiguration();
			}
			if (version.Equals("0.6.8"))
			{
				//changed: timeout = 5000
				//added: reject_ttl = 600
				//changed: cache_size = 1024
				//changed: cache_min_ttl = 2400
				//added: cache_neg_min_ttl = 60
				//added: cache_neg_max_ttl = 600
				DnscryptProxyConfigurationManager.DnscryptProxyConfiguration.timeout = 5000;
				DnscryptProxyConfigurationManager.DnscryptProxyConfiguration.reject_ttl = 600;
				DnscryptProxyConfigurationManager.DnscryptProxyConfiguration.cache_size = 1024;
				DnscryptProxyConfigurationManager.DnscryptProxyConfiguration.cache_min_ttl = 2400;
				DnscryptProxyConfigurationManager.DnscryptProxyConfiguration.cache_neg_min_ttl = 60;
				DnscryptProxyConfigurationManager.DnscryptProxyConfiguration.cache_neg_max_ttl = 600;
				return DnscryptProxyConfigurationManager.SaveConfiguration();
			}
			return false;
		}
	}
}
