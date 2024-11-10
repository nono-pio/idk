using Antlr4.Runtime;
using ConsoleApp1.Core.Sets;
using ConsoleApp1.Latex;
using Boolean = ConsoleApp1.Core.Booleans.Boolean;

namespace ConsoleApp1.Parser;

public class Parser
{
    public static Expr? ParseExpr(string input)
    {
        try
        {
            var inputStream = new AntlrInputStream(input);
            var lexer = new LatexLexer(inputStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new LatexParser(tokenStream);

            var context = parser.expr();

            var visitor = new LatexExprVisitor();
            return visitor.Visit(context);
        }
        catch (Exception e)
        {
            return null;
        }
        
    }
    
    public static Set? ParseSet(string input)
    {
        try
        {
            var inputStream = new AntlrInputStream(input);
            var lexer = new LatexLexer(inputStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new LatexParser(tokenStream);

            var context = parser.set();

            var visitor = new LatexSetVisitor();
            return visitor.Visit(context);
        }
        catch (Exception e)
        {
            return null;
        }
    }
    
    public static Boolean? ParseBoolean(string input)
    {
        try
        {
            var inputStream = new AntlrInputStream(input);
            var lexer = new LatexLexer(inputStream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new LatexParser(tokenStream);

            var context = parser.condition();

            var visitor = new LatexConditionVisitor();
            return visitor.Visit(context);
        }
        catch (Exception e)
        {
            return null;
        }
    }

    public static bool IsLetter(string input)
    {
        return (input.Length == 1 && char.IsLetter(input[0])) || IsGreekLetter(input);
    }

    public static bool IsGreekLetter(string input)
    {
        return Symbols.GreekLetters.Contains(input);
    }
}