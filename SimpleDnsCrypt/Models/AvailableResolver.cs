using System.Collections.Generic;
using System.Threading;
using Caliburn.Micro;
using Newtonsoft.Json;
using SimpleDnsCrypt.Helper;

namespace SimpleDnsCrypt.Models
{
	public enum RouteState
	{
		Empty = 0,
		Invalid = 1,
		Valid = 2
	}

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
		private Route _route;
		private RouteState _routeState;

		[JsonIgnore]
		public string ToolTip => $"Ports: {string.Join(",", _ports.ToArray())}";
		[JsonIgnore]
		public string DisplayName => $"{_name} ({_protocol})";

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

		[JsonIgnore]
		public RouteState RouteState
		{
			get => _routeState;
			set
			{
				_routeState = value;
				NotifyOfPropertyChange(() => RouteState);
			}
		}

		[JsonIgnore]
		public string RouteStateText
		{
			get
			{
				switch (RouteState)
				{
					case RouteState.Empty:
						return LocalizationEx.GetUiString("configure_routes_add", Thread.CurrentThread.CurrentCulture);
					case RouteState.Invalid:
						return LocalizationEx.GetUiString("configure_routes_invalid", Thread.CurrentThread.CurrentCulture);
					case RouteState.Valid:
						return LocalizationEx.GetUiString("configure_routes_change", Thread.CurrentThread.CurrentCulture);
					default:
						return LocalizationEx.GetUiString("configure_routes_unknown", Thread.CurrentThread.CurrentCulture);
				}
			}
		}

		[JsonIgnore]
		public Route Route
		{
			get => _route;
			set
			{
				_route = value;
				NotifyOfPropertyChange(() => Route);
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
