using Avalonia.Controls;
using System;
using FluentAvalonia;
using System.Collections.Generic;

namespace MPDCtrlX.Services.Contracts;

public interface INavigationService
{
    bool NavigateTo(object? selectedPage, object? parameter);
    void Initialize(FluentAvalonia.UI.Controls.FAFrame frame, Dictionary<string, Type> pages);
    void GoBack();
    bool CanGoBack();
}
