using ConsoleApp1.Core.Expressions.Atoms;
using Boolean = ConsoleApp1.Core.Booleans.Boolean;

namespace ConsoleApp1.Core.Sets;

// {value | cond}
public class ConditionSet : Set
{
    public Boolean Condition;
    public Expr Value;
    public Variable[] Variables;
    
    public ConditionSet(Boolean condition, Expr value, Variable[] vars)
    {
        Condition = condition;
        Value = value;
        Variables = vars;
    }
    
    public static Set Construct(Expr value, Boolean condition, Variable[] vars)
    {
        return new ConditionSet(condition, value, vars);
    }
    
    public override Boolean Contains(Expr x)
    {
        // TODO
        return Boolean.False;
    }

    public override string ToLatex()
    {
        return $@"\left{{{Value.ToLatex()} | {Condition.ToString()}\right}}";
    }
}