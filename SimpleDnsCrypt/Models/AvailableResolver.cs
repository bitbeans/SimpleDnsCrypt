using System.Collections.Generic;
using Caliburn.Micro;
using Newtonsoft.Json;

namespace SimpleDnsCrypt.Models
{
	public class AvailableResolver : PropertyChangedBase
	{
		private bool _isInServerList;
		private string _name;
		private string _protocol;
		private bool _dnsSec;
		private bool _noLog;
		private bool _noFilter;
		private string _description;
		private bool _ipv6;
		private List<int> _ports;

		[JsonIgnore]
		public string ToolTip => $"Ports: {string.Join(",", _ports.ToArray())}";

		[JsonIgnore]
		public bool IsInServerList
		{
			get => _isInServerList;
			set
			{
				_isInServerList = value;
				NotifyOfPropertyChange(() => IsInServerList);
			}
		}

		[JsonProperty("name")]
		public string Name
		{
			get => _name;
			set
			{
				_name = value;
				NotifyOfPropertyChange(() => Name);
			}
		}

		[JsonProperty("proto")]
		public string Protocol
		{
			get => _protocol;
			set
			{
				_protocol = value;
				NotifyOfPropertyChange(() => Protocol);
			}
		}

		[JsonProperty("ports")]
		public List<int> Ports
		{
			get => _ports;
			set
			{
				_ports = value;
				NotifyOfPropertyChange(() => Ports);
			}
		}

		[JsonProperty("ipv6")]
		public bool Ipv6
		{
			get => _ipv6;
			set
			{
				_ipv6 = value;
				NotifyOfPropertyChange(() => Ipv6);
			}
		}

		[JsonProperty("dnssec")]
		public bool DnsSec
		{
			get => _dnsSec;
			set
			{
				_dnsSec = value;
				NotifyOfPropertyChange(() => DnsSec);
			}
		}

		[JsonProperty("nolog")]
		public bool NoLog
		{
			get => _noLog;
			set
			{
				_noLog = value;
				NotifyOfPropertyChange(() => NoLog);
			}
		}

		[JsonProperty("nofilter")]
		public bool NoFilter
		{
			get => _noFilter;
			set
			{
				_noFilter = value;
				NotifyOfPropertyChange(() => NoFilter);
			}
		}

		[JsonProperty("description")]
		public string Description
		{
			get => _description;
			set
			{
				_description = value;
				NotifyOfPropertyChange(() => Description);
			}
		}
	}
}
