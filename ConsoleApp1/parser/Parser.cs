using Antlr4.Runtime;

namespace ConsoleApp1.parser;

public class Parser
{
    public static Expr? parse(string input)
    {
        var inputStream = new AntlrInputStream(input);
        var lexer = new MathLexer(inputStream);
        var commonTokenStream = new CommonTokenStream(lexer);
        var parser = new MathParser(commonTokenStream);
        var progContext = parser.prog();
        var visitor = new MathVisitor();
        return visitor.Visit(progContext);
    }
}