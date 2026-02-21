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
        var isArithmetic = instruction.Name is "add" or "sub" or "neg";
        var isLogic = instruction.Name is "eq" or "gt" or "lt";
        var isBitwise = instruction.Name is "and" or "or" or "not";

        if (!isLogic && !isArithmetic && !isBitwise)
        {
            return false;
        }

        if (isArithmetic)
        {
            WriteArithmeticInstructions(instruction);
        }

        if (isLogic)
        {
            WriteLogicInstructions(instruction);
        }

        if (isBitwise)
        {
            WriteBitwiseInstructions(instruction);
        }

        WriteIncrementStackPointer();
        return true;
    }


    private void WriteIncrementStackPointer()
    {
        WriteAsm("@SP",
                 "M=M+1");
    }

    private void WriteTrueSection(string ifLabel)
    {
        WriteAsm($"({ifLabel})",
                 "@SP",
                 "A=M",
                 "M=-1");
    }

    private void WriteFalseSection(string endIfLabel)
    {
        WriteAsm("@SP",
                 "A=M",
                 "M=0");
        WriteAsm($"@{endIfLabel}",
                 "0;JMP");
    }

    private void WriteIfSection(string ifLabel, string endIfLabel, string logicCondition)
    {
        WriteAsm($"@{ifLabel}",
                 $"{logicCondition}");
        WriteFalseSection(endIfLabel);
        WriteTrueSection(ifLabel);
        WriteAsm($"({endIfLabel})");
    }

    private void WriteArithmeticInstructions(VmInstruction instruction)
    {
        var arithmeticInstruction = instruction.Name switch
                                    {
                                        "add" => "M=D+M",
                                        "sub" => "M=M-D",
                                        "neg" => "M=-M",
                                    };
        if (instruction.Name is "add" or "sub") // Бинарная операция, нужно два операнда
        {
            WritePopToD();
        }

        WriteAsm("@SP",
                 "AM=M-1",
                 $"{arithmeticInstruction}");
    }

    private void WriteLogicInstructions(VmInstruction instruction)
    {
        var ifLabel = $"IF_{instruction.LineNumber}";
        var endIfLabel = $"ENDIF_{instruction.LineNumber}";
        var logicCondition = instruction.Name switch
                             {
                                 "eq" => "D;JEQ",
                                 "gt" => "D;JGT",
                                 "lt" => "D;JLT",
                             };
        WritePopToD();
        WriteAsm("@SP",
                 "AM=M-1",
                 "D=M-D");

        WriteIfSection(ifLabel, endIfLabel, logicCondition);
    }

    private void WriteBitwiseInstructions(VmInstruction instruction)
    {
        var bitwiseInstruction = instruction.Name switch
                                 {
                                     "and" => "M=D&M",
                                     "or"  => "M=D|M",
                                     "not" => "M=!M",
                                 };
        if (instruction.Name is "and" or "or") // Бинарная операция, нужно два операнда
        {
            WritePopToD();
        }

        WriteAsm("@SP",
                 "AM=M-1",
                 $"{bitwiseInstruction}");
    }
}