using ConsoleApp1.core.atoms;
using ConsoleApp1.Core.Expr.Fonction.Trigonometrie;
using ConsoleApp1.core.expr.fonctions;
using ConsoleApp1.core.expr.fonctions.trigonometrie;

namespace ConsoleApp1.core.simplification;

// source: https://www.sciencedirect.com/science/article/pii/S0895717706001609

public class FuAlgo
{
    // <-- Basic Trigonometric Rules -->

    // simplify rational polynomial
    public static Expr TR0(Expr expr)
    {
        return expr; //TODO
    }

    // sec, csc -> sin, cos
    public static Expr TR1(Expr expr)
    {
        expr = expr.Map<Sec>(sec => 1 / Cos(sec.x));
        return expr.Map<Csc>(csc => 1 / Sin(csc.x));
    }

    // tan, cot -> sin, cos
    public static Expr TR2(Expr expr)
    {
        expr = expr.Map<Tan>(tan => Sin(tan.x) / Cos(tan.x));
        return expr.Map<Cot>(cot => Sin(cot.x) / Cos(cot.x));
    }

    // Induced formula
    public static Expr TR3(Expr expr)
    {
        return expr; //TODO
    }

    // Values of special angles
    public static Expr TR4(Expr expr)
    {
        return expr; //TODO
    }

    // Sin^2 -> 1-Cos^2
    public static Expr TR5(Expr expr)
    {
        return expr.Map<Power>(pow =>
        {
            if (pow.Exp == Number.Two && pow.Base is Sin sin)
            {
                var x = sin.x;
                return 1 - Pow(Cos(x), Num(2));
            }

            return pow;
        });
    }

    // Cos^2 -> 1-Sin^2
    public static Expr TR6(Expr expr)
    {
        return expr.Map<Power>(pow =>
        {
            if (pow.Exp == Number.Two && pow.Base is Cos cos)
            {
                var x = cos.x;
                return 1 - Pow(Sin(x), Num(2));
            }

            return pow;
        });
    }

    // Cos^2 -> Cos
    public static Expr TR7(Expr expr)
    {
        return expr.Map<Power>(pow =>
        {
            if (pow.Exp == Number.Two && pow.Base is Cos cos)
            {
                var x = cos.x;
                return (1 + Cos(2 * x)) / 2;
            }

            return pow;
        });
    }

    public static Expr TR8(Expr expr)
    {
        return expr; //TODO
    }

    public static Expr TR9(Expr expr)
    {
        return expr; //TODO
    }

    public static Expr TR10(Expr expr)
    {
        return expr; //TODO
    }

    public static Expr TRN10(Expr expr)
    {
        return expr; //TODO
    }

    // Double Angle
    public static Expr TR11(Expr expr)
    {
        return expr; //TODO
    }

    public static Expr TR12(Expr expr)
    {
        return expr; //TODO
    }

    public static Expr TR13(Expr expr)
    {
        return expr; //TODO
    }

    // <-- 

    public static int L(Expr expr)
    {
        return expr.Count<Sin>() +
               expr.Count<Cos>() +
               expr.Count<Tan>() +
               expr.Count<Sec>() +
               expr.Count<Csc>() +
               expr.Count<Cot>();
    }

    public static Expr TR(int i, Expr expr)
    {
        return i switch
        {
            0 => TR0(expr),
            1 => TR1(expr),
            2 => TR2(expr),
            3 => TR3(expr),
            4 => TR4(expr),
            5 => TR5(expr),
            6 => TR6(expr),
            7 => TR7(expr),
            8 => TR8(expr),
            9 => TR9(expr),
            10 => TR10(expr),
            -10 => TRN10(expr),
            11 => TR11(expr),
            12 => TR12(expr),
            13 => TR13(expr),
            _ => throw new Exception("This Rule doesn't exist")
        };
    }

    public static Expr SmallestLTR(Expr expr)
    {
        var minL = L(expr);
        var min = expr;

        Expr F;
        int LF;

        // TR 0-13
        for (var i = 0; i < 14; i++)
        {
            F = TR(i, expr);
            LF = L(F);
            if (LF < minL)
            {
                minL = LF;
                min = F;
            }
        }

        // TRN10
        F = TR(-10, expr);
        LF = L(F);
        if (LF < minL)
        {
            minL = LF;
            min = F;
        }

        return min;
    }

    // <-- Combination Rules -->

    public static Expr CTR1(Expr expr)
    {
        var F1 = TR0(TR5(expr));
        var F2 = TR0(TR6(expr));

        var LF1 = L(F1);
        var LF2 = L(F2);
        var LF = L(expr);

        if (LF1 < LF && LF1 < LF2) return F1;

        if (LF2 < LF) return F2;

        return expr;
    }


    public static Expr CTR2(Expr expr)
    {
        var F1 = TR5(TR11(expr));
        var F2 = TR6(TR11(expr));
        var F3 = TR11(expr);

        var LF1 = L(F1);
        var LF2 = L(F2);
        var LF3 = L(F3);

        if (LF1 < LF3 && LF1 < LF2) return F1;

        if (LF2 < LF3) return F2;

        return expr;
    }

    public static Expr CTR3(Expr expr)
    {
        var F1 = TR8(expr);
        var F2 = TRN10(TR8(expr));

        var LF1 = L(F1);
        var LF2 = L(F2);
        var LF = L(expr);

        if (LF2 < LF) return F2;

        if (LF1 < LF) return F1;

        return expr;
    }

    public static Expr CTR4(Expr expr)
    {
        var F1 = TRN10(TR4(expr));

        var LF1 = L(F1);
        var LF = L(expr);

        if (LF1 < LF) return F1;

        return expr;
    }

    // <--

    // 4 -> 3 -> 4 -> 12 -> 4 -> 13 -> 4 -> 0
    public static Expr RL1(Expr expr)
    {
        return TR0(TR4(TR13(TR4(TR12(TR4(TR3(TR4(expr))))))));
    }

    // 4 -> 3 -> 10 -> 4 -> 3 -> 11 -> 5 -> 7 -> 11 (->) 4 -> C3 -> 0 -> C1 -> 9 -> C2 -> 4 -> 9 -> 0 -> 9 -> C4
    public static Expr RL2(Expr expr)
    {
        expr = TR11(TR7(TR5(TR11(TR3(TR4(TR10(TR3(TR4(expr)))))))));
        return CTR4(TR9(TR0(TR9(TR4(CTR2(TR9(CTR1(TR0(CTR3(TR4(expr)))))))))));
    }

    // <-- Fu -->
    public static Expr Fu(Expr expr)
    {
        if (expr.Has<Sec>() || expr.Has<Csc>()) expr = TR1(expr);

        if (expr.Has<Tan>() || expr.Has<Cot>()) expr = RL1(expr);

        expr = SmallestLTR(expr);

        if (expr.Has<Tan>() || expr.Has<Cot>()) expr = TR2(expr);

        expr = TR0(expr);

        if (expr.Has<Sin>() || expr.Has<Cos>()) expr = RL2(expr);

        return expr;
    }
}