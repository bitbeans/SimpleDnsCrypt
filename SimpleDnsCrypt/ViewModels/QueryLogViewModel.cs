using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Caliburn.Micro;
using DnsCrypt.Blacklist;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using SimpleDnsCrypt.Config;
using SimpleDnsCrypt.Helper;
using SimpleDnsCrypt.Models;
using Application = System.Windows.Application;
using Screen = Caliburn.Micro.Screen;

namespace SimpleDnsCrypt.ViewModels
{
	[Export(typeof(QueryLogViewModel))]
	public class QueryLogViewModel : Screen
	{
		private static readonly ILog Log = LogManagerHelper.Factory();
		private readonly IWindowManager _windowManager;
		private readonly IEventAggregator _events;

		private ObservableCollection<QueryLogLine> _queryLogLines;
		private string _queryLogFile;
		private bool _isQueryLogLogging;
		private QueryLogLine _selectedQueryLogLine;

		[ImportingConstructor]
		public QueryLogViewModel(IWindowManager windowManager, IEventAggregator events)
		{
			_windowManager = windowManager;
			_events = events;
			_events.Subscribe(this);
			_isQueryLogLogging = false;
			_queryLogLines = new ObservableCollection<QueryLogLine>();

			if (!string.IsNullOrEmpty(Properties.Settings.Default.QueryLogFile))
			{
				_queryLogFile = Properties.Settings.Default.QueryLogFile;
			}
			else
			{
				//set default
				_queryLogFile = Path.Combine(Directory.GetCurrentDirectory(), Global.DnsCryptProxyFolder, Global.QueryLogFileName);
				Properties.Settings.Default.QueryLogFile = _queryLogFile;
				Properties.Settings.Default.Save();
			}
		}

		private void AddLogLine(QueryLogLine queryLogLine)
		{
			Execute.OnUIThread(() =>
			{
				QueryLogLines.Add(queryLogLine);
			});
		}

		public async void BlockQueryLogEntry()
		{
			try
			{
				if (_selectedQueryLogLine == null) return;
				if (MainViewModel.Instance.DomainBlacklistViewModel == null) return;
				var dialogSettings = new MetroDialogSettings
				{
					DefaultText = _selectedQueryLogLine.Remote.ToLower(),
					AffirmativeButtonText = LocalizationEx.GetUiString("add", Thread.CurrentThread.CurrentCulture),
					NegativeButtonText = LocalizationEx.GetUiString("cancel", Thread.CurrentThread.CurrentCulture),
					ColorScheme = MetroDialogColorScheme.Theme
				};

				var metroWindow = Application.Current.Windows.OfType<MetroWindow>().FirstOrDefault();
				//TODO: translate
				var dialogResult = await metroWindow.ShowInputAsync(LocalizationEx.GetUiString("message_title_new_blacklist_rule", Thread.CurrentThread.CurrentCulture),
					"Rule:", dialogSettings);

				if (string.IsNullOrEmpty(dialogResult)) return;
				var newCustomRule = dialogResult.ToLower().Trim();
				var parsed = DomainBlacklist.ParseBlacklist(newCustomRule, true);
				var enumerable = parsed as string[] ?? parsed.ToArray();
				if (enumerable.Length != 1) return;
				MainViewModel.Instance.DomainBlacklistViewModel.DomainBlacklistRules.Add(enumerable[0]);
				MainViewModel.Instance.DomainBlacklistViewModel.SaveBlacklistRulesToFile();
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}
		}

		public void ClearQueryLog()
		{
			Execute.OnUIThread(() => { QueryLogLines.Clear(); });
		}

		public ObservableCollection<QueryLogLine> QueryLogLines
		{
			get => _queryLogLines;
			set
			{
				if (value.Equals(_queryLogLines)) return;
				_queryLogLines = value;
				NotifyOfPropertyChange(() => QueryLogLines);
			}
		}

		public string QueryLogFile
		{
			get => _queryLogFile;
			set
			{
				if (value.Equals(_queryLogFile)) return;
				_queryLogFile = value;
				NotifyOfPropertyChange(() => QueryLogFile);
			}
		}

