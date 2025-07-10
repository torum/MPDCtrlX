using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using MPDCtrlX.ViewModels;

namespace MPDCtrlX.Views;

public partial class ArtistPage : UserControl
{
    private readonly MainViewModel? _viewModel;

    public ArtistPage()
    {
        _viewModel = App.GetService<MainViewModel>();
        DataContext = _viewModel;

        InitializeComponent();
    }

    private void ListBox_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_viewModel == null)
        {
            return;
        }

        //this.Artist1x.Width = _viewModel.LibraryColumnHeaderTitleWidth;
        //this.Artist2x.Width = _viewModel.LibraryColumnHeaderFilePathWidth;

    }

    private void ListBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        if (SelectedArtistAlbumsScrollViewer is null)
        {
            return;
        }

        this.SelectedArtistAlbumsScrollViewer.ScrollToHome();
        //DetailsPaneScrollViewer.ChangeView(0,0,1);
    }
}