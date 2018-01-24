using System.Collections.Generic;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using Nett;
// ReSharper disable InconsistentNaming

namespace SimpleDnsCrypt.Models
{
	public class DnscryptProxyConfiguration : PropertyChangedBase
	{
		public List<string> server_names { get; set; }
		public ObservableCollection<string> _listen_addresses;
		private bool _require_nofilter;
		private bool _ipv4_servers;
		private bool _ipv6_servers;
		private bool _require_dnssec;
		private bool _require_nolog;
		private bool _force_tcp;
		private int _timeout;
		private int _cert_refresh_delay;

		private int _log_level;
		private string _log_file;

		private Dictionary<string, Source> _sources;
		private Dictionary<string, Server> _servers;

		[TomlIgnore]
		public new bool IsNotifying
		{
			get => base.IsNotifying;
			set => base.IsNotifying = value;
		}


		/*
		 * # Log level (0-6, default: 2 - 0 is very verbose, 6 only contains fatal errors)

# log_level = 2


# log file for the application

# log_file = 'dnscrypt-proxy.log'
		 * */

		public ObservableCollection<string> listen_addresses
		{
			get => _listen_addresses;
			set
			{
				_listen_addresses = value;
				NotifyOfPropertyChange(() => listen_addresses);
			}
		}

		/// <summary>
		/// Use servers reachable over IPv4
		/// </summary>
		public bool ipv4_servers
		{
			get => _ipv4_servers;
			set
			{
				_ipv4_servers = value;
				NotifyOfPropertyChange(() => ipv4_servers);
			}
		}

		/// <summary>
		/// Use servers reachable over IPv6 -- Do not enable if you don't have IPv6 connectivity
		/// </summary>
		public bool ipv6_servers
		{
			get => _ipv6_servers;
			set
			{
				_ipv6_servers = value;
				NotifyOfPropertyChange(() => ipv6_servers);
			}
		}

		/// <summary>
		/// Server must support DNS security extensions
		/// </summary>
		public bool require_dnssec
		{
			get => _require_dnssec;
			set
			{
				_require_dnssec = value;
				NotifyOfPropertyChange(() => require_dnssec);
			}
		}

		/// <summary>
		/// Server must not log user queries
		/// </summary>
		public bool require_nolog
		{
			get => _require_nolog;
			set
			{
				_require_nolog = value;
				NotifyOfPropertyChange(() => require_nolog);
			}
		}

		/// <summary>
		/// Server must not enforce its own blacklist (for parental control, ads blocking...)
		/// </summary>
		public bool require_nofilter
		{
			get => _require_nofilter;
			set
			{
				_require_nofilter = value;
				NotifyOfPropertyChange(() => require_nofilter);
			}
		}

		/// <summary>
		/// Unused
		/// </summary>
		public bool daemonize { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public bool force_tcp
		{
			get => _force_tcp;
			set
			{
				_force_tcp = value;
				NotifyOfPropertyChange(() => force_tcp);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public int timeout
		{
			get => _timeout;
			set
			{
				_timeout = value;
				NotifyOfPropertyChange(() => timeout);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public int cert_refresh_delay
		{
			get => _cert_refresh_delay;
			set
			{
				_cert_refresh_delay = value;
				NotifyOfPropertyChange(() => cert_refresh_delay);
			}
		}

		public bool block_ipv6 { get; set; }
		public string forwarding_rules { get; set; }
		public bool cache { get; set; }
		public int cache_size { get; set; }
		public int cache_min_ttl { get; set; }
		public int cache_max_ttl { get; set; }
		public int cache_neg_ttl { get; set; }
		public QueryLog query_log { get; set; }
		public Blacklist blacklist { get; set; }

		/// <summary>
		/// 
		/// </summary>
		public Dictionary<string, Source> sources
		{
			get => _sources;
			set
			{
				_sources = value;
				NotifyOfPropertyChange(() => sources);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Dictionary<string, Server> servers
		{
			get => _servers;
			set
			{
				_servers = value;
				NotifyOfPropertyChange(() => servers);
			}
		}
	}

	public class Blacklist
	{
		public string blacklist_file { get; set; }
		public string log_file { get; set; }
		public string log_format { get; set; }
	}

	public class QueryLog : PropertyChangedBase
	{
		private string _format;
		private string _file;

		[TomlIgnore]
		public new bool IsNotifying
		{
			get => base.IsNotifying;
			set => base.IsNotifying = value;
		}

		public string format
		{
			get => _format;
			set
			{
				_format = value;
				NotifyOfPropertyChange(() => format);
			}
		}

		public string file
		{
			get => _file;
			set
			{
				_file = value;
				NotifyOfPropertyChange(() => file);
			}
		}
	}

	public class Source
	{
		public string url { get; set; }
		public string minisign_key { get; set; }
		public string cache_file { get; set; }
		public string format { get; set; }
		public int refresh_delay { get; set; }
		public string prefix { get; set; }
	}

	public class Server
	{
		public string stamp { get; set; }
	}
}
