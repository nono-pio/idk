using System.Diagnostics;
using ConsoleApp1.Core.Booleans;
using ConsoleApp1.Core.Expressions.Atoms;
using Boolean = ConsoleApp1.Core.Booleans.Boolean;

namespace ConsoleApp1.Parser;

public class LatexConditionVisitor : LatexBaseVisitor<Boolean>
{
    public override Boolean VisitCondition(LatexParser.ConditionContext context)
    {
        return Visit(context.or());
    }

    public override Boolean VisitBoolean(LatexParser.BooleanContext context)
    {
        return Visit(context.or());
    }

    public override Boolean VisitOr(LatexParser.OrContext context)
    {
        return Boolean.Or(context.and().Select(Visit).ToArray());
    }

    public override Boolean VisitAnd(LatexParser.AndContext context)
    {
        return Boolean.And(context.not().Select(Visit).ToArray());
    }

    public override Boolean VisitNot(LatexParser.NotContext context)
    {
        var boolean = Visit(context.cond_atom());

        return context.NOT().Aggregate(boolean, (current, _) => Boolean.Not(current));
    }

    public override Boolean VisitCond_atom(LatexParser.Cond_atomContext context)
    {
        if (context.parenthesis_cond() is not null)
            return Visit(context.parenthesis_cond());
        if (context.bool_value() is not null)
            return Visit(context.bool_value());
        if (context.in_set() is not null)
            return Visit(context.in_set());
        if (context.relationnal() is not null)
            return Visit(context.relationnal());

        throw new UnreachableException();
    }

    public override Boolean VisitBool_value(LatexParser.Bool_valueContext context)
    {
        if (context.TRUE() is not null)
            return true;
        if (context.FALSE() is not null)
            return false;

        throw new UnreachableException();
    }

    public override Boolean VisitRelationnal(LatexParser.RelationnalContext context)
    {
        var exprVisitor = new LatexExprVisitor();
        var a = exprVisitor.Visit(context.expr(0));
        var b = exprVisitor.Visit(context.expr(1));

        var rel = context.REL_SIGN().GetText() switch
        {
            "=" => Relations.Equal,
            "<" => Relations.Less,
            ">" => Relations.Greater,
            "<=" => Relations.LessOrEqual,
            ">=" => Relations.GreaterOrEqual,
            "!=" => Relations.NotEqual,
            "\\leq" => Relations.LessOrEqual,
            "\\geq" => Relations.GreaterOrEqual,
            "\\neq" => Relations.NotEqual,
            "\\ne" => Relations.NotEqual,
            _ => throw new UnreachableException()
        };

        return Relationnals.Construct(a, b, rel);
    }

    public override Boolean VisitIn_set(LatexParser.In_setContext context)
    {
        var exprVisitor = new LatexExprVisitor();
        var setVisitor = new LatexSetVisitor();

        var var = exprVisitor.Visit(context.variable()) as Variable;

        if (var is null)
            throw new UnreachableException();

        var set = setVisitor.Visit(context.set());

        return In.Eval(var, set);
    }

    public override Boolean VisitParenthesis_cond(LatexParser.Parenthesis_condContext context)
    {
        return Visit(context.condition());
    }
}