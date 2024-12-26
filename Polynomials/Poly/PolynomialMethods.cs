using System.Numerics;
using Polynomials.Poly.Multivar;
using Polynomials.Poly.Univar;

namespace Polynomials.Poly;

public sealed class PolynomialMethods
{
    // TODO
    // public static PolynomialFactorDecomposition<Poly> Factor<Poly>(Poly poly)
    //     where Poly : Polynomial<Poly>
    // {
    //     return UnivariateFactorization.Factor<Poly>(poly);
    // // }
    //
    //
    // public static PolynomialFactorDecomposition<Poly> Factor<Term, Poly>(Poly poly)
    //     where Term : AMonomial<Term> where Poly : AMultivariatePolynomial<Term, Poly>
    // {
    //     return MultivariateFactorization.Factor(poly);
    // }


    public static PolynomialFactorDecomposition<UnivariatePolynomial<E>> FactorSquareFree<E>(UnivariatePolynomial<E> poly)
    {
        return UnivariateSquareFreeFactorization.SquareFreeFactorization(poly);
    }

    // TODO
    // public static PolynomialFactorDecomposition<Poly> FactorSquareFree<Term, Poly>(Poly poly)
    //     where Term : AMonomial<Term> where Poly : AMultivariatePolynomial<Term, Poly>
    // {
    //     return MultivariateSquareFreeFactorization.SquareFreeFactorization<Term, Poly>(poly);
    // }


    public static UnivariatePolynomial<E> PolynomialGCD<E>(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b)
    {
        return UnivariateGCD.PolynomialGCD(a, b);
    }

    
    public static MultivariatePolynomial<E> PolynomialGCD<E>(MultivariatePolynomial<E> a, MultivariatePolynomial<E> b) {
        return MultivariateGCD.PolynomialGCD(a, b);
    }

    public static Poly PolynomialGCD<Poly>(Poly a, Poly b) where Poly : Polynomial<Poly> {
        return a.Gcd(b);
    }

    public static UnivariatePolynomial<E> PolynomialGCD<E>(params UnivariatePolynomial<E>[] array)
    {
        return UnivariateGCD.PolynomialGCD(array);
    }

    
    public static MultivariatePolynomial<E> PolynomialGCD<E>(params MultivariatePolynomial<E>[] array)
    {
        return MultivariateGCD.PolynomialGCD(array);
    }


    public static UnivariatePolynomial<E> PolynomialGCD<E>(IEnumerable<UnivariatePolynomial<E>> array)
    {
        return UnivariateGCD.PolynomialGCD(array);
    }

    //
    // public static Poly PolynomialGCD<Term, Poly>(IEnumerable<Poly> array) where Term : AMonomial<Term>
    //     where Poly : AMultivariatePolynomial<Term, Poly>
    // {
    //     return MultivariateGCD.PolynomialGCD(array);
    // }
    //
    //
    // public static T[] PolynomialExtendedGCD<T>(T a, T b) where T : IUnivariatePolynomial<T>
    // {
    //     if (a.IsOverField())
    //         return UnivariateGCD.PolynomialExtendedGCD(a, b);
    //     else
    //         throw new ArgumentException("Polynomial over field is expected");
    // }

    //
    // public static Poly[] DivideAndRemainder<Poly>(Poly a, Poly b) where Poly : IUnivariatePolynomial<Poly>
    // {
    //     return UnivariateDivision.DivideAndRemainder(a, b, true);
    // }
    //
    // public static Poly[] DivideAndRemainder<Term, Poly>(Poly a, Poly b) where Term : AMonomial<Term>
    //     where Poly : AMultivariatePolynomial<Term, Poly>
    // {
    //     return MultivariateDivision.DivideAndRemainder<Term, Poly>(a, b);
    // }

