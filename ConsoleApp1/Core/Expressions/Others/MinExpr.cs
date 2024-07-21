namespace ConsoleApp1.Core.Expressions.Others;

public class MinExpr : Expr
{

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

    public override Expr Inverse(Expr y, int argIndex)
    {
        throw new Exception("Min is not reciprocal");
    }

    public override Expr Derivee(string variable)
    {
        throw new Exception("Min is not derivable");
    }
}