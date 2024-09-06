namespace ConsoleApp1.Core.Expressions.ComplexExpressions;

public class ArgExpr(Expr x) : Expr(x)
{
    public Expr X => Args[0];
    
    
    public static Expr Construct(Expr x) => new ArgExpr(x);
    public override Expr Eval(Expr[] exprs, object[]? objects = null)
    {
        return Construct(exprs[0]);
    }

    public override Expr NotEval(Expr[] exprs, object[]? objects = null)
    {
        return new ArgExpr(exprs[0]);
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