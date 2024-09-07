using ConsoleApp1.Latex;
using Boolean = ConsoleApp1.Core.Booleans.Boolean;

namespace ConsoleApp1.Core.Sets;

public class UniversalSet : Set
{
    
    public override bool IsElementsNatural => false;
    public override bool IsElementsInteger => false;
    public override bool IsElementsRational => false;
    public override bool IsElementsReal => false;
    public override bool IsElementsComplex => false;

    public override bool IsElementsPositive => false;
    public override bool IsElementsNegative => false;
    public override Boolean? Contains(Expr x) => Boolean.True;

    public override string ToLatex() => Symbols.UniversalSet;
}