using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using MPDCtrlX.Core.Models;
using MPDCtrlX.Core.ViewModels;
using MPDCtrlX.Core.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace MPDCtrlX.Core.Views;

public partial class QueuePage : UserControl
{
    private bool _isHeaderWidthInitialized;

    public QueuePage() { }
    public QueuePage(MainViewModel vm)
    {
        //var vm = App.GetService<MainViewModel>();
        DataContext = vm;

        InitializeComponent();

        vm.ScrollIntoView += (sender, arg) => { this.OnScrollIntoView(arg); };
        vm.ScrollIntoViewAndSelect += (sender, arg) => { this.OnScrollIntoViewAndSelect(arg); };
        vm.QueueHeaderVisibilityChanged += this.OnQueueHeaderVisibilityChanged;
        vm.QueueFindWindowVisibilityChangedSetFocus += this.OnQueueFindWindowVisibilityChanged_SetFocus;

        this.DetachedFromVisualTree += (s, e) =>
        {
            vm.ScrollIntoView -= (sender, arg) => { this.OnScrollIntoView(arg); };
            vm.ScrollIntoViewAndSelect -= (sender, arg) => { this.OnScrollIntoViewAndSelect(arg); };
            vm.QueueHeaderVisibilityChanged -= this.OnQueueHeaderVisibilityChanged;
            vm.QueueFindWindowVisibilityChangedSetFocus -= this.OnQueueFindWindowVisibilityChanged_SetFocus;
        };
    }

