namespace Cc.Redberry.Rings.Poly.Multivar
{
    /// <summary>
    /// Monomial with coefficient from generic ring
    /// </summary>
    /// <remarks>
    /// @seecc.redberry.rings.poly.multivar.IMonomialAlgebra.MonomialAlgebra
    /// @since1.0
    /// </remarks>
    public class Monomial<E> : AMonomial<Monomial<E>>
    {
        /// <summary>
        /// the coefficient
        /// </summary>
        public readonly E coefficient;

        /// <summary>
        /// </summary>
        /// <param name="degreeVector">DegreeVector</param>
        /// <param name="coefficient">the coefficient</param>
        public Monomial(DegreeVector degreeVector, E coefficient) : base(degreeVector)
        {
            this.coefficient = coefficient;
        }

       
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        /// <param name="coefficient">the coefficient</param>
        public Monomial(int[] exponents, int totalDegree, E coefficient) : base(exponents, totalDegree)
        {
            this.coefficient = coefficient;
        }

      
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="coefficient">the coefficient</param>
        public Monomial(int[] exponents, E coefficient) : base(exponents)
        {
            this.coefficient = coefficient;
        }

       

        public Monomial(int nVariables, E coefficient) : this(new int[nVariables], 0, coefficient)
        {
        }


        public override Monomial<E> SetCoefficientFrom(Monomial<E> oth)
        {
            return new Monomial<E>(this, oth.coefficient);
        }


        public override Monomial<E> SetDegreeVector(DegreeVector oth)
        {
            if (this == oth)
                return this;
            if (oth == null)
                return null;
            if (oth.exponents == exponents)
                return this;
            return new Monomial<E>(oth, coefficient);
        }

      
        public override Monomial<E> SetDegreeVector(int[] exponents, int totalDegree)
        {
            if (this.exponents == exponents)
                return this;
            return new Monomial<E>(exponents, totalDegree, coefficient);
        }

        
        public override Monomial<E> ForceSetDegreeVector(int[] exponents, int totalDegree)
        {
            return new Monomial<E>(exponents, totalDegree, coefficient);
        }

        
        public virtual Monomial<E> SetCoefficient(E c)
        {
            return new Monomial<E>(exponents, totalDegree, c);
        }

        
        public override bool Equals(object? o)
        {
            if (this == o)
                return true;
            if (o == null || GetType() != o.GetType())
                return false;
            if (!base.Equals(o))
                return false;
            Monomial<TWildcardTodo> monomial = (Monomial<TWildcardTodo>)o;
            return coefficient.Equals(monomial.coefficient);
        }

        
        public override int GetHashCode()
        {
            int result = base.GetHashCode();
            result = 31 * result + coefficient.GetHashCode();
            return result;
        }

        
        public override string ToString()
        {
            string dvString = base.ToString();
            string cfString = coefficient.ToString();
            if (dvString.Length == 0)
                return cfString;
            if (cfString.Equals("1"))
                return dvString;
            return coefficient + "*" + dvString;
        }
    }
}