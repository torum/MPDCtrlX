using Avalonia.Controls;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using MPDCtrlX.Core.Models;
using MPDCtrlX.Core.ViewModels;
using System;
using System.Diagnostics;
using System.Text;

namespace MPDCtrlX.Core.Views;

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
