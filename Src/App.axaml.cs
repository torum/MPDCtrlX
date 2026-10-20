using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using MPDCtrlX.Services;
using MPDCtrlX.Services.Contracts;
using MPDCtrlX.ViewModels;
using MPDCtrlX.Views;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Tmds.DBus.Protocol;

namespace MPDCtrlX
{
    public class App : Application, IDisposable
    {
        public const string AppName = "MPDCtrlX";
        private const string AppDeveloper = "torum";

        // Config file path.(/home/<User>/.config/)
        private static readonly string _envDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public static string AppDataFolder { get; } = System.IO.Path.Combine((System.IO.Path.Combine(_envDataFolder, AppDeveloper)), AppName);
        public static string AppConfigFilePath { get; } = System.IO.Path.Combine(AppDataFolder, AppName + ".config");

        // Data folder.(/home/<User>/.local/share/)
        private static readonly string _envAppLocalFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData); // Use Local.
        private static readonly string _envAppLocalAppFolder = System.IO.Path.Combine((System.IO.Path.Combine(_envAppLocalFolder, AppDeveloper)), AppName);

        // Cache folder.(/home/<User>/.cache/ <- needs special handling because env does not support .cache)
        private readonly string _envCacheFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);  //System.IO.Path.GetTempPath();
        private readonly string _envCacheAppFolder;// = System.IO.Path.Combine((System.IO.Path.Combine(_envAppCacheFolder, AppDeveloper)), AppName);
        public static string AlbumCoverCacheFolder { get; private set; } = System.IO.Path.Combine(_envAppLocalAppFolder, "AlbumCoverCache");

        public App()
        {
            // Platform specific Cache folder path
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var oldCachePath = AlbumCoverCacheFolder;
                // /home/<User>/.cache/
                var xdgCache = Environment.GetEnvironmentVariable("XDG_CACHE_HOME");
                _envCacheFolder = !string.IsNullOrEmpty(xdgCache)
                    ? xdgCache
                    : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cache");
                //_envCacheFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cache");
                _envCacheAppFolder = System.IO.Path.Combine((System.IO.Path.Combine(_envCacheFolder, AppDeveloper)), AppName);
                AlbumCoverCacheFolder = System.IO.Path.Combine(_envCacheAppFolder, "AlbumCoverCache");

                if (System.IO.Directory.Exists(oldCachePath) && (!System.IO.Directory.Exists(AlbumCoverCacheFolder)))
                {
                    try
                    {
                        // Directory.Move requires the parent of the destination to exist, but the destination itself must not.
                        System.IO.Directory.CreateDirectory(_envCacheAppFolder);
                        System.IO.Directory.Move(oldCachePath, AlbumCoverCacheFolder);
                    }
                    catch (Exception ex)
                    {
                        AppendErrorLog("Failed to move AlbumCoverCache folder on startup.", ex.ToString());
                        SaveErrorLog();
                    }
                }
            }
            else
            {
                // Use Local for Windows and other. 
                _envCacheFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);  //System.IO.Path.GetTempPath();
                _envCacheAppFolder = System.IO.Path.Combine(System.IO.Path.Combine(_envCacheFolder, AppDeveloper), AppName);
                AlbumCoverCacheFolder = System.IO.Path.Combine(_envCacheAppFolder, "AlbumCoverCache");
            }

            try
            {
                if (!System.IO.Directory.Exists(App.AlbumCoverCacheFolder))
                {
                    System.IO.Directory.CreateDirectory(App.AlbumCoverCacheFolder);
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                AppendErrorLog("Failed to create AlbumCoverCache folder on startup.", ex.ToString());
                SaveErrorLog();
            }

            var services = new ServiceCollection();

            services.AddSingleton<MainWindow>();
            services.AddSingleton<MainView>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<IMpcService, MpcService>();
            services.AddTransient<IMpcBinaryService, MpcBinaryService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IConfigurationService, ConfigurationService>();

            services.AddSingleton<MprisPlayerController>();
            services.AddSingleton<MprisPathMethodHandler>();
            services.AddSingleton<IPathMethodHandler>(provider => provider.GetRequiredService<MprisPathMethodHandler>());
            //services.AddHostedService<MprisDbusHostedService>();
            services.AddSingleton<MprisDbusHostedService>();

            // This is not good for Wayland. There would be an extra invisible window created.
            // So, we will not use this for now.
            //services.AddTransient<InitWindow>();

            services.AddSingleton<QueuePage>();
            services.AddSingleton<SettingsPage>();
            services.AddSingleton<FilesPage>();
            services.AddSingleton<PlaylistItemPage>();
            services.AddSingleton<AlbumPage>();
            services.AddSingleton<ArtistPage>();
            services.AddSingleton<SearchPage>();

            // Build the provider
            ServiceProvider = services.BuildServiceProvider();

        }

        public IServiceProvider ServiceProvider { get; private set; } = null!;

        public static T GetService<T>() where T : class
        {
            return (App.Current as App)!.ServiceProvider.GetRequiredService<T>();
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public async override void OnFrameworkInitializationCompleted()
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

                //Dispatcher.UIThread.UnhandledException += OnUnhandledException;
                RegisterGlobalExceptionHandlers();

                var configurationService = ServiceProvider.GetRequiredService<IConfigurationService>();
                configurationService.Initialize(AppConfigFilePath);
                await configurationService.LoadConfigurationAsync();

                desktop.MainWindow = ServiceProvider.GetRequiredService<MainWindow>();//App.GetService<MainWindow>();

                // In Avalonia UI, don't show or activate the main window.
                // Let the Avalonia UI framework handle the window activation when the app is ready.
                //desktop.MainWindow.ShowActivated = true;
                //desktop.MainWindow.Show();
            }

            base.OnFrameworkInitializationCompleted();
        }

        // Log file.
        private static readonly StringBuilder _errortxt = new();
        private static readonly Lock _logLock = new();

        private static readonly string _logFilePath = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + System.IO.Path.DirectorySeparatorChar + AppName + "_errors.txt";

        private void RegisterGlobalExceptionHandlers()
        {
            // UI thread.
            Dispatcher.UIThread.UnhandledException += OnUnhandledException;

            // Any thread, process is about to terminate.
            AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;

            // Faulted tasks nobody awaited.
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        }

        private void OnUnhandledException(object? sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // Cancellation is normal (e.g. shutdown), not an error.
            if (e.Exception is OperationCanceledException)
            {
                e.Handled = true;
                return;
            }

            System.Diagnostics.Debug.WriteLine($"An unhandled exception occurred: {e.Exception}");
            AppendErrorLog("DispatcherUnhandledException", e.Exception.ToString());
            SaveErrorLog();

            // Keep the app alive for recoverable errors, but never for out-of-memory.
            e.Handled = e.Exception is not OutOfMemoryException;
        }

        private static void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            AppendErrorLog("AppDomain.UnhandledException", e.ExceptionObject?.ToString() ?? "(null)");
            SaveErrorLog(); // Last chance: the process terminates after this.
        }

        private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            if (e.Exception.InnerExceptions.All(ex => ex is OperationCanceledException))
            {
                e.SetObserved();
                return;
            }

            AppendErrorLog("TaskScheduler.UnobservedTaskException", e.Exception.ToString());
            SaveErrorLog();
            e.SetObserved();
        }

        public static void AppendErrorLog(string source, string details)
        {
            var nowString = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);

            lock (_logLock)
            {
                _errortxt.AppendLine(nowString + " - " + source + " - " + details);
            }
        }

        public static void SaveErrorLog()
        {
            if (!App.GetService<MainViewModel>()?.IsSaveLog ?? false)
                return;

            try
            {
                lock (_logLock)
                {
                    if (_errortxt.Length == 0)
                    {
                        return;
                    }

                    Directory.CreateDirectory(AppDataFolder);
                    File.WriteAllText(_logFilePath, _errortxt.ToString());
                }
            }
            catch (Exception ex)
            {
                // Logging must never take the app down.
                System.Diagnostics.Debug.WriteLine($"Could not save error log: {ex.Message}");
            }
        }

        public void Dispose()
        {
            //_appHostStartCts?.Dispose(); // Safe to call more than once.
            GC.SuppressFinalize(this);  // Satisfies CA1816.
        }
    }
}