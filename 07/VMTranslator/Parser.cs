using System;
using System.Collections.Generic;

namespace VMTranslator;

public class Parser
{
    /// <summary>
    /// Читает список строк, пропускает строки, не являющиеся инструкциями,
    /// и возвращает массив инструкций
    /// </summary>
    public VmInstruction[] Parse(string[] vmLines)
    {
        var instructions = new List<VmInstruction>();
        var lineNumber = 1;
        foreach (var line in vmLines)
        {
            var trimmedLine = line.Trim();
            if (trimmedLine.StartsWith("//"))
            {
                lineNumber++;
                continue;
            }

            var splitted = trimmedLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (splitted.Length == 0)
            {
                lineNumber++;
                continue;
            }

            var instructionEnd = splitted.Length;
            for (var i = 0; i < splitted.Length; i++)
            {
                if (splitted[i].StartsWith("//"))
                {
                    instructionEnd = i;
                    break;
                }

                if (splitted[i].Contains("//"))
                {
                    splitted[i] = splitted[i].Split("//")[0];
                    instructionEnd = i + 1;
                    break;
                }
            }

            var instruction = new VmInstruction(lineNumber++, splitted[0], splitted[1..instructionEnd]);
            instructions.Add(instruction);
        }

        return instructions.ToArray();
    }
}