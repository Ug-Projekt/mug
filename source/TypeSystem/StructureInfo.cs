using LLVMSharp.Interop;
using Mug.Models.Generator;
using Mug.MugValueSystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.TypeSystem
{
    public class StructureInfo
    {
        public string Name { get; set; }
        public string[] FieldNames { get; set; }
        public MugValueType[] FieldTypes { get; set; }
        public Range[] FieldPositions { get; set; }
        public LLVMTypeRef LLVMValue { get; set; }

        public Range GetFieldPositionFromName(string name)
        {
            for (int i = 0; i < FieldNames.Length; i++)
                if (FieldNames[i] == name)
                    return FieldPositions[i];

            throw new();
        }

        public MugValueType GetFieldTypeFromName(string name, IRGenerator generator, Range position)
        {
            return FieldTypes[GetFieldIndexFromName(name, generator, position)];
        }

        public bool ContainsFieldWithName(string name)
        {
            for (int i = 0; i < FieldNames.Length; i++)
                if (FieldNames[i] == name)
                    return true;

            return false;
        }

        public int GetFieldIndexFromName(string name, IRGenerator generator, Range position)
        {
            for (int i = 0; i < FieldNames.Length; i++)
                if (FieldNames[i] == name)
                    return i;

            generator.Error(position, "Undeclared field `", name, "`");
            throw new();
        }

        public int Size(int sizeofpointer)
        {
            var result = 0;

            for (int i = 0; i < FieldTypes.Length; i++)
                result += FieldTypes[i].Size(sizeofpointer);

            return result;
        }
    }
}
