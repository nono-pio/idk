namespace ConsoleApp1.Core.Booleans;

public class Or(params Boolean[] values) : Boolean
{
    public Boolean[] Values = values;

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