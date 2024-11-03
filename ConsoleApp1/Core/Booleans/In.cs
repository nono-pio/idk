using ConsoleApp1.Core.Sets;

namespace ConsoleApp1.Core.Booleans;

public class In : Boolean
{

    public Expr Value;
    public Set Domain;

    public In(Expr value, Set domain)
    {
        Value = value;
        Domain = domain;
    }

    public static Boolean Eval(Expr value, Set domain)
    {
        return new In(value, domain);
    }
    
    public override bool? GetValue()
    {
        return Domain.Contains(Value).GetValue();
    }
}