﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace Intense.UI
{
    /// <summary>
    /// Describes customizations to the non-client area of the current window.
    /// </summary>
    public class WindowChrome : DependencyObject
    {
        class WeakVisibleBoundsChangedEventHandler
        {
            private WeakReference<WindowChrome> reference;

            public WeakVisibleBoundsChangedEventHandler(WindowChrome target)
            {
                this.reference = new WeakReference<WindowChrome>(target);

                var appView = ApplicationView.GetForCurrentView();
                appView.VisibleBoundsChanged += OnVisibleBoundsChanged;
            }

            private void OnVisibleBoundsChanged(ApplicationView sender, object args)
            {
                WindowChrome target;
                if (this.reference.TryGetTarget(out target)) {
                    target.CalculateMargin();
                }
                else {
                    sender.VisibleBoundsChanged -= OnVisibleBoundsChanged;
                }
            }
        }

        /// <summary>
        /// Identifies the Chrome attached property.
        /// </summary>
        public static readonly DependencyProperty ChromeProperty = DependencyProperty.RegisterAttached("Chrome", typeof(WindowChrome), typeof(WindowChrome), new PropertyMetadata(null, OnChromeChanged));
        /// <summary>
        /// Identifies the AutoUpdateMargin dependency property.
        /// </summary>
        public static readonly DependencyProperty AutoUpdateMarginProperty = DependencyProperty.Register("AutoUpdateMargin", typeof(bool), typeof(WindowChrome), new PropertyMetadata(true));
        /// <summary>
        /// Identifies the Margin dependency property.
        /// </summary>
        public static readonly DependencyProperty MarginProperty = DependencyProperty.Register("Margin", typeof(Thickness), typeof(WindowChrome), new PropertyMetadata(null, OnMarginChanged));
        /// <summary>
        /// Identifies the StatusBarBackgroundColor dependency property.
        /// </summary>
        public static readonly DependencyProperty StatusBarBackgroundColorProperty = DependencyProperty.Register("StatusBarBackgroundColor", typeof(Color), typeof(WindowChrome), new PropertyMetadata(null, OnStatusBarBackgroundColorChanged));
        /// <summary>
        /// Identifies the StatusBarForegroundColor dependency property.
        /// </summary>
        public static readonly DependencyProperty StatusBarForegroundColorProperty = DependencyProperty.Register("StatusBarForegroundColor", typeof(Color), typeof(WindowChrome), new PropertyMetadata(null, OnStatusBarForegroundColorChanged));

        private FrameworkElement target;

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowChrome"/>.
        /// </summary>
        public WindowChrome()
        {
            new WeakVisibleBoundsChangedEventHandler(this);
            CalculateMargin();
        }

        private void CalculateMargin()
        { 
            var appView = ApplicationView.GetForCurrentView();
            var visibleBounds = appView.VisibleBounds;
            var wndBounds = Window.Current.Bounds;

            if (visibleBounds != wndBounds) {
                var left = Math.Ceiling(visibleBounds.Left - wndBounds.Left);
                var top = Math.Ceiling(visibleBounds.Top - wndBounds.Top);
                var right = Math.Ceiling(wndBounds.Right - visibleBounds.Right);
                var bottom = Math.Ceiling(wndBounds.Bottom - visibleBounds.Bottom);

                this.Margin = new Thickness(left, top, right, bottom);
            }
            else {
                this.Margin = new Thickness();
            }
        }

        private static bool TryGetStatusBar(out StatusBar statusBar)
        {
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar")) {
                statusBar = StatusBar.GetForCurrentView();
                return true;
            }
            statusBar = null;
            return false;
        }

        private static void OnChromeChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            // assign dependency object of type FrameworkElement to the chrome instance
            // this works as long as the chrome instance is not shared among dependency objects
            var chrome = (WindowChrome)args.NewValue;
            chrome?.SetTarget(o as FrameworkElement);
        }

        private void SetTarget(FrameworkElement target)
        {
            this.target = target;
            ApplyMarginToTarget();
        }

        private void ApplyMarginToTarget()
        {
            if (this.AutoUpdateMargin && this.target != null) {
                this.target.Margin = this.Margin;
            }
        }

        private static void OnMarginChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            ((WindowChrome)o).ApplyMarginToTarget();
        }

        private static void OnStatusBarBackgroundColorChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            StatusBar statusBar;
            if (TryGetStatusBar(out statusBar)) {
                // infer opacity from alpha channel of the color
                var color = (Color)args.NewValue;
                statusBar.BackgroundOpacity = (double)color.A / 255d;
                statusBar.BackgroundColor = (Color)args.NewValue;
            }
        }

        private static void OnStatusBarForegroundColorChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            StatusBar statusBar;
            if (TryGetStatusBar(out statusBar)) {
                statusBar.ForegroundColor = (Color)args.NewValue;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the margin of the target framework element is automatically updated when the visible bounds changes.
        /// </summary>
        public bool AutoUpdateMargin
        {
            get { return (bool)GetValue(AutoUpdateMarginProperty); }
            set { SetValue(AutoUpdateMarginProperty, value); }
        }

        /// <summary>
        /// Gets the window margin.
        /// </summary>
        public Thickness Margin
        {
            get { return (Thickness)GetValue(MarginProperty); }
            private set { SetValue(MarginProperty, value); }
        }

        /// <summary>
        /// Gets or set the status bar background color.
        /// </summary>
        public Color StatusBarBackgroundColor
        {
            get { return (Color)GetValue(StatusBarBackgroundColorProperty); }
            set { SetValue(StatusBarBackgroundColorProperty, value); }
        }

        /// <summary>
        /// Gets or set the status bar foreground color.
        /// </summary>
        public Color StatusBarForegroundColor
        {
            get { return (Color)GetValue(StatusBarForegroundColorProperty); }
            set { SetValue(StatusBarForegroundColorProperty, value); }
        }

        /// <summary>
        /// Retrieves the <see cref="WindowChrome"/> attached to specified dependency object instance.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static WindowChrome GetChrome(DependencyObject o)
        {
            if (o == null) {
                throw new ArgumentNullException("o");
            }
            return (WindowChrome)o.GetValue(ChromeProperty);
        }

        /// <summary>
        /// Attaches a <see cref="WindowChrome"/> to specified dependency object.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="chrome"></param>
        public static void SetChrome(DependencyObject o, WindowChrome chrome)
        {
            o.SetValue(ChromeProperty, chrome);
        }
    }
}
