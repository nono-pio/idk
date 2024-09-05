using System.Net.NetworkInformation;
using System.Text;

namespace ConsoleApp1.Core.Models;

public struct MultiNomial
{
    public Expr Coef;
    public int[] Powers;

    public static MultiNomial Zero => new(0, []);
    public static MultiNomial One => new(1, []);

    
    public MultiNomial(Expr coef, int[] powers)
    {
        Coef = coef;
        Powers = powers;
    }
}

public class PolyOfMultiPoly
{
    public MultiPoly[] Coefs;
    
    public PolyOfMultiPoly(params MultiPoly[] coefs)
    {
        Coefs = coefs;
    }
    
    public static PolyOfMultiPoly FromMultiPoly(MultiPoly poly, int varIndex)
    {
        MultiPoly RemoveVar(MultiNomial mono)
        {
            var powers = mono.Powers.ToList();
            powers.RemoveAt(varIndex);
            return new(new MultiNomial(mono.Coef, powers.ToArray()));
        }
        
        var coefs = new MultiPoly[poly.Deg(varIndex) + 1];
        Array.Fill(coefs, MultiPoly.Zero);
        foreach (var term in poly.Terms)
        {
            coefs[term.Powers[varIndex]] += RemoveVar(term);
        }

        return new PolyOfMultiPoly(coefs);
    }

    public (MultiPoly, PolyOfMultiPoly) Primitive()
    {
        var c = MultiPoly.Gcd(Coefs);
        var newCoefs = Coefs.Select(coef => coef / c).ToArray();
        return (c, new PolyOfMultiPoly(newCoefs));
    }
    
    public MultiPoly ToMultiPoly(int varIndex)
    {
        var terms = new List<MultiNomial>();
        for (int i = 0; i < Coefs.Length; i++)
        {
            var mono = Coefs[i];
            if (mono.IsZero())
                continue;

            for (int k = 0; k < mono.Terms.Length; k++)
            {
                var powers = new int[mono.nVars + 1];
                for (int j = 0; j < mono.nVars; j++)
                {
                    if (j < varIndex)
                        powers[j] = mono.Terms[k].Powers[j];
                    else
                        powers[j+1] = mono.Terms[k].Powers[j];
                }
                powers[varIndex] = i;
                terms.Add(new MultiNomial(mono.Terms[k].Coef, powers));    
            }
        }

        return new MultiPoly(terms.ToArray());
    }

    public static PolyOfMultiPoly Gcd(PolyOfMultiPoly a, PolyOfMultiPoly b)
    {
        throw new NotImplementedException();
    }

    public int Deg()
    {
        return Coefs.Length - 1;
    }
    
    public PolyOfMultiPoly Derivee()
    {
        throw new NotImplementedException();
    }
    
    public static PolyOfMultiPoly operator +(PolyOfMultiPoly a, PolyOfMultiPoly b)
    {
        if (a.Coefs.Length != b.Coefs.Length)
            throw new ArgumentException("Both polynomials must have the same number of coefficients");
        
        var coefs = new MultiPoly[a.Coefs.Length];
        for (int i = 0; i < a.Coefs.Length; i++)
        {
            coefs[i] = a.Coefs[i] + b.Coefs[i];
        }

        return new PolyOfMultiPoly(coefs);
    }
    
    public static PolyOfMultiPoly operator -(PolyOfMultiPoly a, PolyOfMultiPoly b)
    {
        if (a.Coefs.Length != b.Coefs.Length)
            throw new ArgumentException("Both polynomials must have the same number of coefficients");
        
        var coefs = new MultiPoly[a.Coefs.Length];
        for (int i = 0; i < a.Coefs.Length; i++)
        {
            coefs[i] = a.Coefs[i] - b.Coefs[i];
        }

        return new PolyOfMultiPoly(coefs);
    }
    
    public static PolyOfMultiPoly operator *(PolyOfMultiPoly a, PolyOfMultiPoly b)
    {
        var coefs = new MultiPoly[a.Coefs.Length + b.Coefs.Length - 1];
        for (int i = 0; i < a.Coefs.Length; i++)
        {
            for (int j = 0; j < b.Coefs.Length; j++)
            {
                coefs[i+j] += a.Coefs[i] * b.Coefs[j];
            }
        }

        return new PolyOfMultiPoly(coefs);
    }
    
