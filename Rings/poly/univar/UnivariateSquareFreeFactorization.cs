using System.Diagnostics;
using System.Numerics;

namespace Rings.poly.univar;

public static class UnivariateSquareFreeFactorization
{
    public static bool isSquareFree<T>(T poly) where T : IUnivariatePolynomial<T>
    {
        return UnivariateGCD.PolynomialGCD(poly, poly.derivative()).isConstant();
    }


    public static PolynomialFactorDecomposition<T> SquareFreeFactorization<T>(T poly) where T : IUnivariatePolynomial<T>
    {
        if (poly.isOverFiniteField())
            return SquareFreeFactorizationMusser(poly);
        else if (UnivariateFactorization.isOverMultivariate(poly))
            return (PolynomialFactorDecomposition<T>)UnivariateFactorization.FactorOverMultivariate(
                (UnivariatePolynomial)poly, MultivariateSquareFreeFactorization::SquareFreeFactorization);
        else if (UnivariateFactorization.isOverUnivariate(poly))
            return (PolynomialFactorDecomposition<T>)UnivariateFactorization.FactorOverUnivariate(
                (UnivariatePolynomial)poly, MultivariateSquareFreeFactorization::SquareFreeFactorization);
        else if (poly.coefficientRingCharacteristic().isZero())
            return SquareFreeFactorizationYunZeroCharacteristics(poly);
        else
            return SquareFreeFactorizationMusser(poly);
    }


    public static T SquareFreePart<T>(T poly) where T : IUnivariatePolynomial<T>
    {
        return SquareFreeFactorization(poly).factors.stream().filter(x-> !x.isMonomial())
            .reduce(poly.createOne(), IUnivariatePolynomial < T >::multiply);
    }


    public static PolynomialFactorDecomposition<Poly>
        SquareFreeFactorizationYunZeroCharacteristics<Poly>(Poly poly) where Poly : IUnivariatePolynomial<Poly>
    {
        if (!poly.coefficientRingCharacteristic().IsZero)
            throw new ArgumentException("Characteristics 0 expected");

        if (poly.isConstant())
            return PolynomialFactorDecomposition.of(poly);

        // x^2 + x^3 -> x^2 (1 + x)
        int exponent = 0;
        while (exponent <= poly.degree() && poly.isZeroAt(exponent))
        {
            ++exponent;
        }

        if (exponent == 0)
            return SquareFreeFactorizationYun0(poly);

        Poly expFree = poly.getRange(exponent, poly.degree() + 1);
        PolynomialFactorDecomposition<Poly> fd = SquareFreeFactorizationYun0(expFree);
        fd.addFactor(poly.createMonomial(1), exponent);
        return fd;
    }


    static PolynomialFactorDecomposition<Poly>
        SquareFreeFactorizationYun0<Poly>(Poly poly) where Poly : IUnivariatePolynomial<Poly>
    {
        if (poly.isConstant())
            return PolynomialFactorDecomposition.of(poly);

        Poly content = poly.contentAsPoly();
        if (poly.signumOfLC() < 0)
            content = content.negate();

        poly = poly.clone().divideByLC(content);
        if (poly.degree() <= 1)
            return PolynomialFactorDecomposition.of(content, poly);

        PolynomialFactorDecomposition<Poly> factorization = PolynomialFactorDecomposition.of(content);
        SquareFreeFactorizationYun0(poly, factorization);
        return factorization;
    }

    private static void SquareFreeFactorizationYun0<Poly>(Poly poly,
        PolynomialFactorDecomposition<Poly> factorization) where Poly : IUnivariatePolynomial<Poly>
    {
        Poly derivative = poly.derivative(), gcd = UnivariateGCD.PolynomialGCD(poly, derivative);
        if (gcd.isConstant())
        {
            factorization.addFactor(poly, 1);
            return;
        }

        Poly quot =
                UnivariateDivision.divideAndRemainder(poly, gcd, false)
                    [0], // safely destroy (cloned) poly (not used further)
            dQuot =
                UnivariateDivision.divideAndRemainder(derivative, gcd, false)
                    [0]; // safely destroy (cloned) derivative (not used further)

        int i = 0;
        while (!quot.isConstant())
        {
            ++i;
            dQuot = dQuot.subtract(quot.derivative());
            Poly factor = UnivariateGCD.PolynomialGCD(quot, dQuot);
            quot = UnivariateDivision.divideAndRemainder(quot, factor,
                false)[0]; // can destroy quot in divideAndRemainder
            dQuot = UnivariateDivision.divideAndRemainder(dQuot, factor,
                false)[0]; // can destroy dQuot in divideAndRemainder
            if (!factor.isOne())
                factorization.addFactor(factor, i);
        }
    }


