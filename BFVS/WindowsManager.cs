using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BFVS
{
    class WindowsManager
    {
        static Dictionary<Win32.VirtualKeys, bool> KeyboardState = new Dictionary<Win32.VirtualKeys, bool>();

        public static Process GetProcess(string process)
        {
            Process p = null;

            Process[] processes = Process.GetProcessesByName(process);
            if (processes.Length > 0)
                p = processes[0];

            return p;
        }

        public static void PressKey(IntPtr handle, Win32.VirtualKeys key)
        {
            Win32.SendMessage(handle, Win32.WM_KEYDOWN, (UInt32)key, IntPtr.Zero);
            Win32.SendMessage(handle, Win32.WM_KEYUP, (UInt32)key, IntPtr.Zero);
            //Win32.PostMessage(handle, Win32.WM_KEYDOWN, (UInt32)key, IntPtr.Zero);
            //Win32.PostMessage(handle, Win32.WM_KEYUP, (UInt32)key, IntPtr.Zero);
        }

        public static void MouseClick(IntPtr handle, bool invert = false)
        {
            MouseClick(handle, 0, invert);
        }

        public static void MouseClick(IntPtr handle, int holdTime, bool invert = false)
        {
            Win32.mouse_event(invert ? Win32.MOUSEEVENTF_RIGHTDOWN : Win32.MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
            Thread.Sleep(holdTime);
            Win32.mouse_event(invert ? Win32.MOUSEEVENTF_RIGHTUP : Win32.MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
        }

        public static bool IsKeyDown(Win32.VirtualKeys key)
        {
            return (1 & (Win32.GetKeyState(key) >> 15)) == 1;
        }

        public static bool IsKeyPressed(Win32.VirtualKeys key)
        {
            bool currentState = IsKeyDown(key);
            if (!KeyboardState.ContainsKey(key))
            {
                KeyboardState[key] = currentState;
                return currentState;
            }

            bool previousState = KeyboardState[key];
            KeyboardState[key] = currentState;

            return currentState && !previousState;
        }

        public static bool IsKeyReleased(Win32.VirtualKeys key)
        {
            bool currentState = IsKeyDown(key);
            if (!KeyboardState.ContainsKey(key))
            {
                KeyboardState[key] = currentState;
                return !currentState;
            }

            bool previousState = KeyboardState[key];
            KeyboardState[key] = currentState;

            return !currentState && previousState;
        }


        public static Point GetMousePosition()
        {
            return System.Windows.Forms.Cursor.Position;
        }

        public static void MoveMouse(Point position)
        {
            System.Windows.Forms.Cursor.Position = position;
        }

        public static void SetWindowInFocus(IntPtr handle)
        {
            bool SyncShow = Win32.SetForegroundWindow(handle);
            bool ASyncShow = Win32.ShowWindowAsync(handle, 9); // SW_RESTORE = 9
        }

        public static bool IsWindowInFocus(IntPtr handle)
        {
            return handle == Win32.GetForegroundWindow();
        }

        public static Color GetPixelColor(IntPtr handle, int x, int y)
        {
            IntPtr hdc = Win32.GetWindowDC(handle);
            uint pixel = Win32.GetPixel(hdc, x, y);
            Win32.ReleaseDC(handle, hdc);
            Color color = Color.FromArgb((int)(pixel & 0x000000FF),
                            (int)(pixel & 0x0000FF00) >> 8,
                            (int)(pixel & 0x00FF0000) >> 16);
            return color;
        }
        
        public static Bitmap GetScreenshotGDI(IntPtr handle)
        {
            Win32.RECT rect;
            if (!Win32.GetWindowRect(handle, out rect))
            {
            }

            var bounds = new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
            var result = new Bitmap(bounds.Width, bounds.Height);

            using (var graphics = Graphics.FromImage(result))
                graphics.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);

            return result;
        }

        public static Point FindWindowCenter(IntPtr handle)
        {
            Win32.RECT rct;

            if (!Win32.GetWindowRect(handle, out rct))
                return Point.Empty;

            int titleHeight = Win32.GetSystemMetrics(Win32.SM_CYCAPTION);

            int width = rct.Right - rct.Left + 1;
            int height = rct.Bottom - rct.Top + 1;
            //int x = rct.Left + width / 2;
            //int y = rct.Top + height / 2;
            int x = width / 2;
            int y = titleHeight + (height - titleHeight) / 2;
            return new Point(x, y);
        }

        public static Bitmap GetCurrentIcon()
        {
            Bitmap cursorIcon = null;

            Win32.CURSORINFO pci;
            pci.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Win32.CURSORINFO));

            if (Win32.GetCursorInfo(out pci))
            {
                using (var icon = new Bitmap(32, 32))
                {
                    using (Graphics g = Graphics.FromImage(icon))
                        Win32.DrawIcon(g.GetHdc(), 0, 0, pci.hCursor);
                    cursorIcon = new Bitmap(icon).Clone() as Bitmap; // We have to "clone" icon because handle won't be released if we continue use it
                }
            }

            return cursorIcon;
        }

        public static void SetWindowFullscreen(IntPtr handle)
        {
            var rect = Screen.FromHandle(handle).Bounds;
            Win32.SetWindowPos(handle, handle, rect.X, rect.Y, rect.Width, rect.Height, Win32.SWP_SHOWWINDOW | Win32.SWP_FRAMECHANGED);
        }

        public static void MinimizeWindow(IntPtr handle)
        {
            Win32.ShowWindow(handle, Win32.SW_MINIMIZE);
        }

        public static void MaximizeWindow(IntPtr handle)
        {
            Win32.ShowWindow(handle, Win32.SW_MAXIMIZE);
        }

        public static void SetWindowStyleOff(IntPtr handle, int nIndex, long dwStylesToOff)
        {
            IntPtr windowStyles = GetWindowStyles(handle, nIndex);
            Win32.SetWindowLongPtr(handle, nIndex, ((windowStyles.ToInt64() | dwStylesToOff) ^ dwStylesToOff));
        }

        public static IntPtr GetWindowStyles(IntPtr handle, int nIndex)
        {
            return Win32.GetWindowLongPtr(handle, nIndex);
        }
    }
}
