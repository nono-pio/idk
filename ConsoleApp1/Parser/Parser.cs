using System.Text.RegularExpressions;
using ConsoleApp1.Core.Classes;
using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Expressions.Others;
using ConsoleApp1.Core.Sets;
using ConsoleApp1.Latex;

namespace ConsoleApp1.Parser;


/*

Parser Grammar:

   + -> Addition, Signe
   - -> Soustraction, Signe
   *, \cdot -> Multiplication
   /, \frac{}{} -> Division
   ^ -> Puissance
   (), \left(, \right) -> Parenthèses
   [] -> Latex Group
   {} -> Latex Group
   \left{, \right} -> Set
   \left|, \right| -> Abs, Norme
   floor, ceil, round -> Arrondi
   _ -> Subscript
   \sqrt[]{} -> Racine carrée
   \sum_{}^{} -> Somme
   \prod_{}^{} -> Produit
   \lim_{}^{} -> Limite
   \int_{}^{} -> Intégrale
   f(x), f(x, ..., x) -> fonction

   \begin{pmatrix} ... \\ ... \end{pmatrix} -> Vecteur
   \begin{bmatrix} ... & ... \\ ... & ... \end{bmatrix} -> Matrice

   greek letters, variables -> Variable
   numbers -> Number
   \pi, \e -> Constant

*/

public class Parser
{
    public static Parsed<Expr> Null => Parsed<Expr>.Null;
    
    public static Expr? Parse(string input)
    {
        var parse = GetExpr(input);
        return parse.IsNull ? null : parse.Value;
    }
    
    public static Parsed<Expr> GetExpr(string input)
    {
        return GetAdd(input);
    }

    public static Parsed<Expr> GetAdd(string input)
    {
        var i = 0;
        
        var mulTest = GetMul(input);
        if (mulTest.IsNull)
            return Null;
        
        var sum = mulTest.Value;
        i += mulTest.Length;

        while (i < input.Length)
        {
            var sign = input[i];
            if (sign != '+' && sign != '-')
                return new(sum, i);
            
            i++;
            
            mulTest = GetMul(input[i..]);
            if (mulTest.IsNull)
                return Null;
            
            i += mulTest.Length;
            
            if (sign == '+')
                sum = Add(sum, mulTest.Value);
            else // sign == '-'
                sum = Sub(sum, mulTest.Value);
        }

        return new(sum, i);
    }
    
    public static Parsed<Expr> GetMul(string input)
    {
        var i = 0;
        
        var powTest = GetPow(input);
        if (powTest.IsNull)
            return Null;
        
        var currentExpr = powTest.Value;
        i += powTest.Length;

        while (i < input.Length)
        {
            var sepTest = GetMulSeparator(input[i..]);
            if (sepTest is null)
                return Null;
            
            var mulType = sepTest.Value.code;
            i += sepTest.Value.lenght;
            
            powTest = GetPow(input[i..]);
            if (powTest.IsNull)
                return mulType == 0 ? new(currentExpr, i) : Null;
            
            i += powTest.Length;
            if (mulType <= 1)
                currentExpr = Mul(currentExpr, powTest.Value);
            else
                currentExpr = Div(currentExpr, powTest.Value);
        }

        return new(currentExpr, i);
    }

    // MulType: 0: None, 1: *, 2: /
    public static (int code, int lenght)? GetMulSeparator(string input)
    {
        // separators: * / None \cdot, \times
        const string cdot = @"\cdot";
        const string times = @"\times";
        
        if (input.Length == 0)
            return null;

        if (input[0] == '*')
            return new(1, 1);
        
        if (input[0] == '/')
            return new(2, 1);
        
        if (input.StartsWith(cdot))
            return new(1, cdot.Length);
        
        if (input.StartsWith(times))
            return new(1, times.Length);

        return new(0, 0); // None
    }
    
    public static Parsed<Expr> GetPow(string input)
    {
        var i = 0;
        
        var fnTest = GetAtom(input);
        if (fnTest.IsNull)
            return Null;
        
        var currentExpr = fnTest.Value;
        i += fnTest.Length;

        if (i >= input.Length || input[i] != '^')
            return new(currentExpr, i);
        
        i++;

        var expTest = GetParenthesisOrLatexBracesOrShort(input[i..]);
        if (expTest.IsNull)
            return Null;
        
        currentExpr = Pow(currentExpr, expTest.Value);
        i += expTest.Length;
    

        return new(currentExpr, i);
    }

