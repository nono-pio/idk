using ConsoleApp1.Latex;
using Sdcb.Arithmetic.Mpfr;

namespace ConsoleApp1.Core.Expressions.Atoms;

public class Constant : Variable
{
    public readonly double AppValue;
    public readonly Func<int, MpfrRounding, MpfrFloat>? App;
    
    public static Constant PI = new("pi", Math.PI, latexName: Symbols.pi, appPrecision: (prec, rnd) => MpfrFloat.ConstPi(prec, rnd));
    public static Constant E = new("e", Math.E, appPrecision: (prec, rnd) => MpfrFloat.Exp(MpfrFloat.From(1), prec, rnd));

    public override bool IsPositive => AppValue > 0;
    public override bool IsNegative => AppValue < 0;

    public override bool IsReal => true;


    public Constant(string name, double appValue, string? latexName = null, Func<int, MpfrRounding, MpfrFloat>? appPrecision = null) : base(name, latexName: latexName)
    {
        AppValue = appValue;
        App = appPrecision;
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
    public override MpfrFloat NPrec(int precision = 333, MpfrRounding rnd = MpfrRounding.ToEven)
    {
        return App is not null ? App(precision, rnd) : base.NPrec(precision, rnd);
    }
}