using ConsoleApp1.Core.Models;
using ConsoleApp1.Core.Sets;
using ConsoleApp1.Latex;

namespace ConsoleApp1.Core.Expressions.Atoms;

public class Variable : Atom
{
    public static Dictionary<string, VariableData> Variables = new();

    public readonly string Name;
    public VariableData? _data = null;
    public VariableData Data
    {
        get {
            if (_data == null)
            {
                _data = Variables[Name];
            }
            return _data;
        }
    }

    public static bool SetValue(string variable, double value)
    {
        var data = Variables[variable];
        if (data is ConstantVar cd)
        {
            cd.Value = value;
            return true;
        } else if (data is ScalarVar sd)
        {
            sd.Value = value;
            return true;
        }
        
        return false;
    }
    
    public override Expr Eval(Expr[] exprs, object[]? objects = null)
    {
        var value = objects?[0];
        return value is not null ? new Variable((string)value) : throw new Exception();
    }
    public override Expr NotEval(Expr[] exprs, object[]? objects = null) => Eval(exprs, objects);

    
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
    public Variable(string name, Expr value) : this(new ExprVar(name, value)) { }
    public Variable(string name, double value) : this(new ScalarVar(name, value)) { }
    public Variable(string name, Fonction fonction, Expr of) : this(new FunctionVar(name, fonction, of)) { }
    
    public static Variable CreateConstant(string name, double value) => new(new ConstantVar(name, value));
    
    public override Expr Derivee(string variable)
    {
        return Name == variable ? Un : Zero;
    }
    
    public override double N()
    {
        return Data.N();
    }
    
    public override Expr Reciprocal(Expr y, int argIndex)
    {
        throw new Exception("Can not take the reciprocal of a variable");
    }

    public override object[] GetArgs()
    {
        return new object[] { Name };
    }

    public override int CompareSelf(Atom expr)
    {
        return string.Compare(Name, ((Variable)expr).Name, StringComparison.Ordinal);
    }
    
    // <-- VariableData Functions -->
    
    public void AddDependency(string name)
    {
        Data.AddDependency(name);
    }
    
    public void RemoveDependency(string name)
    {
        Data.RemoveDependency(name);
    }
    
    public void RemoveDependencies()
    {
        Data.RemoveDependencies();
    }
    
    public bool IsDependentOf(string name, bool deep=false)
    {
        return Data.IsDependentOf(name, deep);
    }

    // <-- Display -->

    public override string ToLatex()
    {
        return Data is FunctionVar f ? LatexUtils.Fonction(Name, f.Of.ToLatex()) : Name;
    }
    
    public override string ToString()
    {
        return Data is FunctionVar f ? $"{Name}({f.Of})" : Name;
    }
}

public abstract class VariableData(string name)
{
    public Set? Domain = null;
    public string Name = name;
    public List<string> Dependencies = new();

    public static VariableData Default(string name) => new ExprVar(name);

    public void AddDependency(string name)
    {
        if (!Dependencies.Contains(name))
        {
            Dependencies.Add(name);
        }
    }
    
    public void RemoveDependency(string name)
    {
        if (Dependencies.Contains(name))
        {
            Dependencies.Remove(name);
        }
    }
    
    public void RemoveDependencies()
    {
        Dependencies.Clear();
    }
    
    public bool IsDependentOf(string name, bool deep=false)
    {
        if (deep)
        {
            return Dependencies.Any(dep => dep == name || Variable.Variables[dep].IsDependentOf(name, true));
        }
        
        return Dependencies.Contains(name);
    }
    
    
    public virtual double N()
    {
        throw new NotImplementedException();
    }
}

/* Constant (represented as Variable) */
public class ConstantVar(string name, double value) : VariableData(name)
{
    public double Value = value;

    public override double N()
    {
        return Value;
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

