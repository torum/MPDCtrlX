using FluentAvalonia.UI.Windowing;

namespace MPDCtrlX.Views.Dialogs;

internal sealed partial class InitWindow : FAAppWindow
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