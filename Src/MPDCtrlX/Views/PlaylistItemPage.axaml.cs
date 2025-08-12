using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.DependencyInjection;
using MPDCtrlX.ViewModels;
using System.Collections.Generic;

namespace MPDCtrlX.Views;

public partial class PlaylistItemPage : UserControl
{
    private bool _isHeaderWidthInitialized;

    public PlaylistItemPage() { }

    public PlaylistItemPage(MainViewModel viewmodel)
    {
        DataContext = viewmodel;

        InitializeComponent();

        viewmodel.PlaylistRenameToDialogShow += this.OnPlaylistRenameToDialogShowAsync;
        viewmodel.PlaylistHeaderVisibilityChanged += this.OnPlaylistHeaderVisibilityChanged;
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
        this.PlaylistColumn1x.Width = vm.PlaylistColumnHeaderPositionWidth;
        //this.PlaylistColumn2x.Width = ;
        this.PlaylistColumn3x.Width = vm.PlaylistColumnHeaderTitleWidth;
        this.PlaylistColumn4x.Width = vm.PlaylistColumnHeaderTimeWidth;
        this.PlaylistColumn5x.Width = vm.PlaylistColumnHeaderArtistWidth;
        this.PlaylistColumn6x.Width = vm.PlaylistColumnHeaderAlbumWidth;
        this.PlaylistColumn7x.Width = vm.PlaylistColumnHeaderDiscWidth;
        this.PlaylistColumn8x.Width = vm.PlaylistColumnHeaderTrackWidth;
        this.PlaylistColumn9x.Width = vm.PlaylistColumnHeaderGenreWidth;
        this.PlaylistColumn10x.Width = vm.PlaylistColumnHeaderLastModifiedWidth;
        */

        // This is a dirty workaround for AvaloniaUI which does not have ListView control at this moment.

        // Position - hidden up/down
        this.DummyHeader.ColumnDefinitions[0].Width = new GridLength(80);//vm.PlaylistColumnHeaderPositionWidth
        //this.Column1.IsVisible = true;
        this.PlaylistColumn1x.Width = 80;//vm.PlaylistColumnHeaderPositionWidth
        this.DummyHeader.ColumnDefinitions[0].Width = GridLength.Auto;

        // Title
        this.DummyHeader.ColumnDefinitions[4].Width = new GridLength(vm.PlaylistColumnHeaderTitleWidth);
        this.PlaylistColumn3.IsVisible = true;
        this.PlaylistColumn3x.Width = vm.PlaylistColumnHeaderTitleWidth;
        this.DummyHeader.ColumnDefinitions[4].Width = GridLength.Auto;

        UpdateColumHeaders();
    }

    // ListBox dummy header Visibility option change.
    private void OnPlaylistHeaderVisibilityChanged(object? sender, System.EventArgs e)
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
        if (vm.IsPlaylistColumnHeaderTimeVisible)
        {
            if (vm.PlaylistColumnHeaderTimeWidth <= 0)
            {
                vm.PlaylistColumnHeaderTimeWidth = 80; // Default width if not set
            }
            this.DummyHeader.ColumnDefinitions[6].Width = new GridLength(vm.PlaylistColumnHeaderTimeWidth);
            this.PlaylistColumn4.IsVisible = true;
            this.PlaylistColumn4x.Width = vm.PlaylistColumnHeaderTimeWidth;
            this.DummyHeader.ColumnDefinitions[6].Width = GridLength.Auto;
        }
        else
        {
            this.PlaylistColumn4.IsVisible = false;
            this.PlaylistColumn4x.Width = 0;
            this.DummyHeader.ColumnDefinitions[6].Width = new GridLength(0);
            this.DummyHeader.ColumnDefinitions[6].Width = GridLength.Auto;
        }

        // Artist
        if (vm.IsPlaylistColumnHeaderArtistVisible)
        {
            if (vm.PlaylistColumnHeaderArtistWidth <= 0)
            {
                vm.PlaylistColumnHeaderArtistWidth = 120; // Default width if not set
            }
            this.DummyHeader.ColumnDefinitions[8].Width = new GridLength(vm.PlaylistColumnHeaderArtistWidth);
            this.PlaylistColumn5.IsVisible = true;
            this.PlaylistColumn5x.Width = vm.PlaylistColumnHeaderArtistWidth;
            this.DummyHeader.ColumnDefinitions[8].Width = GridLength.Auto;
        }
        else
        {
            this.PlaylistColumn5.IsVisible = false;
            this.PlaylistColumn5x.Width = 0;
            this.DummyHeader.ColumnDefinitions[8].Width = new GridLength(0);
            this.DummyHeader.ColumnDefinitions[8].Width = GridLength.Auto;
        }

