namespace ConsoleApp1.Core.Booleans;

public class And(params Boolean[] values) : Boolean
{
    public Boolean[] Values = values;

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