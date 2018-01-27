using Caliburn.Micro;
using System.Windows;
using SimpleDnsCrypt.Models;
using SimpleDnsCrypt.ViewModels;
using SimpleDnsCrypt.Windows;

namespace SimpleDnsCrypt
{
	/// <summary>
	/// Provides a window manager for the application
	/// </summary>
	public class AppWindowManager : WindowManager
	{
		/// <summary>
		/// Selects a base window depending on the view, model and dialog options
		/// </summary>
		/// <param name="model">The model</param>
		/// <param name="view">The view</param>
		/// <param name="isDialog">Whether it's a dialog</param>
		/// <returns>The proper window</returns>
		protected override Window EnsureWindow(object model, object view, bool isDialog)
		{
			Window window = view as BaseWindow;

			if (window == null)
			{
				if (isDialog)
				{
					if (model.GetType() == typeof(LoaderViewModel))
					{
						window = new SplashDialogWindow
						{
							Content = view,
							SizeToContent = SizeToContent.WidthAndHeight
						};
					}
					else if (model.GetType() == typeof(MetroMessageBoxViewModel))
					{
						window = new BaseMessageDialogWindow
						{
							Content = view,
							SizeToContent = SizeToContent.WidthAndHeight
						};
					}
					else
					{
						window = new BaseDialogWindow
						{
							Content = view,
							SizeToContent = SizeToContent.WidthAndHeight
						};
					}
				}
				else
				{
					window = new BaseWindow
					{
						Content = view,
						ResizeMode = ResizeMode.CanResizeWithGrip,
						SizeToContent = SizeToContent.Height
					};
				}

				window.SetValue(View.IsGeneratedProperty, true);
			}
			else
			{
				Window owner = InferOwnerOf(window);
				if (owner != null && isDialog)
				{
					window.Owner = owner;
				}
			}

			return window;
		}
	}
}
