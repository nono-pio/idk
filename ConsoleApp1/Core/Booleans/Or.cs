using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Sets;

namespace ConsoleApp1.Core.Booleans;

public class Or(params Boolean[] values) : Boolean
{
    public Boolean[] Values = values;

    public static Boolean Eval(IEnumerable<Boolean> values)
    {
        List<Boolean> newValues = new();
        bool hasFalse = false;
        foreach (var value in values)
        {
            if (value.IsTrue)
                return true;
            if (value.IsFalse)
            {
                hasFalse = true;
                continue;
            }
            newValues.Add(value);
        }
        
        return newValues.Count switch
        {
            0 => !hasFalse,
            1 => newValues[0],
            _ => new Or(newValues.ToArray())
        };
    }
    
    public override Set SolveFor(Variable x) => Union(Values.Select(v => v.SolveFor(x)).ToArray());

    
    public override bool? GetValue()
    {
        foreach (var value in Values)
        {
            if (value.GetValue() == null)
                return null;
            if (value.GetValue() == true)
                return true;
        }

        return false;
    }
    
    public override string ToString()
    {
        return string.Join(" \u2228 ", Values.Select(v => v.ToString()));
    }
}