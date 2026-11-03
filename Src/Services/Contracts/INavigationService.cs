using Avalonia.Controls;
using System;
using FluentAvalonia;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MPDCtrlX.Services.Contracts;

[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
public delegate System.Type NavigationPageType();

public interface INavigationService
{
    bool NavigateTo(object? selectedPage, object? parameter);
    void Initialize(FluentAvalonia.UI.Controls.FAFrame frame, Dictionary<string, NavigationPageType> pages);
    void GoBack();
    bool CanGoBack();
}
