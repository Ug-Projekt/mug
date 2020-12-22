using System;

/// <summary>
///  use it to print debug informations; in release it will not work
/// </summary>
class debug
{
    public static bool isDebug() {
    #if DEBUG
        return true;
    #endif
        return false;
    }

    public static void exit(params string[] msg)
    {
        Console.WriteLine(string.Join("", msg));
        Environment.Exit(0);
    }
    public static void print(params string[] msg)
    {
#if DEBUG
        Console.WriteLine(string.Join("", msg));
#endif
    }
    public static void printc(ConsoleColor color, params string[] msg)
    {
#if DEBUG
        Console.ForegroundColor = color;
        Console.WriteLine(string.Join("", msg));
        Console.ResetColor();
#endif
    }
    public static void printif(bool condition, params string[] msg)
    {
#if DEBUG
        if (condition)
            Console.WriteLine(string.Join("", msg));
#endif
    }
    public static bool askfast(params string[] msg)
    {
#if DEBUG
        Console.Write(string.Join("", msg)+"? [y/..]: ");
        var x = Console.ReadKey().KeyChar == 'y';
        Console.Write('\n');
        return x;
#endif
    }
    public static void readfast(params string[] msg)
    {
#if DEBUG
        Console.Write(string.Join("", msg));
        Console.ReadKey();
        Console.Write('\n');
#endif
    }
}