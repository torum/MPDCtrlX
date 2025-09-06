using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MPDCtrlX.ViewModels;
using System.Diagnostics;

namespace MPDCtrlX.Views;

public partial class SettingsPage : UserControl
{
    public SettingsPage() { }
    public SettingsPage(MainViewModel vm)
    {
        //var vm = App.GetService<MainViewModel>();
        DataContext = vm;

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