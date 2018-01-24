namespace SimpleDnsCrypt.Config
{
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
	    ///     Remote URI where the application will find the update informations.
	    /// </summary>
	    public const string ApplicationUpdateUri64 =
		    "https://raw.githubusercontent.com/bitbeans/SimpleDnsCrypt/master/update64.yml";

	    /// <summary>
	    ///     The public key to validate the installer.
	    /// </summary>
	    public const string ApplicationUpdatePublicKey = "RWTSM+4BNNvkZPNkHgE88ETlhWa+0HDzU5CN8TvbyvmhVUcr6aQXfssV";

	    /// <summary>
	    ///     The folder where the dnscrypt-proxy lives in.
	    /// </summary>
	    public const string DnsCryptProxyFolder = "dnscrypt-proxy";

	    public const string DnsCryptProxyExecutableName = "dnscrypt-proxy.exe";

		public const string DnsCryptConfigurationFile = "dnscrypt-proxy.toml";

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
		///     List of files must exist.
		/// </summary>
		public static readonly string[] DnsCryptProxyFiles =
	    {
		    "dnscrypt-proxy.exe",
			"dnscrypt-proxy.toml",
			"LICENSE"
	    };

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
	}
}
