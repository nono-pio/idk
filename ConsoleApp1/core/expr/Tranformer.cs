namespace ConsoleApp1.core.expr;

public abstract class Tranformer : Expr
{
    public string[] Vars;

    protected Tranformer(string[] vars, params Expr[] exprs)
    {
        Vars = vars;
    }
}