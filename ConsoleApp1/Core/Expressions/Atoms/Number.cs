using System.Globalization;
using ConsoleApp1.Core.Classes;
using ConsoleApp1.Core.Expressions.Base;

namespace ConsoleApp1.Core.Expressions.Atoms;

/*

From:
- Both (default)
- Minus
- Plus
Data: NumberStruct

*/

/* Use for limit */
public enum From
{
    Both,
    Minus,
    Plus
}


public class Number : Atom
{
    public static readonly double FloatPointTolerance = 1e-5;

    //public readonly From From;
    public readonly NumberStruct Num;

    public Number(NumberStruct num, From from = From.Both)
    {
        //From = from;
        Num = num;
    }

    public static implicit operator Number(int value) => new(value);
    public static implicit operator Number(long value) => new(value);
    public static implicit operator Number(float value) => new(value);
    public static implicit operator Number(double value) => new(value);
    public static implicit operator Number(NumberStruct value) => new(value);

    public override Expr Eval(Expr[] exprs, object[]? objects = null)
    {
        var value = objects?[0];
        return value is not null ? new Number((NumberStruct)value) : throw new Exception();
    }
    public override Expr NotEval(Expr[] exprs, object[]? objects = null) => Eval(exprs, objects);


    public override bool IsZero => Num.IsZero;
    public override bool IsOne => Num.IsOne;
    
    public override bool IsPositive => Num.IsPositive;
    public override bool IsNegative => Num.IsNegative;
    
    public override bool IsInfinity => Num.IsInfinity;
    public override bool IsNegativeInfinity => Num.IsNegativeInfinity;

    public override bool IsNatural => Num.IsInt && IsPositive;
    public override bool IsInteger => Num.IsInt;
    public override bool IsRational => true;

    public override (Expr Num, Expr Den) AsFraction()
    {
        if (Num.IsFraction && Num.Denominator != 1)
            return (Num.Numerator, Num.Denominator);
        
        return base.AsFraction();
    }

    public static int Gcd(int a, int b) => NumberUtils.Gcd(a, b);

    public override double N() => Num.N();
    
    public override Expr Reciprocal(Expr y, int argIndex) =>
        throw new Exception("Cannot Inverse a Atom");

    public override Expr Derivee(string variable) => 0;

    public override (NumberStruct, Expr?) AsMulCoef()
    {
        return (Num, null);
    }

    public static Expr SimplifyPow(Number a, Number b)
    {
        if (a.Num.IsNan || b.Num.IsNan)
            return (Number) NumberStruct.Nan;

        if (a.Num.IsFloat || b.Num.IsFloat)
        {
            var pow = Math.Pow(a.Num.FloatValue, b.Num.FloatValue);
            if (double.IsInfinity(pow))
                return Power.ConstructNotEval(a, b);
            return (Number) pow;
        }

        bool isNeg = b.Num.IsNegative;
        NumberStruct n;
        if (isNeg)
            n = -b.Num;
        else
            n = b.Num;
        
        var p = a.Num.Numerator;
        var q = a.Num.Denominator;
        
        p = (long) Math.Pow(p, n.Numerator);
        q = (long) Math.Pow(q, n.Numerator);

        if (n.Denominator == 1)
            return isNeg ? Num(q, p) : Num(p, q);

        (p, var sqrt_p) = NumberStruct.Sqrt(p, b.Num.Denominator);
        (q, var sqrt_q) = NumberStruct.Sqrt(q, b.Num.Denominator);

        Expr? sqrt_frac = null;
        if (sqrt_p != 1 || sqrt_q != 1)
            sqrt_frac = isNeg ? Num(sqrt_q, sqrt_p) : Num(sqrt_p, sqrt_q);
        
        var frac = isNeg ? Num(q, p) : Num(p, q);
        
        return sqrt_frac is null ? frac : frac * Power.ConstructNotEval(sqrt_frac, Num(1, n.Denominator));
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
        return Num.ToString();
    }

    public override string ToLatex()
    {
        return Num.ToLatex();
    }
}