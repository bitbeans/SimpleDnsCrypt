using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using Caliburn.Micro;
using SimpleDnsCrypt.Views;
using SimpleDnsCrypt.Windows;

namespace SimpleDnsCrypt
{
    internal class AppWindowManager : WindowManager
    {
        private readonly IDictionary<WeakReference, WeakReference> _windows =
            new Dictionary<WeakReference, WeakReference>();

        public override void ShowWindow(object rootModel, object context = null,
            IDictionary<string, object> settings = null)
        {
            NavigationWindow navWindow = null;

            if (Application.Current != null && Application.Current.MainWindow != null)
            {
                navWindow = Application.Current.MainWindow as NavigationWindow;
            }

            if (navWindow != null)
            {
                var window = CreatePage(rootModel, context, settings);
                navWindow.Navigate(window);
            }
            else
            {
                var window = GetExistingWindow(rootModel);
                if (window == null)
                {
                    window = CreateWindow(rootModel, false, context, settings);
                    _windows.Add(new WeakReference(rootModel), new WeakReference(window));
                    window.Show();
                }
                else
                {
                    window.Focus();
                }
            }
        }

        protected virtual Window GetExistingWindow(object model)
        {
            if (!_windows.Any(d => d.Key.IsAlive && d.Key.Target == model))
                return null;

            var existingWindow = _windows.Single(d => d.Key.Target == model).Value;
            return existingWindow.IsAlive ? existingWindow.Target as Window : null;
        }

        protected override Window EnsureWindow(object model, object view, bool isDialog)
        {
            Window window = view as BaseWindow;
            if (window == null)
            {
                if (isDialog)
                {
                    var dialogMessage = view as MetroMessageBoxView;
                    if (dialogMessage == null)
                    {
                        //normal Dialog
                        window = new BaseDialogWindow
                        {
                            Content = view,
                            SizeToContent = SizeToContent.WidthAndHeight
                        };
                    }
                    else
                    {
                        window = new BaseMessageDialogWindow
                        {
                            Content = view,
                            SizeToContent = SizeToContent.WidthAndHeight,
                            ShowTitleBar = false,
                            ShowCloseButton = false,
                            ResizeMode = ResizeMode.NoResize,
                            WindowStyle = WindowStyle.None,
                            WindowState = WindowState.Normal
                        };
                    }
                }
                else
                {
                    window = new BaseWindow
                    {
                        Content = view,
                        ResizeMode = ResizeMode.CanResizeWithGrip,
                        SizeToContent = SizeToContent.Manual
                    };
                }
                window.SetValue(View.IsGeneratedProperty, true);
            }
            else
            {
                var owner = InferOwnerOf(window);
                if (owner != null && isDialog)
                {
                    window.Owner = owner;
                }
            }
            return window;
        }
    }
}
