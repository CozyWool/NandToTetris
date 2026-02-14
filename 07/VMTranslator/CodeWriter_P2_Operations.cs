namespace VMTranslator;

public partial class CodeWriter
{
    /// <summary>
    /// Транслирует инструкции:
    /// * арифметических операция: add sub, neg
    /// * логических операций: eq, gt, lt, and, or, not
    /// </summary>
    /// <returns>true − если это логическая или арифметическая инструкция, иначе — false.</returns>
    private bool TryWriteLogicAndArithmeticCode(VmInstruction instruction)
    {
        return false;
    }
}
