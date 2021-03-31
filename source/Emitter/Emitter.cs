using LLVMSharp.Interop;
using Mug.Compilation;
using Mug.Models.Lexer;
using Mug.Models.Parser;
using Mug.Models.Parser.NodeKinds.Statements;
using Mug.MugValueSystem;
using System;
using System.Collections.Generic;

namespace Mug.Models.Generator.Emitter
{
    internal class MugEmitter
    {
        public unsafe LLVMBuilderRef Builder { get; private set; } = LLVM.CreateBuilder();

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

        public MugValue GetMemoryAllocation(string name, Range position, bool loadreference = false)
        {
            if (!Memory.TryGetValue(name, out var variable))
            {
                _generator.Error(position, "Undeclared member");
                // variable = _generator.EvaluateFunction(name, );
                throw new();
            }

            if (loadreference && variable.Type.TypeKind == MugValueTypeKind.Reference)
                variable = MugValue.From(Builder.BuildLoad(variable.LLVMValue), variable.Type.PointerBaseElementType);

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
                variable.Type.ToMugValueType(_generator),
                variable.Position);
        }

        public (MugValueType, MugValueType) GetCoupleTypes()
        {
            var second = Pop();
            var secondType = second.Type;
            var firstType = PeekType();

            Load(second);

            return (firstType, secondType);
        }

        private void CoerceCoupleConstantIntSize()
        {
            var second = Pop();
            var first = Pop();

            if (second.Type.MatchIntType() && first.Type.MatchIntType())
            {
                if (second.IsConstant())
                    second = MugValue.From(Builder.BuildIntCast(second.LLVMValue, first.Type.LLVMType), first.Type);
                else if (first.IsConstant())
                    first = MugValue.From(Builder.BuildIntCast(first.LLVMValue, second.Type.LLVMType), second.Type);
            }

            Load(first);
            Load(second);
        }

        private void CoerceCoupleConstantFloatSize()
        {
            var second = Pop();
            var first = Pop();

            if (second.Type.MatchFloatType() && first.Type.MatchFloatType())
            {
                if (second.IsConstant())
                    second = MugValue.From(Builder.BuildFPCast(second.LLVMValue, first.Type.LLVMType), first.Type);
                else if (first.IsConstant())
                    first = MugValue.From(Builder.BuildFPCast(first.LLVMValue, second.Type.LLVMType), second.Type);
            }

            Load(first);
            Load(second);
        }

        private void CoerceCoupleConstantFloatIntSize()
        {
            var second = Pop();
            var first = Pop();

            if ((second.Type.MatchFloatType() && first.Type.MatchIntType()) || (first.Type.MatchFloatType() && second.Type.MatchIntType()))
            {
                if (second.IsConstant() && second.Type.MatchIntType())
                {
                    second = MugValue.From(Builder.BuildFPCast(second.LLVMValue, first.Type.LLVMType), first.Type);
                }
                else if (first.IsConstant() && first.Type.MatchIntType())
                    first = MugValue.From(Builder.BuildFPCast(first.LLVMValue, second.Type.LLVMType), second.Type);
            }

            Load(first);
            Load(second);
        }

        public void CoerceCoupleConstantSize()
        {
            CoerceCoupleConstantIntSize();
            CoerceCoupleConstantFloatSize();
            CoerceCoupleConstantFloatIntSize();
        }

        private void CoerceConstantIntSizeTo(MugValueType type)
        {
            var value = Pop();

            if (value.Type.MatchIntType() && value.IsConstant() && type.MatchIntType())
                value = MugValue.From(Builder.BuildIntCast(value.LLVMValue, type.LLVMType), type);

            Load(value);
        }

        private void CoerceConstantFloatSizeTo(MugValueType type)
        {
            var value = Pop();

            if (value.Type.MatchFloatType() && value.IsConstant() && type.MatchFloatType())
                value = MugValue.From(Builder.BuildFPCast(value.LLVMValue, type.LLVMType), type);

            Load(value);
        }

        public void CoerceConstantSizeTo(MugValueType type)
        {
            CoerceConstantIntSizeTo(type);
            CoerceConstantFloatSizeTo(type);
        }

