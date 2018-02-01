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

		[JsonIgnore]
		public string ToolTip => $"empty";

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
