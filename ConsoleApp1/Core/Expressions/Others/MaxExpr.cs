namespace ConsoleApp1.Core.Expressions.Others;

public class MaxExpr : Expr
{

    public Expr[] Elements { get; }
    
    public MaxExpr(Expr[] elements)
    {
        Elements = elements;
    }
    
    public static Expr Construct(Expr[] elements)
    {
        if (elements.Length == 1)
        {
            return elements[0];
        }
        return new MaxExpr(elements);
    }

    public override Expr Eval(Expr[] exprs, object[]? objects = null) => Construct(exprs);
    public override Expr NotEval(Expr[] exprs, object[]? objects = null) => new MaxExpr(exprs);

    public override string ToString()
    {
        return ToLatex();
    }

    public override string ToLatex()
    {
       return "max\\left(" + string.Join(",", Elements.Select(e => e.ToLatex())) + "\\right)";
    }

    public override double N()
    {
        return Elements.Max(e => e.N());
    }

    public override Expr Reciprocal(Expr y, int argIndex)
    {
        throw new Exception("Max is not reciprocal");
    }

    public override Expr Derivee(string variable)
    {
        throw new Exception("Max is not derivable");
    }
}