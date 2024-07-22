using ConsoleApp1.Latex;

namespace ConsoleApp1.Core.Expressions.LinearAlgebra;

public class MatrixExpr : Expr
{

    public (int, int) Shape;
    public Expr[] Data => Args;
    
    public MatrixExpr((int, int) shape, params Expr[] values) : base(values)
    {

        if (shape.Item1 * shape.Item2 != values.Length)
        {
            throw new ArgumentException("The shape must correspond to the values");
        }
        
        Shape = shape;
    }
    
    public MatrixExpr(Expr[,] values) : base(From2DTo1D(values))
    {
        Shape = (values.GetLength(0), values.GetLength(1));
    }
    
    public static Expr Construct((int, int) shape, params Expr[] values) => new MatrixExpr(shape, values);
    public static Expr Construct(Expr[,] values) => new MatrixExpr(values);
    public override Expr Eval(Expr[] exprs, object[]? objects = null) => Construct(((int, int))objects[0], exprs);
    public override Expr NotEval(Expr[] exprs, object[]? objects = null) => new MatrixExpr(((int, int))objects[0], exprs);

    public override OrderOfOperation GetOrderOfOperation()
    {
        return OrderOfOperation.Atom;
    }

    public static MatrixExpr FillWith(Expr item, (int, int) shape)
    {
        var values = new Expr[shape.Item1 * shape.Item2];
        Array.Fill(values, item);
        return new MatrixExpr(shape, values);
    }

    public static Expr[] From2DTo1D(Expr[,] values)
    {
        var (line, col) = (values.GetLength(0), values.GetLength(1));
        Expr[] new_values = new Expr[line * col];

        for (int i = 0; i < line; i++)
        {
            for (int j = 0; j < col; j++)
            {
                new_values[i * col + j] = values[i, j];
            }
        }
        
        return new_values;
    }

    public Expr this[int i, int j] => Data[i * Shape.Item1 + j];

    public static MatrixExpr operator +(MatrixExpr A, MatrixExpr B)
    {
        if (A.Shape != B.Shape)
        {
            throw new Exception("Dimension incorrect");
        }

        var datas = new Expr[A.Data.Length];
        for (int i = 0; i < A.Data.Length; i++)
        {
            datas[i] = A.Data[i] + B.Data[i];
        }

        return new MatrixExpr(A.Shape, datas);
    }
    
    public static MatrixExpr operator -(MatrixExpr A, MatrixExpr B)
    {
        if (A.Shape != B.Shape)
        {
            throw new Exception("Dimension incorrect");
        }

        var datas = new Expr[A.Data.Length];
        for (int i = 0; i < A.Data.Length; i++)
        {
            datas[i] = A.Data[i] - B.Data[i];
        }

        return new MatrixExpr(A.Shape, datas);
    }

    public static MatrixExpr operator *(MatrixExpr A, MatrixExpr B)
    {
        if (A.Shape.Item2 != B.Shape.Item1)
        {
            throw new Exception("Dimension incorrect");
        }

        var datas = new Expr[A.Shape.Item1 * B.Shape.Item2];

        int row = A.Shape.Item1;
        int col = B.Shape.Item2;
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < col; j++)
            {
                var sum = Zero;
                for (int k = 0; k < A.Shape.Item2; k++)
                {
                    sum += A[i, k] * B[k, j];
                }
                datas[i * row + j] = sum;
            }
        }

        return new MatrixExpr((row, col), datas);
    }

    public MatrixExpr BitWiseFunc(Func<Expr, Expr> func)
    {
        var datas = new Expr[Data.Length];
        for (int i = 0; i < datas.Length; i++)
        {
            datas[i] = func(Data[i]);
        }

        return new MatrixExpr(Shape, datas);
    }

    public override string ToString()
    {
        throw new NotImplementedException();
    }

    public override string ToLatex()
    {
        
        var components = new string[Shape.Item1][];
        for (int i = 0; i < Shape.Item1; i++)
        {
            components[i] = new string[Shape.Item2];
            for (int j = 0; j < Shape.Item2; j++)
            {
                components[i][j] = Data[i * Shape.Item1 + j].ToLatex();
            }
        }
        
        return LatexUtils.Matrix(components);
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

        Expr[] ddx = new Expr[Data.Length];

        for (int i = 0; i < Data.Length; i++)
        {
            ddx[i] = Data[i].Derivee(variable);
        }

        return Matrix(Shape, ddx);
    }
}