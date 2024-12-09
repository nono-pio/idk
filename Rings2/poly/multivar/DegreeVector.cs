using Cc.Redberry.Rings.Io;
using Cc.Redberry.Rings.Util;

namespace Cc.Redberry.Rings.Poly.Multivar
{
    /// <summary>
    /// Degree vector. This is parent class for all monomials. Instances are immutable. All {@code DegreeVector} methods are
    /// prefixed with "dv" (which expands to "degree vector"), which means that they affect only exponents (not the
    /// coefficients).
    /// </summary>
    /// <remarks>
    /// @seeAMonomial
    /// @since1.0
    /// </remarks>
    public class DegreeVector
    {
        private static readonly long serialVersionUID = 1;
        /// <summary>
        /// exponents
        /// </summary>
        public readonly int[] exponents;

        /// <summary>
        /// Sum of all exponents (total degree)
        /// </summary>
        public readonly int totalDegree;

        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        /// <param name="totalDegree">total degree (sum of exponents)</param>
        public DegreeVector(int[] exponents, int totalDegree)
        {
            this.exponents = exponents;
            this.totalDegree = totalDegree; // assert ArraysUtil.sum(exponents) == totalDegree;
        }

     
        /// <summary>
        /// </summary>
        /// <param name="exponents">exponents</param>
        public DegreeVector(int[] exponents) : this(exponents, ArraysUtil.Sum(exponents))
        {
        }

      
        /// <summary>
        /// Returns number of variables
        /// </summary>
        public int NVariables()
        {
            return exponents.Length;
        }

       
        /// <summary>
        /// Returns whether all exponents are zero
        /// </summary>
        public bool IsZeroVector()
        {
            return totalDegree == 0;
        }

        /// <summary>
        /// Returns whether all exponents are zero
        /// </summary>
        public virtual DegreeVector Dv()
        {
            return this;
        }

       
        /// <summary>
        /// Returns the total degree in specified variables
        /// </summary>
        public int DvTotalDegree(params int[] variables)
        {
            int d = 0;
            foreach (int v in variables)
                d += exponents[v];
            return d;
        }

      
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        public DegreeVector DvMultiply(DegreeVector oth)
        {
            if (oth.IsZeroVector())
                return this;
            int[] res = new int[exponents.Length];
            for (int i = 0; i < exponents.Length; i++)
                res[i] = exponents[i] + oth.exponents[i];
            return new DegreeVector(res, totalDegree + oth.totalDegree);
        }

        
        /// <summary>
        /// Multiplies this by oth
        /// </summary>
        public DegreeVector DvMultiply(int[] oth)
        {
            int deg = totalDegree;
            int[] res = new int[exponents.Length];
            for (int i = 0; i < exponents.Length; i++)
            {
                res[i] = exponents[i] + oth[i];
                deg += oth[i];
            }

            if (deg == 0)
                return this; // avoid copying
            return new DegreeVector(res, deg);
        }

       
        /// <summary>
        /// Multiplies this by variable^exponent
        /// </summary>
        public DegreeVector DvMultiply(int variable, int exponent)
        {
            int[] res = (int[])exponents.Clone();
            res[variable] += exponent;
            if (res[variable] < 0)
                return null;
            return new DegreeVector(res, totalDegree + exponent);
        }

       
        /// <summary>
        /// Divides this by variable^exponent
        /// </summary>
        public DegreeVector DvDivideOrNull(int variable, int exponent)
        {
            return DvMultiply(variable, -exponent);
        }

       
        /// <summary>
        /// Gives quotient {@code this / oth } or null if exact division is not possible (e.g. a^2*b^3 / a^3*b^5)
        /// </summary>
        public DegreeVector DvDivideOrNull(DegreeVector divider)
        {
            if (divider.IsZeroVector())
                return this;
            int[] res = new int[exponents.Length];
            for (int i = 0; i < exponents.Length; i++)
            {
                res[i] = exponents[i] - divider.exponents[i];
                if (res[i] < 0)
                    return null;
            }

            return new DegreeVector(res, totalDegree - divider.totalDegree);
        }

     
        /// <summary>
        /// Gives quotient {@code this / oth } or null if exact division is not possible (e.g. a^2*b^3 / a^3*b^5)
        /// </summary>
        public DegreeVector DvDivideOrNull(int[] divider)
        {
            int deg = totalDegree;
            int[] res = new int[exponents.Length];
            for (int i = 0; i < exponents.Length; i++)
            {
                res[i] = exponents[i] - divider[i];
                if (res[i] < 0)
                    return null;
                deg -= divider[i];
            }

            if (deg == 0)
                return this; // avoid copying
            return new DegreeVector(res, deg);
        }

     

