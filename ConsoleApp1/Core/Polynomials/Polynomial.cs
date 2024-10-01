using System.Text;
using ConsoleApp1.Core.Expressions.Atoms;

namespace ConsoleApp1.Core.Polynomials;

public class Polynomial
{
    public Expr[] Coefs { get; }
    public int Deg => Coefs.Length - 1;

    
    public Polynomial(params Expr[] coefficients)
    {
        Coefs = RemoveZeroes(coefficients);
    }
    
    private Expr[] RemoveZeroes(Expr[] coefs)
    {
        var i = coefs.Length - 1;
        while (i >= 0 && coefs[i].IsZero)
        {
            i--;
        }
        
        if (i == coefs.Length - 1)
            return coefs;
        
        if (i == -1)
            return [0];

        return coefs[..(i + 1)];
    }

    public static Polynomial MonomialToPolynomial(Expr coef, int deg)
    {
        var coefs = new Expr[deg + 1];
        Array.Fill(coefs, 0);
        coefs[deg] = coef;
        return new Polynomial(coefs);
    }

    public static Polynomial Zero => new([0]);
    public static Polynomial One => new([1]);
    public static Polynomial ValueOf(Expr value) => new([value]);

    public bool IsZero() => (Deg == 0 && Coefs[0].IsZero) || Coefs.Length == 0;
    public bool IsOne() => Deg == 1 && Coefs[0].IsOne;
    public bool IsInt(int value) => Deg == 1 && Coefs[0].Is(value);
    
    public Expr LC => Coefs[^1]; // Leading Coefficient
    public Expr CT => Coefs[0]; // Constant Term
    
    
    public Expr Eval(Expr x)
    {
        Expr result = 0;
        for (int i = 0; i <= Deg; i++)
        {
            result += Coefs[i] * Pow(x, i);
        }

        return result;
    }
    
    public Expr EvalHorner(Expr x)
    {
        Expr result = 0;
        for (int i = Deg; i >= 0; i--)
        {
            result = result * x + Coefs[i];
        }

        return result;
    }
    
    public static Polynomial operator +(Polynomial a, Polynomial b)
    {
        var deg = Math.Max(a.Deg, b.Deg);
        var coefs = new Expr[deg + 1];
        for (int i = 0; i <= deg; i++)
        {
            coefs[i] = 0;
            if (i <= a.Deg)
                coefs[i] += a.Coefs[i];
            if (i <= b.Deg)
                coefs[i] += b.Coefs[i];
        }

        return new Polynomial(coefs);
    }

    public static Polynomial operator -(Polynomial a, Polynomial b)
    {
        var deg = Math.Max(a.Deg, b.Deg);
        var coefs = new Expr[deg + 1];
        for (int i = 0; i <= deg; i++)
        {
            coefs[i] = 0;
            if (i <= a.Deg)
                coefs[i] += a.Coefs[i];
            if (i <= b.Deg)
                coefs[i] -= b.Coefs[i];
        }

        return new Polynomial(coefs);
    }

    public static Polynomial operator -(Polynomial a)
    {
        var coefs = new Expr[a.Deg + 1];
        for (int i = 0; i <= a.Deg; i++)
        {
            coefs[i] = -a.Coefs[i];
        }

        return new Polynomial(coefs);
    }

    public static Polynomial operator *(Polynomial a, Polynomial b)
    {
        var coefs = new Expr[a.Deg + b.Deg + 1];
        Array.Fill(coefs, 0);
        
        for (int i = 0; i <= a.Deg; i++)
        {
            for (int j = 0; j <= b.Deg; j++)
            {
                coefs[i + j] += a.Coefs[i] * b.Coefs[j];
            }
        }
        
        return new Polynomial(coefs);
    }
    
    public static Polynomial operator *(Polynomial a, Expr b)
    {
        var coefs = new Expr[a.Deg + 1];
        for (int i = 0; i <= a.Deg; i++)
        {
            coefs[i] = a.Coefs[i] * b;
        }

        return new Polynomial(coefs);
    }

    public static Polynomial operator /(Polynomial a, Polynomial b)
    {
        return DivRem(a, b).Quotient;
    }

    public static Polynomial operator /(Polynomial a, Expr b)
    {
        var coefs = new Expr[a.Deg + 1];
        for (int i = 0; i <= a.Deg; i++)
        {
            coefs[i] = a.Coefs[i] / b;
        }

        return new Polynomial(coefs);
    }

    public static Polynomial operator %(Polynomial a, Polynomial b)
    {
        var rem = RemCheck(a, b);
        if (rem != null)
            return rem;
        
        return DivRem(a, b).Remainder;
    }
    
    public static Polynomial? RemCheck(Polynomial n, Polynomial d)
    {
        if (n.Deg < d.Deg)
            return n;
        
        if (d.Deg == 0)
            return Zero;
        
        if (d.Deg == 1) { // Linear divisor
            var p = -d.CT / d.LC;
            return ValueOf(n.Eval(p));
        }
        
        return null;
    }
    
