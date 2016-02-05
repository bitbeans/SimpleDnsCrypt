using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows.Forms;
using SimpleDnsCrypt.Config;
using Screen = Caliburn.Micro.Screen;

namespace SimpleDnsCrypt.ViewModels
{
    /// <summary>
    ///     The Pluginmanager.
    /// </summary>
    [Export]
    public sealed class PluginManagerViewModel : Screen
    {
        private const string LibdcpluginLdnsIpv6 = "libdcplugin_ldns_aaaa_blocking.dll";
		private const string LibdcpluginLdns = "libdcplugin_ldns_blocking.dll";
		private const string LibdcpluginLogging = "libdcplugin_logging.dll";
        private bool _blockIpv6Plugin;
        private bool _logPlugin;
		private bool _blacklistPlugin;
		private string _addressBlacklistPath;
		private string _domainBlacklistPath;
		private string _logPluginPath;
        private List<string> _plugins;
	    

	    /// <summary>
        ///     PluginManagerViewModel constructor.
        /// </summary>
        [ImportingConstructor]
        public PluginManagerViewModel()
        {
            _plugins = new List<string>();
        }

        /// <summary>
        ///     List of plugins.
        /// </summary>
        public List<string> Plugins
        {
            get { return _plugins; }
            set
            {
                _plugins = value;
                NotifyOfPropertyChange(() => Plugins);
            }
        }

		/// <summary>
		///     To manage the blacklist plugin.
		/// </summary>
		public bool BlacklistPlugin
		{
			get { return _blacklistPlugin; }
			set
			{
				_blacklistPlugin = value;
				if (value)
				{
					if (AddressBlacklistPath != null && File.Exists(AddressBlacklistPath) || DomainBlacklistPath != null && File.Exists(DomainBlacklistPath))
					{
						var p = LibdcpluginLdns + "";
						var kindCounter = 0;

						if (!string.IsNullOrEmpty(AddressBlacklistPath))
						{
							// dnscrypt-proxy will not start if the file is empty
							if (File.ReadAllLines(AddressBlacklistPath).Length != 0)
							{
								p += ",--ips=" + AddressBlacklistPath;
								kindCounter++;
							}
							else
							{
								// better show a notification to the user
								AddressBlacklistPath = string.Empty;
							}
						}

						if (!string.IsNullOrEmpty(DomainBlacklistPath))
						{
							// dnscrypt-proxy will not start if the file is empty
							if (File.ReadAllLines(DomainBlacklistPath).Length != 0)
							{
								p += ",--domains=" + DomainBlacklistPath;
								kindCounter++;
							}
							else
							{
								// better show a notification to the user
								DomainBlacklistPath = string.Empty;
							}
						}

						if (kindCounter > 0)
						{
							Plugins.Add(p);
							_blacklistPlugin = true;
						}
						else
						{
							_blacklistPlugin = false;
						}
					}
					else
					{
						_blacklistPlugin = false;
					}
				}
				else
				{
					for (var i = 0; i < Plugins.Count; i++)
					{
						if (!Plugins[i].StartsWith(LibdcpluginLdns)) continue;
						Plugins.RemoveAt(i);
						NotifyOfPropertyChange(() => BlacklistPlugin);
					}
				}
				NotifyOfPropertyChange(() => BlacklistPlugin);
			}
		}

		/// <summary>
		///     To manage the block IPv6 plugin.
		/// </summary>
		public bool BlockIpv6Plugin
        {
            get { return _blockIpv6Plugin; }
            set
            {
                _blockIpv6Plugin = value;
                if (value)
                {
                    Plugins.Add(LibdcpluginLdnsIpv6);
                }
                else
                {
                    Plugins.Remove(LibdcpluginLdnsIpv6);
                }
                NotifyOfPropertyChange(() => BlockIpv6Plugin);
            }
        }

