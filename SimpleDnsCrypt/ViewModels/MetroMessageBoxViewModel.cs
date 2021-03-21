using Caliburn.Micro;
using SimpleDnsCrypt.Models;
using System.ComponentModel.Composition;
using System.Windows;

namespace SimpleDnsCrypt.ViewModels
{
	[Export(typeof(MetroMessageBoxViewModel))]
	public class MetroMessageBoxViewModel : Screen
	{
		private MessageBoxButton _buttons = MessageBoxButton.OK;
		private string _message;
		private BoxType _messageBoxType;
		private string _title;

		/// <summary>
		///     MetroMessageBoxViewModel constructor.
		/// </summary>
		/// <param name="message">The message to show.</param>
		/// <param name="title">The title of the box.</param>
		/// <param name="buttons">Buttons to show up.</param>
		/// <param name="messageBoxType">The type of the box.</param>
		public MetroMessageBoxViewModel(string message, string title, MessageBoxButton buttons,
			BoxType messageBoxType)
		{
			if (title != null) Title = title;
			if (message != null) Message = message;
			MessageBoxType = messageBoxType;
			Buttons = buttons;
		}

		/// <summary>
		///     The MessageBox title.
		/// </summary>
		public string Title
		{
			get => _title;
			set
			{
				if (value.Equals(_title)) return;
				_title = value;
				NotifyOfPropertyChange(() => Title);
			}
		}

		/// <summary>
		///     The MessageBox type.
		/// </summary>
		public BoxType MessageBoxType
		{
			get => _messageBoxType;
			set
			{
				if (value.Equals(_messageBoxType)) return;
				_messageBoxType = value;
				NotifyOfPropertyChange(() => MessageBoxType);
			}
		}

		/// <summary>
		///     Show the No button.
		/// </summary>
		public bool IsNoButtonVisible => _buttons == MessageBoxButton.YesNo || _buttons == MessageBoxButton.YesNoCancel;

		/// <summary>
		///     Show the Yes button.
		/// </summary>
		public bool IsYesButtonVisible => _buttons == MessageBoxButton.YesNo || _buttons == MessageBoxButton.YesNoCancel;

		/// <summary>
		///     Show the Cancel button.
		/// </summary>
		public bool IsCancelButtonVisible => _buttons == MessageBoxButton.OKCancel || _buttons == MessageBoxButton.YesNoCancel;

		/// <summary>
		///     Show the Ok button.
		/// </summary>
		public bool IsOkButtonVisible => _buttons == MessageBoxButton.OK || _buttons == MessageBoxButton.OKCancel;

		/// <summary>
		///     The MessageBox message.
		/// </summary>
		public string Message
		{
			get => _message;
			set
			{
				if (value.Equals(_message)) return;
				_message = value;
				NotifyOfPropertyChange(() => Message);
			}
		}

		/// <summary>
		///     The MessageBox available buttons.
		/// </summary>
		public MessageBoxButton Buttons
		{
			get => _buttons;
			set
			{
				_buttons = value;
				NotifyOfPropertyChange(() => IsNoButtonVisible);
				NotifyOfPropertyChange(() => IsYesButtonVisible);
				NotifyOfPropertyChange(() => IsCancelButtonVisible);
				NotifyOfPropertyChange(() => IsOkButtonVisible);
			}
		}

		/// <summary>
		///     Return value of the MessageBox.
		/// </summary>
		public MessageBoxResult Result { get; private set; }

		/// <summary>
		///     Manage click of No button.
		/// </summary>
		public void No()
		{
			Result = MessageBoxResult.No;
			TryCloseAsync(false);
		}

		/// <summary>
		///     Manage click of Yes button.
		/// </summary>
		public void Yes()
		{
			Result = MessageBoxResult.Yes;
			TryCloseAsync(true);
		}

		/// <summary>
		///     Manage click of Cancel button.
		/// </summary>
		public void Cancel()
		{
			Result = MessageBoxResult.Cancel;
			TryCloseAsync(false);
		}

		/// <summary>
		///     Manage click of Ok button.
		/// </summary>
		public void Ok()
		{
			Result = MessageBoxResult.OK;
			TryCloseAsync(true);
		}
	}
}
