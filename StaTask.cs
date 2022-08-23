namespace Outsider.Wrapper
{
    /// <summary>
    /// Run task in STA thread.
    /// </summary>
    internal static class StaTask
    {
        internal static Task<T> Start<T>(Func<T> func)
        {
            var task = new TaskCompletionSource<T>();
            Thread thread = new (() =>
            {
                try
                {
                    task.SetResult(func());
                }
                catch (Exception ex)
                {
                    task.SetException(ex);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return task.Task;
        }

        internal static Task<T> Start<P, T>(Func<P, T> func, P param)
        {
            var task = new TaskCompletionSource<T>();
            Thread thread = new (() =>
            {
                try
                {
                    task.SetResult(func(param));
                }
                catch (Exception ex)
                {
                    task.SetException(ex);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return task.Task;
        }
    }
}