        /// <summary>
        ///     To manage the log plugin.
        /// </summary>
        public bool LogPlugin
        {
            get { return _logPlugin; }
            set
            {
                _logPlugin = value;
                if (value)
                {
                    if (LogPluginPath != null && Directory.Exists(Path.GetDirectoryName(LogPluginPath)))
                    {
                        Plugins.Add(LibdcpluginLogging + "," + LogPluginPath);
                        _logPlugin = true;
                    }
                    else
                    {
                        _logPlugin = false;
                    }
                }
                else
                {
                    foreach (var plugin in Plugins)
                    {
                        if (plugin.StartsWith(LibdcpluginLogging))
                        {
                            Plugins.Remove(plugin);
                            NotifyOfPropertyChange(() => LogPlugin);
                        }
                    }
                }
                NotifyOfPropertyChange(() => LogPlugin);
            }
        }

        /// <summary>
        ///     The full path to the log file.
        /// </summary>
        public string LogPluginPath
        {
            get { return _logPluginPath; }
            set
            {
                _logPluginPath = value;
                NotifyOfPropertyChange(() => LogPluginPath);
            }
        }

		/// <summary>
		///     The full path to the domain blacklist file.
		/// </summary>
		public string DomainBlacklistPath
		{
			get { return _domainBlacklistPath; }
			set
			{
				_domainBlacklistPath = value;
				NotifyOfPropertyChange(() => DomainBlacklistPath);
			}
		}

		/// <summary>
		///     The full path to the address blacklist file.
		/// </summary>
		public string AddressBlacklistPath
		{
			get { return _addressBlacklistPath; }
			set
			{
				_addressBlacklistPath = value;
				NotifyOfPropertyChange(() => AddressBlacklistPath);
			}
		}

		/// <summary>
		///     FolderBrowserDialog to select the log folder.
		/// </summary>
		public void SelectFolder()
        {
            try
            {
                var dialog = new FolderBrowserDialog();
                var result = dialog.ShowDialog();
                if (result != DialogResult.OK) return;
                LogPluginPath = Path.Combine(dialog.SelectedPath, Global.DefaultLogFileName);
            }
            catch (Exception)
            {
                LogPluginPath = string.Empty;
            }
        }

		/// <summary>
		///     FileBrowserDialog to select the domain blacklist file.
		/// </summary>
		public void SelectDomainBlacklist()
		{
			try
			{
				var dialog = new OpenFileDialog
				{
					Multiselect = false,
					CheckFileExists = true
				};
				var result = dialog.ShowDialog();
				if (result != DialogResult.OK) return;
				DomainBlacklistPath = dialog.FileName;
			}
			catch (Exception)
			{
				DomainBlacklistPath = string.Empty;
			}
		}

		/// <summary>
		///     FileBrowserDialog to select the address blacklist file.
		/// </summary>
		public void SelectAddressBlacklist()
		{
			try
			{
				var dialog = new OpenFileDialog
				{
					Multiselect = false,
					CheckFileExists = true
				};
				var result = dialog.ShowDialog();
				if (result != DialogResult.OK) return;
				AddressBlacklistPath = dialog.FileName;
			}
			catch (Exception)
			{
				AddressBlacklistPath = string.Empty;
			}
		}

		/// <summary>
		///     Set the used plugins.
		/// </summary>
		/// <param name="plugins">List of plugins.</param>
		public void SetPlugins(List<string> plugins)
        {
            _plugins = plugins;
            foreach (var plugin in _plugins)
            {
                if (plugin.Equals(LibdcpluginLdnsIpv6))
                {
                    _blockIpv6Plugin = true;
                }
				if (plugin.StartsWith(LibdcpluginLdns))
				{
					var a = plugin.Split(',');

					for (var r = 1; r < a.Length; r++)
					{
						if (a[r].StartsWith("--ips"))
						{
							var b = a[r].Split('=');
							_addressBlacklistPath = b[1];
						}
						if (a[r].StartsWith("--domains"))
						{
							var b = a[r].Split('=');
							_domainBlacklistPath = b[1];
						}
					}

					if (!string.IsNullOrEmpty(_addressBlacklistPath) || !string.IsNullOrEmpty(_domainBlacklistPath))
					{
						_blacklistPlugin = true;
					}
				}
				if (plugin.StartsWith(LibdcpluginLogging))
                {
                    var a = plugin.Split(',');
                    _logPluginPath = a[1];
                    _logPlugin = true;
                }
            }
        }

        /// <summary>
        ///     Close the dialog with a positiv result.
        /// </summary>
        public void SendOk()
        {
            TryClose(true);
        }
    }
}