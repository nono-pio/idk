using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Sets;

namespace ConsoleApp1.Core.Booleans;

public class And(params Boolean[] values) : Boolean
{
    public Boolean[] Values = values;

    public static Boolean Eval(IEnumerable<Boolean> values)
    {
        List<Boolean> newValues = new();
        bool hasTrue = false;
        foreach (var value in values)
        {
            if (value.IsFalse)
                return false;
            if (value.IsTrue)
            {
                hasTrue = true;
                continue;
            }
            newValues.Add(value);
        }

        return newValues.Count switch
        {
            0 => hasTrue,
            1 => newValues[0],
            _ => new And(newValues.ToArray())
        };
    }

    public override Set SolveFor(Variable x) => Intersection(Values.Select(v => v.SolveFor(x)).ToArray());

    public override bool? GetValue()
    {
        foreach (var value in Values)
        {
            if (value.GetValue() == null)
                return null;
            if (value.GetValue() == false)
                return false;
        }

        return true;
    }
    
    public override string ToString()
    {
        return string.Join(" \u2228 ", Values.Select(v => v.ToString()));
    }
}