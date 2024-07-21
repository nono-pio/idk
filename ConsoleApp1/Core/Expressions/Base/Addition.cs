using ConsoleApp1.Core.Classes;
using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Latex;

namespace ConsoleApp1.Core.Expressions.Base;

public class Addition : Expr
{
    public Expr[] Therms => Args;

    public Addition(params Expr[] therms) : base(therms)
    {
        if (therms.Length < 2) 
            throw new Exception("You must add two or more therms");
    }

    # region Constructors / Eval
    
    public static Expr Construct(params Expr[] exprs)
    {
        // Rules
        // 1. x + x = 2x
        // 2. x + 0 = x
        // 3. x + (y + z) = x + y + z
        // 4. Sort the terms
        
        if (exprs.Length == 0)
            return 0;
        if (exprs.Length == 1)
            return exprs[0];

        NumberStruct numbersSum = 0;
        var exprsSum = new List<Expr>();
        foreach (var expr in exprs.SelectMany(e => e is Addition add ? add.Therms : [e]))
        {
            if (expr is Number num)
            {
                numbersSum += num.Num;
                continue;
            }

            var hasCombined = false;
            for (int i = 0; i < exprsSum.Count; i++)
            {
                Expr? combine = Combine(expr, exprsSum[i]);
                if (combine is not null)
                {
                    exprsSum[i] = combine;
                    hasCombined = true;
                    break;
                }
            }
        
            if (!hasCombined)
                exprsSum.Add(expr);
        }

        if (!numbersSum.IsZero)
            exprsSum.Add(Num(numbersSum));

        return exprsSum.Count switch
        {
            0 => 0,
            1 => exprsSum[0],
            _ => new Addition(Sorting.BubbleSort(exprsSum.ToArray()))
        };
    }
    private static Expr? Combine(Expr a, Expr b)
    {
        if (a is not Multiplication && b is not Multiplication)
            return a == b ? 2 * a : null;

        if (a is Multiplication aMul && b is Multiplication bMul)
        {
            var hasANum = aMul.Factors[0] is Number ? 1 : 0;
            var hasBNum = bMul.Factors[0] is Number ? 1 : 0;

            if (aMul.Factors.Length - hasANum != bMul.Factors.Length - hasBNum)
                return null;
        
            var length = aMul.Factors.Length - hasANum;
            for (int i = 0; i < length; i++)
            {
                if (aMul.Factors[i + hasANum] != bMul.Factors[i + hasBNum])
                    return null;
            }
        
            NumberStruct sum = (hasANum == 1 ? ((Number)aMul.Factors[0]).Num : 1) + (hasBNum == 1 ? ((Number)bMul.Factors[0]).Num : 1);

            if (sum == 0)
                return 0;

            if (sum == 1)
                return length == 1 ? aMul.Factors[hasANum] : aMul.NotEval(aMul.Factors[hasANum..]);
        
            var exprs = new Expr[length + 1];

            exprs[0] = Num(sum);
            Array.Copy(aMul.Factors, hasANum, exprs, 1, length);
            return aMul.NotEval(exprs);
        }

        Multiplication mul;
        Expr other;
        if (a is Multiplication aMult)
        {
            mul = aMult;
            other = b;
        }
        else
        {
            mul = (Multiplication)b;
            other = a;
        }

        if (mul.Factors.Length != 2 || mul.Factors[0] is not Number)
            return null;

        if (mul.Factors[1] == other)
        {
            var factor = ((Number)mul.Factors[0]).Num + 1;
            return factor == 0 ? 0 : mul.NotEval([Num(factor), other]);
        }
    
        return null;
    } 
    
    public override Expr Eval(Expr[] args, object[]? objects = null) => Construct(args);
    public override Expr NotEval(Expr[] args, object[]? objects = null) => new Addition(args);
    
    # endregion

    public override OrderOfOperation GetOrderOfOperation()
    {
        return OrderOfOperation.Addition;
    }
    
    # region Derivee

    public override Expr Derivee(string variable)
    {
        var newThermes = new Expr[Therms.Length];
        for (var i = 0; i < Therms.Length; i++) 
            newThermes[i] = Therms[i].Derivee(variable);

        return Add(newThermes);
    }

    public override Expr Derivee(string variable, int n)
    {
        var newThermes = new Expr[Therms.Length];
        for (var i = 0; i < Therms.Length; i++) 
            newThermes[i] = Therms[i].Derivee(variable, n);

        return Add(newThermes);
    }
    
    # endregion

    public override Expr Inverse(Expr y, int argIndex)
    {
        // a+b : inv(c, 0) -> c-b
        // a+b : inv(c, 1) -> c-a

        var rest = new Expr[Therms.Length - 1];
        for (var i = 0; i < Therms.Length; i++)
        {
            if (i == argIndex) 
                continue;

            if (i < argIndex)
                rest[i] = -Therms[i];
            else
                rest[i - 1] = -Therms[i];
        }

        return y + NotEval(rest);
    }

    public override double N()
    {
        return Therms.Sum(t => t.N());
    }

    public override string? ToString()
    {
        var result = ParenthesisIfNeeded(Therms[0]);

        for (var i = 1; i < Therms.Length; i++)
        {
            // TODO : Check if the term is negative : + -> -
            result += '+' + ParenthesisIfNeeded(Therms[i]);
        }
        
        return result;
    }

    public override string ToLatex()
    {
        var result = ParenthesisLatexIfNeeded(Therms[0]);

        for (var i = 1; i < Therms.Length; i++)
        {
            // TODO : Check if the term is negative : + -> -
            result += Symbols.Add + ParenthesisLatexIfNeeded(Therms[i]);
        }
        
        return result;
    }
}