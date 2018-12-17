using Caliburn.Micro;
using DnsCrypt.Blacklist;
using MahApps.Metro.Controls;
using MahApps.Metro.SimpleChildWindow;
using SimpleDnsCrypt.Config;
using SimpleDnsCrypt.Helper;
using SimpleDnsCrypt.Models;
using SimpleDnsCrypt.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Application = System.Windows.Application;
using Screen = Caliburn.Micro.Screen;

namespace SimpleDnsCrypt.ViewModels
{
	[Export(typeof(CloakAndForwardViewModel))]
	public class CloakAndForwardViewModel : Screen
	{
		private static readonly ILog Log = LogManagerHelper.Factory();
		private readonly IWindowManager _windowManager;
		private readonly IEventAggregator _events;
		private BindableCollection<Rule> _cloakingRules;
		private int _longestCloakingKey;

		private bool _isCloakingEnabled;
		private Rule _selectedCloakingEntry;

		private string _cloakingRulesFile;

		/// <summary>
		/// Initializes a new instance of the <see cref="CloakAndForwardViewModel"/> class
		/// </summary>
		/// <param name="windowManager">The window manager</param>
		/// <param name="events">The events</param>
		[ImportingConstructor]
		public CloakAndForwardViewModel(IWindowManager windowManager, IEventAggregator events)
		{
			_windowManager = windowManager;
			_events = events;
			_events.Subscribe(this);
			_cloakingRules = new BindableCollection<Rule>();
			_longestCloakingKey = 0;

			if (!string.IsNullOrEmpty(Properties.Settings.Default.CloakingRulesFile))
			{
				_cloakingRulesFile = Properties.Settings.Default.CloakingRulesFile;
				Task.Run(async () =>
				{
					await ReadCloakingRulesFromFile();
				});
			}
			else
			{
				//set default
				_cloakingRulesFile = Path.Combine(Directory.GetCurrentDirectory(), Global.DnsCryptProxyFolder, Global.CloakingRulesFileName);
				Properties.Settings.Default.CloakingRulesFile = _cloakingRulesFile;
				Properties.Settings.Default.Save();
			}
		}

		#region Cloaking
		private async Task ReadCloakingRulesFromFile(string readFromPath = "")
		{
			try
			{
				var file = _cloakingRulesFile;
				if (!string.IsNullOrEmpty(readFromPath))
				{
					file = readFromPath;
				}
				if (string.IsNullOrEmpty(file)) return;

				if (!File.Exists(file)) return;
				var lines = await DomainBlacklist.ReadAllLinesAsync(file);
				if (lines.Length > 0)
				{
					var tmpRules = new List<Rule>();
					foreach (var line in lines)
					{
						if (line.StartsWith("#")) continue;
						var tmp = line.ToLower().Trim();
						if (string.IsNullOrEmpty(tmp)) continue;
						var lineParts = tmp.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
						if (lineParts.Length != 2) continue;
						var rule = new Rule
						{
							Key = lineParts[0].Trim(),
							Value = lineParts[1].Trim()
						};
						if (rule.Key.Length > _longestCloakingKey)
						{
							_longestCloakingKey = rule.Key.Length;
						}
						tmpRules.Add(rule);
					}
					CloakingRules.Clear();
					var orderedTmpRules = tmpRules.OrderBy(r => r.Key);
					CloakingRules = new BindableCollection<Rule>(orderedTmpRules);
				}
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}
		}

		public void SaveCloakingRulesToFile(string saveToPath = "")
		{
			try
			{
				var file = _cloakingRulesFile;
				if (!string.IsNullOrEmpty(saveToPath))
				{
					file = saveToPath;
				}
				if (string.IsNullOrEmpty(file)) return;
				const int extraSpace = 1;
				var lines = new List<string>();
				foreach (var rule in _cloakingRules)
				{
					var spaceCount = _longestCloakingKey - rule.Key.Length;
					var sb = new StringBuilder();
					sb.Append(rule.Key);
					sb.Append(' ', spaceCount + extraSpace);
					sb.Append(rule.Value);
					lines.Add(sb.ToString());
				}
				var orderedTmpRules = lines.OrderBy(r => r);
				File.WriteAllLines(file, orderedTmpRules, Encoding.UTF8);
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}
		}

		public BindableCollection<Rule> CloakingRules
		{
			get => _cloakingRules;
			set
			{
				if (value.Equals(_cloakingRules)) return;
				_cloakingRules = value;
				NotifyOfPropertyChange(() => CloakingRules);
			}
		}

