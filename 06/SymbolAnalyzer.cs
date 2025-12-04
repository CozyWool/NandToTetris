namespace Assembler
{
    public class SymbolAnalyzer
    {
        /// <summary>
        /// Находит все метки в ассемблерном коде, удаляет их из кода и вносит их адреса в таблицу символов.
        /// </summary>
        /// <param name="instructionsWithLabels">Ассемблерный код, возможно, содержащий метки</param>
        /// <param name="instructionsWithoutLabels">Ассемблерный код без меток</param>
        /// <returns>
        /// Таблица символов, содержащая все стандартные предопределенные символы (R0−R15, SCREEN, ...),
        /// а также все найденные в программе метки.
        /// </returns>
        public Dictionary<string, int> CreateSymbolsTable(string[] instructionsWithLabels,
                                                          out string[] instructionsWithoutLabels)
        {
            var table = new Dictionary<string, int>
                        {
                            ["R0"] = 0,
                            ["R1"] = 1,
                            ["R2"] = 2,
                            ["R14"] = 14,
                            ["R15"] = 15,
                            ["SP"] = 0,
                            ["LCL"] = 1,
                            ["ARG"] = 2,
                            ["THIS"] = 3,
                            ["THAT"] = 4,
                            ["SCREEN"] = 0x4000,
                            ["KBD"] = 0x6000
                        };

            // Индекс - номер инструкции в нумерации без меток
            // Значение - номер инструкции в исходной нумерации
            var indices = new List<int>();

            instructionsWithoutLabels = instructionsWithLabels
                                        .Where((instruction, lineNumber) =>
                                               {
                                                   var isNotMark = instruction[0] != '(';
                                                   if (isNotMark)
                                                   {
                                                       indices.Add(lineNumber);
                                                   }

                                                   return isNotMark;
                                               })
                                        .ToArray();

            for (var i = 0; i < instructionsWithLabels.Length; i++)
            {
                var instruction = instructionsWithLabels[i];

                // Инструкция - метка
                if (instruction[0] == '(')
                {
                    table[instruction[1..^1]] = FindInstructionAddress(indices, i);
                }
            }

            return table;
        }

        private static int FindInstructionAddress(List<int> indices, int i)
        {
            // Нужно найти первую инструкцию, идущую после метки
            // Причем эта инструкция не должна быть меткой

            // Из документации: https://learn.microsoft.com/ru-ru/dotnet/api/System.Collections.Generic.List-1.BinarySearch?view=net-6.0
            // Метод BinarySearch возвращает индекс искомого элемента (в нашем случае его никогда там не будет)
            // Если элемента в списке нет, то метод возвращает отрицательное число
            // Операцию побитового дополнения (~) можно применить к этому отрицательному числу,
            // чтобы получить индекс первого элемента, который больше значения поиска (То, что нам и нужно)
            var lineIndex = indices.BinarySearch(i);
            return lineIndex >= 0
                       ? lineIndex
                       : ~lineIndex;
        }
    }
}