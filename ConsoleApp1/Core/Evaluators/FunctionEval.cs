using ConsoleApp1.Core.Expressions.Atoms;
using Boolean = ConsoleApp1.Core.Booleans.Boolean;

namespace ConsoleApp1.Core.Evaluators;

public class FunctionEval
{
    /// x -> f(x)
    public Func<Expr, Expr> Constructor;
    /// f(key) = value
    /// if not in key than not eval
    public Dictionary<Expr, Expr> SpecialsValues;
    /// a func that return the f(e) where e is the arg
    /// if f(e) can not be eval return null
    public Func<Expr, Expr?> SpecialValueRule;
    /// f(NaN) = NaN
    public bool IfNanReturnNan;
    /// f(+-oo) = NaN
    public bool IfInfReturnNan;
    /// f(Float) = Float
    public bool IfFloatReturnFloat;
    /// the app of the func for a double x
    public Func<double, double> NumFunc;
    /// f(-x) = f(x)
    public bool IsEven;
    /// f(-x) = -f(x)
    public bool IsOdd;
    /// if true get the - sign out of the func
    public bool EvalOdd;
    /// f(1/2) = NaN
    public bool IfNotIntegerReturnNaN;
    /// f(-1) = NaN
    public bool IfNotNaturalReturnNaN;
    /// if cond(x) == true then f(x) = NaN
    public Func<Expr, Boolean> NaNCondition;

    public FunctionEval(
        Func<Expr, Expr> constructor,
        Dictionary<Expr, Expr>? specialsValues = null, 
        Func<Expr, Expr?>? specialValueRule = null,
        bool ifNanReturnNan = true, 
        bool ifInfReturnNan = true, 
        bool ifFloatReturnFloat = true,
        Func<double, double>? numFunc = null, 
        bool isEven = false, bool isOdd = false, 
        bool evalOdd = true,
        bool ifNotIntegerReturnNaN = false, 
        bool ifNotNaturalReturnNaN = false,
        Func<Expr, Boolean>? naNCondition = null
        )
    {
        Constructor = constructor;
        SpecialsValues = specialsValues is null ? new(new Expr.ExprQuickComparer()) : new(specialsValues, new Expr.ExprQuickComparer());
        SpecialValueRule = specialValueRule ?? (_ => null);
        IfNanReturnNan = ifNanReturnNan;
        IfInfReturnNan = ifInfReturnNan;
        IfFloatReturnFloat = ifFloatReturnFloat;
        NumFunc = numFunc ?? (_ => throw new ArgumentException("The Evaluator of the function hasn't a numFunc defined"));
        IsEven = isEven;
        IsOdd = isOdd;
        EvalOdd = evalOdd;
        IfNotIntegerReturnNaN = ifNotIntegerReturnNaN;
        IfNotNaturalReturnNaN = ifNotNaturalReturnNaN;
        NaNCondition = naNCondition ?? (_ => false);
    }

    public double N(Expr x) => N(x.N());
    public double N(double x)
    {
        x = NumFunc(x);

        if (IfNanReturnNan && double.IsNaN(x))
            return double.NaN;
        
        if (IfInfReturnNan && double.IsInfinity(x))
            return double.NaN;

        return x;
    }

    public Expr NotEval(Expr[] exprs, object[]? objects = null) => NotEval(exprs[0]);
    public Expr NotEval(Expr expr) => Constructor(expr);
    
    public Expr Eval(Expr[] exprs, object[]? objects = null) => Eval(exprs[0]);
    public Expr Eval(Expr expr)
    {
        // Nan checks
        if (IfNanReturnNan && expr.IsNaN)
            return double.NaN;

        if (IfInfReturnNan && expr.IsNumInf)
            return double.NaN;

        if (IfNotIntegerReturnNaN && !expr.IsInteger && expr.IsRational)
            return double.NaN;
        
        if (IfNotNaturalReturnNaN && !expr.IsNatural && expr.IsInteger)
            return double.NaN;

        if (NaNCondition(expr).IsTrue)
            return double.NaN;
        
        // Simplify Even and Odd
        if (IsEven && expr.CanRemoveNegativeSign())
            expr = -expr;
        if (IsOdd && EvalOdd && expr.CanRemoveNegativeSign())
            return -Eval(-expr);
        
        // Specials Values
        if (SpecialsValues.TryGetValue(expr, out var val))
            return val;

        val = SpecialValueRule(expr);
        if (val is not null)
            return val;

        if (IfFloatReturnFloat && expr is Number num && num.Num.IsFloat)
            return NumFunc(num.Num.FloatValue);

        return Constructor(expr);
    }
}