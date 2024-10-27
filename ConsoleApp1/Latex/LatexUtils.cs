namespace ConsoleApp1.Latex;

public static class LatexUtils
{
    
    public static string Parenthesis(string content) => Symbols.LParen + content + Symbols.RParen;
    public static string Brackets(string content) => Symbols.LBrackets + content + Symbols.RBrackets;
    public static string Braces(string content) => Symbols.LBraces + content + Symbols.RBraces;
    
    public static string LatexBraces(string content) => "{" + content + "}";
    public static string LatexBracesIfNotSingle(string content) => content.Length == 1 ? content : LatexBraces(content);
    
    public static string Subscript(string content, string subscript) => content + "_" + LatexBracesIfNotSingle(subscript);
    
    public static string Fraction(string numerator, string denominator) => 
        @"\frac{" + numerator + "}{" + denominator + "}";
    public static string Power(string content, string power) => content + "^" + LatexBracesIfNotSingle(power);
    public static string Sqrt(string content) => @"\sqrt{" + content + "}";

    public static string NthRoot(string n, string content)
    { 
        if (n == "2")
            return @"\sqrt{" + content + "}";
        
        return  @"\sqrt[" + n + "]{" + content + "}";
    } 
    
    public static string Summation(string start, string end, string expression) => 
        @"\sum_{" + start + "}^{" + end + "} " + expression;
    public static string Product(string start, string end, string expression) =>
        @"\prod_{" + start + "}^{" + end + "} " + expression;
    
    public static string Limit(string variable, string approach, string expression) =>
        @"\lim_{" + variable + @"\to" + approach + "} " + expression;
    
    public static string Integral(string start, string end, string expression) =>
        @"\int_{" + start + "}^{" + end + "} " + expression;

    public static string Fonction(string name, string argument, string? subscript = null, string? power = null)
    {
        var result = name;
        if (subscript != null)
            result += "_" + LatexBracesIfNotSingle(subscript);
        
        if (power != null)
            result += "^" + LatexBracesIfNotSingle(power);
        
        result += Parenthesis(argument);
        return result;
    }
    public static string Fonction(string name, string[] arguments, string? subscript = null, string? power = null)
    {
        var result = name;
        if (subscript != null)
            result += "_" + LatexBracesIfNotSingle(subscript);
        
        if (power != null)
            result += "^" + LatexBracesIfNotSingle(power);
        
        result += Parenthesis(string.Join(Symbols.FonctionSeparator, arguments));
        return result;
    }

    public static string Vector(string[] components)
    {
        return @"\begin{pmatrix}" + string.Join(@"\\", components) + @"\end{pmatrix}";
    }
    
    public static string Matrix(string[][] components)
    {
        return @"\begin{bmatrix}" + string.Join(@"\\", components.Select(row => string.Join("&", row))) + @"\end{bmatrix}";
    }
    
}