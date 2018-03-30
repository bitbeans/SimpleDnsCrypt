using Caliburn.Micro;
using Nett;
using System.Collections.Generic;
using System.Collections.ObjectModel;

// ReSharper disable InconsistentNaming

namespace SimpleDnsCrypt.Models
{
	public class DnscryptProxyConfiguration : PropertyChangedBase
	{
		private ObservableCollection<string> _server_names;
		private ObservableCollection<string> _listen_addresses;
		private int _max_clients;
		private bool _require_nofilter;
		private bool _ipv4_servers;
		private bool _ipv6_servers;
		private bool _dnscrypt_servers;
		private bool _doh_servers;
		private bool _require_dnssec;
		private bool _require_nolog;
		private bool _force_tcp;
		private int _timeout;
		private string _lb_strategy;
		private int _cert_refresh_delay;
		private int _log_level;
		private string _log_file;
		private Dictionary<string, Source> _sources;
		private bool _use_syslog;
		private int _log_files_max_size;
		private int _log_files_max_age;
		private int _log_files_max_backups;
		private bool _block_ipv6;
		private string _forwarding_rules;
		private int _cache_neg_ttl;
		private int _cache_max_ttl;
		private int _cache_min_ttl;
		private int _cache_size;
		private bool _cache;
		private string _fallback_resolver;
		private bool _ignore_system_dns;
		private Dictionary<string, Static> _static;

		[TomlIgnore]
		public new bool IsNotifying
		{
			get => base.IsNotifying;
			set => base.IsNotifying = value;
		}

		/// <summary>
		///     List of servers to use.
		/// </summary>
		public ObservableCollection<string> server_names
		{
			get => _server_names;
			set
			{
				_server_names = value;
				NotifyOfPropertyChange(() => server_names);
			}
		}

		/// <summary>
		///     List of local addresses and ports to listen to. Can be IPv4 and/or IPv6.
		/// </summary>
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
		///     Maximum number of simultaneous client connections to accept.
		/// </summary>
		public int max_clients
		{
			get => _max_clients;
			set
			{
				_max_clients = value;
				NotifyOfPropertyChange(() => max_clients);
			}
		}

		/// <summary>
		///     Use servers reachable over IPv4
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
		///     Use servers reachable over IPv6 -- Do not enable if you don't have IPv6 connectivity
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
		///		Use servers implementing the DNSCrypt protocol
		/// </summary>
		public bool dnscrypt_servers
		{
			get => _dnscrypt_servers;
			set
			{
				_dnscrypt_servers = value;
				NotifyOfPropertyChange(() => dnscrypt_servers);
			}
		}

		/// <summary>
		///		Use servers implementing the DNS-over-HTTPS protocol
		/// </summary>
		public bool doh_servers
		{
			get => _doh_servers;
			set
			{
				_doh_servers = value;
				NotifyOfPropertyChange(() => doh_servers);
			}
		}

		/// <summary>
		///     Server must support DNS security extensions
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
		///     Server must not log user queries
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
		///     Server must not enforce its own blacklist (for parental control, ads blocking...)
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
		///     linux only.
		/// </summary>
		public bool daemonize { get; set; } = false;

		/// <summary>
		///     Always use TCP to connect to upstream servers.
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
		///     How long a DNS query will wait for a response, in milliseconds.
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
		/// Load-balancing strategy: 'p2' (default), 'ph', 'fastest' or 'random'
		/// </summary>
		public string lb_strategy
		{
			get => _lb_strategy;
			set
			{
				_lb_strategy = value;
				NotifyOfPropertyChange(() => lb_strategy);
			}
		}

		/// <summary>
		///     Log level (0-6, default: 2 - 0 is very verbose, 6 only contains fatal errors).
		/// </summary>
		public int log_level
		{
			get => _log_level;
			set
			{
				_log_level = value;
				NotifyOfPropertyChange(() => log_level);
			}
		}

		/// <summary>
		///     log file for the application.
		/// </summary>
		public string log_file
		{
			get => _log_file;
			set
			{
				_log_file = value;
				NotifyOfPropertyChange(() => log_file);
			}
		}

		/// <summary>
		///     Use the system logger (Windows Event Log)
		/// </summary>
		public bool use_syslog
		{
			get => _use_syslog;
			set
			{
				_use_syslog = value;
				NotifyOfPropertyChange(() => use_syslog);
			}
		}

