namespace ConsoleApp1.Core.Sets;

public class FiniteSet : Set
{
    public double[] Elements;
    
    public FiniteSet(params double[] elements)
    {
        Elements = elements;
    }

    public override long? Length() => Elements.Length;
    public override bool IsEnumerable() => true;
    public override IEnumerable<double> GetEnumerable() => Elements;
    
    public override double Max()
    {
        return IsEmpty() ? double.NaN : Elements.Max();
    }

    public override double Min()
    {
        return IsEmpty() ? double.NaN : Elements.Min();
    }

    public override double PrincipalValue()
    {
        return IsEmpty() ? double.NaN : Elements[0];
    }

    public override bool Contain(double x) => Elements.Contains(x);
}