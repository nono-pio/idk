using Cc.Redberry.Rings;
using Cc.Redberry.Rings.Bigint;
using Java.Io;
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
    /// Algebraic operations (multiplication, division) and utility methods for monomials.
    /// </summary>
    /// <remarks>@since2.3</remarks>
    public interface IMonomialAlgebra<Term> : Serializable
    {
        /// <summary>
        /// Multiplies two terms
        /// </summary>
        Term Multiply(Term a, Term b);
        /// <summary>
        /// Multiplies two terms
        /// </summary>
        /// <summary>
        /// Multiplies term by a number
        /// </summary>
        Term Multiply(Term a, BigInteger b);
        /// <summary>
        /// Multiplies two terms
        /// </summary>
        /// <summary>
        /// Multiplies term by a number
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or null if exact division is not possible
        /// </summary>
        Term DivideOrNull(Term dividend, Term divider);
        /// <summary>
        /// Multiplies two terms
        /// </summary>
        /// <summary>
        /// Multiplies term by a number
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or null if exact division is not possible
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or throws {@code ArithmeticException} if exact division is not
        /// possible
        /// </summary>
        Term DivideExact(Term dividend, Term divider)
        {
            Term r = DivideOrNull(dividend, divider);
            if (r == null)
                throw new ArithmeticException("not divisible");
            return r;
        }

        /// <summary>
        /// Multiplies two terms
        /// </summary>
        /// <summary>
        /// Multiplies term by a number
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or null if exact division is not possible
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or throws {@code ArithmeticException} if exact division is not
        /// possible
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or throws {@code ArithmeticException} if exact division is not
        /// possible
        /// </summary>
        Term DivideExact(DegreeVector dividend, Term divider)
        {
            return DivideExact(Create(dividend), divider);
        }

        /// <summary>
        /// Multiplies two terms
        /// </summary>
        /// <summary>
        /// Multiplies term by a number
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or null if exact division is not possible
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or throws {@code ArithmeticException} if exact division is not
        /// possible
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or throws {@code ArithmeticException} if exact division is not
        /// possible
        /// </summary>
        /// <summary>
        /// Raise term in a power of {@code exponent}
        /// </summary>
        /// <param name="term">the term</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code term^exponent}</returns>
        Term Pow(Term term, int exponent);
        /// <summary>
        /// Multiplies two terms
        /// </summary>
        /// <summary>
        /// Multiplies term by a number
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or null if exact division is not possible
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or throws {@code ArithmeticException} if exact division is not
        /// possible
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or throws {@code ArithmeticException} if exact division is not
        /// possible
        /// </summary>
        /// <summary>
        /// Raise term in a power of {@code exponent}
        /// </summary>
        /// <param name="term">the term</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code term^exponent}</returns>
        /// <summary>
        /// Negates term
        /// </summary>
        Term Negate(Term term);
        /// <summary>
        /// Multiplies two terms
        /// </summary>
        /// <summary>
        /// Multiplies term by a number
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or null if exact division is not possible
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or throws {@code ArithmeticException} if exact division is not
        /// possible
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or throws {@code ArithmeticException} if exact division is not
        /// possible
        /// </summary>
        /// <summary>
        /// Raise term in a power of {@code exponent}
        /// </summary>
        /// <param name="term">the term</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code term^exponent}</returns>
        /// <summary>
        /// Negates term
        /// </summary>
        /// <summary>
        /// Whether term is zero
        /// </summary>
        bool IsZero(Term term);
        /// <summary>
        /// Multiplies two terms
        /// </summary>
        /// <summary>
        /// Multiplies term by a number
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or null if exact division is not possible
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or throws {@code ArithmeticException} if exact division is not
        /// possible
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or throws {@code ArithmeticException} if exact division is not
        /// possible
        /// </summary>
        /// <summary>
        /// Raise term in a power of {@code exponent}
        /// </summary>
        /// <param name="term">the term</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code term^exponent}</returns>
        /// <summary>
        /// Negates term
        /// </summary>
        /// <summary>
        /// Whether term is zero
        /// </summary>
        /// <summary>
        /// Whether term is one
        /// </summary>
        bool IsOne(Term term);
        /// <summary>
        /// Multiplies two terms
        /// </summary>
        /// <summary>
        /// Multiplies term by a number
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or null if exact division is not possible
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or throws {@code ArithmeticException} if exact division is not
        /// possible
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or throws {@code ArithmeticException} if exact division is not
        /// possible
        /// </summary>
        /// <summary>
        /// Raise term in a power of {@code exponent}
        /// </summary>
        /// <param name="term">the term</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code term^exponent}</returns>
        /// <summary>
        /// Negates term
        /// </summary>
        /// <summary>
        /// Whether term is zero
        /// </summary>
        /// <summary>
        /// Whether term is one
        /// </summary>
        /// <summary>
        /// Whether term is unit
        /// </summary>
        bool IsUnit(Term term);
        /// <summary>
        /// Multiplies two terms
        /// </summary>
        /// <summary>
        /// Multiplies term by a number
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or null if exact division is not possible
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or throws {@code ArithmeticException} if exact division is not
        /// possible
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or throws {@code ArithmeticException} if exact division is not
        /// possible
        /// </summary>
        /// <summary>
        /// Raise term in a power of {@code exponent}
        /// </summary>
        /// <param name="term">the term</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code term^exponent}</returns>
        /// <summary>
        /// Negates term
        /// </summary>
        /// <summary>
        /// Whether term is zero
        /// </summary>
        /// <summary>
        /// Whether term is one
        /// </summary>
        /// <summary>
        /// Whether term is unit
        /// </summary>
        /// <summary>
        /// Whether term is constant
        /// </summary>
        bool IsConstant(Term term)
        {
            return term.IsZeroVector();
        }

        /// <summary>
        /// Multiplies two terms
        /// </summary>
        /// <summary>
        /// Multiplies term by a number
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or null if exact division is not possible
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or throws {@code ArithmeticException} if exact division is not
        /// possible
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or throws {@code ArithmeticException} if exact division is not
        /// possible
        /// </summary>
        /// <summary>
        /// Raise term in a power of {@code exponent}
        /// </summary>
        /// <param name="term">the term</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code term^exponent}</returns>
        /// <summary>
        /// Negates term
        /// </summary>
        /// <summary>
        /// Whether term is zero
        /// </summary>
        /// <summary>
        /// Whether term is one
        /// </summary>
        /// <summary>
        /// Whether term is unit
        /// </summary>
        /// <summary>
        /// Whether term is constant
        /// </summary>
        /// <summary>
        /// Whether term has unit coefficient
        /// </summary>
        bool IsPureDegreeVector(Term term);
        /// <summary>
        /// Multiplies two terms
        /// </summary>
        /// <summary>
        /// Multiplies term by a number
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or null if exact division is not possible
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or throws {@code ArithmeticException} if exact division is not
        /// possible
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or throws {@code ArithmeticException} if exact division is not
        /// possible
        /// </summary>
        /// <summary>
        /// Raise term in a power of {@code exponent}
        /// </summary>
        /// <param name="term">the term</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code term^exponent}</returns>
        /// <summary>
        /// Negates term
        /// </summary>
        /// <summary>
        /// Whether term is zero
        /// </summary>
        /// <summary>
        /// Whether term is one
        /// </summary>
        /// <summary>
        /// Whether term is unit
        /// </summary>
        /// <summary>
        /// Whether term is constant
        /// </summary>
        /// <summary>
        /// Whether term has unit coefficient
        /// </summary>
        /// <summary>
        /// creates term with specified exponents and unit coefficient
        /// </summary>
        Term Create(int[] exponents);
        /// <summary>
        /// Multiplies two terms
        /// </summary>
        /// <summary>
        /// Multiplies term by a number
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or null if exact division is not possible
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or throws {@code ArithmeticException} if exact division is not
        /// possible
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or throws {@code ArithmeticException} if exact division is not
        /// possible
        /// </summary>
        /// <summary>
        /// Raise term in a power of {@code exponent}
        /// </summary>
        /// <param name="term">the term</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code term^exponent}</returns>
        /// <summary>
        /// Negates term
        /// </summary>
        /// <summary>
        /// Whether term is zero
        /// </summary>
        /// <summary>
        /// Whether term is one
        /// </summary>
        /// <summary>
        /// Whether term is unit
        /// </summary>
        /// <summary>
        /// Whether term is constant
        /// </summary>
        /// <summary>
        /// Whether term has unit coefficient
        /// </summary>
        /// <summary>
        /// creates term with specified exponents and unit coefficient
        /// </summary>
        /// <summary>
        /// creates term with specified exponents and unit coefficient
        /// </summary>
        Term Create(DegreeVector degreeVector);
        /// <summary>
        /// Multiplies two terms
        /// </summary>
        /// <summary>
        /// Multiplies term by a number
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or null if exact division is not possible
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or throws {@code ArithmeticException} if exact division is not
        /// possible
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or throws {@code ArithmeticException} if exact division is not
        /// possible
        /// </summary>
        /// <summary>
        /// Raise term in a power of {@code exponent}
        /// </summary>
        /// <param name="term">the term</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code term^exponent}</returns>
        /// <summary>
        /// Negates term
        /// </summary>
        /// <summary>
        /// Whether term is zero
        /// </summary>
        /// <summary>
        /// Whether term is one
        /// </summary>
        /// <summary>
        /// Whether term is unit
        /// </summary>
        /// <summary>
        /// Whether term is constant
        /// </summary>
        /// <summary>
        /// Whether term has unit coefficient
        /// </summary>
        /// <summary>
        /// creates term with specified exponents and unit coefficient
        /// </summary>
        /// <summary>
        /// creates term with specified exponents and unit coefficient
        /// </summary>
        /// <summary>
        /// creates generic array of specified length
        /// </summary>
        Term[] CreateArray(int length);
        /// <summary>
        /// Multiplies two terms
        /// </summary>
        /// <summary>
        /// Multiplies term by a number
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or null if exact division is not possible
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or throws {@code ArithmeticException} if exact division is not
        /// possible
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or throws {@code ArithmeticException} if exact division is not
        /// possible
        /// </summary>
        /// <summary>
        /// Raise term in a power of {@code exponent}
        /// </summary>
        /// <param name="term">the term</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code term^exponent}</returns>
        /// <summary>
        /// Negates term
        /// </summary>
        /// <summary>
        /// Whether term is zero
        /// </summary>
        /// <summary>
        /// Whether term is one
        /// </summary>
        /// <summary>
        /// Whether term is unit
        /// </summary>
        /// <summary>
        /// Whether term is constant
        /// </summary>
        /// <summary>
        /// Whether term has unit coefficient
        /// </summary>
        /// <summary>
        /// creates term with specified exponents and unit coefficient
        /// </summary>
        /// <summary>
        /// creates term with specified exponents and unit coefficient
        /// </summary>
        /// <summary>
        /// creates generic array of specified length
        /// </summary>
        /// <summary>
        /// creates a unit term
        /// </summary>
        Term GetUnitTerm(int nVariables);
        /// <summary>
        /// Multiplies two terms
        /// </summary>
        /// <summary>
        /// Multiplies term by a number
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or null if exact division is not possible
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or throws {@code ArithmeticException} if exact division is not
        /// possible
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or throws {@code ArithmeticException} if exact division is not
        /// possible
        /// </summary>
        /// <summary>
        /// Raise term in a power of {@code exponent}
        /// </summary>
        /// <param name="term">the term</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code term^exponent}</returns>
        /// <summary>
        /// Negates term
        /// </summary>
        /// <summary>
        /// Whether term is zero
        /// </summary>
        /// <summary>
        /// Whether term is one
        /// </summary>
        /// <summary>
        /// Whether term is unit
        /// </summary>
        /// <summary>
        /// Whether term is constant
        /// </summary>
        /// <summary>
        /// Whether term has unit coefficient
        /// </summary>
        /// <summary>
        /// creates term with specified exponents and unit coefficient
        /// </summary>
        /// <summary>
        /// creates term with specified exponents and unit coefficient
        /// </summary>
        /// <summary>
        /// creates generic array of specified length
        /// </summary>
        /// <summary>
        /// creates a unit term
        /// </summary>
        /// <summary>
        /// creates a zero term
        /// </summary>
        Term GetZeroTerm(int nVariables);
        /// <summary>
        /// Multiplies two terms
        /// </summary>
        /// <summary>
        /// Multiplies term by a number
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or null if exact division is not possible
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or throws {@code ArithmeticException} if exact division is not
        /// possible
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or throws {@code ArithmeticException} if exact division is not
        /// possible
        /// </summary>
        /// <summary>
        /// Raise term in a power of {@code exponent}
        /// </summary>
        /// <param name="term">the term</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code term^exponent}</returns>
        /// <summary>
        /// Negates term
        /// </summary>
        /// <summary>
        /// Whether term is zero
        /// </summary>
        /// <summary>
        /// Whether term is one
        /// </summary>
        /// <summary>
        /// Whether term is unit
        /// </summary>
        /// <summary>
        /// Whether term is constant
        /// </summary>
        /// <summary>
        /// Whether term has unit coefficient
        /// </summary>
        /// <summary>
        /// creates term with specified exponents and unit coefficient
        /// </summary>
        /// <summary>
        /// creates term with specified exponents and unit coefficient
        /// </summary>
        /// <summary>
        /// creates generic array of specified length
        /// </summary>
        /// <summary>
        /// creates a unit term
        /// </summary>
        /// <summary>
        /// creates a zero term
        /// </summary>
        /// <summary>
        /// whether two terms have the same coefficients
        /// </summary>
        bool HaveSameCoefficients(Term a, Term b);
        /// <summary>
        /// Multiplies two terms
        /// </summary>
        /// <summary>
        /// Multiplies term by a number
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or null if exact division is not possible
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or throws {@code ArithmeticException} if exact division is not
        /// possible
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or throws {@code ArithmeticException} if exact division is not
        /// possible
        /// </summary>
        /// <summary>
        /// Raise term in a power of {@code exponent}
        /// </summary>
        /// <param name="term">the term</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code term^exponent}</returns>
        /// <summary>
        /// Negates term
        /// </summary>
        /// <summary>
        /// Whether term is zero
        /// </summary>
        /// <summary>
        /// Whether term is one
        /// </summary>
        /// <summary>
        /// Whether term is unit
        /// </summary>
        /// <summary>
        /// Whether term is constant
        /// </summary>
        /// <summary>
        /// Whether term has unit coefficient
        /// </summary>
        /// <summary>
        /// creates term with specified exponents and unit coefficient
        /// </summary>
        /// <summary>
        /// creates term with specified exponents and unit coefficient
        /// </summary>
        /// <summary>
        /// creates generic array of specified length
        /// </summary>
        /// <summary>
        /// creates a unit term
        /// </summary>
        /// <summary>
        /// creates a zero term
        /// </summary>
        /// <summary>
        /// whether two terms have the same coefficients
        /// </summary>
        /// <summary>
        /// Term algebra for terms over Zp
        /// </summary>
        class MonomialAlgebraZp64 : IMonomialAlgebra<MonomialZp64>
        {
            public readonly IntegersZp64 ring;
            public MonomialAlgebraZp64(IntegersZp64 ring)
            {
                this.ring = ring;
            }

            public virtual MonomialZp64 Multiply(MonomialZp64 a, BigInteger b)
            {
                return new MonomialZp64(a.exponents, ring.Multiply(a.coefficient, ring.Modulus(b)));
            }

            public virtual MonomialZp64 Multiply(MonomialZp64 a, MonomialZp64 b)
            {
                DegreeVector dv = a.DvMultiply(b);
                return new MonomialZp64(dv, ring.Multiply(a.coefficient, b.coefficient));
            }

            public virtual MonomialZp64 DivideOrNull(MonomialZp64 dividend, MonomialZp64 divider)
            {
                DegreeVector dv = dividend.DvDivideOrNull(divider);
                if (dv == null)
                    return null;
                return new MonomialZp64(dv, ring.Divide(dividend.coefficient, divider.coefficient));
            }

            public virtual MonomialZp64 Pow(MonomialZp64 term, int exponent)
            {
                if (exponent == 1)
                    return term;
                if (exponent == 0)
                    return GetUnitTerm(term.NVariables());
                if (term.totalDegree > Integer.MAX_VALUE / exponent)
                    throw new ArithmeticException("overflow");
                int[] exps = new int[term.exponents.Length];
                for (int i = 0; i < exps.Length; ++i)
                    exps[i] = term.exponents[i] * exponent;
                return new MonomialZp64(exps, term.totalDegree * exponent, ring.PowMod(term.coefficient, exponent));
            }

            public virtual MonomialZp64 Negate(MonomialZp64 term)
            {
                return term.SetCoefficient(ring.Negate(term.coefficient));
            }

            public virtual bool IsZero(MonomialZp64 term)
            {
                return term.coefficient == 0;
            }

            public virtual bool IsOne(MonomialZp64 term)
            {
                return IsConstant(term) && term.coefficient == 1;
            }

            public virtual bool IsUnit(MonomialZp64 term)
            {
                return IsConstant(term);
            }

            public virtual bool IsPureDegreeVector(MonomialZp64 term)
            {
                return term.coefficient == 1;
            }

            public virtual MonomialZp64 Create(int[] exponents)
            {
                return new MonomialZp64(exponents, 1);
            }

            public virtual MonomialZp64 Create(DegreeVector degreeVector)
            {
                return new MonomialZp64(degreeVector, 1);
            }

            public virtual MonomialZp64[] CreateArray(int length)
            {
                return new MonomialZp64[length];
            }

            public virtual MonomialZp64 GetUnitTerm(int nVariables)
            {
                return new MonomialZp64(nVariables, 1);
            }

            public virtual MonomialZp64 GetZeroTerm(int nVariables)
            {
                return new MonomialZp64(nVariables, 0);
            }

            public virtual bool HaveSameCoefficients(MonomialZp64 a, MonomialZp64 b)
            {
                return a.coefficient == b.coefficient;
            }
        }

        /// <summary>
        /// Multiplies two terms
        /// </summary>
        /// <summary>
        /// Multiplies term by a number
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or null if exact division is not possible
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or throws {@code ArithmeticException} if exact division is not
        /// possible
        /// </summary>
        /// <summary>
        /// Gives quotient {@code dividend / divider } or throws {@code ArithmeticException} if exact division is not
        /// possible
        /// </summary>
        /// <summary>
        /// Raise term in a power of {@code exponent}
        /// </summary>
        /// <param name="term">the term</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code term^exponent}</returns>
        /// <summary>
        /// Negates term
        /// </summary>
        /// <summary>
        /// Whether term is zero
        /// </summary>
        /// <summary>
        /// Whether term is one
        /// </summary>
        /// <summary>
        /// Whether term is unit
        /// </summary>
        /// <summary>
        /// Whether term is constant
        /// </summary>
        /// <summary>
        /// Whether term has unit coefficient
        /// </summary>
        /// <summary>
        /// creates term with specified exponents and unit coefficient
        /// </summary>
        /// <summary>
        /// creates term with specified exponents and unit coefficient
        /// </summary>
        /// <summary>
        /// creates generic array of specified length
        /// </summary>
        /// <summary>
        /// creates a unit term
        /// </summary>
        /// <summary>
        /// creates a zero term
        /// </summary>
        /// <summary>
        /// whether two terms have the same coefficients
        /// </summary>
        /// <summary>
        /// Term algebra for terms over Zp
        /// </summary>
        /// <summary>
        /// Generic term algebra
        /// </summary>
        class MonomialAlgebra<E> : IMonomialAlgebra<Monomial<E>>
        {
            public readonly Ring<E> ring;
            public MonomialAlgebra(Ring<E> ring)
            {
                this.ring = ring;
            }

            public virtual Monomial<E> Multiply(Monomial<E> a, Monomial<E> b)
            {
                DegreeVector dv = a.DvMultiply(b);
                return new Monomial(dv, ring.Multiply(a.coefficient, b.coefficient));
            }

            public virtual Monomial<E> Multiply(Monomial<E> a, BigInteger b)
            {
                return new Monomial(a.exponents, ring.Multiply(a.coefficient, ring.ValueOfBigInteger(b)));
            }

            public virtual Monomial<E> DivideOrNull(Monomial<E> dividend, Monomial<E> divider)
            {
                DegreeVector dv = dividend.DvDivideOrNull(divider);
                if (dv == null)
                    return null;
                E div = ring.DivideOrNull(dividend.coefficient, divider.coefficient);
                if (div == null)
                    return null;
                return new Monomial(dv, div);
            }

            public virtual Monomial<E> Pow(Monomial<E> term, int exponent)
            {
                if (exponent == 1)
                    return term;
                if (exponent == 0)
                    return GetUnitTerm(term.NVariables());
                if (term.totalDegree > Integer.MAX_VALUE / exponent)
                    throw new ArithmeticException("overflow");
                int[] exps = new int[term.exponents.Length];
                for (int i = 0; i < exps.Length; ++i)
                    exps[i] = term.exponents[i] * exponent;
                return new Monomial(exps, term.totalDegree * exponent, ring.Pow(term.coefficient, exponent));
            }

            public virtual Monomial<E> Negate(Monomial<E> term)
            {
                return term.SetCoefficient(ring.Negate(term.coefficient));
            }

            public virtual bool IsZero(Monomial<E> term)
            {
                return ring.IsZero(term.coefficient);
            }

            public virtual bool IsOne(Monomial<E> term)
            {
                return IsConstant(term) && ring.IsOne(term.coefficient);
            }

            public virtual bool IsUnit(Monomial<E> term)
            {
                return IsConstant(term) && ring.IsUnit(term.coefficient);
            }

            public virtual bool IsPureDegreeVector(Monomial<E> term)
            {
                return ring.IsOne(term.coefficient);
            }

            public virtual Monomial<E> Create(int[] exponents)
            {
                return new Monomial(exponents, ring.GetOne());
            }

            public virtual Monomial<E> Create(DegreeVector degreeVector)
            {
                return new Monomial(degreeVector, ring.GetOne());
            }

            public virtual Monomial<E>[] CreateArray(int length)
            {
                return new Monomial[length];
            }

            public virtual Monomial<E> GetUnitTerm(int nVariables)
            {
                return new Monomial(nVariables, ring.GetOne());
            }

            public virtual Monomial<E> GetZeroTerm(int nVariables)
            {
                return new Monomial(nVariables, ring.GetZero());
            }

            public virtual bool HaveSameCoefficients(Monomial<E> a, Monomial<E> b)
            {
                return a.Equals(b);
            }
        }
    }
}