﻿using Caliburn.Micro;
using DnsCrypt.Models;
using Nett;
using SimpleDnsCrypt.Config;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

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
		private string _http_proxy;
		private int _timeout;
		private int _keepalive;
		private string _lb_strategy;
		private bool _lb_estimator;
		private int _cert_refresh_delay;
		private bool _dnscrypt_ephemeral_keys;
		private bool _tls_disable_session_tickets;
		private bool _log;
		private int _log_level;
		private string _log_file;
		private Dictionary<string, Source> _sources;
		private bool _use_syslog;
		private int _netprobe_timeout;
		private string _netprobe_address;
		private bool _offline_mode;
		private int _log_files_max_size;
		private int _log_files_max_age;
		private int _log_files_max_backups;
		private bool _block_ipv6;
		private bool _block_unqualified;
		private bool _block_undelegated;
		private int _reject_ttl;
		private string _forwarding_rules;
		private string _cloaking_rules;
		private int _cache_neg_min_ttl;
		private int _cache_neg_max_ttl;
		private int _cache_max_ttl;
		private int _cache_min_ttl;
		private int _cache_size;
		private bool _cache;
		private ObservableCollection<string> _fallback_resolvers;
		private bool _ignore_system_dns;
		private Dictionary<string, Static> _static;
		private string _proxy;
		private ObservableCollection<string> _disabled_server_names;
		private string _blocked_query_response;


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
		///     Server names to avoid even if they match all criteria
		/// </summary>
		public ObservableCollection<string> disabled_server_names
		{
			get => _disabled_server_names;
			set
			{
				_disabled_server_names = value;
				NotifyOfPropertyChange(() => disabled_server_names);
			}
		}

		/// <summary>
		///		Response for blocked queries.  Options are `refused`, `hinfo` (default) or
		///		an IP response.  To give an IP response, use the format `a:<IPv4>,aaaa:<IPv6>`.
		///		Using the `hinfo` option means that some responses will be lies.
		///		Unfortunately, the `hinfo` option appears to be required for Android 8+
		/// </summary>
		public string blocked_query_response
		{
			get => _blocked_query_response;
			set
			{
				_blocked_query_response = value;
				NotifyOfPropertyChange(() => blocked_query_response);
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
		/// DNSCrypt: Create a new, unique key for every single DNS query
		/// This may improve privacy but can also have a significant impact on CPU usage
		/// Only enable if you don't have a lot of network load
		/// </summary>
		public bool dnscrypt_ephemeral_keys
		{
			get => _dnscrypt_ephemeral_keys;
			set
			{
				_dnscrypt_ephemeral_keys = value;
				NotifyOfPropertyChange(() => dnscrypt_ephemeral_keys);
			}
		}

		/// <summary>
		/// DoH: Disable TLS session tickets - increases privacy but also latency.
		/// </summary>
		public bool tls_disable_session_tickets
		{
			get => _tls_disable_session_tickets;
			set
			{
				_tls_disable_session_tickets = value;
				NotifyOfPropertyChange(() => tls_disable_session_tickets);
			}
		}

		/// <summary>
		/// Offline mode - Do not use any remote encrypted servers.
		/// The proxy will remain fully functional to respond to queries that
		/// plugins can handle directly (forwarding, cloaking, ...)
		/// </summary>
		public bool offline_mode
		{
			get => _offline_mode;
			set
			{
				_offline_mode = value;
				NotifyOfPropertyChange(() => offline_mode);
			}
		}

		/// <summary>
		/// SOCKS proxy
		/// Uncomment the following line to route all TCP connections to a local Tor node
		/// Tor doesn't support UDP, so set `force_tcp` to `true` as well.
		/// </summary>
		public string proxy
		{
			get => _proxy;
			set
			{
				_proxy = value;
				NotifyOfPropertyChange(() => proxy);
			}
		}

		/// <summary>
		/// HTTP/HTTPS proxy
		/// Only for DoH servers
		/// </summary>
		public string http_proxy
		{
			get => _http_proxy;
			set
			{
				_http_proxy = value;
				NotifyOfPropertyChange(() => http_proxy);
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
		///     Keepalive for HTTP (HTTPS, HTTP/2) queries, in seconds.
		/// </summary>
		public int keepalive
		{
			get => _keepalive;
			set
			{
				_keepalive = value;
				NotifyOfPropertyChange(() => keepalive);
			}
		}

		/// <summary>
		/// Load-balancing strategy: 'p2' (default), 'ph', 'first' or 'random'
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
		/// Set to `true` to constantly try to estimate the latency of all the resolvers
		/// and adjust the load-balancing parameters accordingly, or to `false` to disable.
		/// </summary>
		public bool lb_estimator
		{
			get => _lb_estimator;
			set
			{
				_lb_estimator = value;
				NotifyOfPropertyChange(() => lb_estimator);
			}
		}

		/// <summary>
		/// Maximum time (in seconds) to wait for network connectivity before initializing the proxy.
		/// Useful if the proxy is automatically started at boot, and network
		/// connectivity is not guaranteed to be immediately available.
		/// Use 0 to disable.
		/// </summary>
		public int netprobe_timeout
		{
			get => _netprobe_timeout;
			set
			{
				_netprobe_timeout = value;
				NotifyOfPropertyChange(() => netprobe_timeout);
			}
		}

		/// <summary>
		/// Address and port to try initializing a connection to, just to check
		/// if the network is up. It can be any address and any port, even if
		/// there is nothing answering these on the other side. Just don't use
		/// a local address, as the goal is to check for Internet connectivity.
		/// On Windows, a datagram with a single, nul byte will be sent, only
		/// when the system starts.
		/// On other operating systems, the connection will be initialized
		/// but nothing will be sent at all.
		/// </summary>
		public string netprobe_address
		{
			get => _netprobe_address;
			set
			{
				_netprobe_address = value;
				NotifyOfPropertyChange(() => netprobe_address);
			}
		}

		/// <summary>
		///		Enable or disable dnscrypt proxy logging.
		/// </summary>
		[TomlIgnore]
		public bool log
		{
			get
			{
				if (string.IsNullOrEmpty(log_file)) return false;
				//TODO: make Configurable 
				var logFile = Path.Combine(Directory.GetCurrentDirectory(), Global.LogDirectory, Global.DnsCryptLogFile);
				return log_file.Equals(logFile);
			}
			set
			{
				_log = value;
				if (value)
				{
					var logFile = Path.Combine(Directory.GetCurrentDirectory(), Global.LogDirectory, Global.DnsCryptLogFile);
					_log_level = 0;
					_log_file = logFile;
				}
				else
				{
					_log_level = 0;
					_log_file = null;
				}
				NotifyOfPropertyChange(() => log);
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
		///     Fallback resolvers
		/// 	These are normal, non-encrypted DNS resolvers, that will be only used
		/// 	for one-shot queries when retrieving the initial resolvers list, and
		/// 	only if the system DNS configuration doesn't work.
		/// 	No user application queries will ever be leaked through these resolvers,
		/// 	and they will not be used after IP addresses of resolvers URLs have been found.
		/// 	They will never be used if lists have already been cached, and if stamps
		/// 	don't include host names without IP addresses.
		/// 	They will not be used if the configured system DNS works.
		/// 	Resolvers supporting DNSSEC are recommended.
		/// 	
		/// 	People in China may need to use 114.114.114.114:53 here.
		/// 	Other popular options include 8.8.8.8 and 1.1.1.1.
		/// 	
		/// 	If more than one resolver is specified, they will be tried in sequence.
		/// </summary>
		public ObservableCollection<string> fallback_resolvers
		{
			get => _fallback_resolvers;
			set
			{
				_fallback_resolvers = value;
				NotifyOfPropertyChange(() => fallback_resolvers);
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
		///     Immediately respond to A and AAAA queries for host names without a domain name.
		/// </summary>
		public bool block_unqualified
		{
			get => _block_unqualified;
			set
			{
				_block_unqualified = value;
				NotifyOfPropertyChange(() => block_unqualified);
			}
		}

		/// <summary>
		///     Immediately respond to queries for local zones instead of leaking them to
		///     upstream resolvers (always causing errors or timeouts).
		/// </summary>
		public bool block_undelegated
		{
			get => _block_undelegated;
			set
			{
				_block_undelegated = value;
				NotifyOfPropertyChange(() => block_undelegated);
			}
		}

		/// <summary>
		///		TTL for synthetic responses sent when a request has been blocked (due to IPv6 or blacklists).
		/// </summary>
		public int reject_ttl
		{
			get => _reject_ttl;
			set
			{
				_reject_ttl = value;
				NotifyOfPropertyChange(() => reject_ttl);
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
		///     Cloaking rule file.
		/// </summary>
		public string cloaking_rules
		{
			get => _cloaking_rules;
			set
			{
				_cloaking_rules = value;
				NotifyOfPropertyChange(() => cloaking_rules);
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
		///     Minimum TTL for negatively cached entries.
		/// </summary>
		public int cache_neg_min_ttl
		{
			get => _cache_neg_min_ttl;
			set
			{
				_cache_neg_min_ttl = value;
				NotifyOfPropertyChange(() => cache_neg_min_ttl);
			}
		}

		/// <summary>
		///     Maximum TTL for negatively cached entries
		/// </summary>
		public int cache_neg_max_ttl
		{
			get => _cache_neg_max_ttl;
			set
			{
				_cache_neg_max_ttl = value;
				NotifyOfPropertyChange(() => cache_neg_max_ttl);
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
		public Blacklist blocked_names { get; set; }

		/// <summary>
		///     Pattern-based IP blocking (IP blacklists).
		/// </summary>
		public Blacklist blocked_ips { get; set; }

		public AnonymizedDns anonymized_dns { get; set; }

		public BrokenImplementations broken_implementations { get; set; }

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

	public class AnonymizedDns : PropertyChangedBase
	{
		[TomlIgnore]
		public new bool IsNotifying
		{
			get => base.IsNotifying;
			set => base.IsNotifying = value;
		}

		private List<Route> _routes;
		private bool _skip_incompatible;

		public List<Route> routes
		{
			get => _routes;
			set
			{
				_routes = value;
				NotifyOfPropertyChange(() => _routes);
			}
		}

		/// <summary>
		/// skip resolvers incompatible with anonymization instead of using them directly.
		/// </summary>
		public bool skip_incompatible
		{
			get => _skip_incompatible;
			set
			{
				_skip_incompatible = value;
				NotifyOfPropertyChange(() => skip_incompatible);
			}
		}
	}

	public class BrokenImplementations : PropertyChangedBase
	{
		[TomlIgnore]
		public new bool IsNotifying
		{
			get => base.IsNotifying;
			set => base.IsNotifying = value;
		}

		private List<string> _broken_query_padding;
		private List<string> _fragments_blocked;

		public List<string> broken_query_padding
		{
			get => _broken_query_padding;
			set
			{
				_broken_query_padding = value;
				NotifyOfPropertyChange(() => broken_query_padding);
			}
		}

		public List<string> fragments_blocked
		{
			get => _fragments_blocked;
			set
			{
				_fragments_blocked = value;
				NotifyOfPropertyChange(() => fragments_blocked);
			}
		}
	}

	public class Route : PropertyChangedBase
	{
		[TomlIgnore]
		public new bool IsNotifying
		{
			get => base.IsNotifying;
			set => base.IsNotifying = value;
		}

		private string _server_name;

		public string server_name
		{
			get => _server_name;
			set
			{
				_server_name = value;
				NotifyOfPropertyChange(() => _server_name);
			}
		}

		private ObservableCollection<string> _via;

		public ObservableCollection<string> via
		{
			get => _via;
			set
			{
				_via = value;
				NotifyOfPropertyChange(() => via);
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