		/// <summary>
		///     Delay, in minutes, after which certificates are reloaded.
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

		/// <summary>
		///     Fallback resolver
		///     This is a normal, non-encrypted DNS resolver, that will be only used
		///     for one-shot queries when retrieving the initial resolvers list, and
		///     only if the system DNS configuration doesn't work.
		///     No user application queries will ever be leaked through this resolver,
		///     and it will not be used after IP addresses of resolvers URLs have been found.
		///     It will never be used if lists have already been cached, and if stamps
		///     don't include host names without IP addresses.
		///     It will not be used if the configured system DNS works.
		///     A resolver supporting DNSSEC is recommended. This may become mandatory.
		/// </summary>
		public string fallback_resolver
		{
			get => _fallback_resolver;
			set
			{
				_fallback_resolver = value;
				NotifyOfPropertyChange(() => fallback_resolver);
			}
		}

		/// <summary>
		///     Never try to use the system DNS settings;
		///     unconditionally use the fallback resolver.
		/// </summary>
		public bool ignore_system_dns
		{
			get => _ignore_system_dns;
			set
			{
				_ignore_system_dns = value;
				NotifyOfPropertyChange(() => ignore_system_dns);
			}
		}

		/// <summary>
		///  Maximum log files size in MB.
		/// </summary>
		public int log_files_max_size
		{
			get => _log_files_max_size;
			set
			{
				_log_files_max_size = value;
				NotifyOfPropertyChange(() => log_files_max_size);
			}
		}

		/// <summary>
		/// Maximum log files age in days.
		/// </summary>
		public int log_files_max_age
		{
			get => _log_files_max_age;
			set
			{
				_log_files_max_age = value;
				NotifyOfPropertyChange(() => log_files_max_age);
			}
		}

		/// <summary>
		/// Maximum log files backups to keep.
		/// </summary>
		public int log_files_max_backups
		{
			get => _log_files_max_backups;
			set
			{
				_log_files_max_backups = value;
				NotifyOfPropertyChange(() => log_files_max_backups);
			}
		}

		/// <summary>
		///     Immediately respond to IPv6-related queries with an empty response
		///     This makes things faster when there is no IPv6 connectivity, but can
		///     also cause reliability issues with some stub resolvers.
		/// </summary>
		public bool block_ipv6
		{
			get => _block_ipv6;
			set
			{
				_block_ipv6 = value;
				NotifyOfPropertyChange(() => block_ipv6);
			}
		}

		/// <summary>
		///     Forwarding rule file.
		/// </summary>
		public string forwarding_rules
		{
			get => _forwarding_rules;
			set
			{
				_forwarding_rules = value;
				NotifyOfPropertyChange(() => forwarding_rules);
			}
		}

		/// <summary>
		///     Enable a DNS cache to reduce latency and outgoing traffic.
		/// </summary>
		public bool cache
		{
			get => _cache;
			set
			{
				_cache = value;
				NotifyOfPropertyChange(() => cache);
			}
		}

		/// <summary>
		///     Cache size.
		/// </summary>
		public int cache_size
		{
			get => _cache_size;
			set
			{
				_cache_size = value;
				NotifyOfPropertyChange(() => cache_size);
			}
		}

		/// <summary>
		///     Minimum TTL for cached entries.
		/// </summary>
		public int cache_min_ttl
		{
			get => _cache_min_ttl;
			set
			{
				_cache_min_ttl = value;
				NotifyOfPropertyChange(() => cache_min_ttl);
			}
		}

		/// <summary>
		///     Maxmimum TTL for cached entries.
		/// </summary>
		public int cache_max_ttl
		{
			get => _cache_max_ttl;
			set
			{
				_cache_max_ttl = value;
				NotifyOfPropertyChange(() => cache_max_ttl);
			}
		}

		/// <summary>
		///     TTL for negatively cached entries.
		/// </summary>
		public int cache_neg_ttl
		{
			get => _cache_neg_ttl;
			set
			{
				_cache_neg_ttl = value;
				NotifyOfPropertyChange(() => cache_neg_ttl);
			}
		}

		/// <summary>
		///     Log client queries to a file.
		/// </summary>
		public QueryLog query_log { get; set; }