        // Album
        if (vm.IsPlaylistColumnHeaderAlbumVisible)
        {
            if (vm.PlaylistColumnHeaderAlbumWidth <= 0)
            {
                vm.PlaylistColumnHeaderAlbumWidth = 120; // Default width if not set
            }
            this.DummyHeader.ColumnDefinitions[10].Width = new GridLength(vm.PlaylistColumnHeaderAlbumWidth);
            this.PlaylistColumn6.IsVisible = true;
            this.PlaylistColumn6x.Width = vm.PlaylistColumnHeaderAlbumWidth;
            this.DummyHeader.ColumnDefinitions[10].Width = GridLength.Auto;
        }
        else
        {
            this.PlaylistColumn6.IsVisible = false;
            this.PlaylistColumn6x.Width = 0;
            this.DummyHeader.ColumnDefinitions[10].Width = new GridLength(0);
            this.DummyHeader.ColumnDefinitions[10].Width = GridLength.Auto;
        }

        // Disc
        if (vm.IsPlaylistColumnHeaderDiscVisible)
        {
            if (vm.PlaylistColumnHeaderDiscWidth <= 0)
            {
                vm.PlaylistColumnHeaderDiscWidth = 62; // Default width if not set
            }
            this.DummyHeader.ColumnDefinitions[12].Width = new GridLength(vm.PlaylistColumnHeaderDiscWidth);
            this.PlaylistColumn7.IsVisible = true;
            this.PlaylistColumn7x.Width = vm.PlaylistColumnHeaderDiscWidth;
            this.DummyHeader.ColumnDefinitions[12].Width = GridLength.Auto;
        }
        else
        {
            this.PlaylistColumn7.IsVisible = false;
            this.PlaylistColumn7x.Width = 0;
            this.DummyHeader.ColumnDefinitions[12].Width = new GridLength(0);
            this.DummyHeader.ColumnDefinitions[12].Width = GridLength.Auto;
        }

        // Track
        if (vm.IsPlaylistColumnHeaderTrackVisible)
        {
            if (vm.PlaylistColumnHeaderTrackWidth <= 0)
            {
                vm.PlaylistColumnHeaderTrackWidth = 62; // Default width if not set
            }
            this.DummyHeader.ColumnDefinitions[14].Width = new GridLength(vm.PlaylistColumnHeaderTrackWidth);
            this.PlaylistColumn8.IsVisible = true;
            this.PlaylistColumn8x.Width = vm.PlaylistColumnHeaderTrackWidth;
            this.DummyHeader.ColumnDefinitions[14].Width = GridLength.Auto;
        }
        else
        {
            this.PlaylistColumn8.IsVisible = false;
            this.PlaylistColumn8x.Width = 0;
            this.DummyHeader.ColumnDefinitions[14].Width = new GridLength(0);
            this.DummyHeader.ColumnDefinitions[14].Width = GridLength.Auto;
        }

        // Genre
        if (vm.IsPlaylistColumnHeaderGenreVisible)
        {
            if (vm.PlaylistColumnHeaderGenreWidth <= 0)
            {
                vm.PlaylistColumnHeaderGenreWidth = 80; // Default width if not set
            }
            this.DummyHeader.ColumnDefinitions[16].Width = new GridLength(vm.PlaylistColumnHeaderGenreWidth);//
            this.PlaylistColumn9.IsVisible = true;
            this.PlaylistColumn9x.Width = vm.PlaylistColumnHeaderGenreWidth;
            this.DummyHeader.ColumnDefinitions[16].Width = GridLength.Auto;
        }
        else
        {
            this.PlaylistColumn9.IsVisible = false;
            this.PlaylistColumn9x.Width = 0;
            this.DummyHeader.ColumnDefinitions[16].Width = new GridLength(0);//
            this.DummyHeader.ColumnDefinitions[16].Width = GridLength.Auto;
        }

