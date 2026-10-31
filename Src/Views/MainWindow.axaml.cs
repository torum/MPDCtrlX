using Avalonia;
using Avalonia.Animation;
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
using Microsoft.Extensions.DependencyInjection;
using MPDCtrlX.Models;
using MPDCtrlX.Services;
using MPDCtrlX.Services.Contracts;
using MPDCtrlX.ViewModels;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace MPDCtrlX.Views;

internal sealed partial class MainWindow : Window
{
    private readonly IConfigurationService _configurationService;
    private readonly INavigationService _navigationService;
    private readonly MainViewModel _mainViewModel;
    private bool _isClosing;
    private bool _isAllowClose;
    private readonly Dictionary<string, Type> _pageMap = new()
    {
        { "MPDCtrlX.Views.QueuePage", typeof(Views.QueuePage) },
        { "MPDCtrlX.Views.AlbumPage", typeof(Views.AlbumPage) },
        { "MPDCtrlX.Views.ArtistPage", typeof(Views.ArtistPage) },
        { "MPDCtrlX.Views.FilesPage", typeof(Views.FilesPage) },
        { "MPDCtrlX.Views.SearchPage", typeof(Views.SearchPage) },
        { "MPDCtrlX.Views.PlaylistItemPage", typeof(Views.PlaylistItemPage) },
        { "MPDCtrlX.Views.SettingsPage", typeof(Views.SettingsPage) },
    };

    public int WinRestoreWidth { get; private set; } = 1024;
    public int WinRestoreHeight { get; private set; } = 768;
    public int WinRestoreTop { get; private set; } = 100;
    public int WinRestoreLeft { get; private set; } = 100;

    // Parameterless constructor for XAML Previewer
    public MainWindow()
    {
        _configurationService = App.GetService<IConfigurationService>();
        _navigationService = App.GetService<INavigationService>();
        _mainViewModel = App.GetService<MainViewModel>();
        this.DataContext = _mainViewModel;

        InitializeComponent();

        bool isMax = false;
        var mw = _configurationService.CurrentConfiguration.MainWindow;

        // TODO: multi-monitor support. If the saved position is outside of the current monitor, move it to the monitor.
        if ((mw.Left >= 0) && (mw.Top >= 0))
        {
            this.Position = new PixelPoint(mw.Left, mw.Top);
        }
        else
        {
            this.Position = new PixelPoint(0, 0);
        }

        if (mw.Height >= 120)
        {
            this.Height = mw.Height;
        }

        if (mw.Width >= 480)
        {
            this.Width = mw.Width;
        }

        WinRestoreWidth = (int)mw.RestoreWidth;
        WinRestoreHeight = (int)mw.RestoreHeight;
        WinRestoreLeft = mw.RestoreLeft;
        WinRestoreTop = mw.RestoreTop;

        if (mw.WindowState == WindowState.Maximized)
        {
            this.WindowState = WindowState.Maximized;
            isMax = true;
        }
        else
        {
            this.WindowState = WindowState.Normal;
            isMax = false;
        }

        //this.ContentFrame.Navigate(typeof(MainView));
        //this.NavigateViewControl.Content = App.GetService<MainView>();
        _navigationService.Initialize(RootFrame, _pageMap);

        //this.Icon = new WindowIcon(new Bitmap())
        Bitmap bitmap = new(AssetLoader.Open(new Uri("avares://MPDCtrlX/Assets/MPDCtrlX-24.png")));
        ImageAppIcon.Source = bitmap;

        this.AddHandler(InputElement.KeyDownEvent, OnPreviewKeyDown, RoutingStrategies.Tunnel);
        this.AddHandler(InputElement.KeyUpEvent, OnPreviewKeyUp, RoutingStrategies.Tunnel);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            UpdateWindowMargins(isMax);
            UpdateThemeBackground(ActualThemeVariant);
        }
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        this.SizeChanged += this.Window_SizeChanged;
        this.PositionChanged += this.Window_PositionChanged;
        this.ActualThemeVariantChanged += OnActualThemeVariantChanged;
        this.PropertyChanged += this.OnWindow_PropertyChanged;

        if (this.DataContext is not MainViewModel vm)
        {
            return;
        }

        this.Loaded += vm.OnWindowLoaded;

        vm.DebugWindowShowHide += OnDebugWindowShowHide;
        vm.DebugCommandOutput += OnDebugCommandOutput;
        vm.DebugIdleOutput += OnDebugIdleOutput;
        vm.GoToSettingsPage += OnGoToSettingsPage;
        vm.UserCanExecuteChanged += OnUserCanExecuteChanged;
        vm.WorkingStateChanged += OnWorkingStateChanged;
    }

    protected override async void OnClosed(EventArgs e)
    {
        this.SizeChanged -= this.Window_SizeChanged;
        this.PositionChanged -= this.Window_PositionChanged;
        this.ActualThemeVariantChanged -= OnActualThemeVariantChanged;
        this.PropertyChanged -= this.OnWindow_PropertyChanged;

        if (this.DataContext is MainViewModel vm)
        {
            this.Loaded -= vm.OnWindowLoaded;

            vm.DebugWindowShowHide -= OnDebugWindowShowHide;
            vm.DebugCommandOutput -= OnDebugCommandOutput;//(sender, arg) => { this.OnDebugCommandOutput(arg); };
            vm.DebugIdleOutput -= OnDebugIdleOutput;//(sender, arg) => { this.OnDebugIdleOutput(arg); };
            vm.GoToSettingsPage -= OnGoToSettingsPage;
            vm.UserCanExecuteChanged -= OnUserCanExecuteChanged;
            vm.WorkingStateChanged -= OnWorkingStateChanged;

            vm.Dispose();
        }

        // must be the last.
        base.OnClosed(e);
    }

    protected override async void OnClosing(WindowClosingEventArgs e)
    {
        Debug.WriteLine("OnClosing");

        if (_isAllowClose) return;

        e.Cancel = true; // Cancel the default close behavior to allow for async operations

        if (_isClosing) return;
        _isClosing = true;

        this.Hide();

        if (this.DataContext is MainViewModel vm)
        {
            // Tell vm the window is closed, so it should update settings.
            vm.UpdateCurrentSettings();
        }

        // Window Position must be saved before the window is closed, otherwise the position will be lost.
        #region == Save Configuration ==

        var config = _configurationService.CurrentConfiguration;
        config.MainWindow.Width = this.Width;
        config.MainWindow.Height = this.Height;
        config.MainWindow.Left = this.Position.X;
        config.MainWindow.Top = this.Position.Y;
        config.MainWindow.WindowState = this.WindowState;
        config.MainWindow.RestoreWidth = WinRestoreWidth;
        config.MainWindow.RestoreHeight = WinRestoreHeight;
        config.MainWindow.RestoreLeft = WinRestoreLeft;
        config.MainWindow.RestoreTop = WinRestoreTop;

        config.IsNavigationViewMenuOpen = this.NavigateViewControl.IsPaneOpen;

        await _configurationService.SaveConfigurationAsync();

        #endregion

        _isAllowClose = true;

        this.Close();
    }

    private void Window_Activated(object? sender, EventArgs e)
    {
        this.AppTitle.Opacity = 1;
        if (this.WindowState == WindowState.Normal)
        {
            WinRestoreHeight = (int)this.Height;
            WinRestoreWidth = (int)this.Width;
            WinRestoreTop = this.Position.Y;
            WinRestoreLeft = this.Position.X;
        }
    }

    private void Window_Deactivated(object? sender, EventArgs e)
    {
        this.AppTitle.Opacity = 0.5;
    }

    private void UpdateThemeBackground(ThemeVariant theme)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            this.BackgroundLayerBorder.IsVisible = true;
            this.BackgroundLayerBorder.Opacity = 1;

            if (theme == ThemeVariant.Dark)
            {
                //this.TransparencyLevelHint = [WindowTransparencyLevel.AcrylicBlur];
                this.BackgroundLayerBorder.Background = new SolidColorBrush(Color.Parse("#202020"));
            }
            else if (theme == ThemeVariant.Light)
            {
                //this.TransparencyLevelHint = [WindowTransparencyLevel.Blur];
                this.BackgroundLayerBorder.Background = new SolidColorBrush(Color.Parse("#EEEEEE"));
            }
            else if (theme == ThemeVariant.Default)
            {
                //this.TransparencyLevelHint = [WindowTransparencyLevel.AcrylicBlur];
                this.BackgroundLayerBorder.Background = new SolidColorBrush(Color.Parse("#121212"));
            }
        }
    }

    private void OnActualThemeVariantChanged(object? sender, EventArgs e)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            UpdateThemeBackground(ActualThemeVariant);
        }
    }

    private void UpdateWindowMargins(bool isMax)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            if (isMax)
            {
                // It's actually 10px on all sides, but the window border is 2px, so the total is 10px.
                // This covers the window border and makes it look little sophisticated.
                this.ContentGrid.Margin = new Thickness(0, 0, 0, 0);
                this.BackgroundLayerBorder.Margin = new Thickness(0, 0, 0, 0);
            }
            else
            {
                // It's actually 10px on all sides, but the window border is 2px, so the total is 10px.
                // This covers the window border and makes it look little sophisticated.
                this.ContentGrid.Margin = new Thickness(9);
                this.BackgroundLayerBorder.Margin = new Thickness(7);
            }
        }
    }

    private void OnWindow_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            if (sender is not Window)
            {
                return;
            }
            if (e.Property.Name != nameof(WindowState))
            {
                return;
            }

            // For linux.
            if (e.NewValue != null && e.OldValue != null)
            {
                if (e.NewValue.Equals(e.OldValue))
                {
                    return;
                }
            }
            else
            {
                return;
            }

            //Debug.WriteLine($"WindowState changed from {e.OldValue} to {e.NewValue}");

            if (e.NewValue is WindowState.FullScreen || e.NewValue is WindowState.Maximized)
            {
                UpdateWindowMargins(true);
            }
            else
            {
                UpdateWindowMargins(false);
            }
        }
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

        VolumeSlider.IsEnabled = vm.SetVolumeCommand.CanExecute(null);
        SeekSlider.IsEnabled = vm.SetSeekCommand.CanExecute(null);

        RepeatButton.IsEnabled = vm.SetRepeatCommand.CanExecute(null);
        SingleButton.IsEnabled = vm.SetSingleCommand.CanExecute(null);
        RandomButton.IsEnabled = vm.SetRandomCommand.CanExecute(null);
        ConsumeButton.IsEnabled = vm.SetConsumeCommand.CanExecute(null);
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

        if (this.DataContext is not MainViewModel vm)
        {
            return;
        }

        //var item  = vm.MainMenuItems.FirstOrDefault();
        //var navItem = NavigateViewControl.MenuItems.OfType<FANavigationViewItem>().Where(x => x.IsVisible == IsVisible).FirstOrDefault();//.First();
       
        if (_navigationService.NavigateTo("MPDCtrlX.Views.QueuePage", null))
        {
            var queueNode = vm.MainMenuItems.OfType<NodeMenuQueue>().FirstOrDefault();
            vm.SelectedNodeMenu = queueNode;
            vm.SelectedNodeMenu?.Selected = true;

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
        else
        {
            Debug.WriteLine("NavigationView_Loaded: Failed to navigate to QueuePage");
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

        if (e.SelectedItem is NodeMenu node)
        {
            vm.SelectedNodeMenu = node;
            //Debug.WriteLine($"e.SelectedItem is NodeTree. @NavigationView_SelectionChanged: {node.Tag}");
        }
        else
        {
            Debug.WriteLine("e.SelectedItem is not NodeTree. @NavigationView_SelectionChanged");
            vm.SelectedNodeMenu = null;
        }
    }

    // The cache size is informational only. A failure must not block navigation to Settings.
    private static async Task RefreshCacheFolderSizeSafeAsync(MainViewModel vm)
    {
        try
        {
            await vm.GetCacheFolderSizeAsync();
        }
        catch (OperationCanceledException)
        {
            // Ignore.
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception @GetCacheFolderSizeAsync {ex}");
            App.AppendErrorLog("Exception @MainWindow GetCacheFolderSizeAsync", ex.ToString());
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
            //await vm.GetCacheFolderSizeAsync();
            await RefreshCacheFolderSizeSafeAsync(vm);
            /*
            nv.Content = App.GetService<SettingsPage>();
            */
            if (_navigationService.NavigateTo("MPDCtrlX.Views.SettingsPage", null))
            {
                Debug.WriteLine($"NavigationView_ItemInvoked: Navigated to SettingsPage");
            }
            return;
        }

        if (e.InvokedItemContainer.DataContext is NodeMenu node)
        {
            if (node is NodeMenuPlaylistItem && RootFrame.Content is PlaylistItemPage page)
            {
                // Clear the content so that the page is animated when navigating to the same page with a different parameter.
                RootFrame.Content = null;
            }

            if (_navigationService.NavigateTo(node.Tag, null))
            {
                //Debug.WriteLine($"NavigationView_ItemInvoked: Navigated to {node.GetType().FullName}");
            }
            else
            {
                Debug.WriteLine($"NavigationView_ItemInvoked: Failed to navigate to {node.GetType().FullName}");
            }
        }
        else
        {
            Debug.WriteLine("NavigationView_ItemInvoked: e.InvokedItemContainer.DataContext is not NodeMenu");
        }

        /*
        var mainView = App.GetService<MainView>();
        if (nv.Content is not MainView)
        {
            nv.Content = mainView;
        }
        */
    }

    private void Window_SizeChanged(object? sender, Avalonia.Controls.SizeChangedEventArgs e)
    {
        if (this.Width < 340)
        {
            PlaybackOptions.IsVisible = false;
            this.NavigateViewControl.OpenPaneLength = 280;
        }
        else if (this.Width < 740)
        {
            PlaybackOptions.IsVisible = false;
            this.NavigateViewControl.OpenPaneLength = 280;
        }
        else if (this.Width < 1008)
        {
            PlaybackOptions.IsVisible = true;
            this.NavigateViewControl.OpenPaneLength = 280;
        }
        else if (this.Width < 1800)
        {
            PlaybackOptions.IsVisible = true;
            this.NavigateViewControl.OpenPaneLength = 280;
        }
        else
        {
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
            WinRestoreTop = this.Position.Y;
            WinRestoreLeft = this.Position.X;
        }
    }

    private void Window_PositionChanged(object? sender, PixelPointEventArgs e)
    {
        if (this.WindowState == WindowState.Normal)
        {
            WinRestoreHeight = (int)this.Height;
            WinRestoreWidth = (int)this.Width;
            WinRestoreTop = this.Position.Y;
            WinRestoreLeft = this.Position.X;
        }
    }

    //private readonly StringBuilder _sbCommandOutput = new();
    public void OnDebugCommandOutput(object? sender, string arg)
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
    public void OnDebugIdleOutput(object? sender, string arg)
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
        /*
        //await vm.GetCacheFolderSizeAsync();
        await RefreshCacheFolderSizeSafeAsync(vm);

        vm.SelectedNodeMenu = null;
        try
        {
            // This causes FluentAvalonia to throw null ex in some cases.
            this.NavigateViewControl.SelectedItem = null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception @NavigateViewControl.SelectedItem = null; {ex}");
        }

        this.NavigateViewControl.Content = App.GetService<SettingsPage>();
        //this.ContentFrame.Navigate(typeof(SettingsPage));
        */

        await RefreshCacheFolderSizeSafeAsync(vm);

        if (_navigationService.NavigateTo("MPDCtrlX.Views.SettingsPage", null))
        {
            Debug.WriteLine($"NavigationView_ItemInvoked: Navigated to SettingsPage");

            vm.SelectedNodeMenu = null;
            this.NavigateViewControl.SelectedItem = null;
        }
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

    private void OnPreviewKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Avalonia.Input.Key.Escape)
        {
            if (this.DataContext is not MainViewModel vm)
            {
                return;
            }
            vm.EscapeCommand.Execute(null);
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
            try
            {
                await vm.PlayCommand.ExecuteAsync(null);
            }
            catch (OperationCanceledException)
            {
                // Ignore.
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception @OnPreviewKeyUp {ex}");
                App.AppendErrorLog("Exception @MainWindow.OnPreviewKeyUp", ex.ToString());
            }

            // TODO:
            //e.Handled = true;
        }
    }

    private async void PlaybackPlay_Loaded(object? sender, RoutedEventArgs e)
    {
        // Delays focus until all current layout/focus cycles are finished
        Dispatcher.UIThread.Post(async () =>
        {
            try
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
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception @PlaybackPlay_Loaded {ex}");
            }
        }, DispatcherPriority.Background);
    }

    public static partial class NativeMethods
    {
        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_KEYMENU = 0xF100;

        private const uint TPM_RETURNCMD = 0x0100;

        private const uint MF_BYCOMMAND = 0x00000000;
        private const uint MF_ENABLED = 0x00000000;
        private const uint MF_GRAYED = 0x00000001;

        private const uint SC_RESTORE = 0xF120;
        private const uint SC_MOVE = 0xF010;
        private const uint SC_SIZE = 0xF000;
        private const uint SC_MINIMIZE = 0xF020;
        private const uint SC_MAXIMIZE = 0xF030;
        private const uint SC_CLOSE = 0xF060;

        //[DllImport("user32.dll", SetLastError = true)]
        //public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [LibraryImport("user32.dll", EntryPoint = "DefWindowProcW", SetLastError = true)]
        private static partial IntPtr DefWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [LibraryImport("user32.dll", SetLastError = true)]
        private static partial IntPtr GetSystemMenu(IntPtr hWnd, [MarshalAs(UnmanagedType.Bool)] bool bRevert);

        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

        [LibraryImport("user32.dll", SetLastError = true)]
        private static partial int TrackPopupMenu(IntPtr hMenu, uint uFlags, int x, int y, int nReserved, IntPtr hWnd, IntPtr prcRect);

        [LibraryImport("user32.dll", EntryPoint = "PostMessageW", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        // Helper method to show the system window menu (aka the Control Menu) for a given window handle
        public static void ShowSystemWindowMenu(IntPtr hWnd)
        {
            //DefWindowProc(hWnd, WM_SYSCOMMAND, (IntPtr)SC_KEYMENU, (IntPtr)32);

            IntPtr sysMenu = GetSystemMenu(hWnd, false);
            if (sysMenu != IntPtr.Zero)
            {
                // Close is always active
                EnableMenuItem(sysMenu, SC_CLOSE, MF_BYCOMMAND | MF_ENABLED);

                // FORCE WINDOWS TO RE-EVALUATE/ENABLE SYSTEM MENU ITEMS
                // wParam is the menu handle; lParam contains flags (0 is standard)
                //SendMessage(hWnd, WM_INITMENUPOPUP, sysMenu, IntPtr.Zero);

                DefWindowProc(hWnd, WM_SYSCOMMAND, (IntPtr)SC_KEYMENU, (IntPtr)32);
            }
        }

        public static void ShowNativeSystemMenu(PointerPressedEventArgs e, IntPtr hWnd, PixelPoint screenPoint, WindowState windowState, bool canResize)
        {
            // Fetch the window's live System Menu handle
            IntPtr sysMenu = GetSystemMenu(hWnd, false);
            if (sysMenu != IntPtr.Zero)
            {
                // FORCE WINDOWS TO RE-EVALUATE/ENABLE SYSTEM MENU ITEMS
                // wParam is the menu handle; lParam contains flags (IntPtr.Zero is standard)
                // 0x00010000 tells Windows: "This is the System Menu, update states relative to the window"
                //IntPtr lParam = new IntPtr(0x00010000);
                //SendMessage(hWnd, WM_INITMENUPOPUP, sysMenu, lParam);

                // Explicitly force the Close menu option to become enabled

                // Close is always active
                EnableMenuItem(sysMenu, SC_CLOSE, MF_BYCOMMAND | MF_ENABLED);

                // Adjust actions based on WindowState
                if (windowState == WindowState.Maximized)
                {
                    EnableMenuItem(sysMenu, SC_RESTORE, MF_BYCOMMAND | MF_ENABLED);
                    EnableMenuItem(sysMenu, SC_MAXIMIZE, MF_BYCOMMAND | MF_GRAYED);
                    EnableMenuItem(sysMenu, SC_MOVE, MF_BYCOMMAND | MF_GRAYED);
                    EnableMenuItem(sysMenu, SC_SIZE, MF_BYCOMMAND | MF_GRAYED);
                }
                else if (windowState == WindowState.Minimized)
                {
                    EnableMenuItem(sysMenu, SC_RESTORE, MF_BYCOMMAND | MF_ENABLED);
                    EnableMenuItem(sysMenu, SC_MINIMIZE, MF_BYCOMMAND | MF_GRAYED);
                }
                else // Normal / Restored
                {
                    EnableMenuItem(sysMenu, SC_RESTORE, MF_BYCOMMAND | MF_GRAYED);
                    EnableMenuItem(sysMenu, SC_MAXIMIZE, canResize ? (MF_BYCOMMAND | MF_ENABLED) : (MF_BYCOMMAND | MF_GRAYED));
                    EnableMenuItem(sysMenu, SC_MINIMIZE, MF_BYCOMMAND | MF_ENABLED);
                    EnableMenuItem(sysMenu, SC_MOVE, MF_BYCOMMAND | MF_ENABLED);
                    EnableMenuItem(sysMenu, SC_SIZE, canResize ? (MF_BYCOMMAND | MF_ENABLED) : (MF_BYCOMMAND | MF_GRAYED));
                }

                // Display the native menu at the cursor context.
                // TPM_RETURNCMD forces TrackPopupMenu to return the ID of the clicked menu item.
                int command = TrackPopupMenu(sysMenu, TPM_RETURNCMD, screenPoint.X, screenPoint.Y, 0, hWnd, IntPtr.Zero);

                if (command > 0)
                {
                    // Post the selected command back to the window loop so Windows handles it (Minimize, Maximize, Close, Move, etc.)
                    PostMessage(hWnd, WM_SYSCOMMAND, (IntPtr)command, IntPtr.Zero);
                }

                e.Handled = true;
            }
        }

    }

}
