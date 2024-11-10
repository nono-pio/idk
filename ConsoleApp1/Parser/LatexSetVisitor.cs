using System.Diagnostics;
using ConsoleApp1.Core.Sets;

namespace ConsoleApp1.Parser;

public class LatexSetVisitor : LatexBaseVisitor<Set>
{
    public override Set VisitSet(LatexParser.SetContext context)
    {
        return Visit(context.unions());
    }

    public override Set VisitUnions(LatexParser.UnionsContext context)
    {
        return Union(context.intersections().Select(Visit).ToArray());
    }

    public override Set VisitIntersections(LatexParser.IntersectionsContext context)
    {
        return Union(context.differences().Select(Visit).ToArray());
    }

    public override Set VisitDifferences(LatexParser.DifferencesContext context)
    {
        var result = Visit(context.set_atom());
        if (context.differences() is null)
            return result;

        return SetDifference(result, Visit(context.differences()));
    }

    public override Set VisitSet_atom(LatexParser.Set_atomContext context)
    {
        if (context.finite_set() is not null)
            return Visit(context.finite_set());
        if (context.conditionnal_set() is not null)
            return Visit(context.conditionnal_set());
        if (context.number_set() is not null)
            return Visit(context.number_set());
        if (context.parenthesis_set() is not null)
            return Visit(context.parenthesis_set());

        throw new UnreachableException();
    }

    public override Set VisitFinite_set(LatexParser.Finite_setContext context)
    {
        var exprVisitor = new LatexExprVisitor();
        return ArraySet(context.expr().Select(exprVisitor.Visit).ToArray());
    }

    public override Set VisitNumber_set(LatexParser.Number_setContext context)
    {
        return context.GetText() switch
        {
            "\\N" => N,
            "\\Z" => Z,
            "\\Q" => Q,
            "\\R" => R,
            "\\empty" => EmptySet,
            _ => throw new UnreachableException()
        };
    }

    public override Set VisitConditionnal_set(LatexParser.Conditionnal_setContext context)
    {
        var exprVisitor = new LatexExprVisitor();
        var exprCondVisitor = new LatexConditionVisitor();
        var expr = exprVisitor.Visit(context.expr());
        var condition = exprCondVisitor.Visit(context.condition());

        throw new NotImplementedException(); // TODO
    }

    public override Set VisitParenthesis_set(LatexParser.Parenthesis_setContext context)
    {
        return Visit(context.set());
    }
}