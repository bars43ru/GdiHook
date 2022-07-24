using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Interop.Win32
{
    public delegate bool EnumWindowProc(IntPtr hwnd, IntPtr lParam);


    static internal class User32
    {
        [DllImport("user32.dll")]
        static public extern IntPtr WindowFromPoint(tagPOINT Point);

        [DllImport("user32.dll", SetLastError = true)]
        static public extern uint GetWindowThreadProcessId(IntPtr hWnd, out IntPtr lpdwProcessId);

        [DllImport("user32.dll")]
        static public extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, RedrawWindowFlags flags);
        //static public extern bool RedrawWindow(IntPtr hWnd, [In] ref tagRECT lprcUpdate, IntPtr hrgnUpdate, RedrawWindowFlags flags);

        [DllImport("user32.dll", SetLastError = true)]
        static public extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        static public extern bool UpdateWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static public extern IntPtr WindowFromDC(IntPtr hDC);

        [DllImport("user32.dll", SetLastError = true)]
        static public extern bool GetWindowRect(IntPtr hwnd, out tagRECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        static public extern bool GetClientRect(IntPtr hWnd, out tagRECT lpRect);

        [DllImport("user32.dll")]
        static public extern bool ClientToScreen(IntPtr hWnd, ref System.Drawing.Point lpPoint);

        [DllImport("user32.dll")]
        static public extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);
        static public int GetProcessId(IntPtr hWnd)
        {
            GetWindowThreadProcessId(hWnd, out var threadProcessId);
            var process = Process.GetProcessById(threadProcessId.ToInt32());
            return process.Id;
        }

        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static public extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr lParam);

        [DllImport("user32")]
        static public extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32")]
        static public extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32")]
        static public extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32")]
        static public extern IntPtr GetShellWindow();

        [DllImport("user32")]
        static public extern bool EnumWindows(EnumWindowProc enumFunc, int lParam);
    }
}
