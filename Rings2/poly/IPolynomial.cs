using System.Numerics;
using Cc.Redberry.Rings.Io;


namespace Cc.Redberry.Rings.Poly
{
    /// <summary>
    /// Parent interface for all polynomials. All polynomial instances are mutable, so all structural operations except those
    /// where it is stated explicitly will in general modify the instance. All arithmetic operations ({@code add(oth),
    /// multiply(oth), monic()} etc.) applies to {@code this} inplace and return {@code this} reference ( so e.g. {@code
    /// (poly == poly.add(other))}).
    /// 
    /// <p><b>Note:</b> modifier operations are not synchronized.
    /// </summary>
    /// <param name="<Poly>">the type of polynomial (self type)</param>
    /// <remarks>@since1.0</remarks>
    public interface IPolynomial<Poly> : IComparable<Poly>, Stringifiable<Poly> where Poly : IPolynomial<Poly>
    {
        /// <summary>
        /// Returns whether {@code oth} and {@code this} have the same coefficient ring
        /// </summary>
        /// <param name="oth">other polynomial</param>
        /// <returns>whether this and oth are over the same coefficient ring</returns>
        bool SameCoefficientRingWith(Poly oth);
       
        /// <summary>
        /// Checks whether {@code oth} and {@code this} have the same coefficient ring, if not exception will be thrown
        /// </summary>
        /// <param name="oth">other polynomial</param>
        /// <exception cref="IllegalArgumentException">if this and oth have different coefficient ring</exception>
        void AssertSameCoefficientRingWith(Poly oth)
        {
            if (!SameCoefficientRingWith(oth))
                throw new ArgumentException("Mixing polynomials over different coefficient rings: " + this.CoefficientRingToString() + " and " + oth.CoefficientRingToString());
        }

        
        Poly SetCoefficientRingFrom(Poly poly);
        
        Poly SetCoefficientRingFromOptional(Poly poly)
        {
            if (SameCoefficientRingWith(poly))
                return (Poly)this;
            else
                return SetCoefficientRingFrom(poly);
        }

        
        /// <summary>
        /// Returns the degree of this polynomial
        /// </summary>
        /// <returns>the degree</returns>
        int Degree();
        
        /// <summary>
        /// Returns the size of this polynomial
        /// </summary>
        /// <returns>the size</returns>
        int Size();
      
        /// <summary>
        /// Returns {@code true} if this is zero
        /// </summary>
        /// <returns>whether {@code this} is zero</returns>
        bool IsZero();
       
        /// <summary>
        /// Returns {@code true} if this is one
        /// </summary>
        /// <returns>whether {@code this} is one</returns>
        bool IsOne();
       
        /// <summary>
        /// Returns {@code true} if this polynomial is monic
        /// </summary>
        /// <returns>whether {@code this} is monic</returns>
        bool IsMonic();
    
        /// <summary>
        /// Returns true if constant term is equal to one
        /// </summary>
        /// <returns>whether constant term is 1</returns>
        bool IsUnitCC();
     
        /// <summary>
        /// Returns true if constant term is zero
        /// </summary>
        /// <returns>whether constant term is zero</returns>
        bool IsZeroCC();
       
       
        /// <summary>
        /// Returns {@code true} if this polynomial has only constant term
        /// </summary>
        /// <returns>whether {@code this} is constant</returns>
        bool IsConstant();
     
        /// <summary>
        /// Returns {@code true} if this polynomial has only one monomial term
        /// </summary>
        /// <returns>whether {@code this} has only one monomial term</returns>
        bool IsMonomial();
       
      
        /// <summary>
        /// Returns whether the coefficient ring of this polynomial is a field
        /// </summary>
        /// <returns>whether the coefficient ring of this polynomial is a field</returns>
        bool IsOverField();
       
      
        /// <summary>
        /// Returns whether the coefficient ring of this polynomial is Z
        /// </summary>
        /// <returns>whether the coefficient ring of this polynomial is Z</returns>
        bool IsOverZ();
        
