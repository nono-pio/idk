using Boolean = ConsoleApp1.Core.Booleans.Boolean;

namespace ConsoleApp1.Core.Sets;

public class Complement(Set x, Set? univers = null) : Set
{

    public Set X = x;
    public Set Univers = univers ?? new Real();
    
    public override bool IsElementsNatural => Univers.IsElementsNatural;
    public override bool IsElementsInteger => Univers.IsElementsInteger;
    public override bool IsElementsRational => Univers.IsElementsRational;
    public override bool IsElementsReal => Univers.IsElementsReal;
    public override bool IsElementsComplex => Univers.IsElementsComplex;

    public override bool IsElementsPositive => Univers.IsElementsPositive;
    public override bool IsElementsNegative => Univers.IsElementsNegative;

    public override Boolean Contains(Expr x)
    {
        return Boolean.And(Univers.Contains(x), !X.Contains(x));
    }

    public override string ToLatex()
    {
        return $"{X.ToLatex()}^C";
    }
}