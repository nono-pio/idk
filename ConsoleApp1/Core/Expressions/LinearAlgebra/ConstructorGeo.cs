namespace ConsoleApp1.Core.Expressions.LinearAlgebra;

public static class ConstructorGeo
{
    
    public static Expr Vec(params Expr[] exprs) => VecteurExpr.Construct(exprs);
    public static Expr Matrix((int, int) shape, params Expr[] values) => MatrixExpr.Construct(shape, values);
    public static Expr Matrix(Expr[,] values) => MatrixExpr.Construct(values);
    
    
}