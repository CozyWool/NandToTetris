using System;
using System.Collections.Generic;

namespace JackCompiling
{
    public partial class CodeWriter
    {
        private static readonly Dictionary<string, string> BinaryOps = new()
                                                                       {
                                                                           {"+", "add"},
                                                                           {"-", "sub"},
                                                                           {"*", "call Math.multiply 2"},
                                                                           {"/", "call Math.divide 2"},
                                                                           {"&", "and"},
                                                                           {"|", "or"},
                                                                           {"<", "lt"},
                                                                           {">", "gt"},
                                                                           {"=", "eq"}
                                                                       };

        /// <summary>2+x</summary>
        public void WriteExpression(ExpressionSyntax expression)
        {
            WriteTerm(expression.Term);

            foreach (var tail in expression.Tail)
            {
                WriteTerm(tail.Term);

                if (!BinaryOps.TryGetValue(tail.Operator.Value, out var command))
                {
                    throw new FormatException($"Unknown operator {tail.Operator.Value}");
                }

                Write(command);
            }
        }

        private void WriteTerm(TermSyntax term)
        {
            var ok = TryWriteStringValue(term)
                     || TryWriteArrayAccess(term)
                     || TryWriteObjectValue(term)
                     || TryWriteSubroutineCall(term)
                     || TryWriteNumericTerm(term);
            if (!ok)
            {
                throw new FormatException($"Unknown term [{term}]");
            }
        }

        /// <summary>42 | true | false | varName | -x | ( x )</summary>
        private bool TryWriteNumericTerm(TermSyntax term)
        {
            return term switch
                   {
                       ValueTermSyntax v         => TryWriteConstant(v) || TryWriteKeyword(v) || TryWriteVariable(v),
                       UnaryOpTermSyntax u       => TryWriteUnary(u),
                       ParenthesizedTermSyntax p => TryWriteParenthesized(p),
                       _                         => false
                   };
        }

        private bool TryWriteConstant(ValueTermSyntax term)
        {
            if (term.Value.TokenType != TokenType.IntegerConstant)
            {
                return false;
            }

            Write($"push constant {term.Value.Value}");
            return true;
        }

        private bool TryWriteKeyword(ValueTermSyntax term)
        {
            if (term.Value.TokenType != TokenType.Keyword)
            {
                return false;
            }

            switch (term.Value.Value)
            {
                case "true":
                    Write("push constant 0");
                    Write("not");
                    return true;

                case "false":
                case "null":
                    Write("push constant 0");
                    return true;

                case "this":
                    Write("push pointer 0");
                    return true;

                default:
                    return false;
            }
        }

        private bool TryWriteVariable(ValueTermSyntax term)
        {
            if (term.Value.TokenType != TokenType.Identifier)
            {
                return false;
            }

            var varInfo = FindVarInfo(term.Value.Value)
                          ?? throw new Exception($"Unknown variable {term.Value.Value}");

            if (term.Indexing is not null)
            {
                PushVar(varInfo);
                WriteExpression(term.Indexing.Index);
                Write("add");
                Write("pop pointer 1");
                Write("push that 0");
            }
            else
            {
                PushVar(varInfo);
            }

            return true;
        }

        private bool TryWriteUnary(UnaryOpTermSyntax term)
        {
            WriteTerm(term.Term);

            switch (term.UnaryOp.Value)
            {
                case "-":
                    Write("neg");
                    break;
                case "~":
                    Write("not");
                    break;
                default:
                    throw new FormatException($"Unknown unary op {term.UnaryOp.Value}");
            }

            return true;
        }

        private bool TryWriteParenthesized(ParenthesizedTermSyntax term)
        {
            WriteExpression(term.Expression);
            return true;
        }

        private void PushVar(VarInfo varInfo)
        {
            Write($"push {varInfo.SegmentName} {varInfo.Index}");
        }
    }
}