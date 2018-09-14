using Caliburn.Micro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using SimpleDnsCrypt.Config;
using SimpleDnsCrypt.Helper;
using SimpleDnsCrypt.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DnsCrypt.Blacklist;
using Application = System.Windows.Application;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using Screen = Caliburn.Micro.Screen;


namespace SimpleDnsCrypt.ViewModels
{
	[Export(typeof(DomainBlacklistViewModel))]
	public class DomainBlacklistViewModel : Screen
	{
		private static readonly ILog Log = LogManagerHelper.Factory();
		private readonly IWindowManager _windowManager;
	    private readonly IEventAggregator _events;

		private BindableCollection<string> _domainBlacklistRules;
		private BindableCollection<string> _domainWhitelistRules;
		
		private string _selectedDomainBlacklistEntry;
		private string _selectedDomainWhitelistEntry;
		private string _domainWhitelistRuleFilePath;
		private string _domainBlacklistRuleFilePath;
		private bool _isBlacklistEnabled;
		private string _domainBlacklistFile;


		/// <summary>
		/// Initializes a new instance of the <see cref="DomainBlacklistViewModel"/> class
		/// </summary>
		/// <param name="windowManager">The window manager</param>
		/// <param name="events">The events</param>
		[ImportingConstructor]
	    public DomainBlacklistViewModel(IWindowManager windowManager, IEventAggregator events)
	    {
		    _windowManager = windowManager;
		    _events = events;
		    _events.Subscribe(this);
		    _domainBlacklistRules = new BindableCollection<string>();
		    _domainWhitelistRules = new BindableCollection<string>();

		    if (!string.IsNullOrEmpty(Properties.Settings.Default.DomainBlacklistFile))
		    {
				_domainBlacklistFile = Properties.Settings.Default.DomainBlacklistFile;
			}
			else
		    {
				//set default
				_domainBlacklistFile = Path.Combine(Directory.GetCurrentDirectory(), Global.DnsCryptProxyFolder, Global.BlacklistFileName);
				Properties.Settings.Default.DomainBlacklistFile = _domainBlacklistFile;
				Properties.Settings.Default.Save();
			}

			if (!string.IsNullOrEmpty(Properties.Settings.Default.DomainWhitelistRules))
		    {
			    _domainWhitelistRuleFilePath = Properties.Settings.Default.DomainWhitelistRules;
			    Task.Run(async () =>
			    {
					await ReadWhitelistRulesFromFile();
				});
		    }
		    else
		    {
				//set default
				_domainWhitelistRuleFilePath = Path.Combine(Directory.GetCurrentDirectory(), Global.DnsCryptProxyFolder, Global.WhitelistRuleFileName);
			    Properties.Settings.Default.DomainWhitelistRules = _domainWhitelistRuleFilePath;
			    Properties.Settings.Default.Save();
			}

		    if (!string.IsNullOrEmpty(Properties.Settings.Default.DomainBlacklistRules))
		    {
			    _domainBlacklistRuleFilePath = Properties.Settings.Default.DomainBlacklistRules;
			    Task.Run(async () =>
			    {
				    await ReadBlacklistRulesFromFile();
			    });
		    }
		    else
		    {
				//set default
			    _domainBlacklistRuleFilePath = Path.Combine(Directory.GetCurrentDirectory(), Global.DnsCryptProxyFolder, Global.BlacklistRuleFileName);
			    Properties.Settings.Default.DomainBlacklistRules = _domainBlacklistRuleFilePath;
			    Properties.Settings.Default.Save();
		    }
		}

		public string DomainBlacklistFile
		{
			get => _domainBlacklistFile;
			set
			{
				if (value.Equals(_domainBlacklistFile)) return;
				_domainBlacklistFile = value;
				Properties.Settings.Default.DomainBlacklistFile = _domainBlacklistFile;
				Properties.Settings.Default.Save();
				NotifyOfPropertyChange(() => DomainBlacklistFile);
			}
		}

		public bool IsBlacklistEnabled
		{
			get => _isBlacklistEnabled;
			set
			{
				_isBlacklistEnabled = value;
				ManageDnsCryptBlacklist(DnscryptProxyConfigurationManager.DnscryptProxyConfiguration);
				NotifyOfPropertyChange(() => IsBlacklistEnabled);
			}
		}

