using System.Collections.Generic;

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
            foreach (var instruction in instructionsWithLabels)
            {
                if (instruction[0] == '(' && instruction.Length > 2) // Инструкция начинается с "(" и не пустые скобки
                {
                }
            }
            instructionsWithoutLabels = new string[instructionsWithLabels.Length];
            return table;
        }
    }
}