using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using MPDCtrlX.Core.ViewModels;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace MPDCtrlX.Core.Views;

public partial class SettingsPage : UserControl
{
    public SettingsPage() { }
    public SettingsPage(MainViewModel vm)
    {
        //var vm = App.GetService<MainViewModel>();
        DataContext = vm;

        InitializeComponent();

        // do this in shell page on navigate to settings page.
        //vm.GetCacheFolderSize();
    }

    private void PageGrid_SizeChanged(object? sender, Avalonia.Controls.SizeChangedEventArgs e)
    {
        if (!e.WidthChanged)
        {
            return;
        }

        if (DataContext is not MainViewModel)
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

    private async void HyperlinkButton_AlbumCacheFolderPath_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var dir = App.AppDataCacheFolder;

        var mainWin = App.GetService<MainWindow>();
        var launcher = TopLevel.GetTopLevel(mainWin)?.Launcher;
        if (launcher is null)
        {
            return;
        }

        // Open in default app.
        //await launcher.LaunchFileInfoAsync(new FileInfo(_currentFile));

        // Open in explorer/file manager.
        if (await launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(dir)))
        {
            // ok
        }
        else
        {
            // TODO: show error message.

            // This failes in Debug session when the app is attached VSCode from Snap packages on Linux. 
        }

    }
}