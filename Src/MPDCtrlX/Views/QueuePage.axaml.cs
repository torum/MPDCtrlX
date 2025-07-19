using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using MPDCtrlX.Models;
using MPDCtrlX.ViewModels;
using MPDCtrlX.ViewModels.Dialogs;
using MPDCtrlX.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace MPDCtrlX.Views;

public partial class QueuePage : UserControl
{
    private readonly MainViewModel? vm;

    private bool _isHeaderWidthInitialized;

    public QueuePage() { }

    public QueuePage(MainViewModel viewmodel)
    {
        //vm = App.GetService<MainViewModel>();
        vm = viewmodel;

        DataContext = vm;

        InitializeComponent();

        vm.ScrollIntoView += (sender, arg) => { this.OnScrollIntoView(arg); };
        vm.ScrollIntoViewAndSelect += (sender, arg) => { this.OnScrollIntoViewAndSelect(arg); };
        vm.QueueSaveToDialogShow += this.QueueSaveToDialogShowAsync;
        vm.QueueListviewSaveToDialogShow += this.QueueListviewSaveToDialogShowAsync;
        vm.QueueHeaderVisivilityChanged += this.OnQueueHeaderVisivilityChanged;
        vm.QueueFindWindowVisivilityChanged_SetFocus += this.OnQueueFindWindowVisivilityChanged_SetFocus;
        /*
        Unloaded += (sender, e) =>
        {
            if (vm != null)
            {
                vm.ScrollIntoView -= (sender, arg) => { this.OnScrollIntoView(arg); };
                vm.ScrollIntoViewAndSelect -= (sender, arg) => { this.OnScrollIntoViewAndSelect(arg); };
                vm.QueueSaveAsDialogShow -= this.QueueSaveAsDialogShowAsync;
                vm.QueueSaveToDialogShow -= this.QueueSaveToDialogShowAsync;
                vm.QueueHeaderVisivilityChanged -= this.OnQueueHeaderVisivilityChanged;
            }
        };
        */
    }

    private void ListBox_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (vm == null)
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
        this.Column1X.Width = 80;//vm.QueueColumnHeaderPositionWidth
        this.DummyHeader.ColumnDefinitions[0].Width = GridLength.Auto;

        // Title
        this.DummyHeader.ColumnDefinitions[4].Width = new GridLength(vm.QueueColumnHeaderTitleWidth);
        this.Column3.IsVisible = true;
        this.Column3X.Width = vm.QueueColumnHeaderTitleWidth;
        this.DummyHeader.ColumnDefinitions[4].Width = GridLength.Auto;

