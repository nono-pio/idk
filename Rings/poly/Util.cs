using Rings.poly.univar;

namespace Rings.poly;

public static class Util {

    public static void ensureOverFiniteField<T>(params IPolynomial<T>[] polys) where T : IPolynomial<T>
    {
        foreach (IPolynomial<T> poly in polys)
            if (!poly.isOverFiniteField())
                throw new ArgumentException("Polynomial over finite field is expected; " + poly.GetType());
    }

    public static void ensureOverField<T>(params IPolynomial<T>[] polys) where T : IPolynomial<T>{
        foreach (IPolynomial<T> poly in polys)
            if (!poly.isOverField())
                throw new ArgumentException("Polynomial over finite field is expected; " + poly.GetType());
    }

    public static void ensureOverZ<T>(params IPolynomial<T>[] polys) where T : IPolynomial<T>{
        foreach (var poly in polys)
            if (!poly.isOverZ())
                throw new ArgumentException("Polynomial over Z is expected, but got " + poly.GetType());
    }

    /**
     * Test whether poly is over Zp with modulus less then 2^63
     */
    public static bool canConvertToZp64<T>(IPolynomial<T> poly) where T : IPolynomial<T> {
        Ring ring;
        if (poly is UnivariatePolynomial)
            ring = ((UnivariatePolynomial<>) poly).ring;
        else if (poly is MultivariatePolynomial)
            ring = ((MultivariatePolynomial) poly).ring;
        else
            return false;

        return ring is IntegersZp && ((IntegersZp) ring).Modulus.bitLength() < MachineArithmetic.MAX_SUPPORTED_MODULUS_BITS;
    }

    /** Whether coefficient domain is rationals */
    public static bool isOverRationals<T>(T poly) where T : IPolynomial<T> {
        if (poly is UnivariatePolynomial && ((UnivariatePolynomial) poly).ring is Rationals)
            return true;
        else if (poly is MultivariatePolynomial && ((MultivariatePolynomial) poly).ring is Rationals)
            return true;
        else
            return false;
    }

    /** Whether coefficient domain is F(alpha) */
    public static bool isOverSimpleFieldExtension<T>(T poly) where T : IPolynomial<T> {
        if (poly instanceof UnivariatePolynomial
                && ((UnivariatePolynomial) poly).ring instanceof SimpleFieldExtension)
            return true;
        else if (poly instanceof MultivariatePolynomial
                && ((MultivariatePolynomial) poly).ring instanceof SimpleFieldExtension)
            return true;
        else
            return false;
    }

    /** Whether coefficient domain is F(alpha1, alpha2, ...) */
    @SuppressWarnings("unchecked")
    public static <T extends IPolynomial<T>> boolean isOverMultipleFieldExtension(T poly) {
        if (poly instanceof UnivariatePolynomial
                && ((UnivariatePolynomial) poly).ring instanceof MultipleFieldExtension)
            return true;
        else if (poly instanceof MultivariatePolynomial
                && ((MultivariatePolynomial) poly).ring instanceof MultipleFieldExtension)
            return true;
        else
            return false;
    }

    /** Whether coefficient domain is Q(alpha) */
    @SuppressWarnings("unchecked")
    public static <T extends IPolynomial<T>> boolean isOverSimpleNumberField(T poly) {
        if (poly instanceof UnivariatePolynomial
                && ((UnivariatePolynomial) poly).ring instanceof AlgebraicNumberField
                && isOverQ(((AlgebraicNumberField) ((UnivariatePolynomial) poly).ring).getMinimalPolynomial()))
            return true;
        else if (poly instanceof MultivariatePolynomial
                && ((MultivariatePolynomial) poly).ring instanceof AlgebraicNumberField
                && isOverQ(((AlgebraicNumberField) ((MultivariatePolynomial) poly).ring).getMinimalPolynomial()))
            return true;
        else
            return false;
    }

    /** Whether coefficient domain is Q(alpha) */
    @SuppressWarnings("unchecked")
    public static <T extends IPolynomial<T>> boolean isOverRingOfIntegersOfSimpleNumberField(T poly) {
        if (poly instanceof UnivariatePolynomial
                && ((UnivariatePolynomial) poly).ring instanceof AlgebraicNumberField
                && isOverZ(((AlgebraicNumberField) ((UnivariatePolynomial) poly).ring).getMinimalPolynomial()))
            return true;
        else if (poly instanceof MultivariatePolynomial
                && ((MultivariatePolynomial) poly).ring instanceof AlgebraicNumberField
                && isOverZ(((AlgebraicNumberField) ((MultivariatePolynomial) poly).ring).getMinimalPolynomial()))
            return true;
        else
            return false;
    }

