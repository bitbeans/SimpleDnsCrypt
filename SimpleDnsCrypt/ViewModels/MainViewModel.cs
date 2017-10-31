using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Caliburn.Micro;
using MaterialDesignThemes.Wpf;
using SimpleDnsCrypt.Config;
using SimpleDnsCrypt.Models;
using SimpleDnsCrypt.Tools;
using WPFLocalizeExtension.Engine;
using RedistributableChecker;

namespace SimpleDnsCrypt.ViewModels
{
	/// <summary>
	///     The MainViewModel.
	/// </summary>
	[Export]
	public sealed class MainViewModel : Screen, IShell
	{
		private readonly BindableCollection<LocalNetworkInterface> _localNetworkInterfaces =
			new BindableCollection<LocalNetworkInterface>();

		private ISnackbarMessageQueue _messageQueue;
		private CancellationTokenSource _analyseCancellationTokenSource = new CancellationTokenSource();
		private string _windowTitle;
		private readonly UserData _userData;
		private readonly IWindowManager _windowManager;
		private bool _actAsGlobalGateway;
		private bool _isAnalysing;
		private bool _isPrimaryResolverRunning;
		private bool _isRefreshingResolverList;
		private bool _isSecondaryResolverRunning;
		private bool _isUninstallingServices;
		private bool _isWorkingOnPrimaryService;
		private bool _isWorkingOnSecondaryService;
		private ObservableCollection<Language> _languages;
		private int _overlayDependencies;
		private List<string> _plugins;
		private DnsCryptProxyEntry _primaryResolver;
		private string _primaryResolverTitle;
		private List<DnsCryptProxyEntry> _resolvers;
		private DnsCryptProxyEntry _secondaryResolver;
		private string _secondaryResolverTitle;
		private Language _selectedLanguage;
		private bool _showHiddenCards;
		private bool _updateResolverListOnStart;
		private bool _useTcpOnly;
		private bool _isCheckingUpdates;
		private bool _filterDnssec;
		private bool _filterNoLogs;
		private bool _filterIpv4;
		private bool _filterIpv6;
		private readonly string _proxyList;
		private readonly string _proxyListSignature;
		private bool _unsavedChanges;


		/// <summary>
		///     MainViewModel construcor for XAML.
		/// </summary>
		public MainViewModel()
		{
		}

