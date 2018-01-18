using System.Net.NetworkInformation;

namespace SimpleDnsCrypt.Lib.Models
{
	public class DnsServer
	{
		public string Address { get; set; }
		public NetworkInterfaceComponent Type { get; set; }
	}
}
