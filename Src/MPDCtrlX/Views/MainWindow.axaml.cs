﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
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
    private readonly MainView? _shellPage;// = (App.Current as App)?.AppHost.Services.GetRequiredService<MainView>();
    private readonly SettingsPage? _settingsPage;// = (App.Current as App)?.AppHost.Services.GetRequiredService<SettingsPage>();

    private NavigationViewItem? _navigationViewSelectedItem;

    public MainWindow() { }

    public MainWindow(MainView shellPage, MainViewModel vm, SettingsPage settingsPage)
    {
        _shellPage = shellPage;
        _settingsPage = settingsPage;

        this.DataContext = vm;
        
        #region == This must be before InitializeComponent() ==

        if ((vm.WindowLeft > 0) && (vm.WindowTop > 0)) 
        {
            this.Position = new PixelPoint(vm.WindowLeft, vm.WindowTop);
        }
        
        if (vm.WindowHeight >= 120)
        {
            this.Height = vm.WindowHeight;
        }
        //else { this.Height = 180; }

        if (vm.WindowWidth >= 480)
        {
            this.Width = vm.WindowWidth;
        }
        //else { this.Width = 740; }

        #endregion

        InitializeComponent();

        this.navigateView.Content = shellPage;

        this.Loaded += vm.OnWindowLoaded;
        this.Closing += vm.OnWindowClosing;

        //this.ContentRendered += vm.OnContentRendered;
        //vm.CurrentSongChanged += (sender, arg) => OnCurrentSongChanged(arg);
        /*
        Unloaded += (sender, e) =>
        {
            this.Loaded -= vm.OnWindowLoaded;
            this.Closing -= vm.OnWindowClosing;
        };
        */



        //
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

    /*
    private void OnCurrentSongChanged(string msg)
    {
        //this.Title = msg;
    }
    */

    private void NavigationView_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is not NavigationView)
        {
            return;
        }

        if (this.DataContext is MainViewModel vm)
        {
            var hoge = this.navigateView.MenuItems.OfType<NavigationViewItem>().FirstOrDefault();
            if (hoge != null)
            {
                hoge.IsSelected = true;

                //
                _navigationViewSelectedItem = hoge;
            }

            vm.SelectedNodeMenu = vm.MainMenuItems.FirstOrDefault();
            if (vm.SelectedNodeMenu != null)
            {
                vm.SelectedNodeMenu.Selected = true;
            }

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
    }

    private void NavigationView_SelectionChanged(object? sender, FluentAvalonia.UI.Controls.NavigationViewSelectionChangedEventArgs e)
    {
        if (sender is not NavigationView)
        {
            return;
        }

        if (this.DataContext is MainViewModel vm)
        {
            if (e.SelectedItem is NodeMenuPlaylists)
            {
                // don't change page here.
                //pl.Selected = false;
                if (vm.SelectedNodeMenu != null)
                {
                    //vm.SelectedNodeMenu.Selected = true;
                }
            }
            else if (e.SelectedItem is NodeMenuLibrary)
            {
                // don't change page here.
                //lb.Selected = false;
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
    }

    private void NavigationView_ItemInvoked(object? sender, FluentAvalonia.UI.Controls.NavigationViewItemInvokedEventArgs e)
    {
        if (sender is NavigationView nv)
        {
            if (e.IsSettingsInvoked == true)
            {
                nv.Content = _settingsPage;
                return;
            }

            if (nv.Content != _shellPage)
            {
                nv.Content = _shellPage;
            }


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
        }
    }

}
