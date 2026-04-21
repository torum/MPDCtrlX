using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FluentAvalonia.Core;
using MPDCtrlX.Models;
using MPDCtrlX.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

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

        vm.AlbumsCollectionHasBeenReset += this.OnAlbumsCollectionHasBeenReset;
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

        if (AlbumSongsListBox.ItemCount > 0)
        {
            AlbumSongsListBox.ScrollIntoView(0);
        }
        /*
        var vm = App.GetService<MainViewModel>();
        if (AlbumsListBox.SelectedItem is AlbumEx alb)
        {
            Dispatcher.UIThread.Post(async () =>
            {
                await vm.AlbumsItemInvoked(alb);
            }, DispatcherPriority.Normal);
        }
        */
        /*
        if (AlbumsListBox.SelectedItem is not null)
        {
            var vm = App.GetService<MainViewModel>();
            _ = Task.Run(async () =>
            {
                await vm.AlbumsItemInvoked(AlbumsListBox.SelectedItem);

                vm.IsAlbumContentPanelVisible = true;
            });
        }
        */
    }

    public void OnAlbumsCollectionHasBeenReset(object? sender, System.EventArgs e)
    {
        if (AlbumsListBox is null)
        {
            return;
        }

        if (AlbumsListBox.ItemsSource is null)
        {
            return;
        }

        if (AlbumsListBox.ItemCount > 0)
        {
            AlbumsListBox.ScrollIntoView(0);
        }

        Dispatcher.UIThread.Post(async () =>
        {
            var scrollViewer = AlbumsListBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();

            var wrapPanel = AlbumsListBox.GetVisualDescendants().OfType<Avalonia.Controls.Primitives.UniformGrid>().FirstOrDefault();

            if ((scrollViewer != null) && (wrapPanel != null))
            {
                UpdateVisibleItems(AlbumsListBox, scrollViewer, wrapPanel);
            }
        }, DispatcherPriority.Render);
    }

    private static void UpdateVisibleItems(ListBox listBox, ScrollViewer scrollViewer, Avalonia.Controls.Primitives.UniformGrid uniformGridPanel)
    {
        //var _scrollViewer = listBox.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
        if (scrollViewer == null) return;

        //var _wrapPanel = listBox.GetVisualDescendants().OfType<WrapPanel>().FirstOrDefault();
        if (uniformGridPanel == null) return;

        var viewportRect = new Rect(new Point(scrollViewer.Offset.X, scrollViewer.Offset.Y), scrollViewer.Viewport);

        var visibleObjects = uniformGridPanel.Children.Where(child => child.Bounds.Intersects(viewportRect)).ToList();

        var visibleItems = new List<object>();

        foreach (var itemContainer in visibleObjects)
        {
            var dataItem = itemContainer.DataContext;
            if (dataItem != null)
            {
                visibleItems.Add(dataItem);
            }
        }

        var vm = App.GetService<MainViewModel>();
        vm.VisibleViewportItemsAlbumEx = visibleItems;
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
            this.HeaderGridSpacer.Width = 24;
        }
        else if (e.NewSize.Width < 1320)
        {
            if (ugrid is not null)
            {
                ugrid.Columns = Convert.ToInt32(Math.Round(e.NewSize.Width / 280));
                //Debug.WriteLine($"{Math.Round(e.NewSize.Width)}  < 1320 = {ugrid.Columns}");
            }
            //
            this.HeaderGridSpacer.Width = 24;
        }
        else if (e.NewSize.Width < 2000)
        {
            if (ugrid is not null)
            {
                ugrid.Columns = Convert.ToInt32(Math.Round(e.NewSize.Width / 280));
                //Debug.WriteLine($"{Math.Round(e.NewSize.Width)}  < 2000 = {ugrid.Columns}");
            }
            //
            this.HeaderGridSpacer.Width = 24;
        }
        else
        {
            if (ugrid is not null)
            {
                ugrid.Columns = Convert.ToInt32(Math.Round(e.NewSize.Width / 280));
                //Debug.WriteLine($"{Math.Round(e.NewSize.Width)} else  = {ugrid.Columns}");
            }
            //
            this.HeaderGridSpacer.Width = 24;
        }
    }

    private void TglButtonAlbumFilter_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Dispatcher.UIThread.Post(async () =>
        {
            await Task.Yield(); // Ensure the UI has processed the opened event

            if (this.TglButtonAlbumFilter is ToggleButton tb)
            {
                if (tb.IsChecked == true)
                {
                    if (FilterAlbumQueryTextBox.Focusable)
                    {
                        this.FilterAlbumQueryTextBox.Focus();
                    }
                }
            }

        }, DispatcherPriority.Render);
    }

    private void FilterAlbumListBox_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        if (this.FilterAlbumListBox.SelectedItem is AlbumEx album)
        {
            if (this.AlbumsListBox is ListBox lb)
            {
                var vm = App.GetService<MainViewModel>();
                if (vm is not null)
                {
                    vm.AlbumFilterSelect(album);
                }
            }
        }
    }

    private void Page_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key == Avalonia.Input.Key.Escape)
        {
            TglButtonAlbumFilter.IsChecked = false;
        }
    }

    private void FilterAlbumPopup_Opened(object? sender, System.EventArgs e)
    {
        Dispatcher.UIThread.Post(async () =>
        {
            await Task.Yield(); // Ensure the UI has processed the opened event
            if (FilterAlbumPopup.Focusable)
            {
                FilterAlbumPopup.Focus();
            }

            if (FilterAlbumQueryTextBox.Focusable)
            {
                this.FilterAlbumQueryTextBox.Focus();
            }
        }, DispatcherPriority.Render);
    }
}