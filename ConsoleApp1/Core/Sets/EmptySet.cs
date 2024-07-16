namespace ConsoleApp1.Core.Sets;
using Boolean = ConsoleApp1.Core.Booleans.Boolean;

public class EmptySet : Set
{
    public override Boolean? Contains(Expr x)
    {
        return Boolean.False;
    }
}