using ConsoleApp1.Core.Classes;
using ConsoleApp1.Core.Expressions.Atoms;
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
 X numbers -> Number
   \pi, \e -> Constant

*/

public class Parser
{
    
    private static string StringTest = "1 + 2 + x * 3";

    // (T, int)? GetT(string input)
    // input: input string
    // T: Type of the token (ex: Number, Addition)
    // return: if the token is found return the token and the length of the token string, else return null

    /*
    
    // TODO: Somme, Produit, Limite
    Expr -> Add
    Add -> Mul +/- ... +/- Mul | Mul
    Mul -> Pow * / frac ... * / frac Pow | Pow
    Power -> Fn ^ Power | Fn
    Fn -> Fonction | Sqrt | Intégrale | Abs | Arrondi | Atom
    Atom -> Number | Variable | (Expr) | Set | Vecteur | Matrice
    
    // TODO IDK: f(x) -> fonction f de x ou f * x
    // TODO IDK: uv -> u * v ou variable uv
    
    */
    public static (Expr Expr, int Length)? GetExpr(string input)
    {
        throw new NotImplementedException();
    }

    public static (Variable Var, int Length)? GetVariable(string input)
    {
        // Variable -> (ID | GreekLetter ) (_ expr)?
        // TODO: ID -> Char
        if (input.Length == 0)
            return null;

        var i = 0;
        string name;
        
        var idTest = GetID(input);
        if (idTest is not null)
        {
            name = idTest.Value.ID;
            i += idTest.Value.Length;
        }
        else
        {
            var greekTest = GetGreekLetter(input);
            if (greekTest is null)
                return null;
            
            name = greekTest.Value.Letter;
            i += greekTest.Value.Length;
        }
        
        // TODO
        return (Var(name), i);
        
        var subscriptTest = GetSubscript(input[i..]);
        if (subscriptTest is null)
            return (Var(name), i);
        
        var subscript = subscriptTest.Value.Expr;
        i += subscriptTest.Value.Length;
        //return (Var(name, subscript:subscript), i);
    }
    
    public static (Expr Expr, int Length)? GetSubscript(string input)
    {
        // Substcript -> _ ([0-9a-zA-Z] | \[...]+ | { Expr })
        
        if (input.Length <= 1 || input[0] != '_')
            return null;

        var i = 1;

        // [0-9a-zA-Z]
        if (char.IsDigit(input[i]))
            return (Num(input[i] - '0'), 2);
        if (char.IsLetter(input[i]))
            return (Var(input[i].ToString()), 2);
        
        // \[...]+
        if (input[i] == '\\')
        {
            // TODO
            throw new NotImplementedException();
        }
        
        // { Expr }
        var exprTest = GetLatexBracesExpr(input);
        if (exprTest is null)
            return null;
            
        var expr = exprTest.Value.Expr;
        i += exprTest.Value.Length;
        return (expr, i);
    }
    
    public static (Expr Expr, int Length)? GetLatexBracesExpr(string input)
    {
        // LatexBracesExpr -> { Expr }
        
        if (input.Length <= 2 || input[0] != '{')
            return null;
        
        throw new NotImplementedException();
    }

    public static (string Letter, int Length)? GetGreekLetter(string input)
    {
        var i = 0;
        
        if (input.Length <= 1 || input[i] != '\\')
            return null;
            
        string? name = null;
        foreach (var letter in Symbols.GreekLetters)
        {
            if (input.StartsWith(letter))
            {
                name = letter;
                i += letter.Length;
                break;
            }
        }
            
        if (name is null)
            return null;

        return (name, i);
    }
    
    public static (string ID, int Length)? GetID(string input)
    {
        // ID -> [a-zA-Z]+
        
        if (input.Length == 0)
            return null;
        
        var i = 0;
        while (i < input.Length && char.IsLetter(input[i]))
            i++;
        
        if (i == 0)
            return null;
        
        return (input[..i], i);
    }
    
    public static (Number Num, int Length)? GetNumber(string input)
    {
        // Number -> INT | FLOAT

        if (input.Length == 0)
            return null;
        
        var intTest = GetInt(input);
        if (intTest is not null)
            return (intTest.Value.Int, intTest.Value.Length);
        
        var floatTest = GetFloat(input);
        if (floatTest is not null)
            return (floatTest.Value.Float, floatTest.Value.Length);
        
        return null;
    }

    public static (long Int, int Length)? GetInt(string input)
    {
        // INT -> -? [0-9]+

        if (input.Length == 0)
            return null;
        
        var i = 0;
        
        // -?
        long sign = 1;
        if (input[i] == '-')
        {
            sign = -1;
            i++;
        }
        
        var numberTest = GetDigits(input[i..]);
        
        if (numberTest is null)
            return null;

        return (sign * numberTest.Value.Number, i + numberTest.Value.Length);
    }

    public static (double Float, int Length)? GetFloat(string input)
    {
        // FLOAT -> -? [0-9]+ [.,] [0-9]* EXP:([eE][+-]?[0-9]+)?
        
        if (input.Length == 0)
            return null;
        
        var i = 0;
        
        // -?
        long sign = 1;
        if (input[i] == '-')
        {
            sign = -1;
            i++;
        }
        
        // [0-9]+
        var intTest = GetDigits(input[i..]);
        if (intTest is null)
            return null;
        
        var intNumber = intTest.Value.Number;
        i += intTest.Value.Length;
        
        // [.,]
        if (i >= input.Length || (input[i] != '.' && input[i] != ','))
            return null;
        
        i++;
        
        // [0-9]*
        var decimalTest = GetDigits(input[i..]);

        long decimalNumber;
        if (decimalTest is null)
            decimalNumber = 0;
        else
        {
            decimalNumber = decimalTest.Value.Number;
            i += decimalTest.Value.Length;
        }

        double currentFloat = sign * (intNumber + (double)decimalNumber / Math.Pow(10, decimalNumber.ToString().Length));
        
        // EXP:([eE][+-]?[0-9]+)?
        
        if (i+1 >= input.Length)
            return (currentFloat, i);
        
        // [eE]
        if (input[i] != 'e' && input[i] != 'E')
            return (currentFloat, i);
        
        i++;
        
        // [+-]?
        long expSign = 1;
        if (input[i] == '-')
        {
            expSign = -1;
            i++;
        }
        else if (input[i] == '+')
            i++;
        
        // [0-9]+
        var expTest = GetDigits(input[i..]);
        if (expTest is null)
            return null;
        
        var expNumber = expTest.Value.Number;
        i += expTest.Value.Length;
        
        return (currentFloat * Math.Pow(10, expSign * expNumber), i);
    }

    public static (long Number, int Length)? GetDigits(string input)
    {
        // DIGITS -> [0-9]+

        var i = 0;
        var number = 0L;
        while (i < input.Length && char.IsDigit(input[i]))
        {
            number = number * 10 + (input[i] - '0'); // Convert char to int
            i++;
        }
        
        if (i == 0)
            return null;

        return (number, i);
    } 

}