using LLVMSharp.Interop;
using Mug.Models.Parser.NodeKinds.Statements;
using System;
using System.Collections.Generic;
using Mug.Compilation;
using static LLVMSharp.Interop.LLVM;

namespace Mug.Models.Generator.Emitter
{
    public class MugEmitter
    {
        unsafe public LLVMBuilderRef Builder { get; private set; } = CreateBuilder();

        unsafe private readonly Stack<LLVMOpaqueValue> _stack = new();
        unsafe private readonly Dictionary<string, LLVMOpaqueValue> _memory = new();
        private readonly IRGenerator _generator;

        // public static readonly LLVMBool ConstLLVMFalse = new LLVMBool(0);
        // public static readonly LLVMBool ConstLLVMTrue = new LLVMBool(1);

        // implicit function operators
        public const string StringConcatenationIF = "string_concat(i8*, i8*)";
        public const string StringToCharArrayIF = "string_to_chararray(i8*)";

        public MugEmitter(IRGenerator generator)
        {
            _generator = generator;
        }

        unsafe public void Load(LLVMOpaqueValue* value)
        {
            _stack.Push(*value);
        }

        unsafe public LLVMValueRef Pop()
        {
            var p = _stack.Pop();
            return &p;
        }

        unsafe public LLVMValueRef Peek()
        {
            var p = _stack.Peek();
            return &p;
        }

        unsafe public LLVMTypeRef PeekType()
        {
            return Peek().TypeOf;
        }

        private void CallOperatorFunction(string name, Range position)
        {
            var function = _generator.GetSymbol(name, position);
            Call(function, (int)function.CountParams());
        }

        unsafe public void Add(Range position)
        {
            var exprType = PeekType();

            if (_generator.MatchStringType(exprType))
                CallOperatorFunction(StringConcatenationIF, position);
            else
            {
                var second = Pop();

                
                Load(BuildAdd(Builder, Pop(), second, "".ToSbytePointer()));
            }
        }

        unsafe public void Sub()
        {
            var second = Pop();
            Load(BuildSub(Builder, Pop(), second, "".ToSbytePointer()));
        }

        unsafe public void Mul()
        {
            var second = Pop();
            Load(BuildMul(Builder, Pop(), second, "".ToSbytePointer()));
        }

        unsafe public void Div()
        {
            var second = Pop();
            Load(BuildSDiv(Builder, Pop(), second, "".ToSbytePointer()));
        }

        unsafe public void CastInt(LLVMTypeRef type)
        {
            Load(BuildIntCast(Builder, Pop(), type, "".ToSbytePointer()));
        }

        unsafe private LLVMValueRef GetFromMemory(string name)
        {
            var p = _memory[name];
            return &p;
        }

        unsafe private void SetMemory(string name, LLVMOpaqueValue* value)
        {
            _memory.TryAdd(name, *value);
        }

        public void DeclareVariable(VariableStatement variable)
        {
            DeclareVariable(variable.Name, _generator.TypeToLLVMType(variable.Type, variable.Position), variable.Position);
        }

        unsafe public void DeclareVariable(string name, LLVMTypeRef type, Range position)
        {
            if (IsDeclared(name))
                _generator.Error(position, "Variable already declared");

            SetMemory(name, BuildAlloca(Builder, type, name.ToSbytePointer()));
        }

        unsafe public void StoreVariable(string name)
        {
            BuildStore(Builder, Pop(), GetFromMemory(name));
        }

        private bool IsDeclared(string name)
        {
            return _memory.ContainsKey(name);
        }

        unsafe public void LoadFromMemory(string name, Range position)
        {
            if (!IsDeclared(name))
                _generator.Error(position, "Undeclared variable");

            Load(BuildLoad(Builder, GetFromMemory(name), "".ToSbytePointer()));
        }

        unsafe public void Ret()
        {
            BuildRet(Builder, Pop());
        }

        unsafe public void RetVoid()
        {
            BuildRetVoid(Builder);
        }

        unsafe public void NegInt()
        {
            Load(BuildNeg(Builder, Pop(), "".ToSbytePointer()));
        }

        unsafe public void NegBool()
        {
            Load(BuildNot(Builder, Pop(), "".ToSbytePointer()));
        }

        /// <summary>
        /// buils an array of the parameters to pass and calls the function
        /// </summary>
        unsafe public void Call(LLVMValueRef function, int paramCount)
        {
            var parameters = new LLVMOpaqueValue*[paramCount];

            for (int i = 0; i < paramCount; i++)
                parameters[i] = Pop();

            fixed (LLVMOpaqueValue** @params = parameters)
            {
                var result = BuildCall(Builder, function, @params, (uint)paramCount, "".ToSbytePointer());

                if (GetTypeKind(TypeOf(result)) != LLVMTypeKind.LLVMVoidTypeKind)
                    Load(result);
            }
        }
    }
}
