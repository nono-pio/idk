namespace ConsoleApp1.Parser;

public class ParserHelper
{

    public static Parsed<T> Or<T>(string input, params Func<string, Parsed<T>>[] parsers)
    {
        foreach (var parse in parsers)
        {
            var result = parse(input);
            if (!result.IsNull)
                return result;
        }

        return Parsed<T>.Null;
    }

    public static Parsed<T> Sequence<T, T1, T2>(string input, Func<string, Parsed<T1>> first, 
        Func<string, Parsed<T2>> second, Func<T1, T2, T> result)
    {
        var firstParse = first(input);
        if (firstParse.IsNull)
            return Parsed<T>.Null;

        var secondParse = second(input[firstParse.Length..]);
        if (secondParse.IsNull)
            return Parsed<T>.Null;

        return new(result(firstParse.Value, secondParse.Value), firstParse.Length + secondParse.Length);
    }
    
    public static Parsed<string> ParseString(string input, string pattern)
    {
        if (input.StartsWith(pattern))
            return new(pattern, pattern.Length);

        return Parsed<string>.Null;
    }
    
    public static Parsed<string> ParseStrings(string input, params string[] patterns)
    {
        foreach (var pattern in patterns)
        {
            if (input.StartsWith(pattern))
                return new(pattern, pattern.Length);
        }
        
        return Parsed<string>.Null;
    }

    

    public static Parsed<T> ParseBetween<T>(string input, string start, string end,
        Func<string, Parsed<T>> parseInside)
    {
        
        if (!input.StartsWith(start))
            return Parsed<T>.Null;
        
        for (int j = start.Length; j < input.Length; j++)
        {
            if (input.ContainsAt(end, j))
            {
                var insideParse = parseInside(input[start.Length..j]);
                if (insideParse.IsNull)
                    return Parsed<T>.Null;
                
                return new(insideParse.Value, j + end.Length);
            }
        }
        
        return Parsed<T>.Null;
    }
    
    public static Parsed<T> ParseBetweenRecursive<T>(string input, string start, string end, Func<string, Parsed<T>> parseInside)
    {
        
        if (!input.StartsWith(start))
            return Parsed<T>.Null;
        
        var deep = 0;
        for (int j = 0; j < input.Length; j++)
        {
            if (input.ContainsAt(start, j))
            {
                deep++;
                j += start.Length-1;
                continue;
            }
            
            if (input.ContainsAt(end, j))
            {
                deep--;
                if (deep == 0)
                {
                    var insideParse = parseInside(input[start.Length..j]);
                    if (insideParse.IsNull)
                        return Parsed<T>.Null;
                    
                    return new(insideParse.Value, j + end.Length);
                }
                j += end.Length-1;
            }
        }
        
        return Parsed<T>.Null;
    }


    public static Parsed<T[]> ParseList<T>(string input, string separator, Func<string, Parsed<T>> parse)
    {
        var last = 0;
        List<T> elements = new();
        for (int i = 0; i < input.Length; i++)
        {
            if (input.ContainsAt(separator, i))
            {

                var parsed = parse(input[last..i]);
                if (parsed.IsNull)
                    return Parsed<T[]>.Null;
                
                elements.Add(parsed.Value);
                
                i += separator.Length;
                last = i;
            }
        }

        var lastParsed = parse(input[last..]);
        if (lastParsed.IsNull)
            return Parsed<T[]>.Null;
        
        elements.Add(lastParsed.Value);

        return new(elements.ToArray(), input.Length);
    }

    public static Parsed<int> ParseDigit(string input)
    {
        // Digit -> [0-9]

        if (input.Length == 0 || !char.IsDigit(input[0]))
            return Parsed<int>.Null;
        
        return new(input[0] - '0', 1);
    }
    
    public static Parsed<long> ParseLong(string input)
    {
        // INT -> -? [0-9]+

        if (input.Length == 0)
            return Parsed<long>.Null;
        
        var i = 0;
        
        // -?
        long sign = 1;
        if (input[i] == '-')
        {
            sign = -1;
            i++;
        }

        var result = 0;
        while (i < input.Length && char.IsDigit(input[i]))
        {
            result = result * 10 + (input[i] - '0');
            i++;
        }

        return new(sign * result, i);
    }
    
    public static Parsed<double> ParseFloat(string input)
    {
        // FLOAT -> -? [0-9]+ [.,] [0-9]* EXP:([eE][+-]?[0-9]+)?
        
        if (input.Length == 0)
            return Parsed<double>.Null;
        
        var i = 0;
        
        // -?
        long sign = 1;
        if (input[i] == '-')
        {
            sign = -1;
            i++;
        }
        
        // [0-9]+
        var intTest = ParseLong(input[i..]);
        if (intTest.IsNull)
            return Parsed<double>.Null;
        
        var intNumber = intTest.Value;
        i += intTest.Length;
        
        // [.,]
        if (i >= input.Length || (input[i] != '.' && input[i] != ','))
            return Parsed<double>.Null;
        
        i++;
        
        // [0-9]*
        var decimalTest = ParseLong(input[i..]);

        long decimalNumber;
        if (decimalTest.IsNull)
            decimalNumber = 0;
        else
        {
            decimalNumber = decimalTest.Value;
            i += decimalTest.Length;
        }

        double currentFloat = sign * (intNumber + (double)decimalNumber / Math.Pow(10, decimalNumber.ToString().Length));
        
        // EXP:([eE][+-]?[0-9]+)?
        
        if (i+1 >= input.Length)
            return new(currentFloat, i);
        
        // [eE]
        if (input[i] != 'e' && input[i] != 'E')
            return new(currentFloat, i);
        
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
        var expTest = ParseLong(input[i..]);
        if (expTest.IsNull)
            return Parsed<double>.Null;
        
        var expNumber = expTest.Value;
        i += expTest.Length;
        
        return new(currentFloat * Math.Pow(10, expSign * expNumber), i);
    }
    
}