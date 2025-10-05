using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Logging;
using Avalonia.Threading;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MPDCtrlX.Desktop;

class Program
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

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace(LogEventLevel.Warning);

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
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

            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }
        catch (Exception ex)
        {
            AppendErrorLog(ex.Message + System.IO.Path.DirectorySeparatorChar + ex.StackTrace, "Fatal exception in Main");
        }
        finally
        {
            SaveErrorLog();

            //_pipeCancellationTokenSource.Cancel();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _mutex?.ReleaseMutex();
                //_mutex.Dispose();
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
                    AppendErrorLog(ex.Message + Environment.NewLine + ex.StackTrace, "Exception in StartPipeServer");
                    SaveErrorLog();
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

    /*
    private static async Task StartPipeServer(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                // The PipeName must be unique for your app
                using var server = new NamedPipeServerStream(PipeName, PipeDirection.In);
                await server.WaitForConnectionAsync(token);

                using var reader = new StreamReader(server);
                string? command = await reader.ReadLineAsync();

                if (command == "FOCUS")
                {
                    // This is running in a background thread, so we need to
                    // dispatch the window activation to the UI thread.
                    if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                    {
                        Dispatcher.UIThread.Post(() =>
                        {
                            var window = desktop.MainWindow;
                            if (window != null)
                            {
                                // Handle platform-specific focus logic
                                FocusExistingWindow(window);
                            }
                        });
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // The task was cancelled, so exit gracefully.
        }
        catch (Exception ex)
        {
            // Log the error
            Console.WriteLine($"Pipe server error: {ex.Message}");
        }
    }

    private static void SendFocusCommandToExistingInstance()
    {
        try
        {
            using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
            client.Connect(2000); // Timeout after 2 seconds
            using var writer = new StreamWriter(client);
            writer.WriteLine("FOCUS");
            writer.Flush();
        }
        catch (Exception)
        {
            // The first instance may have crashed or closed, do nothing.
        }
    }
    */

    private static void FocusExistingWindow(Window window)
    {
        // This is a minimal cross-platform approach.
        // For more robust behavior on Windows, you would use P/Invoke.
        if (window.WindowState == WindowState.Minimized)
        {
            window.WindowState = WindowState.Normal;
        }
        window.Activate();
    }

    private static void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        if (e.Exception.InnerException is not null)
        {
            AppendErrorLog("TaskScheduler_UnobservedTaskException", e.Exception.InnerException.Message);

            SaveErrorLog();
        }

        e.SetObserved();
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var exception = e.ExceptionObject as Exception;

        if (exception is TaskCanceledException exp)
        {
            // can ignore.
            AppendErrorLog("CurrentDomain_UnhandledException (TaskCanceledException)", exp.Message);
        }
        else
        {
            if (exception is not null)
            { 
                AppendErrorLog("CurrentDomain_UnhandledException", exception.Message); 
            }
        }

        SaveErrorLog();

        // TODO: Exit?
        //Environment.Exit(1);
    }

    // Log file.
    private static readonly StringBuilder _errortxt = new();
    private static readonly string _logFilePath = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + System.IO.Path.DirectorySeparatorChar + "MPDCtrlX_crash.txt";

    private static void AppendErrorLog(string errorTxt, string kindTxt)
    {
        DateTime dt = DateTime.Now;
        string nowString = dt.ToString("yyyy/MM/dd HH:mm:ss");

        _errortxt.AppendLine(nowString + " - " + kindTxt + " - " + errorTxt);
    }

    private static void SaveErrorLog()
    {
        if (string.IsNullOrEmpty(_logFilePath))
            return;
#if DEBUG

        string s = _errortxt.ToString();
        if (!string.IsNullOrEmpty(s))
            File.WriteAllText(_logFilePath, s);
#else
#endif

    }
}
