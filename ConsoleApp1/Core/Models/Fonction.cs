﻿using ConsoleApp1.Core.Expressions.Atoms;

namespace ConsoleApp1.Core.Models;

public class Fonction
{
    public Expr Fx;
    public string NameVariable;
    
    public Fonction(Expr fx, string variable)
    {
        Fx = fx;
        NameVariable = variable;
    }

    public Expr Of(Expr x)
    {
        return Fx.Substitue(NameVariable, x);
    }
    
    public double N(double x)
    {
        var isSet = Variable.SetValue(NameVariable, x);
        return isSet ? Fx.N() : Of(x).N();
    }

    public Fonction Derivee()
    {
        return new Fonction(Fx.Derivee(NameVariable), NameVariable);
    }
    
}