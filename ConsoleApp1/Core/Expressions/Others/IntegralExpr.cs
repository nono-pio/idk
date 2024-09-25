using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.NumericalAnalysis;

namespace ConsoleApp1.Core.Expressions.Others;

public class IntegralExpr : Expr
{

    public Expr f;
    public Variable var;
    public (Expr a, Expr b)? Bornes; 
    
    public IntegralExpr(Expr f, Variable var, (Expr a, Expr b)? bornes = null) : base(f)
    {
        this.f = f;
        this.var = var;
        this.Bornes = bornes;
    }

    public override object[]? GetArgs()
    {
        return [var, Bornes];
    }

    public static Expr Eval(Expr f, Variable var, (Expr a, Expr b)? bornes = null)
    {

        if (f.IsZero)
            return 0;
        
        if (f.Constant(var))
        {
            if (bornes is null)
                return f * var + "C";
            
            return f * (bornes.Value.b - bornes.Value.a);
        }
        
        return new IntegralExpr(f, var, bornes);
    }
    public override Expr Eval(Expr[] exprs, object[]? objects = null)
    {
        return Eval(exprs[0], (Variable) objects[1], ((Expr, Expr)) objects[2]);
    }

    public override Expr NotEval(Expr[] exprs, object[]? objects = null)
    {
        return new IntegralExpr(exprs[0], (Variable) objects[1], ((Expr, Expr)) objects[2]);
    }

    public override string ToLatex()
    {
        string bornes_latex = Bornes is null ? "" : "_{" + Bornes.Value.a.ToLatex() + "}^{" + Bornes.Value.b.ToLatex() + "}";
        
        return "\\int" + bornes_latex + " " + f.ToLatex() + " d" + var.ToLatex();
    }

    public override double N()
    {
        if (Bornes is null)
            return Double.NaN;
        
        var a = Bornes.Value.a.N();
        var b = Bornes.Value.b.N();
        var func = f.AsFonction(var);

        IntegralApproximation.Integral(a, b, func.N);
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