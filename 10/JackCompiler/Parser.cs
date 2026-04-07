using System;
using System.Collections.Generic;

namespace JackCompiling
{
    public class Parser
    {
        private readonly Tokenizer _tokenizer;

        private static readonly HashSet<char> Operators = new()
                                                          {
                                                              '+', '-', '*', '/', '&', '|', '<', '>', '='
                                                          };

        private static readonly HashSet<char> UnaryOperators = new()
                                                               {
                                                                   '-', '~'
                                                               };

        public Parser(Tokenizer tokenizer)
        {
            _tokenizer = tokenizer;
        }

        public ClassSyntax ReadClass()
        {
            var classToken = _tokenizer.Read("class");
            var className = ReadIdentifier();
            var openBrace = _tokenizer.Read("{");
            var classVarDec = _tokenizer.ReadList(ReadClassVariableDeclaration);
            var subroutineDec = _tokenizer.ReadList(ReadSubroutineDeclaration);
            var closeBrace = _tokenizer.Read("}");
            return new ClassSyntax(classToken, className, openBrace, classVarDec, subroutineDec, closeBrace);
        }

        private ClassVarDecSyntax? ReadClassVariableDeclaration(Token kindKeyword)
        {
            if (kindKeyword.Value is not "static" and not "field")
            {
                return null;
            }

            var type = ReadType();
            var delimitedNames =
                _tokenizer.ReadDelimitedList(ReadIdentifier, ",", ";");
            var semicolon = _tokenizer.Read(";");

            return new ClassVarDecSyntax(kindKeyword, type, delimitedNames, semicolon);
        }


        private SubroutineDecSyntax? ReadSubroutineDeclaration(Token kindKeyword)
        {
            if (kindKeyword.Value is not "constructor" and not "function" and not "method")
            {
                return null;
            }

            var returnType = _tokenizer.Read();
            if (returnType.Value is not "void")
            {
                _tokenizer.PushBack(returnType);
                returnType = ReadType();
            }

            var subroutineName = ReadIdentifier();
            var openArgsParenthesis = _tokenizer.Read("(");
            var parameterList = ReadParameterList();
            var closeArgsParenthesis = _tokenizer.Read(")");
            var subroutineBody = ReadSubroutineBody();

            return new SubroutineDecSyntax(kindKeyword,
                                           returnType,
                                           subroutineName,
                                           openArgsParenthesis,
                                           parameterList,
                                           closeArgsParenthesis,
                                           subroutineBody);
        }

        private SubroutineBodySyntax ReadSubroutineBody()
        {
            var openBrace = _tokenizer.Read("{");
            var varDec = _tokenizer.ReadList(ReadVariableDeclaration);
            var statements = ReadStatements();
            var closeBrace = _tokenizer.Read("}");
            return new SubroutineBodySyntax(openBrace, varDec, statements, closeBrace);
        }

        private VarDecSyntax? ReadVariableDeclaration(Token kindKeyword)
        {
            if (kindKeyword.Value is not "var")
            {
                return null;
            }

            var type = ReadType();
            var delimitedNames =
                _tokenizer.ReadDelimitedList(ReadIdentifier, ",", ";");
            var semicolon = _tokenizer.Read(";");
            return new VarDecSyntax(kindKeyword, type, delimitedNames, semicolon);
        }

        public StatementsSyntax ReadStatements() => new(_tokenizer.ReadList(ReadStatement));

        private StatementSyntax? ReadStatement(Token kindKeyword)
        {
            return kindKeyword.Value switch
                   {
                       "let"    => ReadLetStatement(kindKeyword),
                       "if"     => ReadIfStatement(kindKeyword),
                       "while"  => ReadWhileStatement(kindKeyword),
                       "do"     => ReadDoStatement(kindKeyword),
                       "return" => ReadReturnStatement(kindKeyword),
                       _        => null
                   };
        }

        private LetStatementSyntax ReadLetStatement(Token kindKeyword)
        {
            var varName = ReadIdentifier();

            Indexing? index = null;
            var openBracket = _tokenizer.Read();
            if (openBracket.Value == "[")
            {
                var indexExpression = ReadExpression();
                var closeBracket = _tokenizer.Read("]");
                index = new Indexing(openBracket, indexExpression, closeBracket);
            }
            else
            {
                _tokenizer.PushBack(openBracket);
            }

            var equals = _tokenizer.Read("=");
            var expression = ReadExpression();
            var semicolon = _tokenizer.Read(";");
            return new LetStatementSyntax(kindKeyword, varName, index, equals, expression, semicolon);
        }

        private IfStatementSyntax ReadIfStatement(Token kindKeyword)
        {
            var openParenthesis = _tokenizer.Read("(");
            var conditionExpression = ReadExpression();
            var closeParenthesis = _tokenizer.Read(")");

            var openTrueBrace = _tokenizer.Read("{");
            var trueStatements = ReadStatements();
            var closeTrueBrace = _tokenizer.Read("}");

            ElseClause? elseClause = null;
            var elseKeyword = _tokenizer.TryReadNext();
            if (elseKeyword?.Value is "else")
            {
                elseClause = ReadElseClause(elseKeyword);
            }
            else
            {
                _tokenizer.PushBack(elseKeyword);
            }

            return new IfStatementSyntax(kindKeyword,
                                         openParenthesis,
                                         conditionExpression,
                                         closeParenthesis,
                                         openTrueBrace,
                                         trueStatements,
                                         closeTrueBrace,
                                         elseClause);
        }