    public static PolynomialFactorDecomposition<Poly>
        SquareFreeFactorizationMusserZeroCharacteristics<Poly>(Poly poly) where Poly : IUnivariatePolynomial<Poly>
    {
        if (!poly.coefficientRingCharacteristic().IsZero)
            throw new ArgumentException("Characteristics 0 expected");

        if (poly.isConstant())
            return PolynomialFactorDecomposition.of(poly);

        Poly content = poly.contentAsPoly();
        if (poly.signumOfLC() < 0)
            content = content.negate();

        poly = poly.clone().divideByLC(content);
        if (poly.degree() <= 1)
            return PolynomialFactorDecomposition.of(content, poly);

        PolynomialFactorDecomposition<Poly> factorization = PolynomialFactorDecomposition.of(content);
        SquareFreeFactorizationMusserZeroCharacteristics0(poly, factorization);
        return factorization;
    }

    private static void SquareFreeFactorizationMusserZeroCharacteristics0<Poly>(Poly poly,
        PolynomialFactorDecomposition<Poly> factorization) where Poly : IUnivariatePolynomial<Poly>
    {
        Poly derivative = poly.derivative(), gcd = UnivariateGCD.PolynomialGCD(poly, derivative);
        if (gcd.isConstant())
        {
            factorization.addFactor(poly, 1);
            return;
        }

        Poly quot = UnivariateDivision.divideAndRemainder(poly, gcd, false)[0]; // safely destroy (cloned) poly
        int i = 0;
        while (true)
        {
            ++i;
            Poly nextQuot = UnivariateGCD.PolynomialGCD(gcd, quot);
            gcd = UnivariateDivision.divideAndRemainder(gcd, nextQuot, false)[0]; // safely destroy gcd (reassigned)
            Poly factor =
                UnivariateDivision.divideAndRemainder(quot, nextQuot, false)
                    [0]; // safely destroy quot (reassigned further)
            if (!factor.isConstant())
                factorization.addFactor(factor, i);
            if (nextQuot.isConstant())
                break;
            quot = nextQuot;
        }
    }


    public static PolynomialFactorDecomposition<Poly>
        SquareFreeFactorizationMusser<Poly>(Poly poly) where Poly : IUnivariatePolynomial<Poly>
    {
        if (canConvertToZp64(poly))
            return SquareFreeFactorizationMusser(asOverZp64(poly)).mapTo(Conversions64bit::convert);

        poly = poly.clone();
        Poly lc = poly.lcAsPoly();
        //make poly monic
        poly = poly.monic();

        if (poly.isConstant())
            return PolynomialFactorDecomposition.of(lc);

        if (poly.degree() <= 1)
            return PolynomialFactorDecomposition.of(lc, poly);

        PolynomialFactorDecomposition<Poly> factorization;
        // x^2 + x^3 -> x^2 (1 + x)
        int exponent = 0;
        while (exponent <= poly.degree() && poly.isZeroAt(exponent))
        {
            ++exponent;
        }

        if (exponent == 0)
            factorization = SquareFreeFactorizationMusser0(poly);
        else
        {
            Poly expFree = poly.getRange(exponent, poly.degree() + 1);
            factorization = SquareFreeFactorizationMusser0(expFree);
            factorization.addFactor(poly.createMonomial(1), exponent);
        }

        return factorization.setUnit(lc);
    }


