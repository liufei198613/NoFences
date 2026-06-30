using System;
using System.Runtime.InteropServices;

namespace NoFences.Win32
{
    public class DesktopUtil
    {
        private const Int32 GWL_STYLE = -16;
        private const Int32 GWL_EXSTYLE = -20;
        private const Int32 GWL_HWNDPARENT = -8;
        private const Int32 WS_MAXIMIZEBOX = 0x00010000;
        private const Int32 WS_MINIMIZEBOX = 0x00020000;
        private const Int32 WS_EX_TRANSPARENT = 0x00000020;
        private const Int32 WS_EX_TOOLWINDOW = 0x00000080;

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", SetLastError = true)]
        private extern static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private extern static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        // For 32-bit compatibility, fallback to GetWindowLong/SetWindowLong
        [DllImport("user32.dll", EntryPoint = "GetWindowLong", SetLastError = true)]
        private extern static IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private extern static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        // Helper methods that work for both 32-bit and 64-bit
        private static IntPtr GetWindowLongPtrSafe(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 8)
                return GetWindowLongPtr(hWnd, nIndex);
            else
                return GetWindowLong(hWnd, nIndex);
        }

        private static IntPtr SetWindowLongPtrSafe(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 8)
                return SetWindowLongPtr(hWnd, nIndex, dwNewLong);
            else
                return SetWindowLong(hWnd, nIndex, dwNewLong);
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpWindowClass, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);


        public static void PreventMinimize(IntPtr handle)
        {
            IntPtr windowStyle = GetWindowLongPtrSafe(handle, GWL_STYLE);
            int styleVal = windowStyle.ToInt32();
            styleVal &= ~WS_MAXIMIZEBOX & ~WS_MINIMIZEBOX;
            SetWindowLongPtrSafe(handle, GWL_STYLE, new IntPtr(styleVal));
        }

        public static void GlueToDesktop(IntPtr handle)
        {
            // Find the correct desktop window
            IntPtr desktopWindow = IntPtr.Zero;
            
            // First try to find the WorkerW window that contains the desktop
            IntPtr currentWindow = IntPtr.Zero;
            do
            {
                currentWindow = FindWindowEx(IntPtr.Zero, currentWindow, "WorkerW", null);
                IntPtr shelfWindow = FindWindowEx(currentWindow, IntPtr.Zero, "SHELLDLL_DefView", null);
                if (shelfWindow != IntPtr.Zero)
                {
                    desktopWindow = currentWindow;
                    break;
                }
            } while (currentWindow != IntPtr.Zero);

            // If no WorkerW found, fall back to Progman
            if (desktopWindow == IntPtr.Zero)
            {
                desktopWindow = FindWindow("Progman", null);
            }
            
            // Set parent to desktop window only if we found it
            if (desktopWindow != IntPtr.Zero)
            {
                SetParent(handle, desktopWindow);
            }
            
            // Only add WS_EX_TOOLWINDOW to prevent Win+D from minimizing
            // WS_EX_TRANSPARENT is NOT needed here because we handle click-through in WndProc by returning HTTRANSPARENT
            // Adding WS_EX_TRANSPARENT here would make the entire window transparent and invisible!
            IntPtr exStylePtr = GetWindowLongPtrSafe(handle, GWL_EXSTYLE);
            int exStyle = exStylePtr.ToInt32();
            exStyle |= WS_EX_TOOLWINDOW;
            // Remove WS_EX_TRANSPARENT if it was already added
            exStyle &= ~WS_EX_TRANSPARENT;
            SetWindowLongPtrSafe(handle, GWL_EXSTYLE, new IntPtr(exStyle));
        }
    }
}
