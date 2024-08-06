namespace ConsoleApp1.Core.Models;

public class MultiRational(MultiPoly num, MultiPoly den)
{
    public MultiPoly Num = num;
    public MultiPoly Den = den;

    public MultiRational Normal()
    {
        var gcd = MultiPoly.Gcd(Num, Den);
        
        return new MultiRational(Num / gcd, Den / gcd);
    }

    public static MultiRational operator +(MultiRational a, MultiRational b)
    {
        return null;
    }
    
    public static MultiRational operator -(MultiRational a, MultiRational b)
    {
        return null;
    }
    
    public static MultiRational operator *(MultiRational a, MultiRational b)
    {
        return null;
    }
    
    public static MultiRational operator *(MultiRational a, MultiPoly b)
    {
        return null;
    }
    
    public static MultiRational operator /(MultiRational a, MultiRational b)
    {
        return null;
    }
    
}