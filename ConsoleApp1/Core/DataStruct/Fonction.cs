namespace ConsoleApp1.core;

public class Fonction
{
    public Expr Fx;
    public string Variable;
    
    public Fonction(Expr fx, string variable)
    {
        Fx = fx;
        Variable = variable;
    }

    public Expr Of(Expr x)
    {
        return Fx.Substitue(Variable, x);
    }
    
    public double N(double x)
    {
        throw new NotImplementedException("Fonction.Of");
        //return Fx.Substitue(Variable, x).N();
        // TODO : optimiser
    }
    
}