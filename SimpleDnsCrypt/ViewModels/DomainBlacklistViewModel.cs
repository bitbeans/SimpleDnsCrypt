using Caliburn.Micro;
using SimpleDnsCrypt.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DnsCrypt.Blacklist;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using SimpleDnsCrypt.Helper;
using SimpleDnsCrypt.Models;
using Application = System.Windows.Application;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using Screen = Caliburn.Micro.Screen;


namespace SimpleDnsCrypt.ViewModels
{
	[Export(typeof(DomainBlacklistViewModel))]
	public class DomainBlacklistViewModel : Screen
	{
		private const string WhitelistFileName = "domain-whitelist.txt";
		private const string BlacklistFileName = "domain-blacklist.txt";

		private static readonly ILog Log = LogManagerHelper.Factory();
		private readonly IWindowManager _windowManager;
	    private readonly IEventAggregator _events;

		private BindableCollection<string> _domainBlacklist;
		private BindableCollection<string> _domainWhitelist;
		
		private string _selectedDomainBlacklistEntry;
		private string _selectedDomainWhitelistEntry;
		private string _domainWhitelistFilePath;

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
		    _domainBlacklist = new BindableCollection<string>();
		    _domainWhitelist = new BindableCollection<string>();

		    if (!string.IsNullOrEmpty(Properties.Settings.Default.DomainWhitelist))
		    {
			    _domainWhitelistFilePath = Properties.Settings.Default.DomainWhitelist;
			    Task.Run(async () =>
			    {
					await ReadWhitelistFromFile();
				});
		    }
		    else
		    {
				//set default
				_domainWhitelistFilePath = Path.Combine(Directory.GetCurrentDirectory(), Global.DnsCryptProxyFolder, WhitelistFileName);
			    Properties.Settings.Default.DomainWhitelist = _domainWhitelistFilePath;
			    Properties.Settings.Default.Save();
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

		public BindableCollection<string> DomainWhitelist
		{
			get => _domainWhitelist;
			set
			{
				if (value.Equals(_domainWhitelist)) return;
				_domainWhitelist = value;
				NotifyOfPropertyChange(() => DomainWhitelist);
			}
		}

		public string DomainWhitelistFilePath
		{
			get => _domainWhitelistFilePath;
			set
			{
				if (value.Equals(_domainWhitelistFilePath)) return;
				_domainWhitelistFilePath = value;
				Properties.Settings.Default.DomainWhitelist = _domainWhitelistFilePath;
				Properties.Settings.Default.Save();
				SaveWhitelistToFile();
				NotifyOfPropertyChange(() => DomainWhitelistFilePath);
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
				var whitelistLines = await AddressBlacklist.ReadAllLinesAsync(openWhitelistFileDialog.FileName);
				var parsed = AddressBlacklist.ParseBlacklist(whitelistLines, true);
				var enumerable = parsed as string[] ?? parsed.ToArray();
				if (!enumerable.Any()) return;
				DomainWhitelist.Clear();
				DomainWhitelist = new BindableCollection<string>(enumerable);
				SaveWhitelistToFile();
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
				File.WriteAllLines(saveWhitelistFileDialog.FileName, _domainWhitelist);
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}
		}

		public void ChangeWhitelistFilePath()
		{
			try
			{
				var whitelistFolderDialog = new FolderBrowserDialog
				{
					ShowNewFolderButton = true
				};
				if (!string.IsNullOrEmpty(_domainWhitelistFilePath))
				{
					whitelistFolderDialog.SelectedPath = Path.GetDirectoryName(_domainWhitelistFilePath);
				}
				var result = whitelistFolderDialog.ShowDialog();
				if (result == DialogResult.OK)
				{
					DomainWhitelistFilePath = Path.Combine(whitelistFolderDialog.SelectedPath, WhitelistFileName);
				}
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}
		}

		private async Task ReadWhitelistFromFile()
		{
			try
			{
				if (string.IsNullOrEmpty(_domainWhitelistFilePath)) return;
				var whitelist = await AddressBlacklist.ReadAllLinesAsync(_domainWhitelistFilePath);
				DomainWhitelist.Clear();
				DomainWhitelist = new BindableCollection<string>(whitelist);
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}
		}

		public void SaveWhitelistToFile()
		{
			try
			{
				if (!string.IsNullOrEmpty(_domainWhitelistFilePath))
				{
					File.WriteAllLines(_domainWhitelistFilePath, _domainWhitelist, Encoding.UTF8);
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
				DomainWhitelist.Remove(_selectedDomainWhitelistEntry);
				SaveWhitelistToFile();
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
					AffirmativeButtonText = "Add",
					NegativeButtonText = "Cancel",
					ColorScheme = MetroDialogColorScheme.Theme
				};

				var metroWindow = Application.Current.Windows.OfType<MetroWindow>().FirstOrDefault();
				var dialogResult = await metroWindow.ShowInputAsync("New domain whitelist rule", "Example: *.example.com, ads.*, *sex*, you also can add a list (seperated by: ,)", dialogSettings);

				if (string.IsNullOrEmpty(dialogResult)) return;
				dialogResult = dialogResult.Replace(" ", "");
				var list = dialogResult.Split(new []{','}, StringSplitOptions.RemoveEmptyEntries);
				var parsed = AddressBlacklist.ParseBlacklist(list, true);
				var enumerable = parsed as string[] ?? parsed.ToArray();
				if (enumerable.Length <= 0) return;
				DomainWhitelist.AddRange(enumerable);
				SaveWhitelistToFile();
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}
		}

		#endregion

		#region Blacklist


		#endregion






		public string SelectedDomainBlacklistEntry
		{
			get => _selectedDomainBlacklistEntry;
			set
			{
				_selectedDomainBlacklistEntry = value;
				NotifyOfPropertyChange(() => SelectedDomainBlacklistEntry);
			}
		}

		public BindableCollection<string> DomainBlacklist
		{
			get => _domainBlacklist;
			set
			{
				if (value.Equals(_domainBlacklist)) return;
				_domainBlacklist = value;
				NotifyOfPropertyChange(() => DomainBlacklist);
			}
		}





	}
}
