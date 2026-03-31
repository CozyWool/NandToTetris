using System;
using System.Collections.Generic;
using System.Linq;

namespace JackCompiling
{
    public class Tokenizer
    {
        public Tokenizer(string text)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Откатывает токенайзер на один токен назад.
        /// Если token - null, то игнорирует его и никуда не возвращает.
        /// Поддержка null нужна для удобства, чтобы использовать TryReadNext, вместе с PushBack без лишних if-ов.
        /// </summary>
        public void PushBack(Token? token)
        {
            throw new NotImplementedException();
        }
    }
}
