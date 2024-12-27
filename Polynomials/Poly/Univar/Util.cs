using System.Numerics;
using Polynomials.Poly.Multivar;
using Polynomials.Utils;


namespace Polynomials.Poly.Univar;

public static class Util
{
    public static void EnsureOverFiniteField<E>(params UnivariatePolynomial<E>[] polys)
    {
        foreach (var poly in polys)
            if (!poly.IsOverFiniteField())
                throw new ArgumentException("Polynomial over finite field is expected; " + poly.GetType());
    }
    
    public static void EnsureOverFiniteField<E>(params MultivariatePolynomial<E>[] polys)
    {
        foreach (var poly in polys)
            if (!poly.IsOverFiniteField())
                throw new ArgumentException("Polynomial over finite field is expected; " + poly.GetType());
    }

    public static void EnsureOverField<E>(params UnivariatePolynomial<E>[] polys)
    {
        foreach (var poly in polys)
            if (!poly.IsOverField())
                throw new ArgumentException("Polynomial over finite field is expected; " + poly.GetType());
    }
    
    public static void EnsureOverField<E>(params MultivariatePolynomial<E>[] polys)
    {
        foreach (var poly in polys)
            if (!poly.IsOverField())
                throw new ArgumentException("Polynomial over finite field is expected; " + poly.GetType());
    }

    public static void EnsureOverZ<E>(params UnivariatePolynomial<E>[] polys)
    {
        foreach (var poly in polys)
            if (!poly.IsOverZ())
                throw new ArgumentException("Polynomial over Z is expected, but got " + poly.GetType());
    }

    public static void EnsureOverZ<E>(params MultivariatePolynomial<E>[] polys)
    {
        foreach (var poly in polys)
            if (!poly.IsOverZ())
                throw new ArgumentException("Polynomial over Z is expected, but got " + poly.GetType());
    }

    public static bool CanConvertToZp64<E>(UnivariatePolynomial<E> poly)
    {
        return poly.ring is IntegersZp zp && zp.modulus.GetBitLength() < MachineArithmetic.MAX_SUPPORTED_MODULUS_BITS;
    }

    public static bool CanConvertToZp64<E>(MultivariatePolynomial<E> poly)
    {
        return poly.ring is IntegersZp zp && zp.modulus.GetBitLength() < MachineArithmetic.MAX_SUPPORTED_MODULUS_BITS;
    }


    public static bool IsOverRationals<E>(UnivariatePolynomial<E> poly)
    {
        return poly.ring is IRationals;
    }
    
    public static bool IsOverRationals<E>(MultivariatePolynomial<E> poly)
    {
        return poly.ring is IRationals;
    }

    public static bool IsOverSimpleFieldExtension<T>(UnivariatePolynomial<T> poly)
    {
        return poly.ring is ISimpleFieldExtension;
    }

    public static bool IsOverSimpleFieldExtension<T>(MultivariatePolynomial<T> poly)
    {
        return poly.ring is ISimpleFieldExtension;
    }

    
    public static bool IsOverMultipleFieldExtension<T>(UnivariatePolynomial<T> poly) 
    {
        return poly.ring is IMultipleFieldExtension;
    }
    
    
    public static bool IsOverMultipleFieldExtension<T>(MultivariatePolynomial<T> poly) 
    {
        return poly.ring is IMultipleFieldExtension;
    }
    public static bool IsOverSimpleNumberField<T>(UnivariatePolynomial<T> poly) 
    {
        return poly.ring is AlgebraicNumberField<Rationals<BigInteger>>;
    }
    
    public static bool IsOverSimpleNumberField<T>(MultivariatePolynomial<T> poly) 
    {
        return poly.ring is AlgebraicNumberField<Rationals<BigInteger>>;
    }
    
    public static bool IsOverRingOfIntegersOfSimpleNumberField<T>(UnivariatePolynomial<T> poly)
    {
        return poly.ring is AlgebraicNumberField<BigInteger>;
    }
    
    public static bool IsOverRingOfIntegersOfSimpleNumberField<T>(MultivariatePolynomial<T> poly)
    {
        return poly.ring is AlgebraicNumberField<BigInteger>;
    }
    
