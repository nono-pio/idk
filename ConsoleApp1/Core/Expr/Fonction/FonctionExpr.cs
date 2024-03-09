namespace ConsoleApp1.Core.Expr.Fonction;

public abstract class FonctionExpr : Core.Expr.Expr
{
    protected FonctionExpr(Core.Expr.Expr x) : base(x)
    {
    }

    public Core.Expr.Expr x => Args[0];


    /// The name of the fonction
    public abstract string Name();

    /// <para>
    ///     Derivée sans dérivée intérieur
    ///     <example>sin(x^2) -> cos(x^2)</example>
    /// </para>
    protected abstract Core.Expr.Expr BaseDerivee();

    public override Core.Expr.Expr Derivee(string variable)
    {
        return x.Derivee(variable) * BaseDerivee();
    }

    /// Retourne la reciproque de la fonction pour y : f^r(y)
    public abstract Core.Expr.Expr Reciproque(Core.Expr.Expr y);

    public override Core.Expr.Expr Inverse(Core.Expr.Expr y, int argIndex)
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