﻿namespace ConsoleApp1.Core.Expressions.Atoms;

public abstract class Atom : Expr
{
    public abstract int CompareSelf(Atom expr);
    
    public override OrderOfOperation GetOrderOfOperation()
    {
        return OrderOfOperation.Atom;
    }

    public int CompareAtom(Atom? expr)
    {
        if (expr is null) return -1;

        // cmp type
        var cmpType = TypeId().CompareTo(expr.TypeId());
        if (cmpType != 0) return cmpType;

        return CompareSelf(expr);
    }
}