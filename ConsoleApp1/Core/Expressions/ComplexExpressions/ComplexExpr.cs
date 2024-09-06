using ConsoleApp1.Core.Complexes;

namespace ConsoleApp1.Core.Expressions.ComplexExpressions;

public class ComplexExpr(Expr real, Expr imaginary) : Expr
{
    
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

    public override Expr Derivee(string variable)
    {
        throw new NotImplementedException();
    }
}