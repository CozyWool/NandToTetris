using System.Text;

namespace Assembler
{
    public class HackTranslator
    {
        private const int MaxAddress = 32767;
        private const int FirstVariableAddress = 16;

        private int _nextVariableAddress = FirstVariableAddress;

        private static readonly Dictionary<string, string> JumpCodes = new()
                                                                       {
                                                                           ["JGT"] = "001",
                                                                           ["JEQ"] = "010",
                                                                           ["JGE"] = "011",
                                                                           ["JLT"] = "100",
                                                                           ["JNE"] = "101",
                                                                           ["JLE"] = "110",
                                                                           ["JMP"] = "111"
                                                                       };

        private static readonly Dictionary<string, string> ComputeCodes = new()
                                                                          {
                                                                              // a = 0
                                                                              ["0"] = "0101010",
                                                                              ["1"] = "0111111",
                                                                              ["-1"] = "0111010",
                                                                              ["D"] = "0001100",
                                                                              ["A"] = "0110000",
                                                                              ["!D"] = "0001101",
                                                                              ["!A"] = "0110001",
                                                                              ["-D"] = "0001111",
                                                                              ["-A"] = "0110011",
                                                                              ["D+1"] = "0011111",
                                                                              ["A+1"] = "0110111",
                                                                              ["D-1"] = "0001110",
                                                                              ["A-1"] = "0110010",

                                                                              ["D+A"] = "0000010",
                                                                              ["A+D"] = "0000010",

                                                                              ["D-A"] = "0010011",
                                                                              ["A-D"] = "0000111",

                                                                              ["D&A"] = "0000000",
                                                                              ["A&D"] = "0000000",

                                                                              ["D|A"] = "0010101",
                                                                              ["A|D"] = "0010101",


                                                                              // a = 1
                                                                              ["M"] = "1110000",
                                                                              ["!M"] = "1110001",
                                                                              ["-M"] = "1110011",
                                                                              ["M+1"] = "1110111",
                                                                              ["M-1"] = "1110010",

                                                                              ["D+M"] = "1000010",
                                                                              ["M+D"] = "1000010",

                                                                              ["D-M"] = "1010011",
                                                                              ["M-D"] = "1000111",

                                                                              ["M&D"] = "1000000",
                                                                              ["D&M"] = "1000000",

                                                                              ["D|M"] = "1010101",
                                                                              ["M|D"] = "1010101",
                                                                          };

        /// <summary>
        /// Транслирует инструкции ассемблерного кода (без меток) в бинарное представление.
        /// </summary>
        /// <param name="instructions">Ассемблерный код без меток</param>
        /// <param name="symbolTable">Таблица символов</param>
        /// <returns>Строки инструкций в бинарном формате</returns>
        /// <exception cref="FormatException">Ошибка трансляции</exception>
        public string[] TranslateAsmToHack(string[] instructions, Dictionary<string, int> symbolTable)
        {
            return instructions
                   .Select(instruction => InstructionToCode(instruction, symbolTable))
                   .ToArray();
        }

        private string InstructionToCode(string instruction, Dictionary<string, int> symbolTable)
        {
            return instruction.StartsWith("@")
                       ? AInstructionToCode(instruction, symbolTable)
                       : CInstructionToCode(instruction);
        }

        /// <summary>
        /// Транслирует одну A-инструкцию ассемблерного кода в бинарное представление
        /// </summary>
        /// <param name="aInstruction">Ассемблерная A-инструкция, например, @42 или @SCREEN</param>
        /// <param name="symbolTable">Таблица символов</param>
        /// <returns>Строка, содержащее нули и единицы — бинарное представление ассемблерной инструкции, например, "0000000000000101"</returns>
        public string AInstructionToCode(string aInstruction, Dictionary<string, int> symbolTable)
        {
            var symbol = aInstruction[1..];

            var parsedAddress = ParseAddress(symbol, symbolTable);

            var instruction = Convert.ToString(parsedAddress, 2).PadLeft(16, '0');
            return instruction;
        }

        private int ParseAddress(string symbol, Dictionary<string, int> symbolTable)
        {
            int parsedAddress;
            if (symbolTable.TryGetValue(symbol, out var knownAddress)) // Метка или уже заданная переменная
            {
                parsedAddress = knownAddress;
            }
            else if (int.TryParse(symbol, out var numericAddress) // Прямой адрес регистра
                     && numericAddress is >= 0 and <= MaxAddress)
            {
                parsedAddress = numericAddress;
            }
            else // Переменная
            {
                symbolTable[symbol] = _nextVariableAddress;
                parsedAddress = _nextVariableAddress++;
            }

            return parsedAddress;
        }

        /// <summary>
        /// Транслирует одну C-инструкцию ассемблерного кода в бинарное представление
        /// </summary>
        /// <param name="cInstruction">Ассемблерная C-инструкция, например, A=D+M</param>
        /// <returns>Строка, содержащее нули и единицы — бинарное представление ассемблерной инструкции,
        /// например, "1111000010100000"</returns>
        public string CInstructionToCode(string cInstruction)
        {
            var equalsIndex = cInstruction.IndexOf('=');
            var semiColonIndex = cInstruction.IndexOf(';');

            var destinationBinary = GetDestinationBinary(cInstruction, equalsIndex);
            var computeBinary = GetComputeBinary(cInstruction, equalsIndex, semiColonIndex);
            var jumpBinary = GetJumpBinary(cInstruction);

            return $"111{computeBinary}{destinationBinary}{jumpBinary}";
        }

        private static string GetJumpBinary(string cInstruction)
        {
            return cInstruction.Contains(';') && JumpCodes.TryGetValue(cInstruction[^3..], out var jumpCode)
                       ? jumpCode
                       : "000";
        }

        private static string GetComputeBinary(string cInstruction, int equalsIndex, int semiColonIndex)
        {
            var computeStart = equalsIndex == -1 ? 0 : equalsIndex + 1;
            var computeEnd = semiColonIndex == -1 ? cInstruction.Length : semiColonIndex;
            var computeBinary = ComputeCodes[cInstruction[computeStart..computeEnd]];
            return computeBinary;
        }

        private static StringBuilder GetDestinationBinary(string cInstruction, int equalsIndex)
        {
            var destinationBinary = new StringBuilder("000");
            if (equalsIndex == -1)
            {
                return destinationBinary;
            }

            for (var i = 0; i < equalsIndex; i++)
            {
                var index = cInstruction[i] switch
                            {
                                'A' => 3,
                                'D' => 2,
                                'M' => 1,
                                _   => 0
                            };

                destinationBinary[^index] = '1';
            }

            return destinationBinary;
        }
    }
}