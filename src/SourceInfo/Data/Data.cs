using System;

struct Data {
    public string Name;
    public object Type;
    public override string ToString() {
        return "Name: "+Name+" Type: "+Type;
    }
}