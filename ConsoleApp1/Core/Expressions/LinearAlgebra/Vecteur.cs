namespace ConsoleApp1.Core.Expressions.LinearAlgebra;

public class Vecteur : Expr
{

    public int Dim => Args.Length;
    
    public Expr X => Args[0];
    public Expr Y => Args[1];
    public Expr Z => Args[2];
    public Expr W => Args[3];


    public Vecteur(params Expr[] args) : base(args) {}

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