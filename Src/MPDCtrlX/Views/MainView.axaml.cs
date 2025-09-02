using Avalonia.Controls;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using MPDCtrlX.Models;
using MPDCtrlX.ViewModels;
using System;
using System.Diagnostics;
using System.Text;

namespace MPDCtrlX.Views;

public partial class MainView : UserControl
{
    public MainView() { }
    public MainView(MainViewModel vm)
    {
        //var vm = App.GetService<MainViewModel>();
        DataContext = vm;

        InitializeComponent();
    }
}
