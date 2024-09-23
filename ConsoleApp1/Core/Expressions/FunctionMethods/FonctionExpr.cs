using ConsoleApp1.Core.Expressions.FunctionMethods;
using ConsoleApp1.Core.Expressions.Others;
using ConsoleApp1.Core.Models;
using ConsoleApp1.Latex;

namespace ConsoleApp1.Core.Expressions;

public abstract class FonctionExpr(Expr x) : Expr(x)
{
    public Expr X => Args[0];

    public virtual FuncAttributes Attributes { get; } = new FuncAttributes();
    
    public override bool IsPositive => Attributes.Positive;
    public override bool IsNegative => Attributes.Negative;

    public override bool IsZero => Attributes.YInterceptZero && X.IsZero;

    /// The name of the fonction
    public abstract string Name { get; }
    public virtual string LatexName => Name;

    public override OrderOfOperation GetOrderOfOperation()
    {
        return OrderOfOperation.Atom;
    }
    
    public virtual Expr fDerivee() => throw new NotImplementedException();
    public override Expr fDerivee(int argIndex)
    {
        if (argIndex == 0)
        {
            return fDerivee();
        }

        throw new ArgumentException("ArgIndex must be 0 for a univariate function");
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