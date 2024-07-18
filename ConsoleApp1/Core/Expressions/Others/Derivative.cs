namespace ConsoleApp1.Core.Expressions.Others;

public class Derivative(Expr function, string variable, Expr? n = null) : Expr(function, n ?? Num(1))
{
    public Expr Function => Args[0];
    public string Variable = variable;
    public Expr n => Args[1];
    
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

    public override Expr Inverse(Expr y, int argIndex)
    {
        throw new NotImplementedException();
    }

    public override Expr Derivee(string variable)
    {
        if (variable == Variable)
            return new Derivative(Function, Variable, n + 1);
        return new Derivative(Function.Derivee(variable), Variable, n);
    }
}