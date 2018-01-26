using Caliburn.Micro;
using SimpleDnsCrypt.Config;
using SimpleDnsCrypt.Helper;
using SimpleDnsCrypt.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleDnsCrypt.ViewModels
{
	[Export(typeof(DomainBlockLogViewModel))]
	public class DomainBlockLogViewModel : Screen
	{
		private readonly IWindowManager _windowManager;
		private readonly IEventAggregator _events;

		private ObservableCollection<DomainBlockLogLine> _domainBlockLogLines;
		private string _domainBlockLogFile;
		private bool _isDomainBlockLogLogging;
		private DomainBlockLogLine _selectedDomainBlockLogLine;

		[ImportingConstructor]
		public DomainBlockLogViewModel(IWindowManager windowManager, IEventAggregator events)
		{
			_windowManager = windowManager;
			_events = events;
			_events.Subscribe(this);
			_isDomainBlockLogLogging = false;
			_domainBlockLogLines = new ObservableCollection<DomainBlockLogLine>();
		}

		private void AddLogLine(DomainBlockLogLine domainBlockLogLine)
		{
			Execute.OnUIThread(() =>
			{
				DomainBlockLogLines.Add(domainBlockLogLine);
			});
		}

		public void ClearDomainBlockLog()
		{
			Execute.OnUIThread(() => { DomainBlockLogLines.Clear(); });
		}

		public ObservableCollection<DomainBlockLogLine> DomainBlockLogLines
		{
			get => _domainBlockLogLines;
			set
			{
				if (value.Equals(_domainBlockLogLines)) return;
				_domainBlockLogLines = value;
				NotifyOfPropertyChange(() => DomainBlockLogLines);
			}
		}

		public string DomainBlockLogFile
		{
			get => _domainBlockLogFile;
			set
			{
				if (value.Equals(_domainBlockLogFile)) return;
				_domainBlockLogFile = value;
				NotifyOfPropertyChange(() => DomainBlockLogFile);
			}
		}

		public DomainBlockLogLine SelectedDomainBlockLogLine
		{
			get => _selectedDomainBlockLogLine;
			set
			{
				_selectedDomainBlockLogLine = value;
				NotifyOfPropertyChange(() => SelectedDomainBlockLogLine);
			}
		}

		public bool IsDomainBlockLogLogging
		{
			get => _isDomainBlockLogLogging;
			set
			{
				_isDomainBlockLogLogging = value;
				DomainBlockLog(DnscryptProxyConfigurationManager.DnscryptProxyConfiguration);
				NotifyOfPropertyChange(() => IsDomainBlockLogLogging);
			}
		}

		private async void DomainBlockLog(DnscryptProxyConfiguration dnscryptProxyConfiguration)
		{
			const string defaultLogFileName = "blocked.log";
			const string defaultLogFormat = "ltsv";
			try
			{
				if (_isDomainBlockLogLogging)
				{
					if (dnscryptProxyConfiguration == null) return;

					var saveAndRestartService = false;
					if (dnscryptProxyConfiguration.blacklist == null)
					{
						dnscryptProxyConfiguration.blacklist = new Blacklist
						{
							blacklist_file = defaultLogFileName,
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

					if (string.IsNullOrEmpty(dnscryptProxyConfiguration.blacklist.blacklist_file) ||
						!dnscryptProxyConfiguration.blacklist.blacklist_file.Equals(defaultLogFileName))
					{
						dnscryptProxyConfiguration.blacklist.blacklist_file = defaultLogFileName;
						saveAndRestartService = true;
					}

					if (saveAndRestartService)
					{
						DnscryptProxyConfigurationManager.DnscryptProxyConfiguration = dnscryptProxyConfiguration;
						if (DnscryptProxyConfigurationManager.SaveConfiguration())
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
								DnsCryptProxyManager.Install();
								DnsCryptProxyManager.Start();
								await Task.Delay(Global.ServiceStartTime).ConfigureAwait(false);
							}
					}

					DomainBlockLogFile = Path.Combine(Directory.GetCurrentDirectory(), Global.DnsCryptProxyFolder,
						dnscryptProxyConfiguration.blacklist.blacklist_file);

					if (!string.IsNullOrEmpty(_domainBlockLogFile))
						if (File.Exists(_domainBlockLogFile))
							await Task.Run(() =>
							{
								using (var reader = new StreamReader(new FileStream(_domainBlockLogFile,
									FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
								{
									//start at the end of the file
									var lastMaxOffset = reader.BaseStream.Length;

									while (_isDomainBlockLogLogging)
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
											var blockLogLine = new DomainBlockLogLine(line);
											AddLogLine(blockLogLine);
										}

										//update the last max offset
										lastMaxOffset = reader.BaseStream.Position;
									}
								}
							}).ConfigureAwait(false);
						else
							IsDomainBlockLogLogging = false;
					else
						IsDomainBlockLogLogging = false;
				}
				else
				{
					Execute.OnUIThread(() => { DomainBlockLogLines.Clear(); });
				}
			}
			catch (Exception)
			{
			}
		}
	}
}
