using ConsoleApp1.Core.Sets;

namespace ConsoleApp1.Core.Expressions.Others;

public class SetExpr(Set set) : Expr
{
    public Set Set = set;

    public override object[]? GetArgs() => [Set];

    public static Expr Construct(Set set)
    {
        // TODO
        return new SetExpr(set);
    }

    public override Expr Eval(Expr[] exprs, object[]? objects = null) => Construct((Set)objects[0]);
    public override Expr NotEval(Expr[] exprs, object[]? objects = null) => new SetExpr((Set)objects[0]);

    public static SetExpr BinaryOnFinite(FiniteSet a, FiniteSet b, Func<Expr, Expr, Expr> op)
    {
        var aCount = a.Elements.Count;
        var bCount = b.Elements.Count;
        if (aCount == 0 || bCount == 0)
        {
            return new SetExpr(Set.EmptySet);
        }
        
        var exprs = new Expr[aCount * bCount];
        var i = 0;
        foreach (var a_i in a.Elements)
        {
            foreach (var b_j in b.Elements)
            {
                exprs[i] = op(a_i, b_j);
                i++;
            }
        }
        
        return new SetExpr(Set.CreateFiniteSet(exprs));
    }
    
    public static Expr Add(Set a, Set b)
    {
        // Empty + Set
        if (a.IsEmpty || b.IsEmpty)
            return Construct(Set.EmptySet);
        
        // Universal + Set
        if (a is UniversalSet || b is UniversalSet)
            return Construct(Set.U);

        // BNS + BNS
        if (a is BasicNumberSet bnsA && b is BasicNumberSet bnsB)
            return bnsA._Level > bnsB._Level ? Construct(bnsA) : Construct(bnsB);
        
        // BNS + Interval
        // TODO
        
        // Interval + Interval
        // TODO
        
        // Finite + Finite
        if (a is FiniteSet finiteA && b is FiniteSet finiteB)
            return BinaryOnFinite(finiteA, finiteB, (x,y) => x+y);
        
        // Finite + Set
        // TODO
        

        throw new NotImplementedException();
    }
    
    
    public override string ToLatex()
    {
        return set.ToLatex();
    }

    public override double N() => double.NaN;

    public override Expr Reciprocal(Expr y, int argIndex)
    {
        throw new NotImplementedException();
    }

    public override Expr Derivee(string variable)
    {
        throw new NotImplementedException();
    }
}