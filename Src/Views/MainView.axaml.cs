using Avalonia.Controls;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using MPDCtrlX.Models;
using MPDCtrlX.ViewModels;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace MPDCtrlX.Views;

public partial class MainView : UserControl
{
    // Optional parameterless constructor for XAML Previewer
    public MainView() { InitializeComponent(); }
    public MainView(MainViewModel vm)
    {
        //var vm = App.GetService<MainViewModel>();
        DataContext = vm;

        InitializeComponent();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            //this.PageGrid.Margin = new Avalonia.Thickness(0, 32, 0, 0);
        }
    }
}
