using LLVMSharp.Interop;
using Mug.Compilation;
using Mug.Models.Generator.Emitter;
using Mug.Models.Lexer;
using Mug.Models.Parser;
using Mug.Models.Parser.NodeKinds;
using Mug.Models.Parser.NodeKinds.Statements;
using Mug.TypeSystem;
using System;
using System.Runtime.CompilerServices;

namespace Mug.Models.Generator
{
    public class LocalGenerator
    {
        // code emitter
        private MugEmitter _emitter;
        // function info
        private readonly FunctionNode _function;
        // pointers
        private readonly IRGenerator _generator;
        private readonly LLVMValueRef _llvmfunction;

        public LocalGenerator(IRGenerator errorHandler, ref LLVMValueRef llvmfunction, ref FunctionNode function, ref MugEmitter emitter)
        {
            _generator = errorHandler;
            _emitter = emitter;
            _function = function;
            _llvmfunction = llvmfunction;
        }

        private void Error(Range position, params string[] error)
        {
            _generator.Parser.Lexer.Throw(position, error);
        }

        /// <summary>
        /// converts a constant in token format to one in LLVMValueRef format
        /// </summary>
        public LLVMValueRef ConstToLLVMConst(Token constant, Range position)
        {
            return constant.Kind switch
            {
                TokenKind.ConstantDigit => LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, Convert.ToUInt64(constant.Value)),
                TokenKind.ConstantBoolean => LLVMValueRef.CreateConstInt(LLVMTypeRef.Int1, _generator.StringBoolToIntBool(constant.Value)),
                TokenKind.ConstantString => _emitter.Builder.BuildGEP(
                    _emitter.Builder.BuildGlobalString(constant.Value),
                    new[]
                    {
                        LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0),
                        LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, 0),
                    }
                ),
                TokenKind.ConstantChar => LLVMValueRef.CreateConstInt(LLVMTypeRef.Int16, _generator.StringCharToIntChar(constant.Value)),
                _ => _generator.NotSupportedType<LLVMValueRef>(constant.Kind.ToString(), position)
            };
        }

        /// <summary>
        /// the function manages the operator implementations for all the types
        /// </summary>
        private void EmitOperator(OperatorKind kind, LLVMTypeRef ft, LLVMTypeRef st, Range position)
        {
            switch (kind)
            {
                case OperatorKind.Sum:
                    if (_generator.MatchSameIntType(ft, st))
                        _emitter.AddInt();
                    else
                        _emitter.CallOperator("+", position, ft, st);
                    break;
                case OperatorKind.Subtract:
                    if (_generator.MatchSameIntType(ft, st))
                        _emitter.SubInt();
                    else
                        _emitter.CallOperator("-", position, ft, st);
                    break;
                case OperatorKind.Multiply:
                    if (_generator.MatchSameIntType(ft, st))
                        _emitter.MulInt();
                    else
                        _emitter.CallOperator("*", position, ft, st);
                    break;
                case OperatorKind.Divide:
                    if (_generator.MatchSameIntType(ft, st))
                        _emitter.DivInt();
                    else
                        _emitter.CallOperator("/", position, ft, st);
                    break;
                case OperatorKind.CompareEQ:
                    if (_generator.MatchSameIntType(ft, st))
                        _emitter.CompareInt(LLVMIntPredicate.LLVMIntEQ);
                    else
                        _emitter.CallOperator("==", position, ft, st);
                    break;
                case OperatorKind.CompareNEQ:
                    if (_generator.MatchSameIntType(ft, st))
                        _emitter.CompareInt(LLVMIntPredicate.LLVMIntNE);
                    else
                        _emitter.CallOperator("!=", position, ft, st);
                    break;
                case OperatorKind.CompareMajor:
                    if (_generator.MatchSameIntType(ft, st))
                        _emitter.CompareInt(LLVMIntPredicate.LLVMIntSGT);
                    else
                        _emitter.CallOperator(">", position, ft, st);
                    break;
                case OperatorKind.CompareMajorEQ:
                    if (_generator.MatchSameIntType(ft, st))
                        _emitter.CompareInt(LLVMIntPredicate.LLVMIntSGE);
                    else
                        _emitter.CallOperator(">=", position, ft, st);
                    break;
                case OperatorKind.CompareMinor:
                    if (_generator.MatchSameIntType(ft, st))
                        _emitter.CompareInt(LLVMIntPredicate.LLVMIntSLT);
                    else
                        _emitter.CallOperator("<", position, ft, st);
                    break;
                case OperatorKind.CompareMinorEQ:
                    if (_generator.MatchSameIntType(ft, st))
                        _emitter.CompareInt(LLVMIntPredicate.LLVMIntSLE);
                    else
                        _emitter.CallOperator("<=", position, ft, st);
                    break;
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
            var castType = _generator.TypeToLLVMType(type, position);

            if (_generator.MatchAnyTypeOfIntType(expressionType) &&
                _generator.MatchAnyTypeOfIntType(castType)) // LLVM has different instructions for each type convertion
                _emitter.CastInt(castType);
            else
                _emitter.CallOperator($"as {type}", position, expressionType);
        }

        /// <summary>
        /// wip function
        /// the function evaluates an instance node, for example: base.method()
        /// </summary>
        private string EvaluateInstanceName(INode instance)
        {
            switch (instance)
            {
                case Token t:
                    return t.Value;
                default:
                    Error(instance.Position, "Not supported yet");
                    return null;
            }
        }

        /// <summary>
        /// the function returns a string representing the function id and the array of
        /// parameter types in parentheses separated by ', ',
        /// to allow overload of functions
        /// </summary>
        private string BuildName(string name, LLVMTypeRef[] parameters)
        {
            return $"{name}({string.Join(", ", parameters)})";
        }

        /// <summary>
        /// the function converts a Callstatement node to the corresponding low-level code
        /// </summary>
        private void EmitCallStatement(CallStatement c, bool expectedNonVoid)
        {
            // an array is prepared for the parameter types of function to call
            var parameters = new LLVMTypeRef[c.Parameters.Lenght];

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
            var function = _generator.GetSymbol(BuildName(EvaluateInstanceName(c.Name), parameters), c.Position);
            
            // function type: <ret_type> <param_types>
            var functionType = function.TypeOf.ElementType;

            if (expectedNonVoid)
                _generator.ExpectNonVoidType(
                    // (<ret_type> <param_types>).GetElementType() -> <ret_type>
                    functionType.ReturnType,
                    c.Position);

            _emitter.Call(function, c.Parameters.Lenght);
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
                        _emitter.Load(ConstToLLVMConst(t, t.Position));
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

                // allocating the local variable
                _emitter.DeclareVariable(parameter.Name, _generator.TypeToLLVMType(parameter.Type, parameter.Position), parameter.Position);
                // loading onto the stack the parameter
                _emitter.Load(_llvmfunction.GetParam((uint)i));

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
                _emitter = new MugEmitter(_generator, oldemitter.Memory);
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

        private void DefineConditionBody(LLVMBasicBlockRef then, LLVMBasicBlockRef endifelse, BlockNode body, MugEmitter oldemitter)
        {
            // allocating a new emitter with the old symbols
            _emitter = new MugEmitter(_generator, oldemitter.Memory);
            // locating the emitter builder at the end of the block
            _emitter.Builder.PositionAtEnd(then);

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

        private void EmitConditionalStatement(ConditionalStatement i)
        {
            // evaluate expression
            EvaluateConditionExpression(i.Expression, i.Position);

            // if block
            var then = _llvmfunction.AppendBasicBlock("");

            // else block
            var @else = _llvmfunction.AppendBasicBlock("");

            LLVMBasicBlockRef endifelse = i.ElseNode is not null ? _llvmfunction.AppendBasicBlock("") : @else;

            // compare
            _emitter.CompareJump(then, @else);

            // save the old emitter
            var oldemitter = _emitter;

            // define if and else bodies
            DefineConditionBody(then, endifelse, i.Body, oldemitter);

            if (i.ElseNode is not null)
                DefineElseBody(@else, endifelse, i.ElseNode, oldemitter);

            // restore old emitter
            _emitter = new(_generator, oldemitter.Memory);
            // re emit the entry block
            _emitter.Builder.PositionAtEnd(endifelse);
        }

        private INode GetDefaultValueOf(MugType type)
        {
            return type.Kind switch
            {
                TypeKind.Char or TypeKind.Int32 or TypeKind.Int64 or
                TypeKind.UInt8 or TypeKind.UInt32 or TypeKind.UInt64 => new Token(TokenKind.ConstantDigit, "0", new()),
                TypeKind.Bool => new Token(TokenKind.ConstantBoolean, "false", new()),
                TypeKind.String => new Token(TokenKind.ConstantString, "", new()),
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
                _generator.ExpectSameTypes(_generator.TypeToLLVMType(_function.Type, @return.Position), @return.Position, "Expected non-void expression", LLVMTypeRef.Void);
                _emitter.RetVoid();
            }
            else
            {
                /*
                 * if instead the expression of the return statement has is nothing,
                 * it will be evaluated and then it will be compared the type of the result with the type of return of the function
                 */
                EvaluateExpression(@return.Body);
                var type = _generator.TypeToLLVMType(_function.Type, @return.Position);
                _generator.ExpectSameTypes(type, @return.Position, $"Expected {type.ToMugTypeString()} type, got {_emitter.PeekType().ToMugTypeString()} type", _emitter.PeekType());
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

                variable.Body = GetDefaultValueOf(variable.Type);
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

        private void EmitAssignmentStatement(AssignmentStatement a)
        {
            var name = EvaluateInstanceName(a.Name);

            EvaluateExpression(a.Body);

            _emitter.StoreVariable(name, a.Position);
        }

        private void EmitConstantStatement(ConstantStatement constant)
        {
            // evaluating the body expression of the constant
            EvaluateExpression(constant.Body);

            // match the constant explicit type and expression type are the same
            if (!constant.Type.IsAutomatic())
                _generator.ExpectSameTypes(_generator.TypeToLLVMType(constant.Type, constant.Position), constant.Body.Position, $"Expected {constant.Type} type, got {_emitter.PeekType().ToMugTypeString()} type", _emitter.PeekType());

            // declaring the constant with a name
            _emitter.DeclareConstant(constant.Name, constant.Position);
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
            foreach (var statement in statements.Statements)
                RecognizeStatement(statement);
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
