using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Interop.Win32;
using System.Text;

namespace GdiHook.Interop
{
    public class WindowHelper
    {
        public static IEnumerable<IntPtr> GetOpenWindows()
        {
            IntPtr shellWindow = User32.GetShellWindow();
            List<IntPtr> windows = new();

            User32.EnumWindows(delegate (IntPtr hWnd, IntPtr lParam)
            {
                if (hWnd == shellWindow) return true;
                if (!User32.IsWindowVisible(hWnd)) return true;
                if (User32.GetWindowTextLength(hWnd) == 0) return true;
                windows.Add(hWnd);
                return true;
            }, 0);

            return windows;
        }


        public static IEnumerable<IntPtr> ChildHandles(IntPtr hwnd)
        {
            List<IntPtr> childHandles = new();

            GCHandle gcChildhandlesList = GCHandle.Alloc(childHandles);
            IntPtr pointerChildHandlesList = GCHandle.ToIntPtr(gcChildhandlesList);

            try
            {
                EnumWindowProc childProc = new EnumWindowProc(EnumWindow);
                User32.EnumChildWindows(hwnd, childProc, pointerChildHandlesList);
            }
            finally
            {
                gcChildhandlesList.Free();
            }

            return childHandles;
        }

        static private bool EnumWindow(IntPtr hWnd, IntPtr lParam)
        {
            GCHandle? gcChildhandlesList = GCHandle.FromIntPtr(lParam);

            if (gcChildhandlesList == null || gcChildhandlesList?.Target == null)
            {
                return false;
            }

            List<IntPtr> childHandles = gcChildhandlesList?.Target as List<IntPtr>;
            childHandles.Add(hWnd);

            return true;
        }

        static public string Title(IntPtr hwnd)
        {
            int length = User32.GetWindowTextLength(hwnd);
            if (length == 0)
            {
                return "";
            }

            StringBuilder builder = new StringBuilder(length);
            User32.GetWindowText(hwnd, builder, length + 1);

            return builder.ToString();

        }
    }
}
