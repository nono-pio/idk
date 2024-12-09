

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
        private sealed class Lex : Comparator<DegreeVector>
        {
            private static readonly Lex instance = new Lex();
            private Lex()
            {
            }

            public int Compare(DegreeVector a, DegreeVector b)
            {
                for (int i = 0; i < a.exponents.Length; ++i)
                {
                    int c = a.exponents[i].CompareTo(b.exponents[i]);
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
        private sealed class Grlex : Comparator<DegreeVector>
        {
            public static readonly Grlex instance = new Grlex();
            private Grlex()
            {
            }

            public int Compare(DegreeVector a, DegreeVector b)
            {
                int c = a.totalDegree.CompareTo(b.totalDegree);
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
        private sealed class Alex : Comparator<DegreeVector>
        {
            public static readonly Alex instance = new Alex();
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
        private sealed class Grevlex : Comparator<DegreeVector>
        {
            public static readonly Grevlex instance = new Grevlex();
            private Grevlex()
            {
            }

            public int Compare(DegreeVector a, DegreeVector b)
            {
                int c = a.totalDegree.CompareTo(b.totalDegree);
                if (c != 0)
                    return c;
                for (int i = a.exponents.Length - 1; i >= 0; --i)
                {
                    c = b.exponents[i].CompareTo( a.exponents[i]);
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
            switch (@string.ToLower())
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

        sealed class ProductOrder : Comparator<DegreeVector>
        {
            readonly Comparator<DegreeVector>[] orderings;
            readonly int[] nVariables;

            public ProductOrder(Comparator<DegreeVector>[] orderings, int[] nVariables)
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
                if (!orderings.SequenceEqual(that.orderings))
                    return false;
                return nVariables.SequenceEqual(that.nVariables);
            }

            // for each block
            // Probably incorrect - comparing Object[] arrays with Arrays.equals
            public int GetHashCode()
            {
                int result = orderings.GetHashCode();
                result = 31 * result + nVariables.GetHashCode();
                return result;
            }
        }

        public sealed class GrevLexWithPermutation : Comparator<DegreeVector>
        {
            readonly int[] permutation;
            GrevLexWithPermutation(int[] permutation)
            {
                this.permutation = permutation;
            }

            public int Compare(DegreeVector a, DegreeVector b)
            {
                int c = a.totalDegree.CompareTo(b.totalDegree);
                if (c != 0)
                    return c;
                for (int i = a.exponents.Length - 1; i >= 0; --i)
                {
                    c = b.exponents[permutation[i]].CompareTo(a.exponents[permutation[i]]);
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
                return permutation.SequenceEqual(that.permutation);
            }

            public int GetHashCode()
            {
                return permutation.GetHashCode();
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
                int c = o1.exponents[variable].CompareTo(o2.exponents[variable]);
                if (c != 0)
                    return c;
                return baseOrder.Compare(o1, o2);
            }
        }
    }
}