        /// <summary>
        /// Returns whether the coefficient ring of this polynomial is a finite field
        /// </summary>
        /// <returns>whether the coefficient ring of this polynomial is a finite field</returns>
        bool IsOverFiniteField();
       
      
        /// <summary>
        /// Returns whether this polynomial is linear (i.e. of the form {@code a * X + b})
        /// </summary>
        bool IsLinearOrConstant();
       
      
        /// <summary>
        /// Returns whether this polynomial is linear (i.e. of the form {@code a * X + b} with nonzero {@code a})
        /// </summary>
        bool IsLinearExactly();
       
     
        /// <summary>
        /// Returns cardinality of the coefficient ring of this poly
        /// </summary>
        /// <returns>cardinality of the coefficient ring</returns>
        BigInteger CoefficientRingCardinality();
       
      
        /// <summary>
        /// Returns characteristic of the coefficient ring of this poly
        /// </summary>
        /// <returns>characteristic of the coefficient ring</returns>
        BigInteger CoefficientRingCharacteristic();
       
      
        
        /// <summary>
        /// Returns whether the {@code coefficientRingCardinality()} is a perfect power
        /// </summary>
        /// <returns>whether the {@code coefficientRingCardinality()} is a perfect power</returns>
        bool IsOverPerfectPower();
       
      
        
        /// <summary>
        /// Returns {@code base} so that {@code coefficientRingCardinality() == base^exponent} or null if cardinality is not
        /// finite
        /// </summary>
        /// <returns>{@code base} so that {@code coefficientRingCardinality() == base^exponent} or null if cardinality is not
        ///         finite</returns>
        BigInteger CoefficientRingPerfectPowerBase();
       
        
        /// <summary>
        /// Returns {@code exponent} so that {@code coefficientRingCardinality() == base^exponent} or null if cardinality is
        /// not finite
        /// </summary>
        /// <returns>{@code exponent} so that {@code coefficientRingCardinality() == base^exponent} or null if cardinality is
        ///         not finite</returns>
        BigInteger CoefficientRingPerfectPowerExponent();
       
      
        
       
        /// <summary>
        /// Sets {@code this} to its monic part (that is {@code this} divided by its leading coefficient), or returns {@code
        /// null} (causing loss of internal data) if some of the elements can't be exactly divided by the {@code lc()}. NOTE:
        /// if {@code null} is returned, the content of {@code this} is destroyed.
        /// </summary>
        /// <returns>monic {@code this} or {@code null}</returns>
        Poly Monic();
       
      
        
       
        /// <summary>
        /// Sets {@code this} to its monic part (that is {@code this} divided by its leading coefficient), or throws {@code
        /// ArithmeticException} if some of the elements can't be exactly divided by the l.c.
        /// </summary>
        /// <returns>monic {@code this} or {@code null}</returns>
        /// <exception cref="ArithmeticException">if some of the elements can't be exactly divided by the l.c.</exception>
        Poly MonicExact()
        {
            Poly self = Monic();
            if (self == null)
                throw new ArithmeticException("Not divisible by lc.");
            return self;
        }

       
      
        /// <summary>
        /// Makes this poly monic if coefficient ring is field, otherwise makes this primitive
        /// </summary>
        Poly Canonical()
        {
            if (IsOverField())
                return Monic();
            else
                return PrimitivePart();
        }

       
        /// <summary>
        /// Gives signum of the leading coefficient
        /// </summary>
        /// <returns>signum of the leading coefficient</returns>
        int SignumOfLC();
       
      
        /// <summary>
        /// If signum of leading coefficient is minus one, negate this
        /// </summary>
        Poly ToPositiveLC()
        {
            if (SignumOfLC() < 0)
                return Negate();
            return (Poly)this;
        }

       
      
        /// <summary>
        /// Sets this to zero
        /// </summary>
        /// <returns>this := zero</returns>
        Poly ToZero();
       
      
      
