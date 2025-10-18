using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.DependencyInjection;
using MPDCtrlX.Core.Models;
using MPDCtrlX.Core.ViewModels;
using MPDCtrlX.Core.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace MPDCtrlX.Core.Views;

public partial class SearchPage : UserControl
{
    private bool _isHeaderWidthInitialized;

    public SearchPage() { }
    public SearchPage(MainViewModel vm)
    {
        //var vm = App.GetService<MainViewModel>();
        DataContext = vm;

        InitializeComponent();

        vm.SearchHeaderVisibilityChanged += this.OnSearchHeaderVisibilityChanged;
        vm.SearchHeaderVisibilityChanged += this.OnSearchHeaderVisibilityChanged;

        this.DetachedFromVisualTree += (s, e) =>
        {
            vm.SearchHeaderVisibilityChanged -= this.OnSearchHeaderVisibilityChanged;
            vm.SearchHeaderVisibilityChanged -= this.OnSearchHeaderVisibilityChanged;
        };
    }

    private void ListBox_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_isHeaderWidthInitialized)
        {
            // Everytime page is changed back, this loaded is called. So.
            return;
        }

        _isHeaderWidthInitialized = true;
        /*
        this.SearchColumn1x.Width = vm.SearchColumnHeaderPositionWidth;
        //this.SearchColumn2x.Width = vm.SearchColumnHeaderNowPlayingWidth;
        this.SearchColumn3x.Width = vm.SearchColumnHeaderTitleWidth;
        this.SearchColumn4x.Width = vm.SearchColumnHeaderTimeWidth;
        this.SearchColumn5x.Width = vm.SearchColumnHeaderArtistWidth;
        this.SearchColumn6x.Width = vm.SearchColumnHeaderAlbumWidth;
        this.SearchColumn7x.Width = vm.SearchColumnHeaderDiscWidth;
        this.SearchColumn8x.Width = vm.SearchColumnHeaderTrackWidth;
        this.SearchColumn9x.Width = vm.SearchColumnHeaderGenreWidth;
        this.SearchColumn10x.Width = vm.SearchColumnHeaderLastModifiedWidth;
        */

        // This is a dirty workaround for AvaloniaUI which does not have ListView control at this moment.

        // Position - hidden up/down
        this.DummyHeader.ColumnDefinitions[0].Width = new GridLength(80);//vm.PlaylistColumnHeaderPositionWidth
        //this.Column1.IsVisible = true;
        this.SearchColumn1x.Width = 80;//vm.PlaylistColumnHeaderPositionWidth
        this.DummyHeader.ColumnDefinitions[0].Width = GridLength.Auto;
        /*
        // Title
        this.DummyHeader.ColumnDefinitions[4].Width = new GridLength(vm.SearchColumnHeaderTitleWidth);
        this.SearchColumn3.IsVisible = true;
        this.SearchColumn3x.Width = vm.SearchColumnHeaderTitleWidth;
        this.DummyHeader.ColumnDefinitions[4].Width = GridLength.Auto;
        */
        UpdateColumHeaders();
    }

    // ListBox dummy header Visibility option change
    private void OnSearchHeaderVisibilityChanged(object? sender, System.EventArgs e)
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
        if (vm.IsSearchColumnHeaderTimeVisible)
        {
            this.DummyHeader.ColumnDefinitions[3].Width = new GridLength(70);
        }
        else
        {
            this.DummyHeader.ColumnDefinitions[3].Width = new GridLength(0);
        }

        // Artist
        if (vm.IsSearchColumnHeaderArtistVisible)
        {
            this.DummyHeader.ColumnDefinitions[4].Width = GridLength.Parse("3*");
        }
        else
        {
            this.DummyHeader.ColumnDefinitions[4].Width = new GridLength(0);
        }

        // Album
        if (vm.IsSearchColumnHeaderAlbumVisible)
        {
            this.DummyHeader.ColumnDefinitions[5].Width = GridLength.Parse("3*");
        }
        else
        {
            this.DummyHeader.ColumnDefinitions[5].Width = new GridLength(0);
        }

        // Disc
        if (vm.IsSearchColumnHeaderDiscVisible)
        {
            this.DummyHeader.ColumnDefinitions[6].Width = new GridLength(65);
        }
        else
        {
            this.DummyHeader.ColumnDefinitions[6].Width = new GridLength(0);
        }

        // Track
        if (vm.IsSearchColumnHeaderTrackVisible)
        {
            this.DummyHeader.ColumnDefinitions[7].Width = new GridLength(65);
        }
        else
        {
            this.DummyHeader.ColumnDefinitions[7].Width = new GridLength(0);
        }

        // Genre
        if (vm.IsSearchColumnHeaderGenreVisible)
        {
            this.DummyHeader.ColumnDefinitions[8].Width = GridLength.Parse("2*");
        }
        else
        {
            this.DummyHeader.ColumnDefinitions[8].Width = new GridLength(0);
        }

        // Last modified
        if (vm.IsSearchColumnHeaderLastModifiedVisible)
        {
            this.DummyHeader.ColumnDefinitions[9].Width = GridLength.Parse("2*");
        }
        else
        {
            this.DummyHeader.ColumnDefinitions[9].Width = new GridLength(0);
        }


        /*
        // Time
        if (vm.IsSearchColumnHeaderTimeVisible)
        {
            if (vm.SearchColumnHeaderTimeWidth <= 0)
            {
                //vm.SearchColumnHeaderTimeWidth = 80; // Default width if not set
            }
            this.DummyHeader.ColumnDefinitions[6].Width = new GridLength(vm.SearchColumnHeaderTimeWidth);
            this.SearchColumn4.IsVisible = true;
            this.SearchColumn4x.Width = vm.SearchColumnHeaderTimeWidth;
            this.DummyHeader.ColumnDefinitions[6].Width = GridLength.Auto;
        }
        else
        {
            //this.SearchColumn4.IsVisible = false;
            this.SearchColumn4x.Width = 0;
            this.DummyHeader.ColumnDefinitions[6].Width = new GridLength(0);
            this.DummyHeader.ColumnDefinitions[6].Width = GridLength.Auto;
        }

        // Artist
        if (vm.IsSearchColumnHeaderArtistVisible)
        {
            if (vm.SearchColumnHeaderArtistWidth <= 0)
            {
                //vm.SearchColumnHeaderArtistWidth = 120; // Default width if not set
            }
            this.DummyHeader.ColumnDefinitions[8].Width = new GridLength(vm.SearchColumnHeaderArtistWidth);
            this.SearchColumn5.IsVisible = true;
            this.SearchColumn5x.Width = vm.SearchColumnHeaderArtistWidth;
            this.DummyHeader.ColumnDefinitions[8].Width = GridLength.Auto;
        }
        else
        {
            //this.SearchColumn5.IsVisible = false;
            this.SearchColumn5x.Width = 0;
            this.DummyHeader.ColumnDefinitions[8].Width = new GridLength(0);
            this.DummyHeader.ColumnDefinitions[8].Width = GridLength.Auto;
        }

        // Album
        if (vm.IsPlaylistColumnHeaderAlbumVisible)
        {
            if (vm.PlaylistColumnHeaderAlbumWidth <= 0)
            {
                //vm.PlaylistColumnHeaderAlbumWidth = 120; // Default width if not set
            }
            this.DummyHeader.ColumnDefinitions[10].Width = new GridLength(vm.SearchColumnHeaderAlbumWidth);
            this.SearchColumn6.IsVisible = true;
            this.SearchColumn6x.Width = vm.SearchColumnHeaderAlbumWidth;
            this.DummyHeader.ColumnDefinitions[10].Width = GridLength.Auto;
        }
        else
        {
            //this.SearchColumn6.IsVisible = false;
            this.SearchColumn6x.Width = 0;
            this.DummyHeader.ColumnDefinitions[10].Width = new GridLength(0);
            this.DummyHeader.ColumnDefinitions[10].Width = GridLength.Auto;
        }

        // Disc
        if (vm.IsSearchColumnHeaderDiscVisible)
        {
            if (vm.SearchColumnHeaderDiscWidth <= 0)
            {
                //vm.SearchColumnHeaderDiscWidth = 62; // Default width if not set
            }
            this.DummyHeader.ColumnDefinitions[12].Width = new GridLength(vm.SearchColumnHeaderDiscWidth);
            this.SearchColumn7.IsVisible = true;
            this.SearchColumn7x.Width = vm.SearchColumnHeaderDiscWidth;
            this.DummyHeader.ColumnDefinitions[12].Width = GridLength.Auto;
        }
        else
        {
            //this.SearchColumn7.IsVisible = false;
            this.SearchColumn7x.Width = 0;
            this.DummyHeader.ColumnDefinitions[12].Width = new GridLength(0);
            this.DummyHeader.ColumnDefinitions[12].Width = GridLength.Auto;
        }

        // Track
        if (vm.IsSearchColumnHeaderTrackVisible)
        {
            if (vm.SearchColumnHeaderTrackWidth <= 0)
            {
                //vm.SearchColumnHeaderTrackWidth = 62; // Default width if not set
            }
            this.DummyHeader.ColumnDefinitions[14].Width = new GridLength(vm.SearchColumnHeaderTrackWidth);
            this.SearchColumn8.IsVisible = true;
            this.SearchColumn8x.Width = vm.SearchColumnHeaderTrackWidth;
            this.DummyHeader.ColumnDefinitions[14].Width = GridLength.Auto;
        }
        else
        {
            //this.SearchColumn8.IsVisible = false;
            this.SearchColumn8x.Width = 0;
            this.DummyHeader.ColumnDefinitions[14].Width = new GridLength(0);
            this.DummyHeader.ColumnDefinitions[14].Width = GridLength.Auto;
        }

        // Genre
        if (vm.IsSearchColumnHeaderGenreVisible)
        {
            if (vm.SearchColumnHeaderGenreWidth <= 0)
            {
                //vm.SearchColumnHeaderGenreWidth = 80; // Default width if not set
            }
            this.DummyHeader.ColumnDefinitions[16].Width = new GridLength(vm.SearchColumnHeaderGenreWidth);//
            this.SearchColumn9.IsVisible = true;
            this.SearchColumn9x.Width = vm.SearchColumnHeaderGenreWidth;
            this.DummyHeader.ColumnDefinitions[16].Width = GridLength.Auto;
        }
        else
        {
            //this.SearchColumn9.IsVisible = false;
            this.SearchColumn9x.Width = 0;
            this.DummyHeader.ColumnDefinitions[16].Width = new GridLength(0);//
            this.DummyHeader.ColumnDefinitions[16].Width = GridLength.Auto;
        }

        // Last modified
        if (vm.IsSearchColumnHeaderLastModifiedVisible)
        {
            if (vm.SearchColumnHeaderLastModifiedWidth <= 0)
            {
                //vm.SearchColumnHeaderLastModifiedWidth = 120; // Default width if not set
            }
            this.DummyHeader.ColumnDefinitions[18].Width = new GridLength(vm.SearchColumnHeaderLastModifiedWidth);
            this.SearchColumn10.IsVisible = true;
            this.SearchColumn10x.Width = vm.SearchColumnHeaderLastModifiedWidth;
            this.DummyHeader.ColumnDefinitions[18].Width = GridLength.Auto;
        }
        else
        {
            //this.SearchColumn10.IsVisible = false;
            this.SearchColumn10x.Width = 0;
            this.DummyHeader.ColumnDefinitions[18].Width = new GridLength(0);
            this.DummyHeader.ColumnDefinitions[18].Width = GridLength.Auto;
        }
        */
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
            vm.IsSearchColumnHeaderTimeVisible = false;
            vm.IsSearchColumnHeaderArtistVisible = false;
            vm.IsSearchColumnHeaderAlbumVisible = false;
            vm.IsSearchColumnHeaderDiscVisible = false;
            vm.IsSearchColumnHeaderTrackVisible = false;
            vm.IsSearchColumnHeaderGenreVisible = false;
            vm.IsSearchColumnHeaderLastModifiedVisible = false;
            //
            this.HeaderGridSpacer.Width = 48;
        }
        else if (e.NewSize.Width < 740)
        {
            vm.IsSearchColumnHeaderTimeVisible = true;
            vm.IsSearchColumnHeaderArtistVisible = false;
            vm.IsSearchColumnHeaderAlbumVisible = false;
            vm.IsSearchColumnHeaderDiscVisible = false;
            vm.IsSearchColumnHeaderTrackVisible = false;
            vm.IsSearchColumnHeaderGenreVisible = false;
            vm.IsSearchColumnHeaderLastModifiedVisible = false;
            //
            this.HeaderGridSpacer.Width = 48;
        }
        else if (e.NewSize.Width < 1008)
        {
            vm.IsSearchColumnHeaderTimeVisible = true;
            vm.IsSearchColumnHeaderArtistVisible = true;
            vm.IsSearchColumnHeaderAlbumVisible = true;
            vm.IsSearchColumnHeaderDiscVisible = false;
            vm.IsSearchColumnHeaderTrackVisible = false;
            vm.IsSearchColumnHeaderGenreVisible = false;
            vm.IsSearchColumnHeaderLastModifiedVisible = false;
            //
            this.HeaderGridSpacer.Width = 24;
        }
        else if (e.NewSize.Width < 1320)
        {
            vm.IsSearchColumnHeaderTimeVisible = true;
            vm.IsSearchColumnHeaderArtistVisible = true;
            vm.IsSearchColumnHeaderAlbumVisible = true;
            vm.IsSearchColumnHeaderDiscVisible = true;
            vm.IsSearchColumnHeaderTrackVisible = true;
            vm.IsSearchColumnHeaderGenreVisible = true;
            vm.IsSearchColumnHeaderLastModifiedVisible = false;
            //
            this.HeaderGridSpacer.Width = 24;
        }
        else if (e.NewSize.Width < 2000)
        {
            vm.IsSearchColumnHeaderTimeVisible = true;
            vm.IsSearchColumnHeaderArtistVisible = true;
            vm.IsSearchColumnHeaderAlbumVisible = true;
            vm.IsSearchColumnHeaderDiscVisible = true;
            vm.IsSearchColumnHeaderTrackVisible = true;
            vm.IsSearchColumnHeaderGenreVisible = true;
            vm.IsSearchColumnHeaderLastModifiedVisible = true;
            //
            this.HeaderGridSpacer.Width = 24;
        }
        else
        {
            vm.IsSearchColumnHeaderTimeVisible = true;
            vm.IsSearchColumnHeaderArtistVisible = true;
            vm.IsSearchColumnHeaderAlbumVisible = true;
            vm.IsSearchColumnHeaderDiscVisible = true;
            vm.IsSearchColumnHeaderTrackVisible = true;
            vm.IsSearchColumnHeaderGenreVisible = true;
            vm.IsSearchColumnHeaderLastModifiedVisible = true;
            //
            this.HeaderGridSpacer.Width = 24;
        }

        UpdateColumHeaders();
    }

    // This is a workaround to keep the header in sync with the ListBox scrolling.
    private void ScrollViewer_ScrollChanged(object? sender, Avalonia.Controls.ScrollChangedEventArgs e)
    {
        if (sender is Avalonia.Controls.ScrollViewer sv)
        {
            this.SearchListViewHeaderScrollViewer.Offset = sv.Offset;
        }
    }

    private void UserControl_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.SearchQueryInputTextBox.Focus();
    }
}