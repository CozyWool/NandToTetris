using System.Collections.Generic;
using System.Linq;
using System;

namespace Assembler
{
    public class Preprocessor
    {
        /// <summary>
        /// Преобразует нестандартные макро-инструкции в инструкции обычного языка ассемблера.
        /// </summary>
        public string[] PreprocessAsm(string[] instructions)
        {
            var asmCode = new List<string>();
            for (int i = 0; i < instructions.Length; i++)
            {
                var instr = instructions[i];
                try
                {
                    TranslateInstruction(instr, asmCode);
                }
                catch (Exception e)
                {
                    throw new FormatException($"Can't parse at line {i + 1}: {instr}", e);
                }
            }

            return asmCode.ToArray();
        }

        public void TranslateInstruction(string instruction, List<string> asmCode)
        {
            //TODO: ...
            asmCode.Add(instruction);
        }
    }
}
