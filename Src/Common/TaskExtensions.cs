using System;
using System.Collections.Generic;
using System.Text;

namespace MPDCtrlX.Common;

internal static class TaskExtensions
{
    /// <summary>
    /// Observes a task that nobody awaits. Cancellation is ignored; other exceptions are logged.
    /// </summary>
    public static async Task SafeAsync(this Task task, string origin, bool continueOnCapturedContext = false)
    {
        try
        {
            //
            if (continueOnCapturedContext)
            {
                await task.ConfigureAwait(false);
            }
            else
            {
                await task;// no ConfigureAwait(false): continuations stay on the UI thread
            }
        }
        catch (OperationCanceledException)
        {
            // Expected during shutdown or when a newer request supersedes this one.
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Exception @{origin}: {ex}");
            App.AppendErrorLog(origin, ex.ToString());
            App.SaveErrorLog();
        }
    }
}
