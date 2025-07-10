using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using MPDCtrlX.ViewModels;

namespace MPDCtrlX.Views;

public partial class FilesPage : UserControl
{
    private readonly MainViewModel? vm;

    public FilesPage() { }

    public FilesPage(MainViewModel viewmodel)
    {
        vm = viewmodel;
        //_viewModel = App.GetService<MainViewModel>();

        DataContext = vm;

        InitializeComponent();
    }

    private void ListBox_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (vm == null)
        {
            return;
        }

        this.Library1x.Width = vm.LibraryColumnHeaderTitleWidth;
        this.Library2x.Width = vm.LibraryColumnHeaderFilePathWidth;

    }

    // This is a workaround to keep the header in sync with the ListBox scrolling.
    private void ScrollViewer_ScrollChanged(object? sender, Avalonia.Controls.ScrollChangedEventArgs e)
    {
        if (sender is Avalonia.Controls.ScrollViewer sv)
        {
            this.FilesListViewHeaderScrollViewer.Offset = sv.Offset;
        }
    }
}