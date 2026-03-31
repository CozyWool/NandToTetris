using System;
using System.Collections.Generic;
using System.Linq;

namespace JackCompiling
{
    public class Parser
    {
        private readonly Tokenizer tokenizer;

        public Parser(Tokenizer tokenizer)
        {
            this.tokenizer = tokenizer;
        }

        public ClassSyntax ReadClass()
        {
            throw new NotImplementedException();
        }

        public StatementsSyntax ReadStatements()
        {
            throw new NotImplementedException();
        }

        public SubroutineCall ReadSubroutineCall()
        {
            throw new NotImplementedException();
        }

        public ParameterListSyntax ReadParameterList()
        {
            throw new NotImplementedException();
        }

        public ExpressionSyntax ReadExpression()
        {
            throw new NotImplementedException();
        }

        public TermSyntax ReadTerm()
        {
            throw new NotImplementedException();
        }
    }
}
