using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.DependencyInjection;
using MPDCtrlX.Models;
using MPDCtrlX.ViewModels;
using MPDCtrlX.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace MPDCtrlX.Views;

public partial class FilesPage : UserControl
{
    private bool _isHeaderWidthInitialized;

    public FilesPage()
    {
        var viewmodel = App.GetService<MainViewModel>();
        DataContext = viewmodel;

        InitializeComponent();

        viewmodel.FilesPageAddToPlaylistDialogShow += this.AddToPlaylistDialogShowAsync;

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

        this.Library1x.Width = vm.LibraryColumnHeaderTitleWidth;
        this.Library2x.Width = vm.LibraryColumnHeaderFilePathWidth;
    }

    // Called on window closing to save dummy header sizes.
    public void SaveFilesHeaderWidth()
    {
        if (DataContext is not MainViewModel vm)
        {
            return;
        }

        if (!_isHeaderWidthInitialized)
        {
            return;
        }

        vm.LibraryColumnHeaderTitleWidth = this.Library1.Bounds.Size.Width;
        vm.LibraryColumnHeaderFilePathWidth = this.Library2.Bounds.Size.Width;
    }

    // This is a workaround to keep the header in sync with the ListBox scrolling.
    private void ScrollViewer_ScrollChanged(object? sender, Avalonia.Controls.ScrollChangedEventArgs e)
    {
        if (sender is Avalonia.Controls.ScrollViewer sv)
        {
            this.FilesListViewHeaderScrollViewer.Offset = sv.Offset;
        }
    }

    private async void AddToPlaylistDialogShowAsync(object? sender, List<string> list)
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

    private void ButtonFilesItemsFilter_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        FilesFilterQueryTextBox.Text = string.Empty;

        if (FilesFilterQueryTextBox.IsVisible)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                FilesFilterQueryTextBox.Focus(NavigationMethod.Unspecified, KeyModifiers.None);
            });
        }
    }
}