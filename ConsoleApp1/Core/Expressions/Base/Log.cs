namespace ConsoleApp1.Core.Expressions.Base;

public class Log : Expr
{

    public Expr Value => Args[0];
    public Expr Base => Args[1];
    
    public Log(Expr value, Expr @base) : base(value, @base) {}
    public Log(Expr value) : base(value, Math.E.Expr()) {}


    public override string ToLatex()
    {
        return @"\log_{" + Base.ToLatex() + "}(" + Value.ToLatex() + ")";
    }

    public override string ToString()
    {
        return "log_{" + Base + "}(" + Value + ")";
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

    
    public override (Expr, Expr) AsComplex()
    {

        var valueTuple = LnComplex(Value.AsComplex());
        var baseTuple = LnComplex(Base.AsComplex());
        
        return ComplexUtils.Div(valueTuple, baseTuple); // log_b(a) = ln(a) / ln(b)
    }

    // ln(a + bi) = 1/2*ln(a^2 + b^2) + i * (arg(a+bi) + 2*PI*n)
    public static (Expr, Expr) LnComplex((Expr, Expr) complexTuple)
    {
        var (r, t) = complexTuple;
        
        var real = Log(r*r + t*t)/2;
        var complex = ComplexUtils.Arg(complexTuple); // TODO: + 2*PI*n 

        return (real, complex);
    }

    public override Expr Derivee(string variable)
    {
        throw new NotImplementedException();
    }
    
    public Expr Eval()
    {
        throw new NotImplementedException();
    }
}