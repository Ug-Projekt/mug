using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.TypeSystem
{
    public enum TypeKind
    {
        Auto,
        Pointer,
        String,
        Char,
        Int8,
        Int32,
        Int64,
        UInt8,
        UInt32,
        UInt64,
        Bool,
        Array,
        Struct,
        GenericStruct,
        Void
    }
}
