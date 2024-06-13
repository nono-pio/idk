namespace ConsoleApp1.Core.Propositions;

public class And : Proposition
{
    public Proposition A { get; set; }
    public Proposition B { get; set; }
    
    public And(Proposition a, Proposition b)
    {
        A = a;
        B = b;
    }

    public override Maybe GetValue()
    {
        var a = A.GetValue();
        var b = B.GetValue();
        
        if (a.IsFalse() || b.IsFalse())
            return Maybe.False;
        
        if (a.IsTrue() && b.IsTrue())
            return Maybe.True;
        
        return Maybe.Maybe;
    }
}