namespace ConsoleApp1.Core.Booleans;

public class Equal(Expr a, Expr b) : Boolean
{
    public Expr A = a;
    public Expr B = b;
    
    public override bool? GetValue()
    {
        try
        {
            return A.N() == B.N();
        }
        catch (Exception e)
        {
            return null;
        }
    }
    
    public override string ToString() => $"{A} = {B}";
}