    public static Parsed<Expr> GetAtom(string input)
    {
        return ParserHelper.Or(input, 
            [GetFrac, GetFunction, GetSqrt, GetAbs, GetIntegerFunctions, GetNumber, GetVariable, GetParenthesesExpr, GetSquareBracketExpr]
            );
    }

    public static Parsed<Expr> GetFrac(string input)
    {
        if (!input.StartsWith("\\frac"))
            return Null;

        var fracParse = ParserHelper.Sequence(input[5..], GetShortOrBracesExpr, GetShortOrBracesExpr,
            (num, den) => num / den);
        if (fracParse.IsNull)
            return Null;
        
        return new(fracParse.Value, fracParse.Length + 5);
    }

    public static Parsed<Expr> GetFunction(string input)
    {
        // Fonction -> FuncName _ Sub ^ Pow \left( Expr, Expr, ..., Expr \right)
            
        var nameParse = GetFuncName(input);
        if (nameParse.IsNull)
            return Null;
        
        var i = nameParse.Length;
        
        var subscriptParse = GetSubscript(input[i..]);
        Expr? subscript = null;
        if (!subscriptParse.IsNull)
        {
            subscript = subscriptParse.Value;
            i += subscriptParse.Length;
        }

        Expr? power = null;
        if (i < input.Length && input[i] == '^')
        {
            i++;
            
            var powerTest = GetShortOrBracesExpr(input[i..]);
            if (powerTest.IsNull)
                return Null;
            
            power = powerTest.Value;
            i += powerTest.Length;
        }
        
        // \left( Expr, Expr, ..., Expr \right)
        var exprsParse = GetParenthesesExprs(input[i..]);
        if (exprsParse.IsNull)
            return Null;
        
        i += exprsParse.Length;

        var func = GetFunctionFromAttributes(nameParse.Value, exprsParse.Value, subscript, power);
        if (func is null)
            return Null;
        
        return new(func, i);
    }

    private static Parsed<string> GetFuncName(string input)
    {
        
        // FuncName -> [a-zA-Z]+ | \[a-zA-Z]+
        
        var i = 0;
        if (input.Length == 0)
            return Parsed<string>.Null;
        
        if (input[i] == '\\')
            i++;

        while (i < input.Length && char.IsLetter(input[i]))
        {
            i++;
        }
        
        return new(input[..i], i);
    }

    private static Expr? GetFunctionFromAttributes(string name, Expr[] vars, Expr? subscript, Expr? power)
    {
        // FuncName -> Letter | FuncNameList
        var univariateFuncsNoSubscripts = new Dictionary<string[], Func<Expr, Expr>>()
        {
            { ["sin"], Sin }, 
            { ["cos"], Cos }, 
            { ["tan"], Tan }, 
            { ["arcsin"], ASin }, 
            { ["arccos"], ACos }, 
            { ["arctan"], ATan },
            
            // { "sinh", Sinh }, 
            // { "cosh", Cosh }, 
            // { "tanh", Tanh }, 
            
            { ["ln"], Ln },
            { ["log"], Log },
            { ["exp"], Exp },
        
            { ["im"], Im },
            { ["re"], Re },
        
            { ["sign"], Sign },
            { ["abs"], Abs },
            { ["floor"], Floor },
            { ["ceil"], Ceil },
            { ["round"], Round },
            
            { ["sqrt"], Sqrt },
            { ["cbrt"], Cbrt },
        };

        var multivariateFuncsNoSubscripts = new Dictionary<string[], Func<Expr[], Expr>>()
        {
            { ["max"], Max },
            { ["min"], Min },
        };
        
        var oldName = name;
        if (name[0] == '\\')
            name = name[1..];
        name = name.ToLower();

        if (power is not null && power == -1 && ((string[]) ["sin", "cos", "tan"]).Contains(name))
        {
            name = "arc" + name;
            power *= -1;
        }
        
        Expr? result = null;
        if (subscript is null)
        {
            // Univariate functions
            foreach (var (names, func) in univariateFuncsNoSubscripts)
            {
                if (names.Contains(name))
                    result = func(vars[0]);
            }
        
            // Multivariate functions
            foreach (var (names, func) in multivariateFuncsNoSubscripts)
            {
                if (names.Contains(name))
                    result = func(vars);
            }    
        }
        else // subscript cases
        {
            if (name == "log")
                result = Log(vars[0], subscript);
            if (name == "exp")
                result = Pow(subscript, vars[0]);
        }

        if (result is null && IsLetter(oldName))
        {
            result = new UndefineFunction(oldName, vars);
        }

        if (result is null)
            return null;
        
        if (power is not null && power != 1)
            result = Pow(result, power);

        return result;
    }

