using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace BigBrother
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Rect
    {
        [FieldOffset(0)]
        public int Left;

        [FieldOffset(4)]
        public int Top;

        [FieldOffset(8)]
        public int Right;

        [FieldOffset(12)]
        public int Bottom;
    }
}
