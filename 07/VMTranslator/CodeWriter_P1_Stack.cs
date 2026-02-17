using System;
using System.Collections.Generic;

namespace VMTranslator;

public partial class CodeWriter
{
    /// <summary>
    /// Транслирует инструкции:
    /// * push [segment] [index] — записывает на стек значение взятое из ячейки [index] сегмента [segment].
    /// * pop [segment] [index] — снимает со стека значение и записывает его в ячейку [index] сегмента [segment].
    ///
    /// Сегменты:
    /// * constant — виртуальный сегмент, по индексу [index] содержит значение [index]
    /// * local — начинается в памяти по адресу Ram[LCL]
    /// * argument — начинается в памяти по адресу Ram[ARG]
    /// * this — начинается в памяти по адресу Ram[THIS]
    /// * that — начинается в памяти по адресу Ram[THAT]
    /// * pointer - по индексу 0, содержит значение Ram[THIS], а по индексу 1 — значение Ram[THAT] 
    /// * temp - начинается в памяти по адресу 5
    /// * static — хранит значения по адресу, который ассемблер выделит переменной @{moduleName}.{index}
    /// </summary>
    /// <returns>
    /// true − если это инструкция работы со стеком, иначе — false.
    /// Если метод возвращает false, он не должен менять ResultAsmCode
    /// </returns>
    private bool TryWriteStackCode(VmInstruction instruction, string moduleName)
    {
        string index, baseAddress;
        switch (instruction.Name)
        {
            case "push":
                (index, baseAddress) = GetBaseAddress(instruction, moduleName);

                WriteLoadD(baseAddress, index);
                WritePushD();

                return true;
            case "pop":
                (index, baseAddress) = GetBaseAddress(instruction, moduleName);

                WritePopToD();
                WriteStoreD(baseAddress, index);

                return true;
            default:
                return false;
        }
    }

    private (string index, string baseAddress) GetBaseAddress(VmInstruction instruction, string moduleName)
    {
        var segment = instruction.Args[0];
        var index = instruction.Args[1];
        var baseAddress = segment switch
                          {
                              "local"    => "LCL",
                              "argument" => "ARG",
                              "this"     => "THIS",
                              "that"     => "THAT",
                              "static"   => $"{moduleName}.{index}",
                              _          => segment
                          };
        return (index, baseAddress);
    }

    // Генерирует код, для сохранения значения D регистра в стек
    private void WritePushD()
    {
        WriteAsm("@SP",
                 "A=M",
                 "M=D",
                 "@SP",
                 "M=M+1");
    }

    private void WriteLoadD(string baseAddress, string index)
    {
        if (baseAddress is "pointer" or "temp" || baseAddress.Contains('.'))
        {
            var address = baseAddress switch
                          {
                              "pointer" => (int.Parse(index) + 3).ToString(),
                              "temp"    => (int.Parse(index) + 5).ToString(),
                              _         => baseAddress
                          };

            WriteAsm($"@{address}",
                     "M=D");
        }
        else
        {
            WriteAsm($"@{index}",
                     "D=A");
            if (baseAddress == "constant") // В D уже и так находится index, дальше вычислять не нужно
            {
                return;
            }

            WriteAsm($"@{baseAddress}",
                     "A=D+M", // Вычисляем адрес
                     "D=M");
        }
    }

    // Генерирует код, для извлечения из стека значения в D регистр
    private void WritePopToD()
    {
        WriteAsm("@SP",
                 "AM=M-1", // Делаем SP-- и заодно адресуемся на SP-1
                 "D=M");
    }

    private void WriteStoreD(string baseAddress, string index)
    {
        if (baseAddress is "LCL" or "ARG" or "THIS" or "THAT")
        {
            WriteAsm("@R14",
                     "M=D", // Сохраняем значение из D
                     $"@{index}",
                     "D=A",
                     $"@{baseAddress}",
                     "D=D+M", // Вычисленный адрес назначения
                     "@R13",
                     "M=D",
                     "@R14",
                     "D=M",
                     "@R13",
                     "A=M",
                     "M=D");
        }
        else
        {
            var address = baseAddress switch
                          {
                              "pointer" => (int.Parse(index) + 3).ToString(),
                              "temp"    => (int.Parse(index) + 5).ToString(),
                              _         => baseAddress
                          };

            WriteAsm($"@{address}",
                     "M=D");
        }
    }
}