		private async void ManageDnsCryptBlacklist(DnscryptProxyConfiguration dnscryptProxyConfiguration)
		{
			const string defaultLogFormat = "ltsv";
			try
			{
				if (_isBlacklistEnabled)
				{
					if (dnscryptProxyConfiguration == null) return;

					var saveAndRestartService = false;
					if (dnscryptProxyConfiguration.blacklist == null)
					{
						dnscryptProxyConfiguration.blacklist = new Blacklist
						{
							blacklist_file = _domainBlacklistFile,
							log_format = defaultLogFormat
						};
						saveAndRestartService = true;
					}

					if (string.IsNullOrEmpty(dnscryptProxyConfiguration.blacklist.log_format) ||
					    !dnscryptProxyConfiguration.blacklist.log_format.Equals(defaultLogFormat))
					{
						dnscryptProxyConfiguration.blacklist.log_format = defaultLogFormat;
						saveAndRestartService = true;
					}

					if (string.IsNullOrEmpty(dnscryptProxyConfiguration.blacklist.blacklist_file))
					{
						dnscryptProxyConfiguration.blacklist.blacklist_file = _domainBlacklistFile;
						saveAndRestartService = true;
					}

					if (!File.Exists(_domainBlacklistFile))
					{
						File.Create(_domainBlacklistFile).Dispose();
						await Task.Delay(50);
					}

					if (saveAndRestartService)
					{
						DnscryptProxyConfigurationManager.DnscryptProxyConfiguration = dnscryptProxyConfiguration;
						if (DnscryptProxyConfigurationManager.SaveConfiguration())
						{
							if (DnsCryptProxyManager.IsDnsCryptProxyInstalled())
							{
								if (DnsCryptProxyManager.IsDnsCryptProxyRunning())
								{
									DnsCryptProxyManager.Restart();
									await Task.Delay(Global.ServiceRestartTime).ConfigureAwait(false);
								}
								else
								{
									DnsCryptProxyManager.Start();
									await Task.Delay(Global.ServiceStartTime).ConfigureAwait(false);
								}
							}
							else
							{
								await Task.Run(() => DnsCryptProxyManager.Install()).ConfigureAwait(false);
								await Task.Delay(Global.ServiceInstallTime).ConfigureAwait(false);
								if (DnsCryptProxyManager.IsDnsCryptProxyInstalled())
								{
									DnsCryptProxyManager.Start();
									await Task.Delay(Global.ServiceStartTime).ConfigureAwait(false);
								}
							}
						}
					}
				}
				else
				{
					//disable blacklist again
					_isBlacklistEnabled = false;
					dnscryptProxyConfiguration.blacklist.blacklist_file = null;
					DnscryptProxyConfigurationManager.DnscryptProxyConfiguration = dnscryptProxyConfiguration;
					DnscryptProxyConfigurationManager.SaveConfiguration();
					if (DnsCryptProxyManager.IsDnsCryptProxyRunning())
					{
						DnsCryptProxyManager.Restart();
						await Task.Delay(Global.ServiceRestartTime).ConfigureAwait(false);
					}
				}
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}
		}

		public void ChangeBlacklistFilePath()
		{
			try
			{
				var blacklistFolderDialog = new FolderBrowserDialog
				{
					ShowNewFolderButton = true
				};
				if (!string.IsNullOrEmpty(_domainBlacklistFile))
				{
					blacklistFolderDialog.SelectedPath = Path.GetDirectoryName(_domainBlacklistFile);
				}
				var result = blacklistFolderDialog.ShowDialog();
				if (result == DialogResult.OK)
				{
					DomainBlacklistFile = Path.Combine(blacklistFolderDialog.SelectedPath, Global.BlacklistFileName);
				}
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}
		}

		#region Whitelist

		public string SelectedDomainWhitelistEntry
		{
			get => _selectedDomainWhitelistEntry;
			set
			{
				_selectedDomainWhitelistEntry = value;
				NotifyOfPropertyChange(() => SelectedDomainWhitelistEntry);
			}
		}

		public BindableCollection<string> DomainWhitelistRules
		{
			get => _domainWhitelistRules;
			set
			{
				if (value.Equals(_domainWhitelistRules)) return;
				_domainWhitelistRules = value;
				NotifyOfPropertyChange(() => DomainWhitelistRules);
			}
		}

		public string DomainWhitelistRuleFilePath
		{
			get => _domainWhitelistRuleFilePath;
			set
			{
				if (value.Equals(_domainWhitelistRuleFilePath)) return;
				_domainWhitelistRuleFilePath = value;
				Properties.Settings.Default.DomainWhitelistRules = _domainWhitelistRuleFilePath;
				Properties.Settings.Default.Save();
				SaveWhitelistRulesToFile();
				NotifyOfPropertyChange(() => DomainWhitelistRuleFilePath);
			}
		}

		public async void ImportWhitelistRules()
		{
			try
			{
				var openWhitelistFileDialog = new OpenFileDialog
				{
					Multiselect = false,
					RestoreDirectory = true
				};
				var result = openWhitelistFileDialog.ShowDialog();
				if (result == null) return;
				if (!result.Value) return;
				var whitelistLines = await DomainBlacklist.ReadAllLinesAsync(openWhitelistFileDialog.FileName);
				var parsed = DomainBlacklist.ParseBlacklist(whitelistLines, true);
				var enumerable = parsed as string[] ?? parsed.ToArray();
				if (!enumerable.Any()) return;
				DomainWhitelistRules.Clear();
				DomainWhitelistRules = new BindableCollection<string>(enumerable);
				SaveWhitelistRulesToFile();
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}
		}

		public void ExportWhitelistRules()
		{
			try
			{
				var saveWhitelistFileDialog = new SaveFileDialog
				{
					RestoreDirectory = true,
					AddExtension = true,
					DefaultExt = ".txt"
				};
				var result = saveWhitelistFileDialog.ShowDialog();
				if (result != DialogResult.OK) return;
				File.WriteAllLines(saveWhitelistFileDialog.FileName, _domainWhitelistRules);
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}
		}

		public void ChangeWhitelistRulesFilePath()
		{
			try
			{
				var whitelistFolderDialog = new FolderBrowserDialog
				{
					ShowNewFolderButton = true
				};
				if (!string.IsNullOrEmpty(_domainWhitelistRuleFilePath))
				{
					whitelistFolderDialog.SelectedPath = Path.GetDirectoryName(_domainWhitelistRuleFilePath);
				}
				var result = whitelistFolderDialog.ShowDialog();
				if (result == DialogResult.OK)
				{
					DomainWhitelistRuleFilePath = Path.Combine(whitelistFolderDialog.SelectedPath, Global.WhitelistRuleFileName);
				}
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}
		}

		private async Task ReadWhitelistRulesFromFile()
		{
			try
			{
				if (string.IsNullOrEmpty(_domainWhitelistRuleFilePath)) return;
				if (!File.Exists(_domainWhitelistRuleFilePath)) return;
				var whitelist = await DomainBlacklist.ReadAllLinesAsync(_domainWhitelistRuleFilePath);
				DomainWhitelistRules.Clear();
				DomainWhitelistRules = new BindableCollection<string>(whitelist);
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}
		}

		public void SaveWhitelistRulesToFile()
		{
			try
			{
				if (!string.IsNullOrEmpty(_domainWhitelistRuleFilePath))
				{
					File.WriteAllLines(_domainWhitelistRuleFilePath, _domainWhitelistRules, Encoding.UTF8);
				}
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}
		}

		public void RemoveWhitelistRule()
		{
			try
			{
				if (string.IsNullOrEmpty(_selectedDomainWhitelistEntry)) return;
				DomainWhitelistRules.Remove(_selectedDomainWhitelistEntry);
				SaveWhitelistRulesToFile();
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}
		}

		public async void AddWhitelistRule()
		{
			try
			{
				var dialogSettings = new MetroDialogSettings
				{
					AffirmativeButtonText = LocalizationEx.GetUiString("add", Thread.CurrentThread.CurrentCulture),
					NegativeButtonText = LocalizationEx.GetUiString("cancel", Thread.CurrentThread.CurrentCulture),
					ColorScheme = MetroDialogColorScheme.Theme
				};

				var metroWindow = Application.Current.Windows.OfType<MetroWindow>().FirstOrDefault();
				var dialogResult = await metroWindow.ShowInputAsync(LocalizationEx.GetUiString("message_title_new_whitelist_rule", Thread.CurrentThread.CurrentCulture),
					LocalizationEx.GetUiString("message_content_new_whitelist_rule", Thread.CurrentThread.CurrentCulture), dialogSettings);

				if (string.IsNullOrEmpty(dialogResult)) return;
				dialogResult = dialogResult.Replace(" ", "");
				var list = dialogResult.Split(new []{','}, StringSplitOptions.RemoveEmptyEntries);
				var parsed = DomainBlacklist.ParseBlacklist(list, true);
				var enumerable = parsed as string[] ?? parsed.ToArray();
				if (enumerable.Length <= 0) return;
				DomainWhitelistRules.AddRange(enumerable);
				SaveWhitelistRulesToFile();
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}
		}
		#endregion

		#region Blacklist
		public async Task BuildBlacklist()
		{
			var tmpFile = Path.GetTempFileName();
			try
			{
				var dialogSettings = new MetroDialogSettings
				{
					ColorScheme = MetroDialogColorScheme.Theme
				};
				var metroWindow = Application.Current.Windows.OfType<MetroWindow>().FirstOrDefault();

				var dialog = new Controls.BaseMetroDialog();
				await metroWindow.ShowMetroDialogAsync(dialog, dialogSettings);

				var blacklistRules = _domainBlacklistRules;
				var blacklistSource = new List<string>();
				var blacklistLocalRules = new List<string>();
				foreach (var blacklistRule in blacklistRules)
				{
					if (blacklistRule.StartsWith("http://") || 
					    blacklistRule.StartsWith("https://") ||
					    blacklistRule.StartsWith("file:"))
					{
						blacklistSource.Add(blacklistRule);
					}
					else
					{
						blacklistLocalRules.Add(blacklistRule);
					}
				}

				File.WriteAllLines(tmpFile, blacklistLocalRules);
				blacklistSource.Add($"file:{tmpFile}");

				var rules = await DomainBlacklist.Build(blacklistSource, new List<string>(_domainWhitelistRules));
				if (rules != null)
				{
					File.WriteAllLines(_domainBlacklistFile, rules);  
				}

				if (DnsCryptProxyManager.IsDnsCryptProxyInstalled())
				{
					if (DnsCryptProxyManager.IsDnsCryptProxyRunning())
					{
						DnsCryptProxyManager.Restart();
						await Task.Delay(Global.ServiceRestartTime);
					}
				}

				await metroWindow.HideMetroDialogAsync(dialog);
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}
			finally
			{
				File.Delete(tmpFile);
			}
		}

		public void ClearDomainBlackList()
		{
			Execute.OnUIThread(() => { DomainBlacklistRules.Clear(); });
			SaveBlacklistRulesToFile();
			BuildBlacklist();
		}

		public async void AddBlacklistRule()
		{
			try
			{
				var dialogSettings = new MetroDialogSettings
				{
					AffirmativeButtonText = LocalizationEx.GetUiString("add", Thread.CurrentThread.CurrentCulture),
					NegativeButtonText = LocalizationEx.GetUiString("cancel", Thread.CurrentThread.CurrentCulture),
					ColorScheme = MetroDialogColorScheme.Theme
				};

				var metroWindow = Application.Current.Windows.OfType<MetroWindow>().FirstOrDefault();
				var dialogResult = await metroWindow.ShowInputAsync(LocalizationEx.GetUiString("message_title_new_blacklist_rule", Thread.CurrentThread.CurrentCulture),
					LocalizationEx.GetUiString("message_content_new_blacklist_rule", Thread.CurrentThread.CurrentCulture), dialogSettings);
				if (string.IsNullOrEmpty(dialogResult)) return;
				dialogResult = dialogResult.Replace(" ", "");
				var list = dialogResult.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

				var remote = new List<string>();
				var local = new List<string>();
				foreach (var l in list)
				{
					if (l.StartsWith("http://") || l.StartsWith("https://") || l.StartsWith("file:"))
					{
						remote.Add(l);
					}
					else
					{
						local.Add(l);
					}
				}
				var parsed = DomainBlacklist.ParseBlacklist(local, true);
				var enumerable = parsed as string[] ?? parsed.ToArray();
				if (enumerable.Length > 0)
				{
					DomainBlacklistRules.AddRange(enumerable);
				}
				
				DomainBlacklistRules.AddRange(remote);
				SaveBlacklistRulesToFile();
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}
		}

		public void RemoveBlacklistRule()
		{
			try
			{
				if (string.IsNullOrEmpty(_selectedDomainBlacklistEntry)) return;
				DomainBlacklistRules.Remove(_selectedDomainBlacklistEntry);
				SaveBlacklistRulesToFile();
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}
		}

		public async void ImportBlacklistRules()
		{
			try
			{
				var openBlacklistFileDialog = new OpenFileDialog
				{
					Multiselect = false,
					RestoreDirectory = true
				};
				var result = openBlacklistFileDialog.ShowDialog();
				if (result == null) return;
				if (!result.Value) return;
				var blacklistLines = await DomainBlacklist.ReadAllLinesAsync(openBlacklistFileDialog.FileName);
				var parsed = DomainBlacklist.ParseBlacklist(blacklistLines, true);
				var enumerable = parsed as string[] ?? parsed.ToArray();
				if (!enumerable.Any()) return;
				DomainBlacklistRules.Clear();
				DomainBlacklistRules = new BindableCollection<string>(enumerable);
				SaveBlacklistRulesToFile();
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}
		}

		public void ExportBlacklistRules()
		{
			try
			{
				var saveBlacklistFileDialog = new SaveFileDialog
				{
					RestoreDirectory = true,
					AddExtension = true,
					DefaultExt = ".txt"
				};
				var result = saveBlacklistFileDialog.ShowDialog();
				if (result != DialogResult.OK) return;
				File.WriteAllLines(saveBlacklistFileDialog.FileName, _domainBlacklistRules);
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}
		}

		public string SelectedDomainBlacklistEntry
		{
			get => _selectedDomainBlacklistEntry;
			set
			{
				_selectedDomainBlacklistEntry = value;
				NotifyOfPropertyChange(() => SelectedDomainBlacklistEntry);
			}
		}

		public BindableCollection<string> DomainBlacklistRules
		{
			get => _domainBlacklistRules;
			set
			{
				if (value.Equals(_domainBlacklistRules)) return;
				_domainBlacklistRules = value;
				NotifyOfPropertyChange(() => DomainBlacklistRules);
			}
		}

		public string DomainBlacklistRuleFilePath
		{
			get => _domainBlacklistRuleFilePath;
			set
			{
				if (value.Equals(_domainBlacklistRuleFilePath)) return;
				_domainBlacklistRuleFilePath = value;
				Properties.Settings.Default.DomainBlacklistFile = _domainBlacklistRuleFilePath;
				Properties.Settings.Default.Save();
				SaveBlacklistRulesToFile();
				NotifyOfPropertyChange(() => DomainBlacklistRuleFilePath);
			}
		}

		public void SaveBlacklistRulesToFile()
		{
			try
			{
				if (!string.IsNullOrEmpty(_domainBlacklistRuleFilePath))
				{
					File.WriteAllLines(_domainBlacklistRuleFilePath, _domainBlacklistRules, Encoding.UTF8);
				}
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}
		}

		private async Task ReadBlacklistRulesFromFile()
		{
			try
			{
				if (string.IsNullOrEmpty(_domainBlacklistRuleFilePath)) return;
				if (!File.Exists(_domainBlacklistRuleFilePath)) return;
				var blacklist = await DomainBlacklist.ReadAllLinesAsync(_domainBlacklistRuleFilePath);
				DomainBlacklistRules.Clear();
				DomainBlacklistRules = new BindableCollection<string>(blacklist);
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}
		}

		public void ChangeBlacklistRulesFilePath()
		{
			try
			{
				var blacklistFolderDialog = new FolderBrowserDialog
				{
					ShowNewFolderButton = true
				};
				if (!string.IsNullOrEmpty(_domainBlacklistRuleFilePath))
				{
					blacklistFolderDialog.SelectedPath = Path.GetDirectoryName(_domainBlacklistRuleFilePath);
				}
				var result = blacklistFolderDialog.ShowDialog();
				if (result == DialogResult.OK)
				{
					DomainBlacklistRuleFilePath = Path.Combine(blacklistFolderDialog.SelectedPath, Global.BlacklistRuleFileName);
				}
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}
		}
		#endregion
	}
}
