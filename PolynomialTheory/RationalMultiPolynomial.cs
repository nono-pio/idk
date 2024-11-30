namespace PolynomialTheory;

public class RationalMultiPolynomial<T> : IEquatable<RationalMultiPolynomial<T>> where T : IEquatable<T>
{
    
    public IRing<T> Ring => Numerator.Ring;
    public int NVars => Numerator.NVars;
    public MultiPolynomial<T> Numerator { get; }
    public MultiPolynomial<T> Denominator { get; }
    
    public RationalMultiPolynomial(MultiPolynomial<T> numerator, MultiPolynomial<T>? denominator = null)
    {

        if (numerator.IsZero())
        {
            Numerator = numerator;
            Denominator = MultiPolynomial<T>.One(numerator.Ring, numerator.NVars);
            return;
        }

        if (denominator is not null && denominator.IsConstant && numerator.Ring.IsField())
        {
            Numerator = numerator / denominator.Multinomials[0].Coef;
            Denominator = MultiPolynomial<T>.One(numerator.Ring, numerator.NVars);
            return;
        }
        
        if (numerator.Equals(denominator))
        {
            Numerator = MultiPolynomial<T>.One(numerator.Ring, numerator.NVars);
            Denominator = MultiPolynomial<T>.One(numerator.Ring, numerator.NVars);
            return;
        }
        
        denominator ??= MultiPolynomial<T>.One(numerator.Ring, numerator.NVars);
        
        Numerator = numerator;
        Denominator = denominator;
    }
    
    public static RationalMultiPolynomial<T> Zero(IRing<T> ring, int nVars) => new (MultiPolynomial<T>.Zero(ring, nVars));
    public static RationalMultiPolynomial<T> One(IRing<T> ring, int nVars) => new (MultiPolynomial<T>.One(ring, nVars));
    
    public static RationalMultiPolynomial<T> operator +(RationalMultiPolynomial<T> a, RationalMultiPolynomial<T> b)
    {
        return new RationalMultiPolynomial<T>(a.Numerator * b.Denominator + b.Numerator * a.Denominator, a.Denominator * b.Denominator);
    }
    
    public static RationalMultiPolynomial<T> operator -(RationalMultiPolynomial<T> a, RationalMultiPolynomial<T> b)
    {
        return new RationalMultiPolynomial<T>(a.Numerator * b.Denominator - b.Numerator * a.Denominator, a.Denominator * b.Denominator);
    }
    
    public static RationalMultiPolynomial<T> operator -(RationalMultiPolynomial<T> a)
    {
        return new RationalMultiPolynomial<T>(-a.Numerator, a.Denominator);
    }
    
    public static RationalMultiPolynomial<T> operator *(RationalMultiPolynomial<T> a, RationalMultiPolynomial<T> b)
    {
        return new RationalMultiPolynomial<T>(a.Numerator * b.Numerator, a.Denominator * b.Denominator);
    }

    public static RationalMultiPolynomial<T> operator *(RationalMultiPolynomial<T> a, int b)
    {
        return new RationalMultiPolynomial<T>(a.Numerator * b, a.Denominator);
    }

    
    public static RationalMultiPolynomial<T> operator /(RationalMultiPolynomial<T> a, RationalMultiPolynomial<T> b)
    {
        return new RationalMultiPolynomial<T>(a.Numerator * b.Denominator, a.Denominator * b.Numerator);
    }

    public static RationalMultiPolynomial<T> operator /(RationalMultiPolynomial<T> a, T b)
    {
        if (a.Ring.IsField())
            return new RationalMultiPolynomial<T>(a.Numerator / b, a.Denominator);
        
        return new RationalMultiPolynomial<T>(a.Numerator, a.Denominator * b);
    }

    
    public static RationalMultiPolynomial<T> operator /(int a, RationalMultiPolynomial<T> b)
    {
        return new RationalMultiPolynomial<T>(b.Denominator * a, b.Numerator);
    }
    
    public static RationalMultiPolynomial<T> operator %(RationalMultiPolynomial<T> a, RationalMultiPolynomial<T> b)
    {
        return Zero(a.Ring, a.NVars);
    }


    public bool Equals(RationalMultiPolynomial<T>? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Numerator.Equals(other.Numerator) && Denominator.Equals(other.Denominator);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((RationalMultiPolynomial<T>)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Numerator, Denominator);
    }

    public int Deg(int i) => Numerator.Deg(i) - Denominator.Deg(i);
    
    public string ToString(string[] vars, Func<T, string>? ringToString = null)
    {
        ringToString ??= t => t.ToString();
        var num = Numerator.IsConstant ? ringToString(Numerator.Multinomials[0].Coef) : Numerator.ToString(vars, ringToString);
        var den = Denominator.ToString(vars, ringToString);
        
        if (num.Contains('+') || num.Contains('-'))
            num = $"({num})";
        
        if (den.Contains('+') || den.Contains('-'))
            den = $"({num})";
        
        return den == "1" ? num : num + " / " + den;
    }

    public override string ToString()
    {
        return ToString(["x", "y", "z", "w"]);
    }

    /// T(x0, x1, x2, ...) -> T[x0, x1, ...](xi)
    public RationalUniPolynomial<MultiPolynomial<T>> ToUniPolynomial(int i)
    {
        var newNum = Numerator.ToUniPolynomial(i);
        var newDen = Denominator.ToUniPolynomial(i);

        return new RationalUniPolynomial<MultiPolynomial<T>>(newNum, newDen);
    }
    
    /// T(x0, x1, x2, ...) -> T(x0, x1, ...)(xi)
    public RationalUniPolynomial<RationalMultiPolynomial<T>> ToUniPolynomialOfRational(int i)
    {
        var newNum = Numerator.ToUniPolynomialOfRational(i);
        var newDen = Denominator.ToUniPolynomialOfRational(i);

        return new RationalUniPolynomial<RationalMultiPolynomial<T>>(newNum, newDen);
    }
    
    
    /// T(t0, t1, t2, ...)[ti] -> T(t0, t1, ...)
    public static RationalMultiPolynomial<T> CombineWithMultiPolynomial(UniPolynomial<RationalMultiPolynomial<T>> poly,
        int i)
    {
        var rmPolyRing = (RationalMultiPolynomialRing<T>) poly.Ring;
        var result = RationalMultiPolynomial<T>.Zero(rmPolyRing.Ring, rmPolyRing.NVars + 1);

        for (int j = 0; j < poly.Coefficients.Length; j++)
        {
            var rmpoly = poly.Coefficients[j];
            var sum = MultiPolynomial<T>.Zero(rmPolyRing.Ring, rmPolyRing.NVars + 1);
            foreach (var mul in rmpoly.Numerator.Multinomials)
            {
                var newDegs = new int[mul.Degs.Length + 1];
                for (int k = 0; k < newDegs.Length; k++)
                {
                    if (k < i)
                        newDegs[k] = mul.Degs[k];
                    if (k == i)
                        newDegs[k] = j;
                    if (k > i)
                        newDegs[k] = mul.Degs[k - 1];
                }

                var newMul = new Multinomial<T>(mul.Coef, newDegs);
                sum += new MultiPolynomial<T>(rmPolyRing.Ring, [newMul]);
            }

            var newDen = new MultiPolynomial<T>(rmPolyRing.Ring, rmpoly.Denominator.Multinomials.Select(mul =>
            {
                var newDegs = new int[mul.Degs.Length + 1];
                for (int k = 0; k < newDegs.Length; k++)
                {
                    if (k < i)
                        newDegs[k] = mul.Degs[k];
                    if (k == i)
                        newDegs[k] = 0;
                    if (k > i)
                        newDegs[k] = mul.Degs[k - 1];
                }
                
                return new Multinomial<T>(mul.Coef, newDegs);
            }).ToArray());
            
            result += new RationalMultiPolynomial<T>(sum, newDen);
        }
        
        return result;
    }

    // T(t0, t1, t2, ...)(ti) -> T(t0, t1, ...)
    public static RationalMultiPolynomial<T> CombineWithMultiPolynomial(RationalUniPolynomial<RationalMultiPolynomial<T>> poly,
        int i)
    {
        return CombineWithMultiPolynomial(poly.Numerator, i) / CombineWithMultiPolynomial(poly.Denominator, i);
    }

}