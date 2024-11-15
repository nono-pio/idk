using ConsoleApp1.Core.Expressions.Atoms;

namespace ConsoleApp1.Core.Solvers;

public class Period
{
    // return period if there is a period, 0 if f is cste and null if there is no period
    public static Expr? FindPeriod(Expr f, Variable x)
    {
        if (f.Constant(x))
            return 0;
        
        if (f.BasePeriod is not null)
        {
            var p = f.BasePeriod;
            var lin = f.Args[0].AsLinear(x);
            if (lin is null)
                return null;

            return p / lin.Value.MulCoef;
        }

        var argsPeriod = new List<Expr>();
        foreach (var arg in f.Args)
        {
            var period = FindPeriod(arg, x);
            if (period is null)
                return null;
            if (!period.IsNumZero)
                argsPeriod.Add(period);
        }

        return argsPeriod.Count == 0 ? 0 : LcmPeriod(argsPeriod);
    }

    /// lcm of numbers or lcm of numbers * pi
    public static Expr? LcmPeriod(List<Expr> args)
    {
        
        if (args.All(e => e is Number))
            return Expr.Lcm(args);

        if (args.All(e => e.AsMulIndependent(PI) is not null))
            return Expr.Lcm(args.Select(e => e.AsMulIndependent(PI)!)) * PI;

        return null;
    }
}