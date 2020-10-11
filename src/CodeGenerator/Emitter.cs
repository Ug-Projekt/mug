using System;
using System.Reflection;
using System.Reflection.Emit;

class Emitter {
    ModuleBuilder Module = null;
    public Assembly Assemble() => Module.Assembly;
}