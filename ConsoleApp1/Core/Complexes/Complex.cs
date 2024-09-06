using ConsoleApp1.Core.Expressions.Base;
using ConsoleApp1.Core.Expressions.ComplexExpressions;

namespace ConsoleApp1.Core.Complexes;

public struct Complex(Expr real, Expr imaginary)
{
    public Expr Real { get; } = real;
    public Expr Imaginary { get; } = imaginary;

    public Expr Norm => Sqrt(SqrNorm);
    public Expr SqrNorm => Real * Real + Imaginary * Imaginary;
    public Expr Argument => Arg(AsExpr());

    public Expr AsExpr() => new ComplexExpr(Real, Imaginary);
    public static implicit operator Complex(int num) => new Complex(num, 0);
    public static Complex FromPolar(Expr norm, Expr arg) => new Complex(norm * Cos(arg), norm * Sin(arg));
    
    
    public static Complex operator +(Complex a, Complex b) => new Complex(a.Real + b.Real, a.Imaginary + b.Imaginary);
    public static Complex operator -(Complex a, Complex b) => new Complex(a.Real - b.Real, a.Imaginary - b.Imaginary);
    public static Complex operator *(Complex a, Complex b) => new Complex(a.Real * b.Real - a.Imaginary * b.Imaginary, a.Real * b.Imaginary + a.Imaginary * b.Real);
    public static Complex operator /(Complex a, Complex b)
    {
        var norm = b.SqrNorm;
        return new Complex((a.Real * b.Real + a.Imaginary * b.Imaginary) / norm, (a.Imaginary * b.Real - a.Real * b.Imaginary) / norm);
    }

    public Complex Pow(Complex exp)
    {
        var norm = Norm;
        var arg = Argument;
        var re = exp.Real;
        var im = exp.Imaginary;
        
        var new_norm = ConstructorBase.Pow(norm, re) * Exp(-im * arg);
        var new_arg = arg * re + Log(norm) * im;
        return FromPolar(new_norm, new_arg);
    }
}