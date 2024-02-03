using ConsoleApp1.core.atoms;

namespace ConsoleApp1.core.expr.fonctions;

public class Power : Expr
{
    public Power(Expr value, Expr exp) : base(value, exp)
    {
    }

    public Expr value
    {
        get => Args[0];
        set => Args[0] = value;
    }

    public Expr exp
    {
        get => Args[1];
        set => Args[1] = value;
    }

    public override Expr Derivee(string variable)
    {
        if (exp.Constant(variable)) return exp * Pow(value, exp - 1) * value.Derivee(variable);

        if (value.Constant(variable)) return null /* Ln(value)*Pow(value, exp)*exp.Derivee(variable) */;

        return null /**/;
    }

    // a^b : inv(c, 0) -> c^(1/b)
    // a^b : inv(c, 1) -> ln_a(c)
    public override Expr Inverse(Expr y, int argIndex)
    {
        return argIndex switch
        {
            0 => Sqrt(y, exp),
            1 => throw new Exception("not implemented"),
            _ => throw new Exception("Power as only 2 args (value:0, exp:1), not " + argIndex)
        };
    }

    public Expr Eval()
    {
        // 1. Special Values    a^b
        // a = 1 -> 1; a = 0 -> 0
        // b = 0 -> 1; b = 1 -> a
        // a, b is number -> a^b simplified

        if (value is Number a && exp is Number b) return SimplifyNumber(a, b);

        if (value.IsOne() || exp.IsZero()) return Num(1);

        if (value.IsZero()) return Num(0);

        if (exp.IsOne()) return value;

        // 2. Power Tower       pow(pow(a,b),c) -> pow(a,bc)
        if (value is Power pow)
        {
            value = pow.value;
            exp = exp * pow.exp;
            return this;
        }

        return this;
    }

    // TODO
    private Expr SimplifyNumber(Number a, Number b)
    {
        try
        {
            return Num(Math.Pow(a.Num, b.Num));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return Pow(a, b);
        }
    }

    public override string ToLatex()
    {
        return value.ToLatex() + "^" + exp.ToLatex();
    }

    public override string ToString()
    {
        return value + "^" + exp;
    }
}