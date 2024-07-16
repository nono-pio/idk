using Boolean = ConsoleApp1.Core.Booleans.Boolean;

namespace ConsoleApp1.Core.Sets;

public class UniversalSet : Set
{
    public override Boolean? Contains(Expr x) => Boolean.True;
}