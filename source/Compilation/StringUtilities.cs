using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Compilation
{
    public static class StringUtilities
    {
        unsafe public static sbyte* ToSbytePointer(this string str)
        {
            // Space for terminating \0
            byte[] bytes = new byte[Encoding.Default.GetByteCount(str) + 1];
            Encoding.Default.GetBytes(str, 0, str.Length, bytes, 0);

            fixed (byte* b = bytes)
                return (sbyte*)b;
        }
    }
}
