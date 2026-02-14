using System;

namespace VMTranslator;

public partial class CodeWriter
{
    /// <summary>
    /// Вставляет вызов функции Sys.init без аргументов
    /// </summary>
    public void WriteSysInitCall()
    {
        TryWriteFunctionCallCode(new VmInstruction(0, "call", "Sys.init", "0"));
    }

    /// <summary>
    /// Транслирует инструкции: call, function, return
    /// </summary>
    private bool TryWriteFunctionCallCode(VmInstruction instruction)
    {
        return false;
    }
}
