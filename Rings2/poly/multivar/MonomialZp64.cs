using Cc.Redberry.Rings.Bigint;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Cc.Redberry.Rings.Poly.Multivar.RoundingMode;
using static Cc.Redberry.Rings.Poly.Multivar.Associativity;
using static Cc.Redberry.Rings.Poly.Multivar.Operator;
using static Cc.Redberry.Rings.Poly.Multivar.TokenType;
using static Cc.Redberry.Rings.Poly.Multivar.SystemInfo;

namespace Cc.Redberry.Rings.Poly.Multivar
{
    /// <summary>
    /// Monomial with coefficient from Zp with p < 2^64
    /// </summary>
    /// <remarks>
    /// @seecc.redberry.rings.poly.multivar.IMonomialAlgebra.MonomialAlgebraZp64
    /// @since1.0
    /// </remarks>
    public sealed class MonomialZp64 : AMonomial<MonomialZp64>
    {
        /// <summary>
        /// the coefficient
        /// </summary>
        public readonly long coefficient;
        /// <summary>
        /// the coefficient
        /// </summary>
        /// <summary>
        /// </summary>
        /// <param name="degreeVector">DegreeVector</param>
        /// <param name="coefficient">the coefficient</param>
        public MonomialZp64(DegreeVector degreeVector, long coefficient) : base(degreeVector)
        {
            this.coefficient = coefficient;
        }

        /// <summary>
        /// the coefficient
        /// </summary>
        /// <summary>
        /// </summary>
        /// <param name="degreeVector">DegreeVector</param>
        /// <param name="coefficient">the coefficient</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        /// <param name="coefficient">the coefficient</param>
        public MonomialZp64(int[] exponents, int totalDegree, long coefficient) : base(exponents, totalDegree)
        {
            this.coefficient = coefficient;
        }

        /// <summary>
        /// the coefficient
        /// </summary>
        /// <summary>
        /// </summary>
        /// <param name="degreeVector">DegreeVector</param>
        /// <param name="coefficient">the coefficient</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        /// <param name="coefficient">the coefficient</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="coefficient">the coefficient</param>
        public MonomialZp64(int[] exponents, long coefficient) : base(exponents)
        {
            this.coefficient = coefficient;
        }

        /// <summary>
        /// the coefficient
        /// </summary>
        /// <summary>
        /// </summary>
        /// <param name="degreeVector">DegreeVector</param>
        /// <param name="coefficient">the coefficient</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        /// <param name="coefficient">the coefficient</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="coefficient">the coefficient</param>
        public MonomialZp64(int nVariables, long coefficient) : this(new int[nVariables], 0, coefficient)
        {
        }

        /// <summary>
        /// the coefficient
        /// </summary>
        /// <summary>
        /// </summary>
        /// <param name="degreeVector">DegreeVector</param>
        /// <param name="coefficient">the coefficient</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        /// <param name="coefficient">the coefficient</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="coefficient">the coefficient</param>
        public override MonomialZp64 SetCoefficientFrom(MonomialZp64 oth)
        {
            if (coefficient == oth.coefficient)
                return this;
            return new MonomialZp64(this, oth.coefficient);
        }

        /// <summary>
        /// the coefficient
        /// </summary>
        /// <summary>
        /// </summary>
        /// <param name="degreeVector">DegreeVector</param>
        /// <param name="coefficient">the coefficient</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        /// <param name="coefficient">the coefficient</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="coefficient">the coefficient</param>
        public override MonomialZp64 SetDegreeVector(DegreeVector oth)
        {
            if (this == oth)
                return this;
            if (oth == null)
                return null;
            if (oth.exponents == exponents)
                return this;
            return new MonomialZp64(oth, coefficient);
        }

        /// <summary>
        /// the coefficient
        /// </summary>
        /// <summary>
        /// </summary>
        /// <param name="degreeVector">DegreeVector</param>
        /// <param name="coefficient">the coefficient</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        /// <param name="coefficient">the coefficient</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="coefficient">the coefficient</param>
        public override MonomialZp64 SetDegreeVector(int[] exponents, int totalDegree)
        {
            if (this.exponents == exponents)
                return this;
            return new MonomialZp64(exponents, totalDegree, coefficient);
        }

        /// <summary>
        /// the coefficient
        /// </summary>
        /// <summary>
        /// </summary>
        /// <param name="degreeVector">DegreeVector</param>
        /// <param name="coefficient">the coefficient</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        /// <param name="coefficient">the coefficient</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="coefficient">the coefficient</param>
        public override MonomialZp64 ForceSetDegreeVector(int[] exponents, int totalDegree)
        {
            return new MonomialZp64(exponents, totalDegree, coefficient);
        }

        /// <summary>
        /// the coefficient
        /// </summary>
        /// <summary>
        /// </summary>
        /// <param name="degreeVector">DegreeVector</param>
        /// <param name="coefficient">the coefficient</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        /// <param name="coefficient">the coefficient</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="coefficient">the coefficient</param>
        public MonomialZp64 SetCoefficient(long c)
        {
            if (coefficient == c)
                return this;
            return new MonomialZp64(exponents, totalDegree, c);
        }

        /// <summary>
        /// the coefficient
        /// </summary>
        /// <summary>
        /// </summary>
        /// <param name="degreeVector">DegreeVector</param>
        /// <param name="coefficient">the coefficient</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        /// <param name="coefficient">the coefficient</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="coefficient">the coefficient</param>
        public Monomial<BigInteger> ToBigMonomial()
        {
            return new Monomial(this, BigInteger.ValueOf(coefficient));
        }

        /// <summary>
        /// the coefficient
        /// </summary>
        /// <summary>
        /// </summary>
        /// <param name="degreeVector">DegreeVector</param>
        /// <param name="coefficient">the coefficient</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        /// <param name="coefficient">the coefficient</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="coefficient">the coefficient</param>
        public override bool Equals(object o)
        {
            if (this == o)
                return true;
            if (o == null || GetType() != o.GetType())
                return false;
            if (!base.Equals(o))
                return false;
            MonomialZp64 that = (MonomialZp64)o;
            return coefficient == that.coefficient;
        }

        /// <summary>
        /// the coefficient
        /// </summary>
        /// <summary>
        /// </summary>
        /// <param name="degreeVector">DegreeVector</param>
        /// <param name="coefficient">the coefficient</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        /// <param name="coefficient">the coefficient</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="coefficient">the coefficient</param>
        public override int GetHashCode()
        {
            int result = base.GetHashCode();
            result = 31 * result + (int)(coefficient ^ (coefficient >>> 32));
            return result;
        }

        /// <summary>
        /// the coefficient
        /// </summary>
        /// <summary>
        /// </summary>
        /// <param name="degreeVector">DegreeVector</param>
        /// <param name="coefficient">the coefficient</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        /// <param name="coefficient">the coefficient</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="coefficient">the coefficient</param>
        public override string ToString()
        {
            string dvString = base.ToString();
            string cfString = Long.ToString(coefficient);
            if (dvString.IsEmpty())
                return cfString;
            if (coefficient == 1)
                return dvString;
            return coefficient + "*" + dvString;
        }
    }
}