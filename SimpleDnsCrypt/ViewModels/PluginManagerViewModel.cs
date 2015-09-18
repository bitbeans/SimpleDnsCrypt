using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Caliburn.Micro;

namespace SimpleDnsCrypt.ViewModels
{
    [Export]
    public sealed class PluginManagerViewModel : Screen
    {
        private const string LibdcpluginLdns = "libdcplugin_ldns_aaaa_blocking.dll";
        private List<string> _plugins;
        private bool _blockIpv6Plugin;

        public List<string> Plugins
        {
            get { return _plugins; }
            set
            {
                _plugins = value;
                NotifyOfPropertyChange(() => Plugins);
            }
        }

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
        ///     PluginManagerViewModel constructor.
        /// </summary>
        [ImportingConstructor]
        public PluginManagerViewModel()
        {
            _plugins = new List<string>();
            
        }

        public void SetPlugins(List<string> plugins)
        {
            _plugins = plugins;
            foreach (var plugin in _plugins)
            {
                if (plugin.Equals(LibdcpluginLdns))
                {
                    _blockIpv6Plugin = true;
                }
            }
        }

        public void SendOk()
        {
            TryClose(true);
        }
    }
}