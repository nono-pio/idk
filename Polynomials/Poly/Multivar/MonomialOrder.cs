namespace Polynomials.Poly.Multivar;

public static class MonomialOrder
{


    public static readonly IComparer<DegreeVector> LEX = Lex.instance;

    private sealed class Lex : IComparer<DegreeVector>
    {
        public static readonly Lex instance = new Lex();

        private Lex()
        {
        }

        public int Compare(DegreeVector? a, DegreeVector? b)
        {
            if (a is null)
                return b is null ? 0 : -1;
            if (b is null)
                return 1;
            
            for (int i = 0; i < a.exponents.Length; ++i)
            {
                int c = a.exponents[i].CompareTo(b.exponents[i]);
                if (c != 0)
                    return c;
            }

            return 0;
        }
    }


    public static readonly IComparer<DegreeVector> GRLEX = Grlex.instance;

    private sealed class Grlex : IComparer<DegreeVector>
    {
        public static readonly Grlex instance = new Grlex();

        private Grlex()
        {
        }

        public int Compare(DegreeVector? a, DegreeVector? b)
        {
            if (a is null)
                return b is null ? 0 : -1;
            if (b is null)
                return 1;
            
            int c = a.totalDegree.CompareTo(b.totalDegree);
            return c != 0 ? c : LEX.Compare(a, b);
        }
    }


    public static readonly IComparer<DegreeVector> ALEX = Alex.instance;

    private sealed class Alex : IComparer<DegreeVector>
    {
        public static readonly Alex instance = new Alex();

        private Alex()
        {
        }

        public int Compare(DegreeVector? a, DegreeVector? b)
        {
            return LEX.Compare(b, a);
        }
    }


    public static readonly IComparer<DegreeVector> GREVLEX = Grevlex.instance;

    private sealed class Grevlex : IComparer<DegreeVector>
    {
        public static readonly Grevlex instance = new Grevlex();

        private Grevlex()
        {
        }

        public int Compare(DegreeVector? a, DegreeVector? b)
        {
            if (a is null)
                return b is null ? 0 : -1;
            if (b is null)
                return 1;
            
            int c = a.totalDegree.CompareTo(b.totalDegree);
            if (c != 0)
                return c;
            for (int i = a.exponents.Length - 1; i >= 0; --i)
            {
                c = b.exponents[i].CompareTo(a.exponents[i]);
                if (c != 0)
                    return c;
            }

            return 0;
        }
    }


    public static readonly IComparer<DegreeVector> DEFAULT = GREVLEX;

    public static IComparer<DegreeVector> Product(IComparer<DegreeVector>[] orderings, int[] nVariables)
    {
        return new ProductOrder(orderings, nVariables);
    }


    public static IComparer<DegreeVector> Product(IComparer<DegreeVector> a, int anVariables,
        IComparer<DegreeVector> b, int bnVariable)
    {
        return new ProductOrder([a, b], [anVariables, bnVariable]);
    }


    public static bool IsGradedOrder(IComparer<DegreeVector> monomialOrder)
    {
        return Equals(monomialOrder, GREVLEX) || Equals(monomialOrder, GRLEX) || monomialOrder is GrevLexWithPermutation;
    }

    sealed class ProductOrder : IComparer<DegreeVector>
    {
        readonly IComparer<DegreeVector>[] orderings;
        readonly int[] nVariables;

        public ProductOrder(IComparer<DegreeVector>[] orderings, int[] nVariables)
        {
            this.orderings = orderings;
            this.nVariables = nVariables;
        }

        public int Compare(DegreeVector? a, DegreeVector? b)
        {
            if (a is null)
                return b is null ? 0 : -1;
            if (b is null)
                return 1;
            
            int prev = 0;
            for (int i = 0; i < nVariables.Length; i++)
            {
                // for each block
                DegreeVector aBlock = a.DvRange(prev, prev + nVariables[i]),
                    bBlock = b.DvRange(prev, prev + nVariables[i]);
                int c = orderings[i].Compare(aBlock, bBlock);
                if (c != 0)
                    return c;
                prev += nVariables[i];
            }

            return 0;
        }

        // for each block
        public override bool Equals(object? o)
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
        public override int GetHashCode()
        {
            int result = orderings.GetHashCode();
            result = 31 * result + nVariables.GetHashCode();
            return result;
        }
    }

    public sealed class GrevLexWithPermutation : IComparer<DegreeVector>
    {
        readonly int[] permutation;

        GrevLexWithPermutation(int[] permutation)
        {
            this.permutation = permutation;
        }

        public int Compare(DegreeVector? a, DegreeVector? b)
        {
            if (a is null)
                return b is null ? 0 : -1;
            if (b is null)
                return 1;
            
            
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

        public override bool Equals(object? o)
        {
            if (this == o)
                return true;
            if (o == null || GetType() != o.GetType())
                return false;
            GrevLexWithPermutation that = (GrevLexWithPermutation)o;
            return permutation.SequenceEqual(that.permutation);
        }

        public override int GetHashCode()
        {
            return permutation.GetHashCode();
        }
    }

    public sealed class EliminationOrder : IComparer<DegreeVector>
    {
        readonly IComparer<DegreeVector> baseOrder;
        readonly int variable;

        public EliminationOrder(IComparer<DegreeVector> baseOrder, int variable)
        {
            this.baseOrder = baseOrder;
            this.variable = variable;
        }

        public int Compare(DegreeVector? o1, DegreeVector? o2)
        {
            if (o1 is null)
                return o2 is null ? 0 : -1;
            if (o2 is null)
                return 1;
            
            int c = o1.exponents[variable].CompareTo(o2.exponents[variable]);
            if (c != 0)
                return c;
            return baseOrder.Compare(o1, o2);
        }
    }
}