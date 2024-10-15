using ConsoleApp1.Core.Booleans;
using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Sets;
using Boolean = ConsoleApp1.Core.Booleans.Boolean;

namespace ConsoleApp1.Core.Solvers;

public class Inequalities
{
    public static Set RelationnalsToSet(Boolean condition, Variable var)
    {
        if (condition is BooleanValue value)
        {
            return value.Value ? Set.R : Set.EmptySet;
        }
        
        if (condition is Or or)
            return Set.CreateUnion(or.Values.Select(b => RelationnalsToSet(b, var)).ToArray());
        if (condition is And and)
            return Set.CreateIntersection(and.Values.Select(b => RelationnalsToSet(b, var)).ToArray());
        if (condition is Not not)
            return Set.CreateComplement(RelationnalsToSet(not.Value, var), Set.R);

        if (condition is Relationnals relationnals)
        {
            if (relationnals.A != var)
                relationnals = relationnals.Swap();
            if (relationnals.A != var)
                throw new Exception("Variable not found in relationnals");

            var b = relationnals.B;
            return relationnals.Relation switch
            {
                Relations.Equal => Set.CreateFiniteSet(b),
                Relations.NotEqual => Set.CreateComplement(Set.CreateFiniteSet(b), Set.R),
                Relations.Greater => Set.CreateInterval(b, Expr.Inf, false, false),
                Relations.GreaterOrEqual => Set.CreateInterval(b, Expr.Inf, true, false),
                Relations.Less => Set.CreateInterval(Expr.NegInf, b, false, false),
                Relations.LessOrEqual => Set.CreateInterval(Expr.NegInf, b, false, true),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        return Set.CreateConditionSet(condition, var, Set.R);
    }
    
    
}