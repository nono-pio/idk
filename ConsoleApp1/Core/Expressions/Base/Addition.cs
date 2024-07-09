using ConsoleApp1.Core.Classes;
using ConsoleApp1.Latex;

namespace ConsoleApp1.Core.Expressions.Base;

public class Addition : Expr, ICoefGrouping<Addition>
{
    public Addition(params Expr[] therms) : base(therms)
    {
        if (therms.Length < 2) throw new Exception("You must add two or more therms");
    }

    public Expr[] Therms
    {
        get => Args;
        set => Args = value;
    }

    public NumberStruct Identity() => 0;
    public NumberStruct GroupConstant(NumberStruct a, NumberStruct b) => a + b;
    public NumberStruct Absorbent() => NumberStruct.Nan;
    public (NumberStruct, Expr?) AsCoefExpr(Expr expr) => expr.AsMulCoef();
    public Addition FromArrayList(Expr[] exprs) => new Addition(exprs);
    public Expr GroupCoefExpr(NumberStruct coef, Expr expr) => coef.IsOne ? expr : Mul(coef.Expr(), expr);

    public override OrderOfOperation GetOrderOfOperation()
    {
        return OrderOfOperation.Addition;
    }

    public Expr Construct(params Expr[] therms)
    {
        return Add(therms);
    }

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

    public override Expr Inverse(Expr y, int argIndex)
    {
        // a+b : inv(c, 0) -> c-b
        // a+b : inv(c, 1) -> c-a


        var rest = new Expr[Therms.Length - 1]; // 1+
        for (var i = 0; i < Therms.Length; i++)
        {
            if (i == argIndex) continue;

            if (i < argIndex)
                rest[i] = -Therms[i];
            else
                rest[i - 1] = -Therms[i];
        }

        return y + Add(rest);
    }

    public override double N()
    {
        return Therms.Sum(t => t.N());
    }

    public override (Expr, Expr) AsComplex()
    {
        return Therms.Aggregate((Zero, Zero), 
            (complexTuple, therm) => ComplexUtils.Add(complexTuple, therm.AsComplex()));
    }

    public Expr Eval()
    {
        return ICoefGrouping<Addition>.GroupEval(this);
    }

    public override string? ToString()
    {
        return ToLatex();
    }

    public override string? ToLatex()
    {
        if (Therms.Length < 2) throw new Exception("You must add two or more therms");

        var result = ParenthesisLatexIfNeeded(Therms[0]);

        for (var i = 1; i < Therms.Length; i++)
        {
            // TODO : Check if the term is negative : + -> -
            result += Symbols.Add + ParenthesisLatexIfNeeded(Therms[i]);
        }
        
        return result;
    }
}