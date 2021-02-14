using LLVMSharp;
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

        /// <summary>
        /// declares a global string and returns a pointer to it
        /// </summary>
        private LLVMValueRef CreateString(string value)
        {
            // global string
            var globalArray = LLVM.AddGlobal(_generator.Module, LLVMTypeRef.ArrayType(LLVMTypeRef.Int8Type(), (uint)value.Length+1), "str");
            // assigning the constant value to the global array
            LLVM.SetInitializer(globalArray, LLVM.ConstString(value, (uint)value.Length, MugEmitter.ConstLLVMFalse));

            // creating pointer
            var x = LLVM.BuildGEP(_emitter.Builder, globalArray,
                new[]
                {
                    // see the gep instruction on the llvm documentation
                    LLVM.ConstInt(LLVMTypeRef.Int32Type(), 0, MugEmitter.ConstLLVMFalse),
                    LLVM.ConstInt(LLVMTypeRef.Int32Type(), 0, MugEmitter.ConstLLVMFalse)
                },
                "");

            return x;
        }

        /// <summary>
        /// converts a constant in token format to one in LLVMValueRef format
        /// </summary>
        public LLVMValueRef ConstToLLVMConst(Token constant, Range position)
        {
            return constant.Kind switch
            {
                TokenKind.ConstantDigit => LLVMTypeRef.ConstInt(LLVMTypeRef.Int32Type(), Convert.ToUInt64(constant.Value), MugEmitter.ConstLLVMFalse),
                TokenKind.ConstantBoolean => LLVMTypeRef.ConstInt(LLVMTypeRef.Int1Type(), _generator.StringBoolToIntBool(constant.Value), MugEmitter.ConstLLVMTrue),
                TokenKind.ConstantString => CreateString(constant.Value),
                TokenKind.ConstantChar => LLVMTypeRef.ConstInt(LLVMTypeRef.Int8Type(), _generator.StringCharToIntChar(constant.Value), MugEmitter.ConstLLVMFalse),
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
                        LLVMTypeRef.Int32Type(),
                        LLVMTypeRef.Int8Type());

                    _emitter.Add();
                    break;
                case OperatorKind.Subtract:
                    ExpectOperatorImplementation(ft, kind, position,
                        LLVMTypeRef.Int32Type(),
                        LLVMTypeRef.Int8Type());

                    _emitter.Sub();
                    break;
                case OperatorKind.Multiply:
                    ExpectOperatorImplementation(ft, kind, position,
                        LLVMTypeRef.Int32Type(),
                        LLVMTypeRef.Int8Type());

                    _emitter.Mul();
                    break;
                case OperatorKind.Divide:
                    ExpectOperatorImplementation(ft, kind, position,
                        LLVMTypeRef.Int32Type(),
                        LLVMTypeRef.Int8Type());

                    _emitter.Div();
                    break;
                case OperatorKind.Range: break;
            }
        }

        /// <summary>
        /// the function manages the 'as' operator
        /// </summary>
        private void EmitCastInstruction(MugType type, Range position)
        {
            // the expression type to cast
            var expressionType = _emitter.PeekType();

            switch (expressionType.TypeKind)
            {
                case LLVMTypeKind.LLVMIntegerTypeKind: // LLVM has different instructions for each type convertion
                    _emitter.CastInt(_generator.TypeToLLVMType(type, position));
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
            var functionType = function.TypeOf().GetElementType();

            if (expectedNonVoid)
                _generator.ExpectNonVoidType(
                    // (<ret_type> <param_types>).GetElementType() -> <ret_type>
                    functionType.GetElementType(),
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
                _emitter.Load(LLVM.GetParam(function, (uint)i));
                _emitter.StoreVariable(parameters.Parameters[i].Name);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void RecognizeStatement(INode statement)
        {
            switch (statement)
            {
                case VariableStatement variable:
                    // the expression in the variable’s body is evaluated
                    EvaluateExpression(variable.Body);

                    /*
                     * if in the statement of variable the type is specified explicitly,
                     * then a check will be made: the specified type and the type of the result of the expression must be the same.
                     */
                    if (!variable.Type.IsAutomatic())
                    {
                        _emitter.DeclareVariable(variable);
                        _generator.ExpectSameTypes(_generator.TypeToLLVMType(variable.Type, variable.Position), variable.Body.Position, "The expression type and the variable type are different", _emitter.PeekType());
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
                        _generator.ExpectSameTypes(_generator.TypeToLLVMType(_function.Type, @return.Position), @return.Position, "Expected non-void expression", LLVM.VoidType());
                        _emitter.RetVoid();
                    }
                    else
                    {
                        /*
                         * if instead the expression of the return statement has is nothing,
                         * it will be evaluated and then it will be compared the type of the result with the type of return of the function
                         */
                        EvaluateExpression(@return.Body);
                        _generator.ExpectSameTypes(_generator.TypeToLLVMType(_function.Type, @return.Position), @return.Position, "The function return type and the expression type are different", _emitter.PeekType());
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
