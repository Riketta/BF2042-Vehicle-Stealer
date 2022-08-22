using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BFVS
{
    class Clicker
    {
        public static readonly Point MBTPoint = new Point(925, 660);
        public static readonly Point IFVPoint = new Point(925, 725);

        static readonly int CursorOffset = 2;
        static readonly int ClickHoldTime = 8;
        static readonly int ClickingInterval = 12;

        static readonly IntPtr handle = IntPtr.Zero;
        static bool isShakeInverted = false;

        public static void ClickAtLocation(Point pos, bool postClickSleep = false)
        {
            WindowsManager.MoveMouse(pos);
            Thread.Sleep(ClickHoldTime);
            WindowsManager.MouseClick(handle, ClickHoldTime);

            if (postClickSleep)
                Thread.Sleep(ClickingInterval);
        }

        public static Point ShakeCursorAtLocation(Point mouse)
        {
            isShakeInverted = !isShakeInverted;
            Point pos = new Point(mouse.X + (isShakeInverted ? CursorOffset : -CursorOffset), mouse.Y);
            return pos;
        }
    }
}
