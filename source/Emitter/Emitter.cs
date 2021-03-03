using LLVMSharp.Interop;
using Mug.Compilation;
using Mug.Models.Parser.NodeKinds.Statements;
using Mug.MugValueSystem;
using System;
using System.Collections.Generic;
using static LLVMSharp.Interop.LLVM;

namespace Mug.Models.Generator.Emitter
{
    internal class MugEmitter
    {
        unsafe public LLVMBuilderRef Builder { get; private set; } = CreateBuilder();

        private readonly Stack<MugValue> _stack = new();
        private readonly IRGenerator _generator;
        public Dictionary<string, MugValue> Memory { get; }

        public MugEmitter(IRGenerator generator, Dictionary<string, MugValue> memory)
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

        public void Load(MugValue value)
        {
            _stack.Push(value);
        }

        public MugValue Pop()
        {
            return _stack.Pop();
        }

        public MugValue Peek()
        {
            return _stack.Peek();
        }

        public MugValueType PeekType()
        {
            return Peek().Type;
        }

        public void AddInt()
        {
            var second = Pop();
            Load(
                MugValue.From(Builder.BuildAdd(Pop().LLVMValue, second.LLVMValue), second.Type)
                );
        }

        public void SubInt()
        {
            var second = Pop();
            Load(
                MugValue.From(Builder.BuildSub(Pop().LLVMValue, second.LLVMValue), second.Type)
                );
        }

        public void MulInt()
        {
            var second = Pop();
            Load(
                MugValue.From(Builder.BuildMul(Pop().LLVMValue, second.LLVMValue), second.Type)
                );
        }

        public void DivInt()
        {
            var second = Pop();
            Load(
                MugValue.From(Builder.BuildSDiv(Pop().LLVMValue, second.LLVMValue), second.Type));
        }

        public void CastInt(MugValueType type)
        {
            Load(
                MugValue.From(Builder.BuildIntCast(Pop().LLVMValue, type.LLVMType), type)
                );
        }

        private MugValue GetFromMemory(string name, Range position)
        {
            if (!Memory.TryGetValue(name, out var variable))
                _generator.Error(position, "Undeclared variable");

            return variable;
        }

        private void SetMemory(string name, MugValue value)
        {
            Memory.Add(name, value);
        }

        public void DeclareVariable(VariableStatement variable)
        {
            DeclareVariable(
                variable.Name,
                variable.Type.ToMugType(variable.Position, _generator.NotSupportedType<MugValueType>),
                variable.Position);
        }

        public void DeclareVariable(string name, MugValueType type, Range position)
        {
            if (IsDeclared(name))
                _generator.Error(position, "Variable already declared");

            SetMemory(name,
                MugValue.From(Builder.BuildAlloca(type.LLVMType, name), type)
                );
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
            if (!variable.IsAllocaInstruction())
                _generator.Error(position, "Unable to change the value of a constant");

            _generator.ExpectSameTypes(
                variable.Type,
                position,
                $"Expected {variable.Type} type, got {PeekType()} type",
                PeekType());

            Builder.BuildStore(Pop().LLVMValue, variable.LLVMValue);
        }

        public bool IsDeclared(string name)
        {
            return Memory.ContainsKey(name);
        }

        public void LoadFromMemory(string name, Range position)
        {
            var variable = GetFromMemory(name, position);

            // variable
            if (variable.IsAllocaInstruction())
                Load(MugValue.From(Builder.BuildLoad(variable.LLVMValue), variable.Type));
            else // constant
                Load(variable);
        }

        public void CallOperator(string op, Range position, params MugValueType[] types)
        {
            var function = _generator.GetSymbol($"{op}({string.Join(", ", types)})", position);
            
            // check the operator overloading is not void
            _generator.ExpectNonVoidType(function.Type.LLVMType.ElementType, position);

            Call(function.LLVMValue, types.Length, function.Type);
        }

        public void Ret()
        {
            Builder.BuildRet(Pop().LLVMValue);
        }

        public void RetVoid()
        {
            Builder.BuildRetVoid();
        }

        public void CompareInt(LLVMIntPredicate kind)
        {
            var second = Pop();
            Load(MugValue.From(Builder.BuildICmp(kind, Pop().LLVMValue, second.LLVMValue), MugValueType.Bool));
        }

        public void NegInt()
        {
            var value = Pop();
            Load(MugValue.From(Builder.BuildNeg(value.LLVMValue), value.Type));
        }

        public void NegBool()
        {
            Load(MugValue.From(Builder.BuildNot(Pop().LLVMValue), MugValueType.Bool));
        }

        /// <summary>
        /// buils an array of the parameters to pass and calls the function
        /// </summary>
        public void Call(LLVMValueRef function, int paramCount, MugValueType returnType)
        {
            var parameters = new MugValue[paramCount];

            for (int i = 0; i < paramCount; i++)
                // paramcount - current index (the last - i) - 1 (offset)
                parameters[paramCount-i-1] = Pop();

            var result = Builder.BuildCall(function, _generator.MugValuesToLLVMValues(parameters));

            if (result.TypeOf.Kind != LLVMTypeKind.LLVMVoidTypeKind)
                Load(MugValue.From(result, returnType));
        }

        public void CompareJump(LLVMBasicBlockRef ifbody, LLVMBasicBlockRef elsebody)
        {
            Builder.BuildCondBr(Pop().LLVMValue, ifbody, elsebody);
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

        public MugValueType PeekTypeFromMemory(string name, Range position)
        {
            return GetFromMemory(name, position).Type;
        }

        public bool IsConstant(string name, Range position)
        {
            return !GetFromMemory(name, position).IsAllocaInstruction();
        }

        public void SelectArrayElement()
        {
            /*var index = Pop();
            var array = Pop();

            Load(
                // load from pointer
                Builder.BuildLoad(
                    // selecting the element
                    Builder.BuildGEP(
                        array.LLVMValue,
                        new[]
                        {
                            index.LLVMValue,
                        })
                    )
                );*/
        }

        internal void Load(LLVMValueRef lLVMValueRef)
        {
            throw new NotImplementedException();
        }
    }
}
