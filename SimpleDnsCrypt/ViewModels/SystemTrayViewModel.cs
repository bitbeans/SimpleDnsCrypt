using Caliburn.Micro;
using System.Windows;

namespace SimpleDnsCrypt.ViewModels
{
	public class SystemTrayViewModel : Screen
	{
		private readonly IWindowManager _windowManager;
		private readonly MainViewModel _mainViewModel;
		private readonly IEventAggregator _events;

		public SystemTrayViewModel(IWindowManager windowManager, IEventAggregator events, MainViewModel mainViewModel)
		{
			_windowManager = windowManager;
			_events = events;
			_mainViewModel = mainViewModel;
		}

		protected override void OnActivate()
		{
			base.OnActivate();

			NotifyOfPropertyChange(() => CanShowWindow);
			NotifyOfPropertyChange(() => CanHideWindow);
		}

		public void ShowWindow()
		{
			if (!_mainViewModel.IsActive)
			{
				_windowManager.ShowWindow(_mainViewModel);
			}
			NotifyOfPropertyChange(() => CanShowWindow);
			NotifyOfPropertyChange(() => CanHideWindow);
		}

		public bool CanShowWindow => !_mainViewModel.IsActive;

		public void HideWindow()
		{
			_mainViewModel.TryClose();

			NotifyOfPropertyChange(() => CanShowWindow);
			NotifyOfPropertyChange(() => CanHideWindow);
		}

		public bool CanHideWindow => _mainViewModel.IsActive;

		public void ExitApplication()
		{
			Application.Current.Shutdown();
		}
	}
}
