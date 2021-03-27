﻿using LLVMSharp;
using LLVMSharp.Interop;
using Mug.Compilation;
using Mug.Models.Generator.Emitter;
using Mug.Models.Lexer;
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

        internal void Error(Range position, params string[] error)
        {
            _generator.Parser.Lexer.Throw(position, error);
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
                default:
                    _generator.NotSupportedType<LLVMValueRef>(constant.Kind.ToString(), position);
                    break;
            }

            return MugValue.From(llvmvalue, type);
        }

        private void EmitSum(Range position)
        {
            _emitter.ForceCoupleConstantIntSize();

            var types = _emitter.GetCoupleTypes();

            if (types.Item1.MatchSameIntType(types.Item2))
                _emitter.AddInt();
            else
                _emitter.CallOperator("+", position, true, types.Item1, types.Item2);
        }

        private void EmitSub(Range position)
        {
            _emitter.ForceCoupleConstantIntSize();

            var types = _emitter.GetCoupleTypes();

            if (types.Item1.MatchSameIntType(types.Item2))
                _emitter.SubInt();
            else
                _emitter.CallOperator("-", position, true, types.Item1, types.Item2);
        }

        private void EmitMul(Range position)
        {
            _emitter.ForceCoupleConstantIntSize();

            var types = _emitter.GetCoupleTypes();

            if (types.Item1.MatchSameIntType(types.Item2))
                _emitter.MulInt();
            else
                _emitter.CallOperator("*", position, true, types.Item1, types.Item2);
        }

        private void EmitDiv(Range position)
        {
            _emitter.ForceCoupleConstantIntSize();

            var types = _emitter.GetCoupleTypes();

            if (types.Item1.MatchSameIntType(types.Item2))
                _emitter.DivInt();
            else
                _emitter.CallOperator("/", position, true, types.Item1, types.Item2);
        }

        private void EmitBooleanOperator(string literal, LLVMIntPredicate llvmpredicate, OperatorKind kind, Range position)
        {
            _emitter.ForceCoupleConstantIntSize();

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
            else if (ft.MatchSameIntType(st))
                _emitter.CompareInt(llvmpredicate);
            else if (ft.TypeKind == MugValueTypeKind.Char &&
                st.TypeKind == MugValueTypeKind.Char &&
                (kind == OperatorKind.CompareEQ || kind == OperatorKind.CompareNEQ))
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
                case OperatorKind.CompareMajor:
                    EmitBooleanOperator(">", LLVMIntPredicate.LLVMIntSGT, kind, position);
                    break;
                case OperatorKind.CompareMajorEQ:
                    EmitBooleanOperator(">=", LLVMIntPredicate.LLVMIntSGE, kind, position);
                    break;
                case OperatorKind.CompareMinor:
                    EmitBooleanOperator("<", LLVMIntPredicate.LLVMIntSLT, kind, position);
                    break;
                case OperatorKind.CompareMinorEQ:
                    EmitBooleanOperator("<=", LLVMIntPredicate.LLVMIntSLE, kind, position);
                    break;
                /*case OperatorKind.And
                    break;*/
                default:
                    Error(position, "`", kind.ToString(), "` operator not supported yet");
                    break;
            }
        }

        private bool IsABitcast(MugValueType expressionType, MugValueType castType)
        {
            return expressionType.TypeKind == MugValueTypeKind.String && castType.Equals(MugValueType.Array(MugValueType.Char)) ||
                 castType.TypeKind == MugValueTypeKind.String && expressionType.Equals(MugValueType.Array(MugValueType.Char));
        }

        /// <summary>
        /// the function manages the 'as' operator
        /// </summary>
        private void EmitCastInstruction(MugType type, Range position)
        {
            // the expression type to cast
            var expressionType = _emitter.PeekType();
            var castType = type.ToMugValueType(_generator);

            if (castType.IsEnum())
            {
                var enumerated = castType.GetEnum();
                var enumBaseType = enumerated.BaseType.ToMugValueType(_generator);

                _emitter.ForceConstantIntSizeTo(enumBaseType);

                if (_emitter.PeekType().TypeKind != enumBaseType.TypeKind)
                    Error(position, "The base type of enum `", enumerated.Name, "` is incompatible with type `", expressionType.ToString(), "`");

                _emitter.CastToEnumMemberFromBaseType(castType);
            }
            else if (expressionType.TypeKind == MugValueTypeKind.Enum)
            {
                var enumBaseType = expressionType.GetEnum().BaseType.ToMugValueType(_generator);

                _emitter.ForceConstantIntSizeTo(enumBaseType);

                if (_emitter.PeekType().GetEnum().BaseType.ToMugValueType(_generator).TypeKind != castType.TypeKind)
                    Error(type.Position, "Enum base type is incompatible with type `", castType.ToString(), "`");

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
                    Error(position, "Expected pointer when in cast expression something is unknown");

                _emitter.Load(MugValue.From(_emitter.Builder.BuildBitCast(value.LLVMValue, castType.LLVMType), castType));
            }
            else if (expressionType.MatchAnyTypeOfIntType() &&
                castType.MatchAnyTypeOfIntType()) // LLVM has different instructions for each type convertion
                _emitter.CastInt(castType);
            else
                _emitter.CallAsOperator(position, expressionType, type.ToMugValueType(_generator));
        }

        /// <summary>
        /// wip function
        /// the function evaluates an instance node, for example: base.method()
        /// </summary>
        private void EvaluateMemberAccess(INode member, bool load)
        {
            switch (member)
            {
                case MemberNode m:
                    EvaluateMemberAccess(m.Base, load);
                    var structure = _emitter.PeekType().GetStructure();
                    _emitter.LoadField(
                        _emitter.Pop(),
                        structure.GetFieldTypeFromName(m.Member.Value),
                        structure.GetFieldIndexFromName(m.Member.Value), load);
                    break;
                case Token t:
                    if (load)
                        _emitter.LoadFromMemory(t.Value, t.Position);
                    else
                        _emitter.LoadMemoryAllocation(t.Value, t.Position);
                    break;
                case ArraySelectElemNode a:
                    EmitExprArrayElemSelect(a);
                    break;
                case PrefixOperator p:
                    EvaluateMemberAccess(p.Expression, p.Prefix == TokenKind.Star);

                    if (p.Prefix == TokenKind.Star)
                        _emitter.Load(_emitter.LoadFromPointer(_emitter.Pop(), p.Position));
                    else if (p.Prefix == TokenKind.BooleanAND)
                    {
                        EvaluateMemberAccess(p.Expression, load);
                        _emitter.LoadReference(_emitter.Pop(), p.Position);
                    }
                    else
                        Error(p.Position, "In member access, the base must be a non-expression");

                    break;
                default:
                    Error(member.Position, "Not supported yet");
                    break;
            }
        }

        private void EvaluateFunctionCallName(INode leftexpression, MugValueType[] parameters, MugValueType[] genericsInput, out bool hasbase)
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
                EvaluateExpression(member.Base);

                name = member.Member.Value;
                position = member.Member.Position;
                hasbase = true;
            }
            else // (expr)()
            {
                EvaluateExpression(leftexpression);
                return;
            }

            _emitter.Load(_generator.EvaluateFunction($"{new string ('.', Convert.ToInt32(hasbase))}{name}", hasbase ? new MugValueType?(_emitter.PeekType()) : null, parameters, genericsInput, position));
        }

        /// <summary>
        /// the function converts a Callstatement node to the corresponding low-level code
        /// </summary>
        private void EmitCallStatement(CallStatement c, bool expectedNonVoid, bool isInCatch = false)
        {
            // an array is prepared for the parameter types of function to call
            var parameters = new MugValueType[c.Parameters.Length];

            /* the array is cycled with the expressions of the respective parameters and each expression
             * is evaluated and assigned its type to the array of parameter types
             */
            for (int i = 0; i < c.Parameters.Length; i++)
            {
                EvaluateExpression(c.Parameters.Nodes[i]);
                parameters[i] = _emitter.PeekType();
            }

            /*
             * the symbol of the function is taken by passing the name of the complete function which consists
             * of the function id and in brackets the list of parameter types separated by ', '
             */

            if (IsBuiltInFunction(c.Name, c.Generics, parameters, out var comptimeExecute))
            {
                comptimeExecute(c.Generics, parameters);
                return;
            }
            
            EvaluateFunctionCallName(c.Name, parameters, _generator.MugTypesToMugValueTypes(c.Generics), out bool hasbase);

            var function = _emitter.Pop();

            if (!function.IsFunction())
                _generator.Error(c.Position, "Unable to call this member");

            // function type: <ret_type> <param_types>
            var functionType = function.Type.GetFunction().Item2;

            if (expectedNonVoid)
                _generator.ExpectNonVoidType(
                    // (<ret_type> <param_types>).GetElementType() -> <ret_type>
                    functionType.LLVMType,
                    c.Position);

            _emitter.Call(function.LLVMValue, c.Parameters.Length, functionType, hasbase);

            if (!isInCatch && functionType.TypeKind == MugValueTypeKind.EnumErrorDefined)
                Error(c.Position, "Uncatched enum error");

            if (isInCatch && functionType.TypeKind != MugValueTypeKind.EnumErrorDefined)
                Error(c.Position, "Catched a non enum error");
        }

        private void CompTime_sizeof(List<MugType> generics, MugValueType[] parameters)
        {
            var size = generics[0].ToMugValueType(_generator).Size(
                (int)LLVMTargetDataRef.FromStringRepresentation(_generator.Module.DataLayout)
                    .StoreSizeOfType(LLVMTypeRef.CreatePointer(LLVMTypeRef.Int32, 0))
                );

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

        private void EmitExprPrefixOperator(PrefixOperator p)
        {
            if (p.Prefix == TokenKind.BooleanAND) // &x reference
            {
                _emitter.LoadReference(EvaluateLeftValue(p.Expression, false), p.Position);
                return;
            }

            EvaluateExpression(p.Expression);

            if (p.Prefix == TokenKind.Negation) // '!' operator
            {
                _generator.ExpectBoolType(_emitter.PeekType(), p.Position);
                _emitter.NegBool();
            }
            else if (p.Prefix == TokenKind.Minus) // '-' operator, for example -(9+2) or -8+2
            {
                _generator.ExpectIntType(_emitter.PeekType(), p.Position);
                _emitter.NegInt();
            }
            else if (p.Prefix == TokenKind.Star)
                _emitter.Load(_emitter.LoadFromPointer(_emitter.Pop(), p.Position));
        }

        private void EmitExpr(ExpressionNode e)
        {
            // evaluated left
            EvaluateExpression(e.Left);
            // evaluated right
            EvaluateExpression(e.Right);
            // operator implementation
            EmitOperator(e.Operator, e.Position);
        }

        private void EmitExprBool(BooleanExpressionNode b)
        {
            EvaluateExpression(b.Left);
            EvaluateExpression(b.Right);
            EmitOperator(b.Operator, b.Position);
        }

        private void EmitExprArrayElemSelect(ArraySelectElemNode a, bool buildload = true)
        {
            // loading the array
            EvaluateExpression(a.Left);

            var indexed = _emitter.PeekType();

            // loading the index expression
            EvaluateExpression(a.IndexExpression);

            // arrays are indexed by int32
            _emitter.ForceConstantIntSizeTo(MugValueType.Int32);

            _emitter.ExpectIndexerType(a.IndexExpression.Position);

            var index = _emitter.PeekType();

            if (indexed.IsIndexable()) // loading the element
                _emitter.SelectArrayElement(buildload);
            else
                _emitter.CallOperator("[]", a.Position, true, indexed, index);
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

                _emitter.ForceConstantIntSizeTo(arraytype.ArrayBaseElementType);

                if (!_emitter.PeekType().Equals(arraytype.ArrayBaseElementType))
                    Error(elem.Position, "Expected ", arraytype.ArrayBaseElementType.ToString(), ", got ", _emitter.PeekType().ToString());

                _emitter.StoreElementArray(arraypointer, i);

                i++;
            }

            _emitter.Load(MugValue.From(arraypointer, arraytype));
        }

        private void EmitExprAllocateStruct(TypeAllocationNode ta)
        {
            if (!ta.Name.IsAllocableTypeNew())
                Error(ta.Position, "Unable to allocate type ", ta.Name.ToString(), " with `new` operator");

            var structure = ta.Name.ToMugValueType(_generator);

            var tmp = _emitter.Builder.BuildAlloca(structure.LLVMType);

            if (structure.IsEnum())
                Error(ta.Position, "Unable to allocate an enum");

            var structureInfo = structure.GetStructure();

            var fields = new List<string>();

            for (int i = 0; i < ta.Body.Count; i++)
            {
                var field = ta.Body[i];

                if (fields.Contains(field.Name))
                    Error(field.Position, "Field reassignment in type allocation");

                fields.Add(field.Name);

                EvaluateExpression(field.Body);

                if (!structureInfo.ContainsFieldWithName(field.Name))
                    Error(field.Position, "Undeclared field");

                var fieldType = structureInfo.GetFieldTypeFromName(field.Name);

                _emitter.ForceConstantIntSizeTo(fieldType);

                _generator.ExpectSameTypes(
                    fieldType, field.Body.Position, $"expected {fieldType}, but got {_emitter.PeekType()}", _emitter.PeekType());

                _emitter.StoreField(tmp, structureInfo.GetFieldIndexFromName(field.Name));
            }

            for (int i = 0; i < structureInfo.FieldNames.Length; i++)
            {
                if (fields.Contains(structureInfo.FieldNames[i]))
                    continue;

                _emitter.Load(GetDefaultValueOf(structureInfo.FieldTypes[i], structureInfo.FieldPositions[i]));

                _emitter.StoreField(tmp, i);
            }

            _emitter.Load(MugValue.From(_emitter.Builder.BuildLoad(tmp), structure));
        }

        private void EmitExprMemberAccess(MemberNode m, bool buildload = true)
        {
            if (m.Base is Token token)
            {
                if (_emitter.IsDeclared(token.Value))
                {
                    _emitter.LoadFieldName(token.Value, token.Position);
                }
                else
                {
                    _emitter.LoadEnumMember(token.Value, m.Member.Value, m.Member.Position, this);
                    return;
                }
            }
            else
            {
                EvaluateExpression(m.Base);
                _emitter.LoadFieldName();
            }

            if (_emitter.PeekType().IsPointer())
                Error(m.Position, "Unable to access to a pointer");

            var structure = _emitter.PeekType().GetStructure();
            var type = structure.GetFieldTypeFromName(m.Member.Value);
            var index = structure.GetFieldIndexFromName(m.Member.Value);
            var instance = _emitter.Pop();

            _emitter.LoadField(instance, type, index, buildload);
        }

        /// <summary>
        /// the function evaluates an expression, looking at the given node type
        /// </summary>
        private void EvaluateExpression(INode expression)
        {
            switch (expression)
            {
                case ExpressionNode e: // binary expression: left op right
                    EmitExpr(e);
                    break;
                case Token t:
                    if (t.Kind == TokenKind.Identifier) // reference value
                        _emitter.LoadFromMemory(t.Value, t.Position);
                    else // constant value
                        _emitter.Load(ConstToMugConst(t, t.Position));
                    break;
                case PrefixOperator p:
                    EmitExprPrefixOperator(p);
                    break;
                case CallStatement c:
                    // call statement inside expression, true as second parameter because an expression cannot be void
                    EmitCallStatement(c, true);
                    break;
                case CastExpressionNode ce:
                    // 'as' operator
                    EvaluateExpression(ce.Expression);

                    EmitCastInstruction(ce.Type, ce.Position);
                    break;
                case BooleanExpressionNode b:
                    EmitExprBool(b);
                    break;
                /*case InlineConditionalExpression i:
                    EmitExprTernary(i);
                    break;*/
                case ArraySelectElemNode a:
                    EmitExprArrayElemSelect(a);
                    break;
                case ArrayAllocationNode aa:
                    EmitExprAllocateArray(aa);
                    break;
                case TypeAllocationNode ta:
                    EmitExprAllocateStruct(ta);
                    break;
                case MemberNode m:
                    EmitExprMemberAccess(m);
                    break;
                case CatchExpressionNode ce:
                    EmitCatchStatement(ce, false);
                    break;
                default:
                    Error(expression.Position, "Expression not supported yet");
                    break;
            }
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
                _emitter.Load(MugValue.From(_llvmfunction.GetParam(0), type, true));
                _emitter.DeclareConstant(baseparameter.Name, baseparameter.Position);
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

        private void EvaluateConditionExpression(INode expression, Range position)
        {
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
                MugValueTypeKind.Char or MugValueTypeKind.Int8 or MugValueTypeKind.Int32 or
                MugValueTypeKind.Int64 or MugValueTypeKind.Bool => MugValue.From(LLVMValueRef.CreateConstInt(type.LLVMType, 0), type, true),
                MugValueTypeKind.String => MugValue.From(CreateConstString(""), type, true),
                MugValueTypeKind.Array => MugValue.From(
                    CreateHeapArray(type.ArrayBaseElementType.LLVMType, LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0)), type, true),
                MugValueTypeKind.Enum or MugValueTypeKind.Struct => GetDefaultValueOfDefinedType(type, position),
                MugValueTypeKind.Unknown or MugValueTypeKind.Pointer => _generator.Error<MugValue>(position, "Pointers must be initialized"),
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
                if (type.IsEnumErrorDefined())
                {
                    _emitter.Load(MugValue.From(Negative1, type));
                    _emitter.Ret();
                }
                else if (type.TypeKind == MugValueTypeKind.Void)
                    _emitter.RetVoid();
                else
                    Error(@return.Position, "Expected non-void expression");
            }
            else
            {
                /*
                 * if instead the expression of the return statement has is nothing,
                 * it will be evaluated and then it will be compared the type of the result with the type of return of the function
                 */
                EvaluateExpression(@return.Body);

                _emitter.ForceConstantIntSizeTo(type);

                var exprType = _emitter.PeekType();
                var errorMessage = $"Expected {type} type, got {exprType} type";

                if (type.IsEnumErrorDefined())
                {
                    var enumerrorType = type.GetEnumErrorDefined();
                    LLVMValueRef value;
                    LLVMValueRef error;

                    if (exprType.Equals(enumerrorType.ErrorType))
                    {
                        value = enumerrorType.SuccessType.TypeKind == MugValueTypeKind.Void ? new() : GetDefaultValueOf(enumerrorType.SuccessType, @return.Body.Position).LLVMValue;
                        error = _emitter.Pop().LLVMValue;
                    }
                    else
                    {
                        _emitter.ForceConstantIntSizeTo(enumerrorType.SuccessType);
                        exprType = _emitter.PeekType();

                        if (exprType.Equals(enumerrorType.SuccessType))
                        {
                            value = enumerrorType.SuccessType.TypeKind == MugValueTypeKind.Void ? new() : _emitter.Pop().LLVMValue;
                            error = Negative1;
                        }
                        else
                        {
                            Error(@return.Body.Position, errorMessage);
                            throw new();
                        }
                    }

                    if (enumerrorType.SuccessType.TypeKind != MugValueTypeKind.Void) {
                        var tmp = _emitter.Builder.BuildAlloca(type.LLVMType);

                        _emitter.Builder.BuildStore(
                            value,
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
                    _generator.ExpectSameTypes(type, @return.Body.Position, errorMessage, exprType);

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
                    Error(variable.Position, "Type specification needed");

                _emitter.Load(GetDefaultValueOf(variable.Type.ToMugValueType(_generator), variable.Position));
            }
            else // the expression in the variable’s body is evaluated
                EvaluateExpression(variable.Body);

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

        private void EmitPostfixOperator(MugValue variabile, TokenKind kind, Range position)
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
                _emitter.CallOperator(PostfixOperatorToString(kind), position, false, _emitter.PeekType());
        }

        /// <summary>
        /// returns a llvm pointer to store the expression in
        /// </summary>
        private MugValue EvaluateLeftValue(INode leftexpression, bool isfirst = true)
        {
            if (leftexpression is Token token)
            {
                if (token.Kind != TokenKind.Identifier)
                    Error(token.Position, "Invalid value in left expression");

                return _emitter.GetMemoryAllocation(token.Value, token.Position);
            }
            else if (leftexpression is ArraySelectElemNode indexing)
            {
                EmitExprArrayElemSelect(indexing, false);

                return _emitter.Pop();
            }
            else if (leftexpression is MemberNode member)
            {
                EmitExprMemberAccess(member, false);

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
                EvaluateExpression(leftexpression);

                return _emitter.Pop();
            }
        }

        private void EmitAssignmentStatement(AssignmentStatement assignment)
        {
            var ptr = EvaluateLeftValue(assignment.Name);

            if (ptr.IsConst)
                Error(assignment.Position, "Unable to change a constant value");

            if (assignment.Operator == TokenKind.OperatorIncrement || assignment.Operator == TokenKind.OperatorDecrement)    
                EmitPostfixOperator(ptr, assignment.Operator, assignment.Position);
            else
            {
                EvaluateExpression(assignment.Body);

                if (assignment.Operator == TokenKind.Equal)
                    _emitter.StoreInsidePointer(ptr);
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
        }

        private void EmitConstantStatement(ConstantStatement constant)
        {
            // evaluating the body expression of the constant
            EvaluateExpression(constant.Body);

            // match the constant explicit type and expression type are the same
            if (!constant.Type.IsAutomatic())
            {
                var constType = constant.Type.ToMugValueType(_generator);

                _emitter.ForceConstantIntSizeTo(constType);

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
                Error(management.Position, "Loop management statements only allowed inside cycle's bodies");

            if (management.Management.Kind == TokenKind.KeyBreak)
                _emitter.Jump(CycleExitBlock);
            else
                _emitter.Jump(CycleCompareBlock);
        }

        private void EmitCompTimeWhen(CompTimeWhenStatement when)
        {
            if (_generator.EvaluateCompTimeExprAndGetResult(when.Expression))
                Generate((BlockNode)when.Body);
        }

        private void EmitCatchStatement(CatchExpressionNode catchstatement, bool isImperativeStatement)
        {
            if (catchstatement.Expression is not CallStatement call)
            {
                Error(catchstatement.Expression.Position, "Unable to catch this expression");
                throw new();
            }

            EmitCallStatement(call, true, true);
            var retType = _emitter.Pop();
            var enumerror = retType.Type.GetEnumErrorDefined();
            MugValue result = new();

            if (catchstatement.OutError is not null)
            {
                /*if (isImperativeStatement && */
                Console.WriteLine(enumerror.LLVMValue);

                _emitter.DeclareConstant(catchstatement.OutError?.Value, catchstatement.OutError.Value.Position);
            }

            if (!isImperativeStatement)
                _emitter.Load(result);
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
                /*case ForLoopStatement loop:
                    break;*/
                default:
                    Error(statement.Position, "Statement not supported yet");
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
        }
    }
}
