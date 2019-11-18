using System.Collections.Generic;
using SimpleDnsCrypt.Models;

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
			if (version.Equals("0.6.8") || version.Equals("0.6.9") || version.Equals("0.7.0"))
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
				var sources = DnscryptProxyConfigurationManager.DnscryptProxyConfiguration.sources;
				if (!sources.ContainsKey("relays"))
				{
					sources.Add("relays", new Models.Source
					{
						urls = new[] { "https://github.com/DNSCrypt/dnscrypt-resolvers/raw/master/v2/relays.md", "https://download.dnscrypt.info/resolvers-list/v2/relays.md" },
						cache_file = "relays.md",
						minisign_key = "RWQf6LRCGA9i53mlYecO4IzT51TGPpvWucNSCh1CBM0QTaLn73Y7GFO3",
						refresh_delay = 72,
						prefix = ""
					});
					DnscryptProxyConfigurationManager.DnscryptProxyConfiguration.sources = sources;
				}
				return DnscryptProxyConfigurationManager.SaveConfiguration();
			}
			if (version.Equals("0.7.1"))
			{
				//changed: ignore_system_dns = true
				//added: broken_implementations
				DnscryptProxyConfigurationManager.DnscryptProxyConfiguration.ignore_system_dns = true;
				var sources = DnscryptProxyConfigurationManager.DnscryptProxyConfiguration.sources;
				if (!sources.ContainsKey("relays"))
				{
					sources.Add("relays", new Models.Source
					{
						urls = new[] { "https://github.com/DNSCrypt/dnscrypt-resolvers/raw/master/v2/relays.md", "https://download.dnscrypt.info/resolvers-list/v2/relays.md" },
						cache_file = "relays.md",
						minisign_key = "RWQf6LRCGA9i53mlYecO4IzT51TGPpvWucNSCh1CBM0QTaLn73Y7GFO3",
						refresh_delay = 72,
						prefix = ""
					});
					DnscryptProxyConfigurationManager.DnscryptProxyConfiguration.sources = sources;
				}

				DnscryptProxyConfigurationManager.DnscryptProxyConfiguration.broken_implementations =
					new BrokenImplementations
					{
						broken_query_padding = new List<string> { "cisco", "cisco-ipv6", "cisco-familyshield" }
					};
				return DnscryptProxyConfigurationManager.SaveConfiguration();
			}

			return false;
		}
	}
}
