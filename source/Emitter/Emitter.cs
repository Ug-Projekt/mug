using LLVMSharp.Interop;
using Mug.Models.Parser.NodeKinds.Statements;
using Mug.MugValueSystem;
using System;
using System.Collections.Generic;
using static LLVMSharp.Interop.LLVM;

namespace Mug.Models.Generator.Emitter
{
    internal class MugEmitter
    {
        public unsafe LLVMBuilderRef Builder { get; private set; } = CreateBuilder();

        private readonly Stack<MugValue> _stack = new();
        private readonly IRGenerator _generator;
        public Dictionary<string, MugValue> Memory { get; }
        internal LLVMBasicBlockRef ExitBlock { get; }
        internal bool IsInsideSubBlock { get; }

        private const string GCMugLibSymbol = "include/standard_symbols/mug_gc";
        private const string GCPointerIncrementStandardSymbol = "gc_ptr_inc";
        private const string GCPointerDecrementStandardSymbol = "gc_ptr_dec";

        public MugEmitter(IRGenerator generator, Dictionary<string, MugValue> memory, LLVMBasicBlockRef exitblock, bool isInsideSubBlock)
        {
            _generator = generator;
            // copying the symbols instead of passing them for reference
            Memory = new(memory);
            ExitBlock = exitblock;
            IsInsideSubBlock = isInsideSubBlock;
        }

        public MugEmitter(IRGenerator generator, LLVMBasicBlockRef exitblock, bool isInsideSubBlock)
        {
            _generator = generator;
            Memory = new();
            ExitBlock = exitblock;
            IsInsideSubBlock = isInsideSubBlock;
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
                MugValue.From(Builder.BuildAdd(Pop().LLVMValue, second.LLVMValue), second.Type));
        }

        public void SubInt()
        {
            var second = Pop();
            Load(
                MugValue.From(Builder.BuildSub(Pop().LLVMValue, second.LLVMValue), second.Type));
        }