    private static PolynomialFactorDecomposition<Poly>
        SquareFreeFactorizationMusser0<Poly>(Poly poly) where Poly : IUnivariatePolynomial<Poly>
    {
        poly.monic();
        if (poly.isConstant())
            return PolynomialFactorDecomposition.of(poly);

        if (poly.degree() <= 1)
            return PolynomialFactorDecomposition.of(poly);

        Poly derivative = poly.derivative();
        if (!derivative.isZero())
        {
            Poly gcd = UnivariateGCD.PolynomialGCD(poly, derivative);
            if (gcd.isConstant())
                return PolynomialFactorDecomposition.of(poly);
            Poly quot = UnivariateDivision.divideAndRemainder(poly, gcd,
                false)[0]; // can safely destroy poly (not used further)

            PolynomialFactorDecomposition<Poly> result = PolynomialFactorDecomposition.of(poly.createOne());
            int i = 0;
            //if (!quot.isConstant())
            while (true)
            {
                ++i;
                Poly nextQuot = UnivariateGCD.PolynomialGCD(gcd, quot);
                Poly factor =
                    UnivariateDivision.divideAndRemainder(quot, nextQuot,
                        false)[0]; // can safely destroy quot (reassigned further)
                if (!factor.isConstant())
                    result.addFactor(factor.monic(), i);

                gcd = UnivariateDivision.divideAndRemainder(gcd, nextQuot, false)[0]; // can safely destroy gcd
                if (nextQuot.isConstant())
                    break;
                quot = nextQuot;
            }

            if (!gcd.isConstant())
            {
                gcd = pRoot(gcd);
                PolynomialFactorDecomposition<Poly> gcdFactorization = SquareFreeFactorizationMusser0(gcd);
                gcdFactorization.raiseExponents((int)poly.coefficientRingCharacteristic());
                result.addAll(gcdFactorization);
                return result;
            }
            else
                return result;
        }
        else
        {
            Poly pRoot = pRoot(poly);
            PolynomialFactorDecomposition<Poly> fd = SquareFreeFactorizationMusser0(pRoot);
            fd.raiseExponents((int)poly.coefficientRingCharacteristic());
            return fd.setUnit(poly.createOne());
        }
    }


    private static Poly pRoot<Poly>(Poly poly) where Poly : IUnivariatePolynomial<Poly>
    {
        if (poly is UnivariatePolynomialZp64)
            return (Poly)pRoot((UnivariatePolynomialZp64)poly);
        else if (poly is UnivariatePolynomial)
            return (Poly)pRoot((UnivariatePolynomial)poly);
        else
            throw new Exception(poly.GetType().ToString());
    }


    private static UnivariatePolynomialZp64 pRoot(UnivariatePolynomialZp64 poly)
    {
        if (poly.ring.Modulus > int.MaxValue)
            throw new ArgumentException("Too big modulus: " + poly.ring.Modulus);
        int modulus = MachineArithmetic.safeToInt(poly.ring.Modulus);
        Debug.Assert(poly.degree() % modulus == 0);
        Debug.Assert(!poly.ring.isPerfectPower()); // just in case
        long[] rootData = new long[poly.degree() / modulus + 1];
        Array.Fill(rootData, 0);
        for (int i = poly.degree(); i >= 0; --i)
            if (poly.data[i] != 0)
            {
                Debug.Assert(i % modulus == 0);
                rootData[i / modulus] = poly.data[i];
            }

        return poly.createFromArray(rootData);
    }


    private static UnivariatePolynomial<E> pRoot<E>(UnivariatePolynomial<E> poly)
    {
        if (poly.coefficientRingCharacteristic().CompareTo(new BigInteger(int.MaxValue)) <= 0)
            throw new ArgumentException("Infinite or too large ring: " + poly.ring);
        Ring<E> ring = poly.ring;
        // p^(m -1) used for computing p-th root of elements
        BigInteger inverseFactor = ring.cardinality() / ring.characteristic();
        int modulus = (int)poly.coefficientRingCharacteristic();
        Debug.Assert(poly.Degree % modulus == 0);
        E[] rootData = poly.ring.createZeroesArray(poly.Degree / modulus + 1);
        for (int i = poly.Degree; i >= 0; --i)
            if (!poly.ring.isZero(poly.data[i]))
            {
                Debug.Assert(i % modulus == 0);
                rootData[i / modulus] = ring.pow(poly.data[i], inverseFactor); // pRoot(poly.data[i], ring);
            }

        return poly.createFromArray(rootData);
    }
}