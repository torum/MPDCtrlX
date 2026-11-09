using Avalonia.Controls;
using FluentAvalonia;
using MPDCtrlX.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace MPDCtrlX.Services;

public class NavigationService : INavigationService
{
    private FluentAvalonia.UI.Controls.FAFrame? _frame;

    public void Initialize(FluentAvalonia.UI.Controls.FAFrame frame)
    {
        _frame = frame;
    }

    public bool NavigateTo(object? selectedPage, object? parameter)
    {
        if (_frame is null)
        {
            return false;
        }

        string? tag = null;

        if (selectedPage is FluentAvalonia.UI.Controls.FANavigationViewItem navItem)
        {
            tag = navItem.Tag as string;
        }
        else if (selectedPage is string str)
        {
            tag = str;
        }

        Type? pageType = ResolvePageType(tag);

        if (pageType is not null && _frame.CurrentSourcePageType != pageType)
        {
            return _frame.Navigate(pageType, parameter);
        }

        Debug.WriteLine(
            $"NavigationService.NavigateTo: No valid page found (or navigated to the same page) for {tag}");

        return false;
    }

    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public static Type? ResolvePageType(string? tag) => tag switch
    {
        "MPDCtrlX.Views.QueuePage" => typeof(MPDCtrlX.Views.QueuePage),
        "MPDCtrlX.Views.AlbumPage" => typeof(MPDCtrlX.Views.AlbumPage),
        "MPDCtrlX.Views.ArtistPage" => typeof(MPDCtrlX.Views.ArtistPage),
        "MPDCtrlX.Views.FilesPage" => typeof(MPDCtrlX.Views.FilesPage),
        "MPDCtrlX.Views.SearchPage" => typeof(MPDCtrlX.Views.SearchPage),
        "MPDCtrlX.Views.PlaylistItemPage" => typeof(MPDCtrlX.Views.PlaylistItemPage),
        "MPDCtrlX.Views.SettingsPage" => typeof(MPDCtrlX.Views.SettingsPage),
        _ => null
    };

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
