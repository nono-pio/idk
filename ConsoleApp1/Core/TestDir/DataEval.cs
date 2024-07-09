using System.Runtime.InteropServices;
using ConsoleApp1.Core.Expressions.Base;

namespace ConsoleApp1.Core.TestDir;

/*
DataTypes:
- Int,
- Double,
- Complex,
- Vectors,
- Matrices,
*/

public interface IDataEval { }


[StructLayout(LayoutKind.Explicit)]
public struct DataEval : IDataEval
{
    enum DataType
    {
        Nan,
        Int,
        Double,
        Complex,
        Vector,
        Matrix
    }
    
    [FieldOffset(0)]
    private DataType Type;
    
    [FieldOffset(4)]
    public DataInt Int;

    [FieldOffset(4)]
    public DataDouble Double;

    [FieldOffset(4)]
    public DataComplex Complex;

    [FieldOffset(4)]
    public DataVector<DataDouble> Vector;

    [FieldOffset(4)]
    public DataMatrix<DataDouble> Matrix;

    public DataEval(int value)
    {
        Type = DataType.Int;
        Int = value;
    }
    
    public DataEval(double value)
    {
        Type = DataType.Double;
        Double = value;
    }
    
    public DataEval(double real, double imaginary)
    {
        Type = DataType.Complex;
        Complex = new DataComplex { Real = real, Imaginary = imaginary };
    }
    
    public DataEval(double[] values)
    {
        Type = DataType.Vector;
        Vector = new DataVector<DataDouble> { Values = values.Map(v => new DataDouble{Value = v}) };
    }

    public DataEval((uint, uint) shape, double[] values)
    {
        Type = DataType.Matrix;
        Matrix = new DataMatrix<DataDouble> { Shape = shape, Values = values.Map(v => new DataDouble{Value = v}) };
    }
}

/* Data */
public struct DataInt : IDataEval
{
    public int Value;
    
    public static implicit operator DataInt(int value) => new DataInt { Value = value };
    
}

public struct DataDouble : IDataEval
{
    public double Value;
    
    public static implicit operator DataDouble(double value) => new DataDouble { Value = value };
}

public struct DataComplex : IDataEval
{
    public double Real;
    public double Imaginary;
}

public struct DataVector<T> : IDataEval where T : IDataEval
{
    public T[] Values;
}

public struct DataMatrix<T> : IDataEval where T : IDataEval
{
    public (uint, uint) Shape;
    public T[] Values;
}