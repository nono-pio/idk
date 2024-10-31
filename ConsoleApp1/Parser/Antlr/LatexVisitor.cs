using ConsoleApp1.Core.Expressions.Atoms;

namespace ConsoleApp1.Parser.Antlr;

public class LatexVisitor : LatexBaseVisitor<Expr>
{
    public override Expr VisitProgram(LatexParser.ProgramContext context)
    {
        return Visit(context.expr());
    }

    public override Expr VisitAddition(LatexParser.AdditionContext context)
    {
        Expr result = Visit(context.multiplication(0));
        for (int i = 1; i < context.multiplication().Length; i++)
        {
            Expr right = Visit(context.multiplication(i));
            if (context.ADD(i - 1) != null)
            {
                result = Add(result, right);
            }
            else if (context.SUB(i - 1) != null)
            {
                result = Sub(result, right);
            }
        }
        return result;
    }

    public override Expr VisitMultiplication(LatexParser.MultiplicationContext context)
    {
        Expr result = Visit(context.unary(0));
        for (int i = 1; i < context.unary().Length; i++)
        {
            Expr right = Visit(context.unary(i));
            if (context.mul(i - 1) != null)
            {
                result = Mul(result, right);
            }
            else if (context.DIV(i - 1) != null)
            {
                result = Div(result, right);
            }
        }
        return result;
    }

    public override Expr VisitUnary(LatexParser.UnaryContext context)
    {

        var results = new Expr[context.unarysufix().Length];
        for (int i = 0; i < results.Length; i++)
        {
            results[i] = Visit(context.unarysufix(i));
        }
        var result = Mul(results);
        
        
        if (context.SUB() != null)
        {
            result = Neg(result);
        }
        return result;
    }

    public override Expr VisitUnarysufix(LatexParser.UnarysufixContext context)
    {
        Expr result = Visit(context.power());
        // foreach (var suffix in context.suffix())
        // {
        //     result = Fac(result);
        // }
        return result;
    }

    public override Expr VisitPower(LatexParser.PowerContext context)
    {
        var result = Visit(context.atom(0));
        for (int i = 1; i < context.atom().Length; i++)
        {
            result = Pow(result, Visit(context.atom(i)));
        }
        return result;
    }

    public override Expr VisitAtom(LatexParser.AtomContext context)
    {
        if (context.parenthesis() != null)
        {
            return Visit(context.parenthesis());
        }
        if (context.number() != null)
        {
            return Visit(context.number());
        }
        if (context.function() != null)
        {
            return Visit(context.function());
        }
        if (context.variable() != null)
        {
            return Visit(context.variable());
        }
        if (context.abs() != null)
        {
            return Visit(context.abs());
        }
        if (context.intfunc() != null)
        {
            return Visit(context.intfunc());
        }
        if (context.frac() != null)
        {
            return Visit(context.frac());
        }
        if (context.sqrt() != null)
        {
            return Visit(context.sqrt());
        }
        throw new NotImplementedException("Unknown atom type");
    }

    public override Expr VisitParenthesis(LatexParser.ParenthesisContext context)
    {
        return Visit(context.expr());
    }

    public override Expr VisitNumber(LatexParser.NumberContext context)
    {
        double value = double.Parse(context.GetText());
        return Num(value);
    }

    public override Expr VisitVariable(LatexParser.VariableContext context)
    {
        return Var(context.GetText());
    }

    public override Expr VisitFunction(LatexParser.FunctionContext context)
    {
        string funcName = context.FUNC_NAME()?.GetText() ?? context.letter().GetText();
        
        Expr[] arguments;
        if (context.func_args() is not null)
            arguments = context.func_args().expr().Select(exprCtx => Visit(exprCtx)).ToArray(); 
        else if (context.power_nofunc() is not null)
            arguments = [Visit(context.power_nofunc())];
        else
            throw new Exception("No arguments");

        Expr? subExpr = context.subexpr() is not null ? Visit(context.subexpr()) : null;
        Expr? supExpr = context.supexpr() is not null ? Visit(context.supexpr()) : null;
        
        var univarianteFuncNoSubscripts = new Dictionary<string, Func<Expr, Expr>>()
        {
            { "sin", Sin }, 
            { "cos", Cos }, 
            { "tan", Tan }, 
            { "arcsin", ASin }, 
            { "arccos", ACos }, 
            { "arctan", ATan },
            
            // { "sinh", Sinh }, 
            // { "cosh", Cosh }, 
            // { "tanh", Tanh }, 
            
            { "ln", Ln },
            { "log", Log },
            { "exp", Exp },
        
            { "im", Im },
            { "re", Re },
        
            { "sign", Sign },
            { "abs", Abs },
            { "floor", Floor },
            { "ceil", Ceil },
            { "round", Round },
            
            { "sqrt", Sqrt },
            { "cbrt", Cbrt },
        };

        var multivariateFuncNoSubscripts = new Dictionary<string, Func<Expr[], Expr>>()
        {
            { "max", Max },
            { "min", Min },
        };
        
        var oldName = funcName;
        if (funcName[0] == '\\')
            funcName = funcName[1..];
        funcName = funcName.ToLower();

        if (supExpr is not null && supExpr == -1 && ((string[]) ["sin", "cos", "tan"]).Contains(funcName))
        {
            funcName = "arc" + funcName;
            supExpr *= -1;
        }
        
        Expr? result = null;
        if (subExpr is null)
        {
            if (univarianteFuncNoSubscripts.ContainsKey(funcName))
                result = univarianteFuncNoSubscripts[funcName](arguments[0]);
            
            if (multivariateFuncNoSubscripts.ContainsKey(funcName))
                result = multivariateFuncNoSubscripts[funcName](arguments);
        }
        else // subscript cases
        {
            if (funcName == "log")
                result = Log(arguments[0], subExpr);
            if (funcName == "exp")
                result = Pow(subExpr, arguments[0]);
        }

        if (result is null && Parser.IsLetter(oldName))
        {
            result = new UndefineFunction(oldName, arguments);
        }

        if (result is null)
            throw new Exception("This is not a function");
        
        if (supExpr is not null && supExpr != 1)
            result = Pow(result, supExpr);

        return result;
    }

    public override Expr VisitFrac(LatexParser.FracContext context)
    {
        Expr numerator = Visit(context.expr(0));
        Expr denominator = Visit(context.expr(1));
        return Div(numerator, denominator);
    }

    public override Expr VisitSqrt(LatexParser.SqrtContext context)
    {
        if (context.expr().Length == 2)
        {
            var root = Visit(context.expr(0));
            var value = Visit(context.expr(1));
            return Sqrt(value, n: root);    
        }
        
        var x = Visit(context.expr(0));
        return Sqrt(x);
    }
}