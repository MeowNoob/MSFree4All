﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.Runtime.InteropServices; // For DllImport
using WinRT;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Composition;
using PInvoke;
using Windows.ApplicationModel;
using WinRT.Interop;
using Microsoft.UI;
using Microsoft.UI.Windowing;

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
namespace MSFree4All.Helpers
{
    public static class TitleBarHelper
    {
        public static void SetExtendedTitleBar(Window window,UIElement AppTitleBar)
        {
            FrameworkElement RootUI = (FrameworkElement)window.Content;
            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                AppWindow AppWindow = AppWindow.GetFromWindowId(Win32Interop.GetWindowIdFromWindow(WindowNative.GetWindowHandle(window)));
                var titlebar = AppWindow.TitleBar;
                titlebar.ExtendsContentIntoTitleBar = true;
                void SetColor(ElementTheme acualTheme)
                {
                    titlebar.ButtonHoverBackgroundColor = App.LayerFillColorDefaultColor;
                    titlebar.ButtonBackgroundColor = titlebar.ButtonInactiveBackgroundColor = Colors.Transparent;
                    switch (acualTheme)
                    {
                        case ElementTheme.Dark:
                            titlebar.ButtonForegroundColor = Colors.White;
                            titlebar.ButtonHoverForegroundColor = Colors.Silver;
                            titlebar.ButtonPressedForegroundColor = Colors.Silver;
                            break;
                        case ElementTheme.Light:
                            titlebar.ButtonForegroundColor = Colors.Black;
                            titlebar.ButtonHoverForegroundColor = Colors.DarkGray;
                            titlebar.ButtonPressedForegroundColor = Colors.DarkGray;
                            break;
                    }
                }
                RootUI.ActualThemeChanged += (s, _) => SetColor(s.ActualTheme);
                window.SetTitleBar(AppTitleBar);
                SetColor(RootUI.ActualTheme);
            }
            else
            {
               window. ExtendsContentIntoTitleBar = true;
               window.SetTitleBar(AppTitleBar);
            }
        }
    }
    
    public static class IconHelper
    {
        public static void SetIcon(Window window)
        {
            var icon = User32.LoadImage(
                hInst: IntPtr.Zero,
                name: $@"{App.GetAppDir()}\Assets\MSFree4All.ico".ToCharArray(),
                type: User32.ImageType.IMAGE_ICON,
                cx: 0,
                cy: 0,
                fuLoad: User32.LoadImageFlags.LR_LOADFROMFILE | User32.LoadImageFlags.LR_DEFAULTSIZE | User32.LoadImageFlags.LR_SHARED
            );
            var Handle = WindowNative.GetWindowHandle(window);
            User32.SendMessage(Handle, User32.WindowMessage.WM_SETICON, (IntPtr)1, icon);
            User32.SendMessage(Handle, User32.WindowMessage.WM_SETICON, (IntPtr)0, icon);
        }
    }
    public class WindowsSystemDispatcherQueueHelper
    {
        private object? _dispatcherQueueController;

        [StructLayout(LayoutKind.Sequential)]
        internal struct DispatcherQueueOptions
        {
            internal int dwSize;
            internal int threadType;
            internal int apartmentType;
        }

        [DllImport("CoreMessaging.dll")]
        private static extern int CreateDispatcherQueueController([In] DispatcherQueueOptions options, [In, Out, MarshalAs(UnmanagedType.IUnknown)] ref object? dispatcherQueueController);

        public void EnsureWindowsSystemDispatcherQueueController()
        {
            if (Windows.System.DispatcherQueue.GetForCurrentThread() != null)
            {
                // one already exists, so we'll just use it.
                return;
            }

            if (_dispatcherQueueController == null)
            {
                DispatcherQueueOptions options;
                options.dwSize = Marshal.SizeOf(typeof(DispatcherQueueOptions));
                options.threadType = 2;
                options.apartmentType = 2;

                CreateDispatcherQueueController(options, ref _dispatcherQueueController);
            }
        }
    }
    public class MicaBackground
    {
        private readonly Window _window;
        private MicaController _micaController = new();
        private SystemBackdropConfiguration _backdropConfiguration = new();
        private readonly WindowsSystemDispatcherQueueHelper _dispatcherQueueHelper = new();

        public MicaBackground(Window window)
        {
            _window = window;
        }

        public bool TrySetMicaBackdrop()
        {
            if (MicaController.IsSupported())
            {
                _dispatcherQueueHelper.EnsureWindowsSystemDispatcherQueueController();
                _window.Activated += WindowOnActivated;
                _window.Closed += WindowOnClosed;
                ((FrameworkElement)_window.Content).ActualThemeChanged += MicaBackground_ActualThemeChanged;
                _backdropConfiguration.IsInputActive = true;
                _backdropConfiguration.Theme = _window.Content switch
                {
                    FrameworkElement { ActualTheme: ElementTheme.Dark } => SystemBackdropTheme.Dark,
                    FrameworkElement { ActualTheme: ElementTheme.Light } => SystemBackdropTheme.Light,
                    FrameworkElement { ActualTheme: ElementTheme.Default } => SystemBackdropTheme.Default,
                    _ => throw new InvalidOperationException("Unknown theme")
                };

                _micaController.AddSystemBackdropTarget(_window.As<ICompositionSupportsSystemBackdrop>());
                _micaController.SetSystemBackdropConfiguration(_backdropConfiguration);
                return true;
            }

            return false;
        }

        private void MicaBackground_ActualThemeChanged(FrameworkElement sender, object args)
        {
            if (_backdropConfiguration != null)
            {
                SetConfigurationSourceTheme();
            }

        }
        private void SetConfigurationSourceTheme()
        {
            switch (((FrameworkElement)_window.Content).ActualTheme)
            {
                case ElementTheme.Dark: _backdropConfiguration.Theme = SystemBackdropTheme.Dark; break;
                case ElementTheme.Light: _backdropConfiguration.Theme = SystemBackdropTheme.Light; break;
                case ElementTheme.Default: _backdropConfiguration.Theme = SystemBackdropTheme.Default; break;
            }
        }
        private void WindowOnClosed(object sender, WindowEventArgs args)
        {
            _micaController.Dispose();
            _micaController = null!;
            _window.Activated -= WindowOnActivated;
            _backdropConfiguration = null!;
        }

        private void WindowOnActivated(object sender, WindowActivatedEventArgs args)
        {
            _backdropConfiguration.IsInputActive = args.WindowActivationState is not WindowActivationState.Deactivated;
        }
    }
}