    public static (Polynomial Quotient, Polynomial Remainder) DivRem(Polynomial n, Polynomial d)
    {
        
        if (d.IsZero())
            throw new DivideByZeroException("Division by zero");
        
        if (n.IsZero())
            return (Zero, Zero);
        
        if (n.Deg < d.Deg)
            return (Zero, n);
        
        if (d.Deg == 0) 
            return (n / d.LC, Zero);
        
        var r = n;
        Expr[] q = new Expr[n.Deg - d.Deg + 1];

        for (int i = n.Deg - d.Deg; i >= 0; --i) {
            if (r.Deg == d.Deg + i) {
                var quot = r.LC / d.LC;
                q[i] = quot;
                r -= MonomialToPolynomial(q[i], i) * d;

            } else 
                q[i] = 0;
        }

        return (new Polynomial(q), r);
    }

    public static Polynomial Gcd(Polynomial a, Polynomial b)
    {
        // TODO: check if coef can be gcd and divide
        
        var g = Expr.Gcd(a.Content(), b.Content());
        
        var old_r = a / g;
        var r = b / g;
        
        while (!r.IsZero()){
            
            var new_r = old_r % r;
            old_r = r;
            r = new_r;
        }

        return old_r / old_r.LC * g;
    }

    public Expr Content()
    {
        return Expr.Gcd(Coefs);
    }
    
    public Polynomial PrimitivePart()
    {
        var content = Content();
        if (content.IsOne)
            return this;
        
        return this / content;
    }
    
    public (Expr Content, Polynomial Primitive) AsPrimitive()
    {
        var content = Content();
        if (content.IsOne)
            return (content, this);
        
        return (content, this / content);
    }

    public static (Polynomial Gcd, Polynomial X, Polynomial Y) ExtendedGcd(Polynomial a, Polynomial b){
        
        var (old_r, r) = (a, b);
        var (old_s, s) = (One, Zero);
        var (old_t, t) = (Zero, One);
        
        while (!r.IsZero())
        {
            var (q, new_r) = DivRem(old_r, r);
            (old_r, r) = (r, new_r);
            (old_s, s) = (s, old_s - q * s);
            (old_t, t) = (t, old_t - q * t);
        }

        var lc_r = old_r.LC;
        old_r /= lc_r;
        old_s /= lc_r;
        old_t /= lc_r;
        
        return (old_r, old_s, old_t);
    }

    public Polynomial Derivee()
    {
        var coefs = new Expr[Deg];
        
        for (int i = 1; i <= Deg; i++)
        {
            coefs[i - 1] = Coefs[i] * i;
        }
        
        return new Polynomial(coefs);
    }
    
    public Polynomial[] YunSquareFree()
    {
        var f = this;
        var df = f.Derivee();
        var a = new List<Polynomial> { Gcd(f, df) };

        var b = f / a[0];
        var c = df / a[0];
        var d = c - b.Derivee();
        var i = 1;

        while (b.Deg > 0)
        {
            a.Add(Gcd(b, d));
            b = b / a[i];
            c = d / a[i];
            d = c - b.Derivee();

            i++;
        }
        
        a.RemoveAt(0);
        return a.ToArray();
    }
    
    public struct PolyFactors
    {
        public Expr Content;
        public List<(Polynomial Factor, int Exp)> Factors { get; }
        
        public PolyFactors(Expr content, List<(Polynomial, int)> factors)
        {
            Content = content;
            Factors = factors;
        }
        
        public void AddFactor(Polynomial factor, int exp)
        {
            for (int i = 0; i < Factors.Count; i++)
            {
                if (Factors[i].Factor == factor)
                {
                    Factors[i] = (factor, Factors[i].Exp + exp);
                    return;
                }
            }
            
            Factors.Add((factor, exp));
        }
        
        public void AddFactorization(PolyFactors factorization, int exp)
        {
            Content *= Pow(factorization.Content, exp);
            
            foreach (var (f, e) in factorization.Factors)
            {
                Factors.Add((f, e * exp));
            }
        }
        
        public override string ToString()
        {
            return $"{Content}*{string.Join("*", Factors.Select(f => $"({f.Factor})^{f.Exp}"))}";
        }
    }
    
    public static PolyFactors Factorization(Polynomial P)
    {
        
        // P(x)x^n
        var n = CTDeg();
        if (n > 0)
            P = P.ShiftRight(n);
        // todo: add to fac objects
        
        if (P.Deg == 0) // cste
            return new PolyFactors(P.Coefs[0], []);

        if (P.Deg <= 4)
        {
            var lc = P.LC;

            var solutions = P.SolveDegree();
            var for_factors = new PolyFactors(lc, []);
            foreach (var sol in solutions)
            {
                for_factors.AddFactor(new Polynomial([-sol, 1]), 1);
            }

            return for_factors;
        }
        
        
        (var content, P) = P.AsPrimitive();
        
        var squareFree = P.YunSquareFree();
        if (squareFree.Length == 1) // cannot be factored by this algorithm
            return new PolyFactors(content, [(P, 1)]);

        var factors = new PolyFactors(content, []);
        foreach (var (f, deg) in squareFree.Select((fac, i) => (fac, i+1)))
        {
            factors.AddFactorization(Factorization(f), deg);
        }

        return factors;
    }

    public override string ToString()
    {
        if (IsZero())
            return "0";
        
        var sb = new StringBuilder();
        bool needPlus = false;
        for (int i = 0; i < Coefs.Length; i++)
        {
            if (Coefs[i].IsZero)
                continue;
            
            if (needPlus)
                sb.Append(" + ");

            if (!Coefs[i].IsOne || i == 0)
            {
                sb.Append(Coefs[i]);
                needPlus = true;
            }
            
            switch (i)
            {
                case 0:
                    continue;
                case 1:
                    sb.Append('x');
                    continue;
                default:
                    sb.Append($"x^{i}");
                    break;
            }
        }

        return sb.ToString();
    }

    #region Solve Degree 1,2,3,4

    public Expr[] SolveDegree()
    {
        return Deg switch
        {
            0 => Array.Empty<Expr>(),
            1 => SolveDegree1(),
            2 => SolveDegree2(),
            3 => SolveDegree3(),
            4 => SolveDegree4(),
            _ => throw new Exception("Degree not supported")
        };
    }
    
    public Expr[] SolveDegree1() => Deg == 1 ? SolveDegree1(Coefs[1], Coefs[0]) : throw new ArgumentException("Not a degree 1 polynomial");

    public static Expr[] SolveDegree1(Expr a, Expr b)
    {
        return [ -b/a ];
    }
    
    public Expr[] SolveDegree2() => Deg == 2 ? SolveDegree2(Coefs[2], Coefs[1], Coefs[0]) : throw new ArgumentException("Not a degree 2 polynomial");
    public static Expr[] SolveDegree2(Expr a, Expr b, Expr c, bool complex=true)
    {
        var delta = Pow(b, 2) - 4*a*c;
        if (delta.IsZero)
        {
            var sol = -b / (2*a);
            return [sol, sol];
        }
        
        if (!complex && delta.IsNegative)
            return Array.Empty<Expr>();
        
        var sqrtDelta = Sqrt(delta);
        return [ (-b + sqrtDelta) / (2*a), (-b - sqrtDelta) / (2*a) ];
    }
    
    public Expr[] SolveDegree3() => Deg == 3 ? SolveDegree3(Coefs[3], Coefs[2], Coefs[1], Coefs[0]) : throw new ArgumentException("Not a degree 3 polynomial");
    public static Expr[] SolveDegree3(Expr a, Expr b, Expr c, Expr d, bool complex=true)
    {
        var p = c/a - Pow(b, 2)/(3*Pow(a, 2));
        var q = 2*Pow(b, 3)/(27*Pow(a, 3)) - b*c/(3*Pow(a, 2)) + d/a;
        
        var delta = -27*Pow(q, 2) - 4*Pow(p, 3);
        var sqrtDelta = Sqrt(-delta/27);
        var u = Cbrt((-q + sqrtDelta) / 2);
        var v = Cbrt((-q - sqrtDelta) / 2);
        
        if (!complex && delta.IsNegative)
            return [ u + v - b/(3*a) ];
        
        var j = Exp(2 * Constant.PI * I / 3);
        var roots = new Expr[3];
        for (int k = 0; k < 3; k++)
        {
            roots[k] = Pow(j, k) * u + Pow(j, -k) * v - b/(3*a);
        }

        return roots;
    }
    
    public Expr[] SolveDegree4() => Deg == 4 ? SolveDegree4(Coefs[4], Coefs[3], Coefs[2], Coefs[1], Coefs[0]) : throw new ArgumentException("Not a degree 4 polynomial");
    public static Expr[] SolveDegree4(Expr a, Expr b, Expr c, Expr d, Expr e, bool complex=true)
    {
        // Algorithm from https://en.wikipedia.org/wiki/Quartic_function#General_formula_for_roots

        var p = (8*a*c-3*Pow(b, 2))/(8*Pow(a, 2));
        var q = (Pow(b, 3) - 4*a*b*c + 8*Pow(a, 2)*d)/(8*Pow(a, 3));
        
        var delta0 = Pow(c, 2) - 3*b*d + 12*a*e;
        var delta1 = 2*Pow(c, 3) - 9*b*c*d + 27*Pow(b, 2)*e + 27*a*Pow(d, 2) - 72*a*c*e;
        
        var Q = Cbrt((delta1 + Sqrt(Pow(delta1, 2) - 4 * Pow(delta0, 3))) / 2); // todo Q=0
        var S = Sqrt(-2*p/3 + (Q+delta0/Q)/(3*a)) / 2; // todo S=0
        
        var finalSqrt = Sqrt(-4 * Pow(S, 2) - 2 * p + q / S);
        
        // todo complex/real roots
        
        var x1 = -b/(4*a) - S + finalSqrt/2;
        var x2 = -b/(4*a) - S - finalSqrt/2;
        var x3 = -b/(4*a) + S + finalSqrt/2;
        var x4 = -b/(4*a) + S - finalSqrt/2;

        return [x1, x2, x3, x4];
    }

    #endregion
}