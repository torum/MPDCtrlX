using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using System.Threading.Tasks;
using System;
using FluentAvalonia.UI.Windowing;

namespace MPDCtrlX.Views.Dialogs;

public partial class InitWindow : AppWindow
{
    public InitWindow()
    {
        InitializeComponent();
        CanResize = false;
        ShowAsDialog = true;
        TitleBar.Height = 0;
    }

    private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.Close();
    }
}