using ConsoleApp1.Core.Expressions.Atoms;

namespace ConsoleApp1.Parser;

public class LatexExprVisitor : LatexBaseVisitor<Expr>
{
    public override Expr VisitExpr(LatexParser.ExprContext context)
    {
        return Visit(context.addition());
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
    
    public override Expr VisitPower_nofunc(LatexParser.Power_nofuncContext context)
    {
        var result = Visit(context.atom_nofunc(0));
        for (int i = 1; i < context.atom_nofunc().Length; i++)
        {
            result = Pow(result, Visit(context.atom_nofunc(i)));
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
    
    public override Expr VisitAtom_nofunc(LatexParser.Atom_nofuncContext context)
    {
        if (context.parenthesis() != null)
        {
            return Visit(context.parenthesis());
        }
        if (context.number() != null)
        {
            return Visit(context.number());
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

    public override Expr VisitSubexpr(LatexParser.SubexprContext context)
    {
        return context.expr() is not null ? Visit(context.expr()) : Visit(context.atom());
    }

    public override Expr VisitSupexpr(LatexParser.SupexprContext context)
    {
        return context.expr() is not null ? Visit(context.expr()) : Visit(context.atom());
    }

    public override Expr VisitAbs(LatexParser.AbsContext context)
    {
        return Visit(context.expr());
    }

    public override Expr VisitIntfunc(LatexParser.IntfuncContext context)
    {
        var lceil = context.L_CEIL() is not null;
        var rceil = context.R_CEIL() is not null;
        var x = Visit(context.expr());
        
        return (lceil, rceil) switch
        {
            (true, true) => Ceil(x),
            (false, false) => Floor(x),
            (false, true) => Round(x),
            (true, false) => Round(x)
        };
    }

    public override Expr VisitParenthesis(LatexParser.ParenthesisContext context)
    {
        return Visit(context.expr());
    }

    public override Expr VisitNumber(LatexParser.NumberContext context)
    {
        if (context.INFTY() is not null)
            return double.PositiveInfinity;
        
        double value = double.Parse(context.GetText());
        if (double.IsInteger(value))
            return Num((int)value);
        return Num(value);
    }

    public override Expr VisitVariable(LatexParser.VariableContext context)
    {
        if (context.GetText() == "e")
            return Constant.E;
        if (context.GetText() == "\\pi")
            return Constant.PI;
        return Var(context.GetText());
    }

    public override Expr VisitFunc_args(LatexParser.Func_argsContext context)
    {
        throw new Exception();
    }

    public override Expr VisitLetter(LatexParser.LetterContext context)
    {
        throw new Exception();
    }

    public override Expr VisitMul(LatexParser.MulContext context)
    {
        throw new Exception();
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
            { "asin", ASin }, 
            { "acos", ACos }, 
            { "atan", ATan },
            
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

        if (supExpr is not null && supExpr.Is(-1) && ((string[]) ["sin", "cos", "tan"]).Contains(funcName))
        {
            funcName = "arc" + funcName;
            supExpr = null;
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
        
        if (supExpr is not null)
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