using Avalonia.Controls;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
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
        }

        Unloaded += (sender, e) =>
        {
            if (_viewModel != null)
            {
                _viewModel.ScrollIntoView -= (sender, arg) => { this.OnScrollIntoView(arg); };
                _viewModel.ScrollIntoViewAndSelect -= (sender, arg) => { this.OnScrollIntoViewAndSelect(arg); };
                _viewModel.QueueSaveAsDialogShow -= this.QueueSaveAsDialogShowAsync;
                _viewModel.QueueSaveToDialogShow -= this.QueueSaveToDialogShowAsync;
            }
        };

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

    public void UpdateHeaderWidth()
    {
        if (_viewModel != null)
        {
            // This is a dirty workaround for AvaloniaUI.
            _viewModel.QueueColumnHeaderPositionWidth = this.Column1X.Width;
            _viewModel.QueueColumnHeaderNowPlayingWidth = this.Column2X.Width;
            _viewModel.QueueColumnHeaderTitleWidth = this.Column3X.Width;
            _viewModel.QueueColumnHeaderTimeWidth = this.Column4X.Width;
            _viewModel.QueueColumnHeaderArtistWidth = this.Column5X.Width;
            _viewModel.QueueColumnHeaderAlbumWidth = this.Column6X.Width;
            _viewModel.QueueColumnHeaderDiscWidth = this.Column7X.Width;
            _viewModel.QueueColumnHeaderTrackWidth = this.Column8X.Width;
            _viewModel.QueueColumnHeaderGenreWidth = this.Column9X.Width;
            _viewModel.QueueColumnHeaderLastModifiedWidth = this.Column10X.Width;
        }
    }

    private void ListBox_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_viewModel == null)
        {
            return;
        }

        // This is a dirty workaround for AvaloniaUI.
        if (_viewModel.QueueColumnHeaderPositionWidth > 10)
        {
            this.Column1X.Width = _viewModel.QueueColumnHeaderPositionWidth;
        }
        else
        {
            this.Column1X.Width = 50; // Default width if not set
        }

        if (_viewModel.QueueColumnHeaderNowPlayingWidth > 10)
        {
            //this.Column2X.Width = _viewModel.QueueColumnHeaderNowPlayingWidth;
        }
        else
        {
            //this.Column2X.Width = 50; // Default width if not set
        }

        if (_viewModel.QueueColumnHeaderTitleWidth > 10)
        {
            this.Column3.Width = _viewModel.QueueColumnHeaderTitleWidth;
        }
        else
        {
            this.Column3X.Width = 50; // Default width if not set
        }

        if (_viewModel.QueueColumnHeaderTimeWidth > 10)
        {
            this.Column4X.Width = _viewModel.QueueColumnHeaderTimeWidth;
        }
        else
        {
            this.Column4X.Width = 50; // Default width if not set
        }

        if (_viewModel.QueueColumnHeaderArtistWidth > 10)
        {
            this.Column5X.Width = _viewModel.QueueColumnHeaderArtistWidth;
        }
        else
        {
            this.Column5X.Width = 50; // Default width if not set
        }

        if (_viewModel.QueueColumnHeaderAlbumWidth > 10)
        {
            this.Column6X.Width = _viewModel.QueueColumnHeaderAlbumWidth;
        }
        else
        {
            this.Column6X.Width = 50; // Default width if not set
        }

        if (_viewModel.QueueColumnHeaderDiscWidth > 10)
        {
            this.Column7X.Width = _viewModel.QueueColumnHeaderDiscWidth;
        }
        else
        {
            this.Column7X.Width = 50; // Default width if not set
        }

        if (_viewModel.QueueColumnHeaderTrackWidth > 10)
        {
            this.Column8X.Width = _viewModel.QueueColumnHeaderTrackWidth;
        }
        else
        {
            this.Column8X.Width = 50; // Default width if not set
        }

        if (_viewModel.QueueColumnHeaderGenreWidth > 10)
        {
            this.Column9X.Width = _viewModel.QueueColumnHeaderGenreWidth;
        }
        else
        {
            this.Column9X.Width = 50; // Default width if not set
        }

        if (_viewModel.QueueColumnHeaderLastModifiedWidth > 10)
        {
            this.Column10X.Width = _viewModel.QueueColumnHeaderLastModifiedWidth;
        }
        else
        {
            this.Column10X.Width = 50; // Default width if not set
        }

    }

    private async void OnScrollIntoView(int ind)
    {
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
        await Task.Delay(1000); // Wait for UI to update
        Dispatcher.UIThread.Post(() =>
        {
            if (this.QueueListBox is ListBox lb)
            {
                lb.ScrollIntoView(ind);

                var test = _viewModel?.Queue.FirstOrDefault(x => x.IsPlaying == true);
                if (test != null)
                {
                    //lb.ScrollIntoView(test.Index);
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

    // This is a workaround to keep the header in sync with the ListBox scrolling.
    private void ScrollViewer_ScrollChanged(object? sender, Avalonia.Controls.ScrollChangedEventArgs e)
    {
        if (sender is Avalonia.Controls.ScrollViewer sv)
        {
            this.QueueListViewHeaderScrollViewer.Offset = sv.Offset;
        }
    }

}