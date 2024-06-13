namespace ConsoleApp1.Core.Sets;

public class Union : Set
{
    
    public Set[] Sets;
    
    public Union(params Set[] sets)
    {
        if (sets.Length == 0)
        {
            throw new Exception("Union of no sets is not allowed.");
        }
        Sets = sets;
    }

    /*
    
    Rules : 
    1. Empty set : A U {} = A
    2. Union of Union : A U (B U C) = A U B U C
    3. Universal set U : U U A = U
    4. Concat FiniteSet : [1,2] U [3,4] = [1,2,3,4]
    5. Concat Interval : [1,2] U [2,3] = [1,3]
    6. TODO ... 
    
    */
    public static Set ConstructEval(params Set[] sets)
    {
        
        switch (sets.Length)
        {
            case 0:
                return EmptySet();
            case 1:
                return sets[0];
        }

        var newSets = new List<Set>();
        var concatFiniteSets = new List<double>();
        foreach (var set in GetEnumerableUnionSets(sets)) // 2. Union of Union
        {

            if (set.IsEmpty()) // 1. Empty set
                continue;
            
            switch (set)
            {
                case UniversalSet: // 2. Concat FiniteSet
                    return set;
                
                case FiniteSet finiteSet: // 4. Concat FiniteSet
                    concatFiniteSets.AddRange(finiteSet.Elements);
                    continue;
                
                case Interval interval: // 5. Concat Interval
                    // TODO ...
                    continue;
                
                default: // 6. TODO ...
                    newSets.Add(set);
                    break;
            }
        }
        
        if (concatFiniteSets.Count > 0)
        {
            newSets.Add(new FiniteSet(concatFiniteSets.ToArray()));
        }

        return newSets.Count switch
        {
            0 => EmptySet(),
            1 => newSets[0],
            _ => new Union(newSets.ToArray())
        };
    }
    
    public override long? Length()
    {
        
        long length = 0;
        foreach (var set in Sets)
        {
            var setLength = set.Length();
            if (setLength is null)
                return null;
            
            length += setLength.Value;
        }

        return length;
    }

    public override bool IsEnumerable()
    {
        return Sets.All(set => set.IsEnumerable());
    }

    public override IEnumerable<double> GetEnumerable()
    {
        return Sets.SelectMany(set => set.GetEnumerable());
    }

    public override double Max()
    {
        
        if (Sets.Length == 0)
            return double.NaN;

        double max = double.NegativeInfinity;
        foreach (var set in Sets)
        {
            var setMax = set.Max();
            if (setMax > max)
                max = setMax;
        }
        
        return max;
    }

    public override double Min()
    {
        if (Sets.Length == 0)
            return double.NaN;
        
        double min = double.PositiveInfinity;
        foreach (var set in Sets)
        {
            var setMin = set.Min();
            if (setMin < min)
                min = setMin;
        }
        
        return min;
    }

    public override double PrincipalValue()
    {
        if (Sets.Length == 0)
            return double.NaN;
        
        return Sets[0].PrincipalValue();
    }

    public override bool Contain(double x)
    {
        return Sets.Any(set => set.Contain(x));
    }
}