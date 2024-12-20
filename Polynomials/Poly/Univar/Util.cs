using System.Numerics;
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

    public static void EnsureOverField<E>(params UnivariatePolynomial<E>[] polys)
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


    public static bool CanConvertToZp64<E>(UnivariatePolynomial<E> poly)
    {
        return poly.ring is IntegersZp zp && zp.modulus.GetBitLength() < MachineArithmetic.MAX_SUPPORTED_MODULUS_BITS;
    }

    // TODO
    // public static bool CanConvertToZp64<E>(MultivariatePolynomial<E> poly)
    // {
    //     return poly.ring is IntegersZp zp && zp.modulus.GetBitLength() < MachineArithmetic.MAX_SUPPORTED_MODULUS_BITS;
    // }

    // TODO
    // public static bool CanConvertToZp64<Poly>(IPolynomial<Poly> poly) where Poly : IPolynomial<Poly>
    // {
    //     // TODO : check if poly is univariate or multivariate (use func) else return false
    //     throw new NotImplementedException();
    // }

    public static bool IsOverRationals<E>(UnivariatePolynomial<E> poly)
    {
        return poly.ring is IRationals;
    }

    // TODO
    // public static bool IsOverSimpleFieldExtension<T>(T poly) where T : IPolynomial<T>
    // {
    //     if (poly is UnivariatePolynomial && ((UnivariatePolynomial)poly).ring is SimpleFieldExtension)
    //         return true;
    //     else if (poly is MultivariatePolynomial && ((MultivariatePolynomial)poly).ring is SimpleFieldExtension)
    //         return true;
    //     else
    //         return false;
    // }
    //
    //
    // public static bool IsOverMultipleFieldExtension<T>(T poly) where T : IPolynomial<T>
    // {
    //     if (poly is UnivariatePolynomial && ((UnivariatePolynomial)poly).ring is MultipleFieldExtension)
    //         return true;
    //     else if (poly is MultivariatePolynomial && ((MultivariatePolynomial)poly).ring is MultipleFieldExtension)
    //         return true;
    //     else
    //         return false;
    // }
    //
    //
    // public static bool IsOverSimpleNumberField<T>(T poly) where T : IPolynomial<T>
    // {
    //     if (poly is UnivariatePolynomial && ((UnivariatePolynomial)poly).ring is AlgebraicNumberField &&
    //         IsOverQ(((AlgebraicNumberField)((UnivariatePolynomial)poly).ring).GetMinimalPolynomial()))
    //         return true;
    //     else if (poly is MultivariatePolynomial && ((MultivariatePolynomial)poly).ring is AlgebraicNumberField &&
    //              IsOverQ(((AlgebraicNumberField)((MultivariatePolynomial)poly).ring).GetMinimalPolynomial()))
    //         return true;
    //     else
    //         return false;
    // }
    //
    //
    // public static bool IsOverRingOfIntegersOfSimpleNumberField<T>(T poly) where T : IPolynomial<T>
    // {
    //     if (poly is UnivariatePolynomial && ((UnivariatePolynomial)poly).ring is AlgebraicNumberField &&
    //         IsOverZ(((AlgebraicNumberField)((UnivariatePolynomial)poly).ring).GetMinimalPolynomial()))
    //         return true;
    //     else if (poly is MultivariatePolynomial && ((MultivariatePolynomial)poly).ring is AlgebraicNumberField &&
    //              IsOverZ(((AlgebraicNumberField)((MultivariatePolynomial)poly).ring).GetMinimalPolynomial()))
    //         return true;
    //     else
    //         return false;
    // }
    //
    //
    // public static bool IsOverQ<T>(T poly) where T : IPolynomial<T>
    // {
    //     object rep;
    //     if (poly is UnivariatePolynomial)
    //         rep = ((UnivariatePolynomial)poly).ring.GetOne();
    //     else if (poly is MultivariatePolynomial)
    //         rep = ((MultivariatePolynomial)poly).ring.GetOne();
    //     else
    //         return false;
    //     if (!(rep is Rational))
    //         return false;
    //     return ((Rational)rep).Numerator() is BigInteger;
    // }
    //
    //
    // public static bool IsOverZ<T>(T poly) where T : IPolynomial<T>
    // {
    //     return poly.IsOverZ();
    // }
    //
    //
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
    
    //
    // public static E CommonDenominator<E>(UnivariatePolynomial<Rational<E>> poly)
    // {
    //     Ring<Rational<E>> field = poly.ring;
    //     Ring<E> integralRing = field.GetOne().ring;
    //     E denominator = integralRing.GetOne();
    //     for (int i = 0; i <= poly.Degree(); i++)
    //         if (!poly.IsZeroAt(i))
    //             denominator = integralRing.Lcm(denominator, poly[i].Denominator());
    //     return denominator;
    // }
    //
    //
    // public static E CommonDenominator<E>(MultivariatePolynomial<Rational<E>> poly)
    // {
    //     Ring<Rational<E>> field = poly.ring;
    //     Ring<E> integralRing = field.GetOne().ring;
    //     E denominator = integralRing.GetOne();
    //     foreach (Rational<E> cf in poly.Coefficients())
    //         denominator = integralRing.Lcm(denominator, cf.Denominator());
    //     return denominator;
    // }
    //
    //
    // public static (MultivariatePolynomial<E>, E) ToCommonDenominator<E>(MultivariatePolynomial<Rational<E>> poly)
    // {
    //     Ring<Rational<E>> field = poly.ring;
    //     Ring<E> integralRing = field.GetOne().ring;
    //     E denominator = integralRing.GetOne();
    //     foreach (Rational<E> cf in poly.Coefficients())
    //         denominator = integralRing.Lcm(denominator, cf.Denominator());
    //     E d = denominator;
    //     MultivariatePolynomial<E> integral = poly.MapCoefficients(integralRing, (cf) =>
    //     {
    //         Rational<E> r = cf.Multiply(d);
    //         return r.Numerator();
    //     });
    //     return (integral, denominator);
    // }
    //
    public static UnivariatePolynomial<Rational<E>> AsOverRationals<E>(Ring<Rational<E>> field,
        UnivariatePolynomial<E> poly)
    {
        return poly.MapCoefficients(field, cf => new Rational<E>(poly.ring, cf));
    }
    
    // public static MultivariatePolynomial<Rational<E>> AsOverRationals<E>(Ring<Rational<E>> field,
    //     MultivariatePolynomial<E> poly)
    // {
    //     return poly.MapCoefficients(field, (cf) => new Rational<E>(poly.ring, cf));
    // }
    //
    // public static UnivariatePolynomial<Rational<E>> DivideOverRationals<E>(Ring<Rational<E>> field,
    //     UnivariatePolynomial<E> poly, E denominator)
    // {
    //     return poly.MapCoefficients(field, (cf) => new Rational<E>(poly.ring, cf, denominator));
    // }
    //
    // public static MultivariatePolynomial<Rational<E>> DivideOverRationals<E>(Ring<Rational<E>> field,
    //     MultivariatePolynomial<E> poly, E denominator)
    // {
    //     return poly.MapCoefficients(field, (cf) => new Rational<E>(poly.ring, cf, denominator));
    // }
}
