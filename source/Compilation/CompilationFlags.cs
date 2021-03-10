using LLVMSharp.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Mug.Compilation
{
    public enum CompilationMode
    {
        Release = 3,
        Debug = 0
    }

    public enum CompilationTarget
    {
        Executable,
        Library, // not available yet
        Bitcode,
        Assembly
    }

    public class CompilationFlags
    {
        private const string USAGE = @"USAGE: mug <action> <file> <options>";
        private const string HELP = @"
Compilation Actions:
  - build: to compile a program, with the following default options: {target: exe, mode: debug, output: <file>.exe}
  - run: build and run
  - help: show this list

Compilation Flags:
  - src: source file to compile (one per time)
  - dump-code: show the generated llvm code
  - dump-output: the file into write the dump-code, default: {stdout}
  - mode: the compilation mode: {release: fast and small exe, debug: faster compilation, slower exe, allows to use llvmdbg}
  - target: file format to generate: {exe, lib, bc, asm}
";

        private readonly string[] _allowedExtensions = new[] { ".mug" };
        private string[] _arguments = null;
        private int _argumentSelector = 0;
        private readonly Dictionary<string, object> _flags = new()
        {
            ["output"]      = null,
            ["target"]      = null,
            ["dump-code"]   = null, // print on dump-output the llvm generated code
            ["dump-output"] = null, // stdout
            ["mode"]        = null,
            ["src"]         = null, // file to compile
        };

        private string GetFile()
        {
            var file = GetFlag<string>("src");

            if (file is null)
                CompilationErrors.Throw("Undefined src to compile");

            return file;
        }

        private void SetFlag(string flag, object value)
        {
            _flags[flag] = value;
        }

        private T GetFlag<T>(string flag)
        {
            return (T)_flags[flag];
        }

        private void SetDefault(string flag, object value)
        {
            if (IsDefault(flag))
                SetFlag(flag, value);
        }

        private bool IsDefault(string flag)
        {
            return GetFlag<object>(flag) is null;
        }

        private void DumpIFRequired(LLVMModuleRef module)
        {
            if (GetFlag<bool>("dump-code"))
            {
                if (IsDefault("dump-output"))
                    module.Dump();
                else
                    File.WriteAllText(GetFlag<string>("dump-output"), module.ToString());
            }
        }

        private string MakeTarget()
        {
            return GetFlag<CompilationTarget>("target") switch
            {
                CompilationTarget.Assembly => "-S",
                _ => ""
            };
        }

        private void Build()
        {
            LoadArguments();

            var unit = new CompilationUnit(GetFile());
            unit.Compile(
                (int)GetFlag<CompilationMode>("mode"),
                GetFlag<string>("output"),
                GetFlag<CompilationTarget>("target") != CompilationTarget.Bitcode,
                MakeTarget());

            DumpIFRequired(unit.IRGenerator.Module);
        }

        private void BuildRun()
        {
            Build();
            Process.Start(GetFlag<string>("output")).WaitForExit();
        }

        private string CheckPath(string path)
        {
            if (!File.Exists(path))
                CompilationErrors.Throw("Unable to find path `", path, "`");

            return path;
        }

        private string CheckMugFile(string src)
        {
            CheckPath(src);

            if (!_allowedExtensions.Contains(Path.GetExtension(src)))
                CompilationErrors.Throw("Unable to recognize source file kind `", src, "`");

            return src;
        }

        private void ConfigureFlag(string flag, object value)
        {
            if (!IsDefault(flag))
                CompilationErrors.Throw("Impossible to specify multiple times the flag `", flag, "`");
            else
                SetFlag(flag, value);
        }

        private string NextArgument()
        {
            if (++_argumentSelector >= _arguments.Length)
                CompilationErrors.Throw("Expected a specification after flag `", _arguments[_argumentSelector-1][1..], "`");

            return _arguments[_argumentSelector];
        }

        public void SetArguments(string[] arguments)
        {
            _arguments = arguments;
        }

        private CompilationTarget GetTarget(string target)
        {
            switch (target)
            {
                case "exe": return CompilationTarget.Executable;
                case "lib": CompilationErrors.Throw("Library traget is not supported yet"); return CompilationTarget.Library;
                case "bc": return CompilationTarget.Bitcode;
                case "asm": return CompilationTarget.Assembly;
                default:
                    CompilationErrors.Throw("Unable to recognize target `", target, "`");
                    return CompilationTarget.Executable;
            }
        }

        private CompilationMode GetMode(string mode)
        {
            switch (mode)
            {
                case "debug": return CompilationMode.Debug;
                case "release": return CompilationMode.Release;
                default:
                    CompilationErrors.Throw("Unable to recognize compilation mode `", mode, "`");
                    return CompilationMode.Debug;
            }
        }

        private static string GetExecutableExtension()
        {
            return Environment.OSVersion.Platform == PlatformID.Win32NT ? "exe" : "";
        }

        private string GetOutputExtension()
        {
            return GetFlag<CompilationTarget>("target") switch
            {
                CompilationTarget.Assembly => "s",
                CompilationTarget.Executable => GetExecutableExtension(),
                _ => ""
            };
        }

        private void SetDefaultIFNeeded()
        {

            SetDefault("dump-code", false);
            SetDefault("target", CompilationTarget.Executable);
            SetDefault("mode", CompilationMode.Debug);
            SetDefault("output", Path.ChangeExtension(GetFile(), GetOutputExtension()));
        }

        private void InterpretArgument(string argument)
        {
            if (argument[0] == '*')
            {
                var arg = argument[1..];
                switch (arg)
                {
                    case "src":
                        ConfigureFlag(arg, CheckMugFile(NextArgument()));
                        break;
                    case "dump-code":
                        ConfigureFlag(arg, true);
                        break;
                    case "dump-output":
                        ConfigureFlag(arg, NextArgument());
                        break;
                    case "mode":
                        ConfigureFlag(arg, GetMode(NextArgument()));
                        break;
                    case "target":
                        ConfigureFlag(arg, GetTarget(NextArgument()));
                        break;
                    case "output":
                        ConfigureFlag(arg, NextArgument());
                        break;
                    case "":
                        CompilationErrors.Throw("Invalid empty flag");
                        break;
                    default:
                        CompilationErrors.Throw("Unknown compiler flag `", arg, "`");
                        break;
                }
            }
            else
                ConfigureFlag("src", CheckMugFile(argument));
        }

        private void LoadArguments()
        {
            for (; _argumentSelector < _arguments.Length; _argumentSelector++)
                InterpretArgument(_arguments[_argumentSelector]);

            SetDefaultIFNeeded();
        }

        private void PrintUsageAndHelp()
        {
            Console.WriteLine(USAGE);
            Console.WriteLine(HELP);
        }

        public void SetCompilationAction(string actionid)
        {
            switch (actionid)
            {
                case "build":
                    Build();
                    break;
                case "run":
                    BuildRun();
                    break;
                case "help":
                    PrintUsageAndHelp();
                    break;
                default:
                    CompilationErrors.Throw("Invalid compilation action `", actionid, "`");
                    break;
            }
        }
    }
}
