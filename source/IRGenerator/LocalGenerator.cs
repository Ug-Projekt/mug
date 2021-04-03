﻿using LLVMSharp;
using LLVMSharp.Interop;
using Mug.Compilation;
using Mug.Compilation.Symbols;
using Mug.Models.Generator.Emitter;
using Mug.Models.Lexer;
using Mug.Models.Lowerering;
using Mug.Models.Parser;
using Mug.Models.Parser.NodeKinds;
using Mug.Models.Parser.NodeKinds.Statements;
using Mug.MugValueSystem;
using Mug.TypeSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mug.Models.Generator
{
    public class LocalGenerator
    {
        // code emitter
        private MugEmitter _emitter;
        // function info
        private readonly FunctionNode _function;
        // pointers
        internal readonly IRGenerator _generator;

        private readonly LLVMValueRef _llvmfunction;
        private LLVMBasicBlockRef _oldcondition;
        private LLVMBasicBlockRef CycleExitBlock { get; set; }
        private LLVMBasicBlockRef CycleCompareBlock { get; set; }

        internal LocalGenerator(IRGenerator errorHandler, ref LLVMValueRef llvmfunction, ref FunctionNode function, ref MugEmitter emitter)
        {
            _generator = errorHandler;
            _emitter = emitter;
            _function = function;
            _llvmfunction = llvmfunction;
        }

        internal void Error(Range position, string error)
        {
            _generator.Parser.Lexer.Throw(position, error);
        }

        internal bool Report(Range position, string error)
        {
            _generator.Parser.Lexer.DiagnosticBag.Report(position, error);
            return false;
        }

        internal void Stop()
        {
            _generator.Parser.Lexer.CheckDiagnostic();
        }

        private LLVMValueRef CreateConstString(string value)
        {
            return _emitter.Builder.BuildGEP(
                    _emitter.Builder.BuildGlobalString(value),
                    new[]
                    {
                        LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0),
                        LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0),
                    }
                );
        }

        /// <summary>
        /// converts a constant in token format to one in LLVMValueRef format
        /// </summary>
        internal MugValue ConstToMugConst(Token constant, Range position, bool isenum = false, MugValueType forcedIntSize = new())
        {
            LLVMValueRef llvmvalue = new();
            MugValueType type = new();

            switch (constant.Kind)
            {
                case TokenKind.ConstantDigit:
                    if (isenum)
                    {
                        llvmvalue = LLVMValueRef.CreateConstInt(forcedIntSize.LLVMType, Convert.ToUInt64(constant.Value));
                        type = forcedIntSize;
                    }
                    else
                    {
                        llvmvalue = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, Convert.ToUInt64(constant.Value));
                        type = MugValueType.Int32;
                    }
                    break;
                case TokenKind.ConstantBoolean:
                    llvmvalue = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int1, _generator.StringBoolToIntBool(constant.Value));
                    type = MugValueType.Bool;
                    break;
                case TokenKind.ConstantString:
                    llvmvalue = CreateConstString(constant.Value);
                    type = MugValueType.String;
                    break;
                case TokenKind.ConstantChar:
                    llvmvalue = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int8, _generator.StringCharToIntChar(constant.Value));
                    type = MugValueType.Char;
                    break;
                case TokenKind.ConstantFloatDigit:
                    llvmvalue = LLVMValueRef.CreateConstUIToFP(LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, (uint)float.Parse(constant.Value)), LLVMTypeRef.Float);
                    type = MugValueType.Float32;
                    break;
                default:
                    _generator.NotSupportedType<LLVMValueRef>(constant.Kind.ToString(), position);
                    break;
            }

            return MugValue.From(llvmvalue, type, isconstant: true);
        }

        private void EmitSum(Range position)
        {
            _emitter.CoerceCoupleConstantSize();

            var types = _emitter.GetCoupleTypes();

            if (types.Item1.MatchSameIntType(types.Item2))
                _emitter.AddInt();
            else if (types.Item1.MatchSameFloatType(types.Item2))
                _emitter.AddFloat();
            else
                _emitter.CallOperator("+", position, true, types.Item1, types.Item2);
        }

        private void EmitSub(Range position)
        {
            _emitter.CoerceCoupleConstantSize();

            var types = _emitter.GetCoupleTypes();

            if (types.Item1.MatchSameIntType(types.Item2))
                _emitter.SubInt();
            else if (types.Item1.MatchSameFloatType(types.Item2))
                _emitter.SubFloat();
            else
                _emitter.CallOperator("-", position, true, types.Item1, types.Item2);
        }

        private void EmitMul(Range position)
        {
            _emitter.CoerceCoupleConstantSize();

            var types = _emitter.GetCoupleTypes();

            if (types.Item1.MatchSameIntType(types.Item2))
                _emitter.MulInt();
            else if (types.Item1.MatchSameFloatType(types.Item2))
                _emitter.MulFloat();
            else
                _emitter.CallOperator("*", position, true, types.Item1, types.Item2);
        }

        private void EmitDiv(Range position)
        {
            _emitter.CoerceCoupleConstantSize();

            var types = _emitter.GetCoupleTypes();

            if (types.Item1.MatchSameIntType(types.Item2))
                _emitter.DivInt();
            else if (types.Item1.MatchSameFloatType(types.Item2))
                _emitter.DivFloat();
            else
                _emitter.CallOperator("/", position, true, types.Item1, types.Item2);
        }

        private LLVMRealPredicate ToFloatComparePredicate(LLVMIntPredicate intpredicate)
        {
            return intpredicate switch
            {
                LLVMIntPredicate.LLVMIntEQ => LLVMRealPredicate.LLVMRealOEQ,
                LLVMIntPredicate.LLVMIntNE => LLVMRealPredicate.LLVMRealONE,
                LLVMIntPredicate.LLVMIntSGE => LLVMRealPredicate.LLVMRealOGE,
                LLVMIntPredicate.LLVMIntSGT => LLVMRealPredicate.LLVMRealOGT,
                LLVMIntPredicate.LLVMIntSLE => LLVMRealPredicate.LLVMRealOLE,
                LLVMIntPredicate.LLVMIntSLT => LLVMRealPredicate.LLVMRealOLT
            };
        }

        private void EmitBooleanOperator(string literal, LLVMIntPredicate llvmpredicate, OperatorKind kind, Range position)
        {
            _emitter.CoerceCoupleConstantSize();

            var types = _emitter.GetCoupleTypes();
            var ft = types.Item1;
            var st = types.Item2;

            if ((kind == OperatorKind.CompareEQ || kind == OperatorKind.CompareNEQ) && ft.IsSameEnumOf(st))
            {
                if (_emitter.OneOfTwoIsOnlyTheEnumType())
                    Error(position, "Cannot apply boolean operator on this expression");

                var second = _emitter.Pop();
                var first = _emitter.Pop();

                var enumBaseType = st.GetEnum().BaseType.ToMugValueType(_generator);

                _emitter.Load(MugValue.From(first.LLVMValue, enumBaseType));
                _emitter.Load(MugValue.From(second.LLVMValue, enumBaseType));

                EmitBooleanOperator(literal, llvmpredicate, kind, position);
            }
            else if ((kind == OperatorKind.CompareEQ || kind == OperatorKind.CompareNEQ) &&
                ft.TypeKind == MugValueTypeKind.EnumError &&
                st.TypeKind == MugValueTypeKind.EnumError)
                _emitter.CompareInt(llvmpredicate);
            else if (ft.MatchSameIntType(st))
                _emitter.CompareInt(llvmpredicate);
            else if (ft.MatchSameFloatType(st))
                _emitter.CompareFloat(ToFloatComparePredicate(llvmpredicate));
            else if (ft.TypeKind == MugValueTypeKind.Char &&
                st.TypeKind == MugValueTypeKind.Char &&
                (kind == OperatorKind.CompareEQ || kind == OperatorKind.CompareNEQ))
                _emitter.CompareInt(llvmpredicate);
            else if ((kind == OperatorKind.CompareEQ || kind == OperatorKind.CompareNEQ) &&
                ft.TypeKind == MugValueTypeKind.Bool &&
                st.TypeKind == MugValueTypeKind.Bool)
                _emitter.CompareInt(llvmpredicate);
            else
                _emitter.CallOperator(literal, position, true, ft, st);
        }

        /// <summary>
        /// the function manages the operator implementations for all the types
        /// </summary>
        private void EmitOperator(OperatorKind kind, Range position)
        {
            switch (kind)
            {
                case OperatorKind.Sum:
                    EmitSum(position);
                    break;
                case OperatorKind.Subtract:
                    EmitSub(position);
                    break;
                case OperatorKind.Multiply:
                    EmitMul(position);
                    break;
                case OperatorKind.Divide:
                    EmitDiv(position);
                    break;
                case OperatorKind.CompareEQ:
                    EmitBooleanOperator("==", LLVMIntPredicate.LLVMIntEQ, kind, position);
                    break;
                case OperatorKind.CompareNEQ:
                    EmitBooleanOperator("!=", LLVMIntPredicate.LLVMIntNE, kind, position);
                    break;
                case OperatorKind.CompareGreater:
                    EmitBooleanOperator(">", LLVMIntPredicate.LLVMIntSGT, kind, position);
                    break;
                case OperatorKind.CompareGEQ:
                    EmitBooleanOperator(">=", LLVMIntPredicate.LLVMIntSGE, kind, position);
                    break;
                case OperatorKind.CompareLess:
                    EmitBooleanOperator("<", LLVMIntPredicate.LLVMIntSLT, kind, position);
                    break;
                case OperatorKind.CompareLEQ:
                    EmitBooleanOperator("<=", LLVMIntPredicate.LLVMIntSLE, kind, position);
                    break;
                default:
                    Error(position, $"`{kind}` operator not supported yet");
                    break;
            }
        }

        private LLVMValueRef? tmp = null;

        private bool EmitAndOperator(INode left, INode right)
        {
            var tmpisnull = !tmp.HasValue;
            if (tmpisnull)
            {
                tmp = _emitter.Builder.BuildAlloca(LLVMTypeRef.Int1);
                _emitter.Builder.BuildStore(LLVMValueRef.CreateConstInt(LLVMTypeRef.Int1, 0), tmp.Value);
            }

            var iftrue = _llvmfunction.AppendBasicBlock("");
            var @finally = _llvmfunction.AppendBasicBlock("");

            if (!EvaluateExpression(left))
                return false;

            var f = _emitter.Peek();
            _generator.ExpectBoolType(f.Type, left.Position);

            _emitter.CompareJump(iftrue, @finally);
            _emitter.Builder.PositionAtEnd(iftrue);

            if (!EvaluateExpression(right))
                return false;

            var s = _emitter.Pop();
            _generator.ExpectBoolType(s.Type, right.Position);
            _emitter.Builder.BuildStore(s.LLVMValue, tmp.Value);

            _emitter.Jump(@finally);
            _emitter.Builder.PositionAtEnd(@finally);

            _emitter.Load(MugValue.From(_emitter.Builder.BuildLoad(tmp.Value), MugValueType.Bool));

            if (tmpisnull)
                tmp = null;

            return true;
        }

        private bool EmitOrOperator(INode left, INode right)
        {
            var tmpisnull = !tmp.HasValue;
            
            if (tmpisnull)
                tmp = _emitter.Builder.BuildAlloca(LLVMTypeRef.Int1);
            
            var iftrue = _llvmfunction.AppendBasicBlock("");
            var @finally = _llvmfunction.AppendBasicBlock("");

            if (!EvaluateExpression(left))
                return false;

            var f = _emitter.Peek();
            _generator.ExpectBoolType(f.Type, left.Position);
            _emitter.Builder.BuildStore(f.LLVMValue, tmp.Value);

            _emitter.CompareJump(@finally, iftrue);
            _emitter.Builder.PositionAtEnd(iftrue);

            if (!EvaluateExpression(right))
                return false;

            var s = _emitter.Pop();
            _generator.ExpectBoolType(s.Type, right.Position);
            _emitter.Builder.BuildStore(s.LLVMValue, tmp.Value);

            _emitter.Jump(@finally);
            _emitter.Builder.PositionAtEnd(@finally);

            _emitter.Load(MugValue.From(_emitter.Builder.BuildLoad(tmp.Value), MugValueType.Bool));

            if (tmpisnull)
                tmp = null;

            return true;
        }

        private bool IsABitcast(MugValueType expressionType, MugValueType castType)
        {
            return (expressionType.TypeKind == MugValueTypeKind.String && castType.Equals(MugValueType.Array(MugValueType.Char))) ||
                 (castType.TypeKind == MugValueTypeKind.String && expressionType.Equals(MugValueType.Array(MugValueType.Char)))   ||
                 (castType.TypeKind == MugValueTypeKind.Reference && expressionType.Equals(MugValueType.Pointer(castType.PointerBaseElementType))) ||
                 (expressionType.TypeKind == MugValueTypeKind.Reference && castType.Equals(MugValueType.Pointer(expressionType.PointerBaseElementType)));
        }

        /// <summary>
        /// the function manages the 'as' operator
        /// </summary>
        private bool EmitCastInstruction(MugType type, Range position)
        {
            // the expression type to cast
            var expressionType = _emitter.PeekType();
            var castType = type.ToMugValueType(_generator);

            if (castType.RawEquals(expressionType))
                return Report(position, "Useless cast");

            if (castType.IsEnum())
            {
                var enumerated = castType.GetEnum();
                var enumBaseType = enumerated.BaseType.ToMugValueType(_generator);

                _emitter.CoerceConstantSizeTo(enumBaseType);

                if (_emitter.PeekType().TypeKind != enumBaseType.TypeKind)
                    return Report(position, $"The base type of enum `{enumerated.Name}` is incompatible with type `{expressionType}`");

                _emitter.CastToEnumMemberFromBaseType(castType);
            }
            else if (expressionType.TypeKind == MugValueTypeKind.Enum)
            {
                var enumBaseType = expressionType.GetEnum().BaseType.ToMugValueType(_generator);

                _emitter.CoerceConstantSizeTo(enumBaseType);

                if (_emitter.PeekType().GetEnum().BaseType.ToMugValueType(_generator).TypeKind != castType.TypeKind)
                    return Report(type.Position, $"Enum base type is incompatible with type `{castType}`");

                _emitter.CastEnumMemberToBaseType(castType);
            }
            else if (IsABitcast(expressionType, castType))
            {
                var value = _emitter.Pop();
                value.Type = castType;
                _emitter.Load(value);
            }
            else if (castType.TypeKind == MugValueTypeKind.Unknown || expressionType.TypeKind == MugValueTypeKind.Unknown)
            {
                var value = _emitter.Pop();

                if (!value.Type.IsPointer() && !value.Type.IsIndexable() && value.Type.TypeKind != MugValueTypeKind.Unknown)
                    return Report(position, "Expected pointer when in cast expression something is unknown");

                _emitter.Load(MugValue.From(_emitter.Builder.BuildBitCast(value.LLVMValue, castType.LLVMType), castType));
            }
            else if (expressionType.MatchAnyTypeOfIntType() && castType.MatchAnyTypeOfIntType())
                _emitter.CastInt(castType);
            else if (expressionType.MatchIntType() && castType.MatchFloatType())
                _emitter.CastIntToFloat(castType);
            else if (expressionType.MatchFloatType() && castType.MatchFloatType())
                _emitter.CastFloat(castType);
            else if (expressionType.MatchFloatType() && castType.MatchIntType())
                _emitter.CastFloatToInt(castType);
            else
                _emitter.CallAsOperator(position, expressionType, type.ToMugValueType(_generator));

            return true;
        }

        /// <summary>
        /// wip function
        /// the function evaluates an instance node, for example: base.method()
        /// </summary>
        private bool EvaluateMemberAccess(INode member, bool load)
        {
            switch (member)
            {
                case MemberNode m:
                    if (!EvaluateMemberAccess(m.Base, load))
                        return false;

                    var structure = _emitter.PeekType().GetStructure();
                    _emitter.LoadField(
                        _emitter.Pop(),
                        structure.GetFieldTypeFromName(m.Member.Value, _generator, m.Member.Position),
                        structure.GetFieldIndexFromName(m.Member.Value, _generator, m.Member.Position), load);
                    break;
                case Token t:
                    if (load)
                    {
                        if (!_emitter.LoadFromMemory(t.Value, t.Position))
                            return false;
                    }
                    else
                    {
                        if (!_emitter.LoadMemoryAllocation(t.Value, t.Position))
                            return false;
                    }
                    break;
                case ArraySelectElemNode a:
                    return EmitExprArrayElemSelect(a);
                case PrefixOperator p:
                    if (!EvaluateMemberAccess(p.Expression, p.Prefix == TokenKind.Star))
                        return false;

                    if (p.Prefix == TokenKind.Star)
                        _emitter.Load(_emitter.LoadFromPointer(_emitter.Pop(), p.Position));
                    else if (p.Prefix == TokenKind.BooleanAND)
                    {
                        if (!EvaluateMemberAccess(p.Expression, load))
                            return false;

                        if (!_emitter.LoadReference(_emitter.Pop(), p.Position))
                            return false;
                    }
                    else
                        return Report(p.Position, "In member access, the base must be a non-expression");

                    break;
                default:
                    Error(member.Position, "Not supported yet");
                    break;
            }

            return true;
        }

        private FunctionIdentifier EvaluateFunctionCallName(INode leftexpression, MugValueType[] parameters, MugValueType[] genericsInput, out bool hasbase)
        {
            string name;
            Range position;
            hasbase = false;

            if (leftexpression is Token token)
            {
                name = token.Value;
                position = token.Position;
            }
            else if (leftexpression is MemberNode member) // (expr).function()
            {
                if (!EvaluateExpression(member.Base, false))
                    return null;

                name = member.Member.Value;
                position = member.Member.Position;
                hasbase = true;
            }
            else // (expr)()
            {
                EvaluateExpression(leftexpression);
                throw new(); // tofix
            }

            return _generator.EvaluateFunction(name, hasbase ? new MugValueType?(_emitter.PeekType()) : null, parameters, genericsInput, position);
        }

        /// <summary>
        /// the function converts a Callstatement node to the corresponding low-level code
        /// </summary>
        private bool EmitCallStatement(CallStatement c, bool expectedNonVoid, bool isInCatch = false)
        {
            // an array is prepared for the parameter types of function to call
            var parameters = new MugValueType[c.Parameters.Length];
            var paramsAreOk = true;

            /* the array is cycled with the expressions of the respective parameters and each expression
             * is evaluated and assigned its type to the array of parameter types
             */
            for (int i = 0; i < c.Parameters.Length; i++)
            {
                paramsAreOk &= EvaluateExpression(c.Parameters.Nodes[i]);

                if (paramsAreOk)
                    parameters[i] = _emitter.PeekType();
            }

            /*
             * the symbol of the function is taken by passing the name of the complete function which consists
             * of the function id and in brackets the list of parameter types separated by ', '
             */

            if (IsBuiltInFunction(c.Name, c.Generics, parameters, out var comptimeExecute))
            {
                comptimeExecute(c.Generics, parameters);
                return true;
            }
            
            var function = EvaluateFunctionCallName(c.Name, parameters, _generator.MugTypesToMugValueTypes(c.Generics), out bool hasbase);

            if (function is null || !paramsAreOk)
                return false;

            // function type: <ret_type> <param_types>
            var functionType = function.ReturnType;

            if (expectedNonVoid)
                _generator.ExpectNonVoidType(
                    // (<ret_type> <param_types>).GetElementType() -> <ret_type>
                    functionType.LLVMType,
                    c.Position);

            _emitter.Call(function.Value.LLVMValue, c.Parameters.Length, functionType, hasbase);

            if (!isInCatch && functionType.TypeKind == MugValueTypeKind.EnumErrorDefined)
                return Report(c.Position, "Uncatched enum error");
            else if (isInCatch && functionType.TypeKind != MugValueTypeKind.EnumErrorDefined)
                return Report(c.Position, "Catched a non enum error");

            return true;
        }

        private void CompTime_sizeof(List<MugType> generics, MugValueType[] parameters)
        {
            var size = generics[0].ToMugValueType(_generator).Size(_generator.SizeOfPointer);

            _emitter.Load(MugValue.From(LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, (uint)size), MugValueType.Int32));
        }

        private bool IsBuiltInFunction(INode name, List<MugType> generics, MugValueType[] parameters, out Action<List<MugType>, MugValueType[]> comptimeExecute)
        {
            comptimeExecute = null;

            if (name is not Token id)
                return false;

            switch (id.Value)
            {
                case "size":
                    comptimeExecute = CompTime_sizeof;
                    return generics.Count == 1 && parameters.Length == 0;
                default: return false;
            }
        }

        private bool EmitExprPrefixOperator(PrefixOperator p)
        {
            if (p.Prefix == TokenKind.BooleanAND) // &x reference
            {
                if (!_emitter.LoadReference(EvaluateLeftValue(p.Expression, false), p.Position))
                    return false;
            }

            if (!EvaluateExpression(p.Expression))
                return false;

            if (p.Prefix == TokenKind.Negation) // '!' operator
            {
                _generator.ExpectBoolType(_emitter.PeekType(), p.Position);
                _emitter.NegBool();
            }
            else if (p.Prefix == TokenKind.Minus) // '-' operator, for example -(9+2) or -8+2
            {
                if (_emitter.PeekType().MatchIntType())
                    _emitter.NegInt();
                else if (_emitter.PeekType().MatchFloatType())
                    _emitter.NegFloat();
                else
                    Error(p.Position, $"Unable to perform operator `-` on type {_emitter.PeekType()}");
            }
            else if (p.Prefix == TokenKind.Star)
                _emitter.Load(_emitter.LoadFromPointer(_emitter.Pop(), p.Position));
            else if (p.Prefix == TokenKind.OperatorIncrement || p.Prefix == TokenKind.OperatorDecrement)
            {
                var left = EvaluateLeftValue(p.Expression);

                EmitPostfixOperator(left, p.Prefix, p.Position, false);

                _emitter.Load(MugValue.From(_emitter.Builder.BuildLoad(left.LLVMValue), left.Type));
            }

            return true;
        }

        private bool EmitExpr(ExpressionNode e)
        {
            // evaluated left and right
            if (!EvaluateExpression(e.Left) || !EvaluateExpression(e.Right))
                return false;

            // operator implementation
            EmitOperator(e.Operator, e.Position);
            return true;
        }

        private bool EmitExprBool(BooleanExpressionNode b)
        {
            if (b.Operator == OperatorKind.And)
                return EmitAndOperator(b.Left, b.Right);
            else if (b.Operator == OperatorKind.Or)
                return EmitOrOperator(b.Left, b.Right);
            else
            {
                if (!EvaluateExpression(b.Left) || !EvaluateExpression(b.Right))
                    return false;

                EmitOperator(b.Operator, b.Position);
            }

            return true;
        }

        private bool EmitExprArrayElemSelect(ArraySelectElemNode a, bool buildload = true)
        {
            // loading the array
            if (!EvaluateExpression(a.Left))
                return false;

            var indexed = _emitter.PeekType();

            // loading the index expression
            if (!EvaluateExpression(a.IndexExpression))
                return false;

            // arrays are indexed by int32
            _emitter.CoerceConstantSizeTo(MugValueType.Int32);

            _emitter.ExpectIndexerType(a.IndexExpression.Position);

            var index = _emitter.PeekType();

            if (indexed.IsIndexable()) // loading the element
                _emitter.SelectArrayElement(buildload);
            else
                _emitter.CallOperator("[]", a.Position, true, indexed, index);

            return true;
        }

        private LLVMValueRef CreateHeapArray(LLVMTypeRef baseElementType, LLVMValueRef size)
        {
            return _emitter.Builder.BuildArrayMalloc(baseElementType, size);
        }

        private void EmitExprAllocateArray(ArrayAllocationNode aa)
        {
            var arraytype = MugValueType.Array(aa.Type.ToMugValueType(_generator));

            // loading the array

            if (aa.SizeIsImplicit)
                _emitter.Load(MugValue.From(LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, (uint)aa.Body.Length), MugValueType.Int32));
            else
                EvaluateExpression(aa.Size);

            var array = MugValue.From(_emitter.Builder.BuildAlloca(arraytype.LLVMType), arraytype);

            var allocation = CreateHeapArray(arraytype.ArrayBaseElementType.LLVMType, _emitter.Pop().LLVMValue);

            _emitter.Builder.BuildStore(allocation, array.LLVMValue);

            var arraypointer = _emitter.Builder.BuildLoad(array.LLVMValue);

            var i = 0;

            foreach (var elem in aa.Body)
            {
                EvaluateExpression(elem);

                _emitter.CoerceConstantSizeTo(arraytype.ArrayBaseElementType);

                if (!_emitter.PeekType().Equals(arraytype.ArrayBaseElementType))
                    Report(elem.Position, $"Expected {arraytype.ArrayBaseElementType}, got {_emitter.PeekType()}");

                _emitter.StoreElementArray(arraypointer, i);

                i++;
            }

            _emitter.Load(MugValue.From(arraypointer, arraytype));
        }

        private bool EmitExprAllocateStruct(TypeAllocationNode ta)
        {
            if (!ta.Name.IsAllocableTypeNew())
                return Report(ta.Position, $"Unable to allocate type {ta.Name} with `new` operator");

            var structure = ta.Name.ToMugValueType(_generator);

            var tmp = _emitter.Builder.BuildAlloca(structure.LLVMType);

            if (structure.IsEnum())
                return Report(ta.Position, "Unable to allocate an enum");

            var structureInfo = structure.GetStructure();

            var fields = new List<string>();

            for (int i = 0; i < ta.Body.Count; i++)
            {
                var field = ta.Body[i];

                if (fields.Contains(field.Name))
                    Report(field.Position, "Field reassignment in type allocation");

                fields.Add(field.Name);

                if (!EvaluateExpression(field.Body))
                    return false;

                if (!structureInfo.ContainsFieldWithName(field.Name))
                {
                    Report(field.Position, "Undeclared field");
                    continue;
                }

                var fieldType = structureInfo.GetFieldTypeFromName(field.Name, _generator, field.Position);

                _emitter.CoerceConstantSizeTo(fieldType);

                _generator.ExpectSameTypes(
                    fieldType, field.Body.Position, $"Expected {fieldType}, but got {_emitter.PeekType()}", _emitter.PeekType());

                _emitter.StoreField(tmp, structureInfo.GetFieldIndexFromName(field.Name, _generator, field.Position));
            }

            for (int i = 0; i < structureInfo.FieldNames.Length; i++)
            {
                if (fields.Contains(structureInfo.FieldNames[i]))
                    continue;

                _emitter.Load(GetDefaultValueOf(structureInfo.FieldTypes[i], structureInfo.FieldPositions[i]));

                _emitter.StoreField(tmp, i);
            }

            _emitter.Load(MugValue.From(_emitter.Builder.BuildLoad(tmp), structure));
            return true;
        }

        private bool EmitExprMemberAccess(MemberNode m, bool buildload = true)
        {
            if (m.Base is Token token)
            {
                if (_emitter.IsDeclared(token.Value))
                {
                    if (!_emitter.LoadFieldName(token.Value, token.Position))
                        return false;
                }
                else
                {
                    _emitter.LoadEnumMember(token.Value, m.Member.Value, m.Member.Position, this);
                    return true;
                }
            }
            else
            {
                EvaluateExpression(m.Base);
                _emitter.LoadFieldName();
            }

            if (!_emitter.PeekType().IsStructure())
                return Report(m.Base.Position, "Accessed inaccessible type");

            var structure = _emitter.PeekType().GetStructure();
            var type = structure.GetFieldTypeFromName(m.Member.Value, _generator, m.Member.Position);
            var index = structure.GetFieldIndexFromName(m.Member.Value, _generator, m.Member.Position);
            var instance = _emitter.Pop();

            _emitter.LoadField(instance, type, index, buildload);
            return true;
        }

        /// <summary>
        /// the function evaluates an expression, looking at the given node type
        /// </summary>
        private bool EvaluateExpression(INode expression, bool loadreference = true)
        {
            switch (expression)
            {
                case ExpressionNode e: // binary expression: left op right
                    return EmitExpr(e);
                case Token t:
                    if (t.Kind == TokenKind.Identifier) // reference value
                    {
                        if (!_emitter.LoadFromMemory(t.Value, t.Position))
                            return false;

                        if (loadreference && _emitter.PeekType().TypeKind == MugValueTypeKind.Reference)
                        {
                            var value = _emitter.Pop();
                            _emitter.Load(MugValue.From(_emitter.Builder.BuildLoad(value.LLVMValue), value.Type.PointerBaseElementType));
                        }
                    }
                    else // constant value
                        _emitter.Load(ConstToMugConst(t, t.Position));
                    break;
                case PrefixOperator p:
                    return EmitExprPrefixOperator(p);
                case PostfixOperator pp:
                    var left = EvaluateLeftValue(pp.Expression);
                    var load = _emitter.Builder.BuildLoad(left.LLVMValue);

                    EmitPostfixOperator(left, pp.Postfix, pp.Position, false);

                    _emitter.Load(MugValue.From(load, left.Type));
                    break;
                case CallStatement c:
                    // call statement inside expression, true as second parameter because an expression cannot be void
                    return EmitCallStatement(c, true);
                case CastExpressionNode ce:
                    // 'as' operator
                    EvaluateExpression(ce.Expression);

                    return EmitCastInstruction(ce.Type, ce.Position);
                case BooleanExpressionNode b:
                    return EmitExprBool(b);
                /*case InlineConditionalExpression i:
                    EmitExprTernary(i);
                    break;*/
                case ArraySelectElemNode a:
                    return EmitExprArrayElemSelect(a);
                case ArrayAllocationNode aa:
                    EmitExprAllocateArray(aa);
                    break;
                case TypeAllocationNode ta:
                    return EmitExprAllocateStruct(ta);
                case MemberNode m:
                    return EmitExprMemberAccess(m);
                case CatchExpressionNode ce:
                    return EmitCatchStatement(ce, false);
                case AssignmentStatement ae:
                    EmitAssignmentStatement(ae);
                    EvaluateExpression(ae.Name);
                    break;
                default:
                    Error(expression.Position, "Expression not supported yet");
                    break;
            }

            return true;
        }

        /// <summary>
        /// functions that do not return a value must still have a ret instruction in the low level representation,
        /// this function manages the implicit emission of the ret instruction when it is not done by the user.
        /// see the caller to better understand
        /// </summary>
        public void AddImplicitRetVoid()
        {
            _emitter.RetVoid();
        }

        private void AllocParameters()
        {
            if (_function.Base.HasValue)
            {
                var baseparameter = _function.Base.Value;
                var type = baseparameter.Type.ToMugValueType(_generator);

                /*if (type.TypeKind == MugValueTypeKind.Reference)
                    _emitter.DeclareBaseReference(baseparameter.Name, _llvmfunction.GetParam(0), type);
                else
                {*/
                    _emitter.Load(MugValue.From(_llvmfunction.GetParam(0), type, true));
                    _emitter.DeclareConstant(baseparameter.Name, baseparameter.Position);
                /*}*/
            }

            var offset = _function.Base.HasValue ? (uint)1 : 0;

            // alias for ...
            var parameters = _function.ParameterList.Parameters;

            for (int i = 0; i < parameters.Count; i++)
            {
                // alias for ...
                var parameter = parameters[i];

                var parametertype = parameter.Type.ToMugValueType(_generator);

                // allocating the local variable
                _emitter.DeclareVariable(
                    parameter.Name,
                    parametertype,
                    parameter.Position);

                // storing the parameter into the variable
                _emitter.InitializeParameter(parameter.Name, _llvmfunction.GetParam((uint)i + offset));
            }
        }

        private void DefineElseBody(LLVMBasicBlockRef @else, LLVMBasicBlockRef endifelse, ConditionalStatement statement, MugEmitter oldemitter)
        {
            // is elif
            if (statement.Kind == TokenKind.KeyElif)
            {
                // preparing the else if body
                _emitter = new MugEmitter(_generator, oldemitter.Memory, endifelse, true);
                _emitter.Builder.PositionAtEnd(@else);

                // evaluating the else if expression
                EvaluateConditionExpression(statement.Expression, statement.Position);

                // creating a new block, the current will be used to decide if jump to the else if body or the next condition/end
                var elseif = _llvmfunction.AppendBasicBlock("");

                // the next condition
                var next = statement.ElseNode is not null ? _llvmfunction.AppendBasicBlock("") : endifelse;
                
                // branch the current or the next
                _emitter.CompareJump(elseif, next);
                // locating the new block
                _emitter.Builder.PositionAtEnd(elseif);

                // generating the low-level code
                Generate(statement.Body);
                // back to the main block, jump ou of the if scope
                _emitter.JumpOutOfScope(elseif.Terminator, endifelse);

                // check if there is another else node
                if (statement.ElseNode is not null)
                    DefineElseBody(next, endifelse, statement.ElseNode, oldemitter);
            }
            else // is else
                DefineConditionBody(@else, endifelse, statement.Body, oldemitter);
        }

        private void DefineConditionBody(
            LLVMBasicBlockRef then,
            LLVMBasicBlockRef endifelse,
            BlockNode body,
            MugEmitter oldemitter
            /*bool isCycle = false, LLVMBasicBlockRef cycleExitBlock = new()*/)
        {
            // allocating a new emitter with the old symbols
            _emitter = new MugEmitter(_generator, oldemitter.Memory, endifelse, true);
            // locating the emitter builder at the end of the block
            _emitter.Builder.PositionAtEnd(then);

            /*if (isCycle)
                _emitter.CycleExitBlock = cycleExitBlock;*/
            
            // generating the low-level code
            Generate(body);
            
            // back to the main block, jump out of the if scope
            _emitter.JumpOutOfScope(then.Terminator, endifelse);
        }

        private void EvaluateConditionExpression(INode expression, Range position, bool allowNull= false)
        {
            if (allowNull && expression is null)
            {
                _emitter.Load(MugValue.From(LLVMValueRef.CreateConstInt(LLVMTypeRef.Int1, 1), MugValueType.Bool));
                return;
            }

            // evaluating conditional expression
            EvaluateExpression(expression);
            // make sure the expression returned bool
            _generator.ExpectBoolType(_emitter.PeekType(), position);
        }

        private void EmitIfStatement(ConditionalStatement i)
        {
            // evaluate expression
            EvaluateConditionExpression(i.Expression, i.Position);

            var saveOldCondition = _oldcondition;

            // if block
            var then = _llvmfunction.AppendBasicBlock("");

            _oldcondition = then;

            // else block
            var @else = i.ElseNode is not null ? _llvmfunction.AppendBasicBlock("") : _emitter.ExitBlock;

            var endcondition = _llvmfunction.AppendBasicBlock("");

            // compare
            _emitter.CompareJump(then,  i.ElseNode is null ? endcondition : @else);

            // save the old emitter
            var oldemitter = _emitter;

            // define if and else bodies
            // if body
            DefineConditionBody(then, endcondition, i.Body, oldemitter);

            // else body
            if (i.ElseNode is not null)
                DefineElseBody(@else, endcondition, i.ElseNode, oldemitter);

            // restore old emitter
            _emitter = new(_generator, oldemitter.Memory, oldemitter.ExitBlock, oldemitter.IsInsideSubBlock);

            /*if (_emitter.IsInsideSubBlock)
            {
                if (i.ElseNode is not null)
                    saveOldCondition.Terminator.SetOperand(1, @else.AsValue());
                else
                    saveOldCondition.Terminator.SetOperand(1, endcondition.AsValue());
            }*/

            _oldcondition = saveOldCondition;

            // re emit the entry block
            _emitter.Builder.PositionAtEnd(endcondition);
        }

        private void EmitWhileStatement(ConditionalStatement i)
        {
            // if block
            var compare = _llvmfunction.AppendBasicBlock("");

            var cycle = _llvmfunction.AppendBasicBlock("");

            var endcycle = _llvmfunction.AppendBasicBlock("");

            var saveOldCondition = _oldcondition;

            _oldcondition = cycle; // compare here

            // jumping to the compare block
            _emitter.Jump(compare);

            // save the old emitter
            var oldemitter = _emitter;

            _emitter = new(_generator, oldemitter.Memory, cycle, true);
            // locating the builder in the compare block
            _emitter.Builder.PositionAtEnd(compare);

            // evaluate expression
            EvaluateConditionExpression(i.Expression, i.Position);

            // compare
            _emitter.CompareJump(cycle, endcycle);

            var oldCycleExitBlock = CycleExitBlock;
            var oldCycleCompareBlock = CycleCompareBlock;

            CycleExitBlock = endcycle;
            CycleCompareBlock = compare;

            // define if and else bodies
            DefineConditionBody(cycle, compare, i.Body, oldemitter);

            // restore old emitter
            _emitter = new(_generator, oldemitter.Memory, oldemitter.ExitBlock, oldemitter.IsInsideSubBlock);

            /*if (_emitter.IsInsideSubBlock)
            {
                if (saveOldCondition.Terminator.OperandCount >= 2)
                    saveOldCondition.Terminator.SetOperand(1, endcycle.AsValue());
            }*/

            // re emit the entry block
            _emitter.Builder.PositionAtEnd(endcycle);

            CycleExitBlock = oldCycleExitBlock;
            CycleCompareBlock = oldCycleCompareBlock;
            _oldcondition = saveOldCondition;
        }

        private void EmitConditionalStatement(ConditionalStatement i)
        {
            if (i.Kind == TokenKind.KeyIf)
                EmitIfStatement(i);
            else
                EmitWhileStatement(i);
        }

        private MugValue GetDefaultValueOfDefinedType(MugValueType type, Range position)
        {
            if (type.IsEnum())
            {
                var enumerated = type.GetEnum();
                var first = ConstToMugConst(enumerated.Body.First().Value, position, true, enumerated.BaseType.ToMugValueType(_generator));
                return MugValue.EnumMember(first.Type, first.LLVMValue);
            }

            var structure = type.GetStructure();

            var tmp = _emitter.Builder.BuildAlloca(structure.LLVMValue);

            for (int i = 0; i < structure.FieldNames.Length; i++)
            {
                _emitter.Load(GetDefaultValueOf(structure.FieldTypes[i], structure.FieldPositions[i]));

                _emitter.StoreField(tmp, i);
            }

            return MugValue.From(_emitter.Builder.BuildLoad(tmp), type);
        }

        private MugValue GetDefaultValueOf(MugValueType type, Range position)
        {
            return type.TypeKind switch
            {
                MugValueTypeKind.Char or
                MugValueTypeKind.Int8 or
                MugValueTypeKind.Int32 or
                MugValueTypeKind.Int64 or
                MugValueTypeKind.Bool => MugValue.From(LLVMValueRef.CreateConstInt(type.LLVMType, 0), type, true),
                MugValueTypeKind.Float32 or
                MugValueTypeKind.Float64 or
                MugValueTypeKind.Float128 => MugValue.From(LLVMValueRef.CreateConstSIToFP(LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0), type.LLVMType), type, true),
                MugValueTypeKind.String => MugValue.From(CreateConstString(""), type, true),
                MugValueTypeKind.Array => MugValue.From(CreateHeapArray(type.ArrayBaseElementType.LLVMType, LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0)), type, true),
                MugValueTypeKind.Enum or
                MugValueTypeKind.Struct => GetDefaultValueOfDefinedType(type, position),
                MugValueTypeKind.Unknown or
                MugValueTypeKind.Reference or
                MugValueTypeKind.Pointer => _generator.Error<MugValue>(position, "References and unknown pointers must be initialized"),
            };
        }

        private readonly LLVMValueRef Negative1 = LLVMValueRef.CreateConstNeg(LLVMValueRef.CreateConstInt(LLVMTypeRef.Int8, 1));

        private void EmitReturnStatement(ReturnStatement @return)
        {
            var type = _function.Type.ToMugValueType(_generator);
            /*
             * if the expression in the return statement is null, condition verified by calling Returnstatement.Isvoid(),
             * check that the type of function in which it is found returns void.
             */
            if (@return.IsVoid())
            {
                if (type.IsEnumErrorDefined() && type.GetEnumErrorDefined().SuccessType.TypeKind == MugValueTypeKind.Void)
                {
                    _emitter.Load(MugValue.From(Negative1, type));
                    _emitter.Ret();
                }
                else if (type.TypeKind == MugValueTypeKind.Void)
                    _emitter.RetVoid();
                else
                {
                    Report(@return.Position, "Expected non-void expression");
                    return;
                }
            }
            else
            {
                /*
                 * if instead the expression of the return statement has is nothing,
                 * it will be evaluated and then it will be compared the type of the result with the type of return of the function
                 */
                if (!EvaluateExpression(@return.Body))
                    return;

                _emitter.CoerceConstantSizeTo(type);

                var exprType = _emitter.PeekType();
                var errorMessage = $"Expected {type} type, got {exprType} type";

                if (type.IsEnumErrorDefined())
                {
                    var enumerrorType = type.GetEnumErrorDefined();
                    LLVMValueRef? value = null;
                    LLVMValueRef error;

                    if (exprType.Equals(enumerrorType.ErrorType))
                        error = _emitter.Pop().LLVMValue;
                    else
                    {
                        _emitter.CoerceConstantSizeTo(enumerrorType.SuccessType);
                        exprType = _emitter.PeekType();

                        if (exprType.Equals(enumerrorType.SuccessType))
                        {
                            value = enumerrorType.SuccessType.TypeKind == MugValueTypeKind.Void ? new() : _emitter.Pop().LLVMValue;
                            error = Negative1;
                        }
                        else
                        {
                            Report(@return.Position, errorMessage);
                            return;
                        }
                    }

                    if (enumerrorType.SuccessType.TypeKind != MugValueTypeKind.Void) {
                        var tmp = _emitter.Builder.BuildAlloca(type.LLVMType);

                        if (value.HasValue)
                            _emitter.Builder.BuildStore(
                                value.Value,
                                _emitter.Builder.BuildGEP(tmp, new[]
                                {
                                    LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0),
                                    LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 1)
                                }));

                        _emitter.Builder.BuildStore(
                            error,
                            _emitter.Builder.BuildGEP(tmp, new[]
                            {
                                LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0),
                                LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0)
                            }));

                        _emitter.Load(MugValue.From(_emitter.Builder.BuildLoad(tmp), type));
                    }
                    else
                        _emitter.Load(MugValue.From(error, type));

                    _emitter.Ret();
                }
                else
                {
                    _generator.ExpectSameTypes(type, @return.Position, errorMessage, exprType);

                    _emitter.Ret();
                }
            }
        }

        private void EmitVariableStatement(VariableStatement variable)
        {
            _generator.ExpectNonVoidType(variable.Type, variable.Position);

            if (!variable.IsAssigned)
            {
                if (variable.Type.IsAutomatic())
                {
                    Report(variable.Position, "Type specification needed");
                    return;
                }

                _emitter.Load(GetDefaultValueOf(variable.Type.ToMugValueType(_generator), variable.Position));
            }
            else if (!EvaluateExpression(variable.Body)) // the expression in the variable’s body is evaluated
                return;

            /*
             * if in the statement of variable the type is specified explicitly,
             * then a check will be made: the specified type and the type of the result of the expression must be the same.
             */
            if (!variable.Type.IsAutomatic())
                _emitter.DeclareVariable(variable);
            else // if the type is not specified, it will come directly allocate a variable with the same type as the expression result
                _emitter.DeclareVariable(variable.Name, _emitter.PeekType(), variable.Position);

            _emitter.StoreVariable(variable.Name, variable.Position, variable.Body is not null ? variable.Body.Position : new());
        }

        private string PostfixOperatorToString(TokenKind kind)
        {
            return kind switch
            {
                TokenKind.OperatorIncrement => "++",
                TokenKind.OperatorDecrement => "--",
                _ => throw new Exception("unreachable")
            };
        }

        private void EmitPostfixOperator(MugValue variabile, TokenKind kind, Range position, bool isStatement)
        {
            _emitter.Load(variabile);

            if (_emitter.PeekType().MatchIntType())
            {
                if (kind == TokenKind.OperatorIncrement)
                    _emitter.MakePostfixIntOperation(_emitter.Builder.BuildAdd);
                else
                    _emitter.MakePostfixIntOperation(_emitter.Builder.BuildSub);
            }
            else
                _emitter.CallOperator(PostfixOperatorToString(kind), position, !isStatement, _emitter.PeekType());
        }

        /// <summary>
        /// returns a llvm pointer to store the expression in
        /// </summary>
        private MugValue EvaluateLeftValue(INode leftexpression, bool isfirst = true)
        {
            if (leftexpression is Token token && token.Kind == TokenKind.Identifier)
            {
                var allocation = _emitter.GetMemoryAllocation(token.Value, token.Position, true);
                if (!allocation.HasValue)
                    Stop();

                return allocation.Value;
            }
            else if (leftexpression is ArraySelectElemNode indexing)
            {
                if (!EmitExprArrayElemSelect(indexing, false))
                    Stop();

                return _emitter.Pop();
            }
            else if (leftexpression is MemberNode member)
            {
                if (!EmitExprMemberAccess(member, false))
                    Stop();

                return _emitter.Pop();
            }
            else if (leftexpression is PrefixOperator prefix)
            {
                if (prefix.Prefix != TokenKind.Star)
                    Error(leftexpression.Position, "Unable to assign a value to an expression");

                var ptr = EvaluateLeftValue(prefix.Expression, false);

                return MugValue.From(ptr.IsConst ? ptr.LLVMValue : _emitter.LoadFromPointer(ptr, prefix.Position).LLVMValue, ptr.Type.PointerBaseElementType);
            }
            else
            {
                Report(leftexpression.Position, "Bad construction: illegal left expression");
                Stop();
                throw new();
                /*EvaluateExpression(leftexpression);

                return _emitter.Pop();*/
            }
        }

        private void EmitAssignmentStatement(AssignmentStatement assignment)
        {
            var ptr = EvaluateLeftValue(assignment.Name);

            if (ptr.IsConst)
            {
                Report(assignment.Position, "Unable to change a constant value");
                return;
            }

            if (ptr.Type.TypeKind == MugValueTypeKind.Reference)
                ptr = MugValue.From(_emitter.Builder.BuildLoad(ptr.LLVMValue), ptr.Type.PointerBaseElementType);

            EvaluateExpression(assignment.Body);

            _emitter.CoerceConstantSizeTo(ptr.Type);

            if (assignment.Operator == TokenKind.Equal)
            {
                _generator.ExpectSameTypes(_emitter.PeekType(), assignment.Position, $"Expected {ptr.Type}, got {_emitter.PeekType()}", ptr.Type);
                _emitter.StoreInsidePointer(ptr);
            }
            else
            {
                _emitter.Load(MugValue.From(_emitter.Builder.BuildLoad(ptr.LLVMValue), ptr.Type));
                _emitter.Swap();

                switch (assignment.Operator)
                {
                    case TokenKind.AddAssignment: EmitSum(assignment.Position); break;
                    case TokenKind.SubAssignment: EmitSub(assignment.Position); break;
                    case TokenKind.MulAssignment: EmitMul(assignment.Position); break;
                    case TokenKind.DivAssignment: EmitDiv(assignment.Position); break;
                    default: throw new();
                }

                _emitter.OperateInsidePointer(ptr);
            }
        }

        private void EmitConstantStatement(ConstantStatement constant)
        {
            // evaluating the body expression of the constant
            EvaluateExpression(constant.Body);

            // match the constant explicit type and expression type are the same
            if (!constant.Type.IsAutomatic())
            {
                var constType = constant.Type.ToMugValueType(_generator);

                _emitter.CoerceConstantSizeTo(constType);

                _generator.ExpectSameTypes(constType,
                    constant.Body.Position, $"Expected {constant.Type} type, got {_emitter.PeekType()} type", _emitter.PeekType());
            }

            // declaring the constant with a name
            _emitter.DeclareConstant(constant.Name, constant.Position);
        }

        private void EmitLoopManagementStatement(LoopManagementStatement management)
        {
            // is not inside a cycle
            if (CycleExitBlock.Handle == IntPtr.Zero)
                Report(management.Position, "`break` only allowed inside cycles' and catches' block");
            else if (management.Management.Kind == TokenKind.KeyBreak)
                _emitter.Jump(CycleExitBlock);
            else if (CycleCompareBlock.Handle == IntPtr.Zero)
                Report(management.Position, "`continue` only allowed inside cycles' block");
            else
                _emitter.Jump(CycleCompareBlock);
        }

        private void EmitCompTimeWhen(CompTimeWhenStatement when)
        {
            if (_generator.EvaluateCompTimeExprAndGetResult(when.Expression))
                Generate((BlockNode)when.Body);
        }

        private MugValue LoadField(ref LLVMValueRef tmp, EnumErrorInfo enumerror, uint index)
        {
            var gep = _emitter.Builder.BuildGEP(tmp, new[]
            {
                    LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0),
                    LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, index)
            });

            return MugValue.From(_emitter.Builder.BuildLoad(gep), enumerror.ErrorType);
        }

        private MugValue _buffer = new();

        private bool EmitCatchStatement(CatchExpressionNode catchstatement, bool isImperativeStatement)
        {
            if (catchstatement.Expression is not CallStatement call)
                return Report(catchstatement.Expression.Position, "Unable to catch this expression");

            if (!EmitCallStatement(call, true, true))
                return false;

            var value = _emitter.Pop();
            var enumerror = value.Type.GetEnumErrorDefined();
            var tmp = _emitter.Builder.BuildAlloca(enumerror.LLVMValue);
            var resultIsVoid = enumerror.SuccessType.TypeKind == MugValueTypeKind.Void;
            var oldBuffer = _buffer;
            var oldCycleExitBlock = CycleExitBlock;

            _emitter.Builder.BuildStore(value.LLVMValue, tmp);
            _buffer = resultIsVoid ? MugValue.From(new(), MugValueType.Void) : MugValue.From(_emitter.Builder.BuildAlloca(enumerror.SuccessType.LLVMType, ""), enumerror.SuccessType);

            if (resultIsVoid)
                _emitter.Load(MugValue.From(value.LLVMValue, enumerror.ErrorType));
            else
            {
                _emitter.Builder.BuildStore(GetDefaultValueOf(enumerror.SuccessType, call.Position).LLVMValue, _buffer.LLVMValue);
                _emitter.Load(LoadField(ref tmp, enumerror, 0));
            }

            var catchbodyErr = _llvmfunction.AppendBasicBlock("");
            var catchbodyOk = resultIsVoid || isImperativeStatement ? new() : _llvmfunction.AppendBasicBlock("");
            var catchend = _llvmfunction.AppendBasicBlock("");

            _emitter.Builder.BuildCondBr(
                _emitter.Builder.BuildICmp(LLVMIntPredicate.LLVMIntNE, _emitter.Pop().LLVMValue, Negative1),
                catchbodyErr,
                resultIsVoid || isImperativeStatement ? catchend : catchbodyOk);

            var oldemitter = _emitter;
            var oldMemory = _emitter.Memory;

            if (!resultIsVoid && !isImperativeStatement)
            {
                _emitter = new MugEmitter(_generator, _emitter.Memory, catchend, true);
                _emitter.Builder.PositionAtEnd(catchbodyOk);

                _emitter.Builder.BuildStore(LoadField(ref tmp, enumerror, 1).LLVMValue, _buffer.LLVMValue);
                _emitter.Exit();

                _emitter = new MugEmitter(_generator, catchend, oldemitter.IsInsideSubBlock);
            }

            oldemitter = _emitter;
            _emitter = new MugEmitter(_generator, _emitter.Memory, catchend, true);
            _emitter.Builder.PositionAtEnd(catchbodyErr);

            CycleExitBlock = catchend;

            if (catchstatement.OutError is not null)
            {
                _emitter.Load(resultIsVoid ? value : LoadField(ref tmp, enumerror, 0));
                _emitter.DeclareConstant(catchstatement.OutError.Value.Value, catchstatement.OutError.Value.Position);
            }

            Generate(catchstatement.Body);

            _emitter.Exit();

            _emitter = new MugEmitter(_generator, oldMemory, catchend, oldemitter.IsInsideSubBlock);
            _emitter.Builder.PositionAtEnd(catchend);

            if (!isImperativeStatement)
            {
                if (resultIsVoid)
                    return Report(catchstatement.Expression.Position, "Unable to evaluate void in expression");

                _emitter.Load(MugValue.From(_emitter.Builder.BuildLoad(_buffer.LLVMValue), _buffer.Type));
            }

            _buffer = oldBuffer;
            CycleExitBlock = oldCycleExitBlock;

            return true;
        }

        private void EmitForStatement(ForLoopStatement forstatement)
        {
            // if block
            var compare = _llvmfunction.AppendBasicBlock("");

            var cycle = _llvmfunction.AppendBasicBlock("");

            var operate = _llvmfunction.AppendBasicBlock("");

            var endcycle = _llvmfunction.AppendBasicBlock("");

            var saveOldCondition = _oldcondition;

            _oldcondition = cycle; // compare here

            var oldMemory = _emitter.Memory;

            if (forstatement.LeftExpression is not null)
            {
                _emitter.ReallocMemory();
                EmitVariableStatement(forstatement.LeftExpression);
            }

            // jumping to the compare block
            _emitter.Jump(compare);

            // save the old emitter
            var oldemitter = _emitter;

            _emitter = new(_generator, oldemitter.Memory, cycle, true);
            // locating the builder in the compare block
            _emitter.Builder.PositionAtEnd(operate);

            if (forstatement.RightExpression is not null)
                RecognizeStatement(forstatement.RightExpression);

            _emitter.Jump(compare);

            // locating the builder in the compare block
            _emitter.Builder.PositionAtEnd(compare);

            // evaluate expression
            EvaluateConditionExpression(forstatement.ConditionExpression, forstatement.Position, true);

            // compare
            _emitter.CompareJump(cycle, endcycle);

            var oldCycleExitBlock = CycleExitBlock;
            var oldCycleCompareBlock = CycleCompareBlock;

            CycleExitBlock = endcycle;
            CycleCompareBlock = compare;

            // define if and else bodies
            DefineConditionBody(cycle, operate, forstatement.Body, oldemitter);

            // restore old emitter
            _emitter = new(_generator, oldMemory, oldemitter.ExitBlock, oldemitter.IsInsideSubBlock);

            // re emit the entry block
            _emitter.Builder.PositionAtEnd(endcycle);

            CycleExitBlock = oldCycleExitBlock;
            CycleCompareBlock = oldCycleCompareBlock;
            _oldcondition = saveOldCondition;
        }

        /// <summary>
        /// 
        /// </summary>
        private void RecognizeStatement(INode statement)
        {
            switch (statement)
            {
                case VariableStatement variable:
                    EmitVariableStatement(variable);
                    break;
                case ReturnStatement @return:
                    EmitReturnStatement(@return);
                    break;
                case ConditionalStatement condition:
                    EmitConditionalStatement(condition);
                    break;
                case CallStatement call:
                    EmitCallStatement(call, false);
                    break;
                case AssignmentStatement assignment:
                    EmitAssignmentStatement(assignment);
                    break;
                case ConstantStatement constant:
                    EmitConstantStatement(constant);
                    break;
                case LoopManagementStatement loopmanagement:
                    EmitLoopManagementStatement(loopmanagement);
                    break;
                case CompTimeWhenStatement comptimewhen:
                    EmitCompTimeWhen(comptimewhen);
                    break;
                case CatchExpressionNode catchstatement:
                    EmitCatchStatement(catchstatement, true);
                    break;
                case PostfixOperator postfix:
                    var left = EvaluateLeftValue(postfix.Expression);

                    EmitPostfixOperator(left, postfix.Postfix, postfix.Position, true);
                    break;
                case PrefixOperator prefix:
                    if (prefix.Prefix == TokenKind.OperatorIncrement || prefix.Prefix == TokenKind.OperatorDecrement)
                    {
                        left = EvaluateLeftValue(prefix.Expression);

                        EmitPostfixOperator(left, prefix.Prefix, prefix.Position, true);
                    }
                    else
                        goto default;
                    break;
                case ForLoopStatement forstatement:
                    EmitForStatement(forstatement);
                    break;
                default:
                    if (!EvaluateExpression(statement))
                        return;

                    if (!_buffer.Type.Equals(_emitter.PeekType()))
                        Report(statement.Position, $"Expected {_buffer.Type}, got {_emitter.PeekType()}");

                    _emitter.Builder.BuildStore(_emitter.Pop().LLVMValue, _buffer.LLVMValue);
                    break;
            }
        }

        /// <summary>
        /// the function cycles all the nodes in the statement array passed
        /// </summary>
        public void Generate(BlockNode statements)
        {
            for (int i = 0; i < statements.Statements.Length; i++)
                RecognizeStatement(statements.Statements[i]);

            if (_emitter.IsInsideSubBlock)
                _emitter.Exit();
        }

        /// <summary>
        /// the function passes all the nodes in the statement array of
        /// a Functionnode to the <see cref="Generate(BlockNode)"/> function and
        /// calls a function to convert them into the corresponding low-level code
        /// </summary>
        public void Generate()
        {
            // allocating parameters as local variable
            AllocParameters();
            Generate(_function.Body);

            if (_generator.IsEntryPoint(_function.Name, _function.ParameterList.Length))
                _generator.Parser.Lexer.CheckDiagnostic();
        }
    }
}
