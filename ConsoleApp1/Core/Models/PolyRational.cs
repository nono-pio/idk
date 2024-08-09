namespace ConsoleApp1.Core.Models;

public class PolyRational
{
    public PolyOfMultiPoly Num;
    public PolyOfMultiPoly Den;
    
    public PolyRational(PolyOfMultiPoly num, PolyOfMultiPoly den)
    {
        Num = num;
        Den = den;
    }
    
    public PolyRational Normal()
    {
        var gcd = PolyOfMultiPoly.Gcd(Num, Den);
        return new PolyRational(Num / gcd, Den / gcd);
    }

}