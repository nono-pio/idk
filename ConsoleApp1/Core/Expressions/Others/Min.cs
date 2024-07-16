namespace ConsoleApp1.Core.Expressions.Others;

public class Min : Expr
{

    public Expr[] Elements { get; }
    
    public Min(Expr[] elements)
    {
        Elements = elements;
    }

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