        // Last modified
        if (vm.IsPlaylistColumnHeaderLastModifiedVisible)
        {
            if (vm.PlaylistColumnHeaderLastModifiedWidth <= 0)
            {
                vm.PlaylistColumnHeaderLastModifiedWidth = 120; // Default width if not set
            }
            this.DummyHeader.ColumnDefinitions[18].Width = new GridLength(vm.PlaylistColumnHeaderLastModifiedWidth);
            this.PlaylistColumn10.IsVisible = true;
            this.PlaylistColumn10x.Width = vm.PlaylistColumnHeaderLastModifiedWidth;
            this.DummyHeader.ColumnDefinitions[18].Width = GridLength.Auto;
        }
        else
        {
            this.PlaylistColumn10.IsVisible = false;
            this.PlaylistColumn10x.Width = 0;
            this.DummyHeader.ColumnDefinitions[18].Width = new GridLength(0);
            this.DummyHeader.ColumnDefinitions[18].Width = GridLength.Auto;
        }
    }

    // Called on window closing to save dummy header sizes.
    public void SavePlaylistItemsHeaderWidth()
    {
        if (DataContext is not MainViewModel vm)
        {
            return;
        }

        if (!_isHeaderWidthInitialized)
        {
            return;
        }

        vm.PlaylistColumnHeaderPositionWidth = this.PlaylistColumn1.Bounds.Size.Width;
        // = this.PlaylistColumn2.Bounds.Size.Width;
        vm.PlaylistColumnHeaderTitleWidth = this.PlaylistColumn3.Bounds.Size.Width;
        vm.PlaylistColumnHeaderTimeWidth = this.PlaylistColumn4.Bounds.Size.Width;
        vm.PlaylistColumnHeaderArtistWidth = this.PlaylistColumn5.Bounds.Size.Width;
        vm.PlaylistColumnHeaderAlbumWidth = this.PlaylistColumn6.Bounds.Size.Width;
        vm.PlaylistColumnHeaderDiscWidth = this.PlaylistColumn7.Bounds.Size.Width;
        vm.PlaylistColumnHeaderTrackWidth = this.PlaylistColumn8.Bounds.Size.Width;
        vm.PlaylistColumnHeaderGenreWidth = this.PlaylistColumn9.Bounds.Size.Width;
        vm.PlaylistColumnHeaderLastModifiedWidth = this.PlaylistColumn10.Bounds.Size.Width;
    }

    // This is a workaround to keep the header in sync with the ListBox scrolling.
    private void ScrollViewer_ScrollChanged(object? sender, Avalonia.Controls.ScrollChangedEventArgs e)
    {
        if (sender is Avalonia.Controls.ScrollViewer sv)
        {
            this.PlaylistItemListViewHeaderScrollViewer.Offset = sv.Offset;
        }
    }

    private async void OnPlaylistRenameToDialogShowAsync(object? sender,string playlist)
    {
        if (DataContext is not MainViewModel vm)
        {
            return;
        }

        var dialog = new ContentDialog
        {
            Title = MPDCtrlX.Properties.Resources.Dialog_Title_NewPlaylistName,
            IsPrimaryButtonEnabled = true,
            PrimaryButtonText = Properties.Resources.Dialog_Ok,
            DefaultButton = ContentDialogButton.Primary,
            IsSecondaryButtonEnabled = false,
            CloseButtonText = Properties.Resources.Dialog_CancelClose,
            Content = new Views.Dialogs.RenameNewPlaylistDialog()
            {
                //DataContext = new DialogViewModel()
            }
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary && dialog.Content is Views.Dialogs.RenameNewPlaylistDialog dlg)
        {
            var plname = dlg.TextBoxPlaylistName.Text;

            if (string.IsNullOrWhiteSpace(plname))
            {
                return;
            }

            if (vm.CheckIfPlaylistExists(plname.Trim()))
            {
                var resultHint = new ContentDialog()
                {
                    Content = MPDCtrlX.Properties.Resources.Dialog_PlaylistNameAlreadyExists,//$"Playlist \"{plname}\" already exists.", //
                    Title = "Result",
                    PrimaryButtonText = "OK"
                };

                _ = resultHint.ShowAsync();

                return;
            }
            else
            {
                vm.PlaylistRenamePlaylist_Execute(playlist, plname.Trim());
            }

        }
    }
}