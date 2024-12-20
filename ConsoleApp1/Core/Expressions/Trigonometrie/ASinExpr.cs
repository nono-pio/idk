﻿using Sdcb.Arithmetic.Mpfr;
using Boolean = ConsoleApp1.Core.Booleans.Boolean;

namespace ConsoleApp1.Core.Expressions.Trigonometrie;

public class ASinExpr(Expr x) : TrigonometrieExpr(x)
{
    public override Boolean DomainCondition => X >= -1 & X <= 1;
    public static Expr Construct(Expr x) => new ASinExpr(x);
    public override Expr Eval(Expr[] exprs, object[]? objects = null) => Construct(exprs[0]);
    public override Expr NotEval(Expr[] exprs, object[]? objects = null) => new ASinExpr(exprs[0]);
    
    public override string Name => "asin";
    public override string LatexName => "sin^{-1}";

    public override Expr fDerivee() => 1 / Sqrt(1 - Pow(X, 2));
    
    public override Expr Reciproque(Expr y) => Sin(y);

    public override double N() => Math.Asin(X.N());
    
    public override MpfrFloat NPrec(int precision = 333, MpfrRounding rnd = MpfrRounding.ToEven)
    {
        return MpfrFloat.Asin(X.NPrec(precision, rnd), precision, rnd);
    }
}