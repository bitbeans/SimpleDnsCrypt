using Caliburn.Micro;
using SimpleDnsCrypt.Config;
using SimpleDnsCrypt.Helper;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;

namespace SimpleDnsCrypt.ViewModels
{
	[Export(typeof(FallbackResolversViewModel))]
	public class FallbackResolversViewModel : Screen
	{
		private string _windowTitle;
		private ObservableCollection<string> _fallbackResolvers;
		private string _selectedFallbackResolver;
		private string _addressInput;


		[ImportingConstructor]
		public FallbackResolversViewModel()
		{
			_fallbackResolvers = new ObservableCollection<string>();
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

		public ObservableCollection<string> FallbackResolvers
		{
			get => _fallbackResolvers;
			set
			{
				_fallbackResolvers = value;
				NotifyOfPropertyChange(() => FallbackResolvers);
			}
		}

		public string SelectedFallbackResolver
		{
			get => _selectedFallbackResolver;
			set
			{
				_selectedFallbackResolver = value;
				NotifyOfPropertyChange(() => SelectedFallbackResolver);
			}
		}

		public string AddressInput
		{
			get => _addressInput;
			set
			{
				_addressInput = value;
				NotifyOfPropertyChange(() => AddressInput);
			}
		}

		public void AddAddress()
		{
			if (string.IsNullOrEmpty(_addressInput)) return;
			var validatedAddress = ValidationHelper.ValidateIpEndpoint(_addressInput);
			if (string.IsNullOrEmpty(validatedAddress)) return;
			if (FallbackResolvers.Contains(validatedAddress)) return;
			FallbackResolvers.Add(validatedAddress);
			AddressInput = string.Empty;
		}

		public void RemoveAddress()
		{
			if (string.IsNullOrEmpty(_selectedFallbackResolver)) return;
			if (_fallbackResolvers.Count == 1) return;
			_fallbackResolvers.Remove(_selectedFallbackResolver);
		}

		public void RestoreDefault()
		{
			FallbackResolvers.Clear();
			FallbackResolvers = new ObservableCollection<string>(Global.DefaultFallbackResolvers);
		}
	}
}
