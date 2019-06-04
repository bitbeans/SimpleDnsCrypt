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
			return false;
        }
	}
}
