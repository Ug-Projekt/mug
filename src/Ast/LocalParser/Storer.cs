using System;
partial class LocalParser : Parser {
    void StoreFunctionCalling() {
        Console.WriteLine(Objects["func"]);
        Console.WriteLine(string.Join(" ", Objects["params"]));
    }
}