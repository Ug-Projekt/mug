using Mono.Cecil;
using Mono.Cecil.Cil;
using Mug.Models.Lexer;
using System;
using System.IO;
using System.Reflection;

namespace Mug.Models.Generator.Emitter
{
    public class MugEmitter
    {
        readonly AssemblyDefinition _program;
        readonly ModuleDefinition _module;
        const string RuntimeConfig = @"{
  ""runtimeOptions"": {
    ""tfm"": ""netcoreapp3.1"",
    ""framework"": {
      ""name"": ""Microsoft.NETCore.App"",
      ""version"": ""3.1.9""
    }
  }
}";
        public MugEmitter(string moduleName)
        {
            _program = AssemblyDefinition.CreateAssembly(
            new(moduleName, new(1, 0, 0, 0)), moduleName, ModuleKind.Dll);
            _module = _program.MainModule;
            _module.Types.Add(new("", moduleName, Mono.Cecil.TypeAttributes.Public | Mono.Cecil.TypeAttributes.Class));
        }
        public void DefineMain(MethodDefinition method)
        {
            DefineFunction(method);
            _module.EntryPoint = method;
        }
        public TypeReference TypeOf(TokenKind kind)
        {
            return kind switch
            {
                TokenKind.KeyTi32 => _module.TypeSystem.Int32,
                TokenKind.KeyTstr => _module.TypeSystem.String,
                TokenKind.KeyTVoid => _module.TypeSystem.Void,
                _ => null
            };
        }
        public void DefineFunction(MethodDefinition body)
        {
            _module.Types[0].Methods.Add(body);
        }
        void WriteRuntimeConfig()
        {
            File.WriteAllText(_module.Name + ".runtimeconfig.json", RuntimeConfig);
        }
        public void Save()
        {
            _program.Write(_module.Name + ".dll");
            WriteRuntimeConfig();
        }
        public MethodReference Import(MethodInfo methodInfo)
        {
            return _module.ImportReference(methodInfo);
        }
    }
}