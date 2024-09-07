namespace ConsoleApp1.Core.Sets;
using Boolean = ConsoleApp1.Core.Booleans.Boolean;

public class EmptySet : Set
{
    public override bool IsElementsNatural => true;
    public override bool IsElementsInteger => true;
    public override bool IsElementsRational => true;
    public override bool IsElementsReal => true;
    public override bool IsElementsComplex => true;
    public override bool IsElementsPositive => true;
    public override bool IsElementsNegative => true;

    public override Boolean? Contains(Expr x)
    {
        return Boolean.False;
    }

    public override string ToString()
    {
        return "∅";
    }

    public override string ToLatex()
    {
        return @"\emptyset";
    }
}