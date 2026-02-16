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

        for (var lineNumber = 0; lineNumber < vmLines.Length; ++lineNumber)
        {
            var line = vmLines[lineNumber];

            if (TryParseInstruction(line, lineNumber + 1, out var instruction))
            {
                instructions.Add(instruction);
            }
        }

        return instructions.ToArray();
    }

    private bool TryParseInstruction(string line, int lineNumber, out VmInstruction instruction)
    {
        instruction = null;
        var lineWithoutComment = RemoveComment(line);
        var parts = lineWithoutComment.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0)
        {
            return false;
        }

        var command = parts[0];
        var arguments = parts.Length > 1 ? parts[1..] : Array.Empty<string>();

        instruction = new VmInstruction(lineNumber, command, arguments);
        return true;
    }

    private string RemoveComment(string line)
    {
        var commentIndex = line.IndexOf("//", StringComparison.Ordinal);
        return commentIndex >= 0 ? line[..commentIndex].Trim() : line.Trim();
    }
}