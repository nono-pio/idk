using ConsoleApp1.Core.Models;

namespace ConsoleApp1.Core.Expressions;

public abstract class FonctionExpr : Expr
{
    protected FonctionExpr(Expr x) : base(x)
    {
    }

    public Expr X => Args[0];


    /// The name of the fonction
    public abstract string Name();

    /// <para>
    ///     Derivée sans dérivée intérieur
    ///     <example>sin(x^2) -> cos(x^2)</example>
    /// </para>
    protected abstract Expr BaseDerivee();

    public override Expr Derivee(string variable)
    {
        return X.Derivee(variable) * BaseDerivee();
    }

    /// Retourne la reciproque de la fonction pour y : f^r(y)
    public abstract Expr Reciproque(Expr y);

    public override Expr Inverse(Expr y, int argIndex)
    {
        return Reciproque(y);
    }

    // <-- String Methodes -->
    public override string ToString()
    {
        return Name() + Parenthesis(X.ToString());
    }

    public override string ToLatex()
    {
        return Name() + ParenthesisLatex(X.ToLatex());
    }

}