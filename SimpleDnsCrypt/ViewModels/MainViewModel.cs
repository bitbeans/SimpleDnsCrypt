using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Caliburn.Micro;
using SimpleDnsCrypt.Config;
using SimpleDnsCrypt.Models;
using SimpleDnsCrypt.Tools;
using WPFLocalizeExtension.Engine;

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

        private readonly IWindowManager _windowManager;
        private bool _actAsGlobalGateway;
        private bool _isDebugModeEnabled;
        private bool _isPrimaryResolverRunning;
        private bool _isRefreshingResolverList;
        private bool _isSecondaryResolverRunning;
        private bool _isUninstallingServices;
        private bool _isWorkingOnPrimaryService;
        private bool _isWorkingOnSecondaryService;
        private int _overlayDependencies;
        private List<string> _plugins;
        private DnsCryptProxyEntry _primaryResolver;
        private string _primaryResolverTitle;
        private List<DnsCryptProxyEntry> _resolvers;
        private DnsCryptProxyEntry _secondaryResolver;
        private string _secondaryResolverTitle;
        private bool _showHiddenCards;
        private bool _useTcpOnly;

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
            _windowManager = windowManager;
            eventAggregator.Subscribe(this);

            // automatically use the correct translations if available (fallback: en)
            LocalizeDictionary.Instance.SetCurrentThreadCulture = true;
            LocalizeDictionary.Instance.Culture = Thread.CurrentThread.CurrentCulture;

            // this is already defined in the app.manifest, but to be sure check it again
            if (!IsAdministrator())
            {
                _windowManager.ShowMetroMessageBox(
                    LocalizationEx.GetUiString("dialog_message_bad_privileges", Thread.CurrentThread.CurrentCulture),
                    LocalizationEx.GetUiString("dialog_error_title", Thread.CurrentThread.CurrentCulture),
                    MessageBoxButton.OK, BoxType.Error);
                Environment.Exit(1);
            }

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

            DisplayName = string.Format("{0} {1} ({2})", Global.ApplicationName, VersionUtilities.PublishVersion,
                LocalizationEx.GetUiString("global_ipv6_disabled", Thread.CurrentThread.CurrentCulture));
            _isDebugModeEnabled = false;
            _resolvers = new List<DnsCryptProxyEntry>();
            _isWorkingOnPrimaryService = false;
            _isWorkingOnSecondaryService = false;

            LocalNetworkInterfaces = new CollectionViewSource {Source = _localNetworkInterfaces};
            PrimaryDnsCryptProxyManager = new DnsCryptProxyManager(DnsCryptProxyType.Primary);
            SecondaryDnsCryptProxyManager = new DnsCryptProxyManager(DnsCryptProxyType.Secondary);
            ShowHiddenCards = false;


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

            var proxyList = Path.Combine(Directory.GetCurrentDirectory(),
                Global.DnsCryptProxyFolder, Global.DnsCryptProxyResolverListName);
            var proxyListSignature = Path.Combine(Directory.GetCurrentDirectory(),
                Global.DnsCryptProxyFolder, Global.DnsCryptProxySignatureFileName);
			//TODO: make UpdateResolverListOnStart configurable
			if (!File.Exists(proxyList) || !File.Exists(proxyListSignature) || Global.UpdateResolverListOnStart)
            {
				// download and verify the proxy list if there is none.
				// it`s a really small file, so go on.
	            RefreshResolverList();
                //DnsCryptProxyListManager.UpdateResolverListAsync().ConfigureAwait(false);
            }

            var dnsProxyList =
                DnsCryptProxyListManager.ReadProxyList(proxyList, proxyListSignature, true);
            if (dnsProxyList != null && dnsProxyList.Any())
            {
                foreach (var dnsProxy in dnsProxyList)
                {
                    if (
                        dnsProxy.Name.Equals(
                            PrimaryDnsCryptProxyManager.DnsCryptProxy.Parameter.ResolverName))
                    {
                        _primaryResolver = dnsProxy;
						// restore the local port
	                    _primaryResolver.LocalPort = PrimaryDnsCryptProxyManager.DnsCryptProxy.Parameter.LocalPort;
                    }
                    if (
                        dnsProxy.Name.Equals(
                            SecondaryDnsCryptProxyManager.DnsCryptProxy.Parameter.ResolverName))
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
                        proxyList, proxyListSignature),
                    LocalizationEx.GetUiString("dialog_error_title", Thread.CurrentThread.CurrentCulture),
                    MessageBoxButton.OK, BoxType.Error);
                Environment.Exit(1);
            }

            // if there is no selected primary resolver, add a default resolver
            if (PrimaryResolver == null)
            {
                var tmpResolver = dnsProxyList.SingleOrDefault(d => d.Name.Equals(Global.DefaultPrimaryResolverName));
                if (tmpResolver == null)
                {
                    tmpResolver =
                        dnsProxyList.SingleOrDefault(d => d.Name.Equals(Global.DefaultPrimaryBackupResolverName));
                }
                PrimaryResolver = tmpResolver;
            }

            // if there is no selected secondary resolver, add a default resolver
            if (SecondaryResolver == null)
            {
                var tmpResolver =
                    dnsProxyList.SingleOrDefault(d => d.Name.Equals(Global.DefaultSecondaryResolverName));
                if (tmpResolver == null)
                {
                    tmpResolver =
                        dnsProxyList.SingleOrDefault(d => d.Name.Equals(Global.DefaultSecondaryBackupResolverName));
                }
                SecondaryResolver = tmpResolver;
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

            if (
                PrimaryDnsCryptProxyManager.DnsCryptProxy.Parameter.LocalAddress.Equals(
                    Global.GlobalGatewayAddress))
            {
                _actAsGlobalGateway = true;
                _primaryResolverTitle = string.Format("{0} ({1}:{2})",
                    LocalizationEx.GetUiString("default_settings_primary_header",
                        Thread.CurrentThread.CurrentCulture),
                    Global.GlobalGatewayAddress, Global.PrimaryResolverPort);
            }
            else
            {
                _actAsGlobalGateway = false;
				_primaryResolverTitle = string.Format("{0}",
				   LocalizationEx.GetUiString("default_settings_primary_header",
					   Thread.CurrentThread.CurrentCulture));
			}

            _secondaryResolverTitle = string.Format("{0} ({1}:{2})",
                LocalizationEx.GetUiString("default_settings_secondary_header", Thread.CurrentThread.CurrentCulture),
                Global.SecondaryResolverAddress,
                Global.SecondaryResolverPort);

            // check for new version on every application start
            UpdateAsync();
        }

        /// <summary>
        ///     Overlay management for MetroMessageBoxViewModel.
        /// </summary>
        public bool IsOverlayVisible
        {
            get { return _overlayDependencies > 0; }
        }

        public DnsCryptProxyManager PrimaryDnsCryptProxyManager { get; set; }
        public DnsCryptProxyManager SecondaryDnsCryptProxyManager { get; set; }

        public CollectionViewSource LocalNetworkInterfaces { get; set; }

        /// <summary>
        ///     The list of loaded resolvers.
        /// </summary>
        public List<DnsCryptProxyEntry> Resolvers
        {
            get { return _resolvers; }
            set
            {
                if (value.Equals(_resolvers)) return;
                _resolvers = value;
                NotifyOfPropertyChange(() => Resolvers);
            }
        }

        /// <summary>
        ///     The selected primary resolver.
        /// </summary>
        public DnsCryptProxyEntry PrimaryResolver
        {
            get { return _primaryResolver; }
            set
            {
                if (value.Equals(_primaryResolver)) return;
                _primaryResolver = value;
                ReloadResolver(DnsCryptProxyType.Primary);
                NotifyOfPropertyChange(() => PrimaryResolver);
            }
        }

        /// <summary>
        ///     The selected secondary resolver.
        /// </summary>
        public DnsCryptProxyEntry SecondaryResolver
        {
            get { return _secondaryResolver; }
            set
            {
                if (value.Equals(_secondaryResolver)) return;
                _secondaryResolver = value;
                ReloadResolver(DnsCryptProxyType.Secondary);
                NotifyOfPropertyChange(() => SecondaryResolver);
            }
        }

        /// <summary>
        ///     Show or hide the filtered network cards.
        /// </summary>
        public bool ShowHiddenCards
        {
            get { return _showHiddenCards; }
            set
            {
                _showHiddenCards = value;
                LoadNetworkCards();
                NotifyOfPropertyChange(() => ShowHiddenCards);
            }
        }

        /// <summary>
        ///     Formatted name of the primary resolver.
        /// </summary>
        public string PrimaryResolverTitle
        {
            get { return _primaryResolverTitle; }
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
            get { return _secondaryResolverTitle; }
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
            get { return _isPrimaryResolverRunning; }
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
            get { return _isSecondaryResolverRunning; }
            set
            {
                HandleService(DnsCryptProxyType.Secondary);
                NotifyOfPropertyChange(() => IsSecondaryResolverRunning);
            }
        }

        /// <summary>
        ///     Show or hide the progress bar for the primary resolver.
        /// </summary>
        public bool IsWorkingOnPrimaryService
        {
            get { return _isWorkingOnPrimaryService; }
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
            get { return _isWorkingOnSecondaryService; }
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
        ///     Method to check if there is a new application version available.
        /// </summary>
        private async void UpdateAsync()
        {
            try
            {
                var update = await ApplicationUpdater.CheckForRemoteUpdateAsync().ConfigureAwait(true);
                if (update.CanUpdate)
                {
                    var boxType = (update.Update.Type == UpdateType.Standard) ? BoxType.Default : BoxType.Warning;
                    var boxText = (update.Update.Type == UpdateType.Standard)
                        ? LocalizationEx.GetUiString("dialog_message_update_standard_text",
                            Thread.CurrentThread.CurrentCulture)
                        : LocalizationEx.GetUiString("dialog_message_update_critical_text",
                            Thread.CurrentThread.CurrentCulture);
                    var boxTitle = (update.Update.Type == UpdateType.Standard)
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
                        DisplayName =
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
            }
            catch (Exception)
            {
            }
        }

	    public void SavePrimaryPort()
	    {
			ReloadResolver(DnsCryptProxyType.Primary);
        }

		private void ReloadResolver(DnsCryptProxyType dnsCryptProxyType)
        {
            if (dnsCryptProxyType == DnsCryptProxyType.Primary)
            {
                if (_primaryResolver != null)
                {
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
                if (ActAsGlobalGateway)
                {
                    dnsCryptProxyParameter.LocalAddress = Global.GlobalGatewayAddress;
                }
                else
                {
                    dnsCryptProxyParameter.LocalAddress = Global.PrimaryResolverAddress;
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
                IsWorkingOnPrimaryService = false;
            }
            else
            {
                IsWorkingOnSecondaryService = true;
                await Task.Run(() => { SecondaryDnsCryptProxyManager.Restart(); }).ConfigureAwait(false);
                Thread.Sleep(Global.ServiceRestartTime);
                _isSecondaryResolverRunning = SecondaryDnsCryptProxyManager.IsDnsCryptProxyRunning();
                NotifyOfPropertyChange(() => IsSecondaryResolverRunning);
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
                    }
                    else
                    {
                        //install and start the service
                        var installResult =
                            await
                                Task.Run(() => { return PrimaryDnsCryptProxyManager.Install(); }).ConfigureAwait(false);
                        Thread.Sleep(Global.ServiceStartTime);
                        _isPrimaryResolverRunning = PrimaryDnsCryptProxyManager.IsDnsCryptProxyRunning();
                        NotifyOfPropertyChange(() => IsPrimaryResolverRunning);

                        if (IsDebugModeEnabled)
                        {
                            if (installResult.Success)
                            {
                                _windowManager.ShowMetroMessageBox(
                                    installResult.StandardOutput,
                                    LocalizationEx.GetUiString("dialog_success_title",
                                        Thread.CurrentThread.CurrentCulture),
                                    MessageBoxButton.OK, BoxType.Default);
                            }
                            else
                            {
                                _windowManager.ShowMetroMessageBox(
                                    installResult.StandardOutput,
                                    LocalizationEx.GetUiString("dialog_error_title", Thread.CurrentThread.CurrentCulture),
                                    MessageBoxButton.OK, BoxType.Error);
                            }
                        }
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
                    }
                    else
                    {
                        //install and start the service
                        var installResult =
                            await
                                Task.Run(() => { return SecondaryDnsCryptProxyManager.Install(); })
                                    .ConfigureAwait(false);
                        Thread.Sleep(Global.ServiceStartTime);
                        _isSecondaryResolverRunning = SecondaryDnsCryptProxyManager.IsDnsCryptProxyRunning();
                        NotifyOfPropertyChange(() => IsSecondaryResolverRunning);

                        if (IsDebugModeEnabled)
                        {
                            if (installResult.Success)
                            {
                                _windowManager.ShowMetroMessageBox(
                                    installResult.StandardOutput,
                                    LocalizationEx.GetUiString("dialog_success_title",
                                        Thread.CurrentThread.CurrentCulture),
                                    MessageBoxButton.OK, BoxType.Default);
                            }
                            else
                            {
                                _windowManager.ShowMetroMessageBox(
                                    installResult.StandardOutput,
                                    LocalizationEx.GetUiString("dialog_error_title", Thread.CurrentThread.CurrentCulture),
                                    MessageBoxButton.OK, BoxType.Error);
                            }
                        }
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
            var localNetworkInterfaces = LocalNetworkInterfaceManager.GetLocalNetworkInterfaces(ShowHiddenCards, false);
            _localNetworkInterfaces.Clear();
            if (localNetworkInterfaces.Count != 0)
            {
                foreach (var localNetworkInterface in localNetworkInterfaces)
                {
                    _localNetworkInterfaces.Add(localNetworkInterface);
                }
            }
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
                localNetworkInterface.UseDnsCrypt = !status;
            }
            else
            {
                var dns = new List<string>();
                if (PrimaryResolver != null)
                {
                    if (!string.IsNullOrEmpty(PrimaryResolver.ProviderPublicKey))
                    {
						// only add the local address if the proxy is running 
						if (PrimaryDnsCryptProxyManager.DnsCryptProxy.IsReady && PrimaryDnsCryptProxyManager.IsDnsCryptProxyRunning())
                        {
                            dns.Add(Global.PrimaryResolverAddress);
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
                            dns.Add(Global.SecondaryResolverAddress);
                        }
                    }
                }
				var status = LocalNetworkInterfaceManager.SetNameservers(localNetworkInterface, dns, NetworkInterfaceComponent.IPv4);
                localNetworkInterface.UseDnsCrypt = status;
            }
	        LoadNetworkCards();
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
                return (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
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
        /// <returns><c>true</c> if alle files are available, otherwise <c>false</c></returns>
        private static bool ValidateDnsCryptProxyFolder()
        {
            foreach (var proxyFile in Global.DnsCryptProxyFiles)
            {
                //TODO: we also could do a security check with some blake2b checksums
                if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), Global.DnsCryptProxyFolder,
                    proxyFile)))
                {
                    return false;
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
            foreach (var nic in LocalNetworkInterfaceManager.GetLocalNetworkInterfaces(true, false))
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
        public async void RefreshResolverList()
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
                    DnsCryptProxyListManager.ReadProxyList(proxyList, proxyListSignature, true);
                if (dnsProxyList != null && dnsProxyList.Any())
                {
                    _resolvers.Clear();
                    foreach (var dnsProxy in dnsProxyList)
                    {
                        if (
                            dnsProxy.ProviderPublicKey.Equals(
                                PrimaryDnsCryptProxyManager.DnsCryptProxy.Parameter.ProviderKey))
                        {
                            _primaryResolver = dnsProxy;
                        }
                        if (
                            dnsProxy.ProviderPublicKey.Equals(
                                SecondaryDnsCryptProxyManager.DnsCryptProxy.Parameter.ProviderKey))
                        {
                            _secondaryResolver = dnsProxy;
                        }
                        _resolvers.Add(dnsProxy);
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
        }

        public bool ActAsGlobalGateway
        {
            get { return _actAsGlobalGateway; }
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
                PrimaryResolverTitle = string.Format("{0} ({1}:{2})",
                    LocalizationEx.GetUiString("default_settings_primary_header", Thread.CurrentThread.CurrentCulture),
                    Global.GlobalGatewayAddress, Global.PrimaryResolverPort);
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
				PrimaryResolverTitle = string.Format("{0}",
                    LocalizationEx.GetUiString("default_settings_primary_header", Thread.CurrentThread.CurrentCulture));
            }
            IsWorkingOnPrimaryService = false;
        }

        public List<string> Plugins
        {
            get { return _plugins; }
            set
            {
                _plugins = value;
                NotifyOfPropertyChange(() => Plugins);
            }
        }

        public void OpenPluginManager()
        {
            var win = new PluginManagerViewModel
            {
                DisplayName = LocalizationEx.GetUiString("window_plugin_title", Thread.CurrentThread.CurrentCulture)
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
            get { return _useTcpOnly; }
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
            get { return _isRefreshingResolverList; }
            set
            {
                _isRefreshingResolverList = value;
                NotifyOfPropertyChange(() => IsRefreshingResolverList);
            }
        }

        public bool IsUninstallingServices
        {
            get { return _isUninstallingServices; }
            set
            {
                _isUninstallingServices = value;
                NotifyOfPropertyChange(() => IsUninstallingServices);
            }
        }

        public bool IsDebugModeEnabled
        {
            get { return _isDebugModeEnabled; }
            set
            {
                _isDebugModeEnabled = value;
                NotifyOfPropertyChange(() => IsDebugModeEnabled);
            }
        }

        #endregion
    }
}