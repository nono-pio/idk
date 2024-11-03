using Antlr4.Runtime;
using ConsoleApp1.Latex;
using ConsoleApp1.Parser.Antlr;

namespace ConsoleApp1.Parser;

public class Parser
{
    public static Expr? Parse(string input)
    {
        var inputStream = new AntlrInputStream(input);
        var lexer = new LatexLexer(inputStream);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new LatexParser(tokenStream);

        var context = parser.expr();

        var visitor = new LatexVisitor();
        return visitor.Visit(context);
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