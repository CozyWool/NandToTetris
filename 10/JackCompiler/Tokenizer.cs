using System;
using System.Collections.Generic;

namespace JackCompiling
{
    public class Tokenizer
    {
        private readonly string _text;
        private int _pos = 0;


        private readonly Stack<Token> _tokens = new();

        private static readonly HashSet<string> Keywords = new()
                                                           {
                                                               "class", "constructor", "function", "method", "field",
                                                               "static", "var",
                                                               "int", "char", "boolean", "void", "true", "false",
                                                               "null", "this",
                                                               "let", "do", "if", "else", "while", "return"
                                                           };

        private static readonly HashSet<char> Symbols = new()
                                                        {
                                                            '{', '}', '(', ')', '[', ']', '.', ',', ';', '+', '-', '*',
                                                            '/', '&', '|', '<', '>', '=', '~'
                                                        };

        public Tokenizer(string text)
        {
            _text = text;
        }

        /// <summary>
        /// Сначала возвращает все токены, которые вернули методом PushBack в порядке First In Last Out.
        /// Потом читает и возвращает один следующий токен, либо null, если больше токенов нет.
        /// Пропускает пробелы и комментарии.
        ///
        /// Хорошо, если внутри Token сохранит ещё и строку и позицию в исходном тексте. Но это не проверяется тестами.
        /// </summary>
        public Token? TryReadNext()
        {
            if (_tokens.Count > 0)
            {
                return _tokens.Pop();
            }

            SkipWhitespacesAndComments();

            if (_pos >= _text.Length)
            {
                return null;
            }

            return TryReadStringConstant() ??
                   TryReadIntegerConstant() ??
                   TryReadIdentifierOrKeyword() ??
                   TryReadSymbol() ??
                   throw new Exception($"Unexpected symbol: {_text[_pos]}");
        }


        private void SkipWhitespacesAndComments()
        {
            while (_pos < _text.Length)
            {
                var current = _text[_pos];
                if (char.IsWhiteSpace(current))
                {
                    _pos++;
                    continue;
                }

                if (SkipComment("//", "\n") || SkipComment("/*", "*/"))
                {
                    continue;
                }

                break;
            }
        }

        private bool SkipComment(string start, string end)
        {
            if (!StartsWith(start))
            {
                return false;
            }

            _pos += start.Length;

            while (_pos < _text.Length && !StartsWith(end))
            {
                _pos++;
            }

            _pos += end.Length;
            return true;
        }


        private Token? TryReadStringConstant()
        {
            if (_text[_pos] != '"')
            {
                return null;
            }

            _pos++; // открывающаяся "

            var start = _pos;
            while (_pos < _text.Length && _text[_pos] != '"')
            {
                _pos++;
            }

            var value = _text[start.._pos];
            _pos++; // закрывающаяся "

            return new Token(TokenType.StringConstant, value, 0, 0);
        }

        private Token? TryReadIntegerConstant()
        {
            if (!char.IsDigit(_text[_pos]))
            {
                return null;
            }

            var start = _pos;
            while (_pos < _text.Length && char.IsDigit(_text[_pos]))
            {
                _pos++;
            }

            return new Token(TokenType.IntegerConstant, _text[start.._pos], 0, 0);
        }

        private Token? TryReadIdentifierOrKeyword()
        {
            if (!(char.IsLetter(_text[_pos]) || _text[_pos] == '_'))
            {
                return null;
            }

            var start = _pos;
            while (_pos < _text.Length &&
                   (char.IsLetterOrDigit(_text[_pos]) || _text[_pos] == '_'))
            {
                _pos++;
            }

            var value = _text[start.._pos];

            var type = Keywords.Contains(value)
                           ? TokenType.Keyword
                           : TokenType.Identifier;

            return new Token(type, value, 0, 0);
        }

        private Token? TryReadSymbol()
        {
            if (!Symbols.Contains(_text[_pos]))
            {
                return null;
            }

            return new Token(TokenType.Symbol, _text[_pos++].ToString(), 0, 0);
        }

        private bool StartsWith(string s)
        {
            if (_pos + s.Length > _text.Length)
            {
                return false;
            }

            for (var i = 0; i < s.Length; i++)
            {
                if (_text[_pos + i] != s[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Откатывает токенайзер на один токен назад.
        /// Если token - null, то игнорирует его и никуда не возвращает.
        /// Поддержка null нужна для удобства, чтобы использовать TryReadNext, вместе с PushBack без лишних if-ов.
        /// </summary>
        public void PushBack(Token? token)
        {
            if (token != null)
            {
                _tokens.Push(token);
            }
        }
    }
}