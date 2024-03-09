namespace ConsoleApp1.Core.Expr.Geometrie;

public static class ConstructorGeo
{


    public static Expr Vec(params Expr[] exprs)
    {
        return new Vecteur(exprs).Eval();
    }
    
    
    public static Expr Matrix((int, int) shape, params Expr[] values)
    {
        return new Matrix(shape, values).Eval();
    }
    
    public static Expr Matrix(Expr[,] values)
    {
        return new Matrix(values).Eval();
    }
    
    
}