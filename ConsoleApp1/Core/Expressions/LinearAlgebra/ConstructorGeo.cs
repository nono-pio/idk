namespace ConsoleApp1.Core.Expressions.LinearAlgebra;

public static class ConstructorGeo
{


    public static Expr Vec(params Expr[] exprs)
    {
        return new VecteurExpr(exprs).Eval();
    }
    
    
    public static Expr Matrix((int, int) shape, params Expr[] values)
    {
        return new MatrixExpr(shape, values).Eval();
    }
    
    public static Expr Matrix(Expr[,] values)
    {
        return new MatrixExpr(values).Eval();
    }
    
    
}