using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.Core;
using MPDCtrlX.Models;
using MPDCtrlX.ViewModels;
using System;
using System.Diagnostics;

namespace MPDCtrlX.Views;

public partial class AlbumPage : UserControl
{
    public AlbumPage() { }

    public AlbumPage(MainViewModel viewmodel)
    {
        DataContext = viewmodel;

        InitializeComponent();
    }

    private void ListBox_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        //if (DataContext is not MainViewModel vm)
        //{
        //    return;
        //}

        //this.Artist1x.Width = _viewModel.LibraryColumnHeaderTitleWidth;
        //this.Artist2x.Width = _viewModel.LibraryColumnHeaderFilePathWidth;

        /*
        var realizedContainers = icAlbums.GetRealizedContainers();
        if (realizedContainers != null)
        {
            Debug.WriteLine($"Realized containers not null.");
            foreach (var container in realizedContainers)
            {
                var dataContext = container.DataContext;

                if (dataContext is Album album)
                {
                    Debug.WriteLine($"Album Name: {album.Name}");
                }
                else
                {
                    Debug.WriteLine($"DataContext is not of type Album: {dataContext?.GetType().Name}");
                }
            }
        }
        else
        {
             Debug.WriteLine("No realized containers found.");
        }
        */
    }

    private void ListBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        if (AlbumSongsListBox is null)
        {
            return;
        }

        if (AlbumSongsListBox.ItemsSource is null)
        {
            return;
        }

        if (AlbumSongsListBox.ItemsSource.Count() > 0)
        {
            AlbumSongsListBox.ScrollIntoView(0);
        }
    }
}