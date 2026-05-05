using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using FluentAvalonia.UI.Navigation;
using FluentAvalonia.UI.Windowing;
using Microsoft.Extensions.DependencyInjection;
using MPDCtrlX.Models;
using MPDCtrlX.ViewModels;
using MPDCtrlX.Views;
using SkiaSharp;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace MPDCtrlX.Views;

public sealed partial class MainWindow : Window//AppWindow//
{
    public int WinRestoreWidth { get; private set; } = 1024;
    public int WinRestoreHeight { get; private set; } = 768;
    public int WinRestoreTop { get; private set; } = 100;
    public int WinRestoreLeft { get; private set; } = 100;

#pragma warning disable CS8618 
    // Optional parameterless constructor for XAML Previewer
    public MainWindow() { InitializeComponent(); }
#pragma warning restore CS8618
    public MainWindow(MainViewModel vm) : base()
    {
        //var vm = App.GetService<MainViewModel>();
        this.DataContext = vm;

        if (vm.WindowState == WindowState.Maximized)
        {
            this.WindowState = WindowState.Maximized;
        }
        else
        {
            this.WindowState = WindowState.Normal;

            if ((vm.WindowLeft > 0) && (vm.WindowTop > 0))
            {
                this.Position = new PixelPoint(vm.WindowLeft, vm.WindowTop);
            }

            if (vm.WindowHeight >= 120)
            {
                this.Height = vm.WindowHeight;
            }

            if (vm.WindowWidth >= 480)
            {
                this.Width = vm.WindowWidth;
            }
        }

        InitializeComponent();

        this.NavigateViewControl.Content = App.GetService<MainView>();
        //this.ContentFrame.Navigate(typeof(MainView));

        //this.Icon = new WindowIcon(new Bitmap())

        this.Loaded += vm.OnWindowLoaded;
        this.Closing += vm.OnWindowClosing;
        this.Closed += vm.OnWindowClosed;
        this.SizeChanged += this.Window_SizeChanged;

        vm.DebugWindowShowHide += () => OnDebugWindowShowHide();
        vm.DebugCommandOutput += (sender, arg) => { this.OnDebugCommandOutput(arg); };
        vm.DebugIdleOutput += (sender, arg) => { this.OnDebugIdleOutput(arg); };
        vm.GoToSettingsPage += OnGoToSettingsPage;
        vm.UserCanExecuteChanged += OnUserCanExecuteChanged;
        vm.WorkingStateChanged += OnWorkingStateChanged;

        this.DetachedFromVisualTree += (s, e) =>
        {
            this.Loaded -= vm.OnWindowLoaded;
            this.Closing -= vm.OnWindowClosing;

            vm.DebugWindowShowHide -= () => OnDebugWindowShowHide();
            vm.DebugCommandOutput -= (sender, arg) => { this.OnDebugCommandOutput(arg); };
            vm.DebugIdleOutput -= (sender, arg) => { this.OnDebugIdleOutput(arg); };
            vm.GoToSettingsPage -= OnGoToSettingsPage;
        };

        Bitmap bitmap = new(AssetLoader.Open(new Uri("avares://MPDCtrlX/Assets/MPDCtrlX-24.png")));
        ImageAppIcon.Source = bitmap;

        /*
        var os = Environment.OSVersion;
        Debug.WriteLine("Current OS Information:");
        Debug.WriteLine("Platform: {0:G}", os.Platform);
        Debug.WriteLine("Version String: {0}", os.VersionString);
        Debug.WriteLine("Version Information:");
        Debug.WriteLine("   Major: {0}", os.Version.Major);
        Debug.WriteLine("   Minor: {0}", os.Version.Minor);
        Debug.WriteLine("Service Pack: '{0}'", os.ServicePack);
        */

        //if (os.Platform.ToString().StartsWith("Win"))
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            //this.ExtendClientAreaToDecorationsHint = true;

            //TitleBar.ExtendsContentIntoTitleBar = true;
            //TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;

            //TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;
            //TransparencyLevelHint = [WindowTransparencyLevel.AcrylicBlur];
            //Background = Brushes.Transparent;

            // Only on Windows
            //ExtendClientAreaToDecorationsHint = true;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            //TitleBar.ExtendsContentIntoTitleBar = true;
            //TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;

            //TransparencyLevelHint = [WindowTransparencyLevel.None];
            //TransparencyLevelHint = [WindowTransparencyLevel.AcrylicBlur];
            //Background = Brushes.Transparent;
            //Background = this.FindResource("ThemeBackgroundBrush") as IBrush;

            // Not currently supported on Linux due to X11.
            //ExtendClientAreaToDecorationsHint = false;
        }
        else
        {
            //
        }

