namespace ConsoleApp1.Core.Expressions.LinearAlgebra;

public class Matrix : Expr
{

    public (int, int) Shape;
    public Expr[] Values1D => Args;
    
    public Matrix((int, int) shape, params Expr[] values) : base(values)
    {

        if (shape.Item1 * shape.Item2 != values.Length)
        {
            throw new ArgumentException("The shape must correspond to the values");
        }
        
        Shape = shape;
    }
    
    public Matrix(Expr[,] values) : base(From2DTo1D(values))
    {
        Shape = (values.GetLength(0), values.GetLength(1));
    }

    public Expr Eval()
    {
        return this;
    }

    public static Expr[] From2DTo1D(Expr[,] values)
    {
        var (line, col) = (values.GetLength(0), values.GetLength(1));
        Expr[] new_values = new Expr[line * col];

        for (int i = 0; i < line; i++)
        {
            for (int j = 0; j < col; j++)
            {
                new_values[i * col + j] = values[line, col];
            }
        }

        return new_values;
    }


    public override string ToString()
    {
        throw new NotImplementedException();
    }

    public override string ToLatex()
    {
        throw new NotImplementedException();
    }

    public override double N()
    {
        throw new NotImplementedException();
    }

    public override Expr Inverse(Expr y, int argIndex)
    {
        throw new NotImplementedException();
    }

    public override Expr Derivee(string variable)
    {

        Expr[] ddx = new Expr[Values1D.Length];

        for (int i = 0; i < Values1D.Length; i++)
        {
            ddx[i] = Values1D[i].Derivee(variable);
        }

        return Matrix(Shape, ddx);
    }
}