using Cc.Redberry.Rings.Io;

namespace Cc.Redberry.Rings.Poly
{
    /// <summary>
    /// Polynomial ring.
    /// </summary>
    /// <remarks>@since1.0</remarks>
    public interface IPolynomialRing<Poly> : Ring<Poly> where Poly : IPolynomial<Poly>
    {
        /// <summary>
        /// Number of polynomial variables
        /// </summary>
        int NVariables();

        /// <summary>
        /// Factory polynomial
        /// </summary>
        Poly Factory();

        /// <summary>
        /// Creates poly representing a single specified variable
        /// </summary>
        Poly Variable(int variable);
        /// <summary>
        /// Number of polynomial variables
        /// </summary>
        /// <summary>
        /// Factory polynomial
        /// </summary>
        /// <summary>
        /// Creates poly representing a single specified variable
        /// </summary>
        new int Signum(Poly element)
        {
            return element.SignumOfLC();
        }

        /// <summary>
        /// Number of polynomial variables
        /// </summary>
        /// <summary>
        /// Factory polynomial
        /// </summary>
        /// <summary>
        /// Creates poly representing a single specified variable
        /// </summary>
        /// <summary>
        /// Parse poly from string using specified variables representation
        /// </summary>
        Poly Parse(string @string, params string[] variables)
        {
            return MkCoder(variables).Parse(@string);
        }

        /// <summary>
        /// Number of polynomial variables
        /// </summary>
        /// <summary>
        /// Factory polynomial
        /// </summary>
        /// <summary>
        /// Creates poly representing a single specified variable
        /// </summary>
        /// <summary>
        /// Parse poly from string using specified variables representation
        /// </summary>
        /// <summary>
        /// Simple coder for this ring
        /// </summary>
        Coder<Poly, _, _> MkCoder(params string[] variables)
        {
            return Coder.MkPolynomialCoder(this, variables);
        }
    }
}