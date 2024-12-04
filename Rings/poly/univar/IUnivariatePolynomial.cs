namespace Rings.poly.univar;


public interface IUnivariatePolynomial<Poly> : IPolynomial<Poly> where Poly : IUnivariatePolynomial<Poly>  {
    new int size() {return degree() + 1;}

    
    int nNonZeroTerms() {
        int c = 0;
        for (int i = degree(); i >= 0; --i)
            if (!isZeroAt(i))
                ++c;
        return c;
    }

    
    bool isZeroAt(int i);

    new bool isZeroCC() {
        return isZeroAt(0);
    }

    
    Poly setZero(int i);

    
    Poly setFrom(int indexInThis, Poly poly, int indexInPoly);

    
    Poly getAsPoly(int i);

    
    TIntHashSet exponents() {
        TIntHashSet degrees = new TIntHashSet();
        for (int i = degree(); i >= 0; --i)
            if (!isZeroAt(i))
                degrees.add(i);
        return degrees;
    }

    
    int firstNonZeroCoefficientPosition();
    
    Poly shiftLeft(int offset);

    
    Poly shiftRight(int offset);

    
    Poly truncate(int newDegree);

    
    Poly getRange(int from, int to);

    
    Poly reverse();

    
    Poly createMonomial(int degree);

    
    Poly derivative();

    new Poly clone();

    
    Poly setAndDestroy(Poly oth);

    
    Poly composition(Poly value);

    
    Poly composition(Ring<Poly> ring, Poly value) {
        if (value.isOne())
            return ring.valueOf(this.clone());
        if (value.isZero())
            return ccAsPoly();

        Poly result = ring.getZero();
        for (int i = degree(); i >= 0; --i)
            result = ring.add(ring.multiply(result, value), getAsPoly(i));
        return result;
    }

    
    IEnumerable<Poly> streamAsPolys();

     UnivariatePolynomial<E> mapCoefficientsAsPolys<E>(Ring<E> ring, Func<Poly, E> mapper) {
        return streamAsPolys().Select(mapper).collect(new UnivariatePolynomial.PolynomialCollector<>(ring));
    }

    
    AMultivariatePolynomial composition(AMultivariatePolynomial value);

    
    AMultivariatePolynomial asMultivariate(Comparator<DegreeVector> ordering);

    
    AMultivariatePolynomial asMultivariate() {
        return asMultivariate(MonomialOrder.DEFAULT);
    }

    void ensureInternalCapacity(int desiredCapacity);

    new bool isLinearOrConstant() {
        return degree() <= 1;
    }

    new bool isLinearExactly() {
        return degree() == 1;
    }
}
