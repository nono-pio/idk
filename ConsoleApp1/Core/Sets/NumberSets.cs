using ConsoleApp1.Latex;
using Boolean = ConsoleApp1.Core.Booleans.Boolean;

namespace ConsoleApp1.Core.Sets;

// For sets like Natural, Integer, Rational, Real, ...
// Where N ⊆ Z ⊆ Q ⊆ R ...
public abstract class NumberSet : Set
{
    // The level of the set in the hierarchy (Natural = 1, Integer = 2, Rational = 3, Real = 4)
    public abstract int _Level { get; }
    
    public override bool IsElementsNatural => _Level <= Natural.Level;
    public override bool IsElementsInteger => _Level <= Integer.Level;
    public override bool IsElementsRational => _Level <= Rational.Level;
    public override bool IsElementsReal => _Level <= Real.Level;
    
    public override bool IsElementsPositive => _Level == Natural.Level;
    public override bool IsElementsNegative => false;
    
    public static NumberSet GetUnionOf(NumberSet a, NumberSet b)
    {
        return a._Level > b._Level ? a : b;
    } 
    
    public static NumberSet GetIntersectionOf(NumberSet a, NumberSet b)
    {
        return a._Level < b._Level ? a : b;
    }

    public override Expr? Infimum()
    {
        return _Level == Natural.Level ? 0 : Expr.NegInf;
    }

    public override Expr? Supremum()
    {   
        return Expr.Inf;
    }
}

public class Natural : NumberSet
{
    public static readonly int Level = 1;
    public override int _Level => Level;
    
    public override Boolean? Contains(Expr x)
    {
        return x.IsNatural;
    }

    public override string ToString()
    {
        return "N";
    }

    public override string ToLatex()
    {
        return Symbols.NaturalNumbers;
    }
}

public class Integer : NumberSet
{    
    public static readonly int Level = 2;
    public override int _Level => Level;

    public override Boolean? Contains(Expr x)
    {
        return x.IsInteger;
    }

    public override string ToString()
    {
        return "Z";
    }
    public override string ToLatex()
    {
        return Symbols.Integers;
    }
}

public class Rational : NumberSet
{
    public static readonly int Level = 3;
    public override int _Level => Level;

    public override Boolean? Contains(Expr x)
    {
        return x.IsRational;
    }
    
    public override string ToString()
    {
        return "Q";
    }
    
    public override string ToLatex()
    {
        return Symbols.RationalNumbers;
    }
}

public class Real : NumberSet
{
    public static readonly int Level = 4;
    public override int _Level => Level;

    public override Boolean? Contains(Expr x)
    {
        return x.IsReal;
    }
    
    public override string ToString()
    {
        return "R";
    }
    
    public override string ToLatex()
    {
        return Symbols.RealNumbers;
    }
}