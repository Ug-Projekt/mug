using LLVMSharp.Interop;
using Mug.Compilation;
using Mug.Models.Parser.NodeKinds.Statements;
using System;
using System.Collections.Generic;
using static LLVMSharp.Interop.LLVM;

namespace Mug.Models.Generator.Emitter
{
    public class MugEmitter
    {
        unsafe public LLVMBuilderRef Builder { get; private set; } = CreateBuilder();

        private readonly Stack<LLVMValueRef> _stack = new();
        public Dictionary<string, LLVMValueRef> Memory { get; }
        private readonly IRGenerator _generator;

        public MugEmitter(IRGenerator generator, Dictionary<string, LLVMValueRef> memory)
        {
            _generator = generator;
            // copying the symbols instead of passing them for reference
            Memory = new(memory);
        }

        public MugEmitter(IRGenerator generator)
        {
            _generator = generator;
            Memory = new();
        }

        public void Load(LLVMValueRef value)
        {
            _stack.Push(value);
        }

        public LLVMValueRef Pop()
        {
            return _stack.Pop();
        }

        public LLVMValueRef Peek()
        {
            return _stack.Peek();
        }

        public LLVMTypeRef PeekType()
        {
            return Peek().TypeOf;
        }

        public void AddInt()
        {
            var second = Pop();
            Load(Builder.BuildAdd(Pop(), second));
        }

        public void SubInt()
        {
            var second = Pop();
            Load(Builder.BuildSub(Pop(), second));
        }

        public void MulInt()
        {
            var second = Pop();
            Load(Builder.BuildMul(Pop(), second));
        }

        public void DivInt()
        {
            var second = Pop();
            Load(Builder.BuildFDiv(Pop(), second));
        }

        public void CastInt(LLVMTypeRef type)
        {
            Load(Builder.BuildIntCast(Pop(), type));
        }

        private LLVMValueRef GetFromMemory(string name, Range position)
        {
            if (!Memory.TryGetValue(name, out var variable))
                _generator.Error(position, "Undeclared variable");

            return variable;
        }

        private void SetMemory(string name, LLVMValueRef value)
        {
            Memory.Add(name, value);
        }

        public void DeclareVariable(VariableStatement variable)
        {
            DeclareVariable(variable.Name, _generator.TypeToLLVMType(variable.Type, variable.Position), variable.Position);
        }

        public void DeclareVariable(string name, LLVMTypeRef type, Range position)
        {
            if (IsDeclared(name))
                _generator.Error(position, "Variable already declared");

            SetMemory(name, Builder.BuildAlloca(type, name));
        }

        public void DeclareConstant(string name, Range position)
        {
            if (IsDeclared(name))
                _generator.Error(position, "Variable already declared");

            SetMemory(name, Pop());
        }

        public void StoreVariable(string name, Range position)
        {
            var variable = GetFromMemory(name, position);

            // check it is a variable and not a constant
            if (variable.IsAAllocaInst.Handle == IntPtr.Zero)
                _generator.Error(position, "Unable to change the value of a constant");

            _generator.ExpectSameTypes(variable.TypeOf.ElementType, position, $"Expected {variable.TypeOf.ElementType.ToMugTypeString()} type, got {PeekType().ToMugTypeString()} type", PeekType());

            Builder.BuildStore(Pop(), variable);
        }

        public bool IsDeclared(string name)
        {
            return Memory.ContainsKey(name);
        }

        public void LoadFromMemory(string name, Range position)
        {
            var variable = GetFromMemory(name, position);

            // check if is not a constant
            if (variable.IsAAllocaInst.Handle != IntPtr.Zero)
                variable = Builder.BuildLoad(variable);

            Load(variable);
        }

        public void CallOperator(string op, Range position, params LLVMTypeRef[] types)
        {
            var function = _generator.GetSymbol($"{op}({string.Join(", ", types)})", position);
            
            // check the operator overloading is not void
            _generator.ExpectNonVoidType(function.TypeOf.ElementType.ReturnType, position);

            Call(function, types.Length);
        }

        public void Ret()
        {
            Builder.BuildRet(Pop());
        }

        public void RetVoid()
        {
            Builder.BuildRetVoid();
        }

        public void CompareInt(LLVMIntPredicate kind)
        {
            var second = Pop();
            Load(Builder.BuildICmp(kind, Pop(), second));
        }

        public void NegInt()
        {
            Load(Builder.BuildNeg(Pop()));
        }

        public void NegBool()
        {
            Load(Builder.BuildNot(Pop()));
        }

        /// <summary>
        /// buils an array of the parameters to pass and calls the function
        /// </summary>
        public void Call(LLVMValueRef function, int paramCount)
        {
            var parameters = new LLVMValueRef[paramCount];

            for (int i = 0; i < paramCount; i++)
                parameters[i] = Pop();

            var result = Builder.BuildCall(function, parameters, "");

            if (result.TypeOf.Kind != LLVMTypeKind.LLVMVoidTypeKind)
                Load(result);
        }

        public void CompareJump(LLVMBasicBlockRef ifbody, LLVMBasicBlockRef elsebody)
        {
            Builder.BuildCondBr(Pop(), ifbody, elsebody);
        }

        public void JumpOutOfScope(LLVMValueRef terminator, LLVMBasicBlockRef targetblock)
        {
            // check if the block has not terminator yet
            if (terminator.Handle == IntPtr.Zero)
                Jump(targetblock);
        }

        public void Jump(LLVMBasicBlockRef targetblock)
        {
            Builder.BuildBr(targetblock);
        }
    }
}
