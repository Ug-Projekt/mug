using System;
using System.Collections.Generic;
using System.Text;

namespace Mug.Compilation
{
    public class Symbol
    {
        public bool IsPublic { get; }
        public string Name { get; }
        public object Value { get; }
        public Range Position { get; }
        public bool IsDefined { get; set; }

        public Symbol(string name, bool isdefined, object value = null, Range position = new(), bool ispublic = false)
        {
            IsPublic = ispublic;
            Name = name;
            Value = value;
            Position = position;
            IsDefined = isdefined;
        }

        public T GetValue<T>()
        {
            return (T)Value;
        }
    }
}
