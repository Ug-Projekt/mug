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
        private LLVMBasicBlockRef _cycleExitBlock { get; set; }

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
        internal MugValue ConstToMugConst(Token constant, Range position, bool isenum = false, MugValueType enumint = new())
        {
            LLVMValueRef llvmvalue = new();
            MugValueType type = new();

            switch (constant.Kind)
            {
                case TokenKind.ConstantDigit:
                    if (!isenum)
                    {
                        llvmvalue = LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, Convert.ToUInt64(constant.Value));
                        type = MugValueType.Int32;
                    }
                    else
                    {
                        llvmvalue = LLVMValueRef.CreateConstInt(enumint.LLVMType, Convert.ToUInt64(constant.Value));
                        type = enumint;
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

        private void EmitSum(MugValueType ft, MugValueType st, Range position)
        {
            if (_generator.MatchSameIntType(ft, st))
                _emitter.AddInt();
            else
                _emitter.CallOperator("+", position, true, ft, st);
        }

        private void EmitSub(MugValueType ft, MugValueType st, Range position)
        {
            if (_generator.MatchSameIntType(ft, st))
                _emitter.SubInt();
            else
                _emitter.CallOperator("-", position, true, ft, st);
        }

        private void EmitMul(MugValueType ft, MugValueType st, Range position)
        {
            if (_generator.MatchSameIntType(ft, st))
                _emitter.MulInt();
            else
                _emitter.CallOperator("*", position, true, ft, st);
        }

        private void EmitDiv(MugValueType ft, MugValueType st, Range position)
        {
            if (_generator.MatchSameIntType(ft, st))
                _emitter.DivInt();
            else
                _emitter.CallOperator("/", position, true, ft, st);
        }

        private void EmitBooleanOperator(string literal, LLVMIntPredicate llvmpredicate, OperatorKind kind, ref MugValueType ft, ref MugValueType st, Range position)
        {
            if ((kind == OperatorKind.CompareEQ || kind == OperatorKind.CompareNEQ) && ft.IsSameEnumOf(st))
            {
                if (_emitter.OneOfTwoIsOnlyTheEnumType())
                    Error(position, "Cannot apply boolean operator on this expression");

                EmitOperator(
                    kind,
                    ft.GetEnum().BaseType.ToMugValueType(position, _generator),
                    st.GetEnum().BaseType.ToMugValueType(position, _generator),
                    position);
            }
            else if (_generator.MatchSameIntType(ft, st))
                _emitter.CompareInt(llvmpredicate);
            else
                _emitter.CallOperator(literal, position, true, ft, st);
        }

        /// <summary>
        /// the function manages the operator implementations for all the types
        /// </summary>
        private void EmitOperator(OperatorKind kind, MugValueType ft, MugValueType st, Range position)
        {
            switch (kind)
            {
                case OperatorKind.Sum:
                    EmitSum(ft, st, position);
                    break;
                case OperatorKind.Subtract:
                    EmitSub(ft, st, position);
                    break;
                case OperatorKind.Multiply:
                    EmitMul(ft, st, position);
                    break;
                case OperatorKind.Divide:
                    EmitDiv(ft, st, position);
                    break;
                case OperatorKind.CompareEQ:
                    EmitBooleanOperator("==", LLVMIntPredicate.LLVMIntEQ, kind, ref ft, ref st, position);
                    break;
                case OperatorKind.CompareNEQ:
                    EmitBooleanOperator("!=", LLVMIntPredicate.LLVMIntNE, kind, ref ft, ref st, position);
                    break;
                case OperatorKind.CompareMajor:
                    EmitBooleanOperator(">", LLVMIntPredicate.LLVMIntSGT, kind, ref ft, ref st, position);
                    break;
                case OperatorKind.CompareMajorEQ:
                    EmitBooleanOperator(">=", LLVMIntPredicate.LLVMIntSGE, kind, ref ft, ref st, position);
                    break;
                case OperatorKind.CompareMinor:
                    EmitBooleanOperator("<", LLVMIntPredicate.LLVMIntSLT, kind, ref ft, ref st, position);
                    break;
                case OperatorKind.CompareMinorEQ:
                    EmitBooleanOperator("<=", LLVMIntPredicate.LLVMIntSLE, kind, ref ft, ref st, position);
                    break;
                /*case OperatorKind.And
                    break;*/
                default:
                    Error(position, "`", kind.ToString(), "` operator not supported yet");
                    break;
            }
        }

        /// <summary>
        /// the function manages the 'as' operator
        /// </summary>
        private void EmitCastInstruction(MugType type, Range position)
        {
            // the expression type to cast
            var expressionType = _emitter.PeekType();
            var castType = type.ToMugValueType(position, _generator);

            if (castType.IsEnum())
            {
                var enumerated = castType.GetEnum();

                if (expressionType.TypeKind != enumerated.BaseType.ToMugValueType(position, _generator).TypeKind)
                    Error(position, "The base type of enum `", enumerated.Name, "` is incompatible with type `", expressionType.ToString(), "`");

                _emitter.CastToEnumMemberFromBaseType(castType);
            }
            else if (expressionType.TypeKind == MugValueTypeKind.Enum)
            {
                if (expressionType.GetEnum().BaseType.ToMugValueType(new(), _generator).TypeKind != castType.TypeKind)
                    Error(position, "Enum base type is incompatible with type `", castType.ToString(), "`");

                _emitter.CastEnumMemberToBaseType(castType);
            }
            else if (expressionType.TypeKind == MugValueTypeKind.String && castType.Equals(MugValueType.Array(MugValueType.Char)))
            {
                var value = _emitter.Pop();
                value.Type = castType;
                _emitter.Load(value);
            }
            else if (expressionType.MatchAnyTypeOfIntType() &&
                castType.MatchAnyTypeOfIntType()) // LLVM has different instructions for each type convertion
                _emitter.CastInt(castType);
            else
                _emitter.CallAsOperator(position, expressionType, type.ToMugValueType(position, _generator));
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
                        structure.GetFieldTypeFromName(m.Member.Value).ToMugValueType(m.Position, _generator),
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
                default:
                    Error(member.Position, "Not supported yet");
                    break;
            }
        }

        /// <summary>
        /// the function returns a string representing the function id and the array of
        /// parameter types in parentheses separated by ', ',
        /// to allow overload of functions
        /// </summary>
        private string BuildName(string name, MugValueType[] parameters)
        {
            return $"{name}({string.Join(", ", parameters)})";
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// the function converts a Callstatement node to the corresponding low-level code
        /// </summary>
        private void EmitCallStatement(CallStatement c, bool expectedNonVoid)
        {
            // an array is prepared for the parameter types of function to call
            var parameters = new MugValueType[c.Parameters.Lenght];

            /* the array is cycled with the expressions of the respective parameters and each expression
             * is evaluated and assigned its type to the array of parameter types
             */
            for (int i = 0; i < c.Parameters.Lenght; i++)
            {
                EvaluateExpression(c.Parameters.Nodes[i]);
                parameters[i] = _emitter.PeekType();
            }

            /*
             * the symbol of the function is taken by passing the name of the complete function which consists
             * of the function id and in brackets the list of parameter types separated by ', '
             */
            var function = _generator.GetSymbol(BuildName(((Token)c.Name).Value, parameters), c.Position);

            // function type: <ret_type> <param_types>
            var functionType = function.Type.LLVMType;

            if (expectedNonVoid)
                _generator.ExpectNonVoidType(
                    // (<ret_type> <param_types>).GetElementType() -> <ret_type>
                    functionType,
                    c.Position);

            _emitter.Call(function.LLVMValue, c.Parameters.Lenght, function.Type);
        }

        private void EmitExprPrefixOperator(PrefixOperator p)
        {
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
        }

        private void EmitExpr(ExpressionNode e)
        {
            // evaluated left
            EvaluateExpression(e.Left);
            // the left expression type
            var ft = _emitter.PeekType();
            // evaluated right
            EvaluateExpression(e.Right);
            // right expression type
            var st = _emitter.PeekType();
            // operator implementation
            EmitOperator(e.Operator, ft, st, e.Position);
        }

        private void EmitExprBool(BooleanExpressionNode b)
        {
            EvaluateExpression(b.Left);
            var ft = _emitter.PeekType();
            EvaluateExpression(b.Right);
            var st = _emitter.PeekType();
            EmitOperator(b.Operator, ft, st, b.Position);
        }

        private void EmitExprArrayElemSelect(ArraySelectElemNode a)
        {
            // loading the array
            EvaluateMemberAccess(a.Left, true);

            var indexed = _emitter.PeekType();

            // loading the index expression
            EvaluateExpression(a.IndexExpression);

            var index = _emitter.PeekType();

            if (indexed.IsIndexable())
                // loading the element
                _emitter.SelectArrayElement();
            else
                _emitter.CallOperator("[]", a.Position, true, indexed, index);
        }

        private void EmitExprAllocateArray(ArrayAllocationNode aa)
        {
            var arraytype = MugValueType.Array(aa.Type.ToMugValueType(aa.Position, _generator));

            // loading the array

            if (aa.SizeIsImplicit)
                _emitter.Load(MugValue.From(LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, (uint)aa.Body.Length), MugValueType.Int32));
            else
                EvaluateExpression(aa.Size);

            var array = MugValue.From(_emitter.Builder.BuildAlloca(arraytype.LLVMType), arraytype);

            var allocation = _emitter.Builder.BuildArrayMalloc(
                    arraytype.ArrayBaseElementType.LLVMType,
                    _emitter.Pop().LLVMValue);

            _emitter.Builder.BuildStore(allocation, array.LLVMValue);

            var arraypointer = _emitter.Builder.BuildLoad(array.LLVMValue);

            var i = 0;

            foreach (var elem in aa.Body)
            {
                EvaluateExpression(elem);

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

            var structure = _generator.GetSymbol(ta.Name.ToMugValueType(ta.Position, _generator).ToString(), ta.Position).Type;

            var tmp = _emitter.Builder.BuildAlloca(
                structure.LLVMType);
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

                var fieldType = structureInfo.GetFieldTypeFromName(field.Name)
                    .ToMugValueType(structureInfo.GetFieldPositionFromName(field.Name), _generator);

                _generator.ExpectSameTypes(
                    fieldType, field.Position, $"expected {fieldType}, but got {_emitter.PeekType()}", _emitter.PeekType());

                _emitter.StoreField(tmp, structureInfo.GetFieldIndexFromName(field.Name));
            }

            _emitter.Load(MugValue.From(_emitter.Builder.BuildLoad(tmp), structure));
        }

        private void EmitExprMemberAccess(MemberNode m)
        {
            if (m.Base is Token token)
            {
                if (_emitter.IsDeclared(token.Value))
                    _emitter.LoadFieldName(token.Value, token.Position);
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

            var structure = _emitter.PeekType().GetStructure();
            var type = structure.GetFieldTypeFromName(m.Member.Value);
            var index = structure.GetFieldIndexFromName(m.Member.Value);
            var instance = _emitter.Pop();

            _emitter.LoadField(instance, type.ToMugValueType(m.Member.Position, _generator), index, true);
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
                default:
                    Error(expression.Position, "expression not supported yet");
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
            // alias for ...
            var parameters = _function.ParameterList.Parameters;

            for (int i = 0; i < parameters.Length; i++)
            {
                // alias for ...
                var parameter = parameters[i];

                var parametertype = parameter.Type.ToMugValueType(parameter.Position, _generator);

                // allocating the local variable
                _emitter.DeclareVariable(
                    parameter.Name,
                    parametertype,
                    parameter.Position);

                // loading onto the stack the parameter
                _emitter.Load(MugValue.From(_llvmfunction.GetParam((uint)i), parametertype));

                // storing the parameter into the variable
                _emitter.StoreVariable(parameter.Name, parameter.Position);
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

            // back to the main block, jump ou of the if scope
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
            _emitter.CompareJump(then, !_emitter.IsInsideSubBlock && i.ElseNode is null ? endcondition : @else);

            // save the old emitter
            var oldemitter = _emitter;

            // define if and else bodies
            // if
            DefineConditionBody(then, endcondition, i.Body, oldemitter);

            // else
            if (i.ElseNode is not null)
                DefineElseBody(@else, endcondition, i.ElseNode, oldemitter);

            // restore old emitter
            _emitter = new(_generator, oldemitter.Memory, oldemitter.ExitBlock, oldemitter.IsInsideSubBlock);

            if (_emitter.IsInsideSubBlock)
            {
                if (i.ElseNode is not null)
                    saveOldCondition.Terminator.SetOperand(1, @else.AsValue());
                else
                    saveOldCondition.Terminator.SetOperand(1, endcondition.AsValue());
            }

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

            _emitter = new(_generator, oldemitter.Memory, endcycle, true);
            // locating the builder in the compare block
            _emitter.Builder.PositionAtEnd(compare);

            // evaluate expression
            EvaluateConditionExpression(i.Expression, i.Position);

            // compare
            _emitter.CompareJump(cycle, endcycle);

            var oldCycleExitBlock = _cycleExitBlock;

            _cycleExitBlock = endcycle;

            // define if and else bodies
            DefineConditionBody(cycle, compare, i.Body, oldemitter);

            // restore old emitter
            _emitter = new(_generator, oldemitter.Memory, oldemitter.ExitBlock, oldemitter.IsInsideSubBlock);

            if (_emitter.IsInsideSubBlock)
            {
                if (saveOldCondition.Terminator.OperandCount >= 2)
                    saveOldCondition.Terminator.SetOperand(1, endcycle.AsValue());
            }

            // re emit the entry block
            _emitter.Builder.PositionAtEnd(endcycle);

            _cycleExitBlock = oldCycleExitBlock;
        }

        private void EmitConditionalStatement(ConditionalStatement i)
        {
            if (i.Kind == TokenKind.KeyIf)
                EmitIfStatement(i);
            else
                EmitWhileStatement(i);
        }

        private List<FieldAssignmentNode> GetDefaultValueOfFields(string name, Range position)
        {
            var s = _generator.GetSymbol(name, position).Type.GetStructure();
            var fields = new FieldAssignmentNode[s.Body.Count];

            for (int i = 0; i < s.Body.Count; i++)
            {
                var field = s.Body[i];

                fields[i] = new FieldAssignmentNode()
                {
                    Name = field.Name,
                    Body = GetDefaultValueOf(field.Type, field.Position)
                };
            }

            return fields.ToList();
        }

        private INode GetDefaultValueOf(MugType type, Range position)
        {
            return type.Kind switch
            {
                TypeKind.Char => new Token(TokenKind.ConstantChar, "\0", new()),
                TypeKind.Int32 => new Token(TokenKind.ConstantDigit, "0", new()),
                TypeKind.UInt8 or TypeKind.UInt32 or TypeKind.UInt64 or TypeKind.Int64 => new CastExpressionNode()
                {
                    Expression = new Token(TokenKind.ConstantChar, "\0", new()),
                    Type = type
                },
                TypeKind.Bool => new Token(TokenKind.ConstantBoolean, "false", new()),
                TypeKind.String => new Token(TokenKind.ConstantString, "", new()),
                TypeKind.Array => new ArrayAllocationNode()
                {
                    Type = type.BaseType is TypeKind kind ? new MugType(position, kind) : (MugType)type.BaseType,
                    Size = new Token(TokenKind.ConstantDigit, "0", new())
                },
                TypeKind.DefinedType => new TypeAllocationNode()
                {
                    Name = type,
                    Body = GetDefaultValueOfFields(type.BaseType.ToString(), position)
                },
                TypeKind.Pointer => _generator.Error<INode>(position, "Pointers must be initialized")
            };
        }

        private void EmitReturnStatement(ReturnStatement @return)
        {
            /*
             * if the expression in the return statement is null, condition verified by calling Returnstatement.Isvoid(),
             * check that the type of function in which it is found returns void.
             */
            if (@return.IsVoid())
            {
                _generator.ExpectSameTypes(
                    _function.Type.ToMugValueType(@return.Position, _generator),
                    @return.Position,
                    "Expected non-void expression",
                    MugValueType.Void);

                _emitter.RetVoid();
            }
            else
            {
                /*
                 * if instead the expression of the return statement has is nothing,
                 * it will be evaluated and then it will be compared the type of the result with the type of return of the function
                 */
                EvaluateExpression(@return.Body);

                var type = _function.Type.ToMugValueType(@return.Position, _generator);

                _generator.ExpectSameTypes(type, @return.Position, $"Expected {type} type, got {_emitter.PeekType()} type", _emitter.PeekType());
                _emitter.Ret();
            }
        }

        private void EmitVariableStatement(VariableStatement variable)
        {
            _generator.ExpectNonVoidType(variable.Type, variable.Position);

            if (!variable.IsAssigned)
            {
                if (variable.Type.IsAutomatic())
                    Error(variable.Position, "Unable to allocate a new variable of `Auto` type");

                variable.Body = GetDefaultValueOf(variable.Type, variable.Position);
            }

            // the expression in the variable’s body is evaluated
            EvaluateExpression(variable.Body);

            /*
             * if in the statement of variable the type is specified explicitly,
             * then a check will be made: the specified type and the type of the result of the expression must be the same.
             */
            if (!variable.Type.IsAutomatic())
                _emitter.DeclareVariable(variable);
            else // if the type is not specified, it will come directly allocate a variable with the same type as the expression result
                _emitter.DeclareVariable(variable.Name, _emitter.PeekType(), variable.Position);

            _emitter.StoreVariable(variable.Name, variable.Position);
        }

        private string IncrementOperatorToString(TokenKind kind)
        {
            return kind switch
            {
                TokenKind.OperatorIncrement => "++",
                TokenKind.OperatorDecrement => "--",
                _ => throw new Exception("unreachable")
            };
        }

        private void EvaluateVariableAssignment(MugValue allocation, TokenKind operatorkind, INode body, Range position, bool isfieldpointer)
        {
            if (!allocation.IsAllocaInstruction() && !isfieldpointer)
                Error(position, "Unable to change a constant value");

            var variableType = allocation.Type;

            if (operatorkind != TokenKind.Equal)
            {
                if (body is not null)
                {
                    _emitter.LoadUnknownAllocation(allocation);

                    EvaluateExpression(body);

                    var expressionType = _emitter.PeekType();

                    if (operatorkind == TokenKind.AddAssignment)
                        EmitSum(variableType, expressionType, position);
                    else if (operatorkind == TokenKind.SubAssignment)
                        EmitSub(variableType, expressionType, position);
                    else if (operatorkind == TokenKind.MulAssignment)
                        EmitMul(variableType, expressionType, position);
                    else if (operatorkind == TokenKind.DivAssignment)
                        EmitDiv(variableType, expressionType, position);
                }
                else if (variableType.MatchIntType())
                {
                    _emitter.LoadUnknownAllocation(allocation);
                    _emitter.Load(MugValue.From(LLVMValueRef.CreateConstInt(variableType.LLVMType, 1), variableType));

                    if (operatorkind == TokenKind.OperatorIncrement)
                        _emitter.AddInt();
                    else if (operatorkind == TokenKind.OperatorIncrement)
                        _emitter.SubInt();
                }
                else
                    _emitter.CallOperator(IncrementOperatorToString(operatorkind), position, false, variableType);
            }
            else
            {
                EvaluateExpression(body);
                _generator.ExpectSameTypes(allocation.Type, position, $"Expected type {allocation.Type}, but got {_emitter.PeekType()}", _emitter.PeekType());
            }
        }

        private void EmitFieldAssignment(MemberNode member, TokenKind operatorkind, INode body, Range position)
        {
            EvaluateMemberAccess(member, false);
            var field = _emitter.Pop();

            EvaluateVariableAssignment(field, operatorkind, body, position, true);

            _emitter.StoreInside(field);
        }

        private void EmitAssignmentStatement(AssignmentStatement a)
        {
            if (a.Name is Token t)
            {
                var allocation = _emitter.GetMemoryAllocation(t.Value, t.Position);
                EvaluateVariableAssignment(allocation, a.Operator, a.Body, a.Position, false);

                if (allocation.Type.IsPointer())
                {
                    _emitter.LoadFromMemory(t.Value, t.Position);
                    _emitter.EmitGCDecrementReferenceCounter();
                }

                _emitter.StoreVariable(t.Value, t.Position);
            }
            else if (a.Name is ArraySelectElemNode aa)
            {
                EvaluateExpression(aa.Left);

                var leftExpr = _emitter.Pop();

                EvaluateExpression(aa.IndexExpression);

                var indexExpr = _emitter.Pop();

                EvaluateExpression(a.Body);

                var expr = _emitter.Pop();

                if (leftExpr.Type.IsIndexable() && leftExpr.Type.ArrayBaseElementType.Equals(expr.Type))
                {
                    var indexptr = _emitter.Builder.BuildGEP(
                        leftExpr.LLVMValue,
                        new[] { indexExpr.LLVMValue });

                    _emitter.Builder.BuildStore(expr.LLVMValue, indexptr);
                }
                else
                {
                    _emitter.Load(leftExpr);
                    _emitter.Load(indexExpr);
                    _emitter.Load(expr);
                    _emitter.CallOperator("[]=", aa.Position, false, leftExpr.Type, indexExpr.Type, expr.Type);
                }
            }
            else if (a.Name is MemberNode m)
                EmitFieldAssignment(m, a.Operator, a.Body, a.Position);
            else
                Error(a.Position, "Not supported yet");
        }

        private void EmitConstantStatement(ConstantStatement constant)
        {
            // evaluating the body expression of the constant
            EvaluateExpression(constant.Body);

            // match the constant explicit type and expression type are the same
            if (!constant.Type.IsAutomatic())
                _generator.ExpectSameTypes(
                    constant.Type.ToMugValueType(
                        constant.Position,
                        _generator), constant.Body.Position, $"Expected {constant.Type} type, got {_emitter.PeekType()} type", _emitter.PeekType());

            // declaring the constant with a name
            _emitter.DeclareConstant(constant.Name, constant.Position);
        }

        private void EmitLoopManagementStatement(LoopManagementStatement management)
        {
            // is not inside a cycle
            if (_cycleExitBlock.Handle == IntPtr.Zero)
                Error(management.Position, "Loop management statements only allowed inside cycle's bodies");

            if (management.Management.Kind == TokenKind.KeyBreak)
                _emitter.Jump(_cycleExitBlock);
            else
                _emitter.Exit();
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
