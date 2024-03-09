namespace ConsoleApp1.Core;

public class Fonction
{
    public Expr.Expr Fx;
    public string Variable;
    
    public Fonction(Expr.Expr fx, string variable)
    {
        Fx = fx;
        Variable = variable;
    }

    public Expr.Expr Of(Expr.Expr x)
    {
        return Fx.Substitue(Variable, x);
    }
    
    public double N(double x)
    {
        return Of(x.Expr()).N();
        // TODO : optimiser
    }

    public Fonction Derivee()
    {
        return new Fonction(Fx.Derivee(Variable), Variable);
    }
    
}