namespace ConsoleApp1.Core.Booleans;

public class And(params Boolean[] values) : Boolean
{
    public Boolean[] Values = values;

    public static Boolean Eval(IEnumerable<Boolean> values)
    {
        List<Boolean> newValues = new();
        foreach (var value in values)
        {
            if (value.IsFalse)
                return false;            
            newValues.Add(value);
        }

        return newValues.Count switch
        {
            0 => false,
            1 => newValues[0],
            _ => new And(newValues.ToArray())
        };
    }
    
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