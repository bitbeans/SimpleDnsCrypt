using System;
using System.Collections.Generic;
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
        private const string LibdcpluginLdns = "libdcplugin_ldns_aaaa_blocking.dll";
        private const string LibdcpluginLogging = "libdcplugin_logging.dll";
        private bool _blockIpv6Plugin;
        private bool _logPlugin;
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
                    Plugins.Add(LibdcpluginLdns);
                }
                else
                {
                    Plugins.Remove(LibdcpluginLdns);
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
        ///     Set the used plugins.
        /// </summary>
        /// <param name="plugins">List of plugins.</param>
        public void SetPlugins(List<string> plugins)
        {
            _plugins = plugins;
            foreach (var plugin in _plugins)
            {
                if (plugin.Equals(LibdcpluginLdns))
                {
                    _blockIpv6Plugin = true;
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