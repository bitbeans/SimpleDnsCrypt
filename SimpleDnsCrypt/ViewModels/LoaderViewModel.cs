using Caliburn.Micro;
using minisign;
using SimpleDnsCrypt.Config;
using SimpleDnsCrypt.Helper;
using SimpleDnsCrypt.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WPFLocalizeExtension.Engine;

namespace SimpleDnsCrypt.ViewModels
{
	[Export(typeof(LoaderViewModel))]
	public class LoaderViewModel : Screen
	{
		private readonly IWindowManager _windowManager;
		private readonly IEventAggregator _events;
		private static readonly ILog Log = LogManagerHelper.Factory();
		private readonly MainViewModel _mainViewModel;
		private readonly SystemTrayViewModel _systemTrayViewModel;

		private string _progressText;
		private string _titleText;

		public LoaderViewModel()
		{

		}

		private async void InitializeApplication()
		{
			try
			{
				if (IsAdministrator())
				{
					ProgressText =
						LocalizationEx.GetUiString("loader_administrative_rights_available", Thread.CurrentThread.CurrentCulture);
				}
				else
				{
					ProgressText =
						LocalizationEx.GetUiString("loader_administrative_rights_missing", Thread.CurrentThread.CurrentCulture);
					await Task.Delay(3000).ConfigureAwait(false);
					Process.GetCurrentProcess().Kill();
				}

				ProgressText = LocalizationEx.GetUiString("loader_redistributable_package_check", Thread.CurrentThread.CurrentCulture);
				if (PrerequisiteHelper.IsRedistributablePackageInstalled())
				{
					ProgressText = LocalizationEx.GetUiString("loader_redistributable_package_already_installed", Thread.CurrentThread.CurrentCulture);
				}
				else
				{
					ProgressText = LocalizationEx.GetUiString("loader_redistributable_package_installing", Thread.CurrentThread.CurrentCulture);
					//minisign needs this (to verify the installer with libsodium)
					await PrerequisiteHelper.DownloadAndInstallRedistributablePackage();
					if (PrerequisiteHelper.IsRedistributablePackageInstalled())
					{
						ProgressText = LocalizationEx.GetUiString("loader_redistributable_package_ready", Thread.CurrentThread.CurrentCulture);
						await Task.Delay(1000).ConfigureAwait(false);
					}
				}

				if (Properties.Settings.Default.AutoUpdate)
				{
					ProgressText = LocalizationEx.GetUiString("loader_checking_version", Thread.CurrentThread.CurrentCulture);
					//TODO: remove in future version (for now we only have one channel)
					Properties.Settings.Default.MinUpdateType = 2;
					Properties.Settings.Default.Save();
					var minUpdateType = (UpdateType)Properties.Settings.Default.MinUpdateType;
					var update = await ApplicationUpdater.CheckForRemoteUpdateAsync(minUpdateType).ConfigureAwait(false);
					if (update.CanUpdate)
					{
						ProgressText =
							string.Format(LocalizationEx.GetUiString("loader_new_version_found", Thread.CurrentThread.CurrentCulture),
								update.Update.Version);
						await Task.Delay(200).ConfigureAwait(false);
						var installer = await StartRemoteUpdateDownload(update).ConfigureAwait(false);
						if (!string.IsNullOrEmpty(installer) && File.Exists(installer))
						{
							ProgressText = LocalizationEx.GetUiString("loader_starting_update", Thread.CurrentThread.CurrentCulture);
							await Task.Delay(200).ConfigureAwait(false);
							if (Properties.Settings.Default.AutoUpdate)
							{
								// auto install
								const string arguments = "/qb /passive /norestart";
								var startInfo = new ProcessStartInfo(installer)
								{
									Arguments = arguments,
									UseShellExecute = false
								};
								Process.Start(startInfo);
							}
							else
							{
								Process.Start(installer);
							}
							Process.GetCurrentProcess().Kill();
						}
						else
						{
							await Task.Delay(500).ConfigureAwait(false);
							ProgressText = LocalizationEx.GetUiString("loader_update_failed", Thread.CurrentThread.CurrentCulture);
						}
					}
					else
					{
						ProgressText = LocalizationEx.GetUiString("loader_latest_version", Thread.CurrentThread.CurrentCulture);
					}
				}
				else
				{
					await Task.Delay(500).ConfigureAwait(false);
				}

				ProgressText =
					string.Format(LocalizationEx.GetUiString("loader_validate_folder", Thread.CurrentThread.CurrentCulture),
						Global.DnsCryptProxyFolder);
				var validatedFolder = ValidateDnsCryptProxyFolder();
				if (validatedFolder.Count == 0)
				{
					ProgressText = LocalizationEx.GetUiString("loader_all_files_available", Thread.CurrentThread.CurrentCulture);
				}
				else
				{
					var fileErrors = "";
					foreach (var pair in validatedFolder)
					{
						fileErrors += $"{pair.Key}: {pair.Value}\n";
					}

					ProgressText =
						string.Format(
							LocalizationEx.GetUiString("loader_missing_files", Thread.CurrentThread.CurrentCulture).Replace("\\n", "\n"),
							Global.DnsCryptProxyFolder, fileErrors, Global.ApplicationName);
					await Task.Delay(5000).ConfigureAwait(false);
					Process.GetCurrentProcess().Kill();
				}

				ProgressText = string.Format(LocalizationEx.GetUiString("loader_loading", Thread.CurrentThread.CurrentCulture),
					Global.DnsCryptConfigurationFile);
				if (DnscryptProxyConfigurationManager.LoadConfiguration())
				{
					ProgressText =
						string.Format(LocalizationEx.GetUiString("loader_successfully_loaded", Thread.CurrentThread.CurrentCulture),
							Global.DnsCryptConfigurationFile);
					_mainViewModel.DnscryptProxyConfiguration = DnscryptProxyConfigurationManager.DnscryptProxyConfiguration;
				}
				else
				{
					ProgressText =
						string.Format(LocalizationEx.GetUiString("loader_failed_loading", Thread.CurrentThread.CurrentCulture),
							Global.DnsCryptConfigurationFile);
					await Task.Delay(5000).ConfigureAwait(false);
					Process.GetCurrentProcess().Kill();
				}

				ProgressText = LocalizationEx.GetUiString("loader_loading_network_cards", Thread.CurrentThread.CurrentCulture);

				List<LocalNetworkInterface> localNetworkInterfaces;
				if (DnscryptProxyConfigurationManager.DnscryptProxyConfiguration.listen_addresses.Contains(Global.GlobalResolver))
				{
					var dnsServer = new List<string>
					{
						Global.DefaultResolverIpv4,
						Global.DefaultResolverIpv6
					};
					localNetworkInterfaces = LocalNetworkInterfaceManager.GetLocalNetworkInterfaces(dnsServer);
				}
				else
				{
					localNetworkInterfaces = LocalNetworkInterfaceManager.GetLocalNetworkInterfaces(
						DnscryptProxyConfigurationManager.DnscryptProxyConfiguration.listen_addresses.ToList());
				}
				_mainViewModel.LocalNetworkInterfaces = new BindableCollection<LocalNetworkInterface>();
				_mainViewModel.LocalNetworkInterfaces.AddRange(localNetworkInterfaces);
				_mainViewModel.Initialize();
				ProgressText = LocalizationEx.GetUiString("loader_starting", Thread.CurrentThread.CurrentCulture);

				if (Properties.Settings.Default.TrayMode)
				{
					Execute.OnUIThread(() => _windowManager.ShowWindow(_systemTrayViewModel));
					Execute.OnUIThread(() => _systemTrayViewModel.ShowWindow());
				}
				else
				{
					Execute.OnUIThread(() => _windowManager.ShowWindow(_mainViewModel));
				}

				TryClose(true);
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}
		}

