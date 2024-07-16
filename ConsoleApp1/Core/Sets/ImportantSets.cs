using Boolean = ConsoleApp1.Core.Booleans.Boolean;

namespace ConsoleApp1.Core.Sets;

public class Natural : Set
{
    public override Boolean? Contains(Expr x)
    {
        return x.IsNatural;
    }
}

public class Integer : Set
{
    public override Boolean? Contains(Expr x)
    {
        return x.IsInteger;
    }
}

public class Rational : Set
{
    public override Boolean? Contains(Expr x)
    {
        return x.IsRational;
    }
}

public class Real : Set
{
    public override Boolean? Contains(Expr x)
    {
        return x.IsReal;
    }
}