    private void ListBox_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel)
        {
            return;
        }

        if (_isHeaderWidthInitialized)
        {
            // Everytime page is changed come back, this loaded is called. So.
            return;
        }

        _isHeaderWidthInitialized = true;

        // This is a dirty workaround for AvaloniaUI which does not have ListView control at this moment.

        // Position - hidden up/down
        this.DummyHeader.ColumnDefinitions[0].Width = new GridLength(80);//vm.QueueColumnHeaderPositionWidth
        //this.Column1.IsVisible = true;
        //this.Column1X.Width = 80;//vm.QueueColumnHeaderPositionWidth
        this.Column1.Width = 80;
        this.DummyHeader.ColumnDefinitions[0].Width = GridLength.Auto;

        /*
        // Title
        this.DummyHeader.ColumnDefinitions[4].Width = new GridLength(vm.QueueColumnHeaderTitleWidth);
        this.Column3.IsVisible = true;
        this.Column3X.Width = vm.QueueColumnHeaderTitleWidth;
        this.DummyHeader.ColumnDefinitions[4].Width = GridLength.Auto;
        */
        UpdateColumHeaders();
    }

    // ListBox dummy header Visibility option change.
    private void OnQueueHeaderVisibilityChanged(object? sender, System.EventArgs e)
    {
        UpdateColumHeaders();
    }

    private void UpdateColumHeaders()
    {
        if (DataContext is not MainViewModel vm)
        {
            return;
        }

        // This is a dirty workaround for AvaloniaUI which does not have ListView control at this moment.

        // Title

        // Time
        if (vm.IsQueueColumnHeaderTimeVisible)
        {
            this.DummyHeader.ColumnDefinitions[3].Width = new GridLength(70);
        }
        else
        {
            this.DummyHeader.ColumnDefinitions[3].Width = new GridLength(0);
        }

        // Artist
        if (vm.IsQueueColumnHeaderArtistVisible)
        {
            this.DummyHeader.ColumnDefinitions[4].Width = GridLength.Parse("3*");
        }
        else
        {
            this.DummyHeader.ColumnDefinitions[4].Width = new GridLength(0);
        }

        // Album
        if (vm.IsQueueColumnHeaderAlbumVisible)
        {
            this.DummyHeader.ColumnDefinitions[5].Width = GridLength.Parse("3*");
        }
        else
        {
            this.DummyHeader.ColumnDefinitions[5].Width = new GridLength(0);
        }

        // Disc
        if (vm.IsQueueColumnHeaderDiscVisible)
        {
            this.DummyHeader.ColumnDefinitions[6].Width = new GridLength(65);
        }
        else
        {
            this.DummyHeader.ColumnDefinitions[6].Width = new GridLength(0);
        }

        // Track
        if (vm.IsQueueColumnHeaderTrackVisible)
        {
            this.DummyHeader.ColumnDefinitions[7].Width = new GridLength(65);
        }
        else
        {
            this.DummyHeader.ColumnDefinitions[7].Width = new GridLength(0);
        }

        // Genre
        if (vm.IsQueueColumnHeaderGenreVisible)
        {
            this.DummyHeader.ColumnDefinitions[8].Width = GridLength.Parse("2*");
        }
        else
        {
            this.DummyHeader.ColumnDefinitions[8].Width = new GridLength(0);
        }

        // Last modified
        if (vm.IsQueueColumnHeaderLastModifiedVisible)
        {
            this.DummyHeader.ColumnDefinitions[9].Width = GridLength.Parse("2*");
        }
        else
        {
            this.DummyHeader.ColumnDefinitions[9].Width = new GridLength(0);
        }
    }

    private async void OnScrollIntoView(int ind)
    {
        await Task.Yield();
        //await Task.Delay(100); // Wait for UI to update
        Dispatcher.UIThread.Post(() =>
        {
            if (this.QueueListBox is ListBox lb)
            {
                //lb.AutoScrollToSelectedItem = true;
                lb.ScrollIntoView(ind);
            }
        });
    }

    private async void OnScrollIntoViewAndSelect(int ind)
    {
        await Task.Yield();
        //await Task.Delay(800); // Need to wait for UI to update
        Dispatcher.UIThread.Post(() =>
        {
            if (this.QueueListBox is ListBox lb)
            {
                lb.ScrollIntoView(ind);

                if (DataContext is not MainViewModel vm)
                {
                    return;
                }

                /* 
                var test = vm?.Queue.FirstOrDefault(x => x.IsPlaying == true);
                if (test != null)
                {
                    //lb.ScrollIntoView(test.Index);
                    test.IsSelected = true;
                }
                */

                var test = vm?.Queue.FirstOrDefault(x => x.Index == ind);
                if (test != null)
                {
                    test.IsSelected = true;
                }

                //lb.AutoScrollToSelectedItem = true;
            }
        });
    }

    // Double click to play
    private void QueueListBox_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            _ = vm.QueueListviewEnterKey();
        }
    }

    // FilterBox
    private void FilterQueueListBox_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        if (this.FilterQueueListBox.SelectedItem is SongInfoEx song)
        {
            if (this.QueueListBox is ListBox lb)
            {
                lb.ScrollIntoView(song.Index);

                song.IsSelected = true;
            }
        }
    }

    // Sets focus in textbox
    private async void OnQueueFindWindowVisibilityChanged_SetFocus(object? sender, System.EventArgs e)
    {
        await Task.Yield();
        await Task.Delay(50); // Need to wait for UI to update

        Dispatcher.UIThread.Post(() =>
        {
            if (this.TglButtonQueueFilter is ToggleButton tb)
            {
                if (tb.IsChecked == true)
                {
                    this.FilterQueueQueryTextBox.Focus();
                }
            }
        });
    }

    // Sets focus in textbox when clicked.
    private void TglButtonQueueFilter_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (this.TglButtonQueueFilter is ToggleButton tb)
        {
            if (tb.IsChecked == true)
            {
                this.FilterQueueQueryTextBox.Focus();
            }
        }
    }

    // This is a workaround to keep the header in sync with the ListBox scrolling.
    private void ScrollViewer_ScrollChanged(object? sender, Avalonia.Controls.ScrollChangedEventArgs e)
    {
        if (sender is Avalonia.Controls.ScrollViewer sv)
        {
            this.QueueListViewHeaderScrollViewer.Offset = sv.Offset;
        }
    }

    private void PageGrid_SizeChanged(object? sender, Avalonia.Controls.SizeChangedEventArgs e)
    {
        if (!e.WidthChanged)
        {
            return;
        }
        
        if (DataContext is not MainViewModel vm)
        {
            return;
        }

        if (e.NewSize.Width < 340)
        {
            vm.IsQueueColumnHeaderTimeVisible = false;
            vm.IsQueueColumnHeaderArtistVisible = false;
            vm.IsQueueColumnHeaderAlbumVisible = false;
            vm.IsQueueColumnHeaderDiscVisible = false;
            vm.IsQueueColumnHeaderTrackVisible = false;
            vm.IsQueueColumnHeaderGenreVisible = false;
            vm.IsQueueColumnHeaderLastModifiedVisible = false;
            //
            this.HeaderGridSpacer.Width = 48;
            this.FindFilterButtonStackPanel.IsVisible = false;
            this.SortToggleSplitButton.IsVisible = false;
        }
        else if (e.NewSize.Width < 740)
        {
            vm.IsQueueColumnHeaderTimeVisible = true;
            vm.IsQueueColumnHeaderArtistVisible = false;
            vm.IsQueueColumnHeaderAlbumVisible = false;
            vm.IsQueueColumnHeaderDiscVisible = false;
            vm.IsQueueColumnHeaderTrackVisible = false;
            vm.IsQueueColumnHeaderGenreVisible = false;
            vm.IsQueueColumnHeaderLastModifiedVisible = false;
            //
            this.HeaderGridSpacer.Width = 48;
            this.FindFilterButtonStackPanel.IsVisible = false;
            this.SortToggleSplitButton.IsVisible = false;
        }
        else if (e.NewSize.Width < 1008)
        {
            vm.IsQueueColumnHeaderTimeVisible = true;
            vm.IsQueueColumnHeaderArtistVisible = true;
            vm.IsQueueColumnHeaderAlbumVisible = true;
            vm.IsQueueColumnHeaderDiscVisible = false;
            vm.IsQueueColumnHeaderTrackVisible = false;
            vm.IsQueueColumnHeaderGenreVisible = false;
            vm.IsQueueColumnHeaderLastModifiedVisible = false;
            //
            this.HeaderGridSpacer.Width = 24;
            this.FindFilterButtonStackPanel.IsVisible = true;
            this.SortToggleSplitButton.IsVisible = true;
        }
        else if (e.NewSize.Width < 1320)
        {
            vm.IsQueueColumnHeaderTimeVisible = true;
            vm.IsQueueColumnHeaderArtistVisible = true;
            vm.IsQueueColumnHeaderAlbumVisible = true;
            vm.IsQueueColumnHeaderDiscVisible = true;
            vm.IsQueueColumnHeaderTrackVisible = true;
            vm.IsQueueColumnHeaderGenreVisible = true;
            vm.IsQueueColumnHeaderLastModifiedVisible = false;
            //
            this.HeaderGridSpacer.Width = 24;
            this.FindFilterButtonStackPanel.IsVisible = true;
            this.SortToggleSplitButton.IsVisible = true;
        }
        else if (e.NewSize.Width < 2000)
        {
            vm.IsQueueColumnHeaderTimeVisible = true;
            vm.IsQueueColumnHeaderArtistVisible = true;
            vm.IsQueueColumnHeaderAlbumVisible = true;
            vm.IsQueueColumnHeaderDiscVisible = true;
            vm.IsQueueColumnHeaderTrackVisible = true;
            vm.IsQueueColumnHeaderGenreVisible = true;
            vm.IsQueueColumnHeaderLastModifiedVisible = true;
            //
            this.HeaderGridSpacer.Width = 24;
            this.FindFilterButtonStackPanel.IsVisible = true;
            this.SortToggleSplitButton.IsVisible = true;
        }
        else
        {
            vm.IsQueueColumnHeaderTimeVisible = true;
            vm.IsQueueColumnHeaderArtistVisible = true;
            vm.IsQueueColumnHeaderAlbumVisible = true;
            vm.IsQueueColumnHeaderDiscVisible = true;
            vm.IsQueueColumnHeaderTrackVisible = true;
            vm.IsQueueColumnHeaderGenreVisible = true;
            vm.IsQueueColumnHeaderLastModifiedVisible = true;
            //
            this.HeaderGridSpacer.Width = 24;
            this.FindFilterButtonStackPanel.IsVisible = true;
            this.SortToggleSplitButton.IsVisible = true;
        }

        UpdateColumHeaders();
    }
}