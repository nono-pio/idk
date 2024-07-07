using ConsoleApp1.Core.Models;

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

    public static Variable Var(string name) => new Variable(name);
    public static Variable Var(VariableData data) => new Variable(data);
    public static Variable Var(string name, VariableData data) => new Variable(name, data);
    public static Variable Var(string name, Expr value) => new Variable(name, value);
    public static Variable Var(string name, double value) => new Variable(name, value);
    public static Variable Var(string name, Fonction fonction, Expr of) => new Variable(name, fonction, of);
    
    public static Variable Constant(string name, double value) => Variable.CreateConstant(name, value);

    public static Variable[] Vars(params string[] ids)
    {
        var vars = new Variable[ids.Length];
        for (var i = 0; i < ids.Length; i++) vars[i] = new Variable(ids[i]);

        return vars;
    }
}