    public static bool IsOverQ<T>(UnivariatePolynomial<T> poly)
    {
        return poly.ring is Rationals<BigInteger>;
    }
    
    public static bool IsOverQ<T>(MultivariatePolynomial<T> poly)
    {
        return poly.ring is Rationals<BigInteger>;
    }
    
    public static bool IsOverZ<T>(UnivariatePolynomial<T> poly)
    {
        return poly.IsOverZ();
    }
    
    
    public static bool IsOverZ<T>(MultivariatePolynomial<T> poly)
    {
        return poly.IsOverZ();
    }
    
    public static (UnivariatePolynomial<E>, E) ToCommonDenominator<E>(UnivariatePolynomial<Rational<E>> poly)
    {
        Ring<Rational<E>> field = poly.ring;
        Ring<E> integralRing = field.GetOne().ring;
        E denominator = integralRing.GetOne();
        for (int i = 0; i <= poly.Degree(); i++)
            if (!poly.IsZeroAt(i))
                denominator = integralRing.Lcm(denominator, poly[i].Denominator());
        E[] data = new E[poly.Degree() + 1];
        for (int i = 0; i <= poly.Degree(); i++)
        {
            Rational<E> cf = poly[i].Multiply(denominator);
            data[i] = cf.Numerator();
        }
    
        return (UnivariatePolynomial<E>.CreateUnsafe(integralRing, data), denominator);
    }
    
    
    public static E CommonDenominator<E>(UnivariatePolynomial<Rational<E>> poly)
    {
        Ring<Rational<E>> field = poly.ring;
        Ring<E> integralRing = field.GetOne().ring;
        E denominator = integralRing.GetOne();
        for (int i = 0; i <= poly.Degree(); i++)
            if (!poly.IsZeroAt(i))
                denominator = integralRing.Lcm(denominator, poly[i].Denominator());
        return denominator;
    }
    
    
    public static E CommonDenominator<E>(MultivariatePolynomial<Rational<E>> poly)
    {
        Ring<Rational<E>> field = poly.ring;
        Ring<E> integralRing = field.GetOne().ring;
        E denominator = integralRing.GetOne();
        foreach (Rational<E> cf in poly.Coefficients())
            denominator = integralRing.Lcm(denominator, cf.Denominator());
        return denominator;
    }
    
    
    public static (MultivariatePolynomial<E>, E) ToCommonDenominator<E>(MultivariatePolynomial<Rational<E>> poly)
    {
        Ring<Rational<E>> field = poly.ring;
        Ring<E> integralRing = field.GetOne().ring;
        E denominator = integralRing.GetOne();
        foreach (Rational<E> cf in poly.Coefficients())
            denominator = integralRing.Lcm(denominator, cf.Denominator());
        E d = denominator;
        MultivariatePolynomial<E> integral = poly.MapCoefficients(integralRing, (cf) =>
        {
            Rational<E> r = cf.Multiply(d);
            return r.Numerator();
        });
        return (integral, denominator);
    }
    
    public static UnivariatePolynomial<Rational<E>> AsOverRationals<E>(Ring<Rational<E>> field,
        UnivariatePolynomial<E> poly)
    {
        return poly.MapCoefficients(field, cf => new Rational<E>(poly.ring, cf));
    }
    
    public static MultivariatePolynomial<Rational<E>> AsOverRationals<E>(Ring<Rational<E>> field,
        MultivariatePolynomial<E> poly)
    {
        return poly.MapCoefficients(field, (cf) => new Rational<E>(poly.ring, cf));
    }
    
    public static UnivariatePolynomial<Rational<E>> DivideOverRationals<E>(Ring<Rational<E>> field,
        UnivariatePolynomial<E> poly, E denominator)
    {
        return poly.MapCoefficients(field, (cf) => new Rational<E>(poly.ring, cf, denominator));
    }
    
    public static MultivariatePolynomial<Rational<E>> DivideOverRationals<E>(Ring<Rational<E>> field,
        MultivariatePolynomial<E> poly, E denominator)
    {
        return poly.MapCoefficients(field, (cf) => new Rational<E>(poly.ring, cf, denominator));
    }
}
