namespace ConsoleApp1.parser;

// generate MathBaseVisitor
// antlr4 -Dlanguage=CSharp -o C:/Users/nolan/RiderProjects/ConsoleApp1/ConsoleApp1/parser\gen -listener -visitor -lib C:/Users/nolan/RiderProjects/ConsoleApp1/ConsoleApp1/parser C:/Users/nolan/RiderProjects/ConsoleApp1/ConsoleApp1/parser\Math.g4

public class MathVisitor : MathBaseVisitor<Expr?>
{
    public override Expr? VisitProg(MathParser.ProgContext context)
    {
        return VisitExpr(context.expr());
    }

    public override Expr? VisitExpr(MathParser.ExprContext context)
    {
        var num = context.INT();
        if (num != null) return Num(double.Parse(num.GetText()));

        return null;
    }
}