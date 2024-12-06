using Java.Io;
using Java.Util;
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
    /// Common monomial orderings.
    /// </summary>
    /// <remarks>@since1.0</remarks>
    public sealed class MonomialOrder
    {
        private MonomialOrder()
        {
        }

        /// <summary>
        /// Lexicographic monomial order.
        /// </summary>
        public static readonly Comparator<DegreeVector> LEX = Lex.instance;
        private sealed class Lex : Comparator<DegreeVector>, Serializable
        {
            private static readonly Lex instance = new Lex();
            private Lex()
            {
            }

            public int Compare(DegreeVector a, DegreeVector b)
            {
                for (int i = 0; i < a.exponents.Length; ++i)
                {
                    int c = Integer.Compare(a.exponents[i], b.exponents[i]);
                    if (c != 0)
                        return c;
                }

                return 0;
            }

            protected object ReadResolve()
            {
                return instance;
            }
        }

        /// <summary>
        /// Graded lexicographic monomial order.
        /// </summary>
        public static readonly Comparator<DegreeVector> GRLEX = Grlex.instance;
        private sealed class Grlex : Comparator<DegreeVector>, Serializable
        {
            private static readonly Grlex instance = new Grlex();
            private Grlex()
            {
            }

            public int Compare(DegreeVector a, DegreeVector b)
            {
                int c = Integer.Compare(a.totalDegree, b.totalDegree);
                return c != 0 ? c : LEX.Compare(a, b);
            }

            protected object ReadResolve()
            {
                return instance;
            }
        }

        /// <summary>
        /// Antilexicographic monomial order.
        /// </summary>
        public static readonly Comparator<DegreeVector> ALEX = Alex.instance;
        private sealed class Alex : Comparator<DegreeVector>, Serializable
        {
            private static readonly Alex instance = new Alex();
            private Alex()
            {
            }

            public int Compare(DegreeVector a, DegreeVector b)
            {
                return LEX.Compare(b, a);
            }

            protected object ReadResolve()
            {
                return instance;
            }
        }

        /// <summary>
        /// Graded reverse lexicographic monomial order
        /// </summary>
        public static readonly Comparator<DegreeVector> GREVLEX = Grevlex.instance;
        private sealed class Grevlex : Comparator<DegreeVector>, Serializable
        {
            private static readonly Grevlex instance = new Grevlex();
            private Grevlex()
            {
            }

            public int Compare(DegreeVector a, DegreeVector b)
            {
                int c = Integer.Compare(a.totalDegree, b.totalDegree);
                if (c != 0)
                    return c;
                for (int i = a.exponents.length - 1; i >= 0; --i)
                {
                    c = Integer.Compare(b.exponents[i], a.exponents[i]);
                    if (c != 0)
                        return c;
                }

                return 0;
            }

            protected object ReadResolve()
            {
                return instance;
            }
        }

        /// <summary>
        /// Default monomial order (GREVLEX)
        /// </summary>
        public static readonly Comparator<DegreeVector> DEFAULT = Parse(System.GetProperty("defaultMonomialOrder", "grevlex").ToLowerCase());
        static Comparator<DegreeVector> Parse(string @string)
        {
            switch (@string.ToLowerCase())
            {
                case "lex":
                    return LEX;
                case "grlex":
                    return GRLEX;
                case "grevlex":
                    return GREVLEX;
                case "alex":
                    return ALEX;
                default:
                    throw new Exception("unknown: " + @string);
                    break;
            }
        }

        /// <summary>
        /// Block product of orderings
        /// </summary>
        public static Comparator<DegreeVector> Product(Comparator<DegreeVector>[] orderings, int[] nVariables)
        {
            return new ProductOrder(orderings, nVariables);
        }

        /// <summary>
        /// Block product of orderings
        /// </summary>
        public static Comparator<DegreeVector> Product(Comparator<DegreeVector> a, int anVariables, Comparator<DegreeVector> b, int bnVariable)
        {
            return new ProductOrder(new Comparator[] { a, b }, new int[] { anVariables, bnVariable });
        }

        /// <summary>
        /// whether monomial order is graded
        /// </summary>
        public static bool IsGradedOrder(Comparator<DegreeVector> monomialOrder)
        {
            return monomialOrder == GREVLEX || monomialOrder == GRLEX || monomialOrder is GrevLexWithPermutation;
        }

        sealed class ProductOrder : Comparator<DegreeVector>, Serializable
        {
            readonly Comparator<DegreeVector>[] orderings;
            readonly int[] nVariables;
            ProductOrder(Comparator<DegreeVector>[] orderings, int[] nVariables)
            {
                this.orderings = orderings;
                this.nVariables = nVariables;
            }

            public int Compare(DegreeVector a, DegreeVector b)
            {
                int prev = 0;
                for (int i = 0; i < nVariables.Length; i++)
                {

                    // for each block
                    DegreeVector aBlock = a.DvRange(prev, prev + nVariables[i]), bBlock = b.DvRange(prev, prev + nVariables[i]);
                    int c = orderings[i].Compare(aBlock, bBlock);
                    if (c != 0)
                        return c;
                    prev += nVariables[i];
                }

                return 0;
            }

            // for each block
            public bool Equals(object o)
            {
                if (this == o)
                    return true;
                if (o == null || GetType() != o.GetType())
                    return false;
                ProductOrder that = (ProductOrder)o;

                // Probably incorrect - comparing Object[] arrays with Arrays.equals
                if (!Arrays.Equals(orderings, that.orderings))
                    return false;
                return Arrays.Equals(nVariables, that.nVariables);
            }

            // for each block
            // Probably incorrect - comparing Object[] arrays with Arrays.equals
            public int GetHashCode()
            {
                int result = Arrays.GetHashCode(orderings);
                result = 31 * result + Arrays.GetHashCode(nVariables);
                return result;
            }
        }

        public sealed class GrevLexWithPermutation : Comparator<DegreeVector>, Serializable
        {
            readonly int[] permutation;
            GrevLexWithPermutation(int[] permutation)
            {
                this.permutation = permutation;
            }

            public int Compare(DegreeVector a, DegreeVector b)
            {
                int c = Integer.Compare(a.totalDegree, b.totalDegree);
                if (c != 0)
                    return c;
                for (int i = a.exponents.length - 1; i >= 0; --i)
                {
                    c = Integer.Compare(b.exponents[permutation[i]], a.exponents[permutation[i]]);
                    if (c != 0)
                        return c;
                }

                return 0;
            }

            public bool Equals(object o)
            {
                if (this == o)
                    return true;
                if (o == null || GetType() != o.GetType())
                    return false;
                GrevLexWithPermutation that = (GrevLexWithPermutation)o;
                return Arrays.Equals(permutation, that.permutation);
            }

            public int GetHashCode()
            {
                return Arrays.GetHashCode(permutation);
            }
        }

        public sealed class EliminationOrder : Comparator<DegreeVector>, Serializable
        {
            readonly Comparator<DegreeVector> baseOrder;
            readonly int variable;
            public EliminationOrder(Comparator<DegreeVector> baseOrder, int variable)
            {
                this.baseOrder = baseOrder;
                this.variable = variable;
            }

            public int Compare(DegreeVector o1, DegreeVector o2)
            {
                int c = Integer.Compare(o1.exponents[variable], o2.exponents[variable]);
                if (c != 0)
                    return c;
                return baseOrder.Compare(o1, o2);
            }
        }
    }
}