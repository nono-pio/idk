using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Sets;

namespace ConsoleApp1.Core.Booleans;

public class In : Boolean
{

    public Variable Value;
    public Set Domain;

    public In(Variable value, Set domain)
    {
        Value = value;
        Domain = domain;
    }

    public static Boolean Eval(Variable value, Set domain)
    {
        return new In(value, domain);
    }
    
    public override bool? GetValue()
    {
        return Domain.Contains(Value).GetValue();
    }
}