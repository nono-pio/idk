namespace ConsoleApp1.Core.Expr;

public abstract class Tranformer : Expr
{
    public string[] Vars;

    protected Tranformer(string[] vars, params Expr[] exprs)
    {
        Vars = vars;
    }
}