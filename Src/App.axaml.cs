using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MPDCtrlX.Services;
using MPDCtrlX.Services.Contracts;
using MPDCtrlX.ViewModels;
using MPDCtrlX.Views;
using MPDCtrlX.Views.Dialogs;
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
        private Task? _appHostStartTask;
        private readonly CancellationTokenSource _appHostStartCts = new();
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

            AppHost = Microsoft.Extensions.Hosting.Host
                    .CreateDefaultBuilder()
                    .UseContentRoot(AppContext.BaseDirectory)
                    .ConfigureLogging((context, logging) =>
                    {
#if DEBUG
                        logging.AddFilter("Microsoft.Extensions.Hosting", LogLevel.Debug);
                        logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Information);
#else
                        // Strips log destinations and shuts down the framework engines for production
                        logging.ClearProviders(); 
                        logging.AddFilter(null, LogLevel.None); 
#endif
                    })
                    .ConfigureServices((context, services) =>
                    {
                        services.AddSingleton<MainWindow>();
                        services.AddSingleton<MainView>();
                        services.AddSingleton<MainViewModel>();
                        services.AddSingleton<IMpcService, MpcService>();
                        services.AddTransient<IMpcBinaryService, MpcBinaryService>();
                        services.AddSingleton<IDialogService, DialogService>();

                        services.AddSingleton<MprisPlayerController>();
                        services.AddSingleton<MprisPathMethodHandler>();
                        services.AddSingleton<IPathMethodHandler>(provider => provider.GetRequiredService<MprisPathMethodHandler>());
                        services.AddHostedService<MprisDbusHostedService>();

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

        public IHost AppHost { get; }

        public static T GetService<T>() where T : class
        {
            return ((App.Current as App)!.AppHost.Services.GetRequiredService<T>());
            /*
            if ((App.Current as App)!.AppHost!.Services.GetService(typeof(T)) is not T service)
            {
                throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
            }

            return service;
            */
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

                //Dispatcher.UIThread.UnhandledException += OnUnhandledException;
                RegisterGlobalExceptionHandlers();

                _appHostStartTask = StartAppHostAsync();
                desktop.MainWindow = App.GetService<MainWindow>();
                //desktop.MainWindow.Closing += OnMainWindowClosing;
                desktop.Exit += OnDesktopExit; // Fallback for shutdowns that bypass window closing.
                /*
                desktop.MainWindow.Closed += async (sender, e) =>
                {
                    try
                    {
                        if (_appHostStartTask is not null)
                        {
                            await _appHostStartTask;
                        }
                        await AppHost.StopAsync();
                    }
                    finally
                    {
                        AppHost.Dispose();
                    }
                };
                */
                desktop.MainWindow.ShowActivated = true;
                desktop.MainWindow.Show();
            }

            base.OnFrameworkInitializationCompleted();
        }

        private async Task StartAppHostAsync()
        {
            try
            {
                await AppHost.StartAsync(_appHostStartCts.Token);
            }
            catch (OperationCanceledException)
            {
                // Start was cancelled because the app is shutting down.
                System.Diagnostics.Debug.WriteLine("AppHost.StartAsync cancelled.");
            }
            catch (Exception ex)
            {
                AppendErrorLog("AppHost.StartAsync", ex.ToString());
                SaveErrorLog();
            }
        }

        private bool _isShuttingDown;
        public bool IsShuttingDown => _isShuttingDown;
        private bool _hostDisposed;

        // Fallback: Shutdown() from code, OS logout, SIGTERM, etc.
        private void OnDesktopExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            /*
            try
            {
                if (_appHostStartTask is not null)
                {
                    await _appHostStartTask;
                }

                await AppHost.StopAsync();
            }
            finally
            {
                AppHost.Dispose();
                System.Diagnostics.Debug.WriteLine("AppHost disposed.");
            }
            */
            if (_isShuttingDown)
                return;

            _isShuttingDown = true;

            var stopTask = Task.Run(StopAndDisposeHostAsync);
            if (!stopTask.Wait(TimeSpan.FromSeconds(10)))
            {
                AppendErrorLog("OnDesktopExit", "Host shutdown timed out.");
                SaveErrorLog();
            }
        }
        /*
        private async void OnMainWindowClosing(object? sender, WindowClosingEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("OnMainWindowClosing @App");
            if (_isShuttingDown)
            {
                return; // Second pass, triggered by Shutdown(). Let the window close.
            }

            _isShuttingDown = true;
            e.Cancel = true; // Keep the process alive while the host stops.

            // Hide immediately so the user doesn't think the app froze.
            if (sender is Window window)
            {
                window.Hide();
            }

            try
            {
                await StopAndDisposeHostAsync();
            }
            finally
            {
                (ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Shutdown();
            }
        }
        */
        /// <summary>
        /// Stops and disposes the host, then shuts down the application.
        /// Called from MainWindow.OnClosing.
        /// </summary>
        public async Task ShutdownAsync()
        {
            if (_isShuttingDown)
            {
                return;
            }

            _isShuttingDown = true;

            try
            {
                await StopAndDisposeHostAsync();
            }
            finally
            {
                (ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Shutdown();
            }
        }

        private async Task StopAndDisposeHostAsync()
        {
            if (_hostDisposed)
            {
                return;
            }
            /*
            try
            {
                if (_appHostStartTask is not null)
                {
                    await _appHostStartTask.ConfigureAwait(false);
                }

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                await AppHost.StopAsync(cts.Token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                AppendErrorLog("AppHost.StopAsync", ex.ToString());
                SaveErrorLog();
            }
            finally
            {
                _hostDisposed = true;
                AppHost.Dispose();
                System.Diagnostics.Debug.WriteLine("AppHost disposed.");
            }
            */
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

                // 1. Abort a start that is still in progress (e.g. a hung D-Bus connect).
                await _appHostStartCts.CancelAsync().ConfigureAwait(false);

                // 2. Wait for the start task, but never longer than the timeout.
                if (_appHostStartTask is not null)
                {
                    try
                    {
                        await _appHostStartTask.WaitAsync(cts.Token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        AppendErrorLog("StopAndDisposeHostAsync", "AppHost start did not finish within the timeout.");
                        SaveErrorLog();
                    }
                }

                // 3. Stop the host, bounded by the same timeout.
                await AppHost.StopAsync(cts.Token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                AppendErrorLog("AppHost.StopAsync", ex.ToString());
                SaveErrorLog();
            }
            finally
            {
                _hostDisposed = true;
                _appHostStartCts.Dispose();
                AppHost.Dispose();
                System.Diagnostics.Debug.WriteLine("AppHost disposed.");
            }
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

        public static void AppendErrorLog(string source, string details)
        {
            var nowString = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);

            lock (_logLock)
            {
                _errortxt.AppendLine(nowString + " - " + source + " - " + details);
            }
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

        public static void SaveErrorLog()
        {
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
            _appHostStartCts?.Dispose(); // Safe to call more than once.
            GC.SuppressFinalize(this);  // Satisfies CA1816.
        }
    }
}