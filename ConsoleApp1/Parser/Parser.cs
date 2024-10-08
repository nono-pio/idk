﻿using ConsoleApp1.Core.Classes;
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
    public static Expr? Parse(string input)
    {
        var parse = GetExpr(input);
        if (parse is null)
            return null;

        return parse.Value.Expr;
    } 
    
    
    public static Set? ParseSet(string input)
    {
        var parse = GetSet(input);
        if (parse is null)
            return null;

        return parse.Value.Set;
    } 

    // (T, int)? GetT(string input)
    // input: input string
    // T: Type of the token (ex: Number, Addition)
    // return: if the token is found return the token and the length of the token string, else return null

    /*
    
    // TODO: Somme, Produit, Limite
    Expr -> Add
    Add -> Mul +/- ... +/- Mul | Mul
    Mul -> Pow * / frac ... * / frac Pow | Pow
    Power -> Fn ^ {Expr} | Fn
    Fn -> Fonction | Sqrt | Intégrale | Abs | Arrondi | Atom
    Atom -> Number | Variable | (Expr) | Set | Vecteur | Matrice
    
    */
    public static (Expr Expr, int Length)? GetExpr(string input)
    {
        return GetAdd(input);
    }

    public static (Expr Add, int Length)? GetAdd(string input)
    {
        var i = 0;
        
        var mulTest = GetMul(input);
        if (mulTest is null)
            return null;
        
        var currentExpr = mulTest.Value.Mul;
        i += mulTest.Value.Length;

        while (i < input.Length)
        {
            var sign = input[i];
            if (sign != '+' && sign != '-')
                return (currentExpr, i);
            
            i++;
            
            mulTest = GetMul(input[i..]);
            if (mulTest is null)
                return null;
            
            i += mulTest.Value.Length;
            
            if (sign == '+')
                currentExpr = Add(currentExpr, mulTest.Value.Mul);
            else // sign == '-'
                currentExpr = Sub(currentExpr, mulTest.Value.Mul);
        }

        return (currentExpr, i);
    }
    
    public static (Expr Mul, int Length)? GetMul(string input)
    {
        var i = 0;
        
        var powTest = GetPow(input);
        if (powTest is null)
            return null;
        
        var currentExpr = powTest.Value.Pow;
        i += powTest.Value.Length;

        while (i < input.Length)
        {
            var sepTest = GetMulSeparator(input[i..]);
            if (sepTest is null)
                return null;
            
            var mulType = sepTest.Value.MulType;
            i += sepTest.Value.Length;
            
            powTest = GetPow(input[i..]);
            if (powTest is null)
                return mulType == 0 ? (currentExpr, i) : null;
            
            i += powTest.Value.Length;
            if (mulType <= 1)
                currentExpr = Mul(currentExpr, powTest.Value.Pow);
            else
                currentExpr = Div(currentExpr, powTest.Value.Pow);
        }

        return (currentExpr, i);
    }

    // MulType: 0: None, 1: *, 2: /
    public static (int MulType, int Length)? GetMulSeparator(string input)
    {
        // separators: * / None \cdot, \times
        const string cdot = @"\cdot";
        const string times = @"\times";
        
        if (input.Length == 0)
            return null;

        if (input[0] == '*')
            return (1, 1);
        
        if (input[0] == '/')
            return (2, 1);
        
        if (input.StartsWith(cdot))
            return (1, cdot.Length);
        
        if (input.StartsWith(times))
            return (1, times.Length);

        return (0, 0); // None
    }
    
    public static (Expr Pow, int Length)? GetPow(string input)
    {
        var i = 0;
        
        var fnTest = GetFn(input);
        if (fnTest is null)
            return null;
        
        var currentExpr = fnTest.Value.Fn;
        i += fnTest.Value.Length;

        if (i >= input.Length || input[i] != '^')
            return (currentExpr, i);
        
        i++;

        var expTest = GetShortOrBracesExpr(input[i..]);
        if (expTest is null)
            return null;
        
        currentExpr = Pow(currentExpr, expTest.Value.Expr);
        i += expTest.Value.Length;
    

        return (currentExpr, i);
    }

    public static (Expr Fn, int Length)? GetFn(string input)
    {
        // Fn -> Fonction | Sqrt | Intégrale | Abs | Arrondi | Atom
        
        // Fonction -> Letter _ Sub ^ Pow \left( Expr, Expr, ..., Expr \right)
        var functionTest = GetFunction(input);
        if (functionTest is not null)
            return (functionTest.Value.Function, functionTest.Value.Length);
        
        // Sqrt -> \sqrt [Expr]? \left( Expr \right)
        var sqrtTest = GetSqrt(input);
        if (sqrtTest is not null)
            return (sqrtTest.Value.Sqrt, sqrtTest.Value.Length);
        
        // Intégrale -> \int _ Sub ^ Pow Expr dLetter
        var intTest = GetIntegral(input);
        if (intTest is not null)
            return (intTest.Value.Intergal, intTest.Value.Length);
        
        // Abs -> \left| Expr \right|
        var absTest = GetAbs(input);
        if (absTest is not null)
            return (absTest.Value.Abs, absTest.Value.Length);
        
        // Arrondi -> floor | ceil | round
        var floorCeilOrRoundTest = GetFloorCeilOrRound(input);
        if (floorCeilOrRoundTest is not null)
            return (floorCeilOrRoundTest.Value.Expr, floorCeilOrRoundTest.Value.Length);
        
        // Atom
        var atomTest = GetAtom(input);
        if (atomTest is not null)
            return (atomTest.Value.Atom, atomTest.Value.Length);

        return null;
    }

    public static (Expr Function, int Length)? GetFunction(string input)
    {
        // Fonction -> FuncName _ Sub ^ Pow \left( Expr, Expr, ..., Expr \right)
            
        var i = 0;
        
        var nameTest = GetFuncName(input);
        if (nameTest is null)
            return null;
        
        var name = nameTest.Value.FuncName;
        i += nameTest.Value.Length;
        
        var subscriptTest = GetSubscript(input[i..]);
        Expr? subscript = null;
        if (subscriptTest is not null)
        {
            subscript = subscriptTest.Value.Expr;
            i += subscriptTest.Value.Length;
        }

        Expr? power = null;
        if (i < input.Length && input[i] == '^')
        {
            i++;
            
            var powerTest = GetShortOrBracesExpr(input[i..]);
            if (powerTest is null)
                return null;
            
            power = powerTest.Value.Expr;
            i += powerTest.Value.Length;
        }
        
        // \left( Expr, Expr, ..., Expr \right)
        var exprsTest = GetExprs(input[i..], "(", ")", ",");
        if (exprsTest is null)
            return null;
        
        var exprs = exprsTest.Value.Exprs;
        i += exprsTest.Value.Length;

        var f = new UndefineFunction(name, exprs[0]);
        return (power is null ? f : Pow(f, power), i);
    }

    public static (string FuncName, int Length)? GetFuncName(string input)
    {
        // FuncName -> Letter | FuncNameList
        var funcNameList = new string[]
        {
            "sin", "cos", "tan", "cot", "sec", "csc",
            "arcsin", "arccos", "arctan", "arccot", "arcsec", "arccsc",
            "sinh", "cosh", "tanh", "coth", "sech", "csch",
            "arsinh", "arcosh", "artanh", "arcoth", "arsech", "arcsch",
            "ln", "log", "exp",
            "lim", "max", "min", "sup", "inf",
            "det", "tr", "rank", "adj", "cof", "diag",
            "dim", "ker", "im", "span", "null", "range",
            "conv", "grad", "div", "curl", "lap", "hess",
            "sgn", "abs", "floor", "ceil", "round",
            "sqrt", "cbrt", "root",
            "sum", "prod", "int", "oint", "iint", "iiint", "iiiint", "idotsint",
            "vec", "mat", "det", "tr", "rank", "adj", "cof", "diag",
            "dim", "ker", "im", "span", "null", "range",
            "conv", "grad", "div", "curl", "lap", "hess",
            "sgn", "abs", "floor", "ceil", "round",
            "sqrt", "cbrt", "root",
            "sum", "prod", "int", "oint", "iint", "iiint", "iiiint", "idotsint",
            "vec", "mat"
        };
        
        foreach (var funcName in funcNameList)
        {
            if (input.StartsWith(funcName))
            {
                return (funcName, funcName.Length);
            }
        }
        
        var letterTest = GetLetter(input);
        if (letterTest is not null)
            return (letterTest.Value.Letter, letterTest.Value.Length);

        return null;
    }
    
    public static (Expr Sqrt, int Length)? GetSqrt(string input) 
    {
        // Sqrt -> \sqrt [Expr]? \left( Expr \right)
        
        const string start = @"\sqrt";
        
        var i = 0;
        if (input.Length <= start.Length || !input.StartsWith(start))
            return null;
        
        i += start.Length;
        
        // [Expr]?
        Expr n = 2;
        if (input[i] == '[')
        {
            i++;
            var expTest = GetExpr(input[i..]);
            if (expTest is null)
                return null;
            
            i += expTest.Value.Length;
            n = expTest.Value.Expr;
            
            if (i >= input.Length || input[i] != ']')
                return null;
            
            i++;
        }
        
        // \left( Expr \right)
        var exprTest = GetShortOrBracesExpr(input[i..]);
        if (exprTest is null)
            return null;
        
        var expr = exprTest.Value.Expr;
        i += exprTest.Value.Length;
        
        return (Sqrt(expr, n), i);
    }
    
    public static (Expr Intergal, int Length)? GetIntegral(string input) 
    {
        // Intégrale -> \int _ Sub ^ Pow Expr dLetter
        
        const string start = @"\int";
        
        var i = 0;
        if (input.Length <= start.Length || !input.StartsWith(start))
            return null;
        
        i += start.Length;
        
        // _ Sub
        var subscriptTest = GetSubscript(input[i..]);
        Expr? subscript = null;
        if (subscriptTest is not null)
        {
            subscript = subscriptTest.Value.Expr;
            i += subscriptTest.Value.Length;
        }
        
        // ^ Pow
        Expr? power = null;
        if (input[i] == '^')
        {
            i++;
            var powerTest = GetShortOrBracesExpr(input[i..]);
            if (powerTest is not null)
            {
                power = powerTest.Value.Expr;
                i += powerTest.Value.Length;
            }
        }
        
        // d
        int j = input.IndexOf('d');
        
        // Expr
        var exprTest = GetExpr(input[i..j]);
        if (exprTest is null)
            return null;
        
        var expr = exprTest.Value.Expr;
        i += exprTest.Value.Length;
        
        // dLetter
        var letterTest = GetLetter(input[j..]);
        if (letterTest is null)
            return null;
        
        var letter = letterTest.Value.Letter;
        i += letterTest.Value.Length;

        throw new NotImplementedException();
        //return (Integral(subscript, power, expr, letter), i);
    }
    
    public static (Expr Abs, int Length)? GetAbs(string input) 
    {
        // Abs -> \left| Expr \right|
        
        const string start = @"\left|";
        const string end = @"\right|";
        
        var i = 0;
        if (input.Length <= start.Length || !input.ContainsAt(start, i))
            return null;
        
        i += start.Length;

        var deep = 0;
        for (int j = i; j < input.Length; j++)
        {
            if (input.ContainsAt(end, j))
                deep++;
            else if (input.ContainsAt(start, j))
            {
                deep--;
                if (deep == 0)
                {
                    var exprTest = GetExpr(input[i..j]);
                    if (exprTest is null)
                        return null;
                    
                    return (exprTest.Value.Expr, j + end.Length);
                }
            
            }
        }
        
        return null;
    }
    
    public static (Expr Expr, int Length)? GetFloorCeilOrRound(string input) 
    {
        // ... -> (\lfloor | \lceil) Expr (\rfloor | \rceil) 

        // 0: none, 1: floor, 2: ceil
        (int Type, int Length) GetType(string floor, string ceil, int i)
        {
            if (input.ContainsAt(floor, i))
                return (1, floor.Length);
            
            if (input.ContainsAt(ceil, i))
                return (2, ceil.Length);
            
            return (0, 0);
        }

        Expr ToExpr(int startType, int endType, Expr expr)
        {
            throw new NotImplementedException();
            //     if (startType == 1 && endType == 1)
            //         return Floor(expr);
            //     
            //     if (startType == 2 && endType == 2)
            //         return Ceil(expr);
            //     
            //     return Round(expr);
        }
        
        const string startFloor = @"\lfloor";
        const string startCeil = @"\lceil";
        
        const string endFloor = @"\rfloor";
        const string endCeil = @"\rceil";


        var i = 0;
        var startType = GetType(startFloor, startCeil, i);
        if (input.Length <= 1 || startType.Type == 0)
            return null;
        
        i += startType.Length;
        
        var deep = 0;
        for (int j = i; j < input.Length; j++)
        {
            var isEnd = GetType(endFloor, endCeil, j);
            var isStart = GetType(startFloor, startCeil, j);
            
            if (isStart.Type != 0)
                deep++;
            else if (isEnd.Type != 0)
            {
                deep--;
                if (deep == 0)
                {
                    var exprTest = GetExpr(input[i..j]);
                    if (exprTest is null)
                        return null;
                    
                    return (ToExpr(startType.Type, isEnd.Type, exprTest.Value.Expr), j + isEnd.Length);
                }
            
            }
        }
        
        return null;
    }
    
    public static (Expr Atom, int Length)? GetAtom(string input)
    {
        // Atom -> Number | Variable | (Expr) | Set | Vecteur | Matrice

        // Number
        var numberTest = GetNumber(input);
        if (numberTest is not null)
            return (numberTest.Value.Num, numberTest.Value.Length);
        
        // Variable
        var variableTest = GetVariable(input);
        if (variableTest is not null)
            return (variableTest.Value.Var, variableTest.Value.Length);
        
        // (Expr)
        var parenthesesTest = GetParentheses(input);
        if (parenthesesTest is not null)
            return (parenthesesTest.Value.Expr, parenthesesTest.Value.Length);
        
        // Set
        var setTest = GetSet(input);
        if (setTest is not null)
            return (SetExpr.Construct(setTest.Value.Set), setTest.Value.Length);
        
        // Vecteur
        var vectorTest = GetVector(input);
        if (vectorTest is not null)
            return (vectorTest.Value.Vector, vectorTest.Value.Length);
        
        // Matrice
        var matrixTest = GetMatrix(input);
        if (matrixTest is not null)
            return (matrixTest.Value.Matrix, matrixTest.Value.Length);

        return null;
    }

    public static (Expr[] Exprs, int Length)? GetExprs(string input, string start, string end, string separator)
    {
        var i = 0;
        if (input.Length <= start.Length || !input.StartsWith(start))
            return null;
        
        i += start.Length;
        var endInd = input.IndexOf(end, StringComparison.Ordinal);
        if (endInd == -1)
            return null;

        var components = input[i..endInd].Split(separator);

        var exprs = new Expr[components.Length];
        for (int j = 0; j < components.Length; j++)
        {
            var exprTest = GetExpr(components[j]);
            if (exprTest is null)
                return null;
            exprs[j] = exprTest.Value.Expr;
        }
        
        return (exprs, endInd + end.Length);
    }
    
    public static (Expr[][] Exprs, int Length)? GetExprs2D(string input, string start, string end, string separator, string separator2)
    {
        var i = 0;
        if (input.Length <= start.Length || !input.StartsWith(start))
            return null;
        
        i += start.Length;
        var endInd = input.IndexOf(end, StringComparison.Ordinal);
        if (endInd == -1)
            return null;

        var rows = input[i..endInd].Split(separator);
        
        var exprsGrid = new Expr[rows.Length][];
        for (int j = 0; j < rows.Length; j++)
        {
            var cols = rows[j].Split(separator2);

            var exprsRow = new Expr[cols.Length];

            for (int k = 0; k < cols.Length; k++)
            {
                var exprTest = GetExpr(cols[k]);
                if (exprTest is null)
                    return null;
                exprsRow[k] = exprTest.Value.Expr;    
            }
            
            exprsGrid[j] = exprsRow;
        }
        
        return (exprsGrid, endInd + end.Length);
    }
    
    public static (Set Set, int Length)? GetSet(string input)
    {
        // TODO: Interval, ...
        const string start = @"\left{";
        const string end = @"\right}";
        const string separator = @",";

        var i = 0;
        
        var exprsTest = GetExprs(input, start, end, separator);
        if (exprsTest is null)
            return null;
        
        var exprs = exprsTest.Value.Exprs;
        i += exprsTest.Value.Length;

        return (Set.CreateFiniteSet(exprs), exprsTest.Value.Length);
    }
    
    public static (Expr Vector, int Length)? GetVector(string input)
    {
        // Vector -> \begin{pmatrix} ... \\ ... \end{pmatrix}

        const string start = @"\begin{pmatrix}";
        const string end = @"\end{pmatrix}";
        const string separator = @"\\";

        var i = 0;
        
        var exprsTest = GetExprs(input, start, end, separator);
        if (exprsTest is null)
            return null;
        
        var exprs = exprsTest.Value.Exprs;
        i += exprsTest.Value.Length;
        
        return (Vec(exprs), i);
    }
    
    public static (Expr Matrix, int Length)? GetMatrix(string input)
    {
        // Matrix -> \begin{bmatrix} ... & ... \\ ... & ... \end{bmatrix}
        
        const string start = @"\begin{bmatrix}";
        const string end = @"\end{bmatrix}";
        const string separator = @"\\";
        const string separator2 = "&";
        
        var exprsTest = GetExprs2D(input, start, end, separator, separator2);
        if (exprsTest is null)
            return null;
        
        var exprs = exprsTest.Value.Exprs;
        var i = exprsTest.Value.Length;
        
        Expr[,] exprs2D = new Expr[exprs.Length, exprs[0].Length];
        for (int j = 0; j < exprs.Length; j++)
        {
            if (exprs[j].Length != exprs[0].Length)
                return null;
            
            for (int k = 0; k < exprs[j].Length; k++)
            {
                exprs2D[j, k] = exprs[j][k];
            }
        }
        
        return (Matrix(exprs2D), i);
    }
    
    public static (Expr Expr, int Length)? GetParentheses(string input)
    {
        // Parentheses -> ( Expr )

        const string left = "\\left(";
        const string right = "\\right)";

        int Left(int i) => input[i] == '(' ? 1 : (input.ContainsAt(left, i) ? left.Length : -1);
        int Right(int i) => input[i] == ')' ? 1 : (input.ContainsAt(right, i) ? right.Length : -1);
        
        
        var i = 0;
        var leftLength = Left(i);
        if (input.Length <= 2 || leftLength == -1)
            return null;

        var deep = 0;
        for (int j = 0; j < input.Length; j++)
        {
            var l = Left(j);
            if (l != -1)
            {
                deep++;
                j += l;
                continue;
            }

            var rightLength = Right(j);
            if (rightLength != -1)
            {
                deep--;
                if (deep == 0)
                {
                    var exprTest = GetExpr(input[leftLength..j]);
                    if (exprTest is null)
                        return null;
                    
                    // TODO: Test length
                    return (exprTest.Value.Expr, j+rightLength);
                }
                j += rightLength;
            }
        }
        
        return null;
    }

    public static (Variable Var, int Length)? GetVariable(string input)
    {
        // Variable -> Letter (_ expr)?

        if (input.Length == 0)
            return null;

        var i = 0;
        
        var letterTest = GetLetter(input);
        if (letterTest is null)
            return null;
        
        var name = letterTest.Value.Letter;
        i += letterTest.Value.Length;
        
        // TODO
        return (Var(name), i);
        
        /*
        var subscriptTest = GetSubscript(input[i..]);
        if (subscriptTest is null)
            return (Var(name), i);
        
        var subscript = subscriptTest.Value.Expr;
        i += subscriptTest.Value.Length;
        return (Var(name, subscript:subscript), i);
        */
    }
    
    public static (string Letter, int Length)? GetLetter(string input)
    {
        if (input.Length == 0)
            return null;
        
        var i = 0;
        if (char.IsLetter(input[i]))
            return (input[i].ToString(), i+1);
        
        var greekTest = GetGreekLetter(input);
        if (greekTest is null)
            return null;
        
        var name = greekTest.Value.Letter;
        i += greekTest.Value.Length;
        
        return (name, i);
    }
    public static (Expr Expr, int Length)? GetSubscript(string input)
    {
        // Subscript -> _ ShortOrBracesExpr
        
        if (input.Length <= 1 || input[0] != '_')
            return null;

        var i = 1;
        
        var shortExprTest = GetShortOrBracesExpr(input[i..]);
        if (shortExprTest is null)
            return null;
        
        var expr = shortExprTest.Value.Expr;
        i += shortExprTest.Value.Length;
        return (expr, i);
    }

    public static (Expr Expr, int Length)? GetShortOrBracesExpr(string input)
    {
        // ShortOrBracesExpr -> ShortExpr | { Expr }
        // ShortExpr -> [0-9] | [a-zA-Z] | LatexCommand (without parameters)
        // ex: 1, x, \pi, \e

        if (input.Length == 0)
            return null;

        var i = 0;

        // [0-9] | [a-zA-Z]
        if (char.IsDigit(input[i]))
            return (Num(input[i] - '0'), 1);
        if (char.IsLetter(input[i]))
            return (Var(input[i].ToString()), 1);
        
        // LatexCommand (without parameters)
        var latexCommandTest = GetLatexCommand(input);
        if (latexCommandTest is not null)
        {
            var latexCommandExpr = LatexCommandToExpr(latexCommandTest.Value.LatexCommand); // Latex Command : Letter
            
            if (latexCommandExpr is not null)
            {
                i += latexCommandTest.Value.Length;
                return (latexCommandExpr, i);
            }
        }
        
        // { Expr }
        var exprTest = GetLatexBracesExpr(input);
        if (exprTest is null)
            return null;
            
        var expr = exprTest.Value.Expr;
        i += exprTest.Value.Length;
        return (expr, i);
    }

    public static (string LatexCommand, int Length)? GetLatexCommand(string input)
    {
        // LatexCommand -> \[a-zA-Z]+
        
        if (input.Length <= 1 || input[0] != '\\')
            return null;
        
        var i = 1;
        while (i < input.Length)
        {
            if (char.IsLetter(input[i]))
                i++;
            else
                break;
        }
        
        if (i == 1)
            return null;
        
        return (input[..i], i);
    }
    
    public static Expr? LatexCommandToExpr(string command)
    {
        if (IsGreekLetter(command))
        {
            return Var(command);
        }

        return null;
    }
    
    public static bool IsGreekLetter(string command)
    {
        return Symbols.GreekLetters.Contains(command);
    }
    
    public static (Expr Expr, int Length)? GetLatexBracesExpr(string input)
    {
        // LatexBracesExpr -> { Expr }
        
        var i = 0;
        if (input.Length <= 2 || input[0] != '{')
            return null;

        var deep = 0;
        foreach (var c in input)
        {
            if (c == '{')
                deep++;
            else if (c == '}')
            {
                deep--;
                if (deep == 0)
                {
                    var exprTest = GetExpr(input[1..i]);
                    if (exprTest is null)
                        return null;
                    
                    return (exprTest.Value.Expr, i+1);
                }
            }

            i++;
        }
        
        return null;
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
        
        var floatTest = GetFloat(input);
        if (floatTest is not null)
            return (floatTest.Value.Float, floatTest.Value.Length);
        
        var intTest = GetInt(input);
        if (intTest is not null)
            return (intTest.Value.Int, intTest.Value.Length);
        
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