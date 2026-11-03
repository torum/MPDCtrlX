using Avalonia.Controls;
using System;
using FluentAvalonia;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MPDCtrlX.Services.Contracts;

public interface INavigationService
{
    bool NavigateTo(object? selectedPage, object? parameter);
    void Initialize(FluentAvalonia.UI.Controls.FAFrame frame);
    void GoBack();
    bool CanGoBack();
}
