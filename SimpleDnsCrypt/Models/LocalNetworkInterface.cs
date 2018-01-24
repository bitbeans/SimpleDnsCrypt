using Caliburn.Micro;
using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace SimpleDnsCrypt.Models
{
	public class LocalNetworkInterface : PropertyChangedBase
	{
		private string _name;
		private NetworkInterfaceType _type;
		private OperationalStatus _operationalStatus;
		private string _description;
		private List<DnsServer> _dns;
		private bool _ipv6Support;
		private bool _ipv4Support;
		private bool _useDnsCrypt;
		private bool _isChangeable;

		public LocalNetworkInterface()
		{
			Dns = new List<DnsServer>();
			_isChangeable = true;
		}

		public string Name
		{
			get => _name;
			set
			{
				_name = value;
				NotifyOfPropertyChange(() => Name);
			}
		}

		/// <summary>
		/// The status of the network card (up/down)
		/// </summary>
		public OperationalStatus OperationalStatus
		{
			get => _operationalStatus;
			set
			{
				_operationalStatus = value;
				NotifyOfPropertyChange(() => OperationalStatus);
			}
		}

		public string Description
		{
			get => _description;
			set
			{
				_description = value;
				NotifyOfPropertyChange(() => Description);
			}
		}

		public NetworkInterfaceType Type
		{
			get => _type;
			set
			{
				_type = value;
				NotifyOfPropertyChange(() => Type);
			}
		}

		public List<DnsServer> Dns
		{
			get => _dns;
			set
			{
				_dns = value;
				NotifyOfPropertyChange(() => Dns);
			}
		}

		public bool Ipv6Support
		{
			get => _ipv6Support;
			set
			{
				_ipv6Support = value;
				NotifyOfPropertyChange(() => Ipv6Support);
			}
		}

		public bool Ipv4Support
		{
			get => _ipv4Support;
			set
			{
				_ipv4Support = value;
				NotifyOfPropertyChange(() => Ipv4Support);
			}
		}

		public bool IsChangeable
		{
			get => _isChangeable;
			set
			{
				_isChangeable = value;
				NotifyOfPropertyChange(() => IsChangeable);
			}
		}

		public bool UseDnsCrypt
		{
			get => _useDnsCrypt;
			set
			{
				_useDnsCrypt = value;
				NotifyOfPropertyChange(() => UseDnsCrypt);
			}
		}
	}
}
