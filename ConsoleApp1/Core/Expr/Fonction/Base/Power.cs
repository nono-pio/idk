using ConsoleApp1.core.atoms;

namespace ConsoleApp1.core.expr.fonctions;

public class Power : Expr
{
    public Power(Expr value, Expr exp) : base(value, exp) { }

    public Expr Base
    {
        get => Args[0];
        private set => Args[0] = value;
    }

    public Expr Exp
    {
        get => Args[1];
        private set => Args[1] = value;
    }

    public override Expr Derivee(string variable)
    {
        if (Exp.Constant(variable)) return Exp * Pow(Base, Exp - 1) * Base.Derivee(variable);

        throw new NotImplementedException("Power.Derivee : Base non constant");
        if (Base.Constant(variable)) return null /* Ln(value)*Pow(value, exp)*exp.Derivee(variable) */;

        return null /**/;
    }

    public override double N()
    {
        return Math.Pow(Base.N(), Exp.N());
    }
    
    /// a^b : inv(c, 0) -> c^(1/b)
    /// a^b : inv(c, 1) -> ln_a(c)
    public override Expr Inverse(Expr y, int argIndex)
    {
        return argIndex switch
        {
            0 => Sqrt(y, Exp),
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

        if (Base is Number a && Exp is Number b) return SimplifyNumber(a, b);

        if (Base.IsOne() || Exp.IsZero()) return Num(1);

        if (Base.IsZero()) return Num(0);

        if (Exp.IsOne()) return Base;

        // 2. Power Tower       pow(pow(a,b),c) -> pow(a,bc)
        if (Base is Power pow)
        {
            Base = pow.Base;
            Exp = Exp * pow.Exp;
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
        return Base.ToLatex() + "^" + Exp.ToLatex();
    }

    public override string ToString()
    {
        return Base + "^" + Exp;
    }
}