    public static Parsed<Expr> GetSqrt(string input) 
    {
        // Sqrt -> \sqrt [Expr]? \left( Expr \right)
        if (!input.StartsWith("\\sqrt"))
            return Null;

        var sqrtParse = ParserHelper.Or(input[5..], [
            (string str) => GetLatexBracesExpr(str).Map(Sqrt),
            (string str) => ParserHelper.Sequence(str, GetSquareBracketExpr, GetLatexBracesExpr, (n,x) => Sqrt(x, n))
        ]);

        return sqrtParse.IsNull ? Null : new(sqrtParse.Value, sqrtParse.Length + 5);
    }
    
    public static Parsed<Expr> GetAbs(string input) 
    {
        // Abs -> \left| Expr \right|

        return ParserHelper.Or(input, [
            str => ParserHelper.ParseBetween(str, "|", "|", GetExpr).Map(Abs),
            str => ParserHelper.ParseBetweenRecursive(str, "\\left|", "\\right|", GetExpr).Map(Abs),
        ]);
    }
    
    public static Parsed<Expr> GetIntegerFunctions(string input) 
    {
        // lfloor rfloor -> floor
        // lceil rceil -> ceil
        // lfloor rceil -> round
        // lceil rfloor -> round
        
        const string startFloor = @"\lfloor";
        const string endFloor = @"\rfloor";

        const string startCeil = @"\lceil";
        const string endCeil = @"\rceil";

        return ParserHelper.Or(input, [
            str => ParserHelper.ParseBetweenRecursive(str, startFloor, endFloor, GetExpr).Map(Floor),
            str => ParserHelper.ParseBetweenRecursive(str, startCeil, endCeil, GetExpr).Map(Ceil),
            str => ParserHelper.ParseBetweenRecursive(str, startFloor, endCeil, GetExpr).Map(Round),
            str => ParserHelper.ParseBetweenRecursive(str, startCeil, endFloor, GetExpr).Map(Round),
        ]);
    }

    public static Parsed<Expr[]> GetParenthesesExprs(string input)
    {
        var insideParsed = GetParenthesesInside(input);
        if (insideParsed.IsNull)
            return Parsed<Expr[]>.Null;

        var elementsParse = ParserHelper.ParseList(insideParsed.Value, ",", GetExpr);
        if (elementsParse.IsNull)
            return Parsed<Expr[]>.Null;
        
        return new(elementsParse.Value, insideParsed.Length);
    }
    
    public static Parsed<Expr> GetParenthesesExpr(string input)
    {
        // Parentheses -> ( Expr )
        return ParserHelper.Or(input, [
            str => ParserHelper.ParseBetweenRecursive(str, "(", ")", GetExpr),
            str => ParserHelper.ParseBetweenRecursive(str, "\\left(", "\\right)", GetExpr),
        ]);
    }
    
    public static Parsed<Expr> GetSquareBracketExpr(string input)
    {
        // Parentheses -> ( Expr )
        return ParserHelper.Or(input, [
            str => ParserHelper.ParseBetweenRecursive(str, "[", "]", GetExpr),
            str => ParserHelper.ParseBetweenRecursive(str, "\\left[", "\\right]", GetExpr),
        ]);
    }

    public static Parsed<string> GetParenthesesInside(string input)
    {
        Func<string, Parsed<string>> identityParsed = x => new(x, x.Length);
        return ParserHelper.Or(input, [
            str => ParserHelper.ParseBetweenRecursive(str, "(", ")", identityParsed),
            str => ParserHelper.ParseBetweenRecursive(str, "\\left(", "\\right)", identityParsed),
        ]);
    }

