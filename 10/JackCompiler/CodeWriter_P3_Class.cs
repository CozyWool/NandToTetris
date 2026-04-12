using System;
using System.Collections.Generic;
using System.Linq;

namespace JackCompiling
{
    public partial class CodeWriter
    {
        /// <summary>
        /// class Name { ... }
        /// </summary>
        public void WriteClass(ClassSyntax classSyntax)
        {
            currentClassName = classSyntax.Name.Value;

            classSymbols.Clear();

            var staticIndex = 0;
            var fieldIndex = 0;

            foreach (var varDec in classSyntax.ClassVars)
            {
                var kind = varDec.KindKeyword.Value;

                foreach (var name in varDec.DelimitedNames)
                {
                    classSymbols[name.Value] = kind switch
                                               {
                                                   "static" => new VarInfo(staticIndex++, VarKind.Static,
                                                                           varDec.Type.Value),
                                                   "field" => new VarInfo(fieldIndex++, VarKind.Field,
                                                                          varDec.Type.Value),
                                                   _ => throw new FormatException($"Unknown variable kind {kind}")
                                               };
                }
            }

            foreach (var subroutine in classSyntax.SubroutineDec)
            {
                switch (subroutine.KindKeyword.Value)
                {
                    case "method":
                        WriteMethod(subroutine);
                        break;
                    case "function":
                        WriteFunction(subroutine);
                        break;
                    case "constructor":
                        WriteConstructor(subroutine);
                        break;
                    default:
                        throw new FormatException($"Unknown subroutine type {subroutine.KindKeyword.Value}");
                }
            }
        }

        /// <summary>
        /// method Type Name ( ParameterList ) { Body }
        /// </summary>
        private void WriteMethod(SubroutineDecSyntax subroutine)
        {
            InitMethodSymbols(subroutine, true);

            var fullName = $"{currentClassName}.{subroutine.Name.Value}";
            var nLocals = CountLocals(subroutine);

            Write($"function {fullName} {nLocals}");

            Write("push argument 0");
            Write("pop pointer 0");

            WriteStatements(subroutine.SubroutineBody.Statements);
        }

        /// <summary>
        /// function Type Name ( ParameterList ) { Body }
        /// </summary>
        private void WriteFunction(SubroutineDecSyntax subroutine)
        {
            InitMethodSymbols(subroutine, false);

            var fullName = $"{currentClassName}.{subroutine.Name.Value}";
            var nLocals = CountLocals(subroutine);

            Write($"function {fullName} {nLocals}");

            WriteStatements(subroutine.SubroutineBody.Statements);
        }

        /// <summary>
        /// constructor Type Name ( ParameterList ) { Body }
        /// </summary>
        private void WriteConstructor(SubroutineDecSyntax subroutine)
        {
            InitMethodSymbols(subroutine, false);

            var fullName = $"{currentClassName}.{subroutine.Name.Value}";
            var nLocals = CountLocals(subroutine);

            Write($"function {fullName} {nLocals}");

            var fieldCount = classSymbols.Values.Count(v => v.Kind == VarKind.Field);

            Write($"push constant {fieldCount}");
            Write("call Memory.alloc 1");
            Write("pop pointer 0");

            WriteStatements(subroutine.SubroutineBody.Statements);
        }

        private void InitMethodSymbols(SubroutineDecSyntax subroutine, bool isMethod)
        {
            var symbols = new Dictionary<string, VarInfo>();

            var argIndex = isMethod ? 1 : 0;

            foreach (var param in subroutine.ParameterList.DelimitedParameters)
            {
                symbols[param.Name.Value] =
                    new VarInfo(argIndex++, VarKind.Parameter, param.Type.Value);
            }

            var localIndex = 0;

            foreach (var varDec in subroutine.SubroutineBody.VarDec)
            {
                foreach (var name in varDec.DelimitedNames)
                {
                    symbols[name.Value] =
                        new VarInfo(localIndex++, VarKind.Local, varDec.Type.Value);
                }
            }

            methodSymbols = symbols;
        }

        private static int CountLocals(SubroutineDecSyntax subroutine) =>
            subroutine.SubroutineBody.VarDec
                      .Sum(v => v.DelimitedNames.Count);

        /// <summary>
        /// ObjOrClassName . SubroutineName ( ExpressionList )
        /// </summary>
        private bool TryWriteSubroutineCall(TermSyntax term)
        {
            if (term is not SubroutineCallTermSyntax callTerm)
            {
                return false;
            }

            var call = callTerm.Call;

            var argCount = 0;
            string fullName;

            if (call.ObjectOrClass is null)
            {
                Write("push pointer 0");
                argCount++;

                fullName = $"{currentClassName}.{call.SubroutineName.Value}";
            }
            else
            {
                var name = call.ObjectOrClass.Name.Value;
                var varInfo = FindVarInfo(name);

                if (varInfo is not null)
                {
                    Write($"push {varInfo.SegmentName} {varInfo.Index}");
                    argCount++;

                    fullName = $"{varInfo.Type}.{call.SubroutineName.Value}";
                }
                else
                {
                    fullName = $"{name}.{call.SubroutineName.Value}";
                }
            }

            foreach (var arg in call.Arguments.DelimitedExpressions)
            {
                WriteExpression(arg);
                argCount++;
            }

            Write($"call {fullName} {argCount}");
            return true;
        }

        /// <summary>
        /// do SubroutineCall ; 
        /// </summary>
        private bool TryWriteDoStatement(StatementSyntax statement)
        {
            if (statement is not DoStatementSyntax doStatement)
            {
                return false;
            }

            TryWriteSubroutineCall(new SubroutineCallTermSyntax(doStatement.SubroutineCall));

            Write("pop temp 0");

            return true;
        }

        /// <summary>
        /// return ;
        /// return Expression ;
        /// </summary>
        private bool TryWriteReturnStatement(StatementSyntax statement)
        {
            if (statement is not ReturnStatementSyntax returnStatement)
            {
                return false;
            }

            if (returnStatement.ReturnValue is not null)
            {
                WriteExpression(returnStatement.ReturnValue);
            }
            else
            {
                Write("push constant 0");
            }

            Write("return");
            return true;
        }

        /// <summary>
        /// this | null
        /// </summary>
        private bool TryWriteObjectValue(TermSyntax term)
        {
            if (term is not ValueTermSyntax v)
            {
                return false;
            }

            if (v.Value.TokenType != TokenType.Keyword)
            {
                return false;
            }

            switch (v.Value.Value)
            {
                case "this":
                    Write("push pointer 0");
                    return true;
                case "null":
                    Write("push constant 0");
                    return true;
                default:
                    return false;
            }
        }
    }
}