using System;

namespace JackCompiling
{
    public partial class CodeWriter
    {
        private int _labelCounter = 0;

        private string NewLabel(string name) => $"{name}_{_labelCounter++}";

        /// <summary>Statement; Statement; ...</summary>
        public void WriteStatements(StatementsSyntax statements)
        {
            foreach (var statement in statements.Statements)
            {
                WriteStatement(statement);
            }
        }

        private void WriteStatement(StatementSyntax statement)
        {
            var ok = TryWriteVarAssignmentStatement(statement)
                     || TryWriteProgramFlowStatement(statement)
                     || TryWriteDoStatement(statement)
                     || TryWriteArrayAssignmentStatement(statement)
                     || TryWriteReturnStatement(statement);
            if (!ok)
            {
                throw new FormatException($"Unknown statement [{statement}]");
            }
        }

        /// <summary>let VarName = Expression;</summary>
        private bool TryWriteVarAssignmentStatement(StatementSyntax statement)
        {
            if (statement is not LetStatementSyntax letStatement)
            {
                return false;
            }

            WriteExpression(letStatement.Value);
            if (letStatement.Index is null)
            {
                var varInfo = FindVarInfo(letStatement.VarName.Value)
                              ?? throw new Exception($"Unknown variable {letStatement.VarName.Value}");

                Write($"pop {varInfo.SegmentName} {varInfo.Index}");
                return true;
            }

            var arrayInfo = FindVarInfo(letStatement.VarName.Value)
                            ?? throw new Exception($"Unknown array {letStatement.VarName.Value}");

            Write($"push {arrayInfo.SegmentName} {arrayInfo.Index}");
            WriteExpression(letStatement.Index.Index);
            Write("add");

            Write("pop pointer 1");
            Write("pop that 0");

            return true;
        }

        /// <summary>
        /// if ( Expression ) { Statements } [else { Statements }
        /// while ( Expression ) { Statements }
        /// </summary>
        private bool TryWriteProgramFlowStatement(StatementSyntax statement)
        {
            switch (statement)
            {
                case IfStatementSyntax ifStatement:
                {
                    WriteIfStatement(ifStatement);
                    return true;
                }

                case WhileStatementSyntax whileStatement:
                {
                    WriteWhileStatement(whileStatement);
                    return true;
                }
            }

            return false;
        }

        private void WriteIfStatement(IfStatementSyntax ifStatement)
        {
            var elseLabel = NewLabel("IF_ELSE");
            var endLabel = NewLabel("IF_END");

            WriteExpression(ifStatement.Condition);
            Write("not");
            Write($"if-goto {elseLabel}");

            WriteStatements(ifStatement.TrueStatements);

            if (ifStatement.ElseClause is not null)
            {
                Write($"goto {endLabel}");
                Write($"label {elseLabel}");

                WriteStatements(ifStatement.ElseClause.FalseStatements);

                Write($"label {endLabel}");
            }
            else
            {
                Write($"label {elseLabel}");
            }
        }

        private void WriteWhileStatement(WhileStatementSyntax whileStatement)
        {
            var startLabel = NewLabel("WHILE_EXP");
            var endLabel = NewLabel("WHILE_END");

            Write($"label {startLabel}");

            WriteExpression(whileStatement.Condition);
            Write("not");
            Write($"if-goto {endLabel}");

            WriteStatements(whileStatement.Statements);
            Write($"goto {startLabel}");

            Write($"label {endLabel}");
        }
    }
}