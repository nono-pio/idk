using Boolean = ConsoleApp1.Core.Booleans.Boolean;

namespace ConsoleApp1.Core.Sets;

public class DifferenceSet : Set
{

    public Set A;
    public Set B;
    public DifferenceSet(Set a, Set b)
    {
        A = a;
        B = b;
    }
    
    public static Set Construct(Set a, Set b)
    {
        return new DifferenceSet(a, b);
    }
    
    public override Boolean Contains(Expr x)
    {
        throw new NotImplementedException();
    }

    public override string ToLatex()
    {
        throw new NotImplementedException();
    }
}