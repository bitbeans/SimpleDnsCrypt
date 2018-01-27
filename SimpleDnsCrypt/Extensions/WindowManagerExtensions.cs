using Caliburn.Micro;
using SimpleDnsCrypt.Models;
using SimpleDnsCrypt.ViewModels;
using System.Windows;

namespace SimpleDnsCrypt.Extensions
{
	/// <summary>
	///     Custom WindowManagerExtensions.
	/// </summary>
	public static class WindowManagerExtensions
	{
		/// <summary>
		///     Extended MetroMessageBox.
		/// </summary>
		/// <param name="this"></param>
		/// <param name="message"></param>
		/// <param name="title"></param>
		/// <param name="buttons"></param>
		/// <param name="messageBoxType"></param>
		/// <returns></returns>
		public static MessageBoxResult ShowMetroMessageBox(this IWindowManager @this, string message, string title,
			MessageBoxButton buttons, BoxType messageBoxType = BoxType.Default)
		{
			MessageBoxResult retval;
			var shellViewModel = IoC.Get<MainViewModel>();

			try
			{
				var model = new MetroMessageBoxViewModel(message, title, buttons, messageBoxType);
				Execute.OnUIThread(() => @this.ShowDialog(model));
				retval = model.Result;
			}
			finally
			{

			}

			return retval;
		}

		/// <summary>
		///     Simple MetroMessageBox.
		/// </summary>
		/// <param name="this"></param>
		/// <param name="message"></param>
		public static void ShowMetroMessageBox(this IWindowManager @this, string message)
		{
			@this.ShowMetroMessageBox(message, "System Message", MessageBoxButton.OK);
		}
	}
}