		/// <summary>
		///     MainViewModel construcor.
		/// </summary>
		/// <param name="windowManager">The current window manager.</param>
		/// <param name="eventAggregator">The event aggregator.</param>
		[ImportingConstructor]
		private MainViewModel(IWindowManager windowManager, IEventAggregator eventAggregator)
		{
			try
			{
				Instance = this;
				_windowManager = windowManager;
				eventAggregator.Subscribe(this);
				_userData = new UserData(Path.Combine(Directory.GetCurrentDirectory(), Global.UserConfigurationFile));
				// fill the language combobox
				_languages = LocalizationEx.GetSupportedLanguages();
				// automatically use the correct translations if available (fallback: en)
				LocalizeDictionary.Instance.SetCurrentThreadCulture = true;
				// overwrite language detection 
				LocalizeDictionary.Instance.Culture = _userData.Language.Equals("auto")
					? Thread.CurrentThread.CurrentCulture
					: new CultureInfo(_userData.Language);
				// select the current language in the combobox
				_selectedLanguage =
					_languages.SingleOrDefault(l => l.ShortCode.Equals(LocalizeDictionary.Instance.Culture.TwoLetterISOLanguageName));
				_messageQueue = new SnackbarMessageQueue(new TimeSpan(0, 0, 0, 5));
				
				// this is already defined in the app.manifest, but to be sure check it again
				if (!IsAdministrator())
				{
					_windowManager.ShowMetroMessageBox(
						LocalizationEx.GetUiString("dialog_message_bad_privileges", Thread.CurrentThread.CurrentCulture),
						LocalizationEx.GetUiString("dialog_error_title", Thread.CurrentThread.CurrentCulture),
						MessageBoxButton.OK, BoxType.Error);
					Environment.Exit(1);
				}

				CheckRedistributablePackageVersion();

				// do a simple check, if all needed files are available
				if (!ValidateDnsCryptProxyFolder())
				{
					_windowManager.ShowMetroMessageBox(
						LocalizationEx.GetUiString("dialog_message_missing_proxy_files",
							Thread.CurrentThread.CurrentCulture),
						LocalizationEx.GetUiString("dialog_error_title", Thread.CurrentThread.CurrentCulture),
						MessageBoxButton.OK, BoxType.Error);
					Environment.Exit(1);
				}

				SetWindowTitle(_userData.UseIpv6);
				_filterIpv6 = _userData.UseIpv6;
				_filterIpv4 = _userData.UseIpv4;
				_filterDnssec = _userData.OnlyUseDnssec;
				_filterNoLogs = _userData.OnlyUseNoLogs;
				_resolvers = new List<DnsCryptProxyEntry>();
				_updateResolverListOnStart = _userData.UpdateResolverListOnStart;
				_isWorkingOnPrimaryService = false;
				_isWorkingOnSecondaryService = false;
				_isAnalysing = false;
				_unsavedChanges = false;
				LocalNetworkInterfaces = new CollectionViewSource {Source = _localNetworkInterfaces};
				PrimaryDnsCryptProxyManager = new DnsCryptProxyManager(DnsCryptProxyType.Primary);
				SecondaryDnsCryptProxyManager = new DnsCryptProxyManager(DnsCryptProxyType.Secondary);
				

				if (PrimaryDnsCryptProxyManager.DnsCryptProxy.Parameter.TcpOnly ||
				    SecondaryDnsCryptProxyManager.DnsCryptProxy.Parameter.TcpOnly)
				{
					_useTcpOnly = true;
				}

				// check the primary resolver for plugins
				if (PrimaryDnsCryptProxyManager.DnsCryptProxy.Parameter.Plugins.Any())
				{
					_plugins = PrimaryDnsCryptProxyManager.DnsCryptProxy.Parameter.Plugins.ToList();
				}
				else
				{
					if (SecondaryDnsCryptProxyManager.DnsCryptProxy.Parameter.Plugins.Any())
					{
						_plugins = SecondaryDnsCryptProxyManager.DnsCryptProxy.Parameter.Plugins.ToList();
					}
					else
					{
						// no stored plugins
						_plugins = new List<string>();
					}
				}
				_proxyList = Path.Combine(Directory.GetCurrentDirectory(),
					Global.DnsCryptProxyFolder, Global.DnsCryptProxyResolverListName);
				_proxyListSignature = Path.Combine(Directory.GetCurrentDirectory(),
					Global.DnsCryptProxyFolder, Global.DnsCryptProxySignatureFileName);
				if (!File.Exists(_proxyList) || !File.Exists(_proxyListSignature) || UpdateResolverListOnStart)
				{
					// download and verify the proxy list if there is no one.
					AsyncHelpers.RunSync(DnsCryptProxyListManager.UpdateResolverListAsync);
				}

				var dnsProxyList = DnsCryptProxyListManager.ReadProxyList(_proxyList, _proxyListSignature, _userData.OnlyUseNoLogs, _userData.OnlyUseDnssec, _userData.UseIpv4, _userData.UseIpv6);
				if (dnsProxyList != null && dnsProxyList.Any())
				{
					foreach (var dnsProxy in dnsProxyList)
					{
						if (dnsProxy.Name.Equals(PrimaryDnsCryptProxyManager.DnsCryptProxy.Parameter.ResolverName))
						{
							_primaryResolver = dnsProxy;
							// restore the local port and address
							_primaryResolver.LocalPort = PrimaryDnsCryptProxyManager.DnsCryptProxy.Parameter.LocalPort;
							_primaryResolver.LocalAddress = PrimaryDnsCryptProxyManager.DnsCryptProxy.Parameter.LocalAddress;
						}
						if (dnsProxy.Name.Equals(SecondaryDnsCryptProxyManager.DnsCryptProxy.Parameter.ResolverName))
						{
							_secondaryResolver = dnsProxy;
						}
						_resolvers.Add(dnsProxy);
					}
				}
				else
				{
					_windowManager.ShowMetroMessageBox(
						string.Format(
							LocalizationEx.GetUiString("dialog_message_missing_file",
								Thread.CurrentThread.CurrentCulture),
							_proxyList, _proxyListSignature),
						LocalizationEx.GetUiString("dialog_error_title", Thread.CurrentThread.CurrentCulture),
						MessageBoxButton.OK, BoxType.Error);
					Environment.Exit(1);
				}

				// if there is no selected primary resolver, add a default resolver
				if (PrimaryResolver == null)
				{
					DnsCryptProxyEntry defaultResolver;
					// first check the config file
					if (_userData.PrimaryResolver.Equals("auto"))
					{
						// automatic, so choose the DefaultPrimaryResolver
						defaultResolver = dnsProxyList.SingleOrDefault(d => d.Name.Equals(Global.DefaultPrimaryResolverName)) ??
						                  dnsProxyList.SingleOrDefault(d => d.Name.Equals(Global.DefaultPrimaryBackupResolverName));
					}
					else
					{
						defaultResolver = dnsProxyList.SingleOrDefault(d => d.Name.Equals(_userData.PrimaryResolver)) ??
						                  (dnsProxyList.SingleOrDefault(d => d.Name.Equals(Global.DefaultPrimaryResolverName)) ??
						                   dnsProxyList.SingleOrDefault(d => d.Name.Equals(Global.DefaultPrimaryBackupResolverName)));
					}

					PrimaryResolver = defaultResolver ?? dnsProxyList.ElementAt(0);
					if (_primaryResolver.LocalPort == 0)
					{
						_primaryResolver.LocalPort = Global.PrimaryResolverPort;
					}
					if (_primaryResolver.LocalAddress == null)
					{
						_primaryResolver.LocalAddress = Global.PrimaryResolverAddress;
					}
				}

				// if there is no selected secondary resolver, add a default resolver
				if (SecondaryResolver == null)
				{
					DnsCryptProxyEntry defaultResolver;
					// first check the config file
					if (_userData.SecondaryResolver.Equals("auto"))
					{
						// automatic, so choose the DefaultPrimaryResolver
						defaultResolver = dnsProxyList.SingleOrDefault(d => d.Name.Equals(Global.DefaultSecondaryResolverName)) ??
						                  dnsProxyList.SingleOrDefault(d => d.Name.Equals(Global.DefaultSecondaryBackupResolverName));
					}
					else
					{
						defaultResolver = dnsProxyList.SingleOrDefault(d => d.Name.Equals(_userData.SecondaryResolver)) ??
						                  (dnsProxyList.SingleOrDefault(d => d.Name.Equals(Global.DefaultSecondaryResolverName)) ??
						                   dnsProxyList.SingleOrDefault(d => d.Name.Equals(Global.DefaultSecondaryBackupResolverName)));
					}

					SecondaryResolver = defaultResolver ?? dnsProxyList.ElementAt(1);
					if (_secondaryResolver.LocalPort == 0)
					{
						_secondaryResolver.LocalPort = Global.SecondaryResolverPort;
					}
					if (_secondaryResolver.LocalAddress == null)
					{
						_secondaryResolver.LocalAddress = Global.SecondaryResolverAddress;
					}
				}


				if (PrimaryDnsCryptProxyManager.IsDnsCryptProxyInstalled())
				{
					if (PrimaryDnsCryptProxyManager.IsDnsCryptProxyRunning())
					{
						_isPrimaryResolverRunning = true;
					}
				}

				if (SecondaryDnsCryptProxyManager.IsDnsCryptProxyInstalled())
				{
					if (SecondaryDnsCryptProxyManager.IsDnsCryptProxyRunning())
					{
						_isSecondaryResolverRunning = true;
					}
				}

				if (PrimaryDnsCryptProxyManager.DnsCryptProxy.Parameter.LocalAddress != null)
				{
					if (PrimaryDnsCryptProxyManager.DnsCryptProxy.Parameter.LocalAddress.Equals(Global.GlobalGatewayAddress))
					{
						_actAsGlobalGateway = true;
						_primaryResolverTitle =
							$"{LocalizationEx.GetUiString("default_settings_primary_header", Thread.CurrentThread.CurrentCulture)} ({Global.GlobalGatewayAddress}:{Global.PrimaryResolverPort})";
					}
					else
					{
						_actAsGlobalGateway = false;
						_primaryResolverTitle =
							$"{LocalizationEx.GetUiString("default_settings_primary_header", Thread.CurrentThread.CurrentCulture)}";
					}
				}
				else
				{
					_actAsGlobalGateway = false;
					_primaryResolverTitle =
						$"{LocalizationEx.GetUiString("default_settings_primary_header", Thread.CurrentThread.CurrentCulture)}";
				}

				_secondaryResolverTitle =
					$"{LocalizationEx.GetUiString("default_settings_secondary_header", Thread.CurrentThread.CurrentCulture)} ({Global.SecondaryResolverAddress}:{Global.SecondaryResolverPort})";

				if (_unsavedChanges)
				{
					SavePrimaryResolver(false);
				}

				// check for new version on every application start
				ShowHiddenCards = false;
				UpdateAsync();
				BlockViewModel = new BlockViewModel(_windowManager);
				LogViewModel = new LogViewModel(_windowManager);
			}
			catch (DllNotFoundException ex)
			{
				if (ex.Message.Contains("libsodium"))
				{
					//The Windows 10 Universal CRT is a Windows operating system component that enables CRT functionality on the Windows operating system.
					//This update allows Windows desktop applications that depend on the Windows 10 Universal CRT release to run on earlier Windows operating systems.
					//Microsoft Visual Studio 2015 creates a dependency on the Universal CRT when applications are built by using the Windows 10 Software Development Kit(SDK).
					//You can install this update on earlier Windows operating systems to enable these applications to run correctly.
					//TODO: translate
					_windowManager.ShowMetroMessageBox(
						"Please install the update for Universal C Runtime in Windows: https://support.microsoft.com/en-us/kb/2999226",
						"Missing Universal C Runtime (CRT)", MessageBoxButton.OK, BoxType.Warning);
					Environment.Exit(1);
				}
			}
		}

