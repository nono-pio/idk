using ConsoleApp1.Core.Models;

namespace ConsoleApp1.Core.Expressions.Atoms;

public class Variable : Atom
{
    public static Dictionary<string, VariableData> Variables = new();
    
    public readonly string Name;
    public VariableData Data => Variables[Name];

    public Variable(string name, VariableData? data = null)
    {
        Name = name;
        
        // set/replace data
        if (data != null)
        {
            Variables[name] = data;
        } // create data if not exists
        else if (!Variables.ContainsKey(name))
        {
            Variables[name] = VariableData.Default();
        }
    }

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

public abstract class VariableData
{
    // public Set? NumberDomain;
    
    public static VariableData Default() => new ExprVar();

    public virtual double N()
    {
        throw new NotImplementedException();
    }
}

public class ScalarVar : VariableData
{
    public double? Value;

    public ScalarVar(double? value = null)
    {
        Value = value;
    }
    
    public override double N()
    {
        return Value ?? throw new Exception("Canot convert a variable to a number");
    }
}

public class ExprVar : VariableData
{
    public Expr? Value;

    public ExprVar(Expr? value = null)
    {
        Value = value;
    }

    public override double N()
    {
        return Value?.N() ?? throw new Exception("Canot convert a variable to a number");
    }
}

public class VectorVar : VariableData
{
    public int? Dim;
    public Expr[]? Value;

    public VectorVar(int? dim = null)
    {
        Dim = dim;
        Value = null;
    }

    public VectorVar(params Expr[] value)
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

    public MatrixVar((int, int)? shape = null)
    {
        Shape = shape;
        Value = null;
    }

    public MatrixVar(Expr[,] value)
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

    public FunctionVar(Expr of)
    {
        Func = null;
        Of = of;
    }

    public FunctionVar(Fonction func, Expr of)
    {
        Func = func;
        Of = of;
    }

    public FunctionVar(Fonction func)
    {
        Func = func;
        Of = new Variable(func.NameVariable);
    }

    public override double N()
    {
        return Func?.N(Of.N()) ?? throw new Exception("Canot convert a variable to a number");
    }
}

