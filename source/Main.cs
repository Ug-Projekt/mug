using LLVMSharp;
using Mug.Compilation;
using System;

if (!debug.isDebug())
{
    try
    {
        if (args.Length == 0)
            CompilationErrors.Throw("No arguments passed");
        for (int i = 0; i < args.Length; i++)
        {
            var unit = new CompilationUnit(args[i]);
            unit.Compile(0);
        }
    }
    catch (CompilationException)
    {
        Console.WriteLine("Cannot build due to previous errors");
        Environment.Exit(1);
    }

}