		public QueryLogLine SelectedQueryLogLine
		{
			get => _selectedQueryLogLine;
			set
			{
				_selectedQueryLogLine = value;
				NotifyOfPropertyChange(() => SelectedQueryLogLine);
			}
		}

		public bool IsQueryLogLogging
		{
			get => _isQueryLogLogging;
			set
			{
				_isQueryLogLogging = value;
				QueryLog(DnscryptProxyConfigurationManager.DnscryptProxyConfiguration);
				NotifyOfPropertyChange(() => IsQueryLogLogging);
			}
		}

		public void ChangeQueryLogFilePath()
		{
			try
			{
				var queryLogFolderDialog = new FolderBrowserDialog
				{
					ShowNewFolderButton = true
				};
				if (!string.IsNullOrEmpty(_queryLogFile))
				{
					queryLogFolderDialog.SelectedPath = Path.GetDirectoryName(_queryLogFile);
				}
				var result = queryLogFolderDialog.ShowDialog();
				if (result == DialogResult.OK)
				{
					QueryLogFile = Path.Combine(queryLogFolderDialog.SelectedPath, Global.QueryLogFileName);
					Properties.Settings.Default.QueryLogFile = _queryLogFile;
					Properties.Settings.Default.Save();
				}
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}
		}

		private async void QueryLog(DnscryptProxyConfiguration dnscryptProxyConfiguration)
		{
			const string defaultLogFormat = "ltsv";
			try
			{
				if (_isQueryLogLogging)
				{
					if (dnscryptProxyConfiguration == null) return;

					var saveAndRestartService = false;
					if (dnscryptProxyConfiguration.query_log == null)
					{
						dnscryptProxyConfiguration.query_log = new QueryLog
						{
							file = _queryLogFile,
							format = defaultLogFormat
						};
						saveAndRestartService = true;
					}

					if (string.IsNullOrEmpty(dnscryptProxyConfiguration.query_log.format) ||
					    !dnscryptProxyConfiguration.query_log.format.Equals(defaultLogFormat))
					{
						dnscryptProxyConfiguration.query_log.format = defaultLogFormat;
						saveAndRestartService = true;
					}

					if (string.IsNullOrEmpty(dnscryptProxyConfiguration.query_log.file))
					{
						dnscryptProxyConfiguration.query_log.file = _queryLogFile;
						saveAndRestartService = true;
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

					if (!string.IsNullOrEmpty(_queryLogFile))
						if (File.Exists(_queryLogFile))
						{
							await Task.Run(() =>
							{
								using (var reader = new StreamReader(new FileStream(_queryLogFile,
									FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
								{
									//start at the end of the file
									var lastMaxOffset = reader.BaseStream.Length;

									while (_isQueryLogLogging)
									{
										Thread.Sleep(500);
										//if the file size has not changed, idle
										if (reader.BaseStream.Length == lastMaxOffset)
											continue;

										//seek to the last max offset
										reader.BaseStream.Seek(lastMaxOffset, SeekOrigin.Begin);

										//read out of the file until the EOF
										string line;
										while ((line = reader.ReadLine()) != null)
										{
											var queryLogLine = new QueryLogLine(line);
											AddLogLine(queryLogLine);
										}

										//update the last max offset
										lastMaxOffset = reader.BaseStream.Position;
									}
								}
							}).ConfigureAwait(false);
						}
						else
						{
							Log.Warn($"Missing {_queryLogFile}");
							_isQueryLogLogging = false;
						}
					else
					{
						_isQueryLogLogging = false;
					}
				}
				else
				{
					//disable query log again
					_isQueryLogLogging = false;
					if (DnsCryptProxyManager.IsDnsCryptProxyRunning())
					{
						dnscryptProxyConfiguration.query_log.file = null;
						DnscryptProxyConfigurationManager.DnscryptProxyConfiguration = dnscryptProxyConfiguration;
						if (DnscryptProxyConfigurationManager.SaveConfiguration())
						{
							DnsCryptProxyManager.Restart();
							await Task.Delay(Global.ServiceRestartTime).ConfigureAwait(false);
						}
					}
					Execute.OnUIThread(() => { QueryLogLines.Clear(); });
				}
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}
		}
	}
}