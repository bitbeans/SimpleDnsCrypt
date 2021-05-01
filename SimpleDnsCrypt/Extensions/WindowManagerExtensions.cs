using System.Threading.Tasks;
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
		public static async Task<MessageBoxResult> ShowMetroMessageBox(this IWindowManager @this, string message, string title,
																	   MessageBoxButton buttons, BoxType messageBoxType = BoxType.Default)
		{
			var model = new MetroMessageBoxViewModel(message, title, buttons, messageBoxType);
			await Execute.OnUIThreadAsync(async () => await @this.ShowDialogAsync(model));
			return model.Result;
		}
	}
}
