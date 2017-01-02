using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SimpleDnsCrypt.Models
{
	public class LocaleRule
	{
		public string Rule { get; set; }
	}

	public class RemoteRule
	{
		public string Rule { get; set; }
	}

	public class DomainBlacklist
	{
		public ObservableCollection<LocaleRule> LocalRules { get; set; }
		public ObservableCollection<RemoteRule> RemoteRules { get; set; }

		public DomainBlacklist()
		{
			LocalRules = new ObservableCollection<LocaleRule>();
			RemoteRules = new ObservableCollection<RemoteRule>();
		}
	}

	public class AddressBlacklist
	{
		public ObservableCollection<string> Rules { get; set; }

		public AddressBlacklist()
		{
			Rules = new ObservableCollection<string>();
		}
	}
}
