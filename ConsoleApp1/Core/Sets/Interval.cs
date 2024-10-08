﻿using Boolean = ConsoleApp1.Core.Booleans.Boolean;

namespace ConsoleApp1.Core.Sets;

public class Interval : Set
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

    private Interval(Expr start, Expr end, bool startInclusive, bool endInclusive)
    {
        Start = start;
        End = end;
        StartInclusive = startInclusive;
        EndInclusive = endInclusive;
    }

    public Set ArithmeticAdd(Interval other)
    {
        return CreateInterval(Start + other.Start, End + other.End, StartInclusive || other.StartInclusive, EndInclusive || other.EndInclusive);
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

    public Set UnionSelf(Interval other)
    {
        throw new NotImplementedException();
    }
    
    public Boolean IsOverlapping(Interval other)
    {
        throw new NotImplementedException();
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
            (true, true) => EmptySet,
            (true, false) => CreateFiniteSet(End),
            (false, true) => CreateFiniteSet(Start),
            (false, false) => CreateFiniteSet(Start, End)
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
}