    public static PolyOfMultiPoly operator /(PolyOfMultiPoly a, PolyOfMultiPoly b)
    {
        throw new NotImplementedException();
    }
}

public class MultiPoly
{
    public static MultiPoly Zero => new(MultiNomial.Zero);    
    public static MultiPoly One => new(MultiNomial.One);

    public Expr[]? Vars;
    public int nVars; // -1 if poly = 0
    public MultiNomial[] Terms;

    public MultiPoly(params MultiNomial[] terms) : this(null, terms) { }
    public MultiPoly(Expr[]? vars, params MultiNomial[] terms)
    {
        // TODO: combine monomial with same powers
        if (terms.Length == 0)
            throw new ArgumentException("A polynomial must have at least one term");
        
        Vars = vars;
        Terms = terms;
        nVars = Vars?.Length ?? Terms[0].Powers.Length;

        if (Terms.Any(mono => mono.Powers.Length != nVars))
            throw new ArgumentException("All monomials must have the same number of powers as the number of variables");
    }

    public MultiPoly(Poly poly)
    {
        Vars = null;
        nVars = 1;
        Terms = poly.AsCoefDeg().Select(coefPow => new MultiNomial(coefPow.Item1, [coefPow.Item2])).ToArray();
    }

    
    public bool IsZero() => Terms.Length == 1 && Terms[0].Coef == 0;
    public bool IsOne() => Terms.Length == 1 && Terms[0].Coef == 1;

    public int GetVarIndex(Expr x)
    {
        if (Vars is null)
            throw new Exception("Variables are not defined for this polynomial");
        
        for (int i = 0; i < Vars.Length; i++)
        {
            if (Vars[i] == x)
                return i;
        }
        
        throw new ArgumentException("Variable not found");
    }

    public bool DependsOn(int varIndex)
    {
        return Terms.Any(mono => mono.Powers[varIndex] != 0);
    }

    public PolyOfMultiPoly ToPoly(int varIndex)
    {
        return PolyOfMultiPoly.FromMultiPoly(this, varIndex);
    }

    
    public Poly? ToPoly()
    {
        var varIndex = -1;
        for (int i = 0; i < nVars; i++)
        {
            if (DependsOn(i))
            {
                if (varIndex != -1)
                    return null;
                varIndex = i;
            }
        }
        
        
        var deg = Deg(varIndex);
        var poly = Poly.VideDeg(deg);
        foreach (var mono in Terms)
        {
            poly.SetCoefDeg(mono.Powers[varIndex], mono.Coef);
        }
        
        return poly;
    }
    
    public int Deg()
    {
        throw new NotImplementedException();
    }

    public int Deg(Expr x) => Deg(GetVarIndex(x));
    public int Deg(int varIndex)
    {
        return Terms.Min(mono => mono.Powers[varIndex]);
    }

    public MultiPoly Derivee(Expr x) => Derivee(GetVarIndex(x));
    public MultiPoly Derivee(int varIndex)
    {
        var terms = new List<MultiNomial>();
        foreach (var term in Terms)
        {
            var powers = term.Powers;
            if (powers[varIndex] == 0)
                continue;
            
            var newPowers = powers.ToArray();
            newPowers[varIndex] -= 1;
            terms.Add(new MultiNomial(term.Coef * powers[varIndex], newPowers));
        }

        return new MultiPoly(Vars, terms.ToArray());
    }

    public static MultiPoly Gcd(IEnumerable<MultiPoly> polys)
    {
        var apolys = polys.ToArray();
        AssertVars(apolys);
        
        if (apolys.Length == 0)
            throw new ArgumentException("No polynomials to compute the gcd of");

        if (apolys.Length == 1)
            return apolys[0];

        if (apolys.Any(p => p.IsOne()))
            return One;
        
        var gcd = apolys[0];
        for (int i = 1; i < apolys.Length && !gcd.IsOne(); i++)
        {
            gcd = Gcd(gcd, apolys[i]);
        }

        return gcd;
    }
    public static MultiPoly Gcd(MultiPoly a, MultiPoly b)
    {
        AssertVars(a, b);

        var apoly = a.ToPoly();
        var bpoly = b.ToPoly();
        if (apoly is not null && bpoly is not null)
        {
            var gcd = Poly.Gcd(apoly, bpoly);
            return new MultiPoly(gcd);
        }

        throw new NotImplementedException();
    }
    
