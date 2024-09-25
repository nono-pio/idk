using System.Diagnostics;
using ConsoleApp1.Core.Expressions.Atoms;
using Boolean = ConsoleApp1.Core.Booleans.Boolean;

namespace ConsoleApp1.Core.Expressions.Others;

public class Piecewise : Expr
{
    public override bool IsNatural => Values.All(el => el.value.IsNatural);
    public override bool IsInteger => Values.All(el => el.value.IsInteger);
    public override bool IsRational => Values.All(el => el.value.IsRational);
    public override bool IsReal => Values.All(el => el.value.IsReal);
    public override bool IsComplex => Values.All(el => el.value.IsComplex);

    public override bool IsPositive => Values.All(el => el.value.IsPositive);
    public override bool IsNegative => Values.All(el => el.value.IsNegative);

    public override bool IsZero => Values.All(el => el.value.IsZero);

    public (Expr value, Boolean condition)[] Values;

    public Piecewise((Expr value, Boolean condition)[] values)
    {
        Values = values;
    }
    
    public static Expr Eval((Expr value, Boolean condition)[] values)
    {

        foreach (var (value, cond) in values)
        {
            if (cond.IsTrue)
                return value;
        }
        
        return new Piecewise(values);
    }

    public override object[]? GetArgs()
    {
        return Values.Select(val => (object)val.condition).ToArray();
    }

    public override Expr Eval(Expr[] exprs, object[]? objects = null)
    {
        Debug.Assert(exprs.Length == objects.Length);
        
        return Eval(exprs.Select((expr, i) => (expr, (Boolean)objects[i])).ToArray());
    }

    public override Expr NotEval(Expr[] exprs, object[]? objects = null)
    {
        
        Debug.Assert(exprs.Length == objects.Length);
        
        return new Piecewise(exprs.Select((expr, i) => (expr, (Boolean)objects[i])).ToArray());
    }

    public override string ToLatex()
    {
        throw new NotImplementedException();
    }

    public override double N()
    {
        foreach (var (value, condition) in Values)
        {
            // TODO: condition.N.IsTrue
            if (condition.IsTrue)
                return value.N();
        }
        
        return Double.NaN;
    }

    public override Expr Reciprocal(Expr y, int argIndex)
    {
        throw new NotImplementedException();
    }

    public override Expr Derivee(Variable variable)
    {
        throw new NotImplementedException();
    }
}