		[ImportingConstructor]
		public LoaderViewModel(IWindowManager windowManager, IEventAggregator events)
		{
			if (Properties.Settings.Default.UpgradeRequired)
			{
				Properties.Settings.Default.Upgrade();
				Properties.Settings.Default.UpgradeRequired = false;
				Properties.Settings.Default.Save();
			}

			_windowManager = windowManager;
			_events = events;
			_events.Subscribe(this);
			_titleText = $"{Global.ApplicationName} {VersionHelper.PublishVersion} {VersionHelper.PublishBuild}";
			LocalizeDictionary.Instance.SetCurrentThreadCulture = true;
			var languages = LocalizationEx.GetSupportedLanguages();
			if (!string.IsNullOrEmpty(Properties.Settings.Default.PreferredLanguage))
			{
				Log.Info($"Preferred language: {Properties.Settings.Default.PreferredLanguage}");
				var preferredLanguage = languages.FirstOrDefault(l => l.ShortCode.Equals(Properties.Settings.Default.PreferredLanguage));
				LocalizeDictionary.Instance.Culture = preferredLanguage != null ? new CultureInfo(preferredLanguage.CultureCode) : Thread.CurrentThread.CurrentCulture;
			}
			else
			{
				var language = languages.FirstOrDefault(l =>
					l.ShortCode.Equals(Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName));
				if (language != null)
				{
					Log.Info($"Using {language.ShortCode} as language");
					LocalizeDictionary.Instance.Culture = new CultureInfo(language.CultureCode);
				}
				else
				{
					Log.Warn($"Translation for {Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName} is not available");
					LocalizeDictionary.Instance.Culture = new CultureInfo("en");
				}
			}

			var selectedLanguage = languages.SingleOrDefault(l => l.ShortCode.Equals(LocalizeDictionary.Instance.Culture.TwoLetterISOLanguageName)) ??
			                       languages.SingleOrDefault(l => l.ShortCode.Equals(LocalizeDictionary.Instance.Culture.Name));

			
			_mainViewModel = new MainViewModel(_windowManager, _events)
			{
				Languages = languages,
				SelectedLanguage = selectedLanguage
			};
			_systemTrayViewModel = new SystemTrayViewModel(_windowManager, _events, _mainViewModel);

			InitializeApplication();
		}
		
