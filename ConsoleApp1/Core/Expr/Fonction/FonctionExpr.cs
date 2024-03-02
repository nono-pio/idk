namespace ConsoleApp1.Core.Expr.Fonction;

public abstract class FonctionExpr : core.expr.Expr
{
    protected FonctionExpr(core.expr.Expr x) : base(x)
    {
    }

    public core.expr.Expr x => Args[0];


    /// The name of the fonction
    public abstract string Name();

    /// <para>
    ///     Derivée sans dérivée intérieur
    ///     <example>sin(x^2) -> cos(x^2)</example>
    /// </para>
    protected abstract core.expr.Expr BaseDerivee();

    public override core.expr.Expr Derivee(string variable)
    {
        return x.Derivee(variable) * BaseDerivee();
    }

    /// Retourne la reciproque de la fonction pour y : f^r(y)
    public abstract core.expr.Expr Reciproque(core.expr.Expr y);

    public override core.expr.Expr Inverse(core.expr.Expr y, int argIndex)
    {
        return Reciproque(y);
    }

    // <-- String Methodes -->
    public override string ToString()
    {
        return Name() + Parenthesis(x.ToString());
    }

    public override string? ToLatex()
    {
        return Name() + ParenthesisLatex(x.ToLatex());
    }
}