namespace ConsoleApp1.Core.Expressions.ComplexExpressions;

public class Complex : Expr
{

    public Expr Real => Args[0];
    public Expr Imag => Args[1];
    
    public Complex(Expr real, Expr imag) : base(real, imag) {}

    public Expr Eval()
    {
        return Imag.IsZero() ? Real : this;
    }
    
    public override string ToLatex()
    {
        return $"{Real.ToLatex()} + {Imag.ToLatex()}i";
    }

    public override string ToString()
    {
        return $"{Real} + {Imag}i";
    }

    public override double N()
    {
        var imag = Imag.N();
        if (imag == 0)
        {
            return Real.N();
        }
        
        throw new Exception("Cannot convert complexe number to real number");
    }

    public override Expr Inverse(Expr y, int argIndex)
    {
        return argIndex switch
        {
            0 => Complex(y, -Imag),
            1 => Complex(Zero, Real - y),
            _ => throw new Exception("argIndex must be 0 or 1")
        };
    }

    public override Expr Derivee(string variable)
    {
        return Complex(Real.Derivee(variable), Imag.Derivee(variable));
    }
    
    # region Operators
    
    public static Expr operator +(Complex a, Complex b)
    {
        return Complex(a.Real + b.Real, a.Imag + b.Imag);
    }
    
    public static Expr operator -(Complex a, Complex b)
    {
        return Complex(a.Real - b.Real, a.Imag - b.Imag);
    }
    
    public static Expr operator *(Complex a, Complex b)
    {
        return Complex(a.Real * b.Real - a.Imag * b.Imag, a.Real * b.Imag + a.Imag * b.Real);
    }
    
    public static Expr operator /(Complex a, Complex b)
    {
        var div = b.Real * b.Real + b.Imag * b.Imag;
        return Complex( (a.Real * b.Real + a.Imag * b.Imag) / div, (a.Imag * b.Real - a.Real * b.Imag) / div);
    }
    
    # endregion
}