using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using MPDCtrlX.ViewModels;

namespace MPDCtrlX.Views;

public partial class PlaylistItemPage : UserControl
{
    private readonly MainViewModel? _viewModel;

    public PlaylistItemPage()
    {
        _viewModel = (App.Current as App)?.AppHost.Services.GetRequiredService<MainViewModel>();
        DataContext = _viewModel;

        InitializeComponent();
    }

    private void ListBox_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_viewModel == null)
        {
            return;
        }

        this.test1x.Width = _viewModel.QueueColumnHeaderPositionWidth;
        this.test2x.Width = _viewModel.QueueColumnHeaderNowPlayingWidth;
        this.test3x.Width = _viewModel.QueueColumnHeaderTitleWidth;
        this.test4x.Width = _viewModel.QueueColumnHeaderTimeWidth;
        this.test5x.Width = _viewModel.QueueColumnHeaderArtistWidth;
        this.test6x.Width = _viewModel.QueueColumnHeaderAlbumWidth;
        this.test7x.Width = _viewModel.QueueColumnHeaderDiscWidth;
        this.test8x.Width = _viewModel.QueueColumnHeaderTrackWidth;
        this.test9x.Width = _viewModel.QueueColumnHeaderGenreWidth;
        this.test10x.Width = _viewModel.QueueColumnHeaderLastModifiedWidth;

    }

    // This is a workaround to keep the header in sync with the ListBox scrolling.
    private void ScrollViewer_ScrollChanged(object? sender, Avalonia.Controls.ScrollChangedEventArgs e)
    {
        if (sender is Avalonia.Controls.ScrollViewer sv)
        {
            this.PlaylistItemListViewHeaderScrollViewer.Offset = sv.Offset;
        }
    }
}