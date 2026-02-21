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
        switch (instruction.Name)
        {
            case "add":
                return true;
            case "sub":
                return true;
            case "neg":
                return true;
            case "eg":
                return true;
            case "gt":
                return true;
            case "lt":
                return true;
            case "and":
                return true;
            case "or":
                return true;
            case "not":
                return true;
            default:
                return false;
        }
    }
}