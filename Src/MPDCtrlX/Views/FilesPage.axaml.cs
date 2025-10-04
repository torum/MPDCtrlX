using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.DependencyInjection;
using MPDCtrlX.Models;
using MPDCtrlX.ViewModels;
using MPDCtrlX.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace MPDCtrlX.Views;

public partial class FilesPage : UserControl
{
    private bool _isHeaderWidthInitialized;

    public FilesPage() { }
    public FilesPage(MainViewModel vm)
    {
        //var vm = App.GetService<MainViewModel>();
        DataContext = vm;

        InitializeComponent();

        vm.FilesHeaderVisibilityChanged += this.OnFilesHeaderVisibilityChanged;

        this.DetachedFromVisualTree += (s, e) =>
        {
            vm.FilesHeaderVisibilityChanged -= this.OnFilesHeaderVisibilityChanged;
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

        UpdateColumHeaders();
    }

    private void OnFilesHeaderVisibilityChanged(object? sender, System.EventArgs e)
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

        // FilePath
        if (vm.IsFilesColumnHeaderFilePathVisible)
        {
            //this.DummyHeader.ColumnDefinitions[2].Width = GridLength.Parse("1*");
        }
        else
        {
            //this.DummyHeader.ColumnDefinitions[2].Width = new GridLength(0);
        }
    }

    // This is a workaround to keep the header in sync with the ListBox scrolling.
    private void ScrollViewer_ScrollChanged(object? sender, Avalonia.Controls.ScrollChangedEventArgs e)
    {
        if (sender is Avalonia.Controls.ScrollViewer sv)
        {
            this.FilesListViewHeaderScrollViewer.Offset = sv.Offset;
        }
    }

    private void ButtonFilesItemsFilter_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        FilesFilterQueryTextBox.Text = string.Empty;

        if (FilesFilterQueryTextBox.IsVisible)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                FilesFilterQueryTextBox.Focus(NavigationMethod.Unspecified, KeyModifiers.None);
            });
        }
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
            vm.IsFilesColumnHeaderFilePathVisible = false;
            //
            this.HeaderGridSpacer.Width = 48;
        }
        else if (e.NewSize.Width < 740)
        {
            vm.IsFilesColumnHeaderFilePathVisible = false;
            //
            this.HeaderGridSpacer.Width = 48;
        }
        else if (e.NewSize.Width < 1008)
        {
            vm.IsFilesColumnHeaderFilePathVisible = true;
            //
            this.HeaderGridSpacer.Width = 24;
        }
        else if (e.NewSize.Width < 1320)
        {
            vm.IsFilesColumnHeaderFilePathVisible = true;
            //
            this.HeaderGridSpacer.Width = 24;
        }
        else if (e.NewSize.Width < 2000)
        {
            vm.IsFilesColumnHeaderFilePathVisible = true;
            //
            this.HeaderGridSpacer.Width = 24;
        }
        else
        {
            vm.IsFilesColumnHeaderFilePathVisible = true;
            //
            this.HeaderGridSpacer.Width = 24;
        }
    }
}