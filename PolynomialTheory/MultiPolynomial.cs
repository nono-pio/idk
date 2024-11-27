namespace PolynomialTheory;

public class MultiPolynomial<T> : IEquatable<MultiPolynomial<T>> where T : IEquatable<T>
{
    public IRing<T> Ring;
    public int NVars;
    public Multinomial<T>[] Multinomials;

    public MultiPolynomial(IRing<T> ring, Multinomial<T>[] multinomials)
    {
        
        if (multinomials.Length == 0)
            throw new ArgumentException();
        
        NVars = multinomials[0].Degs.Length;

        multinomials = multinomials.Where(m => !ring.IsZero(m.Coef)).ToArray();
        if (multinomials.Length == 0)
            multinomials = [new Multinomial<T>(ring.Zero, new int[NVars])];
        
        Ring = ring;
        Multinomials = multinomials;
    }

    public static MultiPolynomial<T> Zero(IRing<T> ring, int nVars) => new MultiPolynomial<T>(ring, [new Multinomial<T>(ring.Zero, new int[nVars])]);
    public static MultiPolynomial<T> One(IRing<T> ring, int nVars) => new MultiPolynomial<T>(ring, [new Multinomial<T>(ring.One, new int[nVars])]);

    public bool IsZero() => Multinomials.Length == 1 && Ring.IsZero(Multinomials[0].Coef);
    
    
    public static MultiPolynomial<T> operator +(MultiPolynomial<T> a, MultiPolynomial<T> b)
    {
        var newMul = new List<Multinomial<T>>(a.Multinomials);
        var ring = a.Ring;
        
        foreach (var bMul in b.Multinomials)
        {
            var added = false;
            for (int i = 0; i < newMul.Count; i++)
            {
                if (bMul.Degs.SequenceEqual(newMul[i].Degs))
                {
                    newMul[i] = new Multinomial<T>(ring.Add(bMul.Coef, newMul[i].Coef), bMul.Degs);
                    added = true;
                    break;
                }
            }
            
            if (!added) 
                newMul.Add(bMul);
        }

        return new MultiPolynomial<T>(ring, newMul.ToArray());
    }
    
    public static MultiPolynomial<T> operator -(MultiPolynomial<T> a, MultiPolynomial<T> b)
    {
        var newMul = new List<Multinomial<T>>(a.Multinomials);
        var ring = a.Ring;
        
        foreach (var bMul in b.Multinomials)
        {
            var added = false;
            for (int i = 0; i < newMul.Count; i++)
            {
                if (bMul.Degs.SequenceEqual(newMul[i].Degs))
                {
                    newMul[i] = new Multinomial<T>(ring.Subtract(bMul.Coef, newMul[i].Coef), bMul.Degs);
                    added = true;
                    break;
                }
            }
            
            if (!added) 
                newMul.Add(new Multinomial<T>(ring.Negate(bMul.Coef), bMul.Degs));
        }

        return new MultiPolynomial<T>(ring, newMul.ToArray());
    }

    public static MultiPolynomial<T> operator *(MultiPolynomial<T> a, MultiPolynomial<T> b)
    {
        var ring = a.Ring;
        var result = Zero(ring, a.NVars);

        foreach (var aMul in a.Multinomials)
        {
            foreach (var bMul in b.Multinomials)
            {
                var newDegs = new int[a.NVars];
                for (int i = 0; i < a.NVars; i++)
                    newDegs[i] = aMul.Degs[i] + bMul.Degs[i];
                
                var newMul = new Multinomial<T>(ring.Multiply(aMul.Coef, bMul.Coef), newDegs);
                result += new MultiPolynomial<T>(ring, [newMul]);
            }
        }

        return result;
    }

    public static MultiPolynomial<T> operator *(MultiPolynomial<T> a, int b)
    {
        var ring = a.Ring;
        return new MultiPolynomial<T>(ring,
            a.Multinomials.Select(m => new Multinomial<T>(ring.Multiply(m.Coef, b), m.Degs)).ToArray());
    }
    
    public static MultiPolynomial<T> operator -(MultiPolynomial<T> a)
    {
        return new MultiPolynomial<T>(a.Ring, a.Multinomials.Select(m => new Multinomial<T>(a.Ring.Negate(m.Coef), m.Degs)).ToArray());
    }

    public string ToString(string[] vars)
    {
        return string.Join(" + ", Multinomials.Select(mul => mul.Coef + mul.Degs.Select((deg, i) => deg == 0 ? "" : deg == 1 ? vars[i] : vars[i] + "^" + deg).Aggregate("", (acc, t) => acc + t)));
    }

    public override string ToString()
    {
        return ToString(["x", "y", "z", "t", "w", "s"]);
    }

    public bool Equals(MultiPolynomial<T>? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Ring.Equals(other.Ring) && Multinomials.Equals(other.Multinomials);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((MultiPolynomial<T>)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Ring, Multinomials);
    }

    public MultiPolynomial<T> Pow(int exp)
    {
        if (exp == 0)
            return One(Ring, NVars);
        
        var result = this;
        for (int i = 1; i < exp; i++)
            result *= this;

        return result;
    }

    public RationalMultiPolynomial<T> ToRationalPoly()
    {
        return new RationalMultiPolynomial<T>(this);
    }
}