using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Outkeep.Grains
{
    public static class TaskExtensions
    {
        /// <summary>
        /// Creates a task that completes when the given task completes or a timeout is reached.
        /// If the timeout is reached, the created task return the given default value, otherwise it returns the
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <param name="value"></param>
        /// <param name="timeout"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Inlining")]
        public static Task<T> WithDefaultOnTimeout<T>(this Task<T> task, T defaultValue, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            if (task == null) ThrowTaskNull();

            // quick path for completed task
            if (task.IsCompleted) return task;

            // quick path for infinite timeout
            if (timeout == TimeSpan.MaxValue) return task;

            // slow path for regular completion
            return Task.WhenAny(
                task,
                Task.Delay(timeout).ContinueWith((x, dv) => (T)dv, defaultValue, cancellationToken, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default))
                .Unwrap();

            void ThrowTaskNull() => throw new ArgumentNullException(nameof(task));
        }
    }
}