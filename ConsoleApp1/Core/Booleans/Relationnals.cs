using ConsoleApp1.Core.Expressions.Atoms;

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
        switch (relation)
        {
            case Relations.Equal:
                return dif.IsZero;
            case Relations.NotEqual:
                return !dif.IsZero;
            case Relations.Greater:
                return dif.IsPositive;
            case Relations.GreaterOrEqual:
                return dif.IsPositive || dif.IsZero;
            case Relations.Less:
                return dif.IsNegative;
            case Relations.LessOrEqual:
                return dif.IsNegative || dif.IsZero;
            default:
                throw new ArgumentOutOfRangeException(nameof(relation), relation, null);
        }
        
        return new Relationnals(a, b, relation);
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