        UpdateColumHeaders();
    }

    // ListBox dummy header visivility option change.
    private void OnQueueHeaderVisivilityChanged(object? sender, System.EventArgs e)
    {
        UpdateColumHeaders();
    }

    private void UpdateColumHeaders()
    {
        if (vm is null)
        {
            return;
        }

        // This is a dirty workaround for AvaloniaUI which does not have ListView control at this moment.

        // Time
        if (vm.IsQueueColumnHeaderTimeVisible)
        {
            if (vm.QueueColumnHeaderTimeWidth <= 0)
            {
                vm.QueueColumnHeaderTimeWidth = 80; // Default width if not set
            }
            this.DummyHeader.ColumnDefinitions[6].Width = new GridLength(vm.QueueColumnHeaderTimeWidth);
            this.Column4.IsVisible = true;
            this.Column4X.Width = vm.QueueColumnHeaderTimeWidth;
            this.DummyHeader.ColumnDefinitions[6].Width = GridLength.Auto;
        }
        else
        {
            this.Column4.IsVisible = false;
            this.Column4X.Width = 0;
            this.DummyHeader.ColumnDefinitions[6].Width = new GridLength(0);
            this.DummyHeader.ColumnDefinitions[6].Width = GridLength.Auto;
        }

        // Artist
        if (vm.IsQueueColumnHeaderArtistVisible)
        {
            if (vm.QueueColumnHeaderArtistWidth <= 0)
            {
                vm.QueueColumnHeaderArtistWidth = 120; // Default width if not set
            }
            this.DummyHeader.ColumnDefinitions[8].Width = new GridLength(vm.QueueColumnHeaderArtistWidth);
            this.Column5.IsVisible = true;
            this.Column5X.Width = vm.QueueColumnHeaderArtistWidth;
            this.DummyHeader.ColumnDefinitions[8].Width = GridLength.Auto;
        }
        else
        {
            this.Column5.IsVisible = false;
            this.Column5X.Width = 0;
            this.DummyHeader.ColumnDefinitions[8].Width = new GridLength(0);
            this.DummyHeader.ColumnDefinitions[8].Width = GridLength.Auto;
        }

        // Album
        if (vm.IsQueueColumnHeaderAlbumVisible)
        {
            if (vm.QueueColumnHeaderAlbumWidth <= 0)
            {
                vm.QueueColumnHeaderAlbumWidth = 120; // Default width if not set
            }
            this.DummyHeader.ColumnDefinitions[10].Width = new GridLength(vm.QueueColumnHeaderAlbumWidth);
            this.Column6.IsVisible = true;
            this.Column6X.Width = vm.QueueColumnHeaderAlbumWidth;
            this.DummyHeader.ColumnDefinitions[10].Width = GridLength.Auto;
        }
        else
        {
            this.Column6.IsVisible = false;
            this.Column6X.Width = 0;
            this.DummyHeader.ColumnDefinitions[10].Width = new GridLength(0);
            this.DummyHeader.ColumnDefinitions[10].Width = GridLength.Auto;
        }

        // Disc
        if (vm.IsQueueColumnHeaderDiscVisible)
        {
            if (vm.QueueColumnHeaderDiscWidth <= 0)
            {
                vm.QueueColumnHeaderDiscWidth = 50; // Default width if not set
            }
            this.DummyHeader.ColumnDefinitions[12].Width = new GridLength(vm.QueueColumnHeaderDiscWidth);
            this.Column7.IsVisible = true;
            this.Column7X.Width = vm.QueueColumnHeaderDiscWidth;
            this.DummyHeader.ColumnDefinitions[12].Width = GridLength.Auto;
        }
        else
        {
            this.Column7.IsVisible = false;
            this.Column7X.Width = 0;
            this.DummyHeader.ColumnDefinitions[12].Width = new GridLength(0);
            this.DummyHeader.ColumnDefinitions[12].Width = GridLength.Auto;
        }

        // Track
        if (vm.IsQueueColumnHeaderTrackVisible)
        {
            if (vm.QueueColumnHeaderTrackWidth <= 0)
            {
                vm.QueueColumnHeaderTrackWidth = 50; // Default width if not set
            }
            this.DummyHeader.ColumnDefinitions[14].Width = new GridLength(vm.QueueColumnHeaderTrackWidth);
            this.Column8.IsVisible = true;
            this.Column8X.Width = vm.QueueColumnHeaderTrackWidth;
            this.DummyHeader.ColumnDefinitions[14].Width = GridLength.Auto;
        }
        else
        {
            this.Column8.IsVisible = false;
            this.Column8X.Width = 0;
            this.DummyHeader.ColumnDefinitions[14].Width = new GridLength(0);
            this.DummyHeader.ColumnDefinitions[14].Width = GridLength.Auto;
        }

        // Genre
        if (vm.IsQueueColumnHeaderGenreVisible)
        {
            if (vm.QueueColumnHeaderGenreWidth <= 0)
            {
                vm.QueueColumnHeaderGenreWidth = 80; // Default width if not set
            }
            this.DummyHeader.ColumnDefinitions[16].Width = new GridLength(vm.QueueColumnHeaderGenreWidth);//
            this.Column9.IsVisible = true;
            this.Column9X.Width = vm.QueueColumnHeaderGenreWidth;
            this.DummyHeader.ColumnDefinitions[16].Width = GridLength.Auto;
        }
        else
        {
            this.Column9.IsVisible = false;
            this.Column9X.Width = 0;
            this.DummyHeader.ColumnDefinitions[16].Width = new GridLength(0);//
            this.DummyHeader.ColumnDefinitions[16].Width = GridLength.Auto;
        }

        // Last modified
        if (vm.IsQueueColumnHeaderLastModifiedVisible)
        {
            if (vm.QueueColumnHeaderLastModifiedWidth <= 0)
            {
                vm.QueueColumnHeaderLastModifiedWidth = 120; // Default width if not set
            }
            this.DummyHeader.ColumnDefinitions[18].Width = new GridLength(vm.QueueColumnHeaderLastModifiedWidth);
            this.Column10.IsVisible = true;
            this.Column10X.Width = vm.QueueColumnHeaderLastModifiedWidth;
            this.DummyHeader.ColumnDefinitions[18].Width = GridLength.Auto;
        }
        else
        {
            this.Column10.IsVisible = false;
            this.Column10X.Width = 0;
            this.DummyHeader.ColumnDefinitions[18].Width = new GridLength(0);
            this.DummyHeader.ColumnDefinitions[18].Width = GridLength.Auto;
        }
    }

    // Called on window closing to save dummy header sizes.
    public void SaveQueueHeaderWidth()
    {
        if (vm is null)
        {
            return;
        }

        if (!_isHeaderWidthInitialized)
        {
            return;
        }

        // This is a dirty workaround for AvaloniaUI.
        vm.QueueColumnHeaderPositionWidth = this.Column1.Bounds.Size.Width;
        vm.QueueColumnHeaderNowPlayingWidth = this.Column2.Bounds.Size.Width;
        vm.QueueColumnHeaderTitleWidth = this.Column3.Bounds.Size.Width;
        vm.QueueColumnHeaderTimeWidth = this.Column4.Bounds.Size.Width;
        vm.QueueColumnHeaderArtistWidth = this.Column5.Bounds.Size.Width;
        vm.QueueColumnHeaderAlbumWidth = this.Column6.Bounds.Size.Width;
        vm.QueueColumnHeaderDiscWidth = this.Column7.Bounds.Size.Width;
        vm.QueueColumnHeaderTrackWidth = this.Column8.Bounds.Size.Width;
        vm.QueueColumnHeaderGenreWidth = this.Column9.Bounds.Size.Width;//.Width;
        vm.QueueColumnHeaderLastModifiedWidth = this.Column10.Bounds.Size.Width;//.Width;
    }

    private async void OnScrollIntoView(int ind)
    {
        await Task.Yield();
        await Task.Delay(100); // Wait for UI to update
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
        await Task.Delay(800); // Need to wait for UI to update
        Dispatcher.UIThread.Post(() =>
        {
            if (this.QueueListBox is ListBox lb)
            {
                lb.ScrollIntoView(ind);
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
            if (MainViewModel.QueueListviewEnterKeyCommand_CanExecute())
            {
                vm.QueueListviewEnterKeyCommand_ExecuteAsync();
            }
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
    private async void OnQueueFindWindowVisivilityChanged_SetFocus(object? sender, System.EventArgs e)
    {
        await Task.Yield();
        await Task.Delay(50); // Need to wait for UI to update
        if (this.TglButtonQueueFilter is ToggleButton tb)
        {
            if (tb.IsChecked == true)
            {
                this.FilterQueueQueryTextBox.Focus();
            }
        }
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

    private async void QueueSaveToDialogShowAsync(object? sender, System.EventArgs e)
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

            //asdf.PlaylistListBox.ItemsSource = vm?.Playlists;
            //asdf.PlaylistListBox.ItemsSource = new ObservableCollection<Playlist>(vm.Playlists.OrderBy(x => x.Name, comp));
            asdf.PlaylistComboBox.ItemsSource = new ObservableCollection<Playlist>(vm.Playlists.OrderBy(x => x.Name, comp));
        }

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            if (dialog.Content is Views.Dialogs.SaveToDialog dlg)
            {

                if (dlg.CreateNewCheckBox.IsChecked is true)
                {
                    var str = dlg.TextBoxPlaylistName.Text ?? string.Empty;

                    if (!string.IsNullOrEmpty(str.Trim()))
                    {
                        vm?.QueueSaveToDialog_Execute(str.Trim());
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

                        vm?.QueueSaveToDialog_Execute(pl.Name.Trim());
                    }
                }
            }
        }
    }

    private async void QueueListviewSaveToDialogShowAsync(object? sender, List<string> list)
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