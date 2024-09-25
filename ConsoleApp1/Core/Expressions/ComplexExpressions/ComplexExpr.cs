using ConsoleApp1.Core.Complexes;
using ConsoleApp1.Core.Expressions.Atoms;

namespace ConsoleApp1.Core.Expressions.ComplexExpressions;

// ComplexExpr(real, imaginary) = real + i*imaginary        for real, imaginary in R
public class ComplexExpr(Expr real, Expr imaginary) : Expr(real, imaginary)
{
    public override bool IsNatural => real.IsNatural && imaginary.IsZero;
    public override bool IsInteger => real.IsInteger && imaginary.IsZero;
    public override bool IsRational => real.IsRational && imaginary.IsZero;
    public override bool IsReal => real.IsReal && imaginary.IsZero;
    public override bool IsComplex => true;
    
    public override bool IsZero => real.IsZero && imaginary.IsZero;

    public override Complex AsComplex()
    {
        return new(real, imaginary);
    }
    
    public static Expr Construct(Expr real, Expr imaginary)
    {
        if (imaginary.IsZero)
            return real;
        
        return new ComplexExpr(real, imaginary);
    }
    public override Expr Eval(Expr[] exprs, object[]? objects = null)
    {
        return Construct(exprs[0], exprs[1]);
    }

    public override Expr NotEval(Expr[] exprs, object[]? objects = null)
    {
        return new ComplexExpr(exprs[0], exprs[1]);
    }

    public override string ToLatex()
    {
        throw new NotImplementedException();
    }

    public override double N()
    {
        throw new NotImplementedException();
    }

    public override Expr Reciprocal(Expr y, int argIndex)
    {
        throw new NotImplementedException();
    }

    public override Expr Derivee(Variable variable)
    {
        throw new NotImplementedException();
    }

    public override string? ToString()
    {
        return $"{real}+i*{imaginary}";
    }
}