using ConsoleApp1.Core.Sets;
using ConsoleApp1.Latex;

namespace ConsoleApp1.Core.Expressions.Atoms;

public class Constant : Variable
{
    public readonly double AppValue;
    
    public static Constant PI = new("pi", Math.PI, latexName: Symbols.pi);
    public static Constant E = new("e", Math.E);

    public override bool IsPositive => AppValue > 0;
    public override bool IsNegative => AppValue < 0;

    public override bool IsReal => true;


    public Constant(string name, double appValue, string? latexName = null) : base(name, latexName: latexName)
    {
        AppValue = appValue;
    }
    
    public override object[] GetArgs()
    {
        return new object[] { Name, AppValue, LatexName };
    }
    public override Expr Eval(Expr[] exprs, object[]? objects = null)
    {
        var name = (string) objects[0];
        var appValue = (double) objects[1];
        var latexName = (string?) objects[2];
        
        return new Constant(name, appValue, latexName);
    }
    public override Expr NotEval(Expr[] exprs, object[]? objects = null) => Eval(exprs, objects);

    public override Expr Derivee(Variable variable) => 0;
    public override Expr Derivee(Variable variable, int n) => 0;
    
    public override double N() => AppValue;
}