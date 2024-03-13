using System.Globalization;

namespace ConsoleApp1.Core.Expressions.Atoms;

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

    public static int Gcd(int a, int b) => NumberUtils.Gcd(a, b);
    
    public bool IsEntier()
    {
        return Equal(Num, (int)Num);
    }
    
    public override double N()
    {
        return Num;
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

    public override (double, Expr?) AsMulCoef()
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
        return Num.ToString(CultureInfo.InvariantCulture);
    }
}