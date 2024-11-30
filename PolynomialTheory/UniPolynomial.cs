namespace PolynomialTheory;

public class UniPolynomial<T> : IEquatable<UniPolynomial<T>> where T : IEquatable<T>
{
    public readonly IRing<T> Ring;
    public T[] Coefficients { get; } // index = degree
    public int Degree => Coefficients.Length - 1;
    public T LC => Coefficients[^1];


    public UniPolynomial(IRing<T> ring, T[] coefficients)
    {
        if (coefficients.Length == 0)
            coefficients = [ring.Zero];

        if (ring.IsZero(coefficients[^1]))
            coefficients = RemoveUnnecessaryCoefficients(ring, coefficients);

        Coefficients = coefficients;
        Ring = ring;
    }

    public static UniPolynomial<T> Zero(IRing<T> ring) => new UniPolynomial<T>(ring, [ring.Zero]);
    public static UniPolynomial<T> One(IRing<T> ring) => new UniPolynomial<T>(ring, [ring.One]);

    public static T[] RemoveUnnecessaryCoefficients(IRing<T> ring, T[] coefficients)
    {
        int i = coefficients.Length - 1;
        while (i >= 0 && ring.IsZero(coefficients[i]))
            i--;

        if (i == -1)
            return [ring.Zero];

        return coefficients[..(i + 1)];
    }

    public bool IsZero() => Ring.IsZero(LC);

    public bool IsOne() => Degree == 0 && Coefficients[0].Equals(Ring.One);

    /// Divide the dividend by the divisor, and return the quotient and the remainder
    /// Exception
    /// - ArgumentException : if the divisor.LC is zero
    /// - ArgumentException : if the divide in the ring is not possible
    public static (UniPolynomial<T> Quotient, UniPolynomial<T> Remainder) Divide(IRing<T> ring,
        UniPolynomial<T> dividend, UniPolynomial<T> divisor)
    {
        var dividendCopy = new List<T>(dividend.Coefficients);
        var divisorLC = divisor.LC;

        if (ring.IsZero(divisorLC))
            throw new ArgumentException("Le diviseur ne peut pas être nul.");

        if (dividend.Degree < divisor.Degree)
            return (new UniPolynomial<T>(ring, [ring.Zero]), dividend);

        var quotient = new T[dividend.Degree - divisor.Degree + 1];
        Array.Fill(quotient, ring.Zero);

        while (dividendCopy.Count >= divisor.Coefficients.Length)
        {
            int degreeDifference = dividendCopy.Count - divisor.Coefficients.Length;
            var leadingQuotient = ring.Divide(dividendCopy[^1], divisorLC);

            quotient[degreeDifference] = leadingQuotient;

            for (int i = 0; i <= divisor.Degree; i++)
            {
                int index = degreeDifference + i;
                dividendCopy[index] = ring.Subtract(dividendCopy[index],
                    ring.Multiply(leadingQuotient, divisor.Coefficients[i]));
            }

            while (dividendCopy.Count > 0 && ring.IsZero(dividendCopy[^1]))
                dividendCopy.RemoveAt(dividendCopy.Count - 1);
        }

        return (new UniPolynomial<T>(ring, quotient), new UniPolynomial<T>(ring, dividendCopy.ToArray()));
    }

    public static (UniPolynomial<T> Quotient, UniPolynomial<T> Remainder) PseudoDivide(IRing<T> ring,
        UniPolynomial<T> dividend, UniPolynomial<T> divisor)
    {
        var bN = ring.Pow(divisor.LC, dividend.Degree - divisor.Degree + 1);

        var dividendCopy = new List<T>(dividend.Coefficients.Select(c => ring.Multiply(c, bN)));
        var divisorLC = divisor.LC;

        if (ring.IsZero(divisorLC))
            throw new ArgumentException("Le diviseur ne peut pas être nul.");

        if (dividend.Degree < divisor.Degree)
            return (new UniPolynomial<T>(ring, [ring.Zero]), dividend);

        var quotient = new T[dividend.Degree - divisor.Degree + 1];

        while (dividendCopy.Count >= divisor.Coefficients.Length)
        {
            int degreeDifference = dividendCopy.Count - divisor.Coefficients.Length;
            var leadingQuotient = ring.Divide(dividendCopy[^1], divisorLC);

            quotient[degreeDifference] = leadingQuotient;

            for (int i = 0; i <= divisor.Degree; i++)
            {
                int index = degreeDifference + i;
                dividendCopy[index] = ring.Subtract(dividendCopy[index],
                    ring.Multiply(leadingQuotient, divisor.Coefficients[i]));
            }

            while (dividendCopy.Count > 0 && ring.IsZero(dividendCopy[^1]))
                dividendCopy.RemoveAt(dividendCopy.Count - 1);
        }

        return (new UniPolynomial<T>(ring, quotient), new UniPolynomial<T>(ring, dividendCopy.ToArray()));
    }

    public static UniPolynomial<T> GCD(IRing<T> ring, UniPolynomial<T> a, UniPolynomial<T> b)
    {
        if (ring.IsField())
            return EuclidGCD(ring, a, b).Monic();

        return PseudoEuclidGCD(ring, a, b);
    }


    public static UniPolynomial<T> EuclidGCD(IRing<T> ring, UniPolynomial<T> a, UniPolynomial<T> b)
    {
        if (!ring.IsField())
            throw new ArgumentException("L'anneau n'est pas un corps.");

        if (a.Degree < b.Degree)
            (b, a) = (a, b);

        var x = a;
        var y = b;
        while (true)
        {
            var r = Divide(ring, x, y).Remainder;

            if (r.IsZero())
                break;
            x = y;
            y = r;
        }

        return y;
    }

    public static UniPolynomial<T> PseudoEuclidGCD(IRing<T> ring, UniPolynomial<T> a, UniPolynomial<T> b)
    {
        if (a.Degree < b.Degree)
            (b, a) = (a, b);

        var x = a;
        var y = b;
        while (true)
        {
            var r = PseudoDivide(ring, x, y).Remainder;

            if (r.IsZero())
                break;
            x = y;
            y = r;
        }

        return y;
    }

    public static (UniPolynomial<T>, UniPolynomial<T>, UniPolynomial<T>) ExtendedEuclidean(IRing<T> ring,
        UniPolynomial<T> a,
        UniPolynomial<T> b)
    {
        var a1 = One(ring);
        var a2 = Zero(ring);
        var b1 = Zero(ring);
        var b2 = One(ring);
        while (!b.IsZero())
        {
            var (q, r) = Divide(ring, a, b);
            a = b;
            b = r;
            var r1 = a1 - q * b1;
            var r2 = a2 - q * b2;
            a1 = b1;
            a2 = b2;
            b1 = r1;
            b2 = r2;
        }

        return (a1, a2, a);
    }

    public static (UniPolynomial<T>, UniPolynomial<T>, UniPolynomial<T>) ExtendedPseudoEuclidean(IRing<T> ring,
        UniPolynomial<T> a,
        UniPolynomial<T> b)
    {
        var a1 = One(ring);
        var a2 = Zero(ring);
        var b1 = Zero(ring);
        var b2 = One(ring);
        while (!b.IsZero())
        {
            var (q, r) = PseudoDivide(ring, a, b);
            a = b;
            b = r;
            var r1 = a1 - q * b1;
            var r2 = a2 - q * b2;
            a1 = b1;
            a2 = b2;
            b1 = r1;
            b2 = r2;
        }

        return (a1, a2, a);
    }

    public UniPolynomial<T> Derivative()
    {
        var coefficients = new T[Degree];
        for (int i = 0; i < Degree; i++)
            coefficients[i] = Ring.Multiply(Coefficients[i + 1], i + 1);
        return new UniPolynomial<T>(Ring, coefficients);
    }

    private UniPolynomial<T> Monic()
    {
        return new UniPolynomial<T>(Ring, Coefficients.Select(c => Ring.Divide(c, LC)).ToArray());
    }

    public override string ToString()
    {
        return ToString("x");
    }

    public string ToString(string variableName, Func<T, string>? ringToString = null)
    {
        ringToString ??= t => t.ToString();
        var result = string.Join(" + ", Coefficients.Select((c, i) => 
            Ring.IsZero(c) ? null
            : ringToString(c) + 
             (i == 0 ? "" : variableName + 
                            (i == 1 ? "" : "^" + i)))
            .Where(c => c is not null));

        return result == "" ? "0" : result;
    }

    public static UniPolynomial<T> operator +(UniPolynomial<T> a, T b) => a + new UniPolynomial<T>(a.Ring, [b]);
    public static UniPolynomial<T> operator -(UniPolynomial<T> a, T b) => a - new UniPolynomial<T>(a.Ring, [b]);

    public static UniPolynomial<T> operator *(UniPolynomial<T> a, T b) =>
        new UniPolynomial<T>(a.Ring, a.Coefficients.Select(c => a.Ring.Multiply(c, b)).ToArray());

    public static UniPolynomial<T> operator *(UniPolynomial<T> a, int b) =>
        new UniPolynomial<T>(a.Ring, a.Coefficients.Select(c => a.Ring.Multiply(c, b)).ToArray());

    public static UniPolynomial<T> operator +(T b, UniPolynomial<T> a) => a + new UniPolynomial<T>(a.Ring, [b]);
    public static UniPolynomial<T> operator -(T b, UniPolynomial<T> a) => a - new UniPolynomial<T>(a.Ring, [b]);

    public static UniPolynomial<T> operator *(T b, UniPolynomial<T> a) =>
        new UniPolynomial<T>(a.Ring, a.Coefficients.Select(c => a.Ring.Multiply(c, b)).ToArray());

    public static UniPolynomial<T> operator /(UniPolynomial<T> a, T b) =>
        new UniPolynomial<T>(a.Ring, a.Coefficients.Select(c => a.Ring.Divide(c, b)).ToArray());

    public static UniPolynomial<T> operator /(UniPolynomial<T> a, int b) =>
        new UniPolynomial<T>(a.Ring, a.Coefficients.Select(c => a.Ring.Divide(c, b)).ToArray());

    public static UniPolynomial<T> operator +(UniPolynomial<T> a, UniPolynomial<T> b)
    {
        var ring = a.Ring;
        var maxDegree = Math.Max(a.Degree, b.Degree);
        var coefficients = new T[maxDegree + 1];

        for (int i = 0; i <= maxDegree; i++)
        {
            var aCoeff = i <= a.Degree ? a.Coefficients[i] : ring.Zero;
            var bCoeff = i <= b.Degree ? b.Coefficients[i] : ring.Zero;
            coefficients[i] = ring.Add(aCoeff, bCoeff);
        }

        return new UniPolynomial<T>(ring, coefficients);
    }

    public static UniPolynomial<T> operator -(UniPolynomial<T> a, UniPolynomial<T> b)
    {
        var ring = a.Ring;
        var maxDegree = Math.Max(a.Degree, b.Degree);
        var coefficients = new T[maxDegree + 1];

        for (int i = 0; i <= maxDegree; i++)
        {
            var aCoeff = i <= a.Degree ? a.Coefficients[i] : ring.Zero;
            var bCoeff = i <= b.Degree ? b.Coefficients[i] : ring.Zero;
            coefficients[i] = ring.Subtract(aCoeff, bCoeff);
        }

        return new UniPolynomial<T>(ring, coefficients);
    }

    public static UniPolynomial<T> operator -(UniPolynomial<T> a)
    {
        var ring = a.Ring;
        var maxDegree = a.Degree;
        var coefficients = new T[maxDegree + 1];

        for (int i = 0; i <= maxDegree; i++)
            coefficients[i] = ring.Negate(a.Coefficients[i]);

        return new UniPolynomial<T>(ring, coefficients);
    }

    public static UniPolynomial<T> operator *(UniPolynomial<T> a, UniPolynomial<T> b)
    {
        var ring = a.Ring;
        var coefficients = new T[a.Degree + b.Degree + 1];
        Array.Fill(coefficients, ring.Zero);

        for (int i = 0; i <= a.Degree; i++)
        {
            for (int j = 0; j <= b.Degree; j++)
            {
                coefficients[i + j] =
                    ring.Add(coefficients[i + j], ring.Multiply(a.Coefficients[i], b.Coefficients[j]));
            }
        }

        return new UniPolynomial<T>(ring, coefficients);
    }

    public static UniPolynomial<T> operator /(UniPolynomial<T> a, UniPolynomial<T> b)
    {
        var ring = a.Ring;
        return Divide(ring, a, b).Quotient;
    }

    public static UniPolynomial<T> operator %(UniPolynomial<T> a, UniPolynomial<T> b)
    {
        var ring = a.Ring;
        return Divide(ring, a, b).Remainder;
    }


    public bool Equals(UniPolynomial<T>? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Ring.Equals(other.Ring) && Coefficients.SequenceEqual(other.Coefficients);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((UniPolynomial<T>)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Ring, Coefficients);
    }


    public UniPolynomial<T> Pow(int n)
    {
        var result = One(Ring);
        for (int i = 0; i < n; i++)
        {
            result *= this;
        }

        return result;
    }
}