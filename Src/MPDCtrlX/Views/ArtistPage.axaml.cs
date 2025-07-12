using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using MPDCtrlX.ViewModels;

namespace MPDCtrlX.Views;

public partial class ArtistPage : UserControl
{
    private readonly MainViewModel? vm;

    public ArtistPage() { }

    public ArtistPage(MainViewModel viewmodel)
    {
        //_viewModel = App.GetService<MainViewModel>();
        vm = viewmodel;

        DataContext = vm;

        InitializeComponent();
    }

    private void ListBox_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (vm == null)
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