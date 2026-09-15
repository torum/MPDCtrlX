using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Logging;
using System.Diagnostics;
using System.IO.Pipes;
using System.Runtime.InteropServices;

namespace MPDCtrlX;

internal sealed class Program
{
    private const string MutexName = "SingleInstanceMutexForMPDCtrlX";
    private const string PipeName = "MPDCtrlX.SingleInstance.Pipe";
    private static Mutex? _mutex;
    //private static CancellationTokenSource _pipeCancellationTokenSource = new();
    private static string GetPipePath()
    {
        var tempPath = Path.GetTempPath();
        return Path.Combine(tempPath, PipeName);
    }

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    //public static void Main(string[] args) => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    public static void Main(string[] args)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // First, use a mutex to ensure thread-safe access to the pipe logic.
            _mutex = new Mutex(true, MutexName, out bool isNewInstance);
            if (!isNewInstance)
            {
                // Second instance: send a message to the first instance and exit.
                //SendFocusCommandToExistingInstance();

                // The mutex is already owned, indicating another instance is running.
                HandleExistingInstance(args);

                return;
            }
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var pipePath = GetPipePath();
            if (File.Exists(pipePath))
            {
                // check actually alive by connecting it.
                if (!HandleExistingInstance(args))
                {
                    if (File.Exists(pipePath))
                    {
                        File.Delete(pipePath);
                    }
                }

                // Exit the new process gracefully.
                Environment.Exit(0);
            }
        }

        // First instance: start the application and listen for pipe messages.
        //Task.Run(() => StartPipeServer(_pipeCancellationTokenSource.Token));

        StartPipeServer(args);

        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

        }
        finally
        {
            //_pipeCancellationTokenSource.Cancel();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _mutex?.ReleaseMutex();
                //_mutex.Dispose();
            }
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            //.With(new AvaloniaNativePlatformOptions { OverlayPopups = true })
            //.With(new Win32PlatformOptions { OverlayPopups = true })
            //.With(new X11PlatformOptions { OverlayPopups = true })
            .LogToTrace(LogEventLevel.Error);

    private static void OnProcessExit(object? sender, EventArgs e)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            if (File.Exists(GetPipePath()))
            {
                try
                {
                    File.Delete(GetPipePath());
                }
                catch (Exception ex)
                {
                    // Log the error.
                    Debug.WriteLine($"Error deleting named pipe: {ex.Message}");
                }
            }
        }
    }

    private static void StartPipeServer(string[] args)
    {
        var pipePath = GetPipePath();

        // Ensure the previous pipe is deleted in case of a crash.
        if (File.Exists(pipePath))
        {
            File.Delete(pipePath);
        }

        Task.Run(() =>
        {
            while (true)
            {
                try
                {
                    // Create a named pipe for incoming connections.
                    var server = new NamedPipeServerStream(pipePath, PipeDirection.In);
                    server.WaitForConnection();

                    // Read messages from the new instance.
                    using (var reader = new StreamReader(server))
                    {
                        string? line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            // Handle the message on the UI thread.
                            if (line == "ShowMainWindow")
                            {
                                Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                                {
                                    // Your app logic to show/focus the window goes here.
                                    if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                                    {
                                        var mainWnd = desktop.MainWindow;
                                        if (mainWnd is not null)
                                        {
                                            if (mainWnd.WindowState == WindowState.Minimized)
                                            {
                                                mainWnd.WindowState = WindowState.Normal;
                                            }
                                            desktop.MainWindow?.Show();
                                            desktop.MainWindow?.Activate();
                                        }
                                    }

                                });
                            }
                        }
                    }

                    //server.Disconnect();
                    if (server.IsConnected)
                    {
                        server.Disconnect();
                        // test
                        //break;
                    }
                }
                catch (Exception ex)
                {
#if DEBUG
                    Debug.WriteLine($"Exception @StartPipeServer while: {ex}");
#else
                //
#endif

                    // Just break out the loop in case of exception.
                    break;
                }
            }
        });
    }

    private static bool HandleExistingInstance(string[] args)
    {
        var pipePath = GetPipePath();

        // Attempt to connect to the named pipe.
        using var client = new NamedPipeClientStream(".", pipePath, PipeDirection.Out);
        try
        {
            // Give it a brief timeout to find the server.
            client.Connect(1000);
            if (client.IsConnected)
            {
                // Send a message to the first instance.
                using var writer = new StreamWriter(client);
                writer.WriteLine("ShowMainWindow");
                foreach (var arg in args)
                {
                    writer.WriteLine(arg);
                }
                writer.Flush();

                return true;
            }
            else
            {
                return false;
            }
        }
        catch (TimeoutException)
        {
            // First instance did not respond, it may have crashed.
            return false;
        }
        catch (Exception ex)
        {
            // Handle other potential pipe communication errors.
            Debug.WriteLine($"Could not connect to existing instance: {ex.Message}");
            return false;
        }
    }

}