        private ElseClause ReadElseClause(Token elseKeyword)
        {
            var openElseBrace = _tokenizer.Read("{");
            var elseStatements = ReadStatements();
            var closeElseBrace = _tokenizer.Read("}");
            return new ElseClause(elseKeyword, openElseBrace, elseStatements, closeElseBrace);
        }

        private WhileStatementSyntax ReadWhileStatement(Token kindKeyword)
        {
            var openParenthesis = _tokenizer.Read("(");
            var conditionExpression = ReadExpression();
            var closeParenthesis = _tokenizer.Read(")");

            var openBrace = _tokenizer.Read("{");
            var statements = ReadStatements();
            var closeBrace = _tokenizer.Read("}");
            return new WhileStatementSyntax(kindKeyword,
                                            openParenthesis,
                                            conditionExpression,
                                            closeParenthesis,
                                            openBrace,
                                            statements,
                                            closeBrace);
        }

        private DoStatementSyntax ReadDoStatement(Token kindKeyword)
        {
            var subroutineCall = ReadSubroutineCall();
            var semicolon = _tokenizer.Read(";");
            return new DoStatementSyntax(kindKeyword, subroutineCall, semicolon);
        }

        private ReturnStatementSyntax ReadReturnStatement(Token kindKeyword)
        {
            var nextToken = _tokenizer.Read();
            ExpressionSyntax? returnExpression = null;
            if (nextToken.Value is not ";")
            {
                _tokenizer.PushBack(nextToken);
                returnExpression = ReadExpression();
            }

            var semiColon = returnExpression is null ? nextToken : _tokenizer.Read(";");
            return new ReturnStatementSyntax(kindKeyword, returnExpression, semiColon);
        }

        public ParameterListSyntax ReadParameterList()
        {
            var delimitedParameters =
                _tokenizer.ReadDelimitedList(() => new Parameter(ReadType(), ReadIdentifier()),
                                             ",", ")");
            return new ParameterListSyntax(delimitedParameters);
        }


        public ExpressionSyntax ReadExpression()
        {
            var term = ReadTerm();
            var tail = _tokenizer.ReadList(token => CheckOp(token) is null
                                                        ? null
                                                        : new ExpressionTail(token, ReadTerm()));
            return new ExpressionSyntax(term, tail);
        }

        public TermSyntax ReadTerm()
        {
            var token = _tokenizer.Read();
            switch (token.TokenType)
            {
                case TokenType.IntegerConstant:
                case TokenType.StringConstant:
                case TokenType.Keyword:
                    return new ValueTermSyntax(token, null);
                case TokenType.Identifier:
                    Indexing? index = null;
                    var nextToken = _tokenizer.TryReadNext();
                    if (nextToken is null)
                    {
                        return new ValueTermSyntax(token, null);
                    }

                    if (nextToken.Value is "[")
                    {
                        var indexExpression = ReadExpression();
                        var closeBracket = _tokenizer.Read("]");
                        index = new Indexing(nextToken, indexExpression, closeBracket);
                    }
                    else if (nextToken.Value is "(" or ".")
                    {
                        _tokenizer.PushBack(nextToken);
                        _tokenizer.PushBack(token);
                        return new SubroutineCallTermSyntax(ReadSubroutineCall());
                    }
                    else
                    {
                        _tokenizer.PushBack(nextToken);
                    }

                    return new ValueTermSyntax(token, index);
                case TokenType.Symbol:
                    if (token.Value is "(")
                    {
                        return new ParenthesizedTermSyntax(token, ReadExpression(), _tokenizer.Read(")"));
                    }

                    return new UnaryOpTermSyntax(CheckUnaryOp(token), ReadTerm());
                default:

                    throw new ExpectedException("term", token);
            }
        }

        public SubroutineCall ReadSubroutineCall()
        {
            MethodObjectOrClass? methodObjectOrClass = null;
            var classOrVarName = ReadIdentifier();
            var dot = _tokenizer.Read();
            if (dot.Value is ".")
            {
                methodObjectOrClass = new MethodObjectOrClass(classOrVarName, dot);
            }
            else
            {
                _tokenizer.PushBack(dot);
            }

            var subroutineName = methodObjectOrClass is null ? classOrVarName : ReadIdentifier();

            var openParenthesis = _tokenizer.Read("(");
            var expressionList = new ExpressionListSyntax(_tokenizer.ReadDelimitedList(ReadExpression, ",", ")"));
            var closeParenthesis = _tokenizer.Read(")");
            return new SubroutineCall(methodObjectOrClass, subroutineName, openParenthesis, expressionList,
                                      closeParenthesis);
        }

        private Token? CheckOp(Token op) =>
            Operators.Contains(op.Value[0]) || UnaryOperators.Contains(op.Value[0])
                ? op
                : null;

        private Token CheckUnaryOp(Token op) => UnaryOperators.Contains(op.Value[0])
                                                    ? op
                                                    : throw new ExpectedException("unary operator", op);

        private Token ReadIdentifier()
        {
            return _tokenizer.Read(TokenType.Identifier);
        }

        private Token ReadType()
        {
            var type = _tokenizer.Read();
            if (type.Value is not "int" and not "char" and not "boolean" && type.TokenType != TokenType.Identifier)
            {
                throw new ExpectedException("type", type);
            }

            return type;
        }
    }
}