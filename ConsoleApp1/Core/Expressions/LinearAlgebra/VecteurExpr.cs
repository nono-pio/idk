using ConsoleApp1.Latex;

namespace ConsoleApp1.Core.Expressions.LinearAlgebra;

public class VecteurExpr : Expr
{

    public int Dim => Args.Length;
    public Expr[] Comps => Args;
    
    public Expr X => Args[0];
    public Expr Y => Args[1];
    public Expr Z => Args[2];
    public Expr W => Args[3];


    public VecteurExpr(params Expr[] args) : base(args) {}

    public Expr Norme()
    {
        return Sqrt(
            Add( Comps.Map(comp => Pow(comp, Deux)) )
            );
    }

    public override OrderOfOperation GetOrderOfOperation()
    {
        return OrderOfOperation.Atom;
    }

    public Expr ScalarProduct(VecteurExpr b) => Dot(this, b);
    public static Expr ScalarProduct(VecteurExpr a, VecteurExpr b) => Dot(a, b);
    public Expr Dot(VecteurExpr b) => Dot(this, b);

    public static Expr Dot(VecteurExpr a, VecteurExpr b)
    {
        
        if (a.Dim != b.Dim)
            throw new Exception("Les vecteurs doivent avoir la même dimension");

        var sum = Zero;
        for (var i = 0; i < a.Dim; i++)
        {
            sum += a.Comps[i] * b.Comps[i];
        }

        return sum;
    }
    
    public Expr Cross(VecteurExpr b) => Cross(this, b);
    public static Expr Cross(VecteurExpr a, VecteurExpr b)
    {
        // TODO : Generaliser à n dimensions
        if (a.Dim != 3 || b.Dim != 3)
            throw new Exception("Les vecteurs doivent être de dimension 3");

        return Vec(
            a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
            a.X * b.Y - a.Y * b.X
        );
    }

    public Expr Eval() => this;
    
    public override string ToLatex()
    {
        return LatexUtils.Vector(Comps.Map(x => x.ToLatex()));
    }

    public override string ToString()
    {
        return ToLatex();
    }

    public override double N()
    {
        throw new Exception("Cannot convert a vector to a scalar");
    }

    public override Expr Inverse(Expr y, int argIndex)
    {
        throw new NotImplementedException();
    }

    public override Expr Derivee(string variable)
    {
        return Vec( Args.Map(x => x.Derivee(variable)) );
    }
}