        public void MulInt()
        {
            var second = Pop();
            Load(
                MugValue.From(Builder.BuildMul(Pop().LLVMValue, second.LLVMValue), second.Type));
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
                MugValue.From(Builder.BuildIntCast(Pop().LLVMValue, type.LLVMType), type));
        }

        public MugValue GetMemoryAllocation(string name, Range position)
        {
            if (!Memory.TryGetValue(name, out var variable))
                variable = _generator.GetSymbol(name, position);

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
                variable.Type.ToMugValueType(variable.Position, _generator),
                variable.Position);
        }

        public bool OneOfTwoIsAConstant()
        {
            var second = Pop();
            var first = Peek();
            Load(second);

            return second.LLVMValue.IsConstant || first.LLVMValue.IsConstant;
        }

        public void DeclareVariable(string name, MugValueType type, Range position)
        {
            if (IsDeclared(name))
                _generator.Error(position, "Variable already declared");

            SetMemory(
                name,
                MugValue.From(Builder.BuildAlloca(type.LLVMType, name), type));
        }

        public void DeclareConstant(string name, Range position)
        {
            if (IsDeclared(name))
                _generator.Error(position, "Variable already declared");

            SetMemory(name, Pop());
        }

        public void StoreVariable(string name, Range position)
        {
            var variable = GetMemoryAllocation(name, position);

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

        public bool OneOfTwoIsOnlyTheEnumType()
        {
            var second = Pop();
            var first = Peek();
            Load(second);

            return second.LLVMValue.Handle == IntPtr.Zero || first.LLVMValue.Handle == IntPtr.Zero;
        }

        public bool IsDeclared(string name)
        {
            return Memory.ContainsKey(name);
        }

        private void EmitGCIncrementReferenceCounter(LLVMValueRef pointer)
        {
            /*Builder.BuildCall(
                _generator.RequireStandardSymbol(GCPointerIncrementStandardSymbol, GCMugLibSymbol).LLVMValue,
                new[] { pointer });*/
        }

        public void EmitGCDecrementReferenceCounter()
        {
            Pop();
            /*Builder.BuildCall(
                _generator.RequireStandardSymbol(GCPointerDecrementStandardSymbol, GCMugLibSymbol).LLVMValue,
                new[] { Pop().LLVMValue });*/
        }

        public void LoadFromMemory(string name, Range position)
        {
            var variable = GetMemoryAllocation(name, position);

            // variable
            if (variable.IsAllocaInstruction())
            {
                if (variable.Type.IsPointer())
                    EmitGCIncrementReferenceCounter(variable.LLVMValue);

                Load(MugValue.From(Builder.BuildLoad(variable.LLVMValue), variable.Type));
            }
            else // constant
                Load(variable);
        }

        public void CallOperator(string op, Range position, params MugValueType[] types)
        {
            var function = _generator.GetSymbol($"{op}({string.Join(", ", types)})", position);

            // check the operator overloading is not void
            _generator.ExpectNonVoidType(function.Type.LLVMType, position);

            Call(function.LLVMValue, types.Length, function.Type);
        }

        public void CallAsOperator(Range position, MugValueType type, MugValueType returntype)
        {
            var function = _generator.GetSymbol($"as({type}): {returntype}", position);

            Call(function.LLVMValue, 1, returntype);
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
                parameters[paramCount - i - 1] = Pop();

            var result = Builder.BuildCall(function, _generator.MugValuesToLLVMValues(parameters));

            if (result.TypeOf.Kind != LLVMTypeKind.LLVMVoidTypeKind)
                Load(MugValue.From(result, returnType));
        }

        public void CastEnumMemberToBaseType(MugValueType type)
        {
            Load(MugValue.From(Pop().LLVMValue, type));
        }

        public void CastToEnumMemberFromBaseType(MugValueType enumerable)
        {
            Load(MugValue.From(Pop().LLVMValue, enumerable));
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

        public void LoadMemoryAllocation(string name, Range position)
        {
            var allocation = GetMemoryAllocation(name, position);
            if (!allocation.IsAllocaInstruction())
            {
                var tmp = Builder.BuildAlloca(allocation.Type.LLVMType);
                Builder.BuildStore(allocation.LLVMValue, tmp);

                allocation = MugValue.From(tmp, allocation.Type);
            }

            Load(allocation);
        }

        public MugValueType PeekTypeFromMemory(string name, Range position)
        {
            return GetMemoryAllocation(name, position).Type;
        }

        public void StoreField(LLVMValueRef tmp, int index)
        {
            Builder.BuildStore(
                Pop().LLVMValue,
                Builder.BuildGEP(
                    tmp,
                    new[]
                    {
                        LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0),
                        LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, (uint)index)
                    })
                );
        }

        public void LoadEnumMember(string enumname, string membername, Range position, LocalGenerator localgenerator)
        {
            var enumerable = _generator.GetSymbol(enumname, position);

            if (!enumerable.Type.IsEnum())
                _generator.Error(position, "Not an enum");

            var type = enumerable.Type.GetEnum();

            Load(type.GetMemberValueFromName(enumerable.Type, type.BaseType.ToMugValueType(position, localgenerator._generator), membername, position, localgenerator));
        }

        public void LoadField(MugValue instance, MugValueType fieldType, int index, bool load)
        {
            var field = Builder.BuildGEP(
                instance.LLVMValue,
                new[]
                {
                    LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0),
                    LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, (uint)index)
                });

            Load(
                MugValue.From(
                    load ? Builder.BuildLoad(field) : field,
                    fieldType)
                );
        }

        public void LoadFieldName(string name, Range position)
        {
            var instance = GetMemoryAllocation(name, position);

            if (!instance.IsAllocaInstruction())
            {
                var tmp = Builder.BuildAlloca(instance.Type.LLVMType);
                Builder.BuildStore(instance.LLVMValue, tmp);
                instance.LLVMValue = tmp;
            }

            Load(instance);
        }

        public void LoadFieldName()
        {
            var value = Pop();

            if (value.LLVMValue.IsALoadInst.Handle != IntPtr.Zero)
                Load(MugValue.From(value.LLVMValue.GetOperand(0), value.Type));
            else if (value.LLVMValue.IsACallInst.Handle != IntPtr.Zero)
            {
                var tmp = Builder.BuildAlloca(value.Type.LLVMType);

                Builder.BuildStore(value.LLVMValue, tmp);
                Load(MugValue.From(tmp, value.Type));
            }
            else
                Load(value);
        }

        public void Exit()
        {
            if (Builder.InsertBlock.Terminator.Handle == IntPtr.Zero)
                Builder.BuildBr(ExitBlock);
        }

        public void StoreInside(MugValue field)
        {
            Builder.BuildStore(Pop().LLVMValue, field.LLVMValue);
        }

        public void LoadUnknownAllocation(MugValue allocation)
        {
            if (allocation.IsAllocaInstruction() || allocation.IsGEP())
                Load(MugValue.From(Builder.BuildLoad(allocation.LLVMValue), allocation.Type));
            else
                throw new("unable to recognize unknonwn allocation");
        }

        public void SelectArrayElement()
        {
            var index = Pop();
            var array = Pop();

            Load(
                MugValue.From(
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
                    , array.Type.ArrayBaseElementType)
                );
        }
    }
}
