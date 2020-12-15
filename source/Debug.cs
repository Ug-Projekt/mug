using System;

/// <summary>
///  use it to print debug information; in release it will not work
/// </summary>
class debug
{
    public static void print(params string[] msg)
    {
#if DEBUG
        Console.WriteLine(string.Join("", msg));
#endif
    }
    public static void printif(bool condition, params string[] msg)
    {
#if DEBUG
        if (condition)
            Console.WriteLine(string.Join("", msg));
#endif
    }
    public static bool askfast(char expected, params string[] msg)
    {
#if DEBUG
        Console.Write(string.Join("", msg)+"? [y/..]: ");
        var x = Console.ReadKey().KeyChar == expected;
        Console.Write('\n');
        return x;
#endif
    }
}