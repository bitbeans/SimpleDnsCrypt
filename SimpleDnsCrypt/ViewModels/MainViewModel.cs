using Caliburn.Micro;
using SimpleDnsCrypt.Config;
using SimpleDnsCrypt.Helper;
using SimpleDnsCrypt.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SimpleDnsCrypt.ViewModels
{
	[Export(typeof(MainViewModel))]
	public class MainViewModel : PropertyChangedBase, IShell
	{
		private readonly IWindowManager _windowManager;
		private readonly IEventAggregator _events;
		private string _windowTitle;
		private bool _showHiddenCards;
		private BindableCollection<LocalNetworkInterface> _localNetworkInterfaces =
			new BindableCollection<LocalNetworkInterface>();

		private bool _isWorkingOnService;
		private bool _isResolverRunning;
		private DnscryptProxyConfiguration _dnscryptProxyConfiguration;

		private ObservableCollection<Language> _languages;
		private Language _selectedLanguage;
		private bool _isSavingConfiguration;
		/// <summary>
		/// Initializes a new instance of the <see cref="MainViewModel"/> class
		/// </summary>
		/// <param name="windowManager">The window manager</param>
		/// <param name="events">The events</param>
		[ImportingConstructor]
		public MainViewModel(IWindowManager windowManager, IEventAggregator events)
		{
			_windowManager = windowManager;
			_events = events;
			_events.Subscribe(this);
			_windowTitle = $"{Global.ApplicationName} {VersionHelper.PublishVersion} {VersionHelper.PublishBuild}";

			_isSavingConfiguration = false;
			_isWorkingOnService = false;

			if (DnsCryptProxyManager.IsDnsCryptProxyInstalled())
			{
				if (DnsCryptProxyManager.IsDnsCryptProxyRunning())
				{
					_isResolverRunning = true;
				}
			}
			QueryLogViewModel = new QueryLogViewModel(_windowManager, _events);
			BlockLogViewModel = new BlockLogViewModel(_windowManager, _events);
			BlacklistViewModel = new BlacklistViewModel(_windowManager, _events);
		}

		public QueryLogViewModel QueryLogViewModel { get; }
		public BlockLogViewModel BlockLogViewModel { get; }
		public BlacklistViewModel BlacklistViewModel { get; }

		public void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!(e.Source is TabControl source)) return;
			var t = source;
			switch (t.SelectedIndex)
			{
				//main_page
				case 0:
					QueryLogViewModel.IsQueryLogLogging = false;
					BlockLogViewModel.IsBlockLogLogging = false;
					break;
				//query_log
				case 1:
					BlockLogViewModel.IsBlockLogLogging = false;
					break;
				//block_log
				case 2:
					QueryLogViewModel.IsQueryLogLogging = false;
					break;
				default:

					break;
			}
		}

		public void About()
		{
			var win = new AboutViewModel(_windowManager, _events)
			{
				WindowTitle = LocalizationEx.GetUiString("about", Thread.CurrentThread.CurrentCulture)
		};
			dynamic settings = new ExpandoObject();
			settings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
			_windowManager.ShowDialog(win, null, settings);
		}

		public DnscryptProxyConfiguration DnscryptProxyConfiguration
		{
			get => _dnscryptProxyConfiguration;
			set
			{
				if (value.Equals(_dnscryptProxyConfiguration)) return;
				_dnscryptProxyConfiguration = value;
				NotifyOfPropertyChange(() => DnscryptProxyConfiguration);
			}
		}

		public void SaveDnsCryptConfiguration()
		{
			IsSavingConfiguration = true;
			try
			{
				if (DnscryptProxyConfiguration == null) return;
				DnscryptProxyConfigurationManager.DnscryptProxyConfiguration = _dnscryptProxyConfiguration;
				if (DnscryptProxyConfigurationManager.SaveConfiguration())
				{
					DnscryptProxyConfigurationManager.LoadConfiguration();
					_dnscryptProxyConfiguration = DnscryptProxyConfigurationManager.DnscryptProxyConfiguration;
					if (DnsCryptProxyManager.IsDnsCryptProxyInstalled())
					{
						if (DnsCryptProxyManager.IsDnsCryptProxyRunning())
						{
							DnsCryptProxyManager.Restart();
						}
						else
						{
							DnsCryptProxyManager.Start();
						}
					}
				}
			}
			catch(Exception) { }
			IsSavingConfiguration = false;
		}

		/// <summary>
		///     The currently selected language.
		/// </summary>
		public Language SelectedLanguage
		{
			get => _selectedLanguage;
			set
			{
				if (value.Equals(_selectedLanguage)) return;
				_selectedLanguage = value;
				LocalizationEx.SetCulture(_selectedLanguage.ShortCode);
				NotifyOfPropertyChange(() => SelectedLanguage);
			}
		}

		/// <summary>
		///     List of all available languages.
		/// </summary>
		public ObservableCollection<Language> Languages
		{
			get => _languages;
			set
			{
				if (value.Equals(_languages)) return;
				_languages = value;
				NotifyOfPropertyChange(() => Languages);
			}
		}

		public bool IsResolverRunning
		{
			get => _isResolverRunning;
			set
			{
				HandleService();
				NotifyOfPropertyChange(() => IsResolverRunning);
			}
		}

		public bool IsSavingConfiguration
		{
			get => _isSavingConfiguration;
			set
			{
				_isSavingConfiguration = value;
				NotifyOfPropertyChange(() => IsSavingConfiguration);
			}
		}
		

		private async void HandleService()
		{
			IsWorkingOnService = true;
			if (IsResolverRunning)
			{
				// service is running, stop it
				await Task.Run(() => { DnsCryptProxyManager.Stop(); }).ConfigureAwait(false);
				Thread.Sleep(Global.ServiceStopTime);
				_isResolverRunning = DnsCryptProxyManager.IsDnsCryptProxyRunning();
				NotifyOfPropertyChange(() => IsResolverRunning);
				ResetNetworkCards();
			}
			else
			{
				if (DnsCryptProxyManager.IsDnsCryptProxyInstalled())
				{
					// service is installed, just start them
					await Task.Run(() => { DnsCryptProxyManager.Start(); }).ConfigureAwait(false);
					Thread.Sleep(Global.ServiceStartTime);
					_isResolverRunning = DnsCryptProxyManager.IsDnsCryptProxyRunning();
					NotifyOfPropertyChange(() => IsResolverRunning);
					ResetNetworkCards();
				}
				else
				{
					//install and start the service
					await Task.Run(() => { return DnsCryptProxyManager.Install(); }).ConfigureAwait(false);
					Thread.Sleep(Global.ServiceStartTime);
					DnsCryptProxyManager.Start();
					_isResolverRunning = DnsCryptProxyManager.IsDnsCryptProxyRunning();
					NotifyOfPropertyChange(() => IsResolverRunning);
					ResetNetworkCards();
				}
			}
			IsWorkingOnService = false;
		}

		private void ResetNetworkCards()
		{
			
		}

		public bool IsWorkingOnService
		{
			get => _isWorkingOnService;
			set
			{
				_isWorkingOnService = value;
				NotifyOfPropertyChange(() => IsWorkingOnService);
			}
		}

		public BindableCollection<LocalNetworkInterface> LocalNetworkInterfaces
		{
			get => _localNetworkInterfaces;
			set
			{
				_localNetworkInterfaces = value;
				NotifyOfPropertyChange(() => LocalNetworkInterfaces);
			}
		}


		/// <summary>
		///		The title of the window.
		/// </summary>
		public string WindowTitle
		{
			get => _windowTitle;
			set
			{
				_windowTitle = value;
				NotifyOfPropertyChange(() => WindowTitle);
			}
		}

		/// <summary>
		///     Show or hide the filtered network cards.
		/// </summary>
		public bool ShowHiddenCards
		{
			get => _showHiddenCards;
			set
			{
				_showHiddenCards = value;
				ReloadLoadNetworkInterfaces();
				NotifyOfPropertyChange(() => ShowHiddenCards);
			}
		}

		/// <summary>
		///     Load the local network cards.
		/// </summary>
		private void ReloadLoadNetworkInterfaces()
		{

			var localNetworkInterfaces = LocalNetworkInterfaceManager.GetLocalNetworkInterfaces(
				DnscryptProxyConfigurationManager.DnscryptProxyConfiguration.listen_addresses.ToList(), ShowHiddenCards);
			_localNetworkInterfaces.Clear();

			if (localNetworkInterfaces.Count == 0) return;

			foreach (var localNetworkInterface in localNetworkInterfaces)
			{
				_localNetworkInterfaces.Add(localNetworkInterface);
			}
		}

		public async void NetworkCardClicked(LocalNetworkInterface localNetworkInterface)
		{
			if (localNetworkInterface == null) return;
			if (!localNetworkInterface.IsChangeable) return;
			localNetworkInterface.IsChangeable = false;
			if (localNetworkInterface.UseDnsCrypt)
			{
				var status = LocalNetworkInterfaceManager.UnsetNameservers(localNetworkInterface);
				localNetworkInterface.UseDnsCrypt = !status;
			}
			else
			{
				// only add the local address if the proxy is running 
				if (DnsCryptProxyManager.IsDnsCryptProxyRunning())
				{
					var status = LocalNetworkInterfaceManager.SetNameservers(localNetworkInterface,
						LocalNetworkInterfaceManager.ConvertToDnsList(
							DnscryptProxyConfigurationManager.DnscryptProxyConfiguration.listen_addresses.ToList()));
					localNetworkInterface.UseDnsCrypt = status;
				}
			}
			await Task.Delay(1000).ConfigureAwait(false);
			localNetworkInterface.IsChangeable = true;
			ReloadLoadNetworkInterfaces();
		}
	}
}