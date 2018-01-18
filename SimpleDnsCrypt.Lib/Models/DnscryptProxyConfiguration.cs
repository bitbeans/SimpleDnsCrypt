using System.Collections.Generic;

namespace SimpleDnsCrypt.Lib.Models
{
	public class DnscryptProxyConfiguration
	{
		public List<string> server_names { get; set; }
		public List<string> listen_addresses { get; set; }
		public bool daemonize { get; set; }
		public bool force_tcp { get; set; }
		public int timeout { get; set; }
		public int cert_refresh_delay { get; set; }
		public bool block_ipv6 { get; set; }
		public string forwarding_rules { get; set; }
		public bool cache { get; set; }
		public int cache_size { get; set; }
		public int cache_min_ttl { get; set; }
		public int cache_max_ttl { get; set; }
		public int cache_neg_ttl { get; set; }
		public QueryLog query_log { get; set; }
		public Blacklist blacklist { get; set; }
		public Sources sources { get; set; }
		public Servers servers { get; set; }
	}

	public class Blacklist
	{
		public string blacklist_file { get; set; }
		public string log_file { get; set; }
		public string log_format { get; set; }
	}

	public class QueryLog
	{
		public string file { get; set; }
		public string format { get; set; }
	}

	public class Sources
	{
		
	}

	public class Servers
	{
		
	}

}
