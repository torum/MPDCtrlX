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
    private readonly MainViewModel? vm;

    public PlaylistItemPage() { }

    public PlaylistItemPage(MainViewModel viedwmodel)
    {
        vm = viedwmodel;

        DataContext = vm;

        InitializeComponent();

        vm.PlaylistRenameToDialogShow += this.PlaylistRenameToDialogShowAsync;
    }

    private void ListBox_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (vm == null)
        {
            return;
        }

        this.test1x.Width = vm.QueueColumnHeaderPositionWidth;
        this.test2x.Width = vm.QueueColumnHeaderNowPlayingWidth;
        this.test3x.Width = vm.QueueColumnHeaderTitleWidth;
        this.test4x.Width = vm.QueueColumnHeaderTimeWidth;
        this.test5x.Width = vm.QueueColumnHeaderArtistWidth;
        this.test6x.Width = vm.QueueColumnHeaderAlbumWidth;
        this.test7x.Width = vm.QueueColumnHeaderDiscWidth;
        this.test8x.Width = vm.QueueColumnHeaderTrackWidth;
        this.test9x.Width = vm.QueueColumnHeaderGenreWidth;
        this.test10x.Width = vm.QueueColumnHeaderLastModifiedWidth;

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
        if (vm is null)
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
            Content = new Views.Dialogs.SaveAsDialog()
            {
                //DataContext = new DialogViewModel()
            }
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            if (dialog.Content is Views.Dialogs.SaveAsDialog dlg)
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