using ConsoleApp1.Core.Expressions.Atoms;
using Boolean = ConsoleApp1.Core.Booleans.Boolean;

namespace ConsoleApp1.Core.Sets;

public class ConditionSet : Set
{
    public Boolean Condition;
    public Variable Variable;
    public Set Domain;
    
    public ConditionSet(Boolean condition, Variable variable, Set domain)
    {
        Condition = condition;
        Variable = variable;
        Domain = domain;
    }
    
    public static Set Construct(Variable x, Boolean condition, Set domain)
    {
        if (condition.IsTrue)
            return domain;
        
        return new ConditionSet(condition, x, domain);
    }
    
    public override Boolean Contains(Expr x)
    {
        return Domain.Contains(x) & Condition.Substitue(Variable, x);
    }

    public override string ToLatex()
    {
        throw new NotImplementedException();
    }
}