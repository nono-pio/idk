using ConsoleApp1.Core.Expressions.Atoms;
using Boolean = ConsoleApp1.Core.Booleans.Boolean;

namespace ConsoleApp1.Core.Sets;

public class FiniteSet(params Expr[] elements) : Set
{
    public readonly List<Expr> Elements = elements.ToList();

    public new static Set CreateFiniteSet(params Expr[] elements)
    {
        if (elements.Length == 0)
            return EmptySet;
        
        return new FiniteSet(elements);
    }

    public override bool IsEnumerable => true;
    public override IEnumerable<Expr> GetEnumerable() => Elements;
    
    public override Set Complement(Set universe)
    {
        if (universe is Interval)
        {
            List<Number> nums = new();
            List<Expr> others = new();
            foreach (var e in Elements)
            {
                if (e is Number num)
                    nums.Add(num);
                else
                    others.Add(e);
            }

            if (universe.IsR && nums.Count > 0)
            {
                nums.Sort();
                var intervals = new List<Set>();
                intervals.Add(CreateInterval(Expr.NegInf, nums[0], false, false));
                for (int i = 1; i < nums.Count-1; i++)
                {
                    intervals.Add(CreateInterval(nums[i-1], nums[i], false, false));
                }
                intervals.Add(CreateInterval(nums[^1], Expr.Inf, false, false));
                if (others.Count > 0)
                    return CreateComplement(CreateUnion(intervals.ToArray()), CreateFiniteSet(others.ToArray()));
                else
                    return CreateUnion(intervals.ToArray());
            }
            else if (nums.Count == 0)
            {
                if (others.Count == 0)
                    return universe;
            
                return CreateComplement(universe, CreateFiniteSet(others.ToArray()));
            }    
        }

        if (universe is FiniteSet)
        {
            var hasnt = new List<Expr>();
            var idk = new List<Expr>();
            foreach (Expr value in Elements)
            {
                var has = universe.Contains(value)?.GetValue();
                if (has is null)
                    idk.Add(value);
                else if (has == false)
                    hasnt.Add(value);
            }
            
            var hasntSet = CreateFiniteSet(hasnt.ToArray());
            if (idk.Count > 0)
                return CreateComplement(hasntSet, CreateFiniteSet(idk.ToArray()));
            return hasntSet;
        }
        
        return base.Complement(universe);
    }
    
    public override Boolean? Contains(Expr x)
    {
        return AsCondition(x);
    }
    
    public Boolean AsCondition(Expr x)
    {
        return Boolean.Or(Elements.Select(e => Boolean.EQ(x, e)).ToArray());
    }

    public override Set Boundary() => this;

    public override Expr? Infimum()
    {
        if (Elements.Count == 0)
            return null;
        return Max(Elements.ToArray());
    }
    
    public override Expr? Supremum()
    {
        if (Elements.Count == 0)
            return null;
        return Min(Elements.ToArray());
    }

    public override string ToString()
    {
        return "{" + string.Join(", ", Elements) + "}";
    }
}