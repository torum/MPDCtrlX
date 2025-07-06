using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using MPDCtrlX.Models;
using MPDCtrlX.ViewModels;
using MPDCtrlX.ViewModels.Dialogs;
using MPDCtrlX.Views.Dialogs;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MPDCtrlX.Views;

public partial class QueuePage : UserControl
{
    private readonly MainViewModel? _viewModel;

    public QueuePage()
    {
        _viewModel = App.GetService<MainViewModel>();

        DataContext = _viewModel;

        InitializeComponent();

        if (_viewModel != null)
        {
            _viewModel.ScrollIntoView += (sender, arg) => { this.OnScrollIntoView(arg); };
            _viewModel.ScrollIntoViewAndSelect += (sender, arg) => { this.OnScrollIntoViewAndSelect(arg); };
            _viewModel.QueueSaveAsDialogShow += this.QueueSaveAsDialogShowAsync;
            _viewModel.QueueSaveToDialogShow += this.QueueSaveToDialogShowAsync;
            _viewModel.QueueHeaderVisivilityChanged += this.OnQueueHeaderVisivilityChanged;
            _viewModel.QueueFindWindowVisivilityChanged_SetFocus += this.OnQueueFindWindowVisivilityChanged_SetFocus;
            //
        }
        /*
        Unloaded += (sender, e) =>
        {
            if (_viewModel != null)
            {
                _viewModel.ScrollIntoView -= (sender, arg) => { this.OnScrollIntoView(arg); };
                _viewModel.ScrollIntoViewAndSelect -= (sender, arg) => { this.OnScrollIntoViewAndSelect(arg); };
                _viewModel.QueueSaveAsDialogShow -= this.QueueSaveAsDialogShowAsync;
                _viewModel.QueueSaveToDialogShow -= this.QueueSaveToDialogShowAsync;
                _viewModel.QueueHeaderVisivilityChanged -= this.OnQueueHeaderVisivilityChanged;
            }
        };
        */
    }

