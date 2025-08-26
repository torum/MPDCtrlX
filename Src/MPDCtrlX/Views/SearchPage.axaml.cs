using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.DependencyInjection;
using MPDCtrlX.Models;
using MPDCtrlX.ViewModels;
using MPDCtrlX.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace MPDCtrlX.Views;

public partial class SearchPage : UserControl
{
    private bool _isHeaderWidthInitialized;

    public SearchPage() { }

    public SearchPage(MainViewModel viewmodel)
    {
        DataContext = viewmodel;

        InitializeComponent();

        viewmodel.SearchPageAddToPlaylistDialogShow += this.OnAddToPlaylistDialogShowAsync;
        viewmodel.SearchHeaderVisibilityChanged += this.OnSearchHeaderVisibilityChanged;
    }

    private void ListBox_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel vm)
        {
            return;
        }

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

        // Title
        this.DummyHeader.ColumnDefinitions[4].Width = new GridLength(vm.SearchColumnHeaderTitleWidth);
        this.SearchColumn3.IsVisible = true;
        this.SearchColumn3x.Width = vm.SearchColumnHeaderTitleWidth;
        this.DummyHeader.ColumnDefinitions[4].Width = GridLength.Auto;

        UpdateColumHeaders();
    }

    // ListBox dummy header Visibility option change.
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
    }

    // Called on window closing to save dummy header sizes.
    public void SaveSearchHeaderWidth()
    {
        if (DataContext is not MainViewModel vm)
        {
            return;
        }

        if (!_isHeaderWidthInitialized)
        {
            return;
        }

        vm.SearchColumnHeaderPositionWidth = this.SearchColumn1.Bounds.Size.Width;
        // = this.PlaylistColumn2.Bounds.Size.Width;
        vm.SearchColumnHeaderTitleWidth = this.SearchColumn3.Bounds.Size.Width;
        vm.SearchColumnHeaderTimeWidth = this.SearchColumn4.Bounds.Size.Width;
        vm.SearchColumnHeaderArtistWidth = this.SearchColumn5.Bounds.Size.Width;
        vm.SearchColumnHeaderAlbumWidth = this.SearchColumn6.Bounds.Size.Width;
        vm.SearchColumnHeaderDiscWidth = this.SearchColumn7.Bounds.Size.Width;
        vm.SearchColumnHeaderTrackWidth = this.SearchColumn8.Bounds.Size.Width;
        vm.SearchColumnHeaderGenreWidth = this.SearchColumn9.Bounds.Size.Width;
        vm.SearchColumnHeaderLastModifiedWidth = this.SearchColumn10.Bounds.Size.Width;
    }

    // This is a workaround to keep the header in sync with the ListBox scrolling.
    private void ScrollViewer_ScrollChanged(object? sender, Avalonia.Controls.ScrollChangedEventArgs e)
    {
        if (sender is Avalonia.Controls.ScrollViewer sv)
        {
            this.SearchListViewHeaderScrollViewer.Offset = sv.Offset;
        }
    }

    private async void OnAddToPlaylistDialogShowAsync(object? sender, List<string> list)
    {
        if (DataContext is not MainViewModel vm)
        {
            return;
        }

        var dialog = new ContentDialog
        {
            Title = MPDCtrlX.Properties.Resources.Dialog_Title_SelectPlaylist,
            IsPrimaryButtonEnabled = true,
            PrimaryButtonText = Properties.Resources.Dialog_Ok,
            DefaultButton = ContentDialogButton.Primary,
            IsSecondaryButtonEnabled = false,
            CloseButtonText = Properties.Resources.Dialog_CancelClose,
            Content = new Views.Dialogs.SaveToDialog()
            {
                //DataContext = new DialogViewModel()
            }
        };

        if (dialog.Content is SaveToDialog asdf)
        {
            // Sort
            CultureInfo ci = CultureInfo.CurrentCulture;
            StringComparer comp = StringComparer.Create(ci, true);

            asdf.PlaylistComboBox.ItemsSource = new ObservableCollection<Playlist>(vm.Playlists.OrderBy(x => x.Name, comp));
        }

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary && dialog.Content is Views.Dialogs.SaveToDialog dlg)
        {
            /*
            var plselitem = dlg.PlaylistComboBox.SelectedItem;

            if (plselitem is Models.Playlist pl)
            {
                if (string.IsNullOrWhiteSpace(pl.Name))
                {
                    return;
                }

                vm?.AddToPlaylist_Execute(pl.Name.Trim(), list);
            }
            */
            if (dlg.CreateNewCheckBox.IsChecked is true)
            {
                var str = dlg.TextBoxPlaylistName.Text ?? string.Empty;

                // TODO; check if already exists?

                if (!string.IsNullOrEmpty(str.Trim()))
                {
                    vm?.AddToPlaylist_Execute(str.Trim(), list);
                }
            }
            else
            {
                var plselitem = dlg.PlaylistComboBox.SelectedItem;

                if (plselitem is Models.Playlist pl)
                {
                    if (string.IsNullOrWhiteSpace(pl.Name))
                    {
                        return;
                    }

                    vm?.AddToPlaylist_Execute(pl.Name.Trim(), list);
                }
            }
        }
    }
}