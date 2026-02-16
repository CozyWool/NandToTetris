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
        string segment, index, baseAddress;
        switch (instruction.Name)
        {
            case "push":
                segment = instruction.Args[0];
                index = instruction.Args[1];
                baseAddress = GetBaseAddress(segment, moduleName, index);
                return true;
            case "pop":
                segment = instruction.Args[0];
                index = instruction.Args[1];
                baseAddress = GetBaseAddress(segment, moduleName, index);
                return true;
        }

        return false;
    }

    private string GetBaseAddress(string segment, string moduleName, string index)
    {
        return segment switch
               {
                   "constant" => "SP",
                   "local"    => "LCL",
                   "argument" => "ARG",
                   "this"     => "THIS",
                   "that"     => "THAT",
                   "temp"     => "5",
                   "static"   => $"{moduleName}.{index}",
                   "pointer"  => index == "1" ? "THAT " : "THIS",
               };
    }

    // Генерирует код, для сохранения значения D регистра в стек
    private void WritePushD()
    {

    }

    // Генерирует код, для извлечения из стека значения в D регистр
    private void WritePopToD()
    {
    }
}