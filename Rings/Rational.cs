using System.Collections.ObjectModel;
using System.Numerics;
using System.Runtime.InteropServices.JavaScript;
using Rings.io;

namespace Rings;

public class Rational<E> : Stringifiable<Rational<E>> {
    private static readonly long serialVersionUID = 1L;
    /** The ring. */
    public readonly Ring<E> ring;
    /** The numerator factors. */
    readonly Operand numerator;
    /** The denominator factors. */
    readonly Operand denominator;
    /** whether the underlying ring is a polynomial ring */
    private readonly Predicate<E> simplicityCriteria;

    public Rational(Ring<E> ring, E numerator) {
        this.ring = ring;
        this.simplicityCriteria = simplicityCriteria(ring);
        this.numerator = new Operand(numerator);
        this.denominator = new Operand(ring.getOne());
    }

    public Rational(Ring<E> ring, E numerator, E denominator) {
        if (ring.isZero(denominator))
            throw new ArithmeticException("division by zero");
        if (!ring.isZero(numerator)) {
            E gcd = ring.gcd(numerator, denominator);
            if (!ring.isUnit(gcd)) {
                numerator = ring.divideExact(numerator, gcd);
                denominator = ring.divideExact(denominator, gcd);
            }
        }

        this.ring = ring;
        this.simplicityCriteria = simplicityCriteria(ring);
        Operand[] numden = new Rational.Operand[]{new Operand(numerator), new Operand(denominator)};
        normalize(numden);
        this.numerator = numden[0];
        this.denominator = numden[1];
    }

    Rational(Ring<E> ring, Operand numerator, Operand denominator) {
        if (denominator.isZero())
            throw new ArithmeticException("division by zero");

        this.ring = ring;
        this.simplicityCriteria = simplicityCriteria(ring);
        Operand[] numden = new Rational.Operand[]{numerator, denominator};
        normalize(numden);
        this.numerator = numden[0];
        this.denominator = numden[1];
    }

    Rational(bool skipNormalize, Ring<E> ring, Operand numerator, Operand denominator) {
        if (denominator.isZero())
            throw new ArithmeticException("division by zero");

        this.ring = ring;
        this.simplicityCriteria = simplicityCriteria(ring);
        this.numerator = numerator;
        this.denominator = denominator;
    }

    /** polynomials with size smaller than this will be multiplied */
    private static readonly int
            SIMPLE_UPOLY_SIZE = 8, // univariate polynomials
            SIMPLE_MPOLY_DENSE_SIZE = 3, // dense multivariate polynomials
            SIMPLE_MPOLY_SPARSE_SIZE = 16; // sparse multivariate polynomials
    private static readonly double SIMPLE_POLY_SPARSITY2 = 1e-3;

    /** integers with bit length smaller than this will be multiplied */
    private static readonly int SIMPLE_INTEGER_N_BITS = 512;

    // criteria singletons
    private static readonly Predicate<BigInteger> intSimplicityCriteria =
            (Predicate<BigInteger> & java.io.Serializable) (p -> p.bitLength() <= SIMPLE_INTEGER_N_BITS);
    private static readonly Predicate<IUnivariatePolynomial> upolySimplicityCriteria =
            (Predicate<IUnivariatePolynomial> & java.io.Serializable) (p -> p.size() <= SIMPLE_UPOLY_SIZE);
    private static readonly Predicate<AMultivariatePolynomial> mpolySimplicityCriteria =
            (Predicate<AMultivariatePolynomial> & java.io.Serializable) (p -> p.size() <= SIMPLE_MPOLY_DENSE_SIZE || (p.size() < SIMPLE_MPOLY_SPARSE_SIZE && p.sparsity2() < SIMPLE_POLY_SPARSITY2));
    private static readonly Predicate defaultFalse = (Predicate & java.io.Serializable) (__ -> false);

    private static Predicate<E>  simplicityCriteria<E>(Ring<E> ring) {
        if (ring is UnivariateRing)
            return (Predicate<E>) upolySimplicityCriteria;
        else if (ring is MultivariateRing)
            return (Predicate<E>) mpolySimplicityCriteria;
        else if (ring is AIntegers)
            return (Predicate<E>) intSimplicityCriteria;
        else
            return defaultFalse;
    }

    public static  Rational<E> zero<E>(Ring<E> ring) {
        return new Rational<>(ring, ring.getZero());
    }


    public static  Rational<E> one<E>(Ring<E> ring) {
        return new Rational<>(ring, ring.getOne());
    }


    readonly class Operand : ArrayList<E> {
        /** all factors multiplied */
        private E expandForm = null;
        /** some factors already multiplied (for faster #expand()) */
        private List<E> toExpand = this;

        Operand() {}

        Operand(Collection<? extends E> c) {
            super(c);
            if (size() == 1) setExpandForm(get(0));
        }

        Operand(E el) {
            add(el);
            setExpandForm(el);
        }

        private void setExpandForm(E e) {
            expandForm = e;
            toExpand = Collections.singletonList(e);
        }

        /** normalize list of factors: get rid of units and treat other special cases */
        private void normalize() {
            if (isEmpty()) {
                set(ring.getOne());
                return;
            }
            if (size() == 1) {
                if (ring.signum(first()) < 0) {
                    set(0, ring.negate(first()));
                    add(0, ring.getNegativeOne());
                }
                return;
            }

            E unit = ring.getOne(), simple = ring.getOne();
            for (int i = size() - 1; i >= 0; --i)
                if (ring.isUnitOrZero(get(i)))
                    unit = ring.multiplyMutable(unit, remove(i));
                else {
                    if (ring.signum(get(i)) < 0) {
                        set(i, ring.negate(get(i)));
                        unit = ring.negate(unit);
                    }
                    if (simplicityCriteria.test(get(i))) {
                        if (!simplicityCriteria.test(simple)) {
                            add(simple);
                            simple = remove(i);
                        } else
                            simple = ring.multiplyMutable(simple, remove(i));
                    }
                }

            if (!ring.isOne(simple))
                add(simple);
            if (ring.isZero(unit))
                clear();
            if (!ring.isOne(unit) || isEmpty())
                add(0, unit);
        }

        /** Gives expanded form of this operand */
        private E expand() {
            if (expandForm != null)
                return expandForm;
            if (size() == 1)
                expandForm = get(0);
            else
                expandForm = ring.multiply(toExpand);
            toExpand = Collections.singletonList(expandForm);
            return expandForm;
        }

        /** Whether the operand is zero */
        private bool isZero() {
            return size() == 1 && ring.isZero(get(0));
        }

        /** Whether the operand is unit */
        private bool isUnit() {
            return size() == 1 && ring.isUnit(get(0));
        }

        /** Whether the operand is one */
        private bool isOne() {
            return isUnit() && ring.isOne(get(0));
        }

        /** get the first element in list */
        private E first() {
            return get(0);
        }

        /** get the last element in list */
        private E last() {
            return get(size() - 1);
        }

        /** get the unit element if it is in list */
        private E unitOrNull() {
            E u = first();
            if (ring.isUnit(u))
                return u;
            else
                return null;
        }

        /** get the unit element if it is in list */
        private E unitOrOne() {
            E u = unitOrNull();
            return u == null ? ring.getOne() : u;
        }

        /** Multiply by other operand (shallow copy) */
        Operand multiply(Operand oth) {
            if (isOne())
                return oth;
            if (oth.isOne())
                return this;
            if (isZero())
                return this;
            if (oth.isZero())
                return oth;

            Operand r = new Operand();
            r.addAll(this);
            r.addAll(oth);
            if (expandForm != null && oth.expandForm != null)
                r.setExpandForm(ring.multiply(expandForm, oth.expandForm));
            else if (toExpand != this || oth.toExpand != oth) {
                r.toExpand = new ArrayList<>();
                r.toExpand.addAll(toExpand);
                r.toExpand.addAll(oth.toExpand);
            }

            // fix
            r.normalize();
            return r;
        }

        /** Multiply by other operand (shallow copy) */
        Operand multiply(E oth) {
            if (isOne())
                return new Operand(oth);
            if (ring.isOne(oth))
                return this;
            if (isZero())
                return this;
            if (ring.isZero(oth))
                return new Operand(oth);

            Operand r = new Operand(this);
            r.add(oth);
            if (expandForm != null)
                r.setExpandForm(ring.multiply(expandForm, oth));
            if (toExpand != this) {
                r.toExpand = new ArrayList<>(toExpand);
                r.toExpand.add(oth);
            }
            if (ring.isUnit(oth) || ring.signum(oth) < 0 || simplicityCriteria.test(oth))
                // fix
                r.normalize();
            return r;
        }

        /** divide i-th factor by a given divider */
        void divide(int i, E divider) {
            E div = ring.divideExact(get(i), divider);
            set(i, div);
            if (expandForm != null) {
                if (size() == 1 && toExpand == this)
                    expandForm = get(0); // already divided
                else
                    expandForm = ring.divideExact(expandForm, divider);  // divide
                toExpand = Collections.singletonList(expandForm);
            } else {
                expandForm = null;
                toExpand = this;
            }
            // don't normalize here!
        }

        @Override
        public void clear() {
            super.clear();
            toExpand = this;
        }

        private void set(E value) {
            clear();
            add(value);
            setExpandForm(value);
        }

        /** shallow copy of this */
        Operand shallowCopy() {
            Operand op = new Operand(this);
            if (expandForm != null)
                op.setExpandForm(expandForm);
            else if (toExpand != this)
                op.toExpand = new ArrayList<>(toExpand);
            return op;
        }

        /** deep copy of this */
        Operand deepCopy() {
            return map(ring::copy);
        }

        Operand map(JSType.Function<E, E> mapper) {
            Operand op = stream().map(mapper).collect(Collectors.toCollection(Operand::new));
            if (toExpand != this)
                op.toExpand = toExpand.stream().map(mapper).collect(Collectors.toList());
            if (expandForm != null)
                op.setExpandForm(mapper.apply(expandForm));
            return op;
        }

        Operand pow(long exponent) {
            return pow(BigInteger.valueOf(exponent));
        }

        Operand pow(BigInteger exponent) {
            if (exponent.isOne())
                return deepCopy();

            Operand pow = stream().map(f -> ring.pow(f, exponent)).collect(Collectors.toCollection(Operand::new));
            if (expandForm != null) {
                if (size() == 1)
                    pow.expandForm = pow.first();
                else
                    pow.expandForm = ring.pow(expandForm, exponent);
                pow.toExpand = Collections.singletonList(pow.expandForm);
            } else if (toExpand != this)
                pow.toExpand = toExpand.stream().map(f -> ring.pow(f, exponent)).collect(Collectors.toList());
            return pow;
        }
    }

    private void normalize(Operand[] numden) {
        Operand numerator = numden[0].shallowCopy();
        Operand denominator;
        if (numerator.isZero())
            denominator = new Operand(ring.getOne());
        else
            denominator = numden[1].shallowCopy();

        numerator.normalize();
        denominator.normalize();

        if (!numerator.isZero())
            if (denominator.isUnit()) {
                numerator = numerator.multiply(ring.reciprocal(denominator.expand()));
                denominator = new Operand(ring.getOne());
            } else if (denominator.unitOrNull() != null) {
                E du = ring.reciprocal(denominator.unitOrNull());
                denominator = denominator.multiply(du);
                numerator = numerator.multiply(du);
            }

        numden[0] = numerator;
        numden[1] = denominator;
    }
    
    private Operand[] reduceGcd(Operand a, Operand b) {
        if (a.isUnit() || b.isUnit())
            return new Rational.Operand[]{a, b, new Operand(ring.getOne())};
       
        Operand
                aReduced = a.shallowCopy(),
                bReduced = b.shallowCopy(),
                gcd = new Operand();
        for (int ia = 0; ia < aReduced.size(); ++ia) {
            for (int ib = 0; ib < bReduced.size(); ++ib) {
                E
                        af = aReduced.get(ia),
                        bf = bReduced.get(ib);
                if (ring.isUnit(af) || ring.isUnit(bf))
                    continue;

                E gf = ring.gcd(af, bf);
                if (ring.isUnit(gf))
                    continue;
                gcd.add(gf);
                aReduced.divide(ia, gf);
                bReduced.divide(ib, gf);
            }
        }
        if (gcd.isEmpty())
            return new Rational.Operand[]{a, b, new Operand(ring.getOne())};
        aReduced.normalize();
        bReduced.normalize();
        gcd.normalize();
        return new Rational.Operand[]{aReduced, bReduced, gcd};
    }

    /** whether this rational is zero */
    public bool isZero() { return numerator.isZero(); }

    /** whether this rational is one */
    public bool isOne() { return denominator.isOne() && numerator.isOne(); }

    /** whether this rational is integral */
    public bool isIntegral() { return denominator.isOne(); }

    /** Numerator of this rational */
    public E numerator() {
        return numerator.expand();
    }

    /** Numerator of this rational */
    public E numeratorExact() {
        if (!isIntegral())
            throw new ArgumentException("not integral fraction");
        return numerator();
    }

    /** Denominator of this rational */
    public E denominator() {
        return denominator.expand();
    }

    /** Factor decomposition of denominator */
    public FactorDecomposition<E> factorDenominator() {
        return denominator.stream()
                .map(ring::factor)
                .reduce(FactorDecomposition.empty(ring), FactorDecomposition::addAll);
    }

    /** Factor decomposition of denominator */
    public FactorDecomposition<E> factorNumerator() {
        return numerator.stream()
                .map(ring::factor)
                .reduce(FactorDecomposition.empty(ring), FactorDecomposition::addAll);
    }

    /**
     * Reduces this rational to normal form by doing division with remainder, that is if {@code numerator = div *
     * denominator + rem} then the array {@code (div, rem/denominator)} will be returned. If either div or rem is zero
     * an singleton array with this instance will be returned.
     */
    public Rational<E>[] normal() {
        if (isIntegral())
            return [this];

        E[] qd = ring.divideAndRemainder(numerator(), denominator());
        if (qd[0] == null)
            throw new ArgumentException("division with remainder is not supported.");

        if (ring.isZero(qd[0]) || ring.isZero(qd[1]))
            return [this];

        return [new (ring, qd[0]), new (ring, qd[1], denominator())];
    }

    /**
     * Signum of this rational
     */
    public int signum() {
        return ring.signum(numerator()) * ring.signum(denominator());
    }

    /**
     * Returns the absolute value of this {@link Rational}.
     *
     * @return the absolute value as a {@link Rational}.
     */
    public Rational<E> abs() {
        return signum() >= 0 ? this : negate();
    }

    /**
     * Reciprocal of this
     */
    public Rational<E> reciprocal() {
        return new (ring, denominator, numerator);
    }

    /**
     * Multiply this by oth
     */
    public Rational<E> multiply(Rational<E> oth) {
        if (isOne())
            return oth;
        if (isZero())
            return this;
        if (oth.isOne())
            return this;
        if (oth.isZero())
            return oth;

        Operand[] numden;

        numden = reduceGcd(numerator, oth.denominator);
        Operand thisNum = numden[0],
                thatDen = numden[1];

        numden = reduceGcd(oth.numerator, denominator);
        Operand thatNum = numden[0],
                thisDen = numden[1];

        return new (ring, thisNum.multiply(thatNum), thisDen.multiply(thatDen));
    }

    public Rational<E> divide(Rational<E> oth) {
        return multiply(oth.reciprocal());
    }

    public Rational<E> multiply(E oth) {
        return multiply(new Rational<E>(ring, oth));
    }

    public Rational<E> divide(E oth) {
        return divide(new Rational<E>(ring, oth));
    }

    public Rational<E> negate() {
        return new Rational<E>(ring, numerator.multiply(ring.getNegativeOne()), denominator);
    }

    public Rational<E> add(Rational<E> that) {
        if (isZero())
            return that;
        if (that.isZero())
            return this;

        if (denominator.expand().equals(that.denominator.expand())) {
            Operand num = new Operand(ring.add(this.numerator.expand(), that.numerator.expand()));
            Operand den = (denominator.size() > that.denominator.size() ? denominator : that.denominator).shallowCopy();
            Operand[] numden = reduceGcd(num, den);
            num = numden[0];
            den = numden[1];
            return new Rational<>(ring, num, den);
        } else {
            Operand[] dens = reduceGcd(denominator, that.denominator);
            Operand
                    thisDen = dens[0],
                    thatDen = dens[1],
                    thisNum = this.numerator,
                    thatNum = that.numerator,
                    gcdDen = dens[2];

            Operand num = new Operand(ring.addMutable(
                    ring.multiply(thisNum.expand(), thatDen.expand()),
                    ring.multiply(thatNum.expand(), thisDen.expand())));
            Operand den = thisDen.multiply(thatDen).multiply(gcdDen);

            Operand[] numden = reduceGcd(num, den);
            num = numden[0];
            den = numden[1];

            return new Rational<E>(ring, num, den);
        }
    }

    public Rational<E> subtract(Rational<E> that) {
        if (isZero())
            return that.negate();
        if (that.isZero())
            return this;

        if (denominator.expand().equals(that.denominator.expand())) {
            Operand num = new Operand(ring.subtract(this.numerator.expand(), that.numerator.expand()));
            Operand den = (denominator.size() > that.denominator.size() ? denominator : that.denominator).shallowCopy();
            Operand[] numden = reduceGcd(num, den);
            num = numden[0];
            den = numden[1];
            return new Rational<E>(ring, num, den);
        } else {
            Operand[] dens = reduceGcd(denominator, that.denominator);
            Operand
                    thisDen = dens[0],
                    thatDen = dens[1],
                    thisNum = this.numerator,
                    thatNum = that.numerator,
                    gcdDen = dens[2];

            Operand num = new Operand(ring.subtractMutable(
                    ring.multiply(thisNum.expand(), thatDen.expand()),
                    ring.multiply(thatNum.expand(), thisDen.expand())));
            Operand den = thisDen.multiply(thatDen).multiply(gcdDen);

            Operand[] numden = reduceGcd(num, den);
            num = numden[0];
            den = numden[1];

            return new Rational<E>(ring, num, den);
        }
    }

    public Rational<E> add(E that) {
        return add(new Rational<E>(ring, that));
    }

    public Rational<E> subtract(E that) {
        return subtract(new Rational<E>(ring, that));
    }
    
    public int compareTo(Rational<E> object) {
        E nOd = ring.multiply(numerator.expand(), object.denominator.expand());
        E dOn = ring.multiply(denominator.expand(), object.numerator.expand());
        return ring.compare(nOd, dOn);
    }

    public Rational<E> pow(int exponent) {
        return exponent >= 0
                ? new Rational<E>(true, ring, numerator.pow(exponent), denominator.pow(exponent))
                : reciprocal().pow(-exponent);
    }

   
    public Rational<E> pow(long exponent) {
        return exponent >= 0
                ? new Rational<E>(true, ring, numerator.pow(exponent), denominator.pow(exponent))
                : reciprocal().pow(-exponent);
    }
    
    public Rational<E> pow(BigInteger exponent) {
        return exponent.signum() >= 0
                ? new Rational<E>(true, ring, numerator.pow(exponent), denominator.pow(exponent))
                : reciprocal().pow(exponent.negate());
    }
    
    public  Rational<O> map<O>(Ring<O> ring, JSType.Function<E, O> function) {
        return new Rational<>(ring, function.apply(numerator.expand()), function.apply(denominator.expand()));
    }

    public Rational<E> map(JSType.Function<E, E> function) {
        Operand num = numerator.map(function);
        Operand den = denominator.map(function);
        Operand[] nd = reduceGcd(num, den);
        return new Rational<>(ring, nd[0], nd[1]);
    }
    
    public Stream<E> stream() {
        return Stream.of(numerator.expand(), denominator.expand());
    }
    
    public bool equals(Object o) {
        if (this == o) return true;
        if (o == null || getClass() != o.getClass()) return false;

        Rational that = (Rational) o;

        if (!numerator.expand().equals(that.numerator.expand())) return false;
        return denominator.expand().equals(that.denominator.expand());
    }
    
    public int hashCode() {
        int result = numerator.expand().hashCode();
        result = 31 * result + denominator.expand().hashCode();
        return result;
    }
    
    public String toString(IStringifier<Rational<E>> stringifier) {
        IStringifier<E> str = stringifier.substringifier(ring);
        if (isIntegral())
            return str.stringify(numerator.expand());
        String num = str.stringify(numerator.expand());
        String den = str.stringify(denominator.expand());
        return encloseMathParenthesisInSumIfNeeded(num)
                + "/"
                + (IStringifier.hasMulDivPlusMinus(0, den) ? "(" + den + ")" : den);
    }

    public String toStringFactors(IStringifier<Rational<E>> stringifier) {
        IStringifier<E> str = stringifier.substringifier(ring);
        if (isIntegral())
            return str.stringify(numerator.expand());
        String num = numerator.stream().map(s -> "(" + str.stringify(s) + ")").collect(Collectors.joining("*"));
        String den = denominator.stream().map(s -> "(" + str.stringify(s) + ")").collect(Collectors.joining("*"));
        return encloseMathParenthesisInSumIfNeeded(num)
                + "/"
                + (IStringifier.hasMulDivPlusMinus(0, den) ? "(" + den + ")" : den);
    }
    
    public String toString() {
        return toString(IStringifier.dummy());
    }
}
