using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using SimpleDnsCrypt.Helper;
using SimpleDnsCrypt.Models;

namespace SimpleDnsCrypt.ViewModels
{
	[Export(typeof(AboutViewModel))]
	public class AboutViewModel : Screen
	{
		private readonly IWindowManager _windowManager;
		private readonly IEventAggregator _events;
		private string _windowTitle;

		private BindableCollection<License> _licenses;
		private License _selectedLicense;

		public AboutViewModel()
		{
		}

		[ImportingConstructor]
		public AboutViewModel(IWindowManager windowManager, IEventAggregator events)
		{
			_windowManager = windowManager;
			_events = events;
			_events.Subscribe(this);
			AddLicenses();
			SelectedLicense = _licenses[0];
		}

		public void HyperlinkRequestNavigate(string type)
		{
			try
			{
				if (string.IsNullOrEmpty(type)) return;
				if (SelectedLicense == null) return;
				if (type.Equals("code"))
				{
					if (SelectedLicense.LicenseCodeLink == null) return;
					if (SelectedLicense.LicenseCodeLink.LinkUri == null) return;
					Process.Start(SelectedLicense.LicenseCodeLink.LinkUri.ToString());
				}
				else if (type.Equals("regular"))
				{
					if (SelectedLicense.LicenseRegularLink == null) return;
					if (SelectedLicense.LicenseRegularLink.LinkUri == null) return;
					Process.Start(SelectedLicense.LicenseRegularLink.LinkUri.ToString());
				}
			}
			catch (Exception)
			{
			}
		}

		/// <summary>
		///     Add the license of the libraries and software we use.
		/// </summary>
		private async void AddLicenses()
		{
			try
			{
				var tmpList = new List<License>
				{
					new License
					{
						LicenseHeaderText = "Simple DNSCrypt",
						LicenseText = await LoadLicense("SimpleDNSCrypt.txt").ConfigureAwait(false),
						LicenseRegularLink = new LicenseLink
						{
							LinkText = "simplednscrypt.org",
							LinkUri = new Uri("https://simplednscrypt.org/")
						},
						LicenseCodeLink = new LicenseLink
						{
							LinkText = LocalizationEx.GetUiString("about_view_on_github", Thread.CurrentThread.CurrentCulture),
							LinkUri = new Uri("https://github.com/bitbeans/SimpleDnsCrypt")
						}
					},
					new License
					{
						LicenseHeaderText = "dnscrypt-proxy",
						LicenseText = await LoadLicense("dnscrypt-proxy.txt").ConfigureAwait(false),
						LicenseCodeLink = new LicenseLink
						{
							LinkText = LocalizationEx.GetUiString("about_view_on_github", Thread.CurrentThread.CurrentCulture),
							LinkUri = new Uri("https://github.com/jedisct1/dnscrypt-proxy")
						}
					},
					new License
					{
						LicenseHeaderText = "Caliburn.Micro",
						LicenseText = await LoadLicense("Caliburn.Micro.txt").ConfigureAwait(false),
						LicenseRegularLink = new LicenseLink
						{
							LinkText = "caliburnmicro.com",
							LinkUri = new Uri("https://caliburnmicro.com/")
						},
						LicenseCodeLink = new LicenseLink
						{
							LinkText = LocalizationEx.GetUiString("about_view_on_github", Thread.CurrentThread.CurrentCulture),
							LinkUri = new Uri("https://github.com/Caliburn-Micro/Caliburn.Micro")
						}
					},
					new License
					{
						LicenseHeaderText = "MahApps.Metro",
						LicenseText = await LoadLicense("MahApps.Metro.txt").ConfigureAwait(false),
						LicenseRegularLink = new LicenseLink
						{
							LinkText = "mahapps.com [http]",
							LinkUri = new Uri("http://mahapps.com/")
						},
						LicenseCodeLink = new LicenseLink
						{
							LinkText = LocalizationEx.GetUiString("about_view_on_github", Thread.CurrentThread.CurrentCulture),
							LinkUri = new Uri("https://github.com/MahApps/MahApps.Metro")
						}
					}
				};
				var orderedList = tmpList.OrderBy(l => l.LicenseHeaderText);
				_licenses = new BindableCollection<License>(orderedList);
			}
			catch (Exception)
			{
			}
		}

		/// <summary>
		///     Collection of Licenses
		/// </summary>
		public BindableCollection<License> Licenses
		{
			get => _licenses;
			set
			{
				if (value.Equals(_licenses)) return;
				_licenses = value;
				NotifyOfPropertyChange(() => Licenses);
			}
		}

		/// <summary>
		///     The selected license in the UI.
		/// </summary>
		public License SelectedLicense
		{
			get => _selectedLicense;
			set
			{
				if (value.Equals(_selectedLicense)) return;
				_selectedLicense = value;
				NotifyOfPropertyChange(() => SelectedLicense);
			}
		}

		/// <summary>
		///     The title of the window.
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
		///     Loads a license as string from an embedded resource text file.
		/// </summary>
		/// <param name="licenseFileName">Name of the license file.</param>
		/// <returns>The license as string.</returns>
		private static async Task<string> LoadLicense(string licenseFileName)
		{
			Stream stream = null;
			try
			{
				var assembly = Assembly.GetExecutingAssembly();
				var resourceName = "SimpleDnsCrypt.Resources.Licenses." + licenseFileName;
				stream = assembly.GetManifestResourceStream(resourceName);
				if (stream != null)
					using (var reader = new StreamReader(stream))
					{
						return await reader.ReadToEndAsync().ConfigureAwait(false);
					}
			}
			catch (Exception)
			{
				return "License missing";
			}
			finally
			{
				stream?.Dispose();
			}
			return null;
		}
	}
}