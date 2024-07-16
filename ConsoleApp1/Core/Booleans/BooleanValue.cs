namespace ConsoleApp1.Core.Booleans;

public class BooleanValue(bool value) : Boolean
{
    public bool Value = value;
    
    public override bool? GetValue() => Value;
    
    public override string ToString() => Value.ToString();
}