    private void ListBox_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_viewModel == null)
        {
            return;
        }

        // This is a dirty workaround for AvaloniaUI which does not have ListView control at this moment.

        // Position - hidden up/down
        this.DummyHeader.ColumnDefinitions[0].Width = new GridLength(80);//_viewModel.QueueColumnHeaderPositionWidth
        //this.Column1.IsVisible = true;
        this.Column1X.Width = 80;//_viewModel.QueueColumnHeaderPositionWidth
        this.DummyHeader.ColumnDefinitions[0].Width = GridLength.Auto;

        // Title
        this.DummyHeader.ColumnDefinitions[4].Width = new GridLength(_viewModel.QueueColumnHeaderTitleWidth);
        this.Column3.IsVisible = true;
        this.Column3X.Width = _viewModel.QueueColumnHeaderTitleWidth;
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
        if (_viewModel is null)
        {
            return;
        }

        // This is a dirty workaround for AvaloniaUI which does not have ListView control at this moment.

        // Time
        if (_viewModel.IsQueueColumnHeaderTimeVisible)
        {
            if (_viewModel.QueueColumnHeaderTimeWidth <= 0)
            {
                _viewModel.QueueColumnHeaderTimeWidth = 80; // Default width if not set
            }
            this.DummyHeader.ColumnDefinitions[6].Width = new GridLength(_viewModel.QueueColumnHeaderTimeWidth);
            this.Column4.IsVisible = true;
            this.Column4X.Width = _viewModel.QueueColumnHeaderTimeWidth;
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
        if (_viewModel.IsQueueColumnHeaderArtistVisible)
        {
            if (_viewModel.QueueColumnHeaderArtistWidth <= 0)
            {
                _viewModel.QueueColumnHeaderArtistWidth = 120; // Default width if not set
            }
            this.DummyHeader.ColumnDefinitions[8].Width = new GridLength(_viewModel.QueueColumnHeaderArtistWidth);
            this.Column5.IsVisible = true;
            this.Column5X.Width = _viewModel.QueueColumnHeaderArtistWidth;
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
        if (_viewModel.IsQueueColumnHeaderAlbumVisible)
        {
            if (_viewModel.QueueColumnHeaderAlbumWidth <= 0)
            {
                _viewModel.QueueColumnHeaderAlbumWidth = 120; // Default width if not set
            }
            this.DummyHeader.ColumnDefinitions[10].Width = new GridLength(_viewModel.QueueColumnHeaderAlbumWidth);
            this.Column6.IsVisible = true;
            this.Column6X.Width = _viewModel.QueueColumnHeaderAlbumWidth;
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
        if (_viewModel.IsQueueColumnHeaderDiscVisible)
        {
            if (_viewModel.QueueColumnHeaderDiscWidth <= 0)
            {
                _viewModel.QueueColumnHeaderDiscWidth = 50; // Default width if not set
            }
            this.DummyHeader.ColumnDefinitions[12].Width = new GridLength(_viewModel.QueueColumnHeaderDiscWidth);
            this.Column7.IsVisible = true;
            this.Column7X.Width = _viewModel.QueueColumnHeaderDiscWidth;
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
        if (_viewModel.IsQueueColumnHeaderTrackVisible)
        {
            if (_viewModel.QueueColumnHeaderTrackWidth <= 0)
            {
                _viewModel.QueueColumnHeaderTrackWidth = 50; // Default width if not set
            }
            this.DummyHeader.ColumnDefinitions[14].Width = new GridLength(_viewModel.QueueColumnHeaderTrackWidth);
            this.Column8.IsVisible = true;
            this.Column8X.Width = _viewModel.QueueColumnHeaderTrackWidth;
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
        if (_viewModel.IsQueueColumnHeaderGenreVisible)
        {
            if (_viewModel.QueueColumnHeaderGenreWidth <= 0)
            {
                _viewModel.QueueColumnHeaderGenreWidth = 80; // Default width if not set
            }
            this.DummyHeader.ColumnDefinitions[16].Width = new GridLength(_viewModel.QueueColumnHeaderGenreWidth);//
            this.Column9.IsVisible = true;
            this.Column9X.Width = _viewModel.QueueColumnHeaderGenreWidth;
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
        if (_viewModel.IsQueueColumnHeaderLastModifiedVisible)
        {
            if (_viewModel.QueueColumnHeaderLastModifiedWidth <= 0)
            {
                _viewModel.QueueColumnHeaderLastModifiedWidth = 120; // Default width if not set
            }
            this.DummyHeader.ColumnDefinitions[18].Width = new GridLength(_viewModel.QueueColumnHeaderLastModifiedWidth);
            this.Column10.IsVisible = true;
            this.Column10X.Width = _viewModel.QueueColumnHeaderLastModifiedWidth;
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
    public void SaveHeaderWidth()
    {
        if (_viewModel is null)
        {
            return;
        }

        // This is a dirty workaround for AvaloniaUI.
        _viewModel.QueueColumnHeaderPositionWidth = this.Column1.Bounds.Size.Width;
        _viewModel.QueueColumnHeaderNowPlayingWidth = this.Column2.Bounds.Size.Width;
        _viewModel.QueueColumnHeaderTitleWidth = this.Column3.Bounds.Size.Width;
        _viewModel.QueueColumnHeaderTimeWidth = this.Column4.Bounds.Size.Width;
        _viewModel.QueueColumnHeaderArtistWidth = this.Column5.Bounds.Size.Width;
        _viewModel.QueueColumnHeaderAlbumWidth = this.Column6.Bounds.Size.Width;
        _viewModel.QueueColumnHeaderDiscWidth = this.Column7.Bounds.Size.Width;
        _viewModel.QueueColumnHeaderTrackWidth = this.Column8.Bounds.Size.Width;
        _viewModel.QueueColumnHeaderGenreWidth = this.Column9.Bounds.Size.Width;//.Width;
        _viewModel.QueueColumnHeaderLastModifiedWidth = this.Column10.Bounds.Size.Width;//.Width;
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
                var test = _viewModel?.Queue.FirstOrDefault(x => x.IsPlaying == true);
                if (test != null)
                {
                    //lb.ScrollIntoView(test.Index);
                    test.IsSelected = true;
                }
                */
                var test = _viewModel?.Queue.FirstOrDefault(x => x.Index == ind);
                if (test != null)
                {
                    test.IsSelected = true;
                }

                //lb.AutoScrollToSelectedItem = true;
            }
        });
    }

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


    private async void QueueSaveAsDialogShowAsync(object? sender, System.EventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = MPDCtrlX.Properties.Resources.Dialog_Title_NewPlaylistName,
            IsPrimaryButtonEnabled = true,
            PrimaryButtonText = Properties.Resources.Dialog_Ok,
            DefaultButton = ContentDialogButton.Primary,
            IsSecondaryButtonEnabled = false,
            CloseButtonText = Properties.Resources.Dialog_CancelClose,
            Content = new Views.Dialogs.QueueSaveAsDialog()
            {
                //DataContext = new DialogViewModel()
            }
        };

        /* Esc doesn't close... >> turns out MainWindow's keybinding is stealing it. No need for this.
        if ((dialog.Content as Views.Dialogs.QueueSaveAsDialog)?.DataContext is DialogViewModel dv)
        {
            dv.CloseDialogEvent += (s, args) =>
            {
                dialog.Hide();
            };
        }
        */

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            if (dialog.Content is Views.Dialogs.QueueSaveAsDialog dlg)
            {
                var plname = dlg.TextBoxPlaylistName.Text;

                if (string.IsNullOrWhiteSpace(plname))
                {
                    return;
                }
                _viewModel?.QueueSaveAsDialogCommand_Execute(plname.Trim());
            }
        }
    }

    private async void QueueSaveToDialogShowAsync(object? sender, System.EventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = MPDCtrlX.Properties.Resources.Dialog_Title_SelectPlaylist,
            IsPrimaryButtonEnabled = true,
            PrimaryButtonText = Properties.Resources.Dialog_Ok,
            DefaultButton = ContentDialogButton.Primary,
            IsSecondaryButtonEnabled = false,
            CloseButtonText = Properties.Resources.Dialog_CancelClose,
            Content = new Views.Dialogs.QueueSaveToDialog()
            {
                //DataContext = new DialogViewModel()
            }
        };

        if (dialog.Content is QueueSaveToDialog asdf)
        {
            asdf.PlaylistListBox.ItemsSource = _viewModel?.Playlists;
        }

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            if (dialog.Content is Views.Dialogs.QueueSaveToDialog dlg)
            {
                var plselitem = dlg.PlaylistListBox.SelectedItem;

                if (plselitem is Models.Playlist pl)
                {
                    if (string.IsNullOrWhiteSpace(pl.Name))
                    {
                        return;
                    }
                    _viewModel?.QueueSaveToDialogCommand_Execute(pl.Name.Trim());
                }
            }
        }
    }

}