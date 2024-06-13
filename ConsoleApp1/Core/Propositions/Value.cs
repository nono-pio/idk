namespace ConsoleApp1.Core.Propositions;

public class Value : Proposition
{
    
    public Maybe A { get; set; }
    
    public Value(Maybe value)
    {
        A = value;
    }
    
    public Value(bool value)
    {
        A = value.ToMaybe();
    }

    public override Maybe GetValue() => A;
    
}