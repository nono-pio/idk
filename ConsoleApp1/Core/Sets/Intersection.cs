namespace ConsoleApp1.Core.Sets;

public class Intersection : Set
{

    public Set A;
    public Set B;
    
    public Intersection(Set a, Set b)
    {
        A = a;
        B = b;
    }


    public override long? Length() => A.Length() is null || B.Length() is null ? null : A.Length() + B.Length();

    public override bool IsEnumerable()
    {
        return A.IsEnumerable() && B.IsEnumerable();
    }
    
    public override IEnumerable<double> GetEnumerable()
    {
        throw new NotImplementedException();
    }

    public override double Max()
    {
        throw new NotImplementedException();
    }

    public override double Min()
    {
        throw new NotImplementedException();
    }

    public override double PrincipalValue()
    {
        throw new NotImplementedException();
    }

    public override bool Contain(double x)
    {
        return A.Contain(x) ^ B.Contain(x);
    }
}