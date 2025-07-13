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
    private readonly MainViewModel? vm;

    public SearchPage() { }
    public SearchPage(MainViewModel viewmodel)
    {
        vm = viewmodel;
        
        DataContext = vm;

        InitializeComponent();

        vm.SearchPageAddToPlaylistDialogShow += this.AddToPlaylistDialogShowAsync;
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
            this.SearchListViewHeaderScrollViewer.Offset = sv.Offset;
        }
    }



    private async void AddToPlaylistDialogShowAsync(object? sender, List<string> list)
    {
        if (vm is null)
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

        if (result == ContentDialogResult.Primary)
        {
            if (dialog.Content is Views.Dialogs.SaveToDialog dlg)
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
}