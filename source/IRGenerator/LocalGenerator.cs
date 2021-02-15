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
        private readonly MugEmitter _emitter;
        private readonly FunctionNode _function;
        private readonly IRGenerator _generator;
        public LocalGenerator(IRGenerator errorHandler, ref FunctionNode function, ref MugEmitter emitter)
        {
            _generator = errorHandler;
            _emitter = emitter;
            _function = function;
        }

        private void Error(Range position, params string[] error)
        {
            _generator.Parser.Lexer.Throw(position, error);
        }

        /*/// <summary>
        /// declares a global string and returns a pointer to it
        /// </summary>
        private LLVMValueRef CreateString(string value)
        {
            // global string
            
            var type = LLVMTypeRef.CreateArray(LLVMTypeRef.Int8, (uint)value.Length + 1);
            var globalArray = _generator.Module.AddGlobal(type, "str");
            // assigning the constant value to the global array
            globalArray.Initializer = LLVMValueRef.CreateConstIntOfString(type, value, 0);

            // creating pointer
            var x = LLVM.BuildGEP(_emitter.Builder, globalArray,
                new[]
                {
                    // see the gep instruction on the llvm documentation
                    LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, (ulong)0),
                    LLVMValueRef.CreateConstInt(LLVMTypeRef.Int32, (ulong)0)
                },
                "");

            return x;
        }*/

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
                TokenKind.ConstantChar => LLVMValueRef.CreateConstInt(LLVMTypeRef.Int8, _generator.StringCharToIntChar(constant.Value)),
                _ => _generator.NotSupportedType<LLVMValueRef>(constant.Kind.ToString(), position)
            };
        }

        /// <summary>
        /// check that type is equal to one of the elements in supportedTypes
        /// </summary>
        private void ExpectOperatorImplementation(LLVMTypeRef type, OperatorKind kind, Range position, params LLVMTypeRef[] supportedTypes)
        {
            for (int i = 0; i < supportedTypes.Length; i++)
                if (Unsafe.Equals(type, supportedTypes[i]))
                    return;

            Error(position, "The expression type does not implement the operator `", kind.ToString(), "`");
        }

        /// <summary>
        /// the function manages the operator implementations for all the types
        /// </summary>
        private void EmitOperator(OperatorKind kind, LLVMTypeRef ft, LLVMTypeRef st, Range position)
        {
            _generator.ExpectSameTypes(ft, position, $"Unsupported operator `{kind}` between different types", st);

            switch (kind)
            {
                case OperatorKind.Sum:
                    ExpectOperatorImplementation(ft, kind, position,
                        LLVMTypeRef.Int64,
                        LLVMTypeRef.Int32,
                        LLVMTypeRef.Int8,
                        LLVMTypeRef.CreatePointer(LLVMTypeRef.Int8, 0));

                    _emitter.Add(position);
                    break;
                case OperatorKind.Subtract:
                    ExpectOperatorImplementation(ft, kind, position,
                        LLVMTypeRef.Int64,
                        LLVMTypeRef.Int32,
                        LLVMTypeRef.Int8);

                    _emitter.Sub();
                    break;
                case OperatorKind.Multiply:
                    ExpectOperatorImplementation(ft, kind, position,
                        LLVMTypeRef.Int32,
                        LLVMTypeRef.Int8);

                    _emitter.Mul();
                    break;
                case OperatorKind.Divide:
                    ExpectOperatorImplementation(ft, kind, position,
                        LLVMTypeRef.Int64,
                        LLVMTypeRef.Int32,
                        LLVMTypeRef.Int8);

                    _emitter.Div();
                    break;
                case OperatorKind.Range: break;
            }
        }

        private void CallOperatorFunction(string name, Range position)
        {
            var function = _generator.GetSymbol(name, position);
            _emitter.Call(function, (int)function.ParamsCount);
        }

        /// <summary>
        /// the function manages the 'as' operator
        /// </summary>
        private void EmitCastInstruction(MugType type, Range position)
        {
            // the expression type to cast
            var expressionType = _emitter.PeekType();

            switch (expressionType.Kind)
            {
                case LLVMTypeKind.LLVMIntegerTypeKind: // LLVM has different instructions for each type convertion
                    _emitter.CastInt(_generator.TypeToLLVMType(type, position));
                    break;
                case LLVMTypeKind.LLVMPointerTypeKind:
                    // string
                    if (Unsafe.Equals(expressionType.ElementType, LLVMTypeRef.Int8))
                    {
                        if (type.Kind == TypeKind.Array && ((MugType)type.BaseType).Kind == TypeKind.Char)
                            CallOperatorFunction(MugEmitter.StringToCharArrayIF, position);
                    }
                    break;
                default:
                    Error(position, "Cast does not support this type yet");
                    break;
            }
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

        /// <summary>
        /// the function evaluates an expression, looking at the given node type
        /// </summary>
        private void EvaluateExpression(INode expression)
        {
            switch (expression)
            {
                case ExpressionNode e: // binary expression: left op right
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
                    break;
                case Token t:
                    if (t.Kind == TokenKind.Identifier) // reference value
                        _emitter.LoadFromMemory(t.Value, t.Position);
                    else // constant value
                        _emitter.Load(ConstToLLVMConst(t, t.Position));
                    break;
                case PrefixOperator p:
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

        public void AllocParameters(LLVMValueRef function, ParameterListNode parameters)
        {
            for (int i = 0; i < parameters.Parameters.Length; i++)
            {
                var parameter = parameters.Parameters[i];
                _emitter.DeclareVariable(parameter.Name, _generator.TypeToLLVMType(parameter.Type, parameter.Position), parameter.Position);
                _emitter.Load(function.GetParam((uint)i));
                _emitter.StoreVariable(parameters.Parameters[i].Name);
            }
        }

        private INode GetDefaultValueOf(MugType type)
        {
            return type.Kind switch
            {
                TypeKind.Char or TypeKind.Int8 or TypeKind.Int32 or TypeKind.Int64 or
                TypeKind.UInt8 or TypeKind.UInt32 or TypeKind.UInt64 => new Token(TokenKind.ConstantDigit, "0", new()),
                TypeKind.Bool => new Token(TokenKind.ConstantBoolean, "false", new()),
                TypeKind.String => new Token(TokenKind.ConstantString, "", new()),
            };
        }

        /// <summary>
        /// 
        /// </summary>
        private void RecognizeStatement(INode statement)
        {
            switch (statement)
            {
                case VariableStatement variable:
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
                    {
                        _emitter.DeclareVariable(variable);
                        var type = _generator.TypeToLLVMType(variable.Type, variable.Position);
                        _generator.ExpectSameTypes(type, variable.Body.Position, $"Expected {type} type, got {_emitter.PeekType()} type", _emitter.PeekType());
                    }
                    else // if the type is not specified, it will come directly allocate a variable with the same type as the expression result
                        _emitter.DeclareVariable(variable.Name, _emitter.PeekType(), variable.Position);

                    _emitter.StoreVariable(variable.Name);
                    break;
                case ReturnStatement @return:
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
                        _generator.ExpectSameTypes(type, @return.Position, $"Expected {type} type, got {_emitter.PeekType()} type", _emitter.PeekType());
                        _emitter.Ret();
                    }
                    break;
                case CallStatement c:
                    EmitCallStatement(c, false);
                    break;
                default:
                    Error(statement.Position, "Statement not supported yet");
                    break;
            }
        }

        /// <summary>
        /// the function cycles all the nodes in the statement array of a Functionnode and
        /// calls a function to convert them into the corresponding low-level code
        /// </summary>
        public void Generate()
        {
            foreach (var statement in _function.Body.Statements)
                RecognizeStatement(statement);
        }
    }
}
