using System;
using System.Runtime.InteropServices;

namespace Interop.Win32
{
    [StructLayout(LayoutKind.Sequential)]
    struct tagPAINTSTRUCT
    {
        public IntPtr hdc;
        public bool fErase;
        public tagRECT rcPaint;
        public bool fRestore;
        public bool fIncUpdate;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] rgbReserved;

        public override string ToString()
        {
            return string.Format(System.Globalization.CultureInfo.CurrentCulture, "{{hdc={0},fErase={1},rcPaint={2},fRestore={3},fIncUpdate={4}}}", hdc, fErase, rcPaint, fRestore, fIncUpdate);
        }
    }
}