        /// <summary>
        /// Gives quotient {@code this / oth } or throws {@code ArithmeticException} if exact division is not possible (e.g.
        /// a^2*b^3 / a^3*b^5)
        /// </summary>
        public DegreeVector DvDivideExact(DegreeVector divider)
        {
            DegreeVector quot = DvDivideOrNull(divider);
            if (quot == null)
                throw new ArithmeticException("not divisible");
            return quot;
        }


        /// <summary>
        /// Gives quotient {@code this / oth } or throws {@code ArithmeticException} if exact division is not possible (e.g.
        /// a^2*b^3 / a^3*b^5)
        /// </summary>
        public DegreeVector DvDivideExact(int[] divider)
        {
            DegreeVector quot = DvDivideOrNull(divider);
            if (quot == null)
                throw new ArithmeticException("not divisible");
            return quot;
        }

     
      
        /// <summary>
        /// Tests whether this can be divided by {@code oth} degree vector
        /// </summary>
        public bool DvDivisibleBy(int[] oth)
        {
            for (int i = 0; i < exponents.Length; i++)
                if (exponents[i] < oth[i])
                    return false;
            return true;
        }

     
      
        /// <summary>
        /// Tests whether this can be divided by {@code oth} degree vector
        /// </summary>
        public bool DvDivisibleBy(DegreeVector oth)
        {
            return DvDivisibleBy(oth.exponents);
        }

    
        /// <summary>
        /// Joins new variable (with zero exponent) to degree vector
        /// </summary>
        public DegreeVector DvJoinNewVariable()
        {
            return DvJoinNewVariables(1);
        }

    
        /// <summary>
        /// Joins new variables (with zero exponents) to degree vector
        /// </summary>
        public DegreeVector DvJoinNewVariables(int n)
        {
            return new DegreeVector(Arrays.CopyOf(exponents, exponents.Length + n), totalDegree);
        }

     
        /// <summary>
        /// internal API
        /// </summary>
        public DegreeVector DvJoinNewVariables(int newNVariables, int[] mapping)
        {
            int[] res = new int[newNVariables];
            int c = 0;
            foreach (int i in mapping)
                res[i] = exponents[c++];
            return new DegreeVector(res, totalDegree);
        }

     
      
        /// <summary>
        /// Sets the number of variables
        /// </summary>
        public DegreeVector DvSetNVariables(int n)
        {
            if (n == exponents.Length)
                return this;
            if (n > exponents.Length)
                return new DegreeVector(Arrays.CopyOf(exponents, n), totalDegree);
            else
                return new DegreeVector(Arrays.CopyOf(exponents, n));
        }

     
        /// <summary>
        /// Sets exponents of all variables except the specified variable to zero
        /// </summary>
        public DegreeVector DvSelect(int var)
        {
            int[] res = new int[exponents.Length];
            res[var] = exponents[var];
            return new DegreeVector(res, exponents[var]);
        }

     

        /// <summary>
        /// Set's exponents of all variables except specified variables to zero
        /// </summary>
        public DegreeVector DvSelect(int[] variables)
        {
            int[] res = new int[exponents.Length];
            int deg = 0;
            foreach (int i in variables)
            {
                res[i] = exponents[i];
                deg += exponents[i];
            }

            return new DegreeVector(res, deg);
        }

     
    
        /// <summary>
        /// Picks only specified exponents
        /// </summary>
        public DegreeVector DvDropSelect(int[] variables)
        {
            int[] res = new int[variables.Length];
            int deg = 0;
            int c = 0;
            foreach (int i in variables)
            {
                res[c++] = exponents[i];
                deg += exponents[i];
            }

            return new DegreeVector(res, deg);
        }

     
     
        /// <summary>
        /// Selects range from this
        /// </summary>
        /// <param name="from">from inclusive</param>
        /// <param name="to">to exclusive</param>
        public DegreeVector DvRange(int from, int to)
        {
            if (from == 0 && to == exponents.Length)
            {
                return this;
            }

            return new DegreeVector(Arrays.CopyOfRange(exponents, from, to));
        }

     
       
        /// <summary>
        /// Set exponent of specified {@code var} to zero
        /// </summary>
        public DegreeVector DvSetZero(int var)
        {
            int[] res = (int[])exponents.Clone();
            res[var] = 0;
            return new DegreeVector(res, totalDegree - exponents[var]);
        }

     
       
