namespace ConsoleApp1.Core.Expressions.Atoms;

public static class ConstructorAtoms
{
    // <-- Number Expression -->
    public static Expr Zero => Num(0);
    public static Expr Un => Num(1);
    public static Expr Deux => Num(2);

    public static Expr NUn => Num(-1);

    public static Expr Num(double num)
    {
        return new Number(num);
    }

    public static Expr Var(string id, VariableData? data = null)
    {
        return new Variable(id, data:data);
    }

    public static Variable[] Vars(params string[] ids)
    {
        var vars = new Variable[ids.Length];
        for (var i = 0; i < ids.Length; i++) vars[i] = new Variable(ids[i]);

        return vars;
    }
}