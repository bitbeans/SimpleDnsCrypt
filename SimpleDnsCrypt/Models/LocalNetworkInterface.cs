using System.Collections.Generic;
using System.Net.NetworkInformation;
using Caliburn.Micro;

namespace SimpleDnsCrypt.Models
{
    public class LocalNetworkInterface : PropertyChangedBase
    {
        private string _name;
        private string _description;
        private NetworkInterfaceType _type;
        private List<string> _ipv4Dns;
        private List<string> _ipv6Dns;
        private bool _ipv6Support;
        private bool _ipv4Support;
        private bool _useDnsCrypt;
        private OperationalStatus _operationalStatus;
	    private bool _useInsecureFallbackDns;

	    public LocalNetworkInterface()
        {
            Ipv4Dns = new List<string>();
            Ipv6Dns = new List<string>();
        }

        public string Name
        {
            get => _name;
	        set {
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

        public List<string> Ipv4Dns
        {
            get => _ipv4Dns;
	        set
            {
                _ipv4Dns = value;
                NotifyOfPropertyChange(() => Ipv4Dns);
            }
        }

        public List<string> Ipv6Dns
        {
            get => _ipv6Dns;
	        set
            {
                _ipv6Dns = value;
                NotifyOfPropertyChange(() => Ipv6Dns);
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

        public bool UseDnsCrypt
        {
            get => _useDnsCrypt;
	        set
            {
                _useDnsCrypt = value;
                NotifyOfPropertyChange(() => UseDnsCrypt);
            }
        }

	    public bool UseInsecureFallbackDns
	    {
		    get => _useInsecureFallbackDns;
		    set
		    {
			    _useInsecureFallbackDns = value;
			    NotifyOfPropertyChange(() => UseInsecureFallbackDns);
		    }
	    }
	}
}