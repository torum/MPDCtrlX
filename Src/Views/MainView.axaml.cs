using Avalonia.Controls;
using MPDCtrlX.ViewModels;
using System.Runtime.InteropServices;

namespace MPDCtrlX.Views;

internal sealed partial class MainView : UserControl
{
    // parameterless constructor for XAML Previewer
    public MainView()
    {
        var vm = App.GetService<MainViewModel>();
        DataContext = vm;

        InitializeComponent();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            //this.PageGrid.Margin = new Avalonia.Thickness(0, 32, 0, 0);
        }
    }
}
