using System;
using Caliburn.Micro;
using SimpleDnsCrypt.Models;
using SimpleDnsCrypt.Tools;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net.Http;
using System.Windows.Data;
using SimpleDnsCrypt.Config;
using SocksSharp;
using SocksSharp.Proxy;

namespace SimpleDnsCrypt.ViewModels
{
	[Export]
	public sealed class AdvancedNetworkSettingsViewModel : Screen
    {
	    private string _windowTitle;
	    public CollectionViewSource InsecureResolverPairs { get; set; }
	    private readonly List<InsecureResolverPair> _insecureResolverPairs;
		private IWindowManager _windowManager;
		private readonly UserData _userData;
	    private ProxySettings _proxySettings;
	    private string _proxySettingsTest;
	    private bool _isTestingProxy;

	    /// <summary>
		///     AdvancedNetworkSettingsViewModel constructor.
		/// </summary>
		[ImportingConstructor]
	    public AdvancedNetworkSettingsViewModel(IWindowManager windowManager, InsecureResolvers insecureResolvers, UserData userData)
		{
			_windowManager = windowManager;
			_userData = userData;
			_insecureResolverPairs = insecureResolvers.InsecureResolverPairs;

			if (_userData.InsecureResolverPair != null)
			{
				if (userData.InsecureResolverPair.Addresses != null)
				{
					if (userData.InsecureResolverPair.Addresses.Count > 0)
					{
						var first = _insecureResolverPairs.FirstOrDefault(i => i.Id == userData.InsecureResolverPair.Id);
						if (first != null) first.IsSelected = true;
					}
					else
					{
						var first = _insecureResolverPairs.FirstOrDefault(i => i.Id == 0);
						if (first != null) first.IsSelected = true;
					}
				}
				else
				{
					var first = _insecureResolverPairs.FirstOrDefault(i => i.Id == 0);
					if (first != null) first.IsSelected = true;
				}
			}
			else
			{
				var first = _insecureResolverPairs.FirstOrDefault(i => i.Id == 0);
				if (first != null) first.IsSelected = true;
			}

			if (_userData.ProxySettings != null)
			{
				_proxySettings = _userData.ProxySettings;
			}

			InsecureResolverPairs = new CollectionViewSource { Source = _insecureResolverPairs };
			_proxySettingsTest = string.Empty;
		}

		public AdvancedNetworkSettingsViewModel()
		{
			
		}

		public void ToogleButtonClicked(InsecureResolverPair clickedInsecureResolverPair)
	    {
		    if (clickedInsecureResolverPair == null) return;
		    _userData.InsecureResolverPair = clickedInsecureResolverPair;
		    _userData.SaveConfigurationFile();
		    foreach (var insecureResolverPair in _insecureResolverPairs)
		    {
			    if (insecureResolverPair != clickedInsecureResolverPair)
			    {
				    insecureResolverPair.IsSelected = false;
			    }
		    }
	    }

		public string WindowTitle
	    {
		    get => _windowTitle;
		    set
		    {
			    _windowTitle = value;
			    NotifyOfPropertyChange(() => WindowTitle);
		    }
	    }

	    public string ProxySettingsTest
		{
		    get => _proxySettingsTest;
		    set
		    {
			    _proxySettingsTest = value;
			    NotifyOfPropertyChange(() => ProxySettingsTest);
		    }
	    }

	    public bool IsTestingProxy
	    {
		    get => _isTestingProxy;
		    set
		    {
			    _isTestingProxy = value;
			    NotifyOfPropertyChange(() => IsTestingProxy);
		    }
	    }

		public ProxySettings ProxySettings
	    {
		    get => _proxySettings;
		    set
		    {
				_proxySettings = value;
			    NotifyOfPropertyChange(() => ProxySettings);
		    }
	    }

		/// <summary>
		///     Close the dialog with a positiv result.
		/// </summary>
		public void SendOk()
	    {
		    TryClose(true);
	    }

	    public async void TestProxy()
	    {
			if (_proxySettings == null) return;
		    try
		    {
			    IsTestingProxy = true;
			    ProxySettingsTest = string.Empty;
			    using (var proxyClientHandler = new ProxyClientHandler<Socks5>(_proxySettings))
			    {
				    using (var client = new HttpClient(proxyClientHandler))
				    {
					    var response = await client.GetStringAsync(Global.ApplicationUpdateUri).ConfigureAwait(false);
					    if (!string.IsNullOrEmpty(response))
					    {
						    //TODO: translate
						    ProxySettingsTest = "success and saved";
						    _userData.ProxySettings = _proxySettings;
						    _userData.SaveConfigurationFile();
					    }
					    else
					    {
						    //TODO: translate
						    ProxySettingsTest = "failed";
					    }
				    }
			    }
		    }
		    catch (Exception)
		    {
			    //TODO: translate
			    ProxySettingsTest = "failed";
		    }
		    finally
		    {
				IsTestingProxy = false;
			}
	    }

	    public void ClearProxy()
	    {
		    ProxySettings = new ProxySettings();
		    ProxySettingsTest = string.Empty;
		    _userData.ProxySettings = _proxySettings;
		    _userData.SaveConfigurationFile();
		}
	}
}
