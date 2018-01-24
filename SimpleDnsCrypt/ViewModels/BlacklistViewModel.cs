using Caliburn.Micro;
using SimpleDnsCrypt.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;

namespace SimpleDnsCrypt.ViewModels
{

	public class Rule : PropertyChangedBase
	{
		private string _content;

		public string Content
		{
			get => _content;
			set
			{
				if (value.Equals(_content)) return;
				_content = value;
				NotifyOfPropertyChange(() => Content);
			}
		}
	}

	[Export(typeof(BlacklistViewModel))]
	public class BlacklistViewModel : Screen
	{

	    private readonly IWindowManager _windowManager;
	    private readonly IEventAggregator _events;

		private ObservableCollection<Rule> _blacklist;
		private string _selectedBlacklistEntry;

		/// <summary>
		/// Initializes a new instance of the <see cref="BlacklistViewModel"/> class
		/// </summary>
		/// <param name="windowManager">The window manager</param>
		/// <param name="events">The events</param>
		[ImportingConstructor]
	    public BlacklistViewModel(IWindowManager windowManager, IEventAggregator events)
	    {
		    _windowManager = windowManager;
		    _events = events;
		    _events.Subscribe(this);
		    _blacklist = new ObservableCollection<Rule>();
			LoadBlacklist();
	    }
		
		public string SelectedBlacklistEntry
		{
			get => _selectedBlacklistEntry;
			set
			{
				if (value.Equals(_selectedBlacklistEntry)) return;
				_selectedBlacklistEntry = value;
				NotifyOfPropertyChange(() => SelectedBlacklistEntry);
			}
		}

		public ObservableCollection<Rule> Blacklist
		{
			get => _blacklist;
			set
			{
				if (value.Equals(_blacklist)) return;
				_blacklist = value;
				NotifyOfPropertyChange(() => Blacklist);
			}
		}

		private void LoadBlacklist()
		{
			try
			{
				var list = new List<Rule>();
				var blacklist = Path.Combine(Directory.GetCurrentDirectory(), Global.DnsCryptProxyFolder, "blacklist.txt");
				if (File.Exists(blacklist))
				{
					var rawBlacklist = File.ReadAllLines(blacklist);

					foreach (var rawBlacklistLine in rawBlacklist)
					{
						if (string.IsNullOrEmpty(rawBlacklistLine)) continue;
						if (rawBlacklistLine.StartsWith("#")) continue;
						list.Add(new Rule
						{
							Content = rawBlacklistLine.Trim()
						});
					}
				}
				_blacklist = new ObservableCollection<Rule>(list);
			}
			catch (Exception)
			{
			}
		}
	}
}
