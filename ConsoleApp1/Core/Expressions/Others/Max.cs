namespace ConsoleApp1.Core.Expressions.Others;

public class Max : Expr
{

    public Expr[] Elements { get; }
    
    public Max(Expr[] elements)
    {
        Elements = elements;
    }

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

    public override Expr Inverse(Expr y, int argIndex)
    {
        throw new Exception("Max is not reciprocal");
    }

    public override Expr Derivee(string variable)
    {
        throw new Exception("Max is not derivable");
    }
}