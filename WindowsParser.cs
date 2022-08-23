using WindowsDesktop;

namespace Outsider.Wrapper;

/// <summary>
/// WinApi32 user api wrapper
/// </summary>

public static class WindowsParser
{
    /// <summary>
    /// Return all apps, even if they hide in tray
    /// </summary>
    /// <returns></returns>
    public static async Task<List<Window>> All()
    {
        List<Window> windows = await WrappedWin32UserApi.ProccesFinder.GetProcessesWindows();
        windows.AddRange(await WrappedWin32UserApi.ProccesFinder.GetOpenShellWindows());
        return windows;
    }

    /// <summary>
    /// Return all not hided apps
    /// </summary>
    /// <returns></returns>
    public static async Task<List<Window>> Visible()
    {
        List<Window> windows = await All();
        List<Window> visibleWindows = new();
        foreach (Window window in windows.Where(win => WrappedWin32UserApi.IsIconic(win.Handle)))
        {
            visibleWindows.Add(window);
        }

        return visibleWindows;
    }

    /// <summary>
    /// Return only windows on current virtual desktop
    /// </summary>
    /// <returns></returns>
    public static async Task<List<Window>> OnCurrentDesktop()
    {
        List<Window> windows = await All();
        List<Window> windowsOnCurrentDesktop = new();
        foreach (Window window in windows.Where(win => VDestops.IsOnCurrentDesktop(win.Handle)))
        {
            windowsOnCurrentDesktop.Add(window);
        }

        return windowsOnCurrentDesktop;
    }
}

internal static class VDestops
{
    internal static int AllDesktops()
    {
        Func<int> method = GetAllDesktopsMethod;
        // Launch in STA thread
        return StaTask.Start(method).Result;
    }

    internal static bool IsOnCurrentDesktop(IntPtr handle)
    {
        Func<IntPtr, bool> method = IsOnCurrentDesktopMethod;
        return StaTask.Start(method, handle).Result;
    }

    private static int GetAllDesktopsMethod()
    {
        return VirtualDesktop.GetDesktops().Length;
    }
    private static bool IsOnCurrentDesktopMethod(IntPtr handle)
    {
        return VirtualDesktop.IsCurrentVirtualDesktop(handle);
    }
}