using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Windowing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MPDCtrlX.Models;
using MPDCtrlX.Services;
using MPDCtrlX.Services.Contracts;
using MPDCtrlX.ViewModels;
using MPDCtrlX.Views;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace MPDCtrlX;

public partial class App : Application
{

    public static readonly string AppName = "MPDCtrlX";
    private static readonly string _appDeveloper = "torum";

    // Data folder and Config file path.
    private static readonly string _envDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    public static string AppDataFolder { get; } = System.IO.Path.Combine((System.IO.Path.Combine(_envDataFolder, _appDeveloper)), AppName);
    public static string AppConfigFilePath { get; } = System.IO.Path.Combine(AppDataFolder, AppName + ".config");

    private static readonly string _envCacheFolder = System.IO.Path.GetTempPath();
    public static string AppDataCacheFolder { get; } = System.IO.Path.Combine(_envCacheFolder, AppName + "_AlbumCoverCache");


    public IHost AppHost { get; private set; }

    public App()
    {
        AppHost = Microsoft.Extensions.Hosting.Host
                .CreateDefaultBuilder()
                .UseContentRoot(AppContext.BaseDirectory)
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<MainWindow>();
                    services.AddSingleton<MainView>();
                    services.AddSingleton<MainViewModel>();
                    services.AddSingleton<IMpcService, MpcService>();
                    services.AddTransient<IBinaryDownloader, BinaryDownloader>();

                    services.AddSingleton<QueuePage>(); 
                    services.AddSingleton<SearchPage>(); 
                    services.AddSingleton<FilesPage>(); 
                    services.AddSingleton<PlaylistItemPage>(); 
                    services.AddSingleton<AlbumPage>();
                    services.AddSingleton<ArtistPage>();
                    services.AddSingleton<SettingsPage>();
                })
                .Build();
    }

    public static T GetService<T>()
    where T : class
    {
        if ((App.Current as App)!.AppHost!.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        // TMP:
        Properties.Resources.Culture = new CultureInfo("en-US");

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            /*
            desktop.MainWindow = new MainWindow()
            {
                
            };
            */

            desktop.MainWindow = App.GetService<MainWindow>();
            //desktop.MainWindow.Show();
        }

        base.OnFrameworkInitializationCompleted();
    }

    // Log file.
    private static readonly StringBuilder _errortxt = new();
    private static readonly string _logFilePath = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + System.IO.Path.DirectorySeparatorChar + AppName + "_errors.txt";

    public static void AppendErrorLog(string errorTxt, string kindTxt)
    {
        DateTime dt = DateTime.Now;
        string nowString = dt.ToString("yyyy/MM/dd HH:mm:ss");

        _errortxt.AppendLine(nowString + " - " + kindTxt + " - " + errorTxt);
    }

    public static void SaveErrorLog()
    {
        if (string.IsNullOrEmpty(_logFilePath))
            return;

        string s = _errortxt.ToString();
        if (!string.IsNullOrEmpty(s))
            File.WriteAllText(_logFilePath, s);
    }
}