        private void MakeConst()
        {
            var value = Pop();

            value.IsConst = true;

            Load(value);
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

            MakeConst();

            SetMemory(name, Pop());
        }

        public void StoreVariable(string name, Range position, Range bodyPosition)
        {
            StoreVariable(GetMemoryAllocation(name, position), position, bodyPosition);
        }

        public void InitializeParameter(string name, LLVMValueRef llvmparameter)
        {
            Builder.BuildStore(llvmparameter, Memory[name].LLVMValue);
        }

        public void StoreVariable(MugValue allocation, Range position, Range bodyPosition)
        {
            // check it is a variable and not a constant
            if (allocation.IsConst)
                _generator.Error(position, "Unable to change the value of a constant");

            CoerceConstantSizeTo(allocation.Type);

            _generator.ExpectSameTypes(
                allocation.Type,
                bodyPosition,
                $"Expected {allocation.Type} type, got {PeekType()} type",
                PeekType());

            Builder.BuildStore(Pop().LLVMValue, allocation.LLVMValue);
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

        public void UndeclareIFExists(string value)
        {
            if (value is not null && IsDeclared(value))
                Memory.Remove(value);
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
            if (!variable.IsConst)
            {
                if (variable.Type.IsPointer())
                    EmitGCIncrementReferenceCounter(variable.LLVMValue);

                Load(MugValue.From(Builder.BuildLoad(variable.LLVMValue), variable.Type));
            }
            else // constant
                Load(variable);
        }

        public void CallOperator(string op, Range position, bool expectedNonVoid, params MugValueType[] types)
        {
            var function = _generator.EvaluateFunction(op, null, types, Array.Empty<MugValueType>(), position);

            if (!function.IsFunction())
                _generator.Error(position, "Unable to call this member");

            var functionRetType = function.Type.GetFunction().Item2;

            if (expectedNonVoid)
                // check the operator overloading is not void
                _generator.ExpectNonVoidType(functionRetType.LLVMType, position);

            Call(function.LLVMValue, types.Length, functionRetType, false);
        }

        public void CallAsOperator(Range position, MugValueType type, MugValueType returntype)
        {
            var function = _generator.EvaluateFunction($"as({type}): {returntype}", null, Array.Empty<MugValueType>(), Array.Empty<MugValueType>(), position, true);

            if (!function.IsFunction())
                _generator.Error(position, "Unable to call this member");

            Call(function.LLVMValue, 1, returntype, false);
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
        public void Call(LLVMValueRef function, int paramCount, MugValueType returnType, bool hasbase)
        {
            var baseoffset = Convert.ToInt32(hasbase);
            var parameters = new List<MugValue>(paramCount + baseoffset);

            if (hasbase)
                parameters.Insert(0, Pop());

            for (int i = 0; i < paramCount; i++)
                parameters.Insert(baseoffset, Pop());

            var result = Builder.BuildCall(function, _generator.MugValuesToLLVMValues(parameters.ToArray()));

            if (result.TypeOf.Kind != LLVMTypeKind.LLVMVoidTypeKind)
                Load(MugValue.From(result, returnType));
        }

        public void CastEnumMemberToBaseType(MugValueType type)
        {
            Load(MugValue.From(Pop().LLVMValue, type));
        }

        public void CastToEnumMemberFromBaseType(MugValueType enumerated)
        {
            Load(MugValue.From(Pop().LLVMValue, enumerated));
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
            var enumerated = _generator.GetSymbol(enumname, position).GetValue<MugValue>();

            if (!enumerated.Type.IsEnum())
            {
                if (enumerated.Type.IsEnumError())
                {
                    var enumerror = enumerated.Type.GetEnumError();
                    var index = enumerror.Body.FindIndex(member => member.Value == membername);

                    if (index == -1)
                        _generator.Error(position, "`", enumname, "` does not contain a definition for `", membername, "`");

                    Load(MugValue.EnumMember(enumerated.Type, LLVMValueRef.CreateConstInt(LLVMTypeRef.Int8, (uint)index)));
                    return;
                }
                else
                    _generator.Error(position, "Not an enum");
            }

            var type = enumerated.Type.GetEnum();

            Load(type.GetMemberValueFromName(enumerated.Type, type.BaseType.ToMugValueType(_generator), membername, position, localgenerator));
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
            
            if (instance.Type.TypeKind == MugValueTypeKind.Reference)
                instance = MugValue.From(Builder.BuildLoad(instance.LLVMValue), instance.Type.PointerBaseElementType);
            
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

        public void ExpectIndexerType(Range position)
        {
            if (!PeekType().MatchIntType())
                _generator.Error(position, "`", PeekType().ToString(), "` is not an indexer type");
        }

        public void LoadReference(MugValue allocation, Range position)
        {
            if (allocation.IsConst)
                _generator.Error(position, "Unable to take the address of a constant value");

            Load(MugValue.From(allocation.LLVMValue, MugValueType.Reference(allocation.Type)));
        }

        public MugValue LoadFromPointer(MugValue value, Range position)
        {
            if (!value.Type.IsPointer())
                _generator.Error(position, "Expected a pointer");

            return MugValue.From(Builder.BuildLoad(value.LLVMValue), value.Type.PointerBaseElementType);
        }

        public void StoreInsidePointer(MugValue ptr)
        {
            Builder.BuildStore(Pop().LLVMValue, ptr.LLVMValue);
        }

        public void StoreElementArray(LLVMValueRef arrayload, int i)
        {
            var ptr = Builder.BuildGEP(arrayload, new[]
            {
                LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, (uint)i)
            });
            
            Builder.BuildStore(Pop().LLVMValue, ptr);
        }

        public void MakePostfixIntOperation(Func<LLVMValueRef, LLVMValueRef, string, LLVMValueRef> operation)
        {
            var target = Pop();
            var result = operation(Builder.BuildLoad(target.LLVMValue), LLVMValueRef.CreateConstInt(target.Type.LLVMType, 1), "");

            Builder.BuildStore(result, target.LLVMValue);
        }

        public void OperateInsidePointer(MugValue target)
        {
            Builder.BuildStore(Pop().LLVMValue, target.LLVMValue);
        }

        public void SelectArrayElement(bool buildload)
        {
            var index = Pop();
            var array = Pop();

            // selecting the element
            var ptr = Builder.BuildGEP( array.LLVMValue, new[] { index.LLVMValue });

            if (buildload) // load from pointer 
                ptr = Builder.BuildLoad(ptr);

            Load(MugValue.From(ptr, array.Type.ArrayBaseElementType));
        }

        public void Swap()
        {
            var second = Pop();
            var first = Pop();
            Load(second);
            Load(first);
        }

        public void DeclareBaseReference(string name, LLVMValueRef value, MugValueType type)
        {
            var tmp = Builder.BuildAlloca(type.LLVMType);
            Builder.BuildStore(value, tmp);
            SetMemory(name, MugValue.From(tmp, type, true));
        }

        public void AddFloat()
        {
            var second = Pop();
            Load(
                MugValue.From(Builder.BuildFAdd(Pop().LLVMValue, second.LLVMValue), second.Type));
        }

        public void SubFloat()
        {
            var second = Pop();
            Load(
                MugValue.From(Builder.BuildFSub(Pop().LLVMValue, second.LLVMValue), second.Type));
        }

        public void MulFloat()
        {
            var second = Pop();
            Load(
                MugValue.From(Builder.BuildFMul(Pop().LLVMValue, second.LLVMValue), second.Type));
        }

        public void DivFloat()
        {
            var second = Pop();
            Load(
                MugValue.From(Builder.BuildFDiv(Pop().LLVMValue, second.LLVMValue), second.Type));
        }

        public void NegFloat()
        {
            var value = Pop();
            Load(MugValue.From(Builder.BuildFNeg(value.LLVMValue), value.Type));
        }

        public void CompareFloat(LLVMRealPredicate kind)
        {
            var second = Pop();
            Load(MugValue.From(Builder.BuildFCmp(kind, Pop().LLVMValue, second.LLVMValue), MugValueType.Bool));
        }
    }
}
