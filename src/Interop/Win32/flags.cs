using System;

namespace Interop.Win32
{
    [Flags()]
    public enum RedrawWindowFlags : uint
    {
        RDW_INVALIDATE = 0x1,
        InternalPaint = 0x2,
        Erase = 0x4,
        Validate = 0x8,
        NoInternalPaint = 0x10,
        NoErase = 0x20,
        NoChildren = 0x40,
        AllChildren = 0x80,
        RDW_UPDATENOW = 0x100,
        EraseNow = 0x200,
        RDW_FRAME = 0x400,
        NoFrame = 0x800
    }

    [Flags]
    public enum ETOOptions : uint
    {
        ETO_CLIPPED = 0x4,
        ETO_GLYPH_INDEX = 0x10,
        ETO_IGNORELANGUAGE = 0x1000,
        ETO_NUMERICSLATIN = 0x800,
        ETO_NUMERICSLOCAL = 0x400,
        ETO_OPAQUE = 0x2,
        ETO_PDY = 0x2000,
        ETO_RTLREADING = 0x800,
    }
}
