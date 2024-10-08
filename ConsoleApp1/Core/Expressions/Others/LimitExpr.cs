﻿using ConsoleApp1.Core.Expressions.Atoms;

namespace ConsoleApp1.Core.Expressions.Others;

public class LimitExpr : Expr
{

    public Expr f;
    public Variable var;
    public Expr lim;
    
    public LimitExpr(Expr f, Variable var, Expr lim)
    {
        this.f = f;
        this.var = var;
        this.lim = lim;
    }

    public override object[] GetArgs()
    {
        return [var];
    }

    public override Expr Eval(Expr[] exprs, object[]? objects = null)
    {
        return new LimitExpr(exprs[0], var, lim);
    }

    public override Expr NotEval(Expr[] exprs, object[]? objects = null)
    {
        return new LimitExpr(exprs[0], var, lim);
    }

    public override string ToLatex()
    {
        throw new NotImplementedException();
    }

    public override double N()
    {
        throw new NotImplementedException();
    }

    public override Expr Reciprocal(Expr y, int argIndex)
    {
        throw new NotImplementedException();
    }

    public override Expr Derivee(Variable variable)
    {
        throw new NotImplementedException();
    }
}