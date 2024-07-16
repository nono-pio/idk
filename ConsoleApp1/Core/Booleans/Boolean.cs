namespace ConsoleApp1.Core.Booleans;

public abstract class Boolean
{
    public static implicit operator Boolean(bool value) => value ? True : False;
    
    public static Boolean True => new BooleanValue(true);
    public static Boolean False => new BooleanValue(false);

    public abstract bool? GetValue();
    
    public static Boolean operator !(Boolean b) => new Not(b);
    public static Boolean operator &(Boolean b1, Boolean b2) => new And(b1, b2);
    public static Boolean operator |(Boolean b1, Boolean b2) => new Or(b1, b2);
    
    public static Boolean Or(params Boolean[] values) => new Or(values);
    public static Boolean And(params Boolean[] values) => new And(values);
    public static Boolean Not(Boolean value) => new Not(value);
    
    public static Boolean Equal(Expr e1, Expr e2) => new Equal(e1, e2);
    public static Boolean NotEqual(Expr e1, Expr e2) => !Equal(e1, e2);
    public static Boolean LessThan(Expr e1, Expr e2) => new Less(e1, e2);
    public static Boolean LessThanOrEqual(Expr e1, Expr e2) => new LessOrEqual(e1, e2);
    public static Boolean GreaterThan(Expr e1, Expr e2) => new Greater(e1, e2);
    public static Boolean GreaterThanOrEqual(Expr e1, Expr e2) => new GreaterOrEqual(e1, e2);
    public static Boolean EQ(Expr e1, Expr e2) => Equal(e1, e2);
    public static Boolean NE(Expr e1, Expr e2) => NotEqual(e1, e2);
    public static Boolean L(Expr e1, Expr e2) => LessThan(e1, e2);
    public static Boolean LE(Expr e1, Expr e2) => LessThanOrEqual(e1, e2);
    public static Boolean G(Expr e1, Expr e2) => GreaterThan(e1, e2);
    public static Boolean GE(Expr e1, Expr e2) => GreaterThanOrEqual(e1, e2);
    
    
    
}