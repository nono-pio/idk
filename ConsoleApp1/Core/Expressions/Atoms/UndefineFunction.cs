namespace ConsoleApp1.Core.Expressions.Atoms;

public class UndefineFunction : Variable
{

    public readonly Expr[] Of;
    
    public UndefineFunction(string name, Expr[] of, string? latexName = null) : base(name, latexName: latexName)
    {
        Of = of;
    }
    
    public override object[] GetArgs()
    {
        return new object[] { Name, Of, LatexName };
    }
    public override Expr Eval(Expr[] exprs, object[]? objects = null)
    {
        var name = (string) objects[0];
        var of = (Expr[]) objects[1];
        var latexName = (string?) objects[2];
        
        return new UndefineFunction(name, of, latexName);
    }
    public override Expr NotEval(Expr[] exprs, object[]? objects = null) => Eval(exprs, objects);
    public override double N() => double.NaN;
}