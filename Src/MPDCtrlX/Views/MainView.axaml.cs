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
    public MainView() { }

    public MainView(MainViewModel? vm)
    {
        DataContext = vm;//App.GetService<MainViewModel>();//new MainViewModel();

        InitializeComponent();

        if (vm is not null)
        {
            vm.DebugWindowShowHide += () => OnDebugWindowShowHide();
            vm.DebugCommandOutput += (sender, arg) => { this.OnDebugCommandOutput(arg); };
            vm.DebugIdleOutput += (sender, arg) => { this.OnDebugIdleOutput(arg); };
        }
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

    /*
    public void OnAckWindowOutput(string arg)
    {
        // AppendText() is much faster than data binding.
        AckTextBox.AppendText(arg);

        AckTextBox.CaretIndex = AckTextBox.Text.Length;
        AckTextBox.ScrollToEnd();
    }

    public void OnAckWindowClear()
    {
        //AckTextBox.Clear();
    }
    */

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


    private void TreeView_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        /*
        NodeMenu? value = ((sender as TreeView)?.SelectedItem as NodeMenu);
        if (value == null)
        {
            return;
        }
        
        if (value is NodeMenuQueue)
        {
            this.ContentFrame.Content = (App.Current as App)?.AppHost.Services.GetRequiredService<QueuePage>();
        }
        else if (value is NodeMenuPlaylists)
        {
            this.ContentFrame.Content = (App.Current as App)?.AppHost.Services.GetRequiredService<PlaylistsPage>();
        }
        else if (value is NodeMenuPlaylistItem)
        {
            this.ContentFrame.Content = (App.Current as App)?.AppHost.Services.GetRequiredService<PlaylistItemPage>();
        }
        else if (value is NodeMenuLibrary)
        {
            this.ContentFrame.Content = (App.Current as App)?.AppHost.Services.GetRequiredService<LibraryPage>();
        }
        else if (value is NodeMenuSearch)
        {
            this.ContentFrame.Content = (App.Current as App)?.AppHost.Services.GetRequiredService<SearchPage>();
        }
        else if (value is NodeMenuAlbum)
        {
            this.ContentFrame.Content = (App.Current as App)?.AppHost.Services.GetRequiredService<AlbumPage>();
        }
        else if (value is NodeMenuArtist)
        {
            this.ContentFrame.Content = (App.Current as App)?.AppHost.Services.GetRequiredService<ArtistPage>();
        }
        */
    }
    

}
