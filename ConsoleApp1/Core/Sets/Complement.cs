using Boolean = ConsoleApp1.Core.Booleans.Boolean;

namespace ConsoleApp1.Core.Sets;

public class Complement(Set x, Set? univers = null) : Set
{

    public Set X = x;
    public Set Univers = univers ?? new Real();

    public override Boolean Contains(Expr x)
    {
        return Boolean.And(Univers.Contains(x), !X.Contains(x));
    }
}