        this.AddHandler(InputElement.KeyDownEvent, OnPreviewKeyDown, RoutingStrategies.Tunnel);
        this.AddHandler(InputElement.KeyUpEvent, OnPreviewKeyUp, RoutingStrategies.Tunnel);
    }

    private void OnWorkingStateChanged(object? sender, bool e)
    {
        if (e)
        {
            this.Cursor = new Cursor(StandardCursorType.AppStarting);
        }
        else
        {
            this.Cursor = new Cursor(StandardCursorType.Arrow);
        }

    }

    private void OnUserCanExecuteChanged(object? sender, EventArgs e)
    {
        if (this.DataContext is not MainViewModel vm)
        {
            return;
        }

        VolumeSlider.IsEnabled = vm.SetVolumeCanExecute();
        SeekSlider.IsEnabled = vm.SetSeekCanExecute();

        RepeatButton.IsEnabled = vm.SetRpeatCanExecute();
        SingleButton.IsEnabled = vm.SetSingleCanExecute();
        RandomButton.IsEnabled = vm.SetRandomCanExecute();
        ConsumeButton.IsEnabled = vm.SetConsumeCanExecute();
    }

    private void NavigationView_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is not FANavigationView)
        {
            return;
        }

        if (this.NavigateViewControl is null)
        {
            return;
        }
        /*
        if (this.NavigationFrame.Navigate(typeof(QueuePage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom }))//, args.RecommendedNavigationTransitionInfo //new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft }
        {
            //_currentPage = typeof(QueuePage);
        }
        */

        if (this.DataContext is not MainViewModel vm)
        {
            return;
        }

        vm.SelectedNodeMenu = vm.MainMenuItems.FirstOrDefault();
        vm.SelectedNodeMenu?.Selected = true;

        /*
         * Debug.WriteLine(this.navigateView.MenuItems.Count.ToString());
        var hoge = this.navigateView.MenuItems.OfType<NavigationViewItem>().FirstOrDefault();
        //var hoge = this.navigateView.MenuItems.FirstOrDefault();
        //if (hoge != null)
        if (hoge is NavigationViewItem nvi)
        {
            nvi.IsSelected = true;

            Debug.WriteLine(nvi.Name + " hoge");
            //
            //_navigationViewSelectedItem = hoge;
        }
        else
        {
            Debug.WriteLine("not item " + hoge?.ToString());
        }
        */

        if (vm.IsNavigationViewMenuOpen)
        {
            foreach (var fuga in vm.MainMenuItems)
            {
                if (fuga is NodeMenuLibrary lib)
                {
                    lib.Expanded = true;
                }
                else if (fuga is NodeMenuPlaylists plt)
                {
                    plt.Expanded = true;
                }
            }
        }
    }

    private void NavigationView_SelectionChanged(object? sender, FluentAvalonia.UI.Controls.FANavigationViewSelectionChangedEventArgs e)
    {
        if (sender is not FluentAvalonia.UI.Controls.FANavigationView)
        {
            return;
        }

        if (this.DataContext is not MainViewModel vm)
        {
            return;
        }

        if (e.SelectedItem is null)
        {
            vm.SelectedNodeMenu = null;
            return;
        }

        if (e.SelectedItem is NodeTree)
        {
            vm.SelectedNodeMenu = e.SelectedItem as NodeTree;
        }
        else
        {
            Debug.WriteLine("e.SelectedItem is not NodeTree. @NavigationView_SelectionChanged");
            vm.SelectedNodeMenu = null;
        }
    }

    private async void NavigationView_ItemInvoked(object? sender, FluentAvalonia.UI.Controls.FANavigationViewItemInvokedEventArgs e)
    {
        if (sender is not FluentAvalonia.UI.Controls.FANavigationView nv)
        {
            return;
        }

        if (this.DataContext is not MainViewModel vm)
        {
            return;
        }

        // Not really "invoked" now that we use clicking on the settings icon....
        if (e.IsSettingsInvoked == true)
        {
            await vm.GetCacheFolderSizeAsync();

            nv.Content = App.GetService<SettingsPage>();

            return;
        }

        var mainView = App.GetService<MainView>();
        if (nv.Content is not MainView)
        {
            nv.Content = mainView;
        }
    }

    private void Window_SizeChanged(object? sender, Avalonia.Controls.SizeChangedEventArgs e)
    {
        if (this.Width < 340)
        {
            //
            PlaybackOptions.IsVisible = false;
            this.NavigateViewControl.OpenPaneLength = 280;
        }
        else if (this.Width < 740)
        {
            //
            PlaybackOptions.IsVisible = false;
            this.NavigateViewControl.OpenPaneLength = 280;
        }
        else if (this.Width < 1008)
        {
            //
            PlaybackOptions.IsVisible = true;
            this.NavigateViewControl.OpenPaneLength = 280;
        }
        else if (this.Width < 1800)
        {
            //
            PlaybackOptions.IsVisible = true;
            this.NavigateViewControl.OpenPaneLength = 280;
        }
        else
        {
            //
            PlaybackOptions.IsVisible = true;
            this.NavigateViewControl.OpenPaneLength = 320;
        }

        if (this.WindowState == WindowState.Maximized)
        {
        }
        else if (this.WindowState == WindowState.Minimized)
        {
        }
        else
        {
            WinRestoreHeight = (int)this.Height;
            WinRestoreWidth = (int)this.Width;
            WinRestoreTop = (int)this.Position.X;
            WinRestoreLeft = (int)this.Position.X;
        }
    }

    //private readonly StringBuilder _sbCommandOutput = new();
    public void OnDebugCommandOutput(string arg)
    {
        // AppendText() is much faster than data binding.
        //DebugCommandTextBox.AppendText(arg);
        //DebugCommandTextBox.CaretIndex = DebugCommandTextBox.Text.Length;
        //DebugCommandTextBox.ScrollToEnd();

        //_sbCommandOutput.Append(arg);
        //DebugCommandTextBox.Text = _sbCommandOutput.ToString();
        DebugCommandTextBox.Text += arg;
        DebugCommandTextBox.CaretIndex = DebugCommandTextBox.Text.Length;
    }

    //private readonly StringBuilder _sbIdleOutput = new();
    public void OnDebugIdleOutput(string arg)
    {
        /*
        // AppendText() is much faster than data binding.
        DebugIdleTextBox.AppendText(arg);

        DebugIdleTextBox.CaretIndex = DebugIdleTextBox.Text.Length;
        DebugIdleTextBox.ScrollToEnd();
        */

        //_sbIdleOutput.Append(arg);
        //DebugIdleTextBox.Text = _sbIdleOutput.ToString();
        DebugIdleTextBox.Text += arg;
        DebugIdleTextBox.CaretIndex = DebugIdleTextBox.Text.Length;
    }

    public void OnDebugWindowShowHide()
    {
        if (this.DebugWindow.IsVisible)
        {
            this.DebugWindow.IsVisible = false;
        }
        else
        {
            this.DebugWindow.IsVisible = true;
        }
    }

    public async void OnGoToSettingsPage(object? sender, System.EventArgs e)
    {
        if (this.DataContext is not MainViewModel vm)
        {
            return;
        }

        await vm.GetCacheFolderSizeAsync();

        vm.SelectedNodeMenu = null;
        this.NavigateViewControl.SelectedItem = null;
        this.NavigateViewControl.Content = App.GetService<SettingsPage>();
        //this.ContentFrame.Navigate(typeof(SettingsPage));
    }

    private void VolumeSlider_PointerWheelChanged(object? sender, Avalonia.Input.PointerWheelEventArgs e)
    {
        // Adjust the Slider's Value based on the mouse wheel delta
        // e.Delta.Y typically indicates vertical scroll, positive for up, negative for down.
        // You might want to adjust the step size based on your requirements.
        double step = 1.0; // Adjust this value as needed for the desired scroll sensitivity

        if (e.Delta.Y > 0) // Scroll up
        {
            VolumeSlider.Value = Math.Min(VolumeSlider.Maximum, VolumeSlider.Value + step);
        }
        else if (e.Delta.Y < 0) // Scroll down
        {
            VolumeSlider.Value = Math.Max(VolumeSlider.Minimum, VolumeSlider.Value - step);
        }

        // Mark the event as handled to prevent further processing by other controls
        e.Handled = true;
    }

    private void SeekSlider_PointerWheelChanged(object? sender, Avalonia.Input.PointerWheelEventArgs e)
    {
        // Adjust the Slider's Value based on the mouse wheel delta
        // e.Delta.Y typically indicates vertical scroll, positive for up, negative for down.
        // You might want to adjust the step size based on your requirements.
        double step = 10.0; // Adjust this value as needed for the desired scroll sensitivity

        if (e.Delta.Y > 0) // Scroll up
        {
            SeekSlider.Value = Math.Min(SeekSlider.Maximum, SeekSlider.Value + step);
        }
        else if (e.Delta.Y < 0) // Scroll down
        {
            SeekSlider.Value = Math.Max(SeekSlider.Minimum, SeekSlider.Value - step);
        }

        // Mark the event as handled to prevent further processing by other controls
        e.Handled = true;
    }

    private async void OnPreviewKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Avalonia.Input.Key.Escape)
        {
            if (this.DataContext is not MainViewModel vm)
            {
                return;
            }
            vm.Escape();
        }
        else if (e.Key == Avalonia.Input.Key.Space)
        {
            if (e.KeyModifiers.HasFlag(KeyModifiers.Alt))
            {
                // Alt + Space pressed.
                Debug.WriteLine("OnPreviewKeyDown: Alt + Space pressed. Ignoring for play/pause to allow system menu access.");

                // Handle this at keyup.
                //if (GetTopLevel(this)?.TryGetPlatformHandle() is not { } handle) return;
                //NativeMethods.ShowSystemWindowMenu(handle.Handle);
                e.Handled = true;
                return;
            }
            /*
            var focusedControl = this.FocusManager?.GetFocusedElement();
            if (focusedControl is Avalonia.Controls.ListBox || focusedControl is Avalonia.Controls.ListBoxItem || focusedControl is Avalonia.Controls.TextBox
                || focusedControl is Avalonia.Controls.Button || focusedControl is Avalonia.Controls.ToggleSplitButton || focusedControl is Avalonia.Controls.CheckBox)
            {
                Debug.WriteLine($"Focused control: {focusedControl?.GetType().Name}. Space key pressed, but focus is on a control that should handle it. Ignoring for play/pause.");

                return;
            }

            if (this.DataContext is not MainViewModel vm)
            {
                Debug.WriteLine("DataContext is not MainViewModel. Cannot toggle play/pause.");
                return;
            }

            Debug.WriteLine("Space key pressed. Toggling play/pause.");

            await vm.Play();
            */

            // TODO:
            //e.Handled = true;
        }
    }

    private async void OnPreviewKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.Key == Avalonia.Input.Key.Space)
        {
            if (e.KeyModifiers.HasFlag(KeyModifiers.Alt))
            {
                // Alt + Space pressed.
                Debug.WriteLine("OnPreviewKeyUp: Alt + Space pressed. Ignoring for play/pause to allow system menu access.");

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    if (GetTopLevel(this)?.TryGetPlatformHandle() is not { } handle) return;
                    NativeMethods.ShowSystemWindowMenu(handle.Handle);
                    e.Handled = true;
                    return;
                }
            }

            var focusedControl = this.FocusManager?.GetFocusedElement();
            if (focusedControl is Avalonia.Controls.ListBox || focusedControl is Avalonia.Controls.ListBoxItem || focusedControl is Avalonia.Controls.TextBox
                || focusedControl is Avalonia.Controls.Button || focusedControl is Avalonia.Controls.ToggleSplitButton || focusedControl is Avalonia.Controls.CheckBox)
            {
                Debug.WriteLine($"OnPreviewKeyUp: Focused control: {focusedControl?.GetType().Name}. Space key pressed, but focus is on a control that should handle it. Ignoring for play/pause.");

                return;
            }

            if (this.DataContext is not MainViewModel vm)
            {
                Debug.WriteLine("OnPreviewKeyUp: DataContext is not MainViewModel. Cannot toggle play/pause.");
                return;
            }

            Debug.WriteLine("OnPreviewKeyUp: Space key pressed. Toggling play/pause.");
            await vm.Play();

            // TODO:
            //e.Handled = true;
        }
    }

    private async void PlaybackPlay_Loaded(object? sender, RoutedEventArgs e)
    {
        // Delays focus until all current layout/focus cycles are finished
        Dispatcher.UIThread.Post(async () =>
        {
            await Task.Delay(500);
            if (PlaybackPlay.Focusable)
            {
                PlaybackPlay.Focus();
            }
            else
            {
                Debug.WriteLine("PlaybackPlay button is not focusable. Cannot set initial focus.");
            }

            this.FocusManager?.Focus(PlaybackPlay, NavigationMethod.Unspecified);

        }, DispatcherPriority.Background);
    }

    public static partial class NativeMethods
    {
        public const int WM_SYSCOMMAND = 0x0112;
        public const int SC_KEYMENU = 0xF100;

        //[DllImport("user32.dll", SetLastError = true)]
        //private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [LibraryImport("user32.dll", EntryPoint = "DefWindowProcW")]
        private static partial IntPtr DefWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        // Helper method to show the Windows System Menu (aka the Control Menu) for a given window handle
        public static void ShowSystemWindowMenu(IntPtr hWnd)
        {
            DefWindowProc(hWnd, WM_SYSCOMMAND, (IntPtr)SC_KEYMENU, (IntPtr)32);
        }
    }
}
