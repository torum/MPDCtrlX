using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.DependencyInjection;
using MPDCtrlX.ViewModels;
using System.Collections.Generic;
using System.Diagnostics;

namespace MPDCtrlX.Views;

public partial class PlaylistItemPage : UserControl
{
    private bool _isHeaderWidthInitialized;

    public PlaylistItemPage() { }
    public PlaylistItemPage(MainViewModel vm)
    {
        //var vm = App.GetService<MainViewModel>();
        DataContext = vm;

        InitializeComponent();

        vm.PlaylistHeaderVisibilityChanged += this.OnPlaylistHeaderVisibilityChanged;
        vm.PlaylistRenameToDialogShow += this.OnPlaylistRenameToDialogShowAsync;
        vm.PlaylistHeaderVisibilityChanged += this.OnPlaylistHeaderVisibilityChanged;

        this.DetachedFromVisualTree += (s, e) =>
        {
            vm.PlaylistHeaderVisibilityChanged -= this.OnPlaylistHeaderVisibilityChanged;
            vm.PlaylistRenameToDialogShow -= this.OnPlaylistRenameToDialogShowAsync;
            vm.PlaylistHeaderVisibilityChanged -= this.OnPlaylistHeaderVisibilityChanged;
        };
    }

    private void ListBox_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_isHeaderWidthInitialized)
        {
            // Everytime page is changed back, this loaded is called. So.
            return;
        }

        _isHeaderWidthInitialized = true;

        // This is a dirty workaround for AvaloniaUI which does not have ListView control at this moment.

        // Position - hidden up/down
        this.DummyHeader.ColumnDefinitions[0].Width = new GridLength(80);//vm.PlaylistColumnHeaderPositionWidth
        //this.Column1.IsVisible = true;
        this.PlaylistColumn1.Width = 80;//this.PlaylistColumn1x.Width = 80;//vm.PlaylistColumnHeaderPositionWidth
        this.DummyHeader.ColumnDefinitions[0].Width = GridLength.Auto;
        /*
        // Title
        this.DummyHeader.ColumnDefinitions[4].Width = new GridLength(vm.PlaylistColumnHeaderTitleWidth);
        this.PlaylistColumn3.IsVisible = true;
        this.PlaylistColumn3x.Width = vm.PlaylistColumnHeaderTitleWidth;
        this.DummyHeader.ColumnDefinitions[4].Width = GridLength.Auto;
        */
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

        // Title

        // Time
        if (vm.IsPlaylistColumnHeaderTimeVisible)
        {
            this.DummyHeader.ColumnDefinitions[3].Width = new GridLength(70);
        }
        else
        {
            this.DummyHeader.ColumnDefinitions[3].Width = new GridLength(0);
        }

        // Artist
        if (vm.IsPlaylistColumnHeaderArtistVisible)
        {
            this.DummyHeader.ColumnDefinitions[4].Width = GridLength.Parse("3*");
        }
        else
        {
            this.DummyHeader.ColumnDefinitions[4].Width = new GridLength(0);
        }

        // Album
        if (vm.IsPlaylistColumnHeaderAlbumVisible)
        {
            this.DummyHeader.ColumnDefinitions[5].Width = GridLength.Parse("3*");
        }
        else
        {
            this.DummyHeader.ColumnDefinitions[5].Width = new GridLength(0);
        }

        // Disc
        if (vm.IsPlaylistColumnHeaderDiscVisible)
        {
            this.DummyHeader.ColumnDefinitions[6].Width = new GridLength(65);
        }
        else
        {
            this.DummyHeader.ColumnDefinitions[6].Width = new GridLength(0);
        }

        // Track
        if (vm.IsPlaylistColumnHeaderTrackVisible)
        {
            this.DummyHeader.ColumnDefinitions[7].Width = new GridLength(65);
        }
        else
        {
            this.DummyHeader.ColumnDefinitions[7].Width = new GridLength(0);
        }

        // Genre
        if (vm.IsPlaylistColumnHeaderGenreVisible)
        {
            this.DummyHeader.ColumnDefinitions[8].Width = GridLength.Parse("2*");
        }
        else
        {
            this.DummyHeader.ColumnDefinitions[8].Width = new GridLength(0);
        }

        // Last modified
        if (vm.IsPlaylistColumnHeaderLastModifiedVisible)
        {
            this.DummyHeader.ColumnDefinitions[9].Width = GridLength.Parse("2*");
        }
        else
        {
            this.DummyHeader.ColumnDefinitions[9].Width = new GridLength(0);
        }


        // This is a dirty workaround for AvaloniaUI which does not have ListView control at this moment.
        /*
        // Time
        if (vm.IsPlaylistColumnHeaderTimeVisible)
        {
            if (vm.PlaylistColumnHeaderTimeWidth <= 0)
            {
                //vm.PlaylistColumnHeaderTimeWidth = 80; // Default width if not set
            }
            this.DummyHeader.ColumnDefinitions[6].Width = new GridLength(vm.PlaylistColumnHeaderTimeWidth);
            this.PlaylistColumn4.IsVisible = true;
            this.PlaylistColumn4x.Width = vm.PlaylistColumnHeaderTimeWidth;
            this.DummyHeader.ColumnDefinitions[6].Width = GridLength.Auto;
        }
        else
        {
            //this.PlaylistColumn4.IsVisible = false;
            this.PlaylistColumn4x.Width = 0;
            this.DummyHeader.ColumnDefinitions[6].Width = new GridLength(0);
            this.DummyHeader.ColumnDefinitions[6].Width = GridLength.Auto;
        }

        // Artist
        if (vm.IsPlaylistColumnHeaderArtistVisible)
        {
            if (vm.PlaylistColumnHeaderArtistWidth <= 0)
            {
                //vm.PlaylistColumnHeaderArtistWidth = 120; // Default width if not set
            }
            this.DummyHeader.ColumnDefinitions[8].Width = new GridLength(vm.PlaylistColumnHeaderArtistWidth);
            this.PlaylistColumn5.IsVisible = true;
            this.PlaylistColumn5x.Width = vm.PlaylistColumnHeaderArtistWidth;
            this.DummyHeader.ColumnDefinitions[8].Width = GridLength.Auto;
        }
        else
        {
            //this.PlaylistColumn5.IsVisible = false;
            this.PlaylistColumn5x.Width = 0;
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
            this.DummyHeader.ColumnDefinitions[10].Width = new GridLength(vm.PlaylistColumnHeaderAlbumWidth);
            this.PlaylistColumn6.IsVisible = true;
            this.PlaylistColumn6x.Width = vm.PlaylistColumnHeaderAlbumWidth;
            this.DummyHeader.ColumnDefinitions[10].Width = GridLength.Auto;
        }
        else
        {
            //this.PlaylistColumn6.IsVisible = false;
            this.PlaylistColumn6x.Width = 0;
            this.DummyHeader.ColumnDefinitions[10].Width = new GridLength(0);
            this.DummyHeader.ColumnDefinitions[10].Width = GridLength.Auto;
        }

        // Disc
        if (vm.IsPlaylistColumnHeaderDiscVisible)
        {
            if (vm.PlaylistColumnHeaderDiscWidth <= 0)
            {
                //vm.PlaylistColumnHeaderDiscWidth = 62; // Default width if not set
            }
            this.DummyHeader.ColumnDefinitions[12].Width = new GridLength(vm.PlaylistColumnHeaderDiscWidth);
            this.PlaylistColumn7.IsVisible = true;
            this.PlaylistColumn7x.Width = vm.PlaylistColumnHeaderDiscWidth;
            this.DummyHeader.ColumnDefinitions[12].Width = GridLength.Auto;
        }
        else
        {
            //this.PlaylistColumn7.IsVisible = false;
            this.PlaylistColumn7x.Width = 0;
            this.DummyHeader.ColumnDefinitions[12].Width = new GridLength(0);
            this.DummyHeader.ColumnDefinitions[12].Width = GridLength.Auto;
        }

        // Track
        if (vm.IsPlaylistColumnHeaderTrackVisible)
        {
            if (vm.PlaylistColumnHeaderTrackWidth <= 0)
            {
                //vm.PlaylistColumnHeaderTrackWidth = 62; // Default width if not set
            }
            this.DummyHeader.ColumnDefinitions[14].Width = new GridLength(vm.PlaylistColumnHeaderTrackWidth);
            this.PlaylistColumn8.IsVisible = true;
            this.PlaylistColumn8x.Width = vm.PlaylistColumnHeaderTrackWidth;
            this.DummyHeader.ColumnDefinitions[14].Width = GridLength.Auto;
        }
        else
        {
            //this.PlaylistColumn8.IsVisible = false;
            this.PlaylistColumn8x.Width = 0;
            this.DummyHeader.ColumnDefinitions[14].Width = new GridLength(0);
            this.DummyHeader.ColumnDefinitions[14].Width = GridLength.Auto;
        }

        // Genre
        if (vm.IsPlaylistColumnHeaderGenreVisible)
        {
            if (vm.PlaylistColumnHeaderGenreWidth <= 0)
            {
                //vm.PlaylistColumnHeaderGenreWidth = 80; // Default width if not set
            }
            this.DummyHeader.ColumnDefinitions[16].Width = new GridLength(vm.PlaylistColumnHeaderGenreWidth);//
            this.PlaylistColumn9.IsVisible = true;
            this.PlaylistColumn9x.Width = vm.PlaylistColumnHeaderGenreWidth;
            this.DummyHeader.ColumnDefinitions[16].Width = GridLength.Auto;
        }
        else
        {
            //this.PlaylistColumn9.IsVisible = false;
            this.PlaylistColumn9x.Width = 0;
            this.DummyHeader.ColumnDefinitions[16].Width = new GridLength(0);//
            this.DummyHeader.ColumnDefinitions[16].Width = GridLength.Auto;
        }

        // Last modified
        if (vm.IsPlaylistColumnHeaderLastModifiedVisible)
        {
            if (vm.PlaylistColumnHeaderLastModifiedWidth <= 0)
            {
                //vm.PlaylistColumnHeaderLastModifiedWidth = 120; // Default width if not set
            }
            this.DummyHeader.ColumnDefinitions[18].Width = new GridLength(vm.PlaylistColumnHeaderLastModifiedWidth);
            this.PlaylistColumn10.IsVisible = true;
            this.PlaylistColumn10x.Width = vm.PlaylistColumnHeaderLastModifiedWidth;
            this.DummyHeader.ColumnDefinitions[18].Width = GridLength.Auto;
        }
        else
        {
            //this.PlaylistColumn10.IsVisible = false;
            this.PlaylistColumn10x.Width = 0;
            this.DummyHeader.ColumnDefinitions[18].Width = new GridLength(0);
            this.DummyHeader.ColumnDefinitions[18].Width = GridLength.Auto;
        }
        */
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
            vm.IsPlaylistColumnHeaderTimeVisible = false;
            vm.IsPlaylistColumnHeaderArtistVisible = false;
            vm.IsPlaylistColumnHeaderAlbumVisible = false;
            vm.IsPlaylistColumnHeaderDiscVisible = false;
            vm.IsPlaylistColumnHeaderTrackVisible = false;
            vm.IsPlaylistColumnHeaderGenreVisible = false;
            vm.IsPlaylistColumnHeaderLastModifiedVisible = false;
            //
            this.HeaderGridSpacer.Width = 48;
        }
        else if (e.NewSize.Width < 740)
        {
            vm.IsPlaylistColumnHeaderTimeVisible = true;
            vm.IsPlaylistColumnHeaderArtistVisible = false;
            vm.IsPlaylistColumnHeaderAlbumVisible = false;
            vm.IsPlaylistColumnHeaderDiscVisible = false;
            vm.IsPlaylistColumnHeaderTrackVisible = false;
            vm.IsPlaylistColumnHeaderGenreVisible = false;
            vm.IsPlaylistColumnHeaderLastModifiedVisible = false;
            //
            this.HeaderGridSpacer.Width = 48;
        }
        else if (e.NewSize.Width < 1008)
        {
            vm.IsPlaylistColumnHeaderTimeVisible = true;
            vm.IsPlaylistColumnHeaderArtistVisible = true;
            vm.IsPlaylistColumnHeaderAlbumVisible = true;
            vm.IsPlaylistColumnHeaderDiscVisible = false;
            vm.IsPlaylistColumnHeaderTrackVisible = false;
            vm.IsPlaylistColumnHeaderGenreVisible = false;
            vm.IsPlaylistColumnHeaderLastModifiedVisible = false;
            //
            this.HeaderGridSpacer.Width = 24;
        }
        else if (e.NewSize.Width < 1320)
        {
            vm.IsPlaylistColumnHeaderTimeVisible = true;
            vm.IsPlaylistColumnHeaderArtistVisible = true;
            vm.IsPlaylistColumnHeaderAlbumVisible = true;
            vm.IsPlaylistColumnHeaderDiscVisible = true;
            vm.IsPlaylistColumnHeaderTrackVisible = true;
            vm.IsPlaylistColumnHeaderGenreVisible = true;
            vm.IsPlaylistColumnHeaderLastModifiedVisible = false;
            //
            this.HeaderGridSpacer.Width = 24;
        }
        else if (e.NewSize.Width < 2000)
        {
            vm.IsPlaylistColumnHeaderTimeVisible = true;
            vm.IsPlaylistColumnHeaderArtistVisible = true;
            vm.IsPlaylistColumnHeaderAlbumVisible = true;
            vm.IsPlaylistColumnHeaderDiscVisible = true;
            vm.IsPlaylistColumnHeaderTrackVisible = true;
            vm.IsPlaylistColumnHeaderGenreVisible = true;
            vm.IsPlaylistColumnHeaderLastModifiedVisible = true;
            //
            this.HeaderGridSpacer.Width = 24;
        }
        else
        {
            vm.IsPlaylistColumnHeaderTimeVisible = true;
            vm.IsPlaylistColumnHeaderArtistVisible = true;
            vm.IsPlaylistColumnHeaderAlbumVisible = true;
            vm.IsPlaylistColumnHeaderDiscVisible = true;
            vm.IsPlaylistColumnHeaderTrackVisible = true;
            vm.IsPlaylistColumnHeaderGenreVisible = true;
            vm.IsPlaylistColumnHeaderLastModifiedVisible = true;
            //
            this.HeaderGridSpacer.Width = 24;
        }
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
                    Title = plname,
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