		/// <summary>
		///     Log queries for nonexistent zones.
		/// </summary>
		public NxLog nx_log { get; set; }

		/// <summary>
		///     Pattern-based blocking (blacklists).
		/// </summary>
		public Blacklist blacklist { get; set; }

		/// <summary>
		///     Pattern-based IP blocking (IP blacklists).
		/// </summary>
		public Blacklist ip_blacklist { get; set; }


		/// <summary>
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
		///     Remote lists of available servers.
		/// </summary>
		public Dictionary<string, Static> Static
		{
			get => _static;
			set
			{
				_static = value;
				NotifyOfPropertyChange(() => Static);
			}
		}
	}

	/// <summary>
	/// </summary>
	public class QueryLog : PropertyChangedBase
	{
		private string _format;
		private string _file;
		private ObservableCollection<string> _ignored_qtypes;

		[TomlIgnore]
		public new bool IsNotifying
		{
			get => base.IsNotifying;
			set => base.IsNotifying = value;
		}

		/// <summary>
		///     Query log format (SimpleDnsCrypt: ltsv).
		/// </summary>
		public string format
		{
			get => _format;
			set
			{
				_format = value;
				NotifyOfPropertyChange(() => format);
			}
		}

		/// <summary>
		///     Path to the query log file (absolute, or relative to the same directory as the executable file).
		/// </summary>
		public string file
		{
			get => _file;
			set
			{
				_file = value;
				NotifyOfPropertyChange(() => file);
			}
		}

		/// <summary>
		///     Do not log these query types, to reduce verbosity. Keep empty to log everything.
		/// </summary>
		public ObservableCollection<string> ignored_qtypes
		{
			get => _ignored_qtypes;
			set
			{
				_ignored_qtypes = value;
				NotifyOfPropertyChange(() => ignored_qtypes);
			}
		}
	}

	/// <summary>
	///     Log queries for nonexistent zones.
	/// </summary>
	public class NxLog : PropertyChangedBase
	{
		private string _format;
		private string _file;

		[TomlIgnore]
		public new bool IsNotifying
		{
			get => base.IsNotifying;
			set => base.IsNotifying = value;
		}

		/// <summary>
		///     Query log format (SimpleDnsCrypt: ltsv).
		/// </summary>
		public string format
		{
			get => _format;
			set
			{
				_format = value;
				NotifyOfPropertyChange(() => format);
			}
		}

		/// <summary>
		///     Path to the query log file (absolute, or relative to the same directory as the executable file).
		/// </summary>
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

	/// <summary>
	///     Blacklists.
	/// </summary>
	public class Blacklist : PropertyChangedBase
	{
		private string _log_format;
		private string _blacklist_file;
		private string _log_file;

		[TomlIgnore]
		public new bool IsNotifying
		{
			get => base.IsNotifying;
			set => base.IsNotifying = value;
		}

		/// <summary>
		///     Path to the file of blocking rules.
		/// </summary>
		public string blacklist_file
		{
			get => _blacklist_file;
			set
			{
				_blacklist_file = value;
				NotifyOfPropertyChange(() => blacklist_file);
			}
		}

		/// <summary>
		///     Path to a file logging blocked queries.
		/// </summary>
		public string log_file
		{
			get => _log_file;
			set
			{
				_log_file = value;
				NotifyOfPropertyChange(() => log_file);
			}
		}

		/// <summary>
		///     Log format (SimpleDnsCrypt: ltsv).
		/// </summary>
		public string log_format
		{
			get => _log_format;
			set
			{
				_log_format = value;
				NotifyOfPropertyChange(() => log_format);
			}
		}
	}

	public class Source : PropertyChangedBase
	{
		[TomlIgnore]
		public new bool IsNotifying
		{
			get => base.IsNotifying;
			set => base.IsNotifying = value;
		}

		private ObservableCollection<Stamp> _stamps;
		public string[] urls { get; set; }
		public string minisign_key { get; set; }
		public string cache_file { get; set; }
		public string format { get; set; }
		public int refresh_delay { get; set; }
		public string prefix { get; set; }

		[TomlIgnore]
		public ObservableCollection<Stamp> Stamps
		{
			get => _stamps;
			set
			{
				_stamps = value;
				NotifyOfPropertyChange(() => Stamps);
			}
		}
	}

	public class Static
	{
		public string stamp { get; set; }
	}
}