using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using SimpleDnsCrypt.Config;
using SimpleDnsCrypt.Models;
using SimpleDnsCrypt.Tools;

namespace SimpleDnsCrypt.ViewModels
{
	[Export]
	public class LogViewModel : Screen
	{
		private readonly IWindowManager _windowManager;
		private bool _isLogging;
		private string _logFile;
		private ObservableCollection<LogLine> _logLines;
		private LogLine _selectedLogLine;

		[ImportingConstructor]
		public LogViewModel(IWindowManager windowManager)
		{
			_windowManager = windowManager;
			_logLines = new ObservableCollection<LogLine>();
			_isLogging = false;
			RefreshPluginData();
		}

		public void RefreshPluginData()
		{
			try
			{
				var found = false;
				foreach (var plugin in MainViewModel.Instance.Plugins)
				{
					if (!plugin.StartsWith(Global.LibdcpluginLogging)) continue;
					var a = plugin.Split(',');
					LogFile = a[1];
					found = true;
				}
				if (!found)
				{
					LogFile = string.Empty;
					IsLogging = false;
					LogLines.Clear();
				}
			}
			catch (Exception)
			{
				LogFile = string.Empty;
				IsLogging = false;
				LogLines.Clear();
			}
		}

		public string LogFile
		{
			get { return _logFile; }
			set
			{
				_logFile = value;
				NotifyOfPropertyChange(() => LogFile);
			}
		}

		public bool IsLogging
		{
			get { return _isLogging; }
			set
			{
				_isLogging = value;
				Log();
				NotifyOfPropertyChange(() => IsLogging);
			}
		}

		public ObservableCollection<LogLine> LogLines
		{
			get { return _logLines; }
			set
			{
				if (value.Equals(_logLines)) return;
				_logLines = value;
				NotifyOfPropertyChange(() => LogLines);
			}
		}

		public LogLine SelectedLogLine
		{
			get { return _selectedLogLine; }
			set
			{
				_selectedLogLine = value;
				NotifyOfPropertyChange(() => SelectedLogLine);
			}
		}

		private async void Log()
		{
			try
			{
				if (_isLogging)
				{
					if (!string.IsNullOrEmpty(_logFile))
					{
						if (File.Exists(_logFile))
						{
							await Task.Run(() =>
							{
								using (var reader = new StreamReader(new FileStream(_logFile,
									FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
								{
									//start at the end of the file
									var lastMaxOffset = reader.BaseStream.Length;

									while (_isLogging)
									{
										Thread.Sleep(100);
										//if the file size has not changed, idle
										if (reader.BaseStream.Length == lastMaxOffset)
											continue;

										//seek to the last max offset
										reader.BaseStream.Seek(lastMaxOffset, SeekOrigin.Begin);

										//read out of the file until the EOF
										string line;
										while ((line = reader.ReadLine()) != null)
										{
											var logLine = new LogLine(line);
											AddLogLine(logLine);
										}

										//update the last max offset
										lastMaxOffset = reader.BaseStream.Position;
									}
								}
							});
						}
						else
						{
							IsLogging = false;
							_windowManager.ShowMetroMessageBox(
								LocalizationEx.GetUiString("log_modal_no_log_file_text", Thread.CurrentThread.CurrentCulture),
								LocalizationEx.GetUiString("log_modal_no_log_file_header", Thread.CurrentThread.CurrentCulture),
								MessageBoxButton.OK, BoxType.Warning);
						}
					}
					else
					{
						IsLogging = false;
						_windowManager.ShowMetroMessageBox(
							LocalizationEx.GetUiString("log_modal_no_log_file_text", Thread.CurrentThread.CurrentCulture),
							LocalizationEx.GetUiString("log_modal_no_log_file_header", Thread.CurrentThread.CurrentCulture),
							MessageBoxButton.OK, BoxType.Warning);
					}
				}
				else
				{
					Execute.OnUIThread(() => { LogLines.Clear(); });
				}
			}
			catch (Exception)
			{
			}
		}


		private void AddLogLine(LogLine l)
		{
			Execute.OnUIThread(() => { LogLines.Add(l); });
		}

		public void Block()
		{
			try
			{
				if (SelectedLogLine == null) return;
				var newRule = SelectedLogLine.Remote.ToLower().Trim();
				var metroWindow = Application.Current.MainWindow as MetroWindow;
				var metroDialogSettings = new MetroDialogSettings
				{
					AffirmativeButtonText = LocalizationEx.GetUiString("ok", Thread.CurrentThread.CurrentCulture),
					NegativeButtonText = LocalizationEx.GetUiString("cancel", Thread.CurrentThread.CurrentCulture),
					DefaultText = newRule
				};
				var result =
					metroWindow.ShowModalInputExternal(
						LocalizationEx.GetUiString("log_modal_add_rule_header", Thread.CurrentThread.CurrentCulture),
						LocalizationEx.GetUiString("log_modal_add_rule_text", Thread.CurrentThread.CurrentCulture), metroDialogSettings);
				if (result == null) return;
				var newCustomRule = result.ToLower().Trim();
				var response = MainViewModel.Instance.BlockViewModel.ParseBlacklist(newCustomRule, true);
				if (response.Count() != 1) return;
				if (
					MainViewModel.Instance.BlockViewModel.DomainBlacklist.LocalRules.FirstOrDefault(l => l.Rule.Equals(newCustomRule)) !=
					null) return;
				MainViewModel.Instance.BlockViewModel.DomainBlacklist.LocalRules.Add(new LocaleRule {Rule = newCustomRule});
				MainViewModel.Instance.BlockViewModel.SaveDomainBlacklist();
			}
			catch (Exception)
			{
			}
		}
	}
}
