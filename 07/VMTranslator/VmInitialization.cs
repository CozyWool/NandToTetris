namespace VMTranslator;

public static class VmInitialization
{
    public const int Sp = 256;
    public const int Local = 300;
    public const int Argument = 400;
    public const int This = 3000;
    public const int That = 3010;


    /// <summary>
    /// Генерирует код инициализации значения регистров SP, LCL, ARG, THIS, THAT в их начальные значения (константы выше)
    /// </summary>
    public static void WriteMemoryInitialization(this CodeWriter translator)
    {
        translator.WriteValueToRam(0, Sp);
        translator.WriteValueToRam(1, Local);
        translator.WriteValueToRam(2, Argument);
        translator.WriteValueToRam(3, This);
        translator.WriteValueToRam(4, That);
    }

    private static void WriteValueToRam(this CodeWriter translator, int index, int value)
    {
        translator.ResultAsmCode.AddRange(new[]
                                          {
                                              $"@{value}",
                                              "D=A",
                                              $"@{index}",
                                              "M=D"
                                          });
    }
}