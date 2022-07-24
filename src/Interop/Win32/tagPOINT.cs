using System.Runtime.InteropServices;

namespace Interop.Win32
{
    [StructLayout(LayoutKind.Sequential)]
    struct tagPOINT
    {
        public int X;
        public int Y;

        public tagPOINT(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static implicit operator System.Drawing.Point(tagPOINT p)
        {
            return new System.Drawing.Point(p.X, p.Y);
        }

        public static implicit operator tagPOINT(System.Drawing.Point p)
        {
            return new tagPOINT(p.X, p.Y);
        }
    }
}
