using Boolean = ConsoleApp1.Core.Booleans.Boolean;

namespace ConsoleApp1.Core.Sets;

public class IntervalSet : Set
{
    public Expr Start;
    public Expr End;
    
    public bool StartInclusive;
    public bool EndInclusive;

    public override bool IsElementsNatural => false;
    public override bool IsElementsInteger => false;
    public override bool IsElementsRational => false;
    public override bool IsElementsReal => true;
    
    public override bool IsElementsPositive => Start.IsPositive && End.IsPositive;
    public override bool IsElementsNegative => Start.IsNegative && End.IsNegative;

    public IntervalSet(Expr start, Expr end, bool startInclusive, bool endInclusive)
    {
        Start = start;
        End = end;
        StartInclusive = startInclusive;
        EndInclusive = endInclusive;
    }

    public Set ArithmeticAdd(IntervalSet other)
    {
        return Interval(Start + other.Start, End + other.End, StartInclusive || other.StartInclusive, EndInclusive || other.EndInclusive);
    }

    public bool Overlap(IntervalSet interval)
    {
        bool startOver = StartInclusive || interval.EndInclusive ? interval.End >= Start : interval.End > Start;
        bool endOver = EndInclusive || interval.StartInclusive ? interval.Start <= End : interval.Start < End;
        
        return startOver && endOver;
    }

    
    public static Set Construct(Expr start, Expr end, bool startInclusive = true, bool endInclusive = true)
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
            return ArraySet(start);
        }

        if (start.IsNegativeInfinity && end.IsInfinity)
            return R;

        if (start.IsNegativeInfinity)
            startInclusive = false;
        if (end.IsInfinity)
            endInclusive = false;
        
        return new IntervalSet(start, end, startInclusive, endInclusive);
    }
    
    
    public override Expr? Infimum() => Start;
    public override Expr? Supremum() => End;
    
    // public override Set Complement(Set universe)
    // {
    //     if (universe.IsR)
    //     {
    //         var a = Interval(Expr.NegInf, Start, false, !StartInclusive);
    //         var b = Interval(End, Expr.Inf, !EndInclusive, false);
    //         return Union(a, b);    
    //     }
    //     
    //     return base.Complement(universe);
    // }

    public override Set Boundary()
    {
        return (Start.IsNegativeInfinity, End.IsInfinity) switch
        {
            (true, true) => EmptySet,
            (true, false) => ArraySet(End),
            (false, true) => ArraySet(Start),
            (false, false) => ArraySet(Start, End)
        };
    }
    
    public override Boolean Contains(Expr x)
    {
        return AsCondition(x);
    }

    public Boolean AsCondition(Expr x)
    {
        var start = StartInclusive ? Boolean.GE(x, Start) : Boolean.G(x, Start);
        var end = EndInclusive ? Boolean.LE(x, End) : Boolean.L(x, End);
        return start & end;
    }
    
    public override string ToString()
    {
        return $"{(StartInclusive ? "[" : "]")}{Start}, {End}{(EndInclusive ? "]" : "[")}";
    }
    
    public override string ToLatex()
    {
        return $"{(StartInclusive ? "[" : "]")}{Start.ToLatex()}, {End.ToLatex()}{(EndInclusive ? "]" : "[")}";
    }

    public override bool IsSubset(Set other)
    {
        if (other is Real)
            return true;

        if (other is IntervalSet inter)
        {
            return Start >= inter.Start && End <= inter.End;
        }
        
        return base.IsSubset(other);
    }

    public override bool IsSuperset(Set other)
    {
        if (other is Real)
            return false;

        if (other is IntervalSet inter)
        {
            return Start <= inter.Start && End >= inter.End;
        }
        
        return base.IsSuperset(other);
    }
}