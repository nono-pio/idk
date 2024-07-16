using Boolean = ConsoleApp1.Core.Booleans.Boolean;

namespace ConsoleApp1.Core.Sets;

public class Interval : Set
{
    public Expr Start;
    public Expr End;
    
    public bool StartInclusive;
    public bool EndInclusive;
    private Interval(Expr start, Expr end, bool startInclusive, bool endInclusive)
    {
        Start = start;
        End = end;
        StartInclusive = startInclusive;
        EndInclusive = endInclusive;
    }
    
    public new static Set CreateInterval(Expr start, Expr end, bool startInclusive = true, bool endInclusive = true)
    {
        if (!start.IsExtendedReal || !end.IsExtendedReal || !(end-start).IsExtendedReal)
            throw new Exception("Invalid interval");

        if ((end - start).IsNegative)
            return EmptySet;

        if (end == start)
        {
            // ]1,1[ = {}
            if (!startInclusive && !endInclusive)
                return EmptySet;
            
            // [oo, oo] = {}
            if (start.IsInfinity || start.IsNegativeInfinity)
                return EmptySet;

            // [1,1] = {1}
            return CreateFiniteSet(start);
        }

        if (start.IsNegativeInfinity)
            startInclusive = false;
        if (end.IsInfinity)
            endInclusive = false;
        
        return new Interval(start, end, startInclusive, endInclusive);
    }

    public override Expr? Infimum() => Start;
    public override Expr? Supremum() => End;
    
    public override Set Complement(Set universe)
    {
        if (universe.IsR)
        {
            var a = CreateInterval(Expr.NegInf, Start, false, !StartInclusive);
            var b = CreateInterval(End, Expr.Inf, !EndInclusive, false);
            return CreateUnion(a, b);    
        }
        
        return base.Complement(universe);
    }

    public override Set Boundary()
    {
        return (Start.IsNegativeInfinity, End.IsInfinity) switch
        {
            (true, true) => Set.EmptySet,
            (true, false) => CreateFiniteSet(End),
            (false, true) => CreateFiniteSet(Start),
            (false, false) => CreateFiniteSet(Start, End)
        };
    }
    
    public override ConsoleApp1.Core.Booleans.Boolean? Contains(Expr x)
    {
        return AsCondition(x);
    }

    public ConsoleApp1.Core.Booleans.Boolean AsCondition(Expr x)
    {
        var start = StartInclusive ? ConsoleApp1.Core.Booleans.Boolean.GE(x, Start) : ConsoleApp1.Core.Booleans.Boolean.G(x, Start);
        var end = EndInclusive ? ConsoleApp1.Core.Booleans.Boolean.LE(x, End) : Boolean.L(x, End);
        return start & end;
    }
    
    public override string ToString()
    {
        return $"{(StartInclusive ? "[" : "]")}{Start}, {End}{(EndInclusive ? "]" : "[")}";
    }
    
}