		public string TitleText
		{
			get => _titleText;
			set
			{
				_titleText = value;
				NotifyOfPropertyChange(() => TitleText);
			}
		}

		public string ProgressText
		{
			get => _progressText;
			set
			{
				_progressText = value;
				NotifyOfPropertyChange(() => ProgressText);
			}
		}

		private async Task<string> StartRemoteUpdateDownload(RemoteUpdate remoteUpdate)
		{
			var installerPath = string.Empty;
			try
			{
				ProgressText = LocalizationEx.GetUiString("loader_downloading_signature", Thread.CurrentThread.CurrentCulture);
				var signature = await ApplicationUpdater.DownloadRemoteSignatureAsync(remoteUpdate.Update.Signature.Uri).ConfigureAwait(false);
				ProgressText = LocalizationEx.GetUiString("loader_downloading_installer", Thread.CurrentThread.CurrentCulture);
				var installer = await ApplicationUpdater.DownloadRemoteInstallerAsync(remoteUpdate.Update.Installer.Uri).ConfigureAwait(false);
				if (!string.IsNullOrEmpty(signature) && installer != null)
				{
					var s = signature.Split('\n');
					var trimmedComment = s[2].Replace("trusted comment: ", "").Trim();
					var trustedCommentBinary = Encoding.UTF8.GetBytes(trimmedComment);
					var loadedSignature = Minisign.LoadSignature(Convert.FromBase64String(s[1]), trustedCommentBinary,
						Convert.FromBase64String(s[3]));
					var publicKey = Minisign.LoadPublicKeyFromString(Global.ApplicationUpdatePublicKey);
					var valid = Minisign.ValidateSignature(installer, loadedSignature, publicKey);

					if (valid)
					{
						var path = Path.Combine(Path.GetTempPath(), remoteUpdate.Update.Installer.Name);
						File.WriteAllBytes(path, installer);
						if (File.Exists(path))
						{
							installerPath = path;
						}
					}
				}
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}
			return installerPath;
		}

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
		private static Dictionary<string, string> ValidateDnsCryptProxyFolder()
		{
			var report = new Dictionary<string, string>();
			foreach (var proxyFile in Global.DnsCryptProxyFiles)
			{
				var proxyFilePath = Path.Combine(Directory.GetCurrentDirectory(), Global.DnsCryptProxyFolder, proxyFile);
				if (!File.Exists(proxyFilePath))
				{
					report[proxyFile] = LocalizationEx.GetUiString("loader_missing", Thread.CurrentThread.CurrentCulture);
				}
				// exclude this check on dev folders
				
				if (proxyFilePath.Contains("bin\\Debug") || proxyFilePath.Contains("bin\\Release") || proxyFilePath.Contains("bin\\x64")) continue;
				// dnscrypt-resolvers.* files are signed with minisign
				//TODO: re-enable
				/*if (!proxyFile.Equals("dnscrypt-proxy.toml") && !proxyFile.Equals("LICENSE"))
				{
					// check if the file is signed
					if (!AuthenticodeTools.IsTrusted(proxyFilePath))
					{
						report[proxyFile] = LocalizationEx.GetUiString("loader_unsigned", Thread.CurrentThread.CurrentCulture);
					}
				}*/
			}
			return report;
		}
	}
}