    public static MultiPoly Lcm(IEnumerable<MultiPoly> polys)
    {
        var apolys = polys.ToArray();
        var gcd = Gcd(apolys);

        var pro = apolys.Aggregate(One, (a, b) => a * b);
        return gcd.IsOne() ? pro : pro / gcd;
    }
    
    public static MultiPoly Lcm(MultiPoly a, MultiPoly b)
    {
        var gcd = Gcd(a, b);
        var pro = a * b;
        return gcd.IsOne() ? pro : pro / gcd;
    }
    
    public static void AssertVars(MultiPoly a, MultiPoly b)
    {
        if (a.nVars != b.nVars)
            throw new ArgumentException("Both polynomials must have the same number of variables");

        if (a.Vars is null || b.Vars is null)
            return;
        
        if (!a.Vars.SequenceEqual(b.Vars))
            throw new ArgumentException("Both polynomials must have the same variables");
    }

    public static void AssertVars(MultiPoly[] polys)
    {
        if (polys.Length <= 1)
            return;
        
        var nVars = polys[0].nVars;
        var vars = polys[0].Vars;

        for (int i = 1; i < polys.Length; i++)
        {
            var p = polys[i];
            if (p.nVars != nVars)
                throw new ArgumentException("All polynomials must have the same number of variables");
            
            if (p.Vars is null)
                continue;

            if (vars is null)
            {
                vars = p.Vars;
                continue;
            }
            
            if (!vars.SequenceEqual(p.Vars))
                throw new ArgumentException("All polynomials must have the same variables");
        }
        
    }
    
    public static MultiPoly operator +(MultiPoly a, MultiPoly b)
    {
        AssertVars(a, b);
        
        var terms = a.Terms.ToList();
        foreach (var term in b.Terms)
        {
            var index = terms.FindIndex(mono => mono.Powers.SequenceEqual(term.Powers));
            if (index == -1)
                terms.Add(term);
            else
            {
                var newCoef = terms[index].Coef + term.Coef;
                if (newCoef.IsZero)
                {
                    terms.RemoveAt(index);
                    continue;
                }
                terms[index] = new MultiNomial(newCoef, term.Powers);
            }
        }
        
        return new MultiPoly(a.Vars, terms.ToArray());
    }
    
    public static MultiPoly operator -(MultiPoly a, MultiPoly b)
    {
        AssertVars(a, b);
        
        var terms = a.Terms.ToList();
        foreach (var term in b.Terms)
        {
            var index = terms.FindIndex(mono => mono.Powers.SequenceEqual(term.Powers));
            if (index == -1)
                terms.Add(new MultiNomial(-term.Coef, term.Powers));
            else
            {
                var newCoef = terms[index].Coef - term.Coef;
                if (newCoef.IsZero)
                {
                    terms.RemoveAt(index);
                    continue;
                }
                terms[index] = new MultiNomial(newCoef, term.Powers);
            }
        }
        
        return new MultiPoly(a.Vars, terms.ToArray());
    }
    
    public static MultiPoly operator *(MultiPoly a, MultiPoly b)
    {
        AssertVars(a, b);
        
        var terms = new List<MultiNomial>();
        foreach (var termA in a.Terms)
        {
            foreach (var termB in b.Terms)
            {
                var coef = termA.Coef * termB.Coef;
                var powers = new int[a.nVars];
                for (int i = 0; i < a.nVars; i++)
                {
                    powers[i] = termA.Powers[i] + termB.Powers[i];
                }
                
                terms.Add(new MultiNomial(coef, powers));
            }
        }

        return new MultiPoly(a.Vars, terms.ToArray());
    }
    
    public static MultiPoly operator /(MultiPoly a, MultiPoly b)
    {
        AssertVars(a, b);
        
        var apoly = a.ToPoly();
        var bpoly = b.ToPoly();
        if (apoly is not null && bpoly is not null)
        {
            var div = Poly.Div(apoly, bpoly).Item1;
            return new MultiPoly(div);
        }

        throw new NotImplementedException();
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var term in Terms)
        {
            sb.Append(term.Coef);
            for (int i = 0; i < term.Powers.Length; i++)
            {
                if (term.Powers[i] == 0)
                    continue;
                
                sb.Append(Vars?[i] ?? $"x{i}");
                if (term.Powers[i] != 1)
                {
                    sb.Append('^');
                    sb.Append(term.Powers[i]);
                }
            }
            sb.Append(" + ");
        }

        return sb.Remove(sb.Length - 3, 3).ToString();
    }
}