using System.Net.NetworkInformation;
using Caliburn.Micro;

namespace SimpleDnsCrypt.Models
{
	public class DnsServer : PropertyChangedBase
	{
		private NetworkInterfaceComponent _type;
		private string _address;

		public NetworkInterfaceComponent Type
		{
			get => _type;
			set
			{
				_type = value;
				NotifyOfPropertyChange(() => Type);
			}
		}

		public string Address
		{
			get => _address;
			set
			{
				_address = value;
				NotifyOfPropertyChange(() => Address);
			}
		}
	}
}