    public static Parsed<Expr> GetParenthesisOrLatexBracesOrShort(string input)
    {
        return ParserHelper.Or(input, [GetParenthesesExpr, GetLatexBracesExpr, GetShortExpr]);
    }

    public static Parsed<Expr> GetVariable(string input)
    {
        // Variable -> Cste | Letter (_ expr)?

        Func<string, Expr> mapToVar = variable => Var(variable);
        return ParserHelper.Or(input, [GetConstant, str => GetLetter(str).Map(mapToVar)]);
    }

    public static Parsed<Expr> GetConstant(string input)
    {
        var constants = new Dictionary<string[], Expr>()
        {
            { ["pi", "\\pi"], Constant.PI },
            { ["e"], Constant.E },
            { ["oo", "\\infty"], Constant.Infinity },
            { ["-oo"], Constant.NegativeInfinity },
            { ["\\text{NaN}"], Constant.NaN }
        };
        
        foreach (var (names, cste) in constants)
        {
            for (int i = 0; i < names.Length; i++)
            {
                if (input.StartsWith(names[i]))
                {
                    return new(cste, names[i].Length);
                }
            }
        }
        
        return Null;
    }
    
    public static Parsed<string> GetLetter(string input)
    {
        if (input.Length == 0)
            return Parsed<string>.Null;
        
        if (char.IsLetter(input[0]))
            return new(input[0].ToString(), 1);
        
        return GetGreekLetter(input);
    }
    
    public static bool IsLetter(string input)
    {
        if (input.Length == 0)
            return false;

        return char.IsLetter(input[0]) || IsGreekLetter(input);
    }
    
    public static Parsed<Expr> GetSubscript(string input)
    {
        // Subscript -> _ ShortOrBracesExpr
        
        if (input.Length <= 1 || input[0] != '_')
            return Null;

        return GetShortOrBracesExpr(input[1..]);
    }
    
    
    public static Parsed<Expr> GetShortOrBracesExpr(string input)
    {
        // ShortOrBracesExpr -> ShortExpr | { Expr }

        return ParserHelper.Or(input, [GetShortExpr, GetLatexBracesExpr]);
    }

    public static Parsed<Expr> GetShortExpr(string input)
    {
        Func<string, Expr> mapToVar = variable => Var(variable);
        return ParserHelper.Or(input, [
            GetDigit, 
            str => GetLetter(str).Map(mapToVar)
        ]);
    }
    
    public static bool IsGreekLetter(string command)
    {
        return Symbols.GreekLetters.Contains(command);
    }
    
    public static Parsed<Expr> GetLatexBracesExpr(string input)
    {
        // LatexBracesExpr -> { Expr }

        return ParserHelper.ParseBetweenRecursive(input, "{", "}", GetExpr);
    }

    public static Parsed<string> GetGreekLetter(string input)
    {
        var i = 0;
        
        if (input.Length <= 1 || input[i] != '\\')
            return Parsed<string>.Null;
            
        foreach (var letter in Symbols.GreekLetters)
        {
            if (input.StartsWith(letter))
                return new(letter, letter.Length);
        }
            
        return Parsed<string>.Null;
    }
    
    public static Parsed<Expr> GetNumber(string input)
    {
        // Number -> INT | FLOAT

        return ParserHelper.Or(input, [GetInteger, GetFloat]);
    }

    public static Parsed<Expr> GetInteger(string input) => ParserHelper.ParseLong(input).Map(num => (Expr) Num(num));
    public static Parsed<Expr> GetFloat(string input) => ParserHelper.ParseFloat(input).Map(num => (Expr) Num(num));
    public static Parsed<Expr> GetDigit(string input) => ParserHelper.ParseDigit(input).Map(num => (Expr) Num(num));
    
    
    
}

public static class StringExtension
{
    public static bool ContainsAt(this string str, string value, int index)
    {
        if (str.Length - index < value.Length)
            return false;
        
        for (int i = 0; i < value.Length; i++)
        {
            if (str[i + index] != value[i])
                return false;
        }

        return true;
    }  
}