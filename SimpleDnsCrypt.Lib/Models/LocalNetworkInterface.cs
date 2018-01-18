using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace SimpleDnsCrypt.Lib.Models
{
	public class LocalNetworkInterface
	{
		public LocalNetworkInterface()
		{
			Dns = new List<DnsServer>(4);
		}

		public string Name { get; set; }

		/// <summary>
		/// The status of the network card (up/down)
		/// </summary>
		public OperationalStatus OperationalStatus { get; set; }

		public string Description { get; set; }

		public NetworkInterfaceType Type { get; set; }

		public List<DnsServer> Dns { get; set; }

		public bool Ipv6Support { get; set; }

		public bool Ipv4Support { get; set; }

		public bool UseDnsCrypt { get; set; }
	}
}
