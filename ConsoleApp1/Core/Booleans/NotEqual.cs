namespace ConsoleApp1.Core.Booleans;

public class NotEqual : Boolean
{
    
    public Expr A;
    public Expr B;
    
    public NotEqual(Expr a, Expr b)
    {
        A = a;
        B = b;
    }
    
    public override bool? GetValue()
    {
        try
        {
            return A.N() != B.N();
        }
        catch (Exception e)
        {
            return null;
        }
    }
}