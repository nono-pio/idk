using Antlr4.Runtime;
using ConsoleApp1.Latex;
using ConsoleApp1.Parser.Antlr;

namespace ConsoleApp1.Parser;

public class Parser
{
    public static Expr? Parse(string input)
    {
        // Création d'un lexer et d'un parser pour analyser le texte LaTeX
        var inputStream = new AntlrInputStream(input);
        var lexer = new LatexLexer(inputStream);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new LatexParser(tokenStream);

        // Analyse de l'entrée LaTeX avec la règle de départ
        var context = parser.expr();

        // Création et utilisation du visiteur pour générer l'Expr
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