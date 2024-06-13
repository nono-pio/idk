using ConsoleApp1.Core.Expressions.ComplexExpressions;

namespace ConsoleApp1.Core;

public interface IStruct { }
public interface IScalarStruct { }

public struct IntStruct(long value) : IStruct, IScalarStruct
{
    private long Value = value;
}

public struct RealStruct(double value) : IStruct, IScalarStruct
{
    private double Value = value;
}

public readonly struct ComplexStruct : IStruct
{
    public readonly double Real;
    public readonly double Imaginary;

    public ComplexStruct(double real, double imaginary)
    {
        Real = real;
        Imaginary = imaginary;
    }
    
    public ComplexStruct(Complex complex)
    {
        Real = complex.Real.N();
        Imaginary = complex.Imag.N();
    }
    
    public ComplexStruct(ComplexPolarStruct polar)
    {
        Real = polar.Radius * Math.Cos(polar.Angle);
        Imaginary = polar.Radius * Math.Sin(polar.Angle);
    }

    public Complex AsComplex() => new(this);
    public ComplexPolarStruct AsPolar() => new(R(), Theta());
    public double Theta() => Arg();
    public double Arg()
    {
        throw new NotImplementedException("");
        // var (real, complex) = complexTuple;
        // return Atan(complex / real);
    }
    public double R()
    {
        return Math.Sqrt(Real * Real + Imaginary * Imaginary);
    }
    
    public static ComplexStruct operator +(ComplexStruct a, ComplexStruct b)
    {
        return new ComplexStruct(a.Real + b.Real, a.Imaginary + b.Imaginary);
    }
    
    public static ComplexStruct operator -(ComplexStruct a, ComplexStruct b)
    {
        return new ComplexStruct(a.Real - b.Real, a.Imaginary - b.Imaginary);
    }
    
    public static ComplexStruct operator *(ComplexStruct a, ComplexStruct b)
    {
        return new ComplexStruct(a.Real * b.Real - a.Imaginary * b.Imaginary, a.Real * b.Imaginary + a.Imaginary * b.Real);
    }
    
    public static ComplexStruct operator /(ComplexStruct a, ComplexStruct b)
    {
        var (r1, c1) = (a.Real, a.Imaginary);
        var (r2, c2) = (b.Real, b.Imaginary);
        
        var m = r2 * r2 + c2 * c2;
        var real = (r1*r2 + c1*c2) / m;
        var complex = (c1*r2 - r1*c2) / m;
        
        return new ComplexStruct(real, complex);
    }
}

public readonly struct ComplexPolarStruct(double radius, double angle) : IStruct
{
    public readonly double Radius = radius;
    public readonly double Angle = angle;
}

public struct RationalStruct(long numerator, long denominator) : IStruct, IScalarStruct
{
    private long Numerator = numerator;
    private long Denominator = denominator;
}

public struct VectorStruct<T>(T[] values) : IStruct where T : IStruct
{
    private T[] Values = values;
}

public struct MatrixStruct<T>(T[,] values) : IStruct where T : IStruct
{
    private T[,] Values = values;
}

public struct ExprStruct(Expr value) : IStruct
{
    private Expr Value = value;
}

public struct ObjectStruct(object value) : IStruct
{
    private object Value = value;
}
