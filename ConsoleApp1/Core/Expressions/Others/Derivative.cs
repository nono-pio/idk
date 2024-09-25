using ConsoleApp1.Core.Expressions.Atoms;

namespace ConsoleApp1.Core.Expressions.Others;

public class Derivative(Expr function, Variable variable, Expr? n = null) : Expr(function, n ?? Num(1))
{
    public Expr Function => Args[0];
    public Variable Variable = variable;
    public Expr n => Args[1];
    
    public static Expr Construct(Expr function, Variable variable, Expr n) => new Derivative(function, variable, n);
    public static Expr Construct(Expr function, Variable variable) => new Derivative(function, variable);
    public override Expr Eval(Expr[] exprs, object[]? objects = null) => Construct(exprs[0], (Variable)objects[0], exprs[1]);
    public override Expr NotEval(Expr[] exprs, object[]? objects = null) => new Derivative(exprs[0], (Variable)objects[0], exprs[1]);

    public Expr DoIt()
    {
        if (n.IsInteger)
            return Function.Derivee(Variable, (int) n.N());
        return this;
    }
    
    public override string ToLatex()
    {
        return $"\\frac{{d^{{{n.ToLatex()}}}}}{{d{Variable}^{{{n.ToLatex()}}}}}{Function.ToLatex()}";
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
        if (variable == Variable)
            return new Derivative(Function, Variable, n + 1);
        return new Derivative(Function.Derivee(variable), Variable, n);
    }
}