		public void ReloadLocalResolverList()
		{
			var dnsProxyList = DnsCryptProxyListManager.ReadProxyList(_proxyList, _proxyListSignature, 
				_filterNoLogs, _filterDnssec, _filterIpv4, _filterIpv6);
			if (dnsProxyList != null && dnsProxyList.Any())
			{
				Resolvers = dnsProxyList;
				if (!dnsProxyList.Contains(_primaryResolver))
				{
					PrimaryResolver = dnsProxyList.ElementAt(0);
				}
				if (!dnsProxyList.Contains(_secondaryResolver))
				{
					SecondaryResolver = dnsProxyList.ElementAt(1);
				}
			}
		}

		public void SetWindowTitle(bool useIpv6 = false)
		{
			WindowTitle = useIpv6
				? $"{Global.ApplicationName} {VersionUtilities.PublishVersion} {VersionUtilities.PublishBuild}"
				: $"{Global.ApplicationName} {VersionUtilities.PublishVersion} {VersionUtilities.PublishBuild} ({LocalizationEx.GetUiString("global_ipv6_disabled", Thread.CurrentThread.CurrentCulture)})";
		}

		public static MainViewModel Instance { get; set; }

		public BlockViewModel BlockViewModel { get; }
		public LogViewModel LogViewModel { get; }

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
				_userData.Language = _selectedLanguage.ShortCode;
				_userData.SaveConfigurationFile();
				if (_userData.UseIpv6)
				{
					WindowTitle = $"{Global.ApplicationName} {VersionUtilities.PublishVersion} {VersionUtilities.PublishBuild}";
				}
				else
				{
					WindowTitle =
						$"{Global.ApplicationName} {VersionUtilities.PublishVersion} {VersionUtilities.PublishBuild} ({LocalizationEx.GetUiString("global_ipv6_disabled", Thread.CurrentThread.CurrentCulture)})";
				}
				if (_actAsGlobalGateway)
				{
					PrimaryResolverTitle =
						$"{LocalizationEx.GetUiString("default_settings_primary_header", Thread.CurrentThread.CurrentCulture)} ({Global.GlobalGatewayAddress}:{Global.PrimaryResolverPort})";
				}
				else
				{
					PrimaryResolverTitle =
						$"{LocalizationEx.GetUiString("default_settings_primary_header", Thread.CurrentThread.CurrentCulture)}";
				}
				SecondaryResolverTitle =
					$"{LocalizationEx.GetUiString("default_settings_secondary_header", Thread.CurrentThread.CurrentCulture)} ({Global.SecondaryResolverAddress}:{Global.SecondaryResolverPort})";
				NotifyOfPropertyChange(() => SelectedLanguage);
			}
		}

		public ISnackbarMessageQueue MessageQueue
		{
			get => _messageQueue;
			set
			{
				if (value.Equals(_messageQueue)) return;
				_messageQueue = value;
				NotifyOfPropertyChange(() => MessageQueue);
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
		///		
		/// </summary>
		public bool IsCheckingUpdates
		{
			get => _isCheckingUpdates;
			set
			{
				_isCheckingUpdates = value;
				NotifyOfPropertyChange(() => IsCheckingUpdates);
			}
		}

		/// <summary>
		///     Overlay management for MetroMessageBoxViewModel.
		/// </summary>
		public bool IsOverlayVisible => _overlayDependencies > 0;

		public DnsCryptProxyManager PrimaryDnsCryptProxyManager { get; set; }
		public DnsCryptProxyManager SecondaryDnsCryptProxyManager { get; set; }

		public CollectionViewSource LocalNetworkInterfaces { get; set; }


		public bool FilterDnssec
		{
			get => _filterDnssec;
			set
			{
				_filterDnssec = value;
				_userData.OnlyUseDnssec = _filterDnssec;
				_userData.SaveConfigurationFile();
				ReloadLocalResolverList();
				NotifyOfPropertyChange(() => FilterDnssec);
			}
		}

		public bool FilterNoLogs
		{
			get => _filterNoLogs;
			set
			{
				_filterNoLogs = value;
				_userData.OnlyUseNoLogs = _filterNoLogs;
				_userData.SaveConfigurationFile();
				ReloadLocalResolverList();
				NotifyOfPropertyChange(() => FilterNoLogs);
			}
		}

		public bool FilterIpv4
		{
			get => _filterIpv4;
			set
			{
				if (!value && !_filterIpv6) return;
				_filterIpv4 = value;
				_userData.UseIpv4 = _filterIpv4;
				_userData.SaveConfigurationFile();
				ReloadLocalResolverList();
				NotifyOfPropertyChange(() => FilterIpv4);
			}
		}


		public bool FilterIpv6
		{
			get => _filterIpv6;
			set
			{
				if (!value && !_filterIpv4) return;
				_filterIpv6 = value;
				SetWindowTitle(_filterIpv6);
				_userData.UseIpv6 = _filterIpv6;
				_userData.SaveConfigurationFile();
				ReloadLocalResolverList();
				NotifyOfPropertyChange(() => FilterIpv6);
			}
		}

		/// <summary>
		///     The list of loaded resolvers.
		/// </summary>
		public List<DnsCryptProxyEntry> Resolvers
		{
			get => _resolvers;
			set
			{
				if (value.Equals(_resolvers)) return;
				_resolvers = value;
				NotifyOfPropertyChange(() => Resolvers);
			}
		}

		/// <summary>
		///     Get the last write time of the resolver csv.
		/// </summary>
		public DateTime ProxyListLastWriteTime
		{
			get
			{
				var proxyList = Path.Combine(Directory.GetCurrentDirectory(),
					Global.DnsCryptProxyFolder, Global.DnsCryptProxyResolverListName);
				var fileInfo = new FileInfo(proxyList);
				return fileInfo.LastWriteTime;
			}
		}

		public bool UnsavedChanges
		{
			get => _unsavedChanges;
			set
			{
				if (value.Equals(_unsavedChanges)) return;
				_unsavedChanges = value;
				NotifyOfPropertyChange(() => UnsavedChanges);
			}
		}

		public int PrimaryResolverLocalPort
		{
			get => PrimaryResolver.LocalPort;
			set
			{
				if (value.Equals(_primaryResolver.LocalPort)) return;
				PrimaryResolver.LocalPort = value;
				UnsavedChanges = true;
				NotifyOfPropertyChange(() => PrimaryResolverLocalPort);
			}
		}

		public int PrimaryResolverLastOctet
		{
			get
			{
				var lastOctet = _primaryResolver.LocalAddress.Split('.')[3];
				return Convert.ToInt32(lastOctet);
			} 
			set
			{
				if (value.Equals(2))
				{
					//TODO: translate
					MessageQueue.Enqueue("127.0.0.2 is reserved for secondary resolver!");
					return;
				}
				if (("127.0.0." + value).Equals(_primaryResolver.LocalAddress)) return;
				PrimaryResolver.LocalAddress = "127.0.0." + value;
				UnsavedChanges = true;
				NotifyOfPropertyChange(() => PrimaryResolverLastOctet);
			}
		}


		/// <summary>
		/// 
		/// </summary>
		public void SavePrimaryResolver(bool messageQueue = true)
		{
			_userData.PrimaryResolver = _primaryResolver.Name;
			_userData.SaveConfigurationFile();
			ReloadResolver(DnsCryptProxyType.Primary);
			NotifyOfPropertyChange(() => PrimaryResolver);
			if (messageQueue) { 
				//TODO: translate
				MessageQueue.Enqueue("Saved!");
			}

			UnsavedChanges = false;
		}

		/// <summary>
		///     The selected primary resolver.
		/// </summary>
		public DnsCryptProxyEntry PrimaryResolver
		{
			get => _primaryResolver;
			set
			{
				if (value == null || value.Equals(_primaryResolver)) return;
				UnsavedChanges = true;
				value.LocalAddress = _primaryResolver.LocalAddress; //keep value
				value.LocalPort = _primaryResolver.LocalPort; //keep value
				_primaryResolver = value;
				NotifyOfPropertyChange(() => PrimaryResolver);
			}
		}

		/// <summary>
		///     The selected secondary resolver.
		/// </summary>
		public DnsCryptProxyEntry SecondaryResolver
		{
			get => _secondaryResolver;
			set
			{
				if (value == null || value.Equals(_secondaryResolver)) return;
				_secondaryResolver = value;
				_userData.SecondaryResolver = _secondaryResolver.Name;
				_userData.SaveConfigurationFile();
				ReloadResolver(DnsCryptProxyType.Secondary);
				NotifyOfPropertyChange(() => SecondaryResolver);
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
				LoadNetworkCards();
				NotifyOfPropertyChange(() => ShowHiddenCards);
			}
		}

		/// <summary>
		///     Update the resolver csv on startup.
		/// </summary>
		public bool UpdateResolverListOnStart
		{
			get => _updateResolverListOnStart;
			set
			{
				_updateResolverListOnStart = value;
				NotifyOfPropertyChange(() => UpdateResolverListOnStart);
			}
		}

		/// <summary>
		///     Formatted name of the primary resolver.
		/// </summary>
		public string PrimaryResolverTitle
		{
			get => _primaryResolverTitle;
			set
			{
				_primaryResolverTitle = value;
				NotifyOfPropertyChange(() => PrimaryResolverTitle);
			}
		}

		/// <summary>
		///     Formatted name of the secondary resolver.
		/// </summary>
		public string SecondaryResolverTitle
		{
			get => _secondaryResolverTitle;
			set
			{
				_secondaryResolverTitle = value;
				NotifyOfPropertyChange(() => SecondaryResolverTitle);
			}
		}

		/// <summary>
		///     Controls the primary resolver.
		/// </summary>
		public bool IsPrimaryResolverRunning
		{
			get => _isPrimaryResolverRunning;
			set
			{
				HandleService(DnsCryptProxyType.Primary);
				NotifyOfPropertyChange(() => IsPrimaryResolverRunning);
			}
		}

		/// <summary>
		///     Controls the secondary resolver.
		/// </summary>
		public bool IsSecondaryResolverRunning
		{
			get => _isSecondaryResolverRunning;
			set
			{
				HandleService(DnsCryptProxyType.Secondary);
				NotifyOfPropertyChange(() => IsSecondaryResolverRunning);
			}
		}

		/// <summary>
		/// </summary>
		public bool IsAnalysing
		{
			get => _isAnalysing;
			set
			{
				_isAnalysing = value;
				NotifyOfPropertyChange(() => IsAnalysing);
			}
		}

		/// <summary>
		///     Show or hide the progress bar for the primary resolver.
		/// </summary>
		public bool IsWorkingOnPrimaryService
		{
			get => _isWorkingOnPrimaryService;
			set
			{
				_isWorkingOnPrimaryService = value;
				NotifyOfPropertyChange(() => IsWorkingOnPrimaryService);
			}
		}

		/// <summary>
		///     Show or hide the progress bar for the secondary resolver.
		/// </summary>
		public bool IsWorkingOnSecondaryService
		{
			get => _isWorkingOnSecondaryService;
			set
			{
				_isWorkingOnSecondaryService = value;
				NotifyOfPropertyChange(() => IsWorkingOnSecondaryService);
			}
		}

		/// <summary>
		///     Overlay management for MetroMessageBoxViewModel.
		/// </summary>
		public void ShowOverlay()
		{
			_overlayDependencies++;
			NotifyOfPropertyChange(() => IsOverlayVisible);
		}

		/// <summary>
		///     Overlay management for MetroMessageBoxViewModel.
		/// </summary>
		public void HideOverlay()
		{
			_overlayDependencies--;
			NotifyOfPropertyChange(() => IsOverlayVisible);
		}

		/// <summary>
		/// Check if Microsoft Visual C++ Redistributable package is installed.
		/// </summary>
		public void CheckRedistributablePackageVersion()
		{
			if (Environment.Is64BitProcess)
			{
				//TODO: tanslate
				if (RedistributablePackage.IsInstalled(RedistributablePackageVersion.VC2017x64)) return;
				_windowManager.ShowMetroMessageBox(
					"Please install the Microsoft Visual C++ Redistributable Visual Studio 2017 (x64) package first: https://go.microsoft.com/fwlink/?LinkId=746572",
					"Missing Microsoft Visual C++ Redistributable for Visual Studio 2017", MessageBoxButton.OK, BoxType.Warning);
				Environment.Exit(1);
			}
			else
			{
				//TODO: translate
				if (RedistributablePackage.IsInstalled(RedistributablePackageVersion.VC2017x86)) return;
				_windowManager.ShowMetroMessageBox(
					"Please install the Microsoft Visual C++ Redistributable Visual Studio 2017 (x86) package first: https://go.microsoft.com/fwlink/?LinkId=746571",
					"Missing Microsoft Visual C++ Redistributable for Visual Studio 2017", MessageBoxButton.OK, BoxType.Warning);
				Environment.Exit(1);
			}
		}

		/// <summary>
		///     Method to check if there is a new application version available.
		/// </summary>
		public async void UpdateAsync()
		{
			try
			{
				IsCheckingUpdates = true;
				var update = await ApplicationUpdater.CheckForRemoteUpdateAsync().ConfigureAwait(true);
				if (update.CanUpdate)
				{
					var boxType = update.Update.Type == UpdateType.Standard ? BoxType.Default : BoxType.Warning;
					var boxText = update.Update.Type == UpdateType.Standard
						? LocalizationEx.GetUiString("dialog_message_update_standard_text",
							Thread.CurrentThread.CurrentCulture)
						: LocalizationEx.GetUiString("dialog_message_update_critical_text",
							Thread.CurrentThread.CurrentCulture);
					var boxTitle = update.Update.Type == UpdateType.Standard
						? LocalizationEx.GetUiString("dialog_message_update_standard_title",
							Thread.CurrentThread.CurrentCulture)
						: LocalizationEx.GetUiString("dialog_message_update_critical_title",
							Thread.CurrentThread.CurrentCulture);
					var userResult =
						_windowManager.ShowMetroMessageBox(
							string.Format(boxText, update.Update.Version), boxTitle,
							MessageBoxButton.YesNo, boxType);

					if (userResult != MessageBoxResult.Yes) return;
					var updateViewModel = new UpdateViewModel(update.Update)
					{
						WindowTitle =
							LocalizationEx.GetUiString("window_update_title", Thread.CurrentThread.CurrentCulture)
					};
					dynamic settings = new ExpandoObject();
					settings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
					settings.Owner = GetView();
					var result = _windowManager.ShowDialog(updateViewModel, null, settings);
					if (!result) return;
					if (!File.Exists(updateViewModel.InstallerPath)) return;
					// start the installer
					Process.Start(updateViewModel.InstallerPath);
					// kill running application
					Process.GetCurrentProcess().Kill();
				}
				else
				{
					//TODO: translate
					MessageQueue.Enqueue("There are no updates available");
				}
				IsCheckingUpdates = false;
			}
			catch (Exception)
			{
			}
		}

		public void ReloadResolver(DnsCryptProxyType dnsCryptProxyType)
		{
			if (dnsCryptProxyType == DnsCryptProxyType.Primary)
			{
				if (_primaryResolver != null)
				{
					if (_primaryResolver.LocalPort == 0)
					{
						_primaryResolver.LocalPort = Global.PrimaryResolverPort;
					}
					if (_primaryResolver.LocalAddress == null)
					{
						_primaryResolver.LocalAddress = Global.PrimaryResolverAddress;
					}
					PrimaryDnsCryptProxyManager.DnsCryptProxy.Parameter = ConvertProxyEntryToParameter(
						_primaryResolver, DnsCryptProxyType.Primary);
					if (PrimaryDnsCryptProxyManager.WriteRegistry(DnsCryptProxyType.Primary))
					{
						if (PrimaryDnsCryptProxyManager.IsDnsCryptProxyInstalled())
						{
							RestartService(DnsCryptProxyType.Primary);
						}
					}
				}
			}
			else
			{
				if (_secondaryResolver != null)
				{
					if (_secondaryResolver.LocalPort == 0)
					{
						_secondaryResolver.LocalPort = Global.SecondaryResolverPort;
					}
					if (_secondaryResolver.LocalAddress == null)
					{
						_secondaryResolver.LocalAddress = Global.SecondaryResolverAddress;
					}
					SecondaryDnsCryptProxyManager.DnsCryptProxy.Parameter =
						ConvertProxyEntryToParameter(_secondaryResolver, DnsCryptProxyType.Secondary);
					if (SecondaryDnsCryptProxyManager.WriteRegistry(DnsCryptProxyType.Secondary))
					{
						if (SecondaryDnsCryptProxyManager.IsDnsCryptProxyInstalled())
						{
							RestartService(DnsCryptProxyType.Secondary);
						}
					}
				}
			}
		}

		private DnsCryptProxyParameter ConvertProxyEntryToParameter(DnsCryptProxyEntry dnsCryptProxyEntry,
			DnsCryptProxyType dnsCryptProxyType)
		{
			var dnsCryptProxyParameter = new DnsCryptProxyParameter
			{
				ProviderKey = dnsCryptProxyEntry.ProviderPublicKey,
				Plugins = Plugins.ToArray(),
				ProviderName = dnsCryptProxyEntry.ProviderName,
				ResolverAddress = dnsCryptProxyEntry.ResolverAddress,
				ResolverName = dnsCryptProxyEntry.Name,
				LocalPort = dnsCryptProxyEntry.LocalPort,
				ResolversList =
					Path.Combine(Directory.GetCurrentDirectory(), Global.DnsCryptProxyFolder,
						Global.DnsCryptProxyResolverListName),
				EphemeralKeys = true,
				TcpOnly = UseTcpOnly
			};
			if (dnsCryptProxyType == DnsCryptProxyType.Primary)
			{
				if (_actAsGlobalGateway)
				{
					dnsCryptProxyParameter.LocalAddress = Global.GlobalGatewayAddress;
				}
				else
				{
					//dnsCryptProxyParameter.LocalAddress = Global.PrimaryResolverAddress;
					dnsCryptProxyParameter.LocalAddress = dnsCryptProxyEntry.LocalAddress;
				}
			}
			else
			{
				dnsCryptProxyParameter.LocalAddress = Global.SecondaryResolverAddress;
			}

			return dnsCryptProxyParameter;
		}

		private async void RestartService(DnsCryptProxyType dnsCryptProxyType)
		{
			if (dnsCryptProxyType == DnsCryptProxyType.Primary)
			{
				IsWorkingOnPrimaryService = true;
				await Task.Run(() => { PrimaryDnsCryptProxyManager.Restart(); }).ConfigureAwait(false);
				Thread.Sleep(Global.ServiceRestartTime);
				_isPrimaryResolverRunning = PrimaryDnsCryptProxyManager.IsDnsCryptProxyRunning();
				NotifyOfPropertyChange(() => IsPrimaryResolverRunning);
				ResetNetworkCards();
				IsWorkingOnPrimaryService = false;
			}
			else
			{
				IsWorkingOnSecondaryService = true;
				await Task.Run(() => { SecondaryDnsCryptProxyManager.Restart(); }).ConfigureAwait(false);
				Thread.Sleep(Global.ServiceRestartTime);
				_isSecondaryResolverRunning = SecondaryDnsCryptProxyManager.IsDnsCryptProxyRunning();
				NotifyOfPropertyChange(() => IsSecondaryResolverRunning);
				ResetNetworkCards();
				IsWorkingOnSecondaryService = false;
			}
		}

		private async void HandleService(DnsCryptProxyType dnsCryptProxyType)
		{
			if (dnsCryptProxyType == DnsCryptProxyType.Primary)
			{
				IsWorkingOnPrimaryService = true;
				if (IsPrimaryResolverRunning)
				{
					// service is running, stop it
					await Task.Run(() => { PrimaryDnsCryptProxyManager.Stop(); }).ConfigureAwait(false);
					Thread.Sleep(Global.ServiceStopTime);
					_isPrimaryResolverRunning = PrimaryDnsCryptProxyManager.IsDnsCryptProxyRunning();
					NotifyOfPropertyChange(() => IsPrimaryResolverRunning);
					ResetNetworkCards();
				}
				else
				{
					if (PrimaryDnsCryptProxyManager.IsDnsCryptProxyInstalled())
					{
						// service is installed, just start them
						await Task.Run(() => { PrimaryDnsCryptProxyManager.Start(); }).ConfigureAwait(false);
						Thread.Sleep(Global.ServiceStartTime);
						_isPrimaryResolverRunning = PrimaryDnsCryptProxyManager.IsDnsCryptProxyRunning();
						NotifyOfPropertyChange(() => IsPrimaryResolverRunning);
						ResetNetworkCards();
					}
					else
					{
						//install and start the service
						await Task.Run(() => { return PrimaryDnsCryptProxyManager.Install(); }).ConfigureAwait(false);
						Thread.Sleep(Global.ServiceStartTime);
						_isPrimaryResolverRunning = PrimaryDnsCryptProxyManager.IsDnsCryptProxyRunning();
						NotifyOfPropertyChange(() => IsPrimaryResolverRunning);
						ResetNetworkCards();
					}
				}
				IsWorkingOnPrimaryService = false;
			}
			else
			{
				IsWorkingOnSecondaryService = true;
				if (IsSecondaryResolverRunning)
				{
					// service is running, stop it
					await Task.Run(() => { SecondaryDnsCryptProxyManager.Stop(); }).ConfigureAwait(false);
					Thread.Sleep(Global.ServiceStopTime);
					_isSecondaryResolverRunning = SecondaryDnsCryptProxyManager.IsDnsCryptProxyRunning();
					NotifyOfPropertyChange(() => IsSecondaryResolverRunning);
					ResetNetworkCards();
				}
				else
				{
					if (SecondaryDnsCryptProxyManager.IsDnsCryptProxyInstalled())
					{
						// service is installed, just start them
						await Task.Run(() => { SecondaryDnsCryptProxyManager.Start(); }).ConfigureAwait(false);
						Thread.Sleep(Global.ServiceStartTime);
						_isSecondaryResolverRunning = SecondaryDnsCryptProxyManager.IsDnsCryptProxyRunning();
						NotifyOfPropertyChange(() => IsSecondaryResolverRunning);
						ResetNetworkCards();
					}
					else
					{
						//install and start the service
						await
							Task.Run(() => { return SecondaryDnsCryptProxyManager.Install(); })
								.ConfigureAwait(false);
						Thread.Sleep(Global.ServiceStartTime);
						_isSecondaryResolverRunning = SecondaryDnsCryptProxyManager.IsDnsCryptProxyRunning();
						NotifyOfPropertyChange(() => IsSecondaryResolverRunning);
						ResetNetworkCards();
					}
				}
				IsWorkingOnSecondaryService = false;
			}
		}

		/// <summary>
		///     Load the local network cards.
		/// </summary>
		private void LoadNetworkCards()
		{
			var localNetworkInterfaces = LocalNetworkInterfaceManager.GetLocalNetworkInterfaces(ShowHiddenCards, false, _primaryResolver.LocalAddress);
			_localNetworkInterfaces.Clear();
			if (localNetworkInterfaces.Count == 0) return;
			foreach (var localNetworkInterface in localNetworkInterfaces)
			{
				_localNetworkInterfaces.Add(localNetworkInterface);
			}
		}

		private void ResetNetworkCards()
		{
			foreach (var localNetworkInterface in _localNetworkInterfaces)
			{
				if (!localNetworkInterface.UseDnsCrypt) continue;
				var dns4 = new List<string>();
				var dns6 = new List<string>();
				if (PrimaryResolver != null)
				{
					if (!string.IsNullOrEmpty(PrimaryResolver.ProviderPublicKey))
					{
						// only add the local address if the proxy is running 
						if (PrimaryDnsCryptProxyManager.DnsCryptProxy.IsReady && PrimaryDnsCryptProxyManager.IsDnsCryptProxyRunning())
						{
							dns4.Add(PrimaryResolver.LocalAddress);
							if (_userData.UseIpv6)
							{
								dns6.Add(Global.PrimaryResolverAddress6);
							}
						}
					}
				}
				if (SecondaryResolver != null)
				{
					if (!string.IsNullOrEmpty(SecondaryResolver.ProviderPublicKey))
					{
						// only add the local address if the proxy is running 
						if (SecondaryDnsCryptProxyManager.DnsCryptProxy.IsReady &&
						    SecondaryDnsCryptProxyManager.IsDnsCryptProxyRunning())
						{
							dns4.Add(Global.SecondaryResolverAddress);
							if (_userData.UseIpv6)
							{
								dns6.Add(Global.SecondaryResolverAddress6);
							}
						}
					}
				}
				var status =
					LocalNetworkInterfaceManager.SetNameservers(localNetworkInterface, dns4, NetworkInterfaceComponent.IPv4);
				if (_userData.UseIpv6)
				{
					LocalNetworkInterfaceManager.SetNameservers(localNetworkInterface, dns6, NetworkInterfaceComponent.IPv6);
				}
				localNetworkInterface.UseDnsCrypt = status;
				//TODO: translate
				MessageQueue.Enqueue($"{localNetworkInterface.Name} reconfigured");
			}
			LoadNetworkCards();
		}

		/// <summary>
		///     Click event for the network cards.
		/// </summary>
		/// <param name="localNetworkInterface">The clicked network card.</param>
		public void NetworkCardClicked(LocalNetworkInterface localNetworkInterface)
		{
			if (localNetworkInterface == null) return;
			if (localNetworkInterface.UseDnsCrypt)
			{
				var status = LocalNetworkInterfaceManager.SetNameservers(localNetworkInterface, new List<string>(),
					NetworkInterfaceComponent.IPv4);
				LocalNetworkInterfaceManager.SetNameservers(localNetworkInterface, new List<string>(),
					NetworkInterfaceComponent.IPv6);
				localNetworkInterface.UseDnsCrypt = !status;
			}
			else
			{
				var dns4 = new List<string>();
				var dns6 = new List<string>();
				if (PrimaryResolver != null)
				{
					if (!string.IsNullOrEmpty(PrimaryResolver.ProviderPublicKey))
					{
						// only add the local address if the proxy is running 
						if (PrimaryDnsCryptProxyManager.DnsCryptProxy.IsReady && PrimaryDnsCryptProxyManager.IsDnsCryptProxyRunning())
						{
							//dns4.Add(Global.PrimaryResolverAddress);
							dns4.Add(PrimaryResolver.LocalAddress);
							if (_userData.UseIpv6)
							{
								dns6.Add(Global.PrimaryResolverAddress6);
							}
						}
					}
				}
				if (SecondaryResolver != null)
				{
					if (!string.IsNullOrEmpty(SecondaryResolver.ProviderPublicKey))
					{
						// only add the local address if the proxy is running 
						if (SecondaryDnsCryptProxyManager.DnsCryptProxy.IsReady && SecondaryDnsCryptProxyManager.IsDnsCryptProxyRunning())
						{
							dns4.Add(Global.SecondaryResolverAddress);
							if (_userData.UseIpv6)
							{
								dns6.Add(Global.SecondaryResolverAddress6);
							}
						}
					}
				}
				var status = LocalNetworkInterfaceManager.SetNameservers(localNetworkInterface, dns4, NetworkInterfaceComponent.IPv4);
				if (_userData.UseIpv6)
				{
					LocalNetworkInterfaceManager.SetNameservers(localNetworkInterface, dns6, NetworkInterfaceComponent.IPv6);
				}
				localNetworkInterface.UseDnsCrypt = status;
			}
			LoadNetworkCards();
		}

		/// <summary>
		///     Get some extra information of the resolvers.
		/// </summary>
		/// <remarks>Note: no IPv6 support, for now.</remarks>
		public async void AnalyseResolvers()
		{
			try
			{
				if (_isAnalysing)
				{
					_analyseCancellationTokenSource.Cancel();
				}
				else
				{
					_analyseCancellationTokenSource = new CancellationTokenSource();
				}
				IsAnalysing = true;
				var tmpResolvers = new List<DnsCryptProxyEntry>();
				await Task.Run(async () =>
				{
					for (var r = 0; r < _resolvers.Count; r++)
					{
						_analyseCancellationTokenSource.Token.ThrowIfCancellationRequested();
						var dnsCryptProxyEntryExtra = await AnalyseProxy.Analyse(_resolvers[r]).ConfigureAwait(false);
						if (dnsCryptProxyEntryExtra == null) continue;
						if (!dnsCryptProxyEntryExtra.Succeeded || dnsCryptProxyEntryExtra.ResponseTime <= 0) continue;
						var dnsCryptProxyEntry = _resolvers[r];
						dnsCryptProxyEntry.Extra = dnsCryptProxyEntryExtra;
						tmpResolvers.Add(dnsCryptProxyEntry);
					}
					tmpResolvers.Sort((a, b) => a.Extra.ResponseTime.CompareTo(b.Extra.ResponseTime));
				}, _analyseCancellationTokenSource.Token).ConfigureAwait(false);
				
				//TODO: translate
				MessageQueue.Enqueue($"Successfully checked {tmpResolvers.Count}/{_resolvers.Count} resolvers");
				Resolvers = tmpResolvers;
				IsAnalysing = false;
			}
			catch (OperationCanceledException)
			{
				foreach (var resolver in _resolvers)
				{
					if (resolver.Extra == null)
					{
						resolver.Extra = new DnsCryptProxyEntryExtra {Succeeded = false, ResponseTime = -1};
					}
				}
				_resolvers.Sort((a, b) => a.Extra.ResponseTime.CompareTo(b.Extra.ResponseTime));
				//TODO: translate
				MessageQueue.Enqueue($"Resolver check aborted");
				IsAnalysing = false;
			}
			catch (Exception)
			{
				IsAnalysing = false;
			}
		}

		#region Helper

		/// <summary>
		///     Check if the current user has administrative privileges.
		/// </summary>
		/// <returns><c>true</c> if the user has administrative privileges, otherwise <c>false</c></returns>
		public static bool IsAdministrator()
		{
			try
			{
				return new WindowsPrincipal(WindowsIdentity.GetCurrent())
					.IsInRole(WindowsBuiltInRole.Administrator);
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>
		///     Check the dnscrypt-proxy directory on completeness.
		/// </summary>
		/// <returns><c>true</c> if all files are available, otherwise <c>false</c></returns>
		private static bool ValidateDnsCryptProxyFolder()
		{
			foreach (var proxyFile in Global.DnsCryptProxyFiles)
			{
				var proxyFilePath = Path.Combine(Directory.GetCurrentDirectory(), Global.DnsCryptProxyFolder, proxyFile);
				if (!File.Exists(proxyFilePath))
				{
					return false;
				}
				// exclude this check on dev folders
				if (proxyFilePath.Contains("bin\\Debug") || proxyFilePath.Contains("bin\\Release") || proxyFilePath.Contains("bin\\x64")) continue;
				// dnscrypt-resolvers.* files are signed with minisign
				if (!proxyFile.StartsWith("dnscrypt-resolvers."))
				{
					// check if the file is signed
					if (!AuthenticodeTools.IsTrusted(proxyFilePath))
					{
						return false;
					}
				}
			}
			return true;
		}

		#endregion

		#region Advanced Settings

		/// <summary>
		///     Uninstall all installed dnscrypt-proxy services.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		/// <exception cref="NetworkInformationException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public async void UninstallServices()
		{
			var result = _windowManager.ShowMetroMessageBox(
				LocalizationEx.GetUiString("dialog_message_uninstall", Thread.CurrentThread.CurrentCulture),
				LocalizationEx.GetUiString("dialog_uninstall_title", Thread.CurrentThread.CurrentCulture),
				MessageBoxButton.YesNo, BoxType.Default);

			if (result == MessageBoxResult.Yes)
			{
				IsUninstallingServices = true;
				await Task.Run(() =>
				{
					PrimaryDnsCryptProxyManager.Uninstall();
					SecondaryDnsCryptProxyManager.Uninstall();
				}).ConfigureAwait(false);
				Thread.Sleep(Global.ServiceUninstallTime);
				IsUninstallingServices = false;
			}

			_isPrimaryResolverRunning = PrimaryDnsCryptProxyManager.IsDnsCryptProxyRunning();
			NotifyOfPropertyChange(() => IsPrimaryResolverRunning);
			_isSecondaryResolverRunning = SecondaryDnsCryptProxyManager.IsDnsCryptProxyRunning();
			NotifyOfPropertyChange(() => IsSecondaryResolverRunning);

			// recover the network interfaces (also the hidden and down cards)
			foreach (var nic in LocalNetworkInterfaceManager.GetLocalNetworkInterfaces(true, false, ""))
			{
				if (!nic.UseDnsCrypt) continue;
				var status = LocalNetworkInterfaceManager.SetNameservers(nic, new List<string>(), NetworkInterfaceComponent.IPv4);
				var card = _localNetworkInterfaces.SingleOrDefault(n => n.Description.Equals(nic.Description));
				if (card != null)
				{
					card.UseDnsCrypt = !status;
				}
			}
		}

		/// <summary>
		///     Refresh the resolver list from the newest csv file.
		/// </summary>
		/// <exception cref="UnauthorizedAccessException"></exception>
		/// <exception cref="NotSupportedException"></exception>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public async void RefreshResolverListAsync()
		{
			IsRefreshingResolverList = true;
			var state = await DnsCryptProxyListManager.UpdateResolverListAsync().ConfigureAwait(false);
			await Task.Run(() =>
			{
				// we do this, to prevent excessive usage
				Thread.Sleep(2000);
			}).ConfigureAwait(false);
			if (state)
			{
				var proxyList = Path.Combine(Directory.GetCurrentDirectory(),
					Global.DnsCryptProxyFolder, Global.DnsCryptProxyResolverListName);
				var proxyListSignature = Path.Combine(Directory.GetCurrentDirectory(),
					Global.DnsCryptProxyFolder, Global.DnsCryptProxySignatureFileName);
				var dnsProxyList =
					DnsCryptProxyListManager.ReadProxyList(proxyList, proxyListSignature, _userData.OnlyUseNoLogs, _userData.OnlyUseDnssec, _userData.UseIpv4, _userData.UseIpv6);
				if (dnsProxyList != null && dnsProxyList.Any())
				{
					var tmpResolvers = new List<DnsCryptProxyEntry>();
					foreach (var dnsProxy in dnsProxyList)
					{
						if (
							dnsProxy.ProviderPublicKey.Equals(
								PrimaryDnsCryptProxyManager.DnsCryptProxy.Parameter.ProviderKey))
						{
							_primaryResolver = dnsProxy;
							// restore the local port
							_primaryResolver.LocalPort = PrimaryDnsCryptProxyManager.DnsCryptProxy.Parameter.LocalPort;
						}
						if (
							dnsProxy.ProviderPublicKey.Equals(
								SecondaryDnsCryptProxyManager.DnsCryptProxy.Parameter.ProviderKey))
						{
							_secondaryResolver = dnsProxy;
						}
						tmpResolvers.Add(dnsProxy);
					}
					Resolvers = tmpResolvers;
					if (_primaryResolver == null)
					{
						PrimaryResolver = dnsProxyList.ElementAt(0);
					}
					if (_secondaryResolver == null)
					{
						SecondaryResolver = dnsProxyList.ElementAt(1);
					}
				}
			}
			else
			{
				_windowManager.ShowMetroMessageBox(
					LocalizationEx.GetUiString("dialog_message_refresh_failed", Thread.CurrentThread.CurrentCulture),
					LocalizationEx.GetUiString("dialog_warning_title", Thread.CurrentThread.CurrentCulture),
					MessageBoxButton.OK, BoxType.Warning);
			}
			IsRefreshingResolverList = false;
			NotifyOfPropertyChange(() => ProxyListLastWriteTime);
		}

		public bool ActAsGlobalGateway
		{
			get => _actAsGlobalGateway;
			set
			{
				_actAsGlobalGateway = value;
				HandleGlobalResolver(_actAsGlobalGateway);
				NotifyOfPropertyChange(() => ActAsGlobalGateway);
			}
		}

		private async void HandleGlobalResolver(bool actAsGlobalGateway)
		{
			IsWorkingOnPrimaryService = true;
			if (actAsGlobalGateway)
			{
				PrimaryDnsCryptProxyManager.DnsCryptProxy.Parameter.LocalAddress = Global.GlobalGatewayAddress;
				PrimaryDnsCryptProxyManager.DnsCryptProxy.Parameter.LocalPort = Global.PrimaryResolverPort;
				PrimaryDnsCryptProxyManager.WriteRegistry(DnsCryptProxyType.Primary);
				await Task.Run(() => { PrimaryDnsCryptProxyManager.Restart(); }).ConfigureAwait(false);
				Thread.Sleep(Global.ServiceRestartTime);
				_isPrimaryResolverRunning = PrimaryDnsCryptProxyManager.IsDnsCryptProxyRunning();
				NotifyOfPropertyChange(() => IsPrimaryResolverRunning);
				PrimaryResolverTitle =
					$"{LocalizationEx.GetUiString("default_settings_primary_header", Thread.CurrentThread.CurrentCulture)} ({Global.GlobalGatewayAddress}:{Global.PrimaryResolverPort})";
			}
			else
			{
				PrimaryDnsCryptProxyManager.DnsCryptProxy.Parameter.LocalAddress = Global.PrimaryResolverAddress;
				PrimaryDnsCryptProxyManager.DnsCryptProxy.Parameter.LocalPort = Global.PrimaryResolverPort;
				PrimaryDnsCryptProxyManager.WriteRegistry(DnsCryptProxyType.Primary);
				await Task.Run(() => { PrimaryDnsCryptProxyManager.Restart(); }).ConfigureAwait(false);
				Thread.Sleep(Global.ServiceRestartTime);
				_isPrimaryResolverRunning = PrimaryDnsCryptProxyManager.IsDnsCryptProxyRunning();
				NotifyOfPropertyChange(() => IsPrimaryResolverRunning);
				PrimaryResolver.LocalPort = Global.PrimaryResolverPort; // reset the resolver port
				PrimaryResolverTitle =
					$"{LocalizationEx.GetUiString("default_settings_primary_header", Thread.CurrentThread.CurrentCulture)}";
			}
			IsWorkingOnPrimaryService = false;
		}

		public List<string> Plugins
		{
			get => _plugins;
			set
			{
				_plugins = value;
				NotifyOfPropertyChange(() => Plugins);
			}
		}

		public void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!(e.Source is TabControl source)) return;
			var t = source;
			switch (t.SelectedIndex)
			{
				case 2:
					BlockViewModel.SetPlugins(Plugins);
					break;
				case 3:
					LogViewModel.RefreshPluginData();
					break;
			}
		}

		public void OpenPluginManager()
		{
			var win = new PluginManagerViewModel
			{
				WindowTitle = LocalizationEx.GetUiString("window_plugin_title", Thread.CurrentThread.CurrentCulture)
			};
			win.SetPlugins(Plugins);
			dynamic settings = new ExpandoObject();
			settings.WindowStartupLocation = WindowStartupLocation.CenterOwner;
			settings.Owner = GetView();
			var inputOk = _windowManager.ShowDialog(win, null, settings);
			if (inputOk == true)
			{
				Plugins = win.Plugins;
				ReloadResolver(DnsCryptProxyType.Primary);
				ReloadResolver(DnsCryptProxyType.Secondary);
			}
		}

		public bool UseTcpOnly
		{
			get => _useTcpOnly;
			set
			{
				if (value.Equals(_useTcpOnly)) return;
				_useTcpOnly = value;
				ReloadResolver(DnsCryptProxyType.Primary);
				ReloadResolver(DnsCryptProxyType.Secondary);
				NotifyOfPropertyChange(() => UseTcpOnly);
			}
		}

		public bool IsRefreshingResolverList
		{
			get => _isRefreshingResolverList;
			set
			{
				_isRefreshingResolverList = value;
				NotifyOfPropertyChange(() => IsRefreshingResolverList);
			}
		}

		public bool IsUninstallingServices
		{
			get => _isUninstallingServices;
			set
			{
				_isUninstallingServices = value;
				NotifyOfPropertyChange(() => IsUninstallingServices);
			}
		}

		#endregion
	}
}