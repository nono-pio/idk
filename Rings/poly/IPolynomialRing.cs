using Rings.io;

namespace Rings.poly;

/**
 * Polynomial ring.
 *
 * @since 1.0
 */
public interface IPolynomialRing<Poly> : Ring<Poly> where Poly : IPolynomial<Poly>
{
    /**
     * Number of polynomial variables
     */
    int nVariables();

    /**
     * Factory polynomial
     */
    Poly factory();

    /**
     * Creates poly representing a single specified variable
     */
    Poly variable(int variable);

    int signum(Poly element)
    {
        return element.signumOfLC();
    }

    /**
     * Parse poly from string using specified variables representation
     */
    Poly parse(String str, params String[] variables)
    {
        return mkCoder(variables).parse(str);
    }

    /**
     * Simple coder for this ring
     */
    Coder<Poly, _, _> mkCoder(params String[] variables)
    {
        return Coder<_, _, _>.mkPolynomialCoder(this, variables);
    }
}