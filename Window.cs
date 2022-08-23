using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outsider.Wrapper
{
    public struct Window
    {
        public string Title { get; set; }
        public IntPtr Handle { get; set; }
        public Process? Process { get; set; }
        public NativeWin32UserApiStructs.WindowInfo WindowInfo { get; set; }
        public IntPtr MonitorHandle { get; set; }

        public Window(Process process)
        {
            Title = process.MainWindowTitle;
            Handle = process.MainWindowHandle;
            this.Process = process;
            WindowInfo = WrappedWin32UserApi.GetWindowInfo(process.MainWindowHandle);
            MonitorHandle = NativeWin32UserApiMethods.MonitorFromWindow(Handle, NativeWin32UserApiStructs.MONITOR_DEFAULTTOPRIMARY);
        }

        public Window(IntPtr handle)
        {
            Title = WrappedWin32UserApi.GetWindowTitle(handle);
            Handle = handle;
            this.Process = System.Diagnostics.Process.GetProcessById(NativeWin32UserApiMethods.GetProcessId(handle));
            WindowInfo = WrappedWin32UserApi.GetWindowInfo(handle);
            MonitorHandle = NativeWin32UserApiMethods.MonitorFromWindow(Handle, NativeWin32UserApiStructs.MONITOR_DEFAULTTOPRIMARY);
        }

        public Window(IntPtr handle, string title)
        {
            Title = title;
            Handle = handle;
            this.Process = System.Diagnostics.Process.GetProcessById(NativeWin32UserApiMethods.GetProcessId(handle));
            WindowInfo = WrappedWin32UserApi.GetWindowInfo(handle);
            MonitorHandle = NativeWin32UserApiMethods.MonitorFromWindow(Handle, NativeWin32UserApiStructs.MONITOR_DEFAULTTOPRIMARY);
        }
    }
}
