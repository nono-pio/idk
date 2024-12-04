using System.Numerics;

namespace Rings;

public class ImageRing<F, I> : Ring<I> {
    public readonly Ring<F> ring;
    public readonly Func<F, I> imageFunc;
    public readonly Func<I, F> inverseFunc;

    public ImageRing(Ring<F> ring, Func<I, F> inverseFunc, Func<F, I> imageFunc) {
        this.ring = ring;
        this.inverseFunc = inverseFunc;
        this.imageFunc = imageFunc;
    }

    public I image(F el) {return imageFunc(el);}

    public I[] image(F[] el) {
        I[] array = new I[el.Length];
        for (int i = 0; i < array.Length; i++)
            array[i] = image(el[i]);
        return array;
    }

    public F inverse(I el) {return inverseFunc(el);}

    public F[] inverse(I[] el) {
        F[] array = new F[el.Length];
        for (int i = 0; i < array.Length; i++)
            array[i] = inverse(el[i]);
        return array;
    }

    
    public bool isField() {
        return ring.isField();
    }

    
    public bool isEuclideanRing() {
        return ring.isEuclideanRing();
    }

    
    public BigInteger cardinality() {
        return ring.cardinality();
    }

    
    public BigInteger characteristic() {
        return ring.characteristic();
    }

    
    public bool isPerfectPower() {
        return ring.isPerfectPower();
    }

    
    public BigInteger perfectPowerBase() {
        return ring.perfectPowerBase();
    }

    
    public BigInteger perfectPowerExponent() {
        return ring.perfectPowerExponent();
    }

    
    public I add(I a, I b) {
        return image(ring.add(inverse(a), inverse(b)));
    }

    
    public I subtract(I a, I b) {
        return image(ring.subtract(inverse(a), inverse(b)));
    }

    
    public I multiply(I a, I b) {
        return image(ring.multiply(inverse(a), inverse(b)));
    }

    
    public I negate(I element) {
        return image(ring.negate(inverse(element)));
    }

    
    public I increment(I element) {
        return image(ring.increment(inverse(element)));
    }

    
    public I decrement(I element) {
        return image(ring.decrement(inverse(element)));
    }

    
    public I add(params I[] elements) {
        return image(ring.add(inverse(elements)));
    }

    
    public I multiply(params I[] elements) {
        return image(ring.multiply(inverse(elements)));
    }

    
    public I abs(I el) {
        return image(ring.abs(inverse(el)));
    }

    
    public I copy(I element) {
        return element;
    }

    
    public I[] divideAndRemainder(I dividend, I divider) {
        return image(ring.divideAndRemainder(inverse(dividend), inverse(divider)));
    }

    
    public I quotient(I dividend, I divider) {
        return image(ring.quotient(inverse(dividend), inverse(divider)));
    }

    
    public I remainder(I dividend, I divider) {
        return image(ring.remainder(inverse(dividend), inverse(divider)));
    }

    
    public I reciprocal(I element) {
        return image(ring.reciprocal(inverse(element)));
    }

    
    public I getZero() {
        return image(ring.getZero());
    }

    
    public I getOne() {
        return image(ring.getOne());
    }

    
    public bool isZero(I element) {
        return ring.isZero(inverse(element));
    }

    
    public bool isOne(I element) {
        return ring.isOne(inverse(element));
    }

    
    public bool isUnit(I element) {
        return ring.isUnit(inverse(element));
    }

    
    public I valueOf(long val) {
        return image(ring.valueOf(val));
    }

    
    public I valueOfBigInteger(BigInteger val) {
        return image(ring.valueOfBigInteger(val));
    }

    
    public I valueOf(I val) {
        return image(ring.valueOf(inverse(val)));
    }

    
    public IEnumerable<I> iterator() {
        return StreamSupport
                .stream(Spliterators.spliteratorUnknownSize(ring.iterator(), Spliterator.ORDERED), false)
                .map(this.image).iterator();
    }

    
    public I gcd(I a, I b) {
        return image(ring.gcd(inverse(a), inverse(b)));
    }

    
    public I[] extendedGCD(I a, I b) {
        return image(ring.extendedGCD(inverse(a), inverse(b)));
    }

    
    public I lcm(I a, I b) {
        return image(ring.lcm(inverse(a), inverse(b)));
    }

    
    public I gcd(params I[] elements) {
        return image(ring.gcd(inverse(elements)));
    }

    
    public I gcd(IEnumerable<I> elements) {
        return image(ring.gcd(() => StreamSupport.stream(elements.spliterator(), false).map(this::inverse).iterator()));
    }

    
    public int signum(I element) {
        return ring.signum(inverse(element));
    }


    
    public FactorDecomposition<I> factorSquareFree(I element) {
        return ring.factorSquareFree(inverse(element)).mapTo(this, this.image);
    }

    
    public FactorDecomposition<I> factor(I element) {
        return ring.factor(inverse(element)).mapTo(this, this.image);
    }

    
    public I parse(string str) {
        return image(ring.parse(str));
    }

    
    public I pow(I @base, int exponent) {
        return image(ring.pow(inverse(@base), exponent));
    }

    
    public I pow(I @base, long exponent) {
        return image(ring.pow(inverse(@base), exponent));
    }

    
    public I pow(I @base, BigInteger exponent) {
        return image(ring.pow(inverse(@base), exponent));
    }

    
    public I factorial(long num) {
        return image(ring.factorial(num));
    }

    
    public I randomElement(RandomGenerator rnd) {
        return image(ring.randomElement(rnd));
    }

    
    public int compare(I o1, I o2) {
        return ring.compare(inverse(o1), inverse(o2));
    }

    
    public bool equals(Object o) {
        if (this == o) return true;
        if (o == null || getClass() != o.getClass()) return false;

        ImageRing<?, ?> that = (ImageRing<?, ?>) o;

        if (!ring.equals(that.ring)) return false;
        if (!inverseFunc.equals(that.inverseFunc)) return false;
        return imageFunc.equals(that.imageFunc);
    }

    
    public int hashCode() {
        int result = ring.hashCode();
        result = 31 * result + inverseFunc.hashCode();
        result = 31 * result + imageFunc.hashCode();
        return result;
    }
}