        /// <summary>
        /// Sets the content of this to {@code oth}
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <returns>this := oth</returns>
        Poly Set(Poly oth);
       
        /// <summary>
        /// Reduces poly to its primitive part (primitive part will always have positive l.c.)
        /// </summary>
        /// <returns>primitive part (poly will be modified)</returns>
        Poly PrimitivePart();
       
      
        /// <summary>
        /// Reduces poly to its primitive part, so that primitive part will have the same signum as the initial poly
        /// </summary>
        /// <returns>primitive part (poly will be modified)</returns>
        Poly PrimitivePartSameSign();
       
      
        /// <summary>
        /// Adds 1 to this
        /// </summary>
        /// <returns>{@code this + 1}</returns>
        Poly Increment();
       
      
        /// <summary>
        /// Subtracts 1 from this
        /// </summary>
        /// <returns>{@code this - 1}</returns>
        Poly Decrement();
       
        /// <summary>
        /// Returns the new instance of zero polynomial (with the same coefficient ring)
        /// </summary>
        /// <returns>new instance of 0</returns>
        Poly CreateZero();
       
      
        /// <summary>
        /// Returns the new instance of unit polynomial (with the same coefficient ring)
        /// </summary>
        /// <returns>new instance of 1</returns>
        Poly CreateOne();
        
        /// <summary>
        /// Creates constant polynomial with specified value
        /// </summary>
        /// <param name="value">the value</param>
        /// <returns>constant polynomial</returns>
        Poly CreateConstant(long value)
        {
            return CreateOne().Multiply(value);
        }

       
      
        /// <summary>
        /// Adds {@code oth} to {@code this}.
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <returns>{@code this + oth}</returns>
        Poly Add(Poly oth);
       
        
        /// <summary>
        /// Adds {@code oth} to {@code this}.
        /// </summary>
        /// <param name="oth">the polynomials</param>
        /// <returns>{@code this + oth}</returns>
        Poly Add(params Poly[] oth)
        {
            foreach (Poly t in oth)
                Add(t);
            return (Poly)this;
        }

       
        /// <summary>
        /// Subtracts {@code oth} from {@code this}.
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <returns>{@code this - oth}</returns>
        Poly Subtract(Poly oth);
       
        /// <summary>
        /// Subtracts {@code oth} from {@code this}.
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <returns>{@code this - oth}</returns>
        Poly Subtract(params Poly[] oth)
        {
            foreach (Poly t in oth)
                Subtract(t);
            return (Poly)this;
        }

       
      
        /// <summary>
        /// Negates this and returns
        /// </summary>
        /// <returns>this negated</returns>
        Poly Negate();
       
       
        /// <summary>
        /// Multiplies this by {@code oth }
        /// </summary>
        /// <param name="oth">the polynomial</param>
        /// <returns>{@code this * oth }</returns>
        Poly Multiply(Poly oth);
       
        
        /// <summary>
        /// Multiplies this by {@code oth }
        /// </summary>
        /// <param name="oth">the polynomials</param>
        /// <returns>{@code this * oth }</returns>
        Poly Multiply(params Poly[] oth)
        {
            foreach (Poly t in oth)
                Multiply(t);
            return (Poly)this;
        }

       
        /// <summary>
        /// Multiplies this by {@code oth }
        /// </summary>
        /// <param name="oth">the polynomials</param>
        /// <returns>{@code this * oth }</returns>
        Poly Multiply(IEnumerable<Poly> oth)
        {
            foreach (Poly t in oth)
                Multiply(t);
            return (Poly)this;
        }

        /// <summary>
        /// Multiplies this by {@code factor}
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this * factor}</returns>
        Poly Multiply(long factor);
       
        
        /// <summary>
        /// Multiplies this by {@code factor}
        /// </summary>
        /// <param name="factor">the factor</param>
        /// <returns>{@code this * factor}</returns>
        Poly MultiplyByBigInteger(BigInteger factor);
       
        /// <summary>
        /// Squares {@code this}
        /// </summary>
        /// <returns>{@code this * this}</returns>
        Poly Square();

