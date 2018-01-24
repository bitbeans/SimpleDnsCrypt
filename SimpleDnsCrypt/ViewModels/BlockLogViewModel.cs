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
	[Export(typeof(BlockLogViewModel))]
	public class BlockLogViewModel : Screen
	{
		private readonly IWindowManager _windowManager;
		private readonly IEventAggregator _events;

		private ObservableCollection<BlockLogLine> _blockLogLines;
		private string _blockLogFile;
		private bool _isBlockLogLogging;
		private BlockLogLine _selectedBlockLogLine;

		[ImportingConstructor]
		public BlockLogViewModel(IWindowManager windowManager, IEventAggregator events)
		{
			_windowManager = windowManager;
			_events = events;
			_events.Subscribe(this);
			_isBlockLogLogging = false;
			_blockLogLines = new ObservableCollection<BlockLogLine>();
		}

		private void AddLogLine(BlockLogLine blockLogLine)
		{
			Execute.OnUIThread(() =>
			{
				BlockLogLines.Add(blockLogLine);
			});
		}

		public void ClearBlockLog()
		{
			Execute.OnUIThread(() => { BlockLogLines.Clear(); });
		}

		public ObservableCollection<BlockLogLine> BlockLogLines
		{
			get => _blockLogLines;
			set
			{
				if (value.Equals(_blockLogLines)) return;
				_blockLogLines = value;
				NotifyOfPropertyChange(() => BlockLogLines);
			}
		}

		public string BlockLogFile
		{
			get => _blockLogFile;
			set
			{
				if (value.Equals(_blockLogFile)) return;
				_blockLogFile = value;
				NotifyOfPropertyChange(() => BlockLogFile);
			}
		}

		public BlockLogLine SelectedBlockLogLine
		{
			get => _selectedBlockLogLine;
			set
			{
				_selectedBlockLogLine = value;
				NotifyOfPropertyChange(() => SelectedBlockLogLine);
			}
		}

		public bool IsBlockLogLogging
		{
			get => _isBlockLogLogging;
			set
			{
				_isBlockLogLogging = value;
				BlockLog(DnscryptProxyConfigurationManager.DnscryptProxyConfiguration);
				NotifyOfPropertyChange(() => IsBlockLogLogging);
			}
		}

		private async void BlockLog(DnscryptProxyConfiguration dnscryptProxyConfiguration)
		{
			const string defaultLogFileName = "blocked.log";
			const string defaultLogFormat = "ltsv";
			try
			{
				if (_isBlockLogLogging)
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

					BlockLogFile = Path.Combine(Directory.GetCurrentDirectory(), Global.DnsCryptProxyFolder,
						dnscryptProxyConfiguration.blacklist.blacklist_file);

					if (!string.IsNullOrEmpty(_blockLogFile))
						if (File.Exists(_blockLogFile))
							await Task.Run(() =>
							{
								using (var reader = new StreamReader(new FileStream(_blockLogFile,
									FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
								{
									//start at the end of the file
									var lastMaxOffset = reader.BaseStream.Length;

									while (_isBlockLogLogging)
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
											var blockLogLine = new BlockLogLine(line);
											AddLogLine(blockLogLine);
										}

										//update the last max offset
										lastMaxOffset = reader.BaseStream.Position;
									}
								}
							}).ConfigureAwait(false);
						else
							IsBlockLogLogging = false;
					else
						IsBlockLogLogging = false;
				}
				else
				{
					Execute.OnUIThread(() => { BlockLogLines.Clear(); });
				}
			}
			catch (Exception)
			{
			}
		}
	}
}
