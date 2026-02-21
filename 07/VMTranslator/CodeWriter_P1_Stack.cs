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
                WritePushFromSegment(baseAddress, index);

                return true;
            case "pop":
                (index, baseAddress) = GetBaseAddress(instruction, moduleName);
                WritePopToSegment(baseAddress, index);

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
                     "D=M");
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

    private void WritePushFromSegment(string baseAddress, string index)
    {
        WriteLoadD(baseAddress, index);
        WritePushD();
    }

    // Генерирует код, для извлечения из стека значения в D регистр
    private void WritePopToD()
    {
        WriteAsm("@SP",
                 "AM=M-1", // Делаем SP-- и заодно адресуемся на SP-1
                 "D=M");
    }

    private void WritePopToSegment(string baseAddress, string index)
    {
        // Вычисляем адрес назначения
        WriteComputeAddressToR13(baseAddress, index);

        // Читаем значение со стека
        WritePopToD();

        // Адресуемся на нужную ячейку
        WriteDestinationAddress(baseAddress, index);

        WriteAsm("M=D"); // Записываем в ячейку значение из D
    }

    // Вычисляем адрес назначения
    private void WriteComputeAddressToR13(string baseAddress, string index)
    {
        if (baseAddress is "LCL" or "ARG" or "THIS" or "THAT") // Остальные случаи можно подсчитать заранее (не в Hack)
        {
            WriteAsm($"@{index}",
                     "D=A",
                     $"@{baseAddress}",
                     "D=D+M",
                     "@R13",
                     "M=D");
        }
    }

    private void WriteDestinationAddress(string baseAddress, string index)
    {
        // Адресуемся на нужную ячейку
        if (baseAddress is "LCL" or "ARG" or "THIS" or "THAT")
        {
            // Записываем в вычисленный адрес
            WriteAsm("@R13",
                     "A=M");
        }
        else
        {
            var address = baseAddress switch
                          {
                              "pointer" => (int.Parse(index) + 3).ToString(),
                              "temp"    => (int.Parse(index) + 5).ToString(),
                              _         => baseAddress
                          };

            WriteAsm($"@{address}");
        }
    }
}