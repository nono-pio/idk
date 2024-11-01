using System.Runtime.CompilerServices;
using ConsoleApp1.Core.Sets;

namespace ConsoleApp1.Core.Expressions.Atoms;


public class Variable : Atom
{
    public override bool IsNatural => Domain?.IsElementsNatural ?? false;
    public override bool IsInteger  => Domain?.IsElementsInteger ?? false;
    public override bool IsRational  => Domain?.IsElementsRational ?? false;
    public override bool IsReal  => Domain?.IsElementsReal ?? false;
    public override bool IsComplex  => Domain?.IsElementsComplex ?? false;

    public override bool IsPositive  => Domain?.IsElementsPositive ?? false;
    public override bool IsNegative  => Domain?.IsElementsNegative ?? false;

    public override Set AsSet() => ArraySet(this);

    public readonly string Name;
    public readonly string LatexName;
    public readonly bool Dummy = false;
    public Set? Domain;
    public List<Variable> Dependencies;
    public Expr? Value;
    
    public Variable(string name, string? latexName = null, bool dummy = false, Set? domain = null, List<Variable>? dependency = null, Expr? value = null)
    {
        Name = name;
        LatexName = latexName ?? name;
        Dummy = dummy;
        Domain = domain;
        Dependencies = dependency ?? new();
        Value = value;
    }
    
    public static Variable CreateDummy(string name)
    {
        return new Variable(name, dummy: true);
    }
    
    public static Variable CreateUnique()
    {
        return CreateDummy("Dummy_" + Guid.NewGuid().ToString("N"));
    }
    
    public override object[] GetArgs()
    {
        return new object[] { Name, LatexName, Dummy, Domain, Dependencies, Value };
    }
    public override Expr Eval(Expr[] exprs, object[]? objects = null)
    {
        var name = (string) objects[0];
        var latexName = (string?) objects[1];
        var dummy = (bool) objects[2];
        var domain = (Set) objects[3];
        var dependencies = (List<Variable>) objects[4];
        var value = (Expr) objects[5];
        
        return new Variable(name, latexName, dummy, domain, dependencies, value);
    }
    public override Expr NotEval(Expr[] exprs, object[]? objects = null) => Eval(exprs, objects);

    public static explicit operator Variable(string str) => new Variable(str);
    
    public override Expr Derivee(Variable variable)
    {
        return this == variable ? 1 : 0;
    }
    
    public override double N()
    {
        return Value?.N() ?? double.NaN;
    }
    
    public override Expr Reciprocal(Expr y, int argIndex)
    {
        throw new Exception("Can not take the reciprocal of a variable");
    }

    public override int CompareSelf(Atom expr)
    {
        var other = (Variable) expr;
        if (other.Dummy || Dummy)
        {
            int adr1 = RuntimeHelpers.GetHashCode(this);
            int adr2 = RuntimeHelpers.GetHashCode(other);

            return adr1.CompareTo(adr2);
        }
        
        return string.Compare(Name, other.Name, StringComparison.Ordinal);
    }
    
    public bool IsDependentOf(Variable name, bool deep=false)
    {
        return Dependencies.Any(var => var == name || (deep && var.IsDependentOf(name, deep)));
    }
    
    public static bool operator ==(Variable a, Variable b)
    {
        return a.CompareSelf(b) == 0;
    }
    public static bool operator !=(Variable a, Variable b) => !(a == b);

    // <-- Display -->

    public override string ToLatex()
    {
        return LatexName;
    }
    
    public override string ToString()
    {
        return Name;
    }
}

