using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MPDCtrlX.ViewModels;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace MPDCtrlX.Views;

public partial class SettingsPage : UserControl
{
    public SettingsPage() { }
    public SettingsPage(MainViewModel vm)
    {
        //var vm = App.GetService<MainViewModel>();
        DataContext = vm;

        InitializeComponent();

        //_ = Task.Run(GetCacheFolderSize);
        vm.GetCacheFolderSize();
    }

    private void PageGrid_SizeChanged(object? sender, Avalonia.Controls.SizeChangedEventArgs e)
    {
        if (!e.WidthChanged)
        {
            return;
        }

        if (DataContext is not MainViewModel vm)
        {
            return;
        }

        if (e.NewSize.Width < 340)
        {
            //
            this.HeaderGridSpacer.Width = 48;
        }
        else if (e.NewSize.Width < 740)
        {
            //
            this.HeaderGridSpacer.Width = 48;
        }
        else if (e.NewSize.Width < 1008)
        {
            //
            this.HeaderGridSpacer.Width = 24;
        }
        else if (e.NewSize.Width < 1320)
        {
            //
            this.HeaderGridSpacer.Width = 24;
        }
        else if (e.NewSize.Width < 2000)
        {
            //
            this.HeaderGridSpacer.Width = 24;
        }
        else
        {
            //
            this.HeaderGridSpacer.Width = 24;
        }
    }
}