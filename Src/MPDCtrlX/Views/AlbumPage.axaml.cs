using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using FluentAvalonia.Core;
using MPDCtrlX.Models;
using MPDCtrlX.ViewModels;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;

namespace MPDCtrlX.Views;

public partial class AlbumPage : UserControl
{
    //private ScrollViewer? _scrollViewer;
    //private WrapPanel? _wrapPanel;
    public AlbumPage() { }
    public AlbumPage(MainViewModel vm)
    {
        //var vm = App.GetService<MainViewModel>();
        DataContext = vm;

        InitializeComponent();
    }

    private void ListBox_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        //_scrollViewer = AlbumsListBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
        //_wrapPanel = AlbumsListBox.GetVisualDescendants().OfType<WrapPanel>().FirstOrDefault();
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

    private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        /*
        if (_scrollViewer is null)
        {
            return;
        }

        if (_wrapPanel is null)
        {
            return;
        }

        //var viewportRect = new Rect(scrollViewer.Offset, scrollViewer.Viewport);
        var viewportRect = new Rect(new Point(_scrollViewer.Offset.X, _scrollViewer.Offset.Y), _scrollViewer.Viewport);

        var visibleItems = _wrapPanel.Children.Where(child => child.Bounds.Intersects(viewportRect)).ToList();

        foreach (var itemContainer in visibleItems)
        {
            // Get the data item from the container's DataContext
            var dataItem = itemContainer.DataContext;
            // Do something with the item...

            if (dataItem != null)
            {
                if (dataItem is AlbumEx alb)
                {
                    if (alb.IsImageAcquired)
                    {
                        continue;
                    }
                    //Debug.WriteLine(alb.Name);
                }
            }
        }
        */
    }

    private void PageGrid_SizeChanged(object? sender, Avalonia.Controls.SizeChangedEventArgs e)
    {
        if (!e.WidthChanged)
        {
            return;
        }

        var ugrid = this.AlbumsListBox.GetVisualDescendants().OfType<Avalonia.Controls.Primitives.UniformGrid>().FirstOrDefault();

        if (e.NewSize.Width < 340)
        {
            if (ugrid is not null)
            {
                ugrid.Columns = 1;
                //Debug.WriteLine($"{Math.Round(e.NewSize.Width)}  < 340 = {ugrid.Columns}");
            }
            //
            this.HeaderGridSpacer.Width = 48;
        }
        else if (e.NewSize.Width < 740)
        {
            if (ugrid is not null)
            {
                ugrid.Columns = Convert.ToInt32(Math.Round(e.NewSize.Width / 280));
                //Debug.WriteLine($"{Math.Round(e.NewSize.Width)}  < 740 = {ugrid.Columns}");
            }
            //
            this.HeaderGridSpacer.Width = 48;
        }
        else if (e.NewSize.Width < 1008)
        {
            if (ugrid is not null)
            {
                ugrid.Columns = Convert.ToInt32(Math.Round(e.NewSize.Width / 280));
                //Debug.WriteLine($"{Math.Round(e.NewSize.Width)}  < 1008 = {ugrid.Columns}");
            }
            //
            this.HeaderGridSpacer.Width = 12;
        }
        else if (e.NewSize.Width < 1320)
        {
            if (ugrid is not null)
            {
                ugrid.Columns = Convert.ToInt32(Math.Round(e.NewSize.Width / 280));
                //Debug.WriteLine($"{Math.Round(e.NewSize.Width)}  < 1320 = {ugrid.Columns}");
            }
            //
            this.HeaderGridSpacer.Width = 12;
        }
        else if (e.NewSize.Width < 2000)
        {
            if (ugrid is not null)
            {
                ugrid.Columns = Convert.ToInt32(Math.Round(e.NewSize.Width / 280));
                //Debug.WriteLine($"{Math.Round(e.NewSize.Width)}  < 2000 = {ugrid.Columns}");
            }
            //
            this.HeaderGridSpacer.Width = 12;
        }
        else
        {
            if (ugrid is not null)
            {
                ugrid.Columns = Convert.ToInt32(Math.Round(e.NewSize.Width / 280));
                //Debug.WriteLine($"{Math.Round(e.NewSize.Width)} else  = {ugrid.Columns}");
            }
            //
            this.HeaderGridSpacer.Width = 12;
        }
    }
}