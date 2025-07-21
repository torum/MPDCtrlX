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

    public PlaylistItemPage(MainViewModel viedwmodel)
    {
        DataContext = viedwmodel;

        InitializeComponent();

        viedwmodel.PlaylistRenameToDialogShow += this.PlaylistRenameToDialogShowAsync;
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

    private async void PlaylistRenameToDialogShowAsync(object? sender,string playlist)
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

        if (result == ContentDialogResult.Primary)
        {
            if (dialog.Content is Views.Dialogs.RenameNewPlaylistDialog dlg)
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
                        Content = $"Playlist \"{plname}\" already exists.",
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
}