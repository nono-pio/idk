namespace ConsoleApp1.Core.Sets;

// Interval [1,2[
public class Interval : Set
{
    
    public double Start;
    public double End;
    public bool StartInclusive;
    public bool EndInclusive;
    
    public Interval(double start, double end, bool startInclusive = true, bool endInclusive = true)
    {
        Start = start;
        End = end;
        StartInclusive = startInclusive;
        EndInclusive = endInclusive;
    }

    public override long? Length() => null;
    public override bool IsEnumerable() => false;

    public override double Max() => End;
    public override double Min() => Start;

    public override double PrincipalValue() => Start;

    public override bool Contain(double x) => (StartInclusive ? x >= Start : x > Start) 
                                              && (EndInclusive ? x <= End : x < End);
}