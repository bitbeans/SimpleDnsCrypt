using Caliburn.Micro;
using DnsCrypt.Models;
using SimpleDnsCrypt.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SimpleDnsCrypt.ViewModels
{
	[Export(typeof(RouteViewModel))]
	public class RouteViewModel : Screen
	{
		private readonly IWindowManager _windowManager;
		private string _windowTitle;
		private AvailableResolver _resolver;

		public RouteViewModel()
		{
		}

		[ImportingConstructor]
		public RouteViewModel(IWindowManager windowManager)
		{
			_windowManager = windowManager;
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

		public AvailableResolver Resolver
		{
			get => _resolver;
			set
			{
				_resolver = value;
				NotifyOfPropertyChange(() => Resolver);
			}
		}

		public BindableCollection<StampFileEntry> Relays { get; internal set; }
	}
}
