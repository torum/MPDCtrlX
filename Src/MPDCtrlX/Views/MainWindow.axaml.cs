using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using FluentAvalonia.UI.Windowing;
using Microsoft.Extensions.DependencyInjection;
using MPDCtrlX.Models;
using MPDCtrlX.ViewModels;
using MPDCtrlX.Views;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MPDCtrlX.Views;

public partial class MainWindow : Window//AppWindow//
{
    public MainWindow() { }
    public MainWindow(MainViewModel vm)
    {
        //var vm = App.GetService<MainViewModel>();
        this.DataContext = vm;

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

        InitializeComponent();

        this.NavigateViewControl.Content = App.GetService<MainView>();

        //this.Icon = new WindowIcon(new Bitmap())

        this.Loaded += vm.OnWindowLoaded;
        this.Closing += vm.OnWindowClosing;


        vm.DebugWindowShowHide += () => OnDebugWindowShowHide();
        vm.DebugCommandOutput += (sender, arg) => { this.OnDebugCommandOutput(arg); };
        vm.DebugIdleOutput += (sender, arg) => { this.OnDebugIdleOutput(arg); };



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

    }


    private void Window_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        //
    }

    private void Window_Closing(object? sender, Avalonia.Controls.WindowClosingEventArgs e)
    {
        //
    }

    private void NavigationView_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is not NavigationView)
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
        if (vm.SelectedNodeMenu != null)
        {
            vm.SelectedNodeMenu.Selected = true;
        }

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

    private void NavigationView_SelectionChanged(object? sender, FluentAvalonia.UI.Controls.NavigationViewSelectionChangedEventArgs e)
    {
        if (sender is not NavigationView)
        {
            return;
        }

        if (this.DataContext is not MainViewModel vm)
        {
            return;
        }

        if (e.SelectedItem is NodeMenuPlaylists)
        {
            // don't change page here.
            if (vm.SelectedNodeMenu != null)
            {
                //vm.SelectedNodeMenu.Selected = true;
            }
        }
        else if (e.SelectedItem is NodeMenuLibrary)
        {
            // don't change page here.
            if (vm.SelectedNodeMenu != null)
            {
                //vm.SelectedNodeMenu.Selected = true;
            }
        }
        else
        {
            if (e.SelectedItem is not null)
            {
                vm.SelectedNodeMenu = e.SelectedItem as NodeTree;
            }
            else
            {
                vm.SelectedNodeMenu = null;
            }
        }
    }

    private void NavigationView_ItemInvoked(object? sender, FluentAvalonia.UI.Controls.NavigationViewItemInvokedEventArgs e)
    {
        if (sender is not NavigationView nv)
        {
            return;
        }

        if (e.IsSettingsInvoked == true)
        {
            nv.Content = App.GetService<SettingsPage>();
            return;
        }

        var mainView = App.GetService<MainView>();
        if (nv.Content != mainView)
        {
            nv.Content = mainView;
        }

        //
        /*
        if (sender is not NavigationView)
        {
            return;
        }

        if (this.DataContext is not MainViewModel vm)
        {
            return;
        }

        if (e.InvokedItemContainer is NavigationViewItem item)
        {
            if (item.DataContext is NodeMenuQueue nt)
            {
                if (this.NavigationFrame.Navigate(typeof(QueuePage), null, e.RecommendedNavigationTransitionInfo))//, args.RecommendedNavigationTransitionInfo //new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft }
                {
                    //_currentPage = typeof(QueuePage);
                    vm.SelectedNodeMenu = nt;
                }
            }
            else if (item.DataContext is NodeMenuLibrary)
            {
                // Do nothing.
            }
            else if (item.DataContext is NodeMenuAlbum)
            {
                if (this.NavigationFrame.Navigate(typeof(AlbumPage), null, e.RecommendedNavigationTransitionInfo))
                {
                    //_currentPage = typeof(AlbumPage);
                }
            }
            else if (item.DataContext is NodeMenuArtist)
            {
                if (this.NavigationFrame.Navigate(typeof(ArtistPage), null, e.RecommendedNavigationTransitionInfo))
                {
                    //_currentPage = typeof(ArtistPage);
                }
            }
            else if (item.DataContext is NodeMenuFiles)
            {
                if (this.NavigationFrame.Navigate(typeof(FilesPage), null, e.RecommendedNavigationTransitionInfo))
                {
                    //_currentPage = typeof(FilesPage);
                }
            }
            else if (item.DataContext is NodeMenuSearch)
            {
                if (this.NavigationFrame.Navigate(typeof(SearchPage), null, e.RecommendedNavigationTransitionInfo))
                {
                    //_currentPage = typeof(SearchPage);
                }
            }
            else if (item.DataContext is NodeMenuPlaylists)
            {
                // Do nothing.
            }
            else if (item.DataContext is NodeMenuPlaylistItem)
            {
                if (this.NavigationFrame.Navigate(typeof(PlaylistItemPage), null, e.RecommendedNavigationTransitionInfo))
                {
                    //_currentPage = typeof(PlaylistItemPage);
                }
            }
            else
            {
                Debug.WriteLine("Not NodeMenu");
            }
        }
        else
        {
            Debug.WriteLine("Not NavigationViewItem @NavigationView_ItemInvoked:" + e.InvokedItem.ToString());
        }
        */
        //


        /*
        if (nv.Content != _shellPage)
        {
            nv.Content = _shellPage;
        }
        */

        /*
        if (nv.SelectedItem is NodeMenuPlaylists)
        {
            // don't change page here.
            //nv.SelectedItem = _navigationViewSelectedItem;
            return;
        }
        else if (nv.SelectedItem is NodeMenuLibrary)
        {
            // don't change page here.
            //nv.SelectedItem = _navigationViewSelectedItem;
            return;
        }

        _navigationViewSelectedItem = nv.SelectedItem as NavigationViewItem;
        */
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
    }

    private readonly StringBuilder _sbCommandOutput = new();
    public void OnDebugCommandOutput(string arg)
    {
        // AppendText() is much faster than data binding.
        //DebugCommandTextBox.AppendText(arg);

        _sbCommandOutput.Append(arg);
        DebugCommandTextBox.Text = _sbCommandOutput.ToString();
        DebugCommandTextBox.CaretIndex = DebugCommandTextBox.Text.Length;
    }

    private readonly StringBuilder _sbIdleOutput = new();
    public void OnDebugIdleOutput(string arg)
    {
        /*
        // AppendText() is much faster than data binding.
        DebugIdleTextBox.AppendText(arg);

        DebugIdleTextBox.CaretIndex = DebugIdleTextBox.Text.Length;
        DebugIdleTextBox.ScrollToEnd();
        */

        //_sbIdleOutput.Append(DebugIdleTextBox.Text);
        _sbIdleOutput.Append(arg);
        DebugIdleTextBox.Text = _sbIdleOutput.ToString();
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
}
