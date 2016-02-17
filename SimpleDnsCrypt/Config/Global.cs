using System.Collections.Generic;

namespace SimpleDnsCrypt.Config
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
        public const string ApplicationUpdateUri = "https://raw.githubusercontent.com/bitbeans/SimpleDnsCrypt/master/update.yml";

		/// <summary>
		///		The public key to validate the installer.
		/// </summary>
	    public const string ApplicationUpdatePublicKey = "RWTSM+4BNNvkZPNkHgE88ETlhWa+0HDzU5CN8TvbyvmhVUcr6aQXfssV";

		/// <summary>
		///     URL to the dnscrypt-resolvers.csv (hosted on github).
		/// </summary>
		public const string ResolverUrl =
            "https://raw.githubusercontent.com/jedisct1/dnscrypt-proxy/master/dnscrypt-resolvers.csv";

        /// <summary>
        ///     URL to the minisign signature, to verify the downloaded dnscrypt-resolvers.csv (hosted on github).
        /// </summary>
        public const string SignatureUrl =
            "https://raw.githubusercontent.com/jedisct1/dnscrypt-proxy/master/dnscrypt-resolvers.csv.minisig";

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
        ///     The default pimary resolver, if no resolver is set (on start up)
        /// </summary>
        public const string DefaultPrimaryResolverName = "dnscrypt.me";

        /// <summary>
        ///     The backup primary resolver, if DefaultPrimaryResolverName is not in the list.
        /// </summary>
        public const string DefaultPrimaryBackupResolverName = "dnscrypt.org-fr";

        /// <summary>
        ///     The default secondary resolver, if no resolver is set (on start up)
        /// </summary>
        public const string DefaultSecondaryResolverName = "d0wn-random-ns1";

        /// <summary>
        ///     The backup secondary resolver, if DefaultSecondaryResolverName is not in the list.
        /// </summary>
        public const string DefaultSecondaryBackupResolverName = "dnscrypt.org-fr";

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
            "Von Microsoft gehosteter",
            "Microsoft hosted",
			"Virtueller Microsoft-Adapter"
		};

        /// <summary>
        ///     List of files must exist.
        /// </summary>
        public static readonly string[] DnsCryptProxyFiles =
        {
            "dnscrypt-proxy.exe",
            "dnscrypt-resolvers.csv",
            "dnscrypt-resolvers.csv.minisig",
            "hostip.exe",
            "libdcplugin_ldns_aaaa_blocking.dll",
            "libdcplugin_logging.dll",
			"libdcplugin_ldns_blocking.dll",
			"libeay32.dll",
            "libgcc_s_dw2-1.dll",
            "libldns-1.dll",
            "libsodium-18.dll",
            "libwinpthread-1.dll"
        };

		/// <summary>
		///     Checksums for DnsCryptProxyFiles (blake2b, without key).
		/// </summary>
		public static readonly Dictionary<string, string> DnsCryptProxyChecksumFiles = new Dictionary<string, string>()
		{
			{ "dnscrypt-proxy.exe","72d83da74358817a34d27e3ea18840b9bb4e59a8a0cd47428635bfbfc4338007b4c15d69b1cdde35acf250243e8b57cb542a82be5812d7f1764ca859ccea179b" },
			{ "hostip.exe","792d285cc638b3d3b95bf5fd644a05dfb136478c627066eb03b364d7f690e1402103930d48d02367ccf1f74ef81318159b621f90baf3fa956464f8ab2c241bcd" },
			{ "libdcplugin_ldns_aaaa_blocking.dll","88773c02f224138fd18d7117dc7ed142a5a251b785ae8008a8339cdd1abaf30a9f81a9e194c6d67e41f1ac14351edadc1e0dff8b0e6fa06463e88a7eb10e7207" },
			{ "libdcplugin_logging.dll","1b849f7e54406850432477676df7ce590661f18d051b045be254d1ecb42372555c95e513ebb6eda7a2c082929f0b745a067f36a2673857beda0519a6ba771676" },
			{ "libdcplugin_ldns_blocking.dll","3512e52524a194cc17e8d9c624390753c874e5e0175d5adcac7d8588515ee3d9b9087a72ebfcfff3c361fa88518aa8d78dcaecb58b2efa78d15ed9361421c839" },
			{ "libeay32.dll","538ba7955efd189acd892652dc1f6f36fb4ffb55dfcde88bf5b5372d248a77a9e9099e248d2b867054db84b92b5c42dafb43f1d5980ad2e39813f920e4392582" },
			{ "libgcc_s_dw2-1.dll","e3eb1ff32ecfb985209941345810aab8ab2858cd0502937ec6a0c605cffb7e7569417c437c8a399e5c7939156ac6c9c78a9095a0f6d988b26039c5b3891adf2d" },
			{ "libldns-1.dll","0e031bba0d50f9b5aafe7d7d8872aa790e384574015c2da173547d0c8cca445f7dd1505595b8736be146748b3753f7de5bf9770f7c29dda7e082a1f0aab09d9f" },
			{ "libsodium-18.dll","1b5476ebc3b2f3ededa871deb06d4ef156d048b757258931e0293c04103cd9d0035c30cdc0d0eaf9434e00091952d4ea201c304f7288a2269119085de4ebab43" },
			{ "libwinpthread-1.dll","f810fc15372ed43123bdfbffe4a1c03a70a42b46bd8cc76ba6238db691de964307924cc12dd7ee48d7680f8e9b7525487e580491c06a879ae1ebc898ef3edd0c" }
		};
	}
}