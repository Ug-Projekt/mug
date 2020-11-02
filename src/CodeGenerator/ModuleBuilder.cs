﻿using System.Collections.Generic;
class Module
{
    List<Method> Methods = new List<Method>();
    string Name;
    public string GetAssembly()
    {
        Methods.Add(new Method("main", "void", "public static", new string[0], new string[0], true, new CodeGenerator().GetMethodAssembly(GlobalParser.Functions["main"].Body))); // fix local variables
        return ".assembly extern mscorlib {}\n.assembly " + Name + " {\n\t.ver 0:0:0:1\n}\n" + string.Join("\n", Methods);
    }
    public Module(string name="Program") => Name = name;
}