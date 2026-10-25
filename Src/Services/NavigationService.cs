using MPDCtrlX.Services.Contracts;
using FluentAvalonia;
using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MPDCtrlX.Services;

public class NavigationService : INavigationService
{
    private FluentAvalonia.UI.Controls.FAFrame? _frame;

    public void Initialize(FluentAvalonia.UI.Controls.FAFrame frame, Dictionary<string, Type> pages)
    {
        _frame = frame;
        _pageMap = pages;
    }

    private Dictionary<string, Type>? _pageMap;

    public bool NavigateTo(object? selectedPage, object? parameter)
    {
        if (_frame is null) return false;
        if (_pageMap is null) return false;

        string? tag = null;
        if (selectedPage is FluentAvalonia.UI.Controls.FANavigationViewItem navItem)
        {
            tag = navItem.Tag as string;
        }
        else if (selectedPage is string str)
        {
            tag = str;
        }

        if (tag != null && _pageMap.TryGetValue(tag, out var pageType) && _frame.CurrentSourcePageType != pageType)
        {
            return _frame.Navigate(pageType, parameter);
        }
        else
        {
            Debug.WriteLine("NavigationService.NavigateTo: No valid page found (or navigated to the same page) for " + tag);
            return false;
        }
    }

    public void GoBack()
    {
        if (_frame != null && _frame.CanGoBack)
        {
            _frame.GoBack();
        }
    }

    public bool CanGoBack()
    {
        return _frame != null && _frame.CanGoBack;
    }
}
