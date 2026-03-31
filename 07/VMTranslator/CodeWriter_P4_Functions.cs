using System;

namespace VMTranslator;

public partial class CodeWriter
{
    private int _callCount = 0;

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
        switch (instruction.Name)
        {
            case "call":
                WriteCall(instruction);
                return true;
            case "function":
                WriteFunction(instruction);
                return true;
            case "return":
                WriteReturn();
                return true;
            default:
                return false;
        }
    }

    private void WriteCall(VmInstruction instruction)
    {
        var returnLabel = $"ret.{_callCount++}";
        var functionName = instruction.Args[0];
        var nArgs = instruction.Args[1];

        // Создаем новый фрейм вызова
        SetupNewFrame(nArgs, returnLabel);

        // Прыгаем к функции
        WriteAsm($"@{functionName}",
                 "0;JMP");

        // Ставим метку для возврата
        WriteAsm($"({returnLabel})");
    }

    private void SetupNewFrame(string nArgs, string returnLabel)
    {
        // Записываем адрес возврата
        WriteAsm($"@{returnLabel}",
                 "D=A");
        WritePushD();

        // Сохраняем LCL, ARG, THIS и THAT
        SaveSegment("LCL");
        SaveSegment("ARG");
        SaveSegment("THIS");
        SaveSegment("THAT");

        // Перемещаем ARG и LCL
        WriteAsm("@SP",
                 "D=M",
                 "@LCL",
                 "M=D",
                 "@5", // Высчитываем новый ARG
                 "D=D-A",
                 $"@{nArgs}",
                 "D=D-A",
                 "@ARG",
                 "M=D");
    }

    private void SaveSegment(string segment)
    {
        WriteAsm($"@{segment}",
                 "D=M");
        WritePushD();
    }

    private void WriteFunction(VmInstruction instruction)
    {
        var functionName = instruction.Args[0];
        var nArgs = int.Parse(instruction.Args[1]);

        WriteAsm($"({functionName})");
        for (var i = 0; i < nArgs; i++)
        {
            WriteAsm("@0",
                     "D=A");
            WritePushD();
        }
    }

    private void WriteReturn()
    {
        // endFrame = LCL
        WriteAsm("@LCL",
                 "D=M",
                 "@R13", // endFrame
                 "M=D");

        // returnAddress = *(endFrame - 5)
        WriteAsm("@5",
                 "A=D-A",
                 "D=M",
                 "@R14", // returnAddress
                 "M=D");

        // *ARG = pop()
        WritePopToD();
        WriteAsm("@ARG",
                 "A=M",
                 "M=D");

        // SP = ARG + 1
        WriteAsm("@ARG",
                 "D=M+1",
                 "@SP",
                 "M=D");

        RestoreSegment("THAT", 1);
        RestoreSegment("THIS", 2);
        RestoreSegment("ARG", 3);
        RestoreSegment("LCL", 4);

        // goto returnAddress
        WriteAsm("@R14",
                 "A=M",
                 "0;JMP");
    }

    private void RestoreSegments()
    {
        var segments = new[] {"THAT", "THIS", "ARG", "LCL"};
        for (var i = 0; i < 4; i++)
        {
            WriteAsm("@R13",
                     "MD=M-1",
                     "A=D",
                     "D=M",
                     $"@{segments[i]}",
                     "M=D");
        }
    }

    private void SaveEndFrameToR13()
    {
        WriteAsm("@LCL",
                 "D=M",
                 "@R13", // endFrame
                 "M=D");
    }

    private void SaveReturnAddressToR14()
    {
        WriteAsm("@5",
                 "A=D-A",
                 "D=M",
                 "@R14", // returnAddress
                 "M=D");
    }
}