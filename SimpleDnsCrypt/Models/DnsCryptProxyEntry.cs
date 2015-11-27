using Caliburn.Micro;

namespace SimpleDnsCrypt.Models
{
    /// <summary>
    ///     Class to represent an entry in the resolver list.
    /// </summary>
    public class DnsCryptProxyEntry : PropertyChangedBase
    {
        /// <summary>
        ///     The short name of the resolver.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     The full name of the resolver.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        ///     The description of the resolver.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     Location of the resolver.
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        ///     Coordinates of the resolver.
        /// </summary>
        public string Coordinates { get; set; }

        /// <summary>
        ///     Website of the resolver.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        ///     Version of the resolver.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        ///     DNSSEC validation support.
        /// </summary>
        public bool DnssecValidation { get; set; }

        /// <summary>
        ///     No log support.
        /// </summary>
        public bool NoLogs { get; set; }

        /// <summary>
        ///     Namecoin support.
        /// </summary>
        public bool Namecoin { get; set; }

        /// <summary>
        ///     The IPv4 or IPv6 address of the resolver.
        /// </summary>
        public string ResolverAddress { get; set; }

        /// <summary>
        ///     The name of the resolvers provider.
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        ///     The public key of the resolvers provider.
        /// </summary>
        public string ProviderPublicKey { get; set; }

        /// <summary>
        ///     The TXT entry of the providers resolver.
        /// </summary>
        public string ProviderPublicKeyTextRecord { get; set; }

		/// <summary>
		///     The local port (not part of the CSV file).
		/// </summary>
		public int LocalPort { get; set; }
	}
}