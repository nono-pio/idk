using ConsoleApp1.Core.Expressions.Atoms;
using Sdcb.Arithmetic.Mpfr;

namespace ConsoleApp1.Core.Expressions.Others;

public class MinExpr : Expr
{
    
    public override bool IsNatural => Elements.All(el => el.IsNatural);
    public override bool IsInteger => Elements.All(el => el.IsInteger);
    public override bool IsReal => Elements.All(el => el.IsReal);

    public override bool IsPositive => Elements.All(el => el.IsPositive);
    public override bool IsNegative => Elements.Any(el => el.IsNegative);

    public Expr[] Elements { get; }
    
    public MinExpr(Expr[] elements)
    {
        Elements = elements;
    }
    
    
    public static Expr Construct(Expr[] elements)
    {
        if (elements.Length == 1)
        {
            return elements[0];
        }
        return new MinExpr(elements);
    }

    public override Expr Eval(Expr[] exprs, object[]? objects = null) => Construct(exprs);
    public override Expr NotEval(Expr[] exprs, object[]? objects = null) => new MinExpr(exprs);

    public override string ToString()
    {
        return ToLatex();
    }

    public override string ToLatex()
    {
        return "min\\left(" + string.Join(",", Elements.Select(e => e.ToLatex())) + "\\right)";
    }

    public override double N()
    {
        return Elements.Min(e => e.N());
    }

    public override Expr Reciprocal(Expr y, int argIndex)
    {
        throw new Exception("Min is not reciprocal");
    }

    public override Expr Derivee(Variable variable)
    {
        throw new Exception("Min is not derivable");
    }
    
    public override MpfrFloat NPrec(int precision = 333, MpfrRounding rnd = MpfrRounding.ToEven)
    {
        var min = Args[0].NPrec(precision, rnd);
        foreach (var arg in Args.Skip(1))
        {
            MpfrFloat.MinInplace(min, min, arg.NPrec(precision, rnd), rnd);
        }

        return min;
    }
}