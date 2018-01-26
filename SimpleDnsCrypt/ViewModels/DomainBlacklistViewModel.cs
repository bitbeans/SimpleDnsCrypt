using Caliburn.Micro;
using SimpleDnsCrypt.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using SimpleDnsCrypt.Models;

namespace SimpleDnsCrypt.ViewModels
{
	[Export(typeof(DomainBlacklistViewModel))]
	public class DomainBlacklistViewModel : Screen
	{

	    private readonly IWindowManager _windowManager;
	    private readonly IEventAggregator _events;

		private ObservableCollection<BlacklistRule> _domainBlacklist;
		private string _selectedDomainBlacklistEntry;

		/// <summary>
		/// Initializes a new instance of the <see cref="DomainBlacklistViewModel"/> class
		/// </summary>
		/// <param name="windowManager">The window manager</param>
		/// <param name="events">The events</param>
		[ImportingConstructor]
	    public DomainBlacklistViewModel(IWindowManager windowManager, IEventAggregator events)
	    {
		    _windowManager = windowManager;
		    _events = events;
		    _events.Subscribe(this);
		    _domainBlacklist = new ObservableCollection<BlacklistRule>();
		    LoadDomainBlacklist();
	    }
		
		public string SelectedDomainBlacklistEntry
		{
			get => _selectedDomainBlacklistEntry;
			set
			{
				if (value.Equals(_selectedDomainBlacklistEntry)) return;
				_selectedDomainBlacklistEntry = value;
				NotifyOfPropertyChange(() => SelectedDomainBlacklistEntry);
			}
		}

		public ObservableCollection<BlacklistRule> DomainBlacklist
		{
			get => _domainBlacklist;
			set
			{
				if (value.Equals(_domainBlacklist)) return;
				_domainBlacklist = value;
				NotifyOfPropertyChange(() => DomainBlacklist);
			}
		}

		private void LoadDomainBlacklist()
		{
			try
			{
				var list = new List<BlacklistRule>();
				var blacklist = Path.Combine(Directory.GetCurrentDirectory(), Global.DnsCryptProxyFolder, "blacklist.txt");
				if (File.Exists(blacklist))
				{
					var rawBlacklist = File.ReadAllLines(blacklist);

					foreach (var rawBlacklistLine in rawBlacklist)
					{
						if (string.IsNullOrEmpty(rawBlacklistLine)) continue;
						if (rawBlacklistLine.StartsWith("#")) continue;
						list.Add(new BlacklistRule
						{
							Content = rawBlacklistLine.Trim()
						});
					}
				}
				_domainBlacklist = new ObservableCollection<BlacklistRule>(list);
			}
			catch (Exception)
			{
			}
		}
	}
}
