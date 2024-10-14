using ConsoleApp1.Core.Complexes;
using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Latex;

namespace ConsoleApp1.Core.Expressions.Base;

public class Logarithm : Expr
{
    public override bool IsReal => Base.IsPositive && Value.IsPositive;
    public override bool IsComplex => true;


    public Expr Value => Args[0];
    public Expr Base => Args[1];
    
    public Logarithm(Expr value, Expr @base) : base(value, @base) {}
    public Logarithm(Expr value) : base(value, Math.E.Expr()) {}

    
    public static Expr Construct(Expr value, Expr @base)
    {
        // log_x(0) = NaN
        if (value.IsNumZero)
            return Atoms.Constant.NaN;
        // log_x(1) = 0
        if (value.IsNumOne)
            return 0;
        // log_1(x) = log(x)/log(1) = log(x)/0 = NaN
        if (@base.IsNumOne)
            return Atoms.Constant.NaN;

        // log_x(x) = lnx/lnx = 1
        if (@base == value)
            return 1;
        // log_x(x^n) = n
        if (value is Power vPow && vPow.Base == @base)
            return vPow.Exp;
        
        return new Logarithm(value, @base);
    }
    public static Expr Construct(Expr value) => new Logarithm(value);
    public override Expr Eval(Expr[] exprs, object[]? objects = null) => Construct(exprs[0], exprs[1]);
    public override Expr NotEval(Expr[] exprs, object[]? objects = null) => new Logarithm(exprs[0], exprs[1]);

    public Complex LnComplex(Complex z)
    {
        var real = Ln(z.SqrNorm)/2;
        var imag = z.Argument + 2*Atoms.Constant.PI*"n";
        
        return new Complex(real, imag);
    }
    public override Complex AsComplex()
    {
        return LnComplex(Value.AsComplex()) / LnComplex(Base.AsComplex());
    }

    public override OrderOfOperation GetOrderOfOperation()
    {
        return OrderOfOperation.Atom;
    }
    
    public override string ToLatex()
    {
        if (Base == Atoms.Constant.E)
            return LatexUtils.Fonction("\\ln", Value.ToLatex());
        
        if (Base.Is(10)) 
            return LatexUtils.Fonction("\\log", Value.ToLatex());

        return LatexUtils.Fonction("\\log", Value.ToLatex(), subscript: Base.ToLatex());
    }

    public override Expr Develop()
    {
        return Ln(Value) / Ln(Base);
    }

    public override string ToString()
    {
        return ToLatex();
    }

    public override double N()
    {
        return Math.Log(Value.N(), Base.N());
    }

    public override Expr Reciprocal(Expr y, int argIndex)
    {
        if (argIndex == 0)
        {
            return Pow(Base, y);
        }
        else if (argIndex == 1)
        {
            return Pow(Value, 1 / y);
        }
        
        throw new ArgumentException("ArgIndex must be 0 (value) or 1 (base)");
    }

    public override Expr fDerivee(int argIndex)
    {
        if (argIndex == 0)
        {
            return 1 / (Value * Ln(Base));
        }
        else if (argIndex == 1)
        {
            return - this / (Base * Ln(Base));
        }
        
        throw new ArgumentException("ArgIndex must be 0 (value) or 1 (base)");
    }
}