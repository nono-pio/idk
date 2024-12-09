

using System.Numerics;

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
        /// Multiplies term by a number
        /// </summary>
        Term Multiply(Term a, BigInteger b);


        /// <summary>
        /// Gives quotient {@code dividend / divider } or null if exact division is not possible
        /// </summary>
        Term DivideOrNull(Term dividend, Term divider);

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
        /// Gives quotient {@code dividend / divider } or throws {@code ArithmeticException} if exact division is not
        /// possible
        /// </summary>
        Term DivideExact(DegreeVector dividend, Term divider)
        {
            return DivideExact(Create(dividend), divider);
        }

       
        /// <summary>
        /// Raise term in a power of {@code exponent}
        /// </summary>
        /// <param name="term">the term</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code term^exponent}</returns>
        Term Pow(Term term, int exponent);
      
        /// <param name="term">the term</param>
        /// <param name="exponent">the exponent</param>
        /// <returns>{@code term^exponent}</returns>
        /// <summary>
        /// Negates term
        /// </summary>
        Term Negate(Term term);
       
        /// <summary>
        /// Whether term is zero
        /// </summary>
        bool IsZero(Term term);
        
        /// <summary>
        /// Whether term is one
        /// </summary>
        bool IsOne(Term term);
        

        /// <summary>
        /// Whether term is unit
        /// </summary>
        bool IsUnit(Term term);
        

        /// <summary>
        /// Whether term is constant
        /// </summary>
        bool IsConstant(Term term)
        {
            return term.IsZeroVector();
        }

        
     
        /// <summary>
        /// Whether term has unit coefficient
        /// </summary>
        bool IsPureDegreeVector(Term term);
        
       
        /// <summary>
        /// creates term with specified exponents and unit coefficient
        /// </summary>
        Term Create(int[] exponents);
    
       
        /// <summary>
        /// creates term with specified exponents and unit coefficient
        /// </summary>
        Term Create(DegreeVector degreeVector);
        
       
        /// <summary>
        /// creates a unit term
        /// </summary>
        Term GetUnitTerm(int nVariables);
        
      

        /// <summary>
        /// creates a zero term
        /// </summary>
        Term GetZeroTerm(int nVariables);
        
       
        /// <summary>
        /// whether two terms have the same coefficients
        /// </summary>
        bool HaveSameCoefficients(Term a, Term b);
        
       

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
                if (term.totalDegree > int.MaxValue / exponent)
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
                return new Monomial<E>(dv, ring.Multiply(a.coefficient, b.coefficient));
            }

            public virtual Monomial<E> Multiply(Monomial<E> a, BigInteger b)
            {
                return new Monomial<E>(a.exponents, ring.Multiply(a.coefficient, ring.ValueOfBigInteger(b)));
            }

            public virtual Monomial<E> DivideOrNull(Monomial<E> dividend, Monomial<E> divider)
            {
                DegreeVector dv = dividend.DvDivideOrNull(divider);
                if (dv == null)
                    return null;
                E div = ring.DivideOrNull(dividend.coefficient, divider.coefficient);
                if (div == null)
                    return null;
                return new Monomial<E>(dv, div);
            }

            public virtual Monomial<E> Pow(Monomial<E> term, int exponent)
            {
                if (exponent == 1)
                    return term;
                if (exponent == 0)
                    return GetUnitTerm(term.NVariables());
                if (term.totalDegree > int.MaxValue / exponent)
                    throw new ArithmeticException("overflow");
                int[] exps = new int[term.exponents.Length];
                for (int i = 0; i < exps.Length; ++i)
                    exps[i] = term.exponents[i] * exponent;
                return new Monomial<E>(exps, term.totalDegree * exponent, ring.Pow(term.coefficient, exponent));
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
                return new Monomial<E>(exponents, ring.GetOne());
            }

            public virtual Monomial<E> Create(DegreeVector degreeVector)
            {
                return new Monomial<E>(degreeVector, ring.GetOne());
            }



            public virtual Monomial<E> GetUnitTerm(int nVariables)
            {
                return new Monomial<E>(nVariables, ring.GetOne());
            }

            public virtual Monomial<E> GetZeroTerm(int nVariables)
            {
                return new Monomial<E>(nVariables, ring.GetZero());
            }

            public virtual bool HaveSameCoefficients(Monomial<E> a, Monomial<E> b)
            {
                return a.Equals(b);
            }
        }
    }
}