        /// <summary>
        /// Set exponents of specified variables to zero
        /// </summary>
        public DegreeVector DvSetZero(int[] variables)
        {
            int[] res = (int[])exponents.Clone();
            int deg = totalDegree;
            foreach (int i in variables)
            {
                deg -= exponents[i];
                res[i] = 0;
            }

            return new DegreeVector(res, deg);
        }

     
        
        /// <summary>
        /// Drops specified variable (number of variables will be reduced)
        /// </summary>
        public DegreeVector DvWithout(int variable)
        {
            return new DegreeVector(ArraysUtil.Remove(exponents, variable), totalDegree - exponents[variable]);
        }

     
       
        /// <summary>
        /// Drops specified variables (number of variables will be reduced)
        /// </summary>
        public DegreeVector DvWithout(int[] variables)
        {
            return new DegreeVector(ArraysUtil.Remove(exponents, variables));
        }

     

        /// <summary>
        /// Inserts new variable
        /// </summary>
        public DegreeVector DvInsert(int variable)
        {
            return new DegreeVector(ArraysUtil.Insert(exponents, variable, 0), totalDegree);
        }


        /// <summary>
        /// Inserts new variables
        /// </summary>
        public DegreeVector DvInsert(int variable, int count)
        {
            return new DegreeVector(ArraysUtil.Insert(exponents, variable, 0, count), totalDegree);
        }

     

        /// <summary>
        /// Set's exponent of specified variable to specified value
        /// </summary>
        /// <param name="variable">the variable</param>
        /// <param name="exponent">new exponent</param>
        public DegreeVector DvSet(int variable, int exponent)
        {
            if (exponents[variable] == exponent)
                return this;
            int deg = totalDegree - exponents[variable] + exponent;
            int[] res = (int[])exponents.Clone();
            res[variable] = exponent;
            return new DegreeVector(res, deg);
        }

     
       
      
        /// <summary>
        /// Creates degree vector with old variables renamed to specified mapping variables
        /// </summary>
        /// <param name="nVariables">new total number of variables</param>
        /// <param name="mapping">mapping from old variables to new variables</param>
        public DegreeVector DvMap(int nVariables, int[] mapping)
        {
            int[] newExponents = new int[nVariables];
            for (int i = 0; i < exponents.Length; ++i)
                newExponents[mapping[i]] = exponents[i];
            return new DegreeVector(newExponents, totalDegree);
        }

     
       
    
        /// <summary>
        /// Creates degree vector with old variables renamed to specified mapping variables
        /// </summary>
        /// <param name="nVariables">new total number of variables</param>
        /// <param name="mapping">mapping from old variables to new variables</param>
        int FirstNonZeroVariable()
        {
            for (int i = 0; i < exponents.Length; ++i)
                if (exponents[i] != 0)
                    return i;
            return -1;
        }

     
       
        /// <summary>
        /// Creates degree vector with old variables renamed to specified mapping variables
        /// </summary>
        /// <param name="nVariables">new total number of variables</param>
        /// <param name="mapping">mapping from old variables to new variables</param>
        private static string ToString0(string var, int exp)
        {
            return exp == 0 ? "" : var + (exp == 1 ? "" : "^" + exp);
        }

     
       
        /// <summary>
        /// String representation of this monomial with specified string names for variables
        /// </summary>
        /// <param name="vars">string names of variables</param>
        public string ToString(string[] vars)
        {
            List<string> result = new List<string>();
            for (int i = 0; i < exponents.Length; i++)
                result.Add(ToString0(vars[i], exponents[i]));
            return string.Join('*', result.Where((s) => s.Length != 0));
        }

     
        /// <summary>
        /// String representation of this monomial with specified string names for variables
        /// </summary>
        /// <param name="vars">string names of variables</param>
        public virtual string ToString()
        {
            return ToString(IStringifier<object>.DefaultVars(exponents.Length));
        }

     
        /// <summary>
        /// String representation of this monomial with specified string names for variables
        /// </summary>
        /// <param name="vars">string names of variables</param>
        public string ToStringArray()
        {
            return exponents.ToString();
        }


        /// <summary>
        /// String representation of this monomial with specified string names for variables
        /// </summary>
        /// <param name="vars">string names of variables</param>
        public bool DvEquals(DegreeVector dVector)
        {
            return totalDegree == dVector.totalDegree && exponents.SequenceEqual(dVector.exponents);
        }

        public override bool Equals(object? o)
        {
            if (this == o)
                return true;
            if (o == null || GetType() != o.GetType())
                return false;
            DegreeVector dVector = (DegreeVector)o;
            return DvEquals(dVector);
        }

     
        public override int GetHashCode()
        {
            return exponents.GetHashCode();
        }
    }
}