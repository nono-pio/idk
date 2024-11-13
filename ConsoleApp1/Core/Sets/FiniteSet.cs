using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Latex;
using Boolean = ConsoleApp1.Core.Booleans.Boolean;

namespace ConsoleApp1.Core.Sets;

public class FiniteSet(HashSet<Expr> elements) : Set
{
    public readonly HashSet<Expr> Elements = elements.ToHashSet();
    
    public override bool IsElementsNatural => Elements.All(el => el.IsNatural);
    public override bool IsElementsInteger =>  Elements.All(el => el.IsInteger);
    public override bool IsElementsRational =>  Elements.All(el => el.IsRational);
    public override bool IsElementsReal =>  Elements.All(el => el.IsReal);
    
    public override bool IsElementsPositive =>  Elements.All(el => el.IsPositive);
    public override bool IsElementsNegative =>  Elements.All(el => el.IsNegative);

    public static Set Construct(params Expr[] elements) => Construct(elements.ToHashSet());
    public static Set Construct(HashSet<Expr> elements)
    {
        elements = elements.Where(e => !e.HasNan()).ToHashSet();
        
        if (elements.Count == 0)
            return EmptySet;
        
        return new FiniteSet(elements);
    }

    public override bool IsEnumerable => true;
    public override IEnumerable<Expr> GetEnumerable() => Elements;
    
    // public override Set Complement(Set universe)
    // {
    //     if (universe is IntervalSet)
    //     {
    //         List<Number> nums = new();
    //         List<Expr> others = new();
    //         foreach (var e in Elements)
    //         {
    //             if (e is Number num)
    //                 nums.Add(num);
    //             else
    //                 others.Add(e);
    //         }
    //
    //         if (universe.IsR && nums.Count > 0)
    //         {
    //             nums.Sort();
    //             var intervals = new List<Set>();
    //             intervals.Add(Interval(Expr.NegInf, nums[0], false, false));
    //             for (int i = 1; i < nums.Count-1; i++)
    //             {
    //                 intervals.Add(Interval(nums[i-1], nums[i], false, false));
    //             }
    //             intervals.Add(Interval(nums[^1], Expr.Inf, false, false));
    //             if (others.Count > 0)
    //                 return CreateComplement(CreateUnion(intervals.ToArray()), CreateFiniteSet(others.ToArray()));
    //             else
    //                 return CreateUnion(intervals.ToArray());
    //         }
    //         else if (nums.Count == 0)
    //         {
    //             if (others.Count == 0)
    //                 return universe;
    //         
    //             return CreateComplement(universe, CreateFiniteSet(others.ToArray()));
    //         }    
    //     }
    //
    //     if (universe is FiniteSet)
    //     {
    //         var hasnt = new List<Expr>();
    //         var idk = new List<Expr>();
    //         foreach (Expr value in Elements)
    //         {
    //             var has = universe.Contains(value)?.GetValue();
    //             if (has is null)
    //                 idk.Add(value);
    //             else if (has == false)
    //                 hasnt.Add(value);
    //         }
    //         
    //         var hasntSet = CreateFiniteSet(hasnt.ToArray());
    //         if (idk.Count > 0)
    //             return CreateComplement(hasntSet, CreateFiniteSet(idk.ToArray()));
    //         return hasntSet;
    //     }
    //     
    //     return base.Complement(universe);
    // }
    
    public override Boolean Contains(Expr x)
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
    
    public override string ToLatex()
    {
        return Symbols.LBraces + string.Join("; ", Elements.Select(e => e.ToLatex())) + Symbols.RBraces;
    }

    public override bool IsSubset(Set other)
    {
        if (other is FiniteSet f)
            return Elements.IsSubsetOf(f.Elements);
        
        return base.IsSubset(other);
    }

    public override bool IsSuperset(Set other)
    {
        if (other is FiniteSet f)
            return Elements.IsSupersetOf(f.Elements);
        
        return base.IsSuperset(other);
    }
}