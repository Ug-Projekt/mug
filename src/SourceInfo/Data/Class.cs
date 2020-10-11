using System.Collections.Generic;

struct ClassData {
    public Data Data;
    public List<FunctionData> Functions;
    public List<VariableData> Variables;
    public List<ClassData> Classes;
    public FunctionData Costructor;
    public object[] Parameters;
}