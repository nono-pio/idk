namespace ConsoleApp1.Core;

public abstract record ExprType
{
    public static readonly UnknownType UnknownType = new UnknownType();
    public static readonly NumberType NumberType = new NumberType();
    public static readonly VectorType UnknownVectorType = new VectorType(UnknownType);
    public static readonly VectorType VectorType = new VectorType(NumberType);
    public static readonly MatrixType UnknownMatrixType = new MatrixType(UnknownType);
    public static readonly MatrixType MatrixType = new MatrixType(NumberType);
}

public record UnknownType : ExprType;

public record NumberType : ExprType;

public record VectorType(ExprType? DataType = null) : ExprType
{
    public ExprType DataType = DataType ?? UnknownType;
}

public record TupleType(params ExprType[] DataTypes) : ExprType
{
    public ExprType[] DataTypes = DataTypes;
}

public record MatrixType(ExprType? DataType = null) : ExprType
{
    public ExprType DataType = DataType ?? UnknownType;
}
