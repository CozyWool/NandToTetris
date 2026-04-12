using System;
using System.Linq;

namespace JackCompiling
{
    public partial class CodeWriter
    {
        /// <summary>
        /// "string constant"
        /// </summary>
        private bool TryWriteStringValue(TermSyntax term)
        {
            if (term is not ValueTermSyntax valueTerm || valueTerm.Value.TokenType != TokenType.StringConstant)
            {
                return false;
            }

            var stringConstant = valueTerm.Value.Value;

            Write($"push constant {stringConstant.Length}");
            Write("call String.new 1");

            foreach (var asciiCode in stringConstant.Select(symbol => (int) symbol))
            {
                Write($"push constant {asciiCode}");
                Write("call String.appendChar 2");
            }

            return true;
        }

        /// <summary>
        /// arr[index]
        /// </summary>
        private bool TryWriteArrayAccess(TermSyntax term)
        {
            if (term is not ValueTermSyntax valueTerm || valueTerm.Indexing is null)
            {
                return false;
            }

            var varInfo = FindVarInfo(valueTerm.Value.Value)
                          ?? throw new Exception($"Unknown variable {valueTerm.Value.Value}");

            Write($"push {varInfo.SegmentName} {varInfo.Index}");
            WriteExpression(valueTerm.Indexing.Index);
            Write("add");

            Write("pop pointer 1");
            Write("push that 0");

            return true;
        }

        /// <summary>
        /// let arr[index] = expr;
        /// </summary>
        private bool TryWriteArrayAssignmentStatement(StatementSyntax statement)
        {
            if (statement is not LetStatementSyntax let || let.Index is null)
            {
                return false;
            }

            var varInfo = FindVarInfo(let.VarName.Value)
                          ?? throw new Exception($"Unknown variable {let.VarName.Value}");

            Write($"push {varInfo.SegmentName} {varInfo.Index}");
            WriteExpression(let.Index.Index);
            Write("add");

            WriteExpression(let.Value);

            Write("pop temp 0");    // сохранить значение правой части
            Write("pop pointer 1"); // THAT = левая часть (адрес)
            Write("push temp 0");   // вернуть значение правой части
            Write("pop that 0");    // левая часть = правая часть

            return true;
        }
    }
}