    /** Whether coefficient domain is Q */
    public static <T extends IPolynomial<T>> boolean isOverQ(T poly) {
        Object rep;

        if (poly instanceof UnivariatePolynomial)
            rep = ((UnivariatePolynomial) poly).ring.getOne();
        else if (poly instanceof MultivariatePolynomial)
            rep = ((MultivariatePolynomial) poly).ring.getOne();
        else
            return false;

        if (!(rep instanceof Rational))
            return false;

        return ((Rational) rep).numerator() instanceof BigInteger;
    }

    /** Whether coefficient domain is Z */
    public static <T extends IPolynomial<T>> boolean isOverZ(T poly) {
        return poly.isOverZ();
    }

    public static final class Tuple2<A, B> {
        public final A _1;
        public final B _2;

        public Tuple2(A _1, B _2) {
            this._1 = _1;
            this._2 = _2;
        }
    }

    /**
     * Brings polynomial with rational coefficients to common denominator
     *
     * @param poly the polynomial
     * @return (reduced poly, common denominator)
     */
    public static <E> Tuple2<UnivariatePolynomial<E>, E> toCommonDenominator(UnivariatePolynomial<Rational<E>> poly) {
        Ring<Rational<E>> field = poly.ring;
        Ring<E> integralRing = field.getOne().ring;
        E denominator = integralRing.getOne();
        for (int i = 0; i <= poly.degree(); i++)
            if (!poly.isZeroAt(i))
                denominator = integralRing.lcm(denominator, poly.get(i).denominator());

        E[] data = integralRing.createArray(poly.degree() + 1);
        for (int i = 0; i <= poly.degree(); i++) {
            Rational<E> cf = poly.get(i).multiply(denominator);
            assert cf.isIntegral();
            data[i] = cf.numerator();
        }
        return new Tuple2<>(UnivariatePolynomial.createUnsafe(integralRing, data), denominator);
    }

    /**
     * Returns a common denominator of given poly
     */
    public static <E> E commonDenominator(UnivariatePolynomial<Rational<E>> poly) {
        Ring<Rational<E>> field = poly.ring;
        Ring<E> integralRing = field.getOne().ring;
        E denominator = integralRing.getOne();
        for (int i = 0; i <= poly.degree(); i++)
            if (!poly.isZeroAt(i))
                denominator = integralRing.lcm(denominator, poly.get(i).denominator());
        return denominator;
    }

    /**
     * Returns a common denominator of given poly
     */
    public static <E> E commonDenominator(MultivariatePolynomial<Rational<E>> poly) {
        Ring<Rational<E>> field = poly.ring;
        Ring<E> integralRing = field.getOne().ring;
        E denominator = integralRing.getOne();
        for (Rational<E> cf : poly.coefficients())
            denominator = integralRing.lcm(denominator, cf.denominator());
        return denominator;
    }

    /**
     * Brings polynomial with rational coefficients to common denominator
     *
     * @param poly the polynomial
     * @return (reduced poly, common denominator)
     */
    public static <E> Tuple2<MultivariatePolynomial<E>, E> toCommonDenominator(MultivariatePolynomial<Rational<E>> poly) {
        Ring<Rational<E>> field = poly.ring;
        Ring<E> integralRing = field.getOne().ring;
        E denominator = integralRing.getOne();
        for (Rational<E> cf : poly.coefficients())
            denominator = integralRing.lcm(denominator, cf.denominator());

        final E d = denominator;
        MultivariatePolynomial<E> integral = poly.mapCoefficients(integralRing, cf -> {
            Rational<E> r = cf.multiply(d);
            assert integralRing.isOne(r.denominator());
            return r.numerator();
        });
        return new Tuple2<>(integral, denominator);
    }

    public static <E> UnivariatePolynomial<Rational<E>> asOverRationals(Ring<Rational<E>> field, UnivariatePolynomial<E> poly) {
        return poly.mapCoefficients(field, cf -> new Rational<>(poly.ring, cf));
    }

    public static <E> MultivariatePolynomial<Rational<E>> asOverRationals(Ring<Rational<E>> field, MultivariatePolynomial<E> poly) {
        return poly.mapCoefficients(field, cf -> new Rational<>(poly.ring, cf));
    }

    public static <E> UnivariatePolynomial<Rational<E>> divideOverRationals(Ring<Rational<E>> field, UnivariatePolynomial<E> poly, E denominator) {
        return poly.mapCoefficients(field, cf -> new Rational<>(poly.ring, cf, denominator));
    }

    public static <E> MultivariatePolynomial<Rational<E>> divideOverRationals(Ring<Rational<E>> field, MultivariatePolynomial<E> poly, E denominator) {
        return poly.mapCoefficients(field, cf -> new Rational<>(poly.ring, cf, denominator));
    }
}
