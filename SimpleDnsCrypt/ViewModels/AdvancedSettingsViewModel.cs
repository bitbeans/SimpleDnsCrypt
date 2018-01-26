using Caliburn.Micro;
using System.ComponentModel.Composition;

namespace SimpleDnsCrypt.ViewModels
{
	[Export(typeof(AdvancedSettingsViewModel))]
	public class AdvancedSettingsViewModel : Screen
	{
		private readonly IWindowManager _windowManager;
		private readonly IEventAggregator _events;

		public AdvancedSettingsViewModel()
		{
		}

		[ImportingConstructor]
		public AdvancedSettingsViewModel(IWindowManager windowManager, IEventAggregator events)
		{
			_windowManager = windowManager;
			_events = events;
			_events.Subscribe(this);


		}
	}
}