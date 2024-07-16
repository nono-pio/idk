namespace ConsoleApp1.Core.Booleans;

public class Not(Boolean value) : Boolean
{
    public Boolean Value = value;
    
    public override bool? GetValue() => !Value.GetValue();
    
    
    public override string ToString() => Value is Equal equ ? $"{equ.A} != {equ.B}" :$"\u00ac({Value})";
}