using System.Numerics;
using Rings.io;

namespace Rings.poly;

/**
 * Parent interface for all polynomials. All polynomial instances are mutable, so all structural operations except those
 * where it is stated explicitly will in general modify the instance. All arithmetic operations ({@code add(oth),
 * multiply(oth), monic()} etc.) applies to {@code this} inplace and return {@code this} reference ( so e.g. {@code
 * (poly == poly.add(other))}).
 *
 * <p><b>Note:</b> modifier operations are not synchronized.
 *
 * @param <Poly> the type of polynomial (self type)
 * @since 1.0
 */
public interface IPolynomial<Poly> : Stringifiable<Poly> where Poly : IPolynomial<Poly> {
    /**
     * Returns whether {@code oth} and {@code this} have the same coefficient ring
     *
     * @param oth other polynomial
     * @return whether this and oth are over the same coefficient ring
     */
    bool sameCoefficientRingWith(Poly oth);

    /**
     * Checks whether {@code oth} and {@code this} have the same coefficient ring, if not exception will be thrown
     *
     * @param oth other polynomial
     * @throws IllegalArgumentException if this and oth have different coefficient ring
     */
    void assertSameCoefficientRingWith(Poly oth) {
        if (!sameCoefficientRingWith(oth))
            throw new ArgumentException("Mixing polynomials over different coefficient rings: " + this.coefficientRingToString() + " and " + oth.coefficientRingToString());
    }

    /**
     * Set the coefficient ring from specified poly
     *
     * @param poly the polynomial
     * @return a copy of this with the coefficient ring taken from {@code poly}
     */
    Poly setCoefficientRingFrom(Poly poly);
     
    Poly setCoefficientRingFromOptional(Poly poly) {
        if (sameCoefficientRingWith(poly))
            return (Poly) this;
        else
            return setCoefficientRingFrom(poly);
    }

    /**
     * Returns the degree of this polynomial
     *
     * @return the degree
     */
    int degree();

    /**
     * Returns the size of this polynomial
     *
     * @return the size
     */
    int size();

    /**
     * Returns {@code true} if this is zero
     *
     * @return whether {@code this} is zero
     */
    bool isZero();

    /**
     * Returns {@code true} if this is one
     *
     * @return whether {@code this} is one
     */
    bool isOne();

    /**
     * Returns {@code true} if this polynomial is monic
     *
     * @return whether {@code this} is monic
     */
    bool isMonic();

    /**
     * Returns true if constant term is equal to one
     *
     * @return whether constant term is 1
     */
    bool isUnitCC();

    /**
     * Returns true if constant term is zero
     *
     * @return whether constant term is zero
     */
    bool isZeroCC();

    /**
     * Returns {@code true} if this polynomial has only constant term
     *
     * @return whether {@code this} is constant
     */
    bool isConstant();

    /**
     * Returns {@code true} if this polynomial has only one monomial term
     *
     * @return whether {@code this} has only one monomial term
     */
    bool isMonomial();

    /**
     * Returns whether the coefficient ring of this polynomial is a field
     *
     * @return whether the coefficient ring of this polynomial is a field
     */
    bool isOverField();

    /**
     * Returns whether the coefficient ring of this polynomial is Z
     *
     * @return whether the coefficient ring of this polynomial is Z
     */
    bool isOverZ();

    /**
     * Returns whether the coefficient ring of this polynomial is a finite field
     *
     * @return whether the coefficient ring of this polynomial is a finite field
     */
    bool isOverFiniteField();

    /**
     * Returns whether this polynomial is linear (i.e. of the form {@code a * X + b})
     */
    bool isLinearOrConstant();

    /**
     * Returns whether this polynomial is linear (i.e. of the form {@code a * X + b} with nonzero {@code a})
     */
    bool isLinearExactly();

    /**
     * Returns cardinality of the coefficient ring of this poly
     *
     * @return cardinality of the coefficient ring
     */
    BigInteger coefficientRingCardinality();

    /**
     * Returns characteristic of the coefficient ring of this poly
     *
     * @return characteristic of the coefficient ring
     */
    BigInteger coefficientRingCharacteristic();

    /**
     * Returns whether the {@code coefficientRingCardinality()} is a perfect power
     *
     * @return whether the {@code coefficientRingCardinality()} is a perfect power
     */
    bool isOverPerfectPower();

    /**
     * Returns {@code base} so that {@code coefficientRingCardinality() == base^exponent} or null if cardinality is not
     * finite
     *
     * @return {@code base} so that {@code coefficientRingCardinality() == base^exponent} or null if cardinality is not
     *         finite
     */
    BigInteger coefficientRingPerfectPowerBase();

    /**
     * Returns {@code exponent} so that {@code coefficientRingCardinality() == base^exponent} or null if cardinality is
     * not finite
     *
     * @return {@code exponent} so that {@code coefficientRingCardinality() == base^exponent} or null if cardinality is
     *         not finite
     */
    BigInteger coefficientRingPerfectPowerExponent();

    /**
     * Sets {@code this} to its monic part (that is {@code this} divided by its leading coefficient), or returns {@code
     * null} (causing loss of internal data) if some of the elements can't be exactly divided by the {@code lc()}. NOTE:
     * if {@code null} is returned, the content of {@code this} is destroyed.
     *
     * @return monic {@code this} or {@code null}
     */
    Poly monic();

    /**
     * Sets {@code this} to its monic part (that is {@code this} divided by its leading coefficient), or throws {@code
     * ArithmeticException} if some of the elements can't be exactly divided by the l.c.
     *
     * @return monic {@code this} or {@code null}
     * @throws ArithmeticException if some of the elements can't be exactly divided by the l.c.
     */
    Poly monicExact() {
        Poly self = monic();
        if (self == null)
            throw new ArithmeticException("Not divisible by lc.");
        return self;
    }

    /**
     * Makes this poly monic if coefficient ring is field, otherwise makes this primitive
     */
    Poly canonical() {
        if (isOverField())
            return monic();
        else
            return primitivePart();
    }

    /**
     * Gives signum of the leading coefficient
     *
     * @return signum of the leading coefficient
     */
    int signumOfLC();

    /**
     * If signum of leading coefficient is minus one, negate this
     */
    Poly toPositiveLC() {
        if (signumOfLC() < 0)
            return negate();
        return (Poly) this;
    }

    /**
     * Sets this to zero
     *
     * @return this := zero
     */
    Poly toZero();

    /**
     * Sets the content of this to {@code oth}
     *
     * @param oth the polynomial
     * @return this := oth
     */
    Poly set(Poly oth);

    /**
     * Reduces poly to its primitive part (primitive part will always have positive l.c.)
     *
     * @return primitive part (poly will be modified)
     */
    Poly primitivePart();

    /**
     * Reduces poly to its primitive part, so that primitive part will have the same signum as the initial poly
     *
     * @return primitive part (poly will be modified)
     */
    Poly primitivePartSameSign();

    /**
     * Adds 1 to this
     *
     * @return {@code this + 1}
     */
    Poly increment();

    /**
     * Subtracts 1 from this
     *
     * @return {@code this - 1}
     */
    Poly decrement();

    /**
     * Returns the new instance of zero polynomial (with the same coefficient ring)
     *
     * @return new instance of 0
     */
    Poly createZero();

    /**
     * Returns the new instance of unit polynomial (with the same coefficient ring)
     *
     * @return new instance of 1
     */
    Poly createOne();

    /**
     * Creates constant polynomial with specified value
     *
     * @param value the value
     * @return constant polynomial
     */
    Poly createConstant(long value) {
        return createOne().multiply(value);
    }

    /**
     * Adds {@code oth} to {@code this}.
     *
     * @param oth the polynomial
     * @return {@code this + oth}
     */
    Poly add(Poly oth);


    /**
     * Adds {@code oth} to {@code this}.
     *
     * @param oth the polynomials
     * @return {@code this + oth}
     */
    Poly add(params Poly[] oth) {
        foreach (Poly t in oth)
            add(t);
        return (Poly) this;
    }

    /**
     * Subtracts {@code oth} from {@code this}.
     *
     * @param oth the polynomial
     * @return {@code this - oth}
     */
    Poly subtract(Poly oth);

    /**
     * Subtracts {@code oth} from {@code this}.
     *
     * @param oth the polynomial
     * @return {@code this - oth}
     */
    Poly subtract(params Poly[] oth) {
        foreach (Poly t in oth)
            subtract(t);
        return (Poly) this;
    }

    /**
     * Negates this and returns
     *
     * @return this negated
     */
    Poly negate();

    /**
     * Multiplies this by {@code oth }
     *
     * @param oth the polynomial
     * @return {@code this * oth }
     */
    Poly multiply(Poly oth);

    /**
     * Multiplies this by {@code oth }
     *
     * @param oth the polynomials
     * @return {@code this * oth }
     */ 
    Poly multiply(params Poly[] oth) {
        foreach (Poly t in oth)
            multiply(t);
        return (Poly) this;
    }

    /**
     * Multiplies this by {@code oth }
     *
     * @param oth the polynomials
     * @return {@code this * oth }
     */
      Poly multiply(IEnumerable<Poly> oth) {
        foreach (Poly t in oth)
            multiply(t);
        return (Poly) this;
    }

    /**
     * Multiplies this by {@code factor}
     *
     * @param factor the factor
     * @return {@code this * factor}
     */
    Poly multiply(long factor);

    /**
     * Multiplies this by {@code factor}
     *
     * @param factor the factor
     * @return {@code this * factor}
     */
    Poly multiplyByBigInteger(BigInteger factor);

    /**
     * Squares {@code this}
     *
     * @return {@code this * this}
     */
    Poly square();

    /**
     * Returns the content of this (gcd of coefficients) as a constant poly
     */
    Poly contentAsPoly();

    /**
     * Returns the leading coefficient as a constant poly
     */
    Poly lcAsPoly();

    /**
     * Returns the constant coefficient as a constant poly
     */
    Poly ccAsPoly();

    /**
     * Divides this polynomial by the leading coefficient of {@code other} or returns {@code null} (causing loss of
     * internal data) if some of the elements can't be exactly divided by the {@code other.lc()}. NOTE: if {@code null}
     * is returned, the content of {@code this} is destroyed.
     *
     * @param other the polynomial
     * @return {@code this} divided by the {@code other.lc()} or {@code null} if exact division is not possible
     */
    Poly divideByLC(Poly other);

    /**
     * Sets {@code this} to its monic part multiplied by the leading coefficient of {@code other};
     *
     * @param other other polynomial
     * @return monic part multiplied by the leading coefficient of {@code other} or null if exact division by the
     *         reduced leading coefficient is not possible
     */
    Poly monicWithLC(Poly other);

    /**
     * Multiply this by the leading coefficient of {@code other}
     *
     * @param other polynomial
     * @return this * lc(other)
     */
    Poly multiplyByLC(Poly other);

    /**
     * Deep copy of this
     *
     * @return deep copy of this
     */
    Poly clone();

    /**
     * Deep copy of this (alias for {@link #clone()}, required for scala)
     *
     * @return deep copy of this
     */
     Poly copy() { return clone(); }
    

    /**
     * String representation of the coefficient ring of this
     */
    string coefficientRingToString(IStringifier<Poly> stringifier);

    /**
     * String representation of the coefficient ring of this
     */
    string coefficientRingToString() {
        return coefficientRingToString(IStringifier<Poly>.dummy<Poly>());
    }

    /**
     * String representation of this polynomial with specified string variables
     */
    string toString(params string[] variables) {
        return toString(IStringifier<Poly>.mkPolyStringifier((Poly) this, variables));
    }

    /**
     * @deprecated use {@link cc.redberry.rings.io.Coder} to parse polynomials
     */
    [Obsolete]
    Poly parsePoly(String str);
}
