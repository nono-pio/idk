using ConsoleApp1.Latex;

namespace ConsoleApp1.Core.Expressions.Base;

public class Logarithm : Expr
{

    public Expr Value => Args[0];
    public Expr Base => Args[1];
    
    public Logarithm(Expr value, Expr @base) : base(value, @base) {}
    public Logarithm(Expr value) : base(value, Math.E.Expr()) {}

    
    public static Expr Construct(Expr value, Expr @base) => new Logarithm(value, @base);
    public static Expr Construct(Expr value) => new Logarithm(value);
    public override Expr Eval(Expr[] exprs, object[]? objects = null) => Construct(exprs[0], exprs[1]);
    public override Expr NotEval(Expr[] exprs, object[]? objects = null) => new Logarithm(exprs[0], exprs[1]);


    public override OrderOfOperation GetOrderOfOperation()
    {
        return OrderOfOperation.Atom;
    }
    
    public override string ToLatex()
    {
        // TODO: if base = e -> ln
        
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

    public override Expr Inverse(Expr y, int argIndex)
    {
        if (argIndex == 0)
        {
            return Pow(Base, y);
        }
        else if (argIndex == 1)
        {
            return Pow(Value, 1 / y);
        }
        
        throw new Exception("ArgIndex must be 0 (value) or 1 (base)");
    }

    public override Expr Derivee(string variable)
    {
        throw new NotImplementedException();
    }
}