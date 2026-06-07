using Serilog;

namespace DotchatServer.src.Core.Extensions;

internal static class TaskExtensions
{
    /// <summary>
    /// Schedules a Task to run without awaiting it and logs unhandled exceptions.
    /// </summary>
    /// <remarks>Attaches a continuation that logs the task's exception when the task faults (OnlyOnFaulted).
    /// Prefer awaiting tasks where exception handling or cancellation is required.</remarks>
    /// <param name="task">The Task to execute without awaiting; any exception is logged rather than propagated.</param>
    public static void FireAndForget(this Task task) 
        => task.ContinueWith(t => Log.Error(t.Exception, "FireAndForget task failed"), TaskContinuationOptions.OnlyOnFaulted);
}