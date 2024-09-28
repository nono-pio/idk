﻿using ConsoleApp1.Core.Sets;

namespace ConsoleApp1.Core.Expressions.Others;

public class RoundExpr(Expr x) : FonctionExpr(x)
{
    public override bool IsNatural => IsPositive;
    public override bool IsInteger => true;

    public static Expr Eval(Expr x)
    {
        if (x.IsInteger)
            return x;
        
        return new RoundExpr(x);
    }

    public override Set AsSet()
    {
        return ArithmeticOnSet.FunctionOnSet(Eval, X.AsSet(), ArithmeticOnSet.FunctionBasicNumber(natural: Set.N, integer: Set.Z, rational: Set.Z, real: Set.Z));
    }

    public override Expr Eval(Expr[] exprs, object[]? objects = null)
    {
        return Eval(exprs[0]);
    }

    public override Expr NotEval(Expr[] exprs, object[]? objects = null)
    {
        return new RoundExpr(exprs[0]);
    }

    public override double N()
    {
        return Math.Round(X.N());
    }

    public override string Name => "round";
}