namespace SimpleDnsCrypt.Models
{
    /// <summary>
    ///     Class to represent the main registry entry.
    /// </summary>
    public class DnsCryptProxy
    {
        /// <summary>
        ///     Initialize a new DnsCryptProxy instance.
        /// </summary>
        /// <param name="dnsCryptProxyType"></param>
        public DnsCryptProxy(DnsCryptProxyType dnsCryptProxyType)
        {
            Type = dnsCryptProxyType;
            IsReady = false;
            Parameter = new DnsCryptProxyParameter();
        }

        /// <summary>
        ///     The internal DnsCryptProxyType type.
        /// </summary>
        public DnsCryptProxyType Type { get; set; }

        /// <summary>
        ///     The display name of the service.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        ///     The imgae path of the service.
        /// </summary>
        public string ImagePath { get; set; }

        /// <summary>
        ///     Service parameter.
        /// </summary>
        public DnsCryptProxyParameter Parameter { get; set; }

        /// <summary>
        ///     The internal state of the service.
        /// </summary>
        public bool IsReady { get; set; }
    }
}