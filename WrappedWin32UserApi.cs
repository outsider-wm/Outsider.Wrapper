using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Outsider.Wrapper
{
    public static class WrappedWin32UserApi
    {

        [DllImport("moniori.dll", EntryPoint = "get_monitor_info")]
        private static extern NativeWin32UserApiStructs.MonitorInfoEx MonioriGetMonitorInfo(IntPtr monitorHandle);

        /// <summary>
        /// Get info about monitor
        /// </summary>
        /// <param name="hMonitor">Monitor pointer</param>
        /// <returns></returns>
        public static NativeWin32UserApiStructs.MonitorInfoEx GetMonitorInfo(IntPtr hMonitor)
        {
            try
            {
                return MonioriGetMonitorInfo(hMonitor);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        /// <summary>
        /// Get handlers of all windows
        /// </summary>
        /// <returns></returns>
        public static List<IntPtr> EnumWindows()
        {
            var result = new List<IntPtr>();

            NativeWin32UserApiMethods.EnumWindows(new NativeWin32UserApiMethods.CallBackPtr((hwnd, lParam) =>
            {
                result.Add(hwnd);
                return true;
            }), 0);

            return result;
        }

        /// <summary>
        /// Return string of window title
        /// </summary>
        /// <param name="intPtr">handler of window</param>
        /// <returns></returns>
        internal static string GetWindowTitle(IntPtr intPtr)
        {
            var result = new StringBuilder();
            if (NativeWin32UserApiMethods.GetWindowText(intPtr, result, 256) > 0)
            {
                return result.ToString();
            }
            return string.Empty;
        }

        public static string GetWindowClass(IntPtr intPtr)
        {
            var result = new StringBuilder(256);
            int nRet = NativeWin32UserApiMethods.GetClassName(intPtr, result, result.Capacity);
            if (nRet != 0)
            {
                return result.ToString();
            }
            return string.Empty;
        }

        /// <summary>
        /// Get focused window
        /// </summary>
        /// <returns></returns>
        public static IntPtr? FocusedWindow()
        {
            foreach (var process in EnumWindows())
            {
                if (process == NativeWin32UserApiMethods.GetForegroundWindow())
                {
                    return process;
                }
            }
            return null;
        }

        /// <summary>
        /// Get flags of window
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public static NativeWin32UserApiStructs.WindowInfo GetWindowInfo(IntPtr handle)
        {
            NativeWin32UserApiStructs.WindowInfo windowInfo = new();
            windowInfo.cbSize = (uint)Marshal.SizeOf(windowInfo);
            NativeWin32UserApiMethods.GetWindowInfo(handle, ref windowInfo);
            return windowInfo;
        }

        public static bool ShowWindow(IntPtr hWnd, NativeWin32UserApiStructs.ShowWindowMode nCmdShow)
        {
            try
            {
                return NativeWin32UserApiMethods.ShowWindow(hWnd, (int)nCmdShow);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }


        /// <summary>
        ///     The MoveWindow function changes the position and dimensions of the specified window. For a top-level window, the
        ///     position and dimensions are relative to the upper-left corner of the screen. For a child window, they are relative
        ///     to the upper-left corner of the parent window's client area.
        ///     <para>
        ///     Go to https://msdn.microsoft.com/en-us/library/windows/desktop/ms633534%28v=vs.85%29.aspx for more
        ///     information
        ///     </para>
        /// </summary>
        /// <param name="hWnd">C++ ( hWnd [in]. Type: HWND )<br /> Handle to the window.</param>
        /// <param name="X">C++ ( X [in]. Type: int )<br />Specifies the new position of the left side of the window.</param>
        /// <param name="Y">C++ ( Y [in]. Type: int )<br /> Specifies the new position of the top of the window.</param>
        /// <param name="nWidth">C++ ( nWidth [in]. Type: int )<br />Specifies the new width of the window.</param>
        /// <param name="nHeight">C++ ( nHeight [in]. Type: int )<br />Specifies the new height of the window.</param>
        /// <param name="bRepaint">
        ///     C++ ( bRepaint [in]. Type: bool )<br />Specifies whether the window is to be repainted. If this
        ///     parameter is TRUE, the window receives a message. If the parameter is FALSE, no repainting of any kind occurs. This
        ///     applies to the client area, the nonclient area (including the title bar and scroll bars), and any part of the
        ///     parent window uncovered as a result of moving a child window.
        /// </param>
        /// <returns>
        ///     If the function succeeds, the return value is nonzero.<br /> If the function fails, the return value is zero.
        ///     <br />To get extended error information, call GetLastError.
        /// </returns>
        public static bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint)
        {
            try
            {
                return NativeWin32UserApiMethods.MoveWindow(hWnd, X, Y, nWidth, nHeight, bRepaint);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        public static bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, UInt32 uFlags)
        {
            try
            {
                return NativeWin32UserApiMethods.SetWindowPos(hWnd, hWndInsertAfter, X, Y, cx, cy, uFlags);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        public static bool IsIconic(IntPtr handle)
        {
            try
            {
                return NativeWin32UserApiMethods.IsIconic(handle);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException(ex.Message);
            }

        }

        /// <summary>
        /// Operate with procces to find windows
        /// </summary>
        internal static class ProccesFinder
        {
            /// <summary>
            /// Get all processes that have windows
            /// </summary>
            /// <returns></returns>
            public static async Task<List<Window>> GetProcessesWindows()
            {
                Process[] processlist = Process.GetProcesses();
                List<Window> windowedApplications = new();
                await Task.Run(() =>
                {
                    foreach (Process process in processlist)
                    {
                        // If the process appears on the Taskbar (if has a title)
                        // print the information of the process
                        if (!string.IsNullOrEmpty(process.MainWindowTitle) && process.MainWindowHandle != IntPtr.Zero)
                        {
                            windowedApplications.Add(new Window(process));
                        }
                    }
                });

                return windowedApplications;
            }

            /// <summary>
            /// Get all shell windows (currently only explorer)
            /// </summary>
            internal static async Task<List<Window>> GetOpenShellWindows()
            {
                List<Window> windows = new();
                SHDocVw.ShellWindows shellWindows = new();

                await Task.Run(() =>
                {
                    foreach (SHDocVw.InternetExplorer window in shellWindows)
                    {
                        windows.Add(new Window((IntPtr)window.HWND, window.Name));
                    }
                });
                return windows;
            }
        }
    }
}
