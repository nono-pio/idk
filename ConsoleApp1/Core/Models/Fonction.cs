using ConsoleApp1.Core.Expressions.Atoms;

namespace ConsoleApp1.Core.Models;

public class Fonction
{
    public Expr Fx;
    public Variable Variable;
    
    public Fonction(Expr fx, Variable variable)
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
        Variable.Value = x;
        return Fx.N();
    }

    public Fonction Derivee()
    {
        return new Fonction(Fx.Derivee(Variable), Variable);
    }
    
}