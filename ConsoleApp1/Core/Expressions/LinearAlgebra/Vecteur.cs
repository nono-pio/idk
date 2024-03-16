namespace ConsoleApp1.Core.Expressions.LinearAlgebra;

public class Vecteur : Expr
{

    public int Dim => Args.Length;
    public Expr[] Comps => Args;
    
    public Expr X => Args[0];
    public Expr Y => Args[1];
    public Expr Z => Args[2];
    public Expr W => Args[3];


    public Vecteur(params Expr[] args) : base(args) {}

    public Expr Norme()
    {
        return Sqrt(
            Add( Comps.Map(comp => Pow(comp, Deux)) )
            );
    }
    
    
    public Expr ScalarProduct(Vecteur b) => Dot(this, b);
    public static Expr ScalarProduct(Vecteur a, Vecteur b) => Dot(a, b);
    public Expr Dot(Vecteur b) => Dot(this, b);

    public static Expr Dot(Vecteur a, Vecteur b)
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
    
    public Expr Cross(Vecteur b) => Cross(this, b);
    public static Expr Cross(Vecteur a, Vecteur b)
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

    public Expr Eval()
    {
        throw new NotImplementedException();
    }
    
    public override string ToLatex()
    {
        throw new NotImplementedException();
    }

    public override string ToString()
    {
        throw new NotImplementedException();
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