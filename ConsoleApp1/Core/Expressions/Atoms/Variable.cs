using ConsoleApp1.Core.Models;

namespace ConsoleApp1.Core.Expressions.Atoms;

public class Variable : Atom
{
    public static Dictionary<string, VariableData> Variables = new();
    
    public readonly string Name;
    public VariableData Data => Variables[Name];

    public Variable(string name)
    {
        Name = name;
        
        if (!Variables.ContainsKey(name))
        {
            Variables[name] = VariableData.Default(name);
        }
    }
    public Variable(VariableData data)
    {
        Name = data.Name;
        Variables[Name] = data;
    }
    public Variable(string name, VariableData data) : this(data) { }
    public Variable(string name, Expr value) : this(name, new ExprVar(name, value)) { }
    public Variable(string name, double value) : this(name, new ScalarVar(name, value)) { }
    public Variable(string name, Fonction fonction, Expr of) : this(name, new FunctionVar(name, fonction, of)) { }

    public override Expr Derivee(string variable)
    {
        return Name == variable ? Un : Zero;
    }
    
    public override double N()
    {
        return Data.N();
    }

    // TODO
    public override Expr Inverse(Expr y, int argIndex)
    {
        throw new NotImplementedException();
    }

    public override object[] GetArgs()
    {
        return new object[] { Name };
    }

    public override int CompareSelf(Atom expr)
    {
        return string.Compare(Name, ((Variable)expr).Name, StringComparison.Ordinal);
    }

    // <-- Display -->

    public override string ToLatex()
    {
        return Name;
    }


    public override string ToString()
    {
        return Name;
    }
}

public abstract class VariableData(string name)
{
    // public Set? NumberDomain;
    public string Name = name;

    public static VariableData Default(string name) => new ExprVar(name);

    public virtual double N()
    {
        throw new NotImplementedException();
    }
}

public class ScalarVar(string name, double? value = null) : VariableData(name)
{
    public double? Value = value;

    public override double N()
    {
        return Value ?? throw new Exception("Canot convert a variable to a number");
    }
}

public class ExprVar(string name, Expr? value = null) : VariableData(name)
{
    public Expr? Value = value;

    public override double N()
    {
        return Value?.N() ?? throw new Exception("Canot convert a variable to a number");
    }
}

public class VectorVar : VariableData
{
    public int? Dim;
    public Expr[]? Value;

    public VectorVar(string name, int? dim = null) : base(name)
    {
        Dim = dim;
        Value = null;
    }

    public VectorVar(string name, params Expr[] value) : base(name)
    {
        Dim = value.Length;
        Value = value;
    }

    public override double N()
    {
        throw new Exception("Canot convert a variable to a number");
    }
}

public class MatrixVar : VariableData
{
    public (int, int)? Shape;
    public Expr[,]? Value;

    public MatrixVar(string name, (int, int)? shape = null) : base(name)
    {
        Shape = shape;
        Value = null;
    }

    public MatrixVar(string name, Expr[,] value) : base(name)
    {
        var rows = value.GetLength(0);
        var cols = value.GetLength(1);
        Shape = (rows, cols);
        Value = value;
    }

    public override double N()
    {
        throw new Exception("Canot convert a variable to a number");
    }
}

public class FunctionVar : VariableData
{
    public Fonction? Func;
    public Expr Of;

    public FunctionVar(string name, Expr of) : base(name)
    {
        Func = null;
        Of = of;
    }

    public FunctionVar(string name, Fonction func, Expr of) : base(name)
    {
        Func = func;
        Of = of;
    }

    public FunctionVar(string name, Fonction func) : base(name)
    {
        Func = func;
        Of = new Variable(func.NameVariable);
    }

    public override double N()
    {
        return Func?.N(Of.N()) ?? throw new Exception("Canot convert a variable to a number");
    }
}

