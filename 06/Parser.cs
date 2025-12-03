namespace Assembler
{
    public class Parser
    {
        /// <summary>
        /// Удаляет все комментарии и пустые строки из программы. Удаляет все пробелы из команд.
        /// </summary>
        /// <param name="asmLines">Строки ассемблерного кода</param>
        /// <returns>Только значащие строки строки ассемблерного кода без комментариев и лишних пробелов</returns>
        public string[] RemoveWhitespacesAndComments(string[] asmLines)
        {
            return asmLines
                   .Select(line => line
                                   .Split('/')        // Делим строчку по '/'
                                   .First()           // Берем все что до /
                                   .Replace(" ", "")) // Убираем лишние пробелы
                   .Where(line => line.Length > 0)    // Убираем пустые строчки
                   .ToArray();
        }
    }
}