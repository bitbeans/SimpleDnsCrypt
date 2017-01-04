using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using SimpleDnsCrypt.Config;
using SimpleDnsCrypt.Models;

namespace SimpleDnsCrypt.ViewModels
{
	[Export]
	public class LogViewModel : Screen
	{
		private bool _isLogging;
		private string _logFile;
		private ObservableCollection<LogLine> _logLines;
		private LogLine _selectedLogLine;

		[ImportingConstructor]
		public LogViewModel()
		{
			_logLines = new ObservableCollection<LogLine>();
			_isLogging = false;

			foreach (var plugin in MainViewModel.Instance.Plugins)
			{
				if (!plugin.StartsWith(Global.LibdcpluginLogging)) continue;
				var a = plugin.Split(',');
				_logFile = a[1];
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
						}
					}
					else
					{
						IsLogging = false;
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
				if (SelectedLogLine != null)
				{
					var newRule = SelectedLogLine.Remote.ToLower().Trim();
					var response = MainViewModel.Instance.BlockViewModel.ParseBlacklist(newRule, true);
					if (response.Count() != 1) return;
					if (MainViewModel.Instance.BlockViewModel.DomainBlacklist.LocalRules.FirstOrDefault(l => l.Rule.Equals(newRule)) !=
						null) return;
					MainViewModel.Instance.BlockViewModel.DomainBlacklist.LocalRules.Add(new LocaleRule { Rule = newRule });
					MainViewModel.Instance.BlockViewModel.SaveDomainBlacklist();
				}
			}
			catch (Exception)
			{

			}
		}
	}
}
