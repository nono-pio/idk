namespace ConsoleApp1.Core.Propositions;

public class Not : Proposition
{
    
    public Proposition A { get; set; }
    
    public Not(Proposition a)
    {
        A = a;
    }
    
    public override Maybe GetValue() => A.GetValue().Not();
}