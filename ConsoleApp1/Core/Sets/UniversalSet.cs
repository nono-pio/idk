namespace ConsoleApp1.Core.Sets;

public class UniversalSet : Set
{
    
    public UniversalSet() {}

    public override long? Length() => null;

    public override bool IsEnumerable() => false;

    public override double Max() => throw new Exception("Universal set has no maximum value."); 
    public override double Min() => throw new Exception("Universal set has no minimum value.");

    public override double PrincipalValue() => throw new Exception("Universal set has no principal value.");

    public override bool Contain(double x) => true;
}