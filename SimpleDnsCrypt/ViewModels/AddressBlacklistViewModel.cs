using Caliburn.Micro;
using SimpleDnsCrypt.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;

namespace SimpleDnsCrypt.ViewModels
{
	[Export(typeof(AddressBlacklistViewModel))]
	public class AddressBlacklistViewModel : Screen
	{

		private readonly IWindowManager _windowManager;
		private readonly IEventAggregator _events;

		private BindableCollection<string> _addressBlacklist;
		private string _selectedAddressBlacklistEntry;

		/// <summary>
		/// Initializes a new instance of the <see cref="AddressBlacklistViewModel"/> class
		/// </summary>
		/// <param name="windowManager">The window manager</param>
		/// <param name="events">The events</param>
		[ImportingConstructor]
		public AddressBlacklistViewModel(IWindowManager windowManager, IEventAggregator events)
		{
			_windowManager = windowManager;
			_events = events;
			_events.Subscribe(this);
			_addressBlacklist = new BindableCollection<string>();
			LoadAddressBlacklist();
		}

		public string SelectedAddressBlacklistEntry
		{
			get => _selectedAddressBlacklistEntry;
			set
			{
				if (value.Equals(_selectedAddressBlacklistEntry)) return;
				_selectedAddressBlacklistEntry = value;
				NotifyOfPropertyChange(() => SelectedAddressBlacklistEntry);
			}
		}

		public BindableCollection<string> AddressBlacklist
		{
			get => _addressBlacklist;
			set
			{
				if (value.Equals(_addressBlacklist)) return;
				_addressBlacklist = value;
				NotifyOfPropertyChange(() => AddressBlacklist);
			}
		}

		private void LoadAddressBlacklist()
		{
			try
			{

			}
			catch (Exception)
			{
			}
		}
	}
}
