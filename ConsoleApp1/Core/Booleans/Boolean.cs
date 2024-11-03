using System.Data.SqlTypes;
using System.Diagnostics;
using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Sets;

namespace ConsoleApp1.Core.Booleans;

public abstract class Boolean
{
    public static implicit operator Boolean(bool value) => value ? True : False;
    
    public static Boolean True => new BooleanValue(true);
    public static Boolean False => new BooleanValue(false);
    
    public bool IsTrue => this is BooleanValue { Value: true };
    public bool IsFalse => this is BooleanValue { Value: false };
    public bool IsIndeterminate => this is not BooleanValue;
    public bool IsDeterminate => this is BooleanValue;
    public bool IsTrueOrFalse => this is BooleanValue;
    public bool IsTrueOrIndeterminate => IsTrue || IsIndeterminate;
    public bool IsFalseOrIndeterminate => IsFalse || IsIndeterminate;

    public virtual Set SolveFor(Variable x) => throw new NotImplementedException();

    public abstract bool? GetValue();
    
    public virtual Boolean Substitue(Variable variable, Expr value) => this;
    
    public static Boolean operator !(Boolean b) => Not(b);
    public static Boolean operator &(Boolean b1, Boolean b2) => And(b1, b2);
    public static Boolean operator |(Boolean b1, Boolean b2) => Or(b1, b2);
    
    public static Boolean Or(params Boolean[] values) => Booleans.Or.Eval(values);
    public static Boolean And(params Boolean[] values) => Booleans.And.Eval(values);
    public static Boolean Not(Boolean value) => new Not(value);
    
    public static Boolean Equal(Expr e1, Expr e2) => Relationnals.Construct(e1, e2, Relations.Equal);
    public static Boolean NotEqual(Expr e1, Expr e2) => Relationnals.Construct(e1, e2, Relations.NotEqual);
    public static Boolean LessThan(Expr e1, Expr e2) => Relationnals.Construct(e1, e2, Relations.Less);
    public static Boolean LessThanOrEqual(Expr e1, Expr e2) => Relationnals.Construct(e1, e2, Relations.LessOrEqual);
    public static Boolean GreaterThan(Expr e1, Expr e2) => Relationnals.Construct(e1, e2, Relations.Greater);
    public static Boolean GreaterThanOrEqual(Expr e1, Expr e2) => Relationnals.Construct(e1, e2, Relations.GreaterOrEqual);
    public static Boolean EQ(Expr e1, Expr e2) => Equal(e1, e2);
    public static Boolean NE(Expr e1, Expr e2) => NotEqual(e1, e2);
    public static Boolean L(Expr e1, Expr e2) => LessThan(e1, e2);
    public static Boolean LE(Expr e1, Expr e2) => LessThanOrEqual(e1, e2);
    public static Boolean G(Expr e1, Expr e2) => GreaterThan(e1, e2);
    public static Boolean GE(Expr e1, Expr e2) => GreaterThanOrEqual(e1, e2);

    public static bool operator true(Boolean cond) => cond.IsTrue;
    public static bool operator false(Boolean cond) => throw new UnreachableException();
    
}