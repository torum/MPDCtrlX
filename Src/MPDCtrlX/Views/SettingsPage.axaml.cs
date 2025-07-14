using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Diagnostics;

namespace MPDCtrlX.Views;

public partial class SettingsPage : UserControl
{
    public SettingsPage()
    {
        InitializeComponent();
    }
    /*
    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        ProcessStartInfo psi = new(e.Uri.AbsoluteUri)
        {
            UseShellExecute = true
        };
        Process.Start(psi);
        e.Handled = true;
    }
    */
}