        /// <summary>
        /// Returns the content of this (gcd of coefficients) as a constant poly
        /// </summary>
        Poly ContentAsPoly();
       
     
        /// <summary>
        /// Returns the leading coefficient as a constant poly
        /// </summary>
        Poly LcAsPoly();
       
       
        /// <summary>
        /// Returns the constant coefficient as a constant poly
        /// </summary>
        Poly CcAsPoly();
       
       
        /// <summary>
        /// Divides this polynomial by the leading coefficient of {@code other} or returns {@code null} (causing loss of
        /// internal data) if some of the elements can't be exactly divided by the {@code other.lc()}. NOTE: if {@code null}
        /// is returned, the content of {@code this} is destroyed.
        /// </summary>
        /// <param name="other">the polynomial</param>
        /// <returns>{@code this} divided by the {@code other.lc()} or {@code null} if exact division is not possible</returns>
        Poly DivideByLC(Poly other);
       
       
        /// <summary>
        /// Sets {@code this} to its monic part multiplied by the leading coefficient of {@code other};
        /// </summary>
        /// <param name="other">other polynomial</param>
        /// <returns>monic part multiplied by the leading coefficient of {@code other} or null if exact division by the
        ///         reduced leading coefficient is not possible</returns>
        Poly MonicWithLC(Poly other);
       
       
        /// <summary>
        /// Multiply this by the leading coefficient of {@code other}
        /// </summary>
        /// <param name="other">polynomial</param>
        /// <returns>this * lc(other)</returns>
        Poly MultiplyByLC(Poly other);
       
     
        /// <summary>
        /// Deep copy of this
        /// </summary>
        /// <returns>deep copy of this</returns>
        Poly Clone();
       
       
      
        /// <summary>
        /// Deep copy of this (alias for {@link #clone()}, required for scala)
        /// </summary>
        /// <returns>deep copy of this</returns>
        Poly Copy()
        {
            return Clone();
        }

       
        /// <summary>
        /// overcome Java generics...
        /// </summary>
        Poly[] CreateArray(int length);
       
       
        /// <summary>
        /// overcome Java generics...
        /// </summary>
        Poly[][] CreateArray2d(int length);
       
       
   
        /// <summary>
        /// overcome Java generics...
        /// </summary>
        Poly[][] CreateArray2d(int length1, int length2);
       
       
      
        /// <summary>
        /// overcome Java generics...
        /// </summary>
        Poly[] CreateArray(Poly a)
        {
            Poly[] r = CreateArray(1);
            r[0] = a;
            return r;
        }

       
      
        /// <summary>
        /// overcome Java generics...
        /// </summary>
        Poly[] CreateArray(Poly a, Poly b)
        {
            Poly[] r = CreateArray(2);
            r[0] = a;
            r[1] = b;
            return r;
        }

      
        /// <summary>
        /// overcome Java generics...
        /// </summary>
        Poly[] CreateArray(Poly a, Poly b, Poly c)
        {
            Poly[] r = CreateArray(3);
            r[0] = a;
            r[1] = b;
            r[2] = c;
            return r;
        }

       
       
        /// <summary>
        /// String representation of the coefficient ring of this
        /// </summary>
        string CoefficientRingToString(IStringifier<Poly> stringifier);
       
       
        /// <summary>
        /// String representation of the coefficient ring of this
        /// </summary>
        string CoefficientRingToString()
        {
            return CoefficientRingToString(IStringifier<Poly>.Dummy<Poly>());
        }

       

        /// <summary>
        /// String representation of this polynomial with specified string variables
        /// </summary>
        string ToString(params string[] variables)
        {
            return ToString(IStringifier<Poly>.MkPolyStringifier((Poly)this, variables));
        }

       

        /// <summary>
        /// </summary>
        /// <remarks>@deprecateduse {@link cc.redberry.rings.io.Coder} to parse polynomials</remarks>
        Poly ParsePoly(string @string);
    }
}