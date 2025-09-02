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

        viewmodel.FilesHeaderVisibilityChanged += this.OnFilesHeaderVisibilityChanged;
        viewmodel.FilesPageAddToPlaylistDialogShow += this.AddToPlaylistDialogShowAsync;
    }

    private void ListBox_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_isHeaderWidthInitialized)
        {
            // Everytime page is changed back, this loaded is called. So.
            return;
        }

        _isHeaderWidthInitialized = true;

        UpdateColumHeaders();
    }

    private void OnFilesHeaderVisibilityChanged(object? sender, System.EventArgs e)
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

        // FilePath
        if (vm.IsFilesColumnHeaderFilePathVisible)
        {
            this.DummyHeader.ColumnDefinitions[2].Width = GridLength.Parse("1*");
        }
        else
        {
            this.DummyHeader.ColumnDefinitions[2].Width = new GridLength(0);
        }
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
            vm.IsFilesColumnHeaderFilePathVisible = false;
            //
            this.HeaderGridSpacer.Width = 48;
        }
        else if (e.NewSize.Width < 740)
        {
            vm.IsFilesColumnHeaderFilePathVisible = false;
            //
            this.HeaderGridSpacer.Width = 48;
        }
        else if (e.NewSize.Width < 1008)
        {
            vm.IsFilesColumnHeaderFilePathVisible = true;
            //
            this.HeaderGridSpacer.Width = 12;
        }
        else if (e.NewSize.Width < 1320)
        {
            vm.IsFilesColumnHeaderFilePathVisible = true;
            //
            this.HeaderGridSpacer.Width = 12;
        }
        else if (e.NewSize.Width < 2000)
        {
            vm.IsFilesColumnHeaderFilePathVisible = true;
            //
            this.HeaderGridSpacer.Width = 12;
        }
        else
        {
            vm.IsFilesColumnHeaderFilePathVisible = true;
            //
            this.HeaderGridSpacer.Width = 12;
        }
    }
}