		public string CloakingRulesFile
		{
			get => _cloakingRulesFile;
			set
			{
				if (value.Equals(_cloakingRulesFile)) return;
				_cloakingRulesFile = value;
				Properties.Settings.Default.CloakingRulesFile = _cloakingRulesFile;
				Properties.Settings.Default.Save();
				NotifyOfPropertyChange(() => CloakingRulesFile);
			}
		}

		public bool IsCloakingEnabled
		{
			get => _isCloakingEnabled;
			set
			{
				_isCloakingEnabled = value;
				ManageDnsCryptCloaking(DnscryptProxyConfigurationManager.DnscryptProxyConfiguration);
				NotifyOfPropertyChange(() => IsCloakingEnabled);
			}
		}

		public void ChangeCloakingRulesFile()
		{
			try
			{
				var cloakingFolderDialog = new FolderBrowserDialog
				{
					ShowNewFolderButton = true
				};
				if (!string.IsNullOrEmpty(_cloakingRulesFile))
				{
					cloakingFolderDialog.SelectedPath = Path.GetDirectoryName(_cloakingRulesFile);
				}
				var result = cloakingFolderDialog.ShowDialog();
				if (result == DialogResult.OK)
				{
					CloakingRulesFile = Path.Combine(cloakingFolderDialog.SelectedPath, Global.CloakingRulesFileName);
				}
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}
		}

		public Rule SelectedCloakingEntry
		{
			get => _selectedCloakingEntry;
			set
			{
				_selectedCloakingEntry = value;
				NotifyOfPropertyChange(() => SelectedCloakingEntry);
			}
		}

		public void RemoveCloakingRule()
		{
			try
			{
				if (_selectedCloakingEntry == null) return;
				CloakingRules.Remove(_selectedCloakingEntry);
				SaveCloakingRulesToFile();
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}
		}

		public async void AddCloakingRule()
		{
			try
			{
				var metroWindow = Application.Current.Windows.OfType<MetroWindow>().FirstOrDefault();
				var addRuleWindow = new AddRuleWindow(RuleWindowType.Cloaking);
				var addRuleWindowResult = await metroWindow.ShowChildWindowAsync<AddRuleWindowResult>(addRuleWindow);

				if (!addRuleWindowResult.Result) return;
				if (string.IsNullOrEmpty(addRuleWindowResult.RuleKey) ||
				    string.IsNullOrEmpty(addRuleWindowResult.RuleValue)) return;
				var tmp = new Rule
				{
					Key = addRuleWindowResult.RuleKey,
					Value = addRuleWindowResult.RuleValue
				};
				_cloakingRules.Add(tmp);
				CloakingRules.Clear();
				var orderedTmpRules = _cloakingRules.OrderBy(r => r.Key);
				CloakingRules = new BindableCollection<Rule>(orderedTmpRules);
				SaveCloakingRulesToFile();
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}
		}

		public void ExportCloakingRules()
		{
			try
			{
				var saveCloakingFileDialog = new SaveFileDialog
				{
					RestoreDirectory = true,
					AddExtension = true,
					DefaultExt = ".txt"
				};
				var result = saveCloakingFileDialog.ShowDialog();
				if (result != DialogResult.OK) return;
				SaveCloakingRulesToFile(saveCloakingFileDialog.FileName);
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}
		}

		public async void ImportCloakingRules()
		{
			try
			{
				var openCloakingFileDialog = new OpenFileDialog
				{
					Multiselect = false,
					RestoreDirectory = true
				};
				var result = openCloakingFileDialog.ShowDialog();
				if (result != DialogResult.OK) return;
				await ReadCloakingRulesFromFile(openCloakingFileDialog.FileName);
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}
		}


		private async void ManageDnsCryptCloaking(DnscryptProxyConfiguration dnscryptProxyConfiguration)
		{
			try
			{
				if (_isCloakingEnabled)
				{
					if (dnscryptProxyConfiguration == null) return;

					var saveAndRestartService = false;

					if (dnscryptProxyConfiguration.cloaking_rules == null)
					{
						dnscryptProxyConfiguration.cloaking_rules = _cloakingRulesFile;
						saveAndRestartService = true;
					}

					if (!File.Exists(_cloakingRulesFile))
					{
						File.Create(_cloakingRulesFile).Dispose();
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
					//disable cloaking again
					_isCloakingEnabled = false;
					dnscryptProxyConfiguration.cloaking_rules = null;
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
	#endregion
	}
}