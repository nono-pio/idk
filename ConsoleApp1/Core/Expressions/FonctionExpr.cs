using ConsoleApp1.Core.Expressions.Others;
using ConsoleApp1.Core.Models;
using ConsoleApp1.Latex;

namespace ConsoleApp1.Core.Expressions;

public abstract class FonctionExpr(Expr x) : Expr(x)
{
    public Expr X => Args[0];


    /// The name of the fonction
    public abstract string Name { get; }
    public virtual string LatexName => Name;

    public override OrderOfOperation GetOrderOfOperation()
    {
        return OrderOfOperation.Atom;
    }

    /// <para>
    ///     Derivée sans dérivée intérieur
    ///     <example>sin(x^2) -> cos(x^2)</example>
    /// </para>
    protected virtual Expr? BaseDerivee() => null;
    public override Expr Derivee(string variable)
    {
        var baseDerivee = BaseDerivee();
        return baseDerivee is null ? new Derivative(this, variable) : X.Derivee(variable) * baseDerivee;
    }

    /// Retourne la reciproque de la fonction pour y : f^r(y)
    public virtual Expr Reciproque(Expr y) => throw new Exception("This function does not have a reciprocal");

    public override Expr Reciprocal(Expr y, int argIndex)
    {
        return Reciproque(y);
    }

    // <-- String Methodes -->
    public override string ToString()
    {
        return $"{Name}({X})";
    }

    public override string ToLatex()
    {
        return LatexUtils.Fonction(LatexName, X.ToLatex());
    }

}