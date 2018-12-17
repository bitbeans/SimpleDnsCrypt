using Caliburn.Micro;
using SimpleDnsCrypt.Models;
using System.ComponentModel.Composition;

namespace SimpleDnsCrypt.ViewModels
{
	[Export(typeof(SettingsViewModel))]
	public class SettingsViewModel : Screen
	{
		private readonly IWindowManager _windowManager;
		private readonly IEventAggregator _events;
		private string _windowTitle;
		private bool _isAdvancedSettingsTabVisible;
		private bool _isStartInTrayEnabled;
		private bool _isTrayModeEnabled;
		private bool _isQueryLogTabVisible;
		private bool _isDomainBlacklistTabVisible;
		private bool _isDomainBlockLogTabVisible;
		private bool _isAddressBlacklistTabVisible;
		private bool _isAddressBlockLogTabVisible;
		private bool _isCloakAndForwardTabVisible;
		private bool _isAutoUpdateEnabled;
		private bool _isAutoUpdateSilentEnabled;
		private bool _backupAndRestoreConfigOnUpdate;

		private UpdateType _selectedUpdateType;

		public SettingsViewModel()
		{
		}

		[ImportingConstructor]
		public SettingsViewModel(IWindowManager windowManager, IEventAggregator events)
		{
			_windowManager = windowManager;
			_events = events;
			_events.Subscribe(this);
			_isAdvancedSettingsTabVisible = Properties.Settings.Default.IsAdvancedSettingsTabVisible;
			_isStartInTrayEnabled = Properties.Settings.Default.StartInTray;
			_isTrayModeEnabled = Properties.Settings.Default.TrayMode;
			_isQueryLogTabVisible = Properties.Settings.Default.IsQueryLogTabVisible;
			_isDomainBlacklistTabVisible = Properties.Settings.Default.IsDomainBlacklistTabVisible;
			_isDomainBlockLogTabVisible = Properties.Settings.Default.IsDomainBlockLogTabVisible;
			_isAddressBlacklistTabVisible = Properties.Settings.Default.IsAddressBlacklistTabVisible;
			_isAddressBlockLogTabVisible = Properties.Settings.Default.IsAddressBlockLogTabVisible;
			_isCloakAndForwardTabVisible = Properties.Settings.Default.IsCloakAndForwardTabVisible;
			_isAutoUpdateEnabled = Properties.Settings.Default.AutoUpdate;
			_isAutoUpdateSilentEnabled = Properties.Settings.Default.AutoUpdateSilent;
			_selectedUpdateType = (UpdateType)Properties.Settings.Default.MinUpdateType;
			_backupAndRestoreConfigOnUpdate = Properties.Settings.Default.BackupAndRestoreConfigOnUpdate;
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

		public UpdateType SelectedUpdateType
		{
			get => _selectedUpdateType;
			set
			{
				_selectedUpdateType = value;
				Properties.Settings.Default.MinUpdateType = (int)_selectedUpdateType;
				NotifyOfPropertyChange(() => SelectedUpdateType);
			}
		}

		public bool IsCloakAndForwardTabVisible
		{
			get => _isCloakAndForwardTabVisible;
			set
			{
				_isCloakAndForwardTabVisible = value;
				Properties.Settings.Default.IsCloakAndForwardTabVisible = _isCloakAndForwardTabVisible;
				NotifyOfPropertyChange(() => IsCloakAndForwardTabVisible);
			}
		}

		public bool IsAdvancedSettingsTabVisible
		{
			get => _isAdvancedSettingsTabVisible;
			set
			{
				_isAdvancedSettingsTabVisible = value;
				Properties.Settings.Default.IsAdvancedSettingsTabVisible = _isAdvancedSettingsTabVisible;
				NotifyOfPropertyChange(() => IsAdvancedSettingsTabVisible);
			}
		}

		public bool IsStartInTrayEnabled
		{
			get => _isStartInTrayEnabled;
			set
			{
				_isStartInTrayEnabled = value;
				Properties.Settings.Default.StartInTray = _isStartInTrayEnabled;
				NotifyOfPropertyChange(() => IsStartInTrayEnabled);
			}
		}
		public bool IsTrayModeEnabled
		{
			get => _isTrayModeEnabled;
			set
			{
				_isTrayModeEnabled = value;
				Properties.Settings.Default.TrayMode = _isTrayModeEnabled;
				NotifyOfPropertyChange(() => IsTrayModeEnabled);
				if (!IsTrayModeEnabled && IsStartInTrayEnabled) IsStartInTrayEnabled = false;
			}
		}

		public bool IsQueryLogTabVisible
		{
			get => _isQueryLogTabVisible;
			set
			{
				_isQueryLogTabVisible = value;
				Properties.Settings.Default.IsQueryLogTabVisible = _isQueryLogTabVisible;
				NotifyOfPropertyChange(() => IsQueryLogTabVisible);
			}
		}

		public bool IsDomainBlockLogTabVisible
		{
			get => _isDomainBlockLogTabVisible;
			set
			{
				_isDomainBlockLogTabVisible = value;
				Properties.Settings.Default.IsDomainBlockLogTabVisible = _isDomainBlockLogTabVisible;
				NotifyOfPropertyChange(() => IsDomainBlockLogTabVisible);
			}
		}

		public bool IsDomainBlacklistTabVisible
		{
			get => _isDomainBlacklistTabVisible;
			set
			{
				_isDomainBlacklistTabVisible = value;
				Properties.Settings.Default.IsDomainBlacklistTabVisible = _isDomainBlacklistTabVisible;
				NotifyOfPropertyChange(() => IsDomainBlacklistTabVisible);
			}
		}

		public bool IsAddressBlockLogTabVisible
		{
			get => _isAddressBlockLogTabVisible;
			set
			{
				_isAddressBlockLogTabVisible = value;
				Properties.Settings.Default.IsAddressBlockLogTabVisible = _isAddressBlockLogTabVisible;
				NotifyOfPropertyChange(() => IsAddressBlockLogTabVisible);
			}
		}

		public bool IsAddressBlacklistTabVisible
		{
			get => _isAddressBlacklistTabVisible;
			set
			{
				_isAddressBlacklistTabVisible = value;
				Properties.Settings.Default.IsAddressBlacklistTabVisible = _isAddressBlacklistTabVisible;
				NotifyOfPropertyChange(() => IsAddressBlacklistTabVisible);
			}
		}

		public bool IsAutoUpdateEnabled
		{
			get => _isAutoUpdateEnabled;
			set
			{
				_isAutoUpdateEnabled = value;
				Properties.Settings.Default.AutoUpdate = _isAutoUpdateEnabled;
				NotifyOfPropertyChange(() => IsAutoUpdateEnabled);
			}
		}

		public bool IsAutoUpdateSilentEnabled
		{
			get => _isAutoUpdateSilentEnabled;
			set
			{
				_isAutoUpdateSilentEnabled = value;
				Properties.Settings.Default.AutoUpdateSilent = _isAutoUpdateSilentEnabled;
				NotifyOfPropertyChange(() => IsAutoUpdateSilentEnabled);
			}
		}

		public bool BackupAndRestoreConfigOnUpdate
		{
			get => _backupAndRestoreConfigOnUpdate;
			set
			{
				_backupAndRestoreConfigOnUpdate = value;
				Properties.Settings.Default.BackupAndRestoreConfigOnUpdate = _backupAndRestoreConfigOnUpdate;
				NotifyOfPropertyChange(() => BackupAndRestoreConfigOnUpdate);
			}
		}
	}
}