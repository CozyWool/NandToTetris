using System;

namespace VMTranslator;

public partial class CodeWriter
{
    /// <summary>
    /// Транслирует инструкции: label, goto, if-goto
    /// </summary>
    private bool TryWriteProgramFlowCode(VmInstruction instruction, string moduleName)
    {
        switch (instruction.Name)
        {
            case "label":
                WriteLabel(instruction, moduleName);
                return true;
            case "goto":
                WriteGoto(instruction, moduleName);
                return true;
            case "if-goto":
                WriteIfGoto(instruction, moduleName);
                return true;
            default:
                return false;
        }
    }

    private void WriteLabel(VmInstruction instruction, string moduleName)
    {
        var label = $"{moduleName}.{instruction.Args[0]}";
        WriteAsm($"({label})");
    }

    private void WriteGoto(VmInstruction instruction, string moduleName)
    {
        WriteAsm($"@{moduleName}.{instruction.Args[0]}",
                 "0;JMP");
    }

    private void WriteIfGoto(VmInstruction instruction, string moduleName)
    {
        var label = $"{moduleName}.{instruction.Args[0]}";
        WritePopToD();
        WriteAsm($"@{label}",
                 "D;JNE");
    }
}