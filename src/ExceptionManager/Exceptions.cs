using System;
using System.Collections.Generic;

class Errors {
    public int Count => Error.Count;
    public bool Contains(string error, string reason, string tryTo, short lineIndex, short? charIndex) {
        for (int i = 0; i < Count; i++)
            if (Error[i] == error &&
                Reason[i] == reason &&
                TryTo[i] == tryTo &&
                LineIndex[i] == lineIndex &&
                CharIndex[i] == charIndex)
                return true;
        return false;
    }
    public void Add(string error, string reason, string tryTo, short lineIndex, short? charIndex) {
        Error.Add(error);
        Reason.Add(reason);
        TryTo.Add(tryTo);
        LineIndex.Add(lineIndex);
        CharIndex.Add(charIndex);
    }
    public void Clear() {
        Error.Clear();
        Reason.Clear();
        TryTo.Clear();
        LineIndex.Clear();
        CharIndex.Clear();
    }
    List<string> Error = new List<string>();
    List<string> Reason = new List<string>();
    List<string> TryTo = new List<string>();
    List<short> LineIndex = new List<short>();
    List<short?> CharIndex = new List<short?>();
    public Tuple<string, string, string, short, short?> this[int index] => new Tuple<string, string, string, short, short?>(Error[index], Reason[index], TryTo[index], LineIndex[index], CharIndex[index]);
}