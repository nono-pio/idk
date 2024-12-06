using Cc.Redberry.Rings;
using Cc.Redberry.Rings.Io;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Cc.Redberry.Rings.Poly.RoundingMode;
using static Cc.Redberry.Rings.Poly.Associativity;
using static Cc.Redberry.Rings.Poly.Operator;
using static Cc.Redberry.Rings.Poly.TokenType;
using static Cc.Redberry.Rings.Poly.SystemInfo;

namespace Cc.Redberry.Rings.Poly
{
    /// <summary>
    /// Polynomial ring.
    /// </summary>
    /// <remarks>@since1.0</remarks>
    public interface IPolynomialRing<Poly> : Ring<Poly>
    {
        /// <summary>
        /// Number of polynomial variables
        /// </summary>
        int NVariables();
        /// <summary>
        /// Number of polynomial variables
        /// </summary>
        /// <summary>
        /// Factory polynomial
        /// </summary>
        Poly Factory();
        /// <summary>
        /// Number of polynomial variables
        /// </summary>
        /// <summary>
        /// Factory polynomial
        /// </summary>
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
        int Signum(Poly element)
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
        Coder<Poly, ?, ?> MkCoder(params string[] variables)
        {
            return Coder.MkPolynomialCoder(this, variables);
        }
    }
}