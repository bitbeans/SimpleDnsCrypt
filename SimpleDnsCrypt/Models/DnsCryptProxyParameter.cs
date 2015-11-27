namespace SimpleDnsCrypt.Models
{
    public class DnsCryptProxyParameter
    {
        public DnsCryptProxyParameter()
        {
            Plugins = new string[0];
        }

        public string[] Plugins { get; set; }
        public string ProviderName { get; set; }
        public string LocalAddress { get; set; }
		public int LocalPort { get; set; }
		public string ProviderKey { get; set; }
        public string ResolversList { get; set; }
        public string ResolverAddress { get; set; }
        public string ResolverName { get; set; }
        public bool EphemeralKeys { get; set; }
        public bool TcpOnly { get; set; }
    }
}