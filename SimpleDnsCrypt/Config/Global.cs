﻿namespace SimpleDnsCrypt.Config
{
	/// <summary>
	///     Global Easy DNSCrypt configuration.
	/// </summary>
	public static class Global
	{
		/// <summary>
		///     The name of this application.
		/// </summary>
		public const string ApplicationName = "Simple DNSCrypt";

		/// <summary>
		///     Remote URI where the application will find the update informations.
		/// </summary>
		public const string ApplicationUpdateUri =
			"https://raw.githubusercontent.com/bitbeans/SimpleDnsCrypt/master/update.yml";

		/// <summary>
		///     The public key to validate the installer.
		/// </summary>
		public const string ApplicationUpdatePublicKey = "RWTSM+4BNNvkZPNkHgE88ETlhWa+0HDzU5CN8TvbyvmhVUcr6aQXfssV";

		/// <summary>
		///     URL to the dnscrypt-resolvers.csv (hosted on github).
		/// </summary>
		public const string ResolverUrl =
			"https://raw.githubusercontent.com/jedisct1/dnscrypt-proxy/master/dnscrypt-resolvers.csv";

		/// <summary>
		///     Backup URL to the dnscrypt-resolvers.csv.
		/// </summary>
		public const string ResolverBackupUrl =
			"https://download.dnscrypt.org/dnscrypt-proxy/dnscrypt-resolvers.csv";
		
		/// <summary>
		///     URL to the minisign signature, to verify the downloaded dnscrypt-resolvers.csv (hosted on github).
		/// </summary>
		public const string SignatureUrl =
			"https://raw.githubusercontent.com/jedisct1/dnscrypt-proxy/master/dnscrypt-resolvers.csv.minisig";

		/// <summary>
		///     URL to the minisign signature, to verify the downloaded dnscrypt-resolvers.csv.
		/// </summary>
		public const string SignatureBackupUrl =
			"https://download.dnscrypt.org/dnscrypt-proxy/dnscrypt-resolvers.csv.minisig";

		/// <summary>
		///     Minisign public key, to verify the dnscrypt-resolvers.csv.
		/// </summary>
		public const string PublicKey = "RWQf6LRCGA9i53mlYecO4IzT51TGPpvWucNSCh1CBM0QTaLn73Y7GFO3";

		/// <summary>
		///     Local address the primary resolver will listen on.
		/// </summary>
		public const string PrimaryResolverAddress = "127.0.0.1";

		/// <summary>
		///     Local address the secondary resolver will listen on.
		/// </summary>
		public const string SecondaryResolverAddress = "127.0.0.2";

		/// <summary>
		///     Local address the primary resolver will listen on.
		/// </summary>
		public const string PrimaryResolverAddress6 = "::FFFF:127.0.0.1";

		/// <summary>
		///     Local address the secondary resolver will listen on.
		/// </summary>
		public const string SecondaryResolverAddress6 = "::FFFF:127.0.0.2";

		/// <summary>
		///     Address for the global gateway.
		/// </summary>
		public const string GlobalGatewayAddress = "0.0.0.0";

		/// <summary>
		///     Local port the primary resolver will listen on.
		/// </summary>
		public const int PrimaryResolverPort = 53;

		/// <summary>
		///     Local port the secondary resolver will listen on.
		/// </summary>
		public const int SecondaryResolverPort = 53;

		/// <summary>
		///     The name of the primary windows service.
		/// </summary>
		public const string PrimaryResolverServiceName = "dnscrypt-proxy";

		/// <summary>
		///     The name of the secondary windows service.
		/// </summary>
		public const string SecondaryResolverServiceName = "dnscrypt-proxy-secondary";

		/// <summary>
		///     The folder where the dnscrypt-proxy lives in.
		/// </summary>
		public const string DnsCryptProxyFolder = "dnscrypt-proxy";

		/// <summary>
		///     The name of the resolver file.
		/// </summary>
		public const string DnsCryptProxyResolverListName = "dnscrypt-resolvers.csv";

		/// <summary>
		///     The name of the resolver signature file.
		/// </summary>
		public const string DnsCryptProxySignatureFileName = "dnscrypt-resolvers.csv.minisig";

		/// <summary>
		///     The name of the dnscrypt-proxy executable.
		/// </summary>
		public const string DnsCryptProxyExecutableName = "dnscrypt-proxy.exe";

		/// <summary>
		///     The name of the dnscrypt-proxy executable.
		/// </summary>
		public const string DnsCryptProxyExecutableSecondaryName = "dnscrypt-proxy-secondary.exe";

		/// <summary>
		///     The default pimary resolver, if no resolver is set (on start up)
		/// </summary>
		public const string DefaultPrimaryResolverName = "dnscrypt.org-fr";

		/// <summary>
		///     The backup primary resolver, if DefaultPrimaryResolverName is not in the list.
		/// </summary>
		public const string DefaultPrimaryBackupResolverName = "ns0.dnscrypt.is";

		/// <summary>
		///     The default secondary resolver, if no resolver is set (on start up)
		/// </summary>
		public const string DefaultSecondaryResolverName = "dnscrypt.eu-nl";

		/// <summary>
		///     The backup secondary resolver, if DefaultSecondaryResolverName is not in the list.
		/// </summary>
		public const string DefaultSecondaryBackupResolverName = "dnscrypt.eu-dk";

		/// <summary>
		///     Time we wait on a service restart (ms).
		/// </summary>
		public const int ServiceRestartTime = 5000;

		/// <summary>
		///     Time we wait on a service start (ms).
		/// </summary>
		public const int ServiceStartTime = 2500;

		/// <summary>
		///     Time we wait on a service stop (ms).
		/// </summary>
		public const int ServiceStopTime = 2500;

		/// <summary>
		///     Time we wait on a service uninstall (ms).
		/// </summary>
		public const int ServiceUninstallTime = 2500;

		/// <summary>
		///     The name of the logfile, for the logging plugin.
		/// </summary>
		public const string DefaultLogFileName = "dns.log";

		/// <summary>
		///     If true, the resolver list will be downloaded on application start.
		/// </summary>
		public const bool UpdateResolverListOnStart = true;

		/// <summary>
		///     User configuration file.
		/// </summary>
		public const string UserConfigurationFile = "config.yaml";

		/// <summary>
		///     List of interfaces, marked as hidden.
		/// </summary>
		public static readonly string[] NetworkInterfaceBlacklist =
		{
			"Microsoft Virtual",
			"Hamachi Network",
			"VMware Virtual",
			"VirtualBox",
			"Software Loopback",
			"Microsoft ISATAP",
			"Microsoft-ISATAP",
			"Teredo Tunneling Pseudo-Interface",
			"Microsoft Wi-Fi Direct Virtual",
			"Microsoft Teredo Tunneling Adapter",
			"Von Microsoft gehosteter",
			"Microsoft hosted",
			"Virtueller Microsoft-Adapter",
			"TAP"
		};

		/// <summary>
		///     List of files must exist.
		/// </summary>
		public static readonly string[] DnsCryptProxyFiles =
		{
			"dnscrypt-proxy.exe",
			"dnscrypt-proxy-secondary.exe",
			"dnscrypt-resolvers.csv",
			"dnscrypt-resolvers.csv.minisig",
			"hostip.exe",
			"libdcplugin_ldns_aaaa_blocking.dll",
			"libdcplugin_logging.dll",
			"libdcplugin_ldns_blocking.dll",
			"libdcplugin_cache.dll",
			"libcrypto-41.dll",
			"libldns-2.dll",
			"libsodium-18.dll",
			"libwinpthread-1.dll"
		};

		/// <summary>
		///     Languages, the config file can handle.
		/// </summary>
		public static readonly string[] SupportedLanguages = {"bg", "de", "da", "en", "es", "fr", "fa", "id", "it", "nl", "ru", "sv", "tr", "zh"};

		/// <summary>
		/// 
		/// </summary>
		public const string DomainBlacklistConfigFile = "domains-blacklist.yaml";

		/// <summary>
		/// 
		/// </summary>
		public const string DomainBlacklistFile = "domains-blacklist.txt";

		/// <summary>
		/// 
		/// </summary>
		public const string AddressBlacklistConfigFile = "addresses-blacklist.yaml";

		/// <summary>
		/// 
		/// </summary>
		public const string AddressBlacklistFile = "addresses-blacklist.txt";

		public const string LibdcpluginLdns = "libdcplugin_ldns_blocking.dll";
		public const string LibdcpluginLdnsIpv6 = "libdcplugin_ldns_aaaa_blocking.dll";
		public const string LibdcpluginLogging = "libdcplugin_logging.dll";
		public const string LibdcpluginCache = "libdcplugin_cache.dll";
	}
}