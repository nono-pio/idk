namespace ConsoleApp1.Core;

public interface IType
{
    public static readonly UnknownType Unknown = new UnknownType();
    public static readonly IntType Int = new IntType();
    public static readonly RationalType Rational = new RationalType();
    public static readonly RealType Real = new RealType();
    public static readonly ComplexType Complex = new ComplexType();
    public static readonly VectorType Vector = new VectorType();
    public static readonly TupleType Tuple = new TupleType(2);
    public static readonly MatrixType Matrix = new MatrixType();
}

public interface IScalarType;

public record UnknownType : IType;

public record IntType : IType, IScalarType; // {1,2,3}
public record RationalType : IType, IScalarType; // {1/2, 2/3, 3/4}
public record RealType : IType, IScalarType; // {1.0, 2.0, 3.0}
public record ComplexType : IType, IScalarType; // {1.0 + 2.0i, 2.0 + 3.0i, 3.0 + 4.0i}

public record VectorType : IType
{
    public VectorType(IType? dataType = null, int? dim = null)
    {
        DataType = dataType ?? IType.Unknown;
        Dim = dim;
    }

    public int? Dim;
    public IType DataType;
}

public record TupleType : IType
{
    public TupleType(int dim, IType? dataType = null)
    {
        DataType = dataType ?? IType.Unknown;
        Dim = dim;
    }

    public int? Dim;
    public IType DataType;
}

public record MatrixType : IType
{
    public MatrixType(IType? dataType = null, (int?, int?)? shape = null)
    {
        DataType = dataType ?? IType.Unknown;
        Shape = shape ?? (null, null);
    }

    public (int?, int?) Shape;
    public IType DataType;
}