    //
    // public static Poly Remainder<Poly>(Poly a, Poly b) where Poly : IPolynomial<Poly>
    // {
    //     if (a is IUnivariatePolynomial)
    //         return (Poly)UnivariateDivision.Remainder((IUnivariatePolynomial)a, (IUnivariatePolynomial)b, true);
    //     else if (a is AMultivariatePolynomial)
    //         return (Poly)MultivariateDivision.DivideAndRemainder((AMultivariatePolynomial)a, (AMultivariatePolynomial)b)
    //             [1];
    //     else
    //         throw new Exception();
    // }
    //
    //
    // public static Poly DivideOrNull<Poly>(Poly a, Poly b) where Poly : IPolynomial<Poly>
    // {
    //     if (a is IUnivariatePolynomial)
    //         return (Poly)UnivariateDivision.DivideOrNull((IUnivariatePolynomial)a, (IUnivariatePolynomial)b, true);
    //     else if (a is AMultivariatePolynomial)
    //         return (Poly)MultivariateDivision.DivideOrNull((AMultivariatePolynomial)a, (AMultivariatePolynomial)b);
    //     else
    //         throw new Exception();
    // }
    //
    //
    public static Poly DivideExact<Poly>(Poly a, Poly b) where Poly : Polynomial<Poly>
    {
        return a.DivideExact(b);
    }
    
    
    public static bool CoprimeQ<E>(params UnivariatePolynomial<E>[] polynomials)
    {
        for (int i = 0; i < polynomials.Length - 1; i++)
        for (int j = i + 1; j < polynomials.Length; j++)
            if (!PolynomialGCD(polynomials[i], polynomials[j]).IsConstant())
                return false;
        return true;
    }
    
    
    public static bool CoprimeQ<E>(IEnumerable<UnivariatePolynomial<E>> polynomials)
    {
        return CoprimeQ(polynomials.ToArray());
    }
    
    public static bool CoprimeQ<E>(params MultivariatePolynomial<E>[] polynomials)
    {
        for (int i = 0; i < polynomials.Length - 1; i++)
        for (int j = i + 1; j < polynomials.Length; j++)
            if (!PolynomialGCD(polynomials[i], polynomials[j]).IsConstant())
                return false;
        return true;
    }
    
    
    public static bool CoprimeQ<E>(IEnumerable<MultivariatePolynomial<E>> polynomials)
    {
        return CoprimeQ(polynomials.ToArray());
    }
    
    // public static bool IrreducibleQ<Poly>(Poly poly) where Poly : IPolynomial<Poly>
    // {
    //     if (poly is IUnivariatePolynomial)
    //         return IrreduciblePolynomials.IrreducibleQ((IUnivariatePolynomial)poly);
    //     else
    //         return MultivariateFactorization.Factor((AMultivariatePolynomial)poly).IsTrivial();
    // }


    public static T PolyPow<T>(T @base, BigInteger exponent, bool copy) where T : Polynomial<T>
    {
        if (exponent.Sign < 0)
            throw new ArgumentException();
        if (exponent.IsOne || @base.IsOne())
            return copy ? @base.Clone() : @base;
        T result = @base.CreateOne();
        T k2p = copy ? @base.Clone() : @base;
        for (;;)
        {
            if (exponent.IsEven)
                result = result.Multiply(k2p);
            exponent = exponent >> 1;
            if (exponent.IsZero)
                return result;
            k2p = k2p.Multiply(k2p);
        }
    }


    public static T PolyPow<T>(T @base, long exponent) where T : Polynomial<T>
    {
        return PolyPow(@base, exponent, true);
    }


    public static T PolyPow<T>(T @base, BigInteger exponent) where T : Polynomial<T>
    {
        return PolyPow(@base, exponent, true);
    }


    public static T PolyPow<T>(T @base, long exponent, bool copy) where T : Polynomial<T>
    {
        if (exponent < 0)
            throw new ArgumentException();
        if (exponent == 1 || @base.IsOne())
            return copy ? @base.Clone() : @base;
        T result = @base.CreateOne();
        T k2p = copy ? @base.Clone() : @base;
        for (;;)
        {
            if ((exponent & 1) != 0)
                result = result.Multiply(k2p);
            exponent = exponent >> 1;
            if (exponent == 0)
                return result;
            k2p = k2p.Multiply(k2p);
        }
    }


    public static T PolyPow<T>(T @base, int exponent, bool copy, Dictionary<int, T> cache) where T : Polynomial<T>
    {
        if (exponent < 0)
            throw new ArgumentException();
        if (exponent == 1)
            return copy ? @base.Clone() : @base;
        T? cached = cache.GetValueOrDefault(exponent, null);
        if (cached != null)
            return cached.Clone();
        T result = @base.CreateOne();
        T k2p = copy ? @base.Clone() : @base;
        int rExp = 0, kExp = 1;
        for (;;)
        {
            if ((exponent & 1) != 0)
                cache.TryAdd(rExp += kExp, result.Multiply(k2p).Clone());
            exponent = exponent >> 1;
            if (exponent == 0)
            {
                cache.TryAdd(rExp, result);
                return result;
            }

            cache.Add(kExp *= 2, k2p.Square().Clone());
        }
    }
}