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
            // Индекс - номер инструкции в нумерации без меток
            // Значение - номер инструкции в исходной нумерации
            var indices = new List<int>();

            instructionsWithoutLabels = instructionsWithLabels
                                        .Where((instruction, lineNumber) =>
                                               {
                                                   var isNotLabel = instruction[0] != '(';
                                                   if (isNotLabel)
                                                   {
                                                       indices.Add(lineNumber);
                                                   }

                                                   return isNotLabel;
                                               })
                                        .ToArray();

            var table = GenerateTable(instructionsWithLabels, indices);

            return table;
        }

        private static int FindInstructionAddress(List<int> indices, int i)
        {
            // Нужно найти первую инструкцию, идущую после метки
            // Причем эта инструкция не должна быть меткой

            // Из документации:
            // https://learn.microsoft.com/ru-ru/dotnet/api/System.Collections.Generic.List-1.BinarySearch?view=net-6.0
            // Метод BinarySearch возвращает индекс искомого элемента (в нашем случае его никогда там не будет)
            // Если элемента в списке нет, то метод возвращает отрицательное число
            // Операцию побитового дополнения (~) можно применить к этому отрицательному числу,
            // чтобы получить индекс первого элемента, который больше значения поиска (То, что нам и нужно)
            var lineIndex = indices.BinarySearch(i);
            return lineIndex >= 0
                       ? lineIndex
                       : ~lineIndex;
        }

        private static Dictionary<string, int> GenerateTable(string[] instructionsWithLabels, List<int> indices)
        {
            var table = CreateBaseTable();
            for (var i = 0; i < instructionsWithLabels.Length; i++)
            {
                var instruction = instructionsWithLabels[i];

                var isLabel = instruction[0] == '(';
                if (isLabel)
                {
                    table[instruction[1..^1]] = FindInstructionAddress(indices, i);
                }
            }

            return table;
        }

        private static Dictionary<string, int> CreateBaseTable() =>
            new()
            {
                ["R0"] = 0,
                ["R1"] = 1,
                ["R2"] = 2,
                ["R3"] = 3,
                ["R4"] = 4,
                ["R5"] = 5,
                ["R6"] = 6,
                ["R7"] = 7,
                ["R8"] = 8,
                ["R9"] = 9,
                ["R10"] = 10,
                ["R11"] = 11,
                ["R12"] = 12,
                ["R13"] = 13,
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
    }
}