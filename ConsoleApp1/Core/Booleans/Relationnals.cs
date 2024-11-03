using System.Diagnostics;
using ConsoleApp1.Core.Equations;
using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Sets;
using ConsoleApp1.Core.Solvers;

namespace ConsoleApp1.Core.Booleans;

public enum Relations
{
    Equal,
    NotEqual,
    Greater,
    GreaterOrEqual,
    Less,
    LessOrEqual
}

public class Relationnals : Boolean
{
    public Expr A;
    public Expr B;
    public Relations Relation;
    
    public Relationnals(Expr a, Expr b, Relations relation)
    {
        A = a;
        B = b;
        Relation = relation;
    }
    
    public static Boolean Construct(Expr a, Expr b, Relations relation)
    {
        var dif = a - b;

        var equal = dif.IsZero || a.Equals(b);
        
        switch (relation)
        {
            case Relations.Equal:
                if (equal)
                    return true;
                break;
            case Relations.NotEqual:
                if (equal)
                    return false;
                if (dif is Number && !dif.IsNumZero)
                    return true;
                break; // TODO
            case Relations.Greater:
                if (dif.IsPositive && !equal)
                    return true;
                if (dif.IsNegative || equal)
                    return false;
                break;
            case Relations.GreaterOrEqual:
                if (dif.IsPositive || equal)
                    return true;
                if (dif.IsNegative && !equal)
                    return false;
                break;
            case Relations.Less:
                if (dif.IsNegative && !equal)
                    return true;
                if (dif.IsPositive || equal)
                    return false;
                break;
            case Relations.LessOrEqual:
                if (dif.IsNegative || equal)
                    return true;
                if (dif.IsPositive && !equal)
                    return false;
                break;
        }
        
        return new Relationnals(a, b, relation);
    }

    public override Set SolveFor(Variable x)
    {
        return Relation switch
        {
            Relations.Equal => Solve.SolveFor(A, B, x) ?? throw new NotImplementedException(),
            Relations.NotEqual => Complement(Solve.SolveFor(A, B, x) ?? throw new NotImplementedException()),
            Relations.Greater => Inequalities.SolveFor(A, B, InequationType.GreaterThan, x),
            Relations.GreaterOrEqual => Inequalities.SolveFor(A, B, InequationType.GreaterThanOrEqual, x),
            Relations.Less => Inequalities.SolveFor(A, B, InequationType.LessThan, x),
            Relations.LessOrEqual => Inequalities.SolveFor(A, B, InequationType.LessThanOrEqual, x),
            _ => throw new UnreachableException()
        };
    }

    public Relationnals Swap()
    {
        var a = B;
        var b = A;
        Relations rel = Relation;
        
        switch (Relation)
        {
            case Relations.Greater:
                rel = Relations.Less;
                break;
            case Relations.GreaterOrEqual:
                rel = Relations.LessOrEqual;
                break;
            case Relations.Less:
                rel = Relations.Greater;
                break;
            case Relations.LessOrEqual:
                rel = Relations.GreaterOrEqual;
                break;
        }
        
        return new Relationnals(a, b, rel);
    }
    
    public static string GetRelationSign(Relations relation) => relation switch
    {
        Relations.Equal => "==",
        Relations.NotEqual => "!=",
        Relations.Greater => ">",
        Relations.GreaterOrEqual => ">=",
        Relations.Less => "<",
        Relations.LessOrEqual => "<=",
        _ => throw new ArgumentOutOfRangeException(nameof(relation), relation, null)
    };

    public override Boolean Substitue(Variable variable, Expr value)
    {
        return Construct(A.Substitue(variable, value), B.Substitue(variable, value), Relation);
    }

    public override bool? GetValue()
    {
        return Relation switch
        {
            Relations.Equal => A.N() == B.N(),
            Relations.NotEqual => A.N() != B.N(),
            Relations.Greater => A.N() > B.N(),
            Relations.GreaterOrEqual => A.N() >= B.N(),
            Relations.Less => A.N() < B.N(),
            Relations.LessOrEqual => A.N() <= B.N(),
            _ => null
        };
    }
    
    public override string ToString()
    {
        return $"{A} {GetRelationSign(Relation)} {B}";
    }
}