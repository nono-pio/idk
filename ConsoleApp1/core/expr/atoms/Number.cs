using ConsoleApp1.core.expr.atoms;

namespace ConsoleApp1.core.atoms;

public class Number : Atom
{
    public static readonly double FloatPointTolerance = 1e-5;

    public static readonly Number Zero = new(0);
    public static readonly Number One = new(1);
    public static readonly Number Two = new(2);
    public static readonly Number NegOne = new(-1);


    public readonly double Num;

    public Number(double num)
    {
        Num = num;
    }

    public static uint Gcd(uint a, uint b)
    {
        if (a < b)
            (a, b) = (b, a);
        
        while (b > 0)
            (a, b) = (b, a % b);

        return a;
    }

    public bool IsEntier()
    {
        return Equal(Num, (int)Num);
    }
    
    // TODO
    public override Expr Inverse(Expr y, int argIndex)
    {
        throw new NotImplementedException();
    }

    public override Expr Derivee(string variable)
    {
        return Zero;
    }

    public override (double, Expr?) HasMulCoef()
    {
        return (Num, null);
    }

    public static bool Equal(double x, double y)
    {
        return Math.Abs(x - y) < FloatPointTolerance;
    }

    public override object[] GetArgs()
    {
        return new object[] { Num };
    }

    public override int CompareSelf(Atom expr)
    {
        return Num.CompareTo(((Number)expr).Num);
    }

    // <-- Display -->
    public override string ToString()
    {
        return ToLatex();
    }

    public override string ToLatex()
    {
        return Num.ToString();
    }
}