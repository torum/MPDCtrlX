using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MPDCtrlX.Services;
using MPDCtrlX.Services.Contracts;
using MPDCtrlX.ViewModels;
using MPDCtrlX.Views;
using MPDCtrlX.Views.Dialogs;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using static System.Environment;

namespace MPDCtrlX
{
    public class App : Application
    {
        public static readonly string AppName = "MPDCtrlX";
        private static readonly string AppDeveloper = "torum";

        // Config file path.(/home/<User>/.config/)
        private static readonly string EnvDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public static string AppDataFolder { get; } = System.IO.Path.Combine((System.IO.Path.Combine(EnvDataFolder, AppDeveloper)), AppName);
        public static string AppConfigFilePath { get; } = System.IO.Path.Combine(AppDataFolder, AppName + ".config");

        // Data folder.(/home/<User>/.local/share/)
        private static readonly string EnvAppLocalFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData); // Use Local.
        private static readonly string EnvAppLocalAppFolder = System.IO.Path.Combine((System.IO.Path.Combine(EnvAppLocalFolder, AppDeveloper)), AppName);

        // Cache folder.(/home/<User>/.cache/ <- needs special handling because env does not support .cache)
        private static string _envAppCacheFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData); // Use Local for Windows. //System.IO.Path.GetTempPath();
        private static readonly string EnvAppCacheAppFolder = System.IO.Path.Combine((System.IO.Path.Combine(_envAppCacheFolder, AppDeveloper)), AppName);
        public static string AppDataCacheFolder { get; } = System.IO.Path.Combine(EnvAppCacheAppFolder, "AlbumCoverCache");

        public IHost AppHost { get;}

        public App()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _envAppCacheFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData); //System.IO.Path.GetTempPath();// Use Local for Windows. 
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                _envAppCacheFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cache");
            }

            AppHost = Microsoft.Extensions.Hosting.Host
                    .CreateDefaultBuilder()
                    .UseContentRoot(AppContext.BaseDirectory)
                    .ConfigureServices((context, services) =>
                    {
                        services.AddSingleton<MainWindow>();
                        services.AddSingleton<MainView>();
                        services.AddSingleton<MainViewModel>();
                        services.AddSingleton<IMpcService, MpcService>();
                        services.AddTransient<IMpcBinaryService, MpcBinaryService>();
                        services.AddSingleton<IDialogService, DialogService>();

                        services.AddSingleton<QueuePage>();
                        services.AddSingleton<SearchPage>();
                        services.AddSingleton<FilesPage>();
                        services.AddSingleton<PlaylistItemPage>();
                        services.AddSingleton<AlbumPage>();
                        services.AddSingleton<ArtistPage>();
                        services.AddSingleton<SettingsPage>();
                        services.AddTransient<InitWindow>();
                    })
                    .Build();
        }

        public static T GetService<T>() where T : class
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
            // TMP: For testing locale strings
            //Properties.Resources.Culture = new CultureInfo("en-US");

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Set custom font for non-Windows platforms.
                if (Application.Current != null)
                {
                    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        Application.Current.Resources["ContentControlThemeFontFamily"] = new FontFamily("fonts:Inter#Inter");
                    }
                }

                Dispatcher.UIThread.UnhandledException += OnUnhandledException;

                desktop.MainWindow = App.GetService<MainWindow>();
                //desktop.MainWindow.Show();
            }

            base.OnFrameworkInitializationCompleted();
        }

        // Log file.
        private static readonly StringBuilder Errortxt = new();
        private static readonly string LogFilePath = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + System.IO.Path.DirectorySeparatorChar + AppName + "_errors.txt";

        private void OnUnhandledException(object? sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // 
            e.Handled = true;

            // Log the exception for debugging
            //Console.WriteLine($"An unhandled exception occurred: {e.Exception}");
            AppendErrorLog("DispatcherUnhandledException", e.Exception.ToString());

            SaveErrorLog();
        }

        public static void AppendErrorLog(string errorTxt, string kindTxt)
        {
            var dt = DateTime.Now;
            var nowString = dt.ToString("yyyy/MM/dd HH:mm:ss");

            Errortxt.AppendLine(nowString + " - " + kindTxt + " - " + errorTxt);
        }

        public static void SaveErrorLog()
        {
            if (string.IsNullOrEmpty(LogFilePath))
                return;

            var s = Errortxt.ToString();
            if (!string.IsNullOrEmpty(s))
                File.WriteAllText(LogFilePath, s);
        }
    }
}