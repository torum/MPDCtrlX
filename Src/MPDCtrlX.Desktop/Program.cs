using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Logging;
using Avalonia.Threading;
using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MPDCtrlX.Desktop;

class Program
{
    private const string MutexName = "SingleInstanceMutexForMPDCtrlX";
    private const string PipeName = "SingleInstancePipeForMPDCtrlX"; 
    private static Mutex? _mutex;
    private static CancellationTokenSource _pipeCancellationTokenSource = new();

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
        _mutex = new Mutex(true, MutexName, out bool isNewInstance);
        if (isNewInstance)
        {
            // First instance: start the application and listen for pipe messages.
            Task.Run(() => StartPipeServer(_pipeCancellationTokenSource.Token));

            try
            {
                BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);

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

                _pipeCancellationTokenSource.Cancel();
                _mutex.ReleaseMutex();
                _mutex.Dispose();
            }
        }
        else
        {
            // Second instance: send a message to the first instance and exit.
            SendFocusCommandToExistingInstance();
        }
    }

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
