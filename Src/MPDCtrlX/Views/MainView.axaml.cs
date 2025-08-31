using Avalonia.Controls;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using MPDCtrlX.Models;
using MPDCtrlX.ViewModels;
using System;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Text;

namespace MPDCtrlX.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        var vm = App.GetService<MainViewModel>();
        DataContext = vm;

        InitializeComponent();


        vm.DebugWindowShowHide += () => OnDebugWindowShowHide();
        vm.DebugCommandOutput += (sender, arg) => { this.OnDebugCommandOutput(arg); };
        vm.DebugIdleOutput += (sender, arg) => { this.OnDebugIdleOutput(arg); };


        /*
        Unloaded += (sender, e) =>
        {
            if (vm is not null)
            {
                // Unsubscribe from ViewModel events.
                vm.DebugWindowShowHide -= () => OnDebugWindowShowHide();
                vm.DebugCommandOutput -= (sender, arg) => { this.OnDebugCommandOutput(arg); };
                vm.DebugIdleOutput -= (sender, arg) => { this.OnDebugIdleOutput(arg); };
                vm.AckWindowOutput -= (sender, arg) => { this.OnAckWindowOutput(arg); };
                vm.AckWindowClear -= () => OnAckWindowClear();
            }
        };
        */
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

    private void HeaderGrid_SizeChanged(object? sender, Avalonia.Controls.SizeChangedEventArgs e)
    {
        if (!e.WidthChanged)
        {
            return;
        }

        if (e.NewSize.Width < 580)
        {
        }
        else
        {
        }

        if (e.NewSize.Width < 740)
        {
            //this.SeekSlider.Width = 250;
            //this.SeekSlider.Width = 380;
            this.AlbumCoverBorder.IsVisible = false;

            //HeaderOverlayColSpaceLeft.Width = 24;
            //HeaderOverlayColSpaceRight.Width = 24;
        }
        else
        {
            this.AlbumCoverBorder.IsVisible = true;

            //HeaderOverlayColSpaceLeft.Width = 170;
            //HeaderOverlayColSpaceRight.Width = 170;
        }
    }

    /*
    private void OnGoToSelectedPage(NodeTree? selectedNodeTree)
    {
        if (this.DataContext is not MainViewModel vm)
        {
            return;
        }

        if (selectedNodeTree is null)
        {
            return;
        }

        if (vm.SelectedNodeMenu != selectedNodeTree)
        {
            // just in case
            vm.SelectedNodeMenu = selectedNodeTree;
        }

        if (selectedNodeTree is NodeMenuQueue)
        {
            if (NavigationFrame.Navigate(typeof(QueuePage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom }))//, args.RecommendedNavigationTransitionInfo //new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft }
            {
                //_currentPage = typeof(QueuePage);
            }
        }
        else if (selectedNodeTree is NodeMenuLibrary)
        {
            // Do nothing.
        }
        else if (selectedNodeTree is NodeMenuAlbum)
        {
            if (NavigationFrame.Navigate(typeof(AlbumPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom }))
            {
                //_currentPage = typeof(AlbumPage);
            }
        }
        else if (selectedNodeTree is NodeMenuArtist)
        {
            if (NavigationFrame.Navigate(typeof(ArtistPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom }))
            {
                //_currentPage = typeof(ArtistPage);
            }
        }
        else if (selectedNodeTree is NodeMenuFiles)
        {
            if (NavigationFrame.Navigate(typeof(FilesPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom }))
            {
                //_currentPage = typeof(FilesPage);
            }
        }
        else if (selectedNodeTree is NodeMenuSearch)
        {
            if (NavigationFrame.Navigate(typeof(SearchPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom }))
            {
                //_currentPage = typeof(SearchPage);
            }
        }
        else if (selectedNodeTree is NodeMenuPlaylists)
        {
            // Do nothing.
        }
        else if (selectedNodeTree is NodeMenuPlaylistItem)
        {
            if (NavigationFrame.Navigate(typeof(PlaylistItemPage), null, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromBottom }))
            {
                //_currentPage = typeof(PlaylistItemPage);
            }
        }
        else
        {
            Debug.WriteLine("Not NodeMenu");
        }
    }
    
        */
}
