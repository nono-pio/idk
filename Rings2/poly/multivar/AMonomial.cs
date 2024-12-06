using Cc.Redberry.Rings.Util;
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
    /// Abstract monomial (degree vector + coefficient). The parent class for {@link MonomialZp64} and {@link Monomial}.
    /// Instances are immutable. Algebraic operations on monomials (multiplication and division) are specified in {@link
    /// IMonomialAlgebra}.
    /// </summary>
    /// <remarks>
    /// @seeDegreeVector
    /// @seeIMonomialAlgebra
    /// @since2.3
    /// </remarks>
    public abstract class AMonomial<Term> : DegreeVector
    {
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        protected AMonomial(int[] exponents, int totalDegree) : base(exponents, totalDegree)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        protected AMonomial(int[] exponents) : this(exponents, ArraysUtil.Sum(exponents))
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <summary>
        /// </summary>
        /// <param name="degreeVector">DegreeVector</param>
        protected AMonomial(DegreeVector degreeVector) : this(degreeVector.exponents, degreeVector.totalDegree)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <summary>
        /// </summary>
        /// <param name="degreeVector">DegreeVector</param>
        /// <summary>
        /// Drop the coefficient
        /// </summary>
        public override DegreeVector Dv()
        {
            return new DegreeVector(exponents, totalDegree);
        }

        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <summary>
        /// </summary>
        /// <param name="degreeVector">DegreeVector</param>
        /// <summary>
        /// Drop the coefficient
        /// </summary>
        /// <summary>
        /// Sets coefficient of this with coefficient of oth
        /// </summary>
        public abstract Term SetCoefficientFrom(Term oth);
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <summary>
        /// </summary>
        /// <param name="degreeVector">DegreeVector</param>
        /// <summary>
        /// Drop the coefficient
        /// </summary>
        /// <summary>
        /// Sets coefficient of this with coefficient of oth
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        public abstract Term SetDegreeVector(DegreeVector oth);
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <summary>
        /// </summary>
        /// <param name="degreeVector">DegreeVector</param>
        /// <summary>
        /// Drop the coefficient
        /// </summary>
        /// <summary>
        /// Sets coefficient of this with coefficient of oth
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        public abstract Term SetDegreeVector(int[] exponents, int totalDegree);
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <summary>
        /// </summary>
        /// <param name="degreeVector">DegreeVector</param>
        /// <summary>
        /// Drop the coefficient
        /// </summary>
        /// <summary>
        /// Sets coefficient of this with coefficient of oth
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        public abstract Term ForceSetDegreeVector(int[] exponents, int totalDegree);
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <summary>
        /// </summary>
        /// <param name="degreeVector">DegreeVector</param>
        /// <summary>
        /// Drop the coefficient
        /// </summary>
        /// <summary>
        /// Sets coefficient of this with coefficient of oth
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        public Term SetDegreeVector(int[] exponents)
        {
            return SetDegreeVector(exponents, ArraysUtil.Sum(exponents));
        }

        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <summary>
        /// </summary>
        /// <param name="degreeVector">DegreeVector</param>
        /// <summary>
        /// Drop the coefficient
        /// </summary>
        /// <summary>
        /// Sets coefficient of this with coefficient of oth
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        public Term Multiply(DegreeVector oth)
        {
            return SetDegreeVector(DvMultiply(oth));
        }

        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <summary>
        /// </summary>
        /// <param name="degreeVector">DegreeVector</param>
        /// <summary>
        /// Drop the coefficient
        /// </summary>
        /// <summary>
        /// Sets coefficient of this with coefficient of oth
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        public Term Multiply(int[] oth)
        {
            return SetDegreeVector(DvMultiply(oth));
        }

        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <summary>
        /// </summary>
        /// <param name="degreeVector">DegreeVector</param>
        /// <summary>
        /// Drop the coefficient
        /// </summary>
        /// <summary>
        /// Sets coefficient of this with coefficient of oth
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        /// <summary>
        /// Gives quotient {@code this / oth } or null if exact division is not possible (e.g. a^2*b^3 / a^3*b^5)
        /// </summary>
        public Term DivideOrNull(DegreeVector divider)
        {
            return SetDegreeVector(DvDivideOrNull(divider));
        }

        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <summary>
        /// </summary>
        /// <param name="degreeVector">DegreeVector</param>
        /// <summary>
        /// Drop the coefficient
        /// </summary>
        /// <summary>
        /// Sets coefficient of this with coefficient of oth
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        /// <summary>
        /// Gives quotient {@code this / oth } or null if exact division is not possible (e.g. a^2*b^3 / a^3*b^5)
        /// </summary>
        /// <summary>
        /// Gives quotient {@code this / oth } or null if exact division is not possible (e.g. a^2*b^3 / a^3*b^5)
        /// </summary>
        public Term DivideOrNull(int[] divider)
        {
            return SetDegreeVector(DvDivideOrNull(divider));
        }

        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <summary>
        /// </summary>
        /// <param name="degreeVector">DegreeVector</param>
        /// <summary>
        /// Drop the coefficient
        /// </summary>
        /// <summary>
        /// Sets coefficient of this with coefficient of oth
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        /// <summary>
        /// Gives quotient {@code this / oth } or null if exact division is not possible (e.g. a^2*b^3 / a^3*b^5)
        /// </summary>
        /// <summary>
        /// Gives quotient {@code this / oth } or null if exact division is not possible (e.g. a^2*b^3 / a^3*b^5)
        /// </summary>
        /// <summary>
        /// Joins new variable (with zero exponent) to degree vector
        /// </summary>
        public Term JoinNewVariable()
        {
            return SetDegreeVector(DvJoinNewVariable());
        }

        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <summary>
        /// </summary>
        /// <param name="degreeVector">DegreeVector</param>
        /// <summary>
        /// Drop the coefficient
        /// </summary>
        /// <summary>
        /// Sets coefficient of this with coefficient of oth
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        /// <summary>
        /// Gives quotient {@code this / oth } or null if exact division is not possible (e.g. a^2*b^3 / a^3*b^5)
        /// </summary>
        /// <summary>
        /// Gives quotient {@code this / oth } or null if exact division is not possible (e.g. a^2*b^3 / a^3*b^5)
        /// </summary>
        /// <summary>
        /// Joins new variable (with zero exponent) to degree vector
        /// </summary>
        /// <summary>
        /// Joins new variables (with zero exponents) to degree vector
        /// </summary>
        public Term JoinNewVariables(int n)
        {
            return SetDegreeVector(DvJoinNewVariables(n));
        }

        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <summary>
        /// </summary>
        /// <param name="degreeVector">DegreeVector</param>
        /// <summary>
        /// Drop the coefficient
        /// </summary>
        /// <summary>
        /// Sets coefficient of this with coefficient of oth
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        /// <summary>
        /// Gives quotient {@code this / oth } or null if exact division is not possible (e.g. a^2*b^3 / a^3*b^5)
        /// </summary>
        /// <summary>
        /// Gives quotient {@code this / oth } or null if exact division is not possible (e.g. a^2*b^3 / a^3*b^5)
        /// </summary>
        /// <summary>
        /// Joins new variable (with zero exponent) to degree vector
        /// </summary>
        /// <summary>
        /// Joins new variables (with zero exponents) to degree vector
        /// </summary>
        /// <summary>
        /// internal API
        /// </summary>
        public Term JoinNewVariables(int newNVariables, int[] mapping)
        {
            return SetDegreeVector(DvJoinNewVariables(newNVariables, mapping));
        }

        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <summary>
        /// </summary>
        /// <param name="degreeVector">DegreeVector</param>
        /// <summary>
        /// Drop the coefficient
        /// </summary>
        /// <summary>
        /// Sets coefficient of this with coefficient of oth
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        /// <summary>
        /// Gives quotient {@code this / oth } or null if exact division is not possible (e.g. a^2*b^3 / a^3*b^5)
        /// </summary>
        /// <summary>
        /// Gives quotient {@code this / oth } or null if exact division is not possible (e.g. a^2*b^3 / a^3*b^5)
        /// </summary>
        /// <summary>
        /// Joins new variable (with zero exponent) to degree vector
        /// </summary>
        /// <summary>
        /// Joins new variables (with zero exponents) to degree vector
        /// </summary>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// Sets the number of variables
        /// </summary>
        public Term SetNVariables(int n)
        {
            return SetDegreeVector(DvSetNVariables(n));
        }

        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <summary>
        /// </summary>
        /// <param name="degreeVector">DegreeVector</param>
        /// <summary>
        /// Drop the coefficient
        /// </summary>
        /// <summary>
        /// Sets coefficient of this with coefficient of oth
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        /// <summary>
        /// Gives quotient {@code this / oth } or null if exact division is not possible (e.g. a^2*b^3 / a^3*b^5)
        /// </summary>
        /// <summary>
        /// Gives quotient {@code this / oth } or null if exact division is not possible (e.g. a^2*b^3 / a^3*b^5)
        /// </summary>
        /// <summary>
        /// Joins new variable (with zero exponent) to degree vector
        /// </summary>
        /// <summary>
        /// Joins new variables (with zero exponents) to degree vector
        /// </summary>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// Sets the number of variables
        /// </summary>
        /// <summary>
        /// Sets exponents of all variables except the specified variable to zero
        /// </summary>
        public Term Select(int var)
        {
            return SetDegreeVector(DvSelect(var));
        }

        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <summary>
        /// </summary>
        /// <param name="degreeVector">DegreeVector</param>
        /// <summary>
        /// Drop the coefficient
        /// </summary>
        /// <summary>
        /// Sets coefficient of this with coefficient of oth
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        /// <summary>
        /// Gives quotient {@code this / oth } or null if exact division is not possible (e.g. a^2*b^3 / a^3*b^5)
        /// </summary>
        /// <summary>
        /// Gives quotient {@code this / oth } or null if exact division is not possible (e.g. a^2*b^3 / a^3*b^5)
        /// </summary>
        /// <summary>
        /// Joins new variable (with zero exponent) to degree vector
        /// </summary>
        /// <summary>
        /// Joins new variables (with zero exponents) to degree vector
        /// </summary>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// Sets the number of variables
        /// </summary>
        /// <summary>
        /// Sets exponents of all variables except the specified variable to zero
        /// </summary>
        /// <summary>
        /// Set's exponents of all variables except specified variables to zero
        /// </summary>
        public Term Select(int[] variables)
        {
            return SetDegreeVector(DvSelect(variables));
        }

        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <summary>
        /// </summary>
        /// <param name="degreeVector">DegreeVector</param>
        /// <summary>
        /// Drop the coefficient
        /// </summary>
        /// <summary>
        /// Sets coefficient of this with coefficient of oth
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        /// <summary>
        /// Gives quotient {@code this / oth } or null if exact division is not possible (e.g. a^2*b^3 / a^3*b^5)
        /// </summary>
        /// <summary>
        /// Gives quotient {@code this / oth } or null if exact division is not possible (e.g. a^2*b^3 / a^3*b^5)
        /// </summary>
        /// <summary>
        /// Joins new variable (with zero exponent) to degree vector
        /// </summary>
        /// <summary>
        /// Joins new variables (with zero exponents) to degree vector
        /// </summary>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// Sets the number of variables
        /// </summary>
        /// <summary>
        /// Sets exponents of all variables except the specified variable to zero
        /// </summary>
        /// <summary>
        /// Set's exponents of all variables except specified variables to zero
        /// </summary>
        /// <summary>
        /// Picks only specified exponents
        /// </summary>
        public Term DropSelect(int[] variables)
        {
            return SetDegreeVector(DvDropSelect(variables));
        }

        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <summary>
        /// </summary>
        /// <param name="degreeVector">DegreeVector</param>
        /// <summary>
        /// Drop the coefficient
        /// </summary>
        /// <summary>
        /// Sets coefficient of this with coefficient of oth
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        /// <summary>
        /// Gives quotient {@code this / oth } or null if exact division is not possible (e.g. a^2*b^3 / a^3*b^5)
        /// </summary>
        /// <summary>
        /// Gives quotient {@code this / oth } or null if exact division is not possible (e.g. a^2*b^3 / a^3*b^5)
        /// </summary>
        /// <summary>
        /// Joins new variable (with zero exponent) to degree vector
        /// </summary>
        /// <summary>
        /// Joins new variables (with zero exponents) to degree vector
        /// </summary>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// Sets the number of variables
        /// </summary>
        /// <summary>
        /// Sets exponents of all variables except the specified variable to zero
        /// </summary>
        /// <summary>
        /// Set's exponents of all variables except specified variables to zero
        /// </summary>
        /// <summary>
        /// Picks only specified exponents
        /// </summary>
        /// <summary>
        /// Selects range from this
        /// </summary>
        /// <param name="from">from inclusive</param>
        /// <param name="to">to exclusive</param>
        public Term Range(int from, int to)
        {
            return SetDegreeVector(DvRange(from, to));
        }

        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <summary>
        /// </summary>
        /// <param name="degreeVector">DegreeVector</param>
        /// <summary>
        /// Drop the coefficient
        /// </summary>
        /// <summary>
        /// Sets coefficient of this with coefficient of oth
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        /// <summary>
        /// Gives quotient {@code this / oth } or null if exact division is not possible (e.g. a^2*b^3 / a^3*b^5)
        /// </summary>
        /// <summary>
        /// Gives quotient {@code this / oth } or null if exact division is not possible (e.g. a^2*b^3 / a^3*b^5)
        /// </summary>
        /// <summary>
        /// Joins new variable (with zero exponent) to degree vector
        /// </summary>
        /// <summary>
        /// Joins new variables (with zero exponents) to degree vector
        /// </summary>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// Sets the number of variables
        /// </summary>
        /// <summary>
        /// Sets exponents of all variables except the specified variable to zero
        /// </summary>
        /// <summary>
        /// Set's exponents of all variables except specified variables to zero
        /// </summary>
        /// <summary>
        /// Picks only specified exponents
        /// </summary>
        /// <summary>
        /// Selects range from this
        /// </summary>
        /// <param name="from">from inclusive</param>
        /// <param name="to">to exclusive</param>
        /// <summary>
        /// Set exponent of specified {@code var} to zero
        /// </summary>
        public Term SetZero(int var)
        {
            return SetDegreeVector(DvSetZero(var));
        }

        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <summary>
        /// </summary>
        /// <param name="degreeVector">DegreeVector</param>
        /// <summary>
        /// Drop the coefficient
        /// </summary>
        /// <summary>
        /// Sets coefficient of this with coefficient of oth
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        /// <summary>
        /// Gives quotient {@code this / oth } or null if exact division is not possible (e.g. a^2*b^3 / a^3*b^5)
        /// </summary>
        /// <summary>
        /// Gives quotient {@code this / oth } or null if exact division is not possible (e.g. a^2*b^3 / a^3*b^5)
        /// </summary>
        /// <summary>
        /// Joins new variable (with zero exponent) to degree vector
        /// </summary>
        /// <summary>
        /// Joins new variables (with zero exponents) to degree vector
        /// </summary>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// Sets the number of variables
        /// </summary>
        /// <summary>
        /// Sets exponents of all variables except the specified variable to zero
        /// </summary>
        /// <summary>
        /// Set's exponents of all variables except specified variables to zero
        /// </summary>
        /// <summary>
        /// Picks only specified exponents
        /// </summary>
        /// <summary>
        /// Selects range from this
        /// </summary>
        /// <param name="from">from inclusive</param>
        /// <param name="to">to exclusive</param>
        /// <summary>
        /// Set exponent of specified {@code var} to zero
        /// </summary>
        /// <summary>
        /// Set all exponents to zero
        /// </summary>
        public Term ToZero()
        {
            if (IsZeroVector())
                return (Term)this;
            return SetDegreeVector(new DegreeVector(new int[NVariables()], 0));
        }

        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <summary>
        /// </summary>
        /// <param name="degreeVector">DegreeVector</param>
        /// <summary>
        /// Drop the coefficient
        /// </summary>
        /// <summary>
        /// Sets coefficient of this with coefficient of oth
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        /// <summary>
        /// Gives quotient {@code this / oth } or null if exact division is not possible (e.g. a^2*b^3 / a^3*b^5)
        /// </summary>
        /// <summary>
        /// Gives quotient {@code this / oth } or null if exact division is not possible (e.g. a^2*b^3 / a^3*b^5)
        /// </summary>
        /// <summary>
        /// Joins new variable (with zero exponent) to degree vector
        /// </summary>
        /// <summary>
        /// Joins new variables (with zero exponents) to degree vector
        /// </summary>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// Sets the number of variables
        /// </summary>
        /// <summary>
        /// Sets exponents of all variables except the specified variable to zero
        /// </summary>
        /// <summary>
        /// Set's exponents of all variables except specified variables to zero
        /// </summary>
        /// <summary>
        /// Picks only specified exponents
        /// </summary>
        /// <summary>
        /// Selects range from this
        /// </summary>
        /// <param name="from">from inclusive</param>
        /// <param name="to">to exclusive</param>
        /// <summary>
        /// Set exponent of specified {@code var} to zero
        /// </summary>
        /// <summary>
        /// Set all exponents to zero
        /// </summary>
        /// <summary>
        /// Set exponents of specified variables to zero
        /// </summary>
        public Term SetZero(int[] variables)
        {
            return SetDegreeVector(DvSetZero(variables));
        }

        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <summary>
        /// </summary>
        /// <param name="degreeVector">DegreeVector</param>
        /// <summary>
        /// Drop the coefficient
        /// </summary>
        /// <summary>
        /// Sets coefficient of this with coefficient of oth
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        /// <summary>
        /// Gives quotient {@code this / oth } or null if exact division is not possible (e.g. a^2*b^3 / a^3*b^5)
        /// </summary>
        /// <summary>
        /// Gives quotient {@code this / oth } or null if exact division is not possible (e.g. a^2*b^3 / a^3*b^5)
        /// </summary>
        /// <summary>
        /// Joins new variable (with zero exponent) to degree vector
        /// </summary>
        /// <summary>
        /// Joins new variables (with zero exponents) to degree vector
        /// </summary>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// Sets the number of variables
        /// </summary>
        /// <summary>
        /// Sets exponents of all variables except the specified variable to zero
        /// </summary>
        /// <summary>
        /// Set's exponents of all variables except specified variables to zero
        /// </summary>
        /// <summary>
        /// Picks only specified exponents
        /// </summary>
        /// <summary>
        /// Selects range from this
        /// </summary>
        /// <param name="from">from inclusive</param>
        /// <param name="to">to exclusive</param>
        /// <summary>
        /// Set exponent of specified {@code var} to zero
        /// </summary>
        /// <summary>
        /// Set all exponents to zero
        /// </summary>
        /// <summary>
        /// Set exponents of specified variables to zero
        /// </summary>
        /// <summary>
        /// Drops specified variable (number of variables will be reduced)
        /// </summary>
        public Term Without(int variable)
        {
            return SetDegreeVector(DvWithout(variable));
        }

        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <summary>
        /// </summary>
        /// <param name="degreeVector">DegreeVector</param>
        /// <summary>
        /// Drop the coefficient
        /// </summary>
        /// <summary>
        /// Sets coefficient of this with coefficient of oth
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        /// <summary>
        /// Gives quotient {@code this / oth } or null if exact division is not possible (e.g. a^2*b^3 / a^3*b^5)
        /// </summary>
        /// <summary>
        /// Gives quotient {@code this / oth } or null if exact division is not possible (e.g. a^2*b^3 / a^3*b^5)
        /// </summary>
        /// <summary>
        /// Joins new variable (with zero exponent) to degree vector
        /// </summary>
        /// <summary>
        /// Joins new variables (with zero exponents) to degree vector
        /// </summary>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// Sets the number of variables
        /// </summary>
        /// <summary>
        /// Sets exponents of all variables except the specified variable to zero
        /// </summary>
        /// <summary>
        /// Set's exponents of all variables except specified variables to zero
        /// </summary>
        /// <summary>
        /// Picks only specified exponents
        /// </summary>
        /// <summary>
        /// Selects range from this
        /// </summary>
        /// <param name="from">from inclusive</param>
        /// <param name="to">to exclusive</param>
        /// <summary>
        /// Set exponent of specified {@code var} to zero
        /// </summary>
        /// <summary>
        /// Set all exponents to zero
        /// </summary>
        /// <summary>
        /// Set exponents of specified variables to zero
        /// </summary>
        /// <summary>
        /// Drops specified variable (number of variables will be reduced)
        /// </summary>
        /// <summary>
        /// Drops specified variables (number of variables will be reduced)
        /// </summary>
        public Term Without(int[] variables)
        {
            return SetDegreeVector(DvWithout(variables));
        }

        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <summary>
        /// </summary>
        /// <param name="degreeVector">DegreeVector</param>
        /// <summary>
        /// Drop the coefficient
        /// </summary>
        /// <summary>
        /// Sets coefficient of this with coefficient of oth
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        /// <summary>
        /// Gives quotient {@code this / oth } or null if exact division is not possible (e.g. a^2*b^3 / a^3*b^5)
        /// </summary>
        /// <summary>
        /// Gives quotient {@code this / oth } or null if exact division is not possible (e.g. a^2*b^3 / a^3*b^5)
        /// </summary>
        /// <summary>
        /// Joins new variable (with zero exponent) to degree vector
        /// </summary>
        /// <summary>
        /// Joins new variables (with zero exponents) to degree vector
        /// </summary>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// Sets the number of variables
        /// </summary>
        /// <summary>
        /// Sets exponents of all variables except the specified variable to zero
        /// </summary>
        /// <summary>
        /// Set's exponents of all variables except specified variables to zero
        /// </summary>
        /// <summary>
        /// Picks only specified exponents
        /// </summary>
        /// <summary>
        /// Selects range from this
        /// </summary>
        /// <param name="from">from inclusive</param>
        /// <param name="to">to exclusive</param>
        /// <summary>
        /// Set exponent of specified {@code var} to zero
        /// </summary>
        /// <summary>
        /// Set all exponents to zero
        /// </summary>
        /// <summary>
        /// Set exponents of specified variables to zero
        /// </summary>
        /// <summary>
        /// Drops specified variable (number of variables will be reduced)
        /// </summary>
        /// <summary>
        /// Drops specified variables (number of variables will be reduced)
        /// </summary>
        /// <summary>
        /// Inserts new variable (with zero exponent)
        /// </summary>
        public Term Insert(int variable)
        {
            return SetDegreeVector(DvInsert(variable));
        }

        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <summary>
        /// </summary>
        /// <param name="degreeVector">DegreeVector</param>
        /// <summary>
        /// Drop the coefficient
        /// </summary>
        /// <summary>
        /// Sets coefficient of this with coefficient of oth
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        /// <summary>
        /// Gives quotient {@code this / oth } or null if exact division is not possible (e.g. a^2*b^3 / a^3*b^5)
        /// </summary>
        /// <summary>
        /// Gives quotient {@code this / oth } or null if exact division is not possible (e.g. a^2*b^3 / a^3*b^5)
        /// </summary>
        /// <summary>
        /// Joins new variable (with zero exponent) to degree vector
        /// </summary>
        /// <summary>
        /// Joins new variables (with zero exponents) to degree vector
        /// </summary>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// Sets the number of variables
        /// </summary>
        /// <summary>
        /// Sets exponents of all variables except the specified variable to zero
        /// </summary>
        /// <summary>
        /// Set's exponents of all variables except specified variables to zero
        /// </summary>
        /// <summary>
        /// Picks only specified exponents
        /// </summary>
        /// <summary>
        /// Selects range from this
        /// </summary>
        /// <param name="from">from inclusive</param>
        /// <param name="to">to exclusive</param>
        /// <summary>
        /// Set exponent of specified {@code var} to zero
        /// </summary>
        /// <summary>
        /// Set all exponents to zero
        /// </summary>
        /// <summary>
        /// Set exponents of specified variables to zero
        /// </summary>
        /// <summary>
        /// Drops specified variable (number of variables will be reduced)
        /// </summary>
        /// <summary>
        /// Drops specified variables (number of variables will be reduced)
        /// </summary>
        /// <summary>
        /// Inserts new variable (with zero exponent)
        /// </summary>
        /// <summary>
        /// Inserts new variables (with zero exponent)
        /// </summary>
        public Term Insert(int variable, int count)
        {
            return SetDegreeVector(DvInsert(variable, count));
        }

        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <summary>
        /// </summary>
        /// <param name="degreeVector">DegreeVector</param>
        /// <summary>
        /// Drop the coefficient
        /// </summary>
        /// <summary>
        /// Sets coefficient of this with coefficient of oth
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        /// <summary>
        /// Gives quotient {@code this / oth } or null if exact division is not possible (e.g. a^2*b^3 / a^3*b^5)
        /// </summary>
        /// <summary>
        /// Gives quotient {@code this / oth } or null if exact division is not possible (e.g. a^2*b^3 / a^3*b^5)
        /// </summary>
        /// <summary>
        /// Joins new variable (with zero exponent) to degree vector
        /// </summary>
        /// <summary>
        /// Joins new variables (with zero exponents) to degree vector
        /// </summary>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// Sets the number of variables
        /// </summary>
        /// <summary>
        /// Sets exponents of all variables except the specified variable to zero
        /// </summary>
        /// <summary>
        /// Set's exponents of all variables except specified variables to zero
        /// </summary>
        /// <summary>
        /// Picks only specified exponents
        /// </summary>
        /// <summary>
        /// Selects range from this
        /// </summary>
        /// <param name="from">from inclusive</param>
        /// <param name="to">to exclusive</param>
        /// <summary>
        /// Set exponent of specified {@code var} to zero
        /// </summary>
        /// <summary>
        /// Set all exponents to zero
        /// </summary>
        /// <summary>
        /// Set exponents of specified variables to zero
        /// </summary>
        /// <summary>
        /// Drops specified variable (number of variables will be reduced)
        /// </summary>
        /// <summary>
        /// Drops specified variables (number of variables will be reduced)
        /// </summary>
        /// <summary>
        /// Inserts new variable (with zero exponent)
        /// </summary>
        /// <summary>
        /// Inserts new variables (with zero exponent)
        /// </summary>
        /// <summary>
        /// Renames old variables to new according to mapping
        /// </summary>
        /// <param name="nVariables">new total number of variables</param>
        /// <param name="mapping">mapping from old variables to new variables</param>
        public Term Map(int nVariables, int[] mapping)
        {
            return SetDegreeVector(DvMap(nVariables, mapping));
        }

        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <summary>
        /// </summary>
        /// <param name="degreeVector">DegreeVector</param>
        /// <summary>
        /// Drop the coefficient
        /// </summary>
        /// <summary>
        /// Sets coefficient of this with coefficient of oth
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        /// <summary>
        /// Gives quotient {@code this / oth } or null if exact division is not possible (e.g. a^2*b^3 / a^3*b^5)
        /// </summary>
        /// <summary>
        /// Gives quotient {@code this / oth } or null if exact division is not possible (e.g. a^2*b^3 / a^3*b^5)
        /// </summary>
        /// <summary>
        /// Joins new variable (with zero exponent) to degree vector
        /// </summary>
        /// <summary>
        /// Joins new variables (with zero exponents) to degree vector
        /// </summary>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// Sets the number of variables
        /// </summary>
        /// <summary>
        /// Sets exponents of all variables except the specified variable to zero
        /// </summary>
        /// <summary>
        /// Set's exponents of all variables except specified variables to zero
        /// </summary>
        /// <summary>
        /// Picks only specified exponents
        /// </summary>
        /// <summary>
        /// Selects range from this
        /// </summary>
        /// <param name="from">from inclusive</param>
        /// <param name="to">to exclusive</param>
        /// <summary>
        /// Set exponent of specified {@code var} to zero
        /// </summary>
        /// <summary>
        /// Set all exponents to zero
        /// </summary>
        /// <summary>
        /// Set exponents of specified variables to zero
        /// </summary>
        /// <summary>
        /// Drops specified variable (number of variables will be reduced)
        /// </summary>
        /// <summary>
        /// Drops specified variables (number of variables will be reduced)
        /// </summary>
        /// <summary>
        /// Inserts new variable (with zero exponent)
        /// </summary>
        /// <summary>
        /// Inserts new variables (with zero exponent)
        /// </summary>
        /// <summary>
        /// Renames old variables to new according to mapping
        /// </summary>
        /// <param name="nVariables">new total number of variables</param>
        /// <param name="mapping">mapping from old variables to new variables</param>
        /// <summary>
        /// Set's exponent of specified variable to specified value
        /// </summary>
        /// <param name="variable">the variable</param>
        /// <param name="exponent">new exponent</param>
        public Term Set(int variable, int exponent)
        {
            return SetDegreeVector(DvSet(variable, exponent));
        }

        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <summary>
        /// </summary>
        /// <param name="degreeVector">DegreeVector</param>
        /// <summary>
        /// Drop the coefficient
        /// </summary>
        /// <summary>
        /// Sets coefficient of this with coefficient of oth
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        /// <summary>
        /// Gives quotient {@code this / oth } or null if exact division is not possible (e.g. a^2*b^3 / a^3*b^5)
        /// </summary>
        /// <summary>
        /// Gives quotient {@code this / oth } or null if exact division is not possible (e.g. a^2*b^3 / a^3*b^5)
        /// </summary>
        /// <summary>
        /// Joins new variable (with zero exponent) to degree vector
        /// </summary>
        /// <summary>
        /// Joins new variables (with zero exponents) to degree vector
        /// </summary>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// Sets the number of variables
        /// </summary>
        /// <summary>
        /// Sets exponents of all variables except the specified variable to zero
        /// </summary>
        /// <summary>
        /// Set's exponents of all variables except specified variables to zero
        /// </summary>
        /// <summary>
        /// Picks only specified exponents
        /// </summary>
        /// <summary>
        /// Selects range from this
        /// </summary>
        /// <param name="from">from inclusive</param>
        /// <param name="to">to exclusive</param>
        /// <summary>
        /// Set exponent of specified {@code var} to zero
        /// </summary>
        /// <summary>
        /// Set all exponents to zero
        /// </summary>
        /// <summary>
        /// Set exponents of specified variables to zero
        /// </summary>
        /// <summary>
        /// Drops specified variable (number of variables will be reduced)
        /// </summary>
        /// <summary>
        /// Drops specified variables (number of variables will be reduced)
        /// </summary>
        /// <summary>
        /// Inserts new variable (with zero exponent)
        /// </summary>
        /// <summary>
        /// Inserts new variables (with zero exponent)
        /// </summary>
        /// <summary>
        /// Renames old variables to new according to mapping
        /// </summary>
        /// <param name="nVariables">new total number of variables</param>
        /// <param name="mapping">mapping from old variables to new variables</param>
        /// <summary>
        /// Set's exponent of specified variable to specified value
        /// </summary>
        /// <param name="variable">the variable</param>
        /// <param name="exponent">new exponent</param>
        public string DvToString(string[] vars)
        {
            return base.ToString(vars);
        }

        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <summary>
        /// </summary>
        /// <param name="degreeVector">DegreeVector</param>
        /// <summary>
        /// Drop the coefficient
        /// </summary>
        /// <summary>
        /// Sets coefficient of this with coefficient of oth
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Sets the degree vector
        /// </summary>
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        /// <summary>
        /// Gives quotient {@code this / oth } or null if exact division is not possible (e.g. a^2*b^3 / a^3*b^5)
        /// </summary>
        /// <summary>
        /// Gives quotient {@code this / oth } or null if exact division is not possible (e.g. a^2*b^3 / a^3*b^5)
        /// </summary>
        /// <summary>
        /// Joins new variable (with zero exponent) to degree vector
        /// </summary>
        /// <summary>
        /// Joins new variables (with zero exponents) to degree vector
        /// </summary>
        /// <summary>
        /// internal API
        /// </summary>
        /// <summary>
        /// Sets the number of variables
        /// </summary>
        /// <summary>
        /// Sets exponents of all variables except the specified variable to zero
        /// </summary>
        /// <summary>
        /// Set's exponents of all variables except specified variables to zero
        /// </summary>
        /// <summary>
        /// Picks only specified exponents
        /// </summary>
        /// <summary>
        /// Selects range from this
        /// </summary>
        /// <param name="from">from inclusive</param>
        /// <param name="to">to exclusive</param>
        /// <summary>
        /// Set exponent of specified {@code var} to zero
        /// </summary>
        /// <summary>
        /// Set all exponents to zero
        /// </summary>
        /// <summary>
        /// Set exponents of specified variables to zero
        /// </summary>
        /// <summary>
        /// Drops specified variable (number of variables will be reduced)
        /// </summary>
        /// <summary>
        /// Drops specified variables (number of variables will be reduced)
        /// </summary>
        /// <summary>
        /// Inserts new variable (with zero exponent)
        /// </summary>
        /// <summary>
        /// Inserts new variables (with zero exponent)
        /// </summary>
        /// <summary>
        /// Renames old variables to new according to mapping
        /// </summary>
        /// <param name="nVariables">new total number of variables</param>
        /// <param name="mapping">mapping from old variables to new variables</param>
        /// <summary>
        /// Set's exponent of specified variable to specified value
        /// </summary>
        /// <param name="variable">the variable</param>
        /// <param name="exponent">new exponent</param>
        public string DvToString()
        {
            return base.ToString();
        }
    }
}