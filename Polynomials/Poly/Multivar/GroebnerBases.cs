namespace Polynomials.Poly.Multivar;

using Cc.Redberry.Rings.ChineseRemainders;
using Cc.Redberry;
using Cc.Redberry.Rings.Bigint;
using Cc.Redberry.Rings.Linear;
using Cc.Redberry.Rings.Linear.LinearSolver;
using Cc.Redberry.Rings.Poly;
using Cc.Redberry.Rings.Poly.Multivar.MonomialOrder;
using Cc.Redberry.Rings.Poly.Univar;
using Cc.Redberry.Rings.Primes;
using Cc.Redberry.Rings.Util;
using Gnu.Trove.Impl;
using Gnu.Trove.List.Array;
using Gnu.Trove.Map.Hash;
using Gnu.Trove.Set.Hash;
using Java;
using Java.Util.Concurrent.Atomic;
using Java.Util.Function;
using Java.Util.Stream;
using Cc.Redberry.Rings;
using Cc.Redberry.Rings.Poly.Multivar;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Cc.Redberry.Rings.Poly.Multivar
{
    public sealed class GroebnerBases
    {
        private GroebnerBases()
        {
        }


        public static IList<Poly> GroebnerBasis<Term extends AMonomial<Term>, Poly
            extends AMultivariatePolynomial<Term, Poly>>(IList<Poly> generators, Comparator<DegreeVector> monomialOrder)
        {
            if (generators.IsEmpty())
                return Collections.EmptyList();
            if (monomialOrder is GrevLexWithPermutation)
            {
                GroebnerAlgorithm<Term, Poly> ga = (p, o, s) => GBResult.NotBuchberger(GroebnerBasis(p, o));
                return GroebnerBasisRegardingGrevLexWithPermutation(generators, ga,
                    (GrevLexWithPermutation)monomialOrder);
            }

            Poly factory = generators[0];
            if (factory.IsOverFiniteField())
                return GroebnerBasisInGF(generators, monomialOrder, null);
            if (factory.IsOverZ())
                return (IList<Poly>)GroebnerBasisInZ((IList)generators, monomialOrder, null, true);
            if (Util.IsOverRationals(factory) && ((MultivariatePolynomial)factory).ring.Equals(Rings.Q))
                return (IList<Poly>)GroebnerBasisInQ((IList)generators, monomialOrder, null, true);
            else
                return BuchbergerGB(generators, monomialOrder);
        }


        public static GBResult<Term, Poly> GroebnerBasisInGF<Term extends AMonomial<Term>, Poly
            extends AMultivariatePolynomial<Term, Poly>>(IList<Poly> generators, Comparator<DegreeVector> monomialOrder,
            HilbertSeries hilbertSeries)
        {
            Poly factory = generators[0];
            if (!factory.IsOverFiniteField())
                throw new ArgumentException("not over finite field");
            if (CanConvertToZp64(factory))
            {
                GBResult<MonomialZp64, MultivariatePolynomialZp64> r = GroebnerBasisInGF(AsOverZp64(generators),
                    monomialOrder, hilbertSeries);
                return new GBResult<Term, Poly>(ConvertFromZp64(r), r);
            }
            else if (IsGradedOrder(monomialOrder))
                return F4GB(generators, monomialOrder, hilbertSeries);
            else if (hilbertSeries != null)
                return BuchbergerGB(generators, monomialOrder, hilbertSeries);
            else if (IsHomogeneousIdeal(generators) || IsHomogenizationCompatibleOrder(monomialOrder))
                return HilbertGB(generators, monomialOrder);
            else
                return BuchbergerGB(generators, monomialOrder);
        }

        // flag that we may be will switch off in the future
        private static readonly bool USE_MODULAR_ALGORITHM = true;

        // maximal number of variables for modular algorithm used by default
        private static readonly int MODULAR_ALGORITHM_MAX_N_VARIABLES = 3;


        public static GBResult<Monomial<BigInteger>, MultivariatePolynomial<BigInteger>> GroebnerBasisInZ(
            IList<MultivariatePolynomial<BigInteger>> generators, Comparator<DegreeVector> monomialOrder,
            HilbertSeries hilbertSeries, bool tryModular)
        {
            MultivariatePolynomial<BigInteger> factory = generators[0];
            if (!factory.IsOverZ())
                throw new ArgumentException();
            if (USE_MODULAR_ALGORITHM && (IsGradedOrder(monomialOrder) || IsHomogeneousIdeal(generators)) &&
                (tryModular && factory.nVariables <= MODULAR_ALGORITHM_MAX_N_VARIABLES))
                return ModularGB(generators, monomialOrder, hilbertSeries);
            if (IsGradedOrder(monomialOrder))
                return F4GB(generators, monomialOrder, hilbertSeries);
            else if (hilbertSeries != null)
                return BuchbergerGB(generators, monomialOrder, hilbertSeries);
            else if (IsHomogeneousIdeal(generators) || IsHomogenizationCompatibleOrder(monomialOrder))
                return HilbertGB(generators, monomialOrder);
            else
                return BuchbergerGB(generators, monomialOrder);
        }


        public static IList<MultivariatePolynomial<Rational<BigInteger>>> GroebnerBasisInQ(
            IList<MultivariatePolynomial<Rational<BigInteger>>> generators, Comparator<DegreeVector> monomialOrder,
            HilbertSeries hilbertSeries, bool tryModular)
        {
            return FracGB(generators, monomialOrder, hilbertSeries, (p, o, s) => GroebnerBasisInZ(p, o, s, tryModular));
        }


        public static IList<Poly> ConvertBasis<Term extends AMonomial<Term>, Poly
            extends AMultivariatePolynomial<Term, Poly>>(IList<Poly> generators, Comparator<DegreeVector> desiredOrder)
        {
            return HilbertConvertBasis(generators, desiredOrder);
        }

        /* **************************************** Common methods ************************************************ */


        static void SetMonomialOrder<Poly extends AMultivariatePolynomial<?, Poly>>(IList<Poly> list,
            Comparator<DegreeVector> order)
        {
            for (int i = 0; i < list.Count; i++)
            {
                Poly p = list[i];
                if (!order.Equals(p.ordering))
                    list[i] = p.SetOrdering(order);
            }
        }


        static IList<Poly> Canonicalize<Poly extends AMultivariatePolynomial<?, Poly>>(IList<Poly> list)
        {
            if (Util.IsOverRationals(list[0]))
                CanonicalizeFrac((IList)list);
            else
                list.ForEach(Poly.Canonical());
            list.Sort(Comparable.CompareTo());
            return list;
        }

        static void CanonicalizeFrac<E>(IList<MultivariatePolynomial<Rational<E>>> list)
        {
            Ring<E> fRing = list[0].ring.GetOne().ring;
            list.ReplaceAll((p) =>
                Util.ToCommonDenominator(p)._1.Canonical().MapCoefficients(p.ring, (c) => new Rational(fRing, c)));
        }


        private static IList<Poly> PrepareGenerators<Term extends AMonomial<Term>, Poly
            extends AMultivariatePolynomial<Term, Poly>>(IList<Poly> generators, Comparator<DegreeVector> monomialOrder)
        {
            // clone generators & set monomial order
            generators = generators.Stream().Map((p) => p.SetOrdering(monomialOrder)).Map(Poly.Canonical())
                .Collect(Collectors.ToList());
            Poly factory = generators[0];
            if (factory.nVariables == 1)

                // univariate case
                return Canonicalize(Collections.SingletonList(MultivariateGCD.PolynomialGCD(generators)));

            // remove zeroes
            generators.RemoveIf(Poly.IsZero());
            if (generators.IsEmpty())
            {
                // empty ideal
                generators.Add(factory.CreateZero());
                return generators;
            }


            // remove redundant elements from the basis
            RemoveRedundant(generators);

            // remove zeroes (may occur after reduction)
            generators.RemoveIf(Poly.IsZero());
            if (generators.IsEmpty())
            {
                // empty ideal
                generators.Add(factory.CreateZero());
                return generators;
            }

            if (generators.Stream().AnyMatch(Poly.IsConstant()))

                // contains non zero constant => ideal == whole ring
                return Collections.SingletonList(factory.CreateOne());
            return generators;
        }


        public static void MinimizeGroebnerBases<Term extends AMonomial<Term>, Poly
            extends AMultivariatePolynomial<Term, Poly>>(IList<Poly> basis)
        {
            outer:
            for (int i = basis.size() - 1; i >= 1; --i)
            {
                for (int j = i - 1; j >= 0; --j)
                {
                    Poly pi = basis[i], pj = basis[j];
                    if (pi.Lt().DvDivisibleBy(pj.Lt()))
                    {
                        basis.Remove(i);
                        continue;
                    }

                    if (pj.Lt().DvDivisibleBy(pi.Lt()))
                    {
                        basis.Remove(j);
                        --i;
                        continue;
                    }
                }
            }
        }


        public static void RemoveRedundant<Term extends AMonomial<Term>, Poly
            extends AMultivariatePolynomial<Term, Poly>>(IList<Poly> basis)
        {
            for (int i = 0, size = basis.size(); i < size; ++i)
            {
                Poly el = basis.Remove(i);
                Poly r = MultivariateDivision.PseudoRemainder(el, basis);
                if (r.IsZero())
                {
                    --i;
                    --size;
                }
                else
                    basis.Add(i, r);
            }
        }


        public class SyzygyPair<Term, Poly>
        {
            readonly int i, j;


            readonly Poly fi, fj;


            readonly DegreeVector syzygyGamma;


            readonly int sugar;


            SyzygyPair(int i, int j, Poly fi, Poly fj)
            {
                if (i > j)
                {
                    int s = i;
                    i = j;
                    j = s;
                    Poly fs = fi;
                    fi = fj;
                    fj = fs;
                }

                this.i = i;
                this.j = j;
                this.fi = fi;
                this.fj = fj;
                this.syzygyGamma = Lcm(fi.Lt(), fj.Lt());
                this.sugar = Math.Max(fi.DegreeSum() - fi.Lt().totalDegree, fj.DegreeSum() - fj.Lt().totalDegree) +
                             syzygyGamma.totalDegree;
            }


            SyzygyPair(int i, int j, IList<Poly> generators) : this(i, j, generators[i], generators[j])
            {
            }


            int Degree()
            {
                return syzygyGamma.totalDegree;
            }


            int Sugar()
            {
                return sugar;
            }
        }


        public static Poly Syzygy<Term extends AMonomial<Term>, Poly extends AMultivariatePolynomial<Term, Poly>>(Poly a
            , Poly b)
        {
            return Syzygy(new DegreeVector(Lcm(a.Multidegree(), b.Multidegree())), a, b);
        }


        public static Poly Syzygy<Term extends AMonomial<Term>, Poly extends AMultivariatePolynomial<Term, Poly>>(
            SyzygyPair<Term, Poly> sPair)
        {
            return Syzygy(sPair.syzygyGamma, sPair.fi, sPair.fj);
        }


        static Poly Syzygy<Term extends AMonomial<Term>, Poly extends AMultivariatePolynomial<Term, Poly>>(DegreeVector
            dvlcm, Poly a, Poly b)
        {
            IMonomialAlgebra<Term> mAlgebra = a.monomialAlgebra;
            Term lcm;
            if (a.IsOverField())
                lcm = mAlgebra.Create(dvlcm);
            else
                lcm = (Term)new Monomial(dvlcm,
                    ((MultivariatePolynomial)a).ring.Lcm(((MultivariatePolynomial)a).Lc(),
                        ((MultivariatePolynomial)b).Lc()));
            Poly aReduced = a.Clone().Multiply(mAlgebra.DivideExact(lcm, a.Lt())),
                bReduced = b.Clone().Multiply(mAlgebra.DivideExact(lcm, b.Lt())),
                syzygy = aReduced.Subtract(bReduced);
            return syzygy;
        }


        interface SyzygySet<Term, Poly>
        {
            void Add(SyzygyPair<Term, Poly> sPair);


            void Remove(SyzygyPair<Term, Poly> sPair);


            bool IsEmpty();


            int Size()
            {
                return AllSets().Stream().MapToInt(TreeSet.Size()).Sum();
            }


            Collection<SyzygyPair<Term, Poly>> GetAndRemoveNextBunch();


            Collection<TreeSet<SyzygyPair<Term, Poly>>> AllSets();


            IList<SyzygyPair<Term, Poly>> AllPairs()
            {
                return AllSets().Stream().FlatMap(Collection.Stream()).Collect(Collectors.ToList());
            }
        }


        sealed class SyzygyTreeSet<Term, Poly> : SyzygySet<Term, Poly>
        {
            readonly TreeSet<SyzygyPair<Term, Poly>> sPairs;

            SyzygyTreeSet(TreeSet<SyzygyPair<Term, Poly>> sPairs)
            {
                this.sPairs = sPairs;
            }

            public void Add(SyzygyPair<Term, Poly> sPair)
            {
                sPairs.Add(sPair);
            }

            public void Remove(SyzygyPair<Term, Poly> sPair)
            {
                sPairs.Remove(sPair);
            }

            public bool IsEmpty()
            {
                return sPairs.IsEmpty();
            }


            public Collection<SyzygyPair<Term, Poly>> GetAndRemoveNextBunch()
            {
                return Collections.Singleton(sPairs.PollFirst());
            }


            public Collection<TreeSet<SyzygyPair<Term, Poly>>> AllSets()
            {
                return new List(Collections.Singleton(sPairs));
            }
        }


        sealed class GradedSyzygyTreeSet<Term, Poly> : SyzygySet<Term, Poly>
        {
            readonly TreeMap<int, TreeSet<SyzygyPair<Term, Poly>>> sPairs;
            readonly Comparator<SyzygyPair> selectionStrategy;
            readonly ToIntFunction<SyzygyPair<Term, Poly>> weightFunction;

            GradedSyzygyTreeSet(TreeMap<int, TreeSet<SyzygyPair<Term, Poly>>> sPairs,
                Comparator<SyzygyPair> selectionStrategy, ToIntFunction<SyzygyPair<Term, Poly>> weightFunction)
            {
                this.sPairs = sPairs;
                this.selectionStrategy = selectionStrategy;
                this.weightFunction = weightFunction;
            }

            public void Add(SyzygyPair<Term, Poly> sPair)
            {
                sPairs.ComputeIfAbsent(weightFunction.ApplyAsInt(sPair), (__) => new TreeSet(selectionStrategy))
                    .Add(sPair);
            }

            public void Remove(SyzygyPair<Term, Poly> sPair)
            {
                int weight = weightFunction.ApplyAsInt(sPair);
                TreeSet<SyzygyPair<Term, Poly>> set = sPairs[weight];
                if (set != null)
                {
                    set.Remove(sPair);
                    if (set.IsEmpty())
                        sPairs.Remove(weight);
                }
            }

            public bool IsEmpty()
            {
                return sPairs.IsEmpty();
            }


            public Collection<SyzygyPair<Term, Poly>> GetAndRemoveNextBunch()
            {
                return sPairs.PollFirstEntry().GetValue();
            }


            public Collection<TreeSet<SyzygyPair<Term, Poly>>> AllSets()
            {
                return sPairs.Values();
            }
        }


        private static void UpdateBasis<Term extends AMonomial<Term>, Poly extends MonomialSetView<Term>>(IList<Poly>
            basis, SyzygySet<Term, Poly> sPairs, Poly newElement)
        {
            // array of lcm( lt(fi), lt(newElement) ) <- cache once for performance
            int[,] lcm = new int[basis.Count];
            Term newLeadTerm = newElement.Lt();
            for (int i = 0; i < basis.Count; i++)
            {
                Poly fi = basis[i];
                if (fi == null)
                    continue;
                lcm[i] = Lcm(fi.Lt().exponents, newLeadTerm.exponents);
            }


            // first indices of new critical pairs to add
            TIntArrayList pairsToAdd = new TIntArrayList();

            // find new critical pairs that should be definitely added
            filter:
            for (int iIndex = 0; iIndex < basis.Count; ++iIndex)
            {
                Poly fi = basis[iIndex];
                if (fi == null)
                    continue;
                if (!ShareVariablesQ(fi.Lt(), newLeadTerm))
                {
                    pairsToAdd.Add(iIndex); // add disjoint elements (will be removed in the next step)
                    continue;
                }


                // ruling out redundant Buchberger triplets: 1st pass
                for (int i = 0; i < pairsToAdd.Count; i++)
                {
                    int jIndex = pairsToAdd[i];
                    Poly fj = basis[jIndex];
                    if (DividesQ(lcm[jIndex], lcm[iIndex]))
                        continue;
                }


                // ruling out redundant Buchberger triplets: 2st pass
                for (int jIndex = iIndex + 1; jIndex < basis.Count; ++jIndex)
                {
                    Poly fj = basis[jIndex];
                    if (fj == null)
                        continue;
                    if (DividesQ(lcm[jIndex], lcm[iIndex]))
                        continue;
                }


                // no any redundant Buchberger triplets found -> add new critical pair
                pairsToAdd.Add(iIndex);
            }


            // now rule out disjoint elements
            for (int i = pairsToAdd.size() - 1; i >= 0; --i)
                if (!ShareVariablesQ(basis[pairsToAdd[i]].Lt(), newLeadTerm))
                    pairsToAdd.RemoveAt(i);

            // ruling out redundant Buchberger triplets from the old set of critical pairs
            IEnumerator<TreeSet<SyzygyPair<Term, Poly>>> it = sPairs.AllSets().Iterator();
            while (it.HasNext())
            {
                TreeSet<SyzygyPair<Term, Poly>> c = it.Next();

                // remove redundant critical pairs
                c.RemoveIf((sPair) => DividesQ(newLeadTerm, sPair.syzygyGamma) &&
                                      !Arrays.Equals(
                                          sPair.fi == basis[sPair.i]
                                              ? lcm[sPair.i]
                                              : Lcm(sPair.fi.Lt().exponents, newLeadTerm.exponents),
                                          sPair.syzygyGamma.exponents) &&
                                      !Arrays.Equals(
                                          sPair.fj == basis[sPair.j]
                                              ? lcm[sPair.j]
                                              : Lcm(sPair.fj.Lt().exponents, newLeadTerm.exponents),
                                          sPair.syzygyGamma.exponents));
                if (c.IsEmpty())
                    it.Remove();
            }


            // now add new element to the basis
            int oldSize = basis.Count;
            basis.Add(newElement);

            // update set of critical pairs with pairsToAdd
            for (int i = 0; i < pairsToAdd.Count; ++i)
            {
                int iIndex = pairsToAdd[i];
                sPairs.Add(new SyzygyPair(iIndex, oldSize, basis));
            } //        The following simplification is removed, since it is valid only when lead reduction is used
            //        (we use full reduction everywhere)
            //
            //        // remove old basis elements that are now redundant
            //        // note: do that only after update of critical pair set
            //        // note: this is compatible only with LeadReduce function,
            //        // in other cases one should keep track of all reducers
            //        // so this will only help to eliminate Buchberger triplets faster
            //        for (int iIndex = 0; iIndex < oldSize; ++iIndex) {
            //            Poly fi = basis.get(iIndex);
            //            if (fi == null)
            //                continue;
            //            if (dividesQ(newLeadTerm, fi.lt()))
            //                basis.set(iIndex, null);
            //        }
        }


        public static bool IsGroebnerBasis<Term extends AMonomial<Term>, Poly
            extends AMultivariatePolynomial<Term, Poly>>(IList<Poly> ideal, IList<Poly> generators,
            Comparator<DegreeVector> monomialOrder)
        {
            // form set of syzygies to check
            SyzygySet<Term, Poly> sPairs = new SyzygyTreeSet(new TreeSet(DefaultSelectionStrategy(monomialOrder)));
            IList<Poly> tmp = new List();
            ideal.ForEach((g) => UpdateBasis(tmp, sPairs, g));
            Poly factory = ideal[0];
            Poly[] gb = generators.ToArray(factory.CreateArray(generators.Count));
            return sPairs.AllSets().Stream().FlatMap(Collection.Stream())
                .AllMatch((p) => MultivariateDivision.PseudoRemainder(Syzygy(p), gb).IsZero());
        }


        public static Comparator<SyzygyPair> NormalSelectionStrategy(Comparator<DegreeVector> monomialOrder)
        {
            return (sa, sb) =>
            {
                int c = monomialOrder.Compare(sa.syzygyGamma, sb.syzygyGamma);
                if (c != 0)
                    return c;
                c = Integer.Compare(sa.j, sb.j);
                if (c != 0)
                    return c;
                return Integer.Compare(sa.i, sb.i);
            };
        }


        public static Comparator<SyzygyPair> WithSugar(Comparator<SyzygyPair> initial)
        {
            return (sa, sb) =>
            {
                int c = Integer.Compare(sa.sugar, sb.sugar);
                if (c != 0)
                    return c;
                return initial.Compare(sa, sb);
            };
        }


        public static Comparator<SyzygyPair> DefaultSelectionStrategy(Comparator<DegreeVector> monomialOrder)
        {
            Comparator<SyzygyPair> selectionStrategy = NormalSelectionStrategy(monomialOrder);

            // fixme use sugar always?
            if (!IsGradedOrder(monomialOrder))

                // add sugar for non-graded orders
                selectionStrategy = WithSugar(selectionStrategy);
            return selectionStrategy;
        }


        private sealed class EcartComparator<Poly> : Comparator<Poly>
        {
            public int Compare(Poly a, Poly b)
            {
                int c = Integer.Compare(a.Ecart(), b.Ecart());
                if (c != 0)
                    return c;
                return a.ordering.Compare(a.Lt(), b.Lt());
            }
        }


        static bool IsHomogenizationCompatibleOrder(Comparator<DegreeVector> monomialOrder)
        {
            return IsGradedOrder(monomialOrder) || monomialOrder == MonomialOrder.LEX;
        }


        static bool IsEasyOrder(Comparator<DegreeVector> monomialOrder)
        {
            return IsGradedOrder(monomialOrder);
        }


        static DegreeVector Lcm(DegreeVector a, DegreeVector b)
        {
            return new DegreeVector(Lcm(a.exponents, b.exponents));
        }


        static int[] Lcm(int[] a, int[] b)
        {
            return ArraysUtil.Max(a, b);
        }


        private static bool DividesQ(int[] divider, int[] dividend)
        {
            for (int i = 0; i < dividend.Length; i++)
                if (dividend[i] < divider[i])
                    return false;
            return true;
        }


        private static bool DividesQ(DegreeVector divider, DegreeVector dividend)
        {
            return dividend.DvDivisibleBy(divider);
        }


        static bool ShareVariablesQ(DegreeVector a, DegreeVector b)
        {
            for (int i = 0; i < a.exponents.Length; i++)
                if (a.exponents[i] != 0 && b.exponents[i] != 0)
                    return true;
            return false;
        }

        static IList<Poly> Homogenize<Term extends AMonomial<Term>, Poly extends AMultivariatePolynomial<Term, Poly>>(
            IList<Poly> ideal)
        {
            return ideal.Stream().Map((p) => p.Homogenize(p.nVariables)).Collect(Collectors.ToList());
        }

        static IList<Poly> Dehomogenize<Term extends AMonomial<Term>, Poly extends AMultivariatePolynomial<Term, Poly>>(
            IList<Poly> ideal)
        {
            return ideal.Stream().Map((p) => p.DropVariable(p.nVariables - 1)).Collect(Collectors.ToList());
        }

        static IList<MultivariatePolynomial<E>> ToIntegral<E>(IList<MultivariatePolynomial<Rational<E>>> ideal)
        {
            return ideal.Stream().Map((p) => Util.ToCommonDenominator(p)._1).Collect(Collectors.ToList());
        }

        static IList<MultivariatePolynomial<Rational<E>>> ToFractions<E>(IList<MultivariatePolynomial<E>> ideal)
        {
            Ring<Rational<E>> ring = Frac(ideal[0].ring);
            return ideal.Stream().Map((p) => p.MapCoefficients(ring, (c) => new Rational(p.ring, c)))
                .Collect(Collectors.ToList());
        }


        static GBResult<Monomial<Rational<E>>, MultivariatePolynomial<Rational<E>>> FracGB<E>(
            IList<MultivariatePolynomial<Rational<E>>> ideal, Comparator<DegreeVector> monomialOrder, HilbertSeries hps,
            GroebnerAlgorithm<Monomial<E>, MultivariatePolynomial<E>> algorithm)
        {
            return FracGB(algorithm).GroebnerBasis(ideal, monomialOrder, hps);
        }


        static GroebnerAlgorithm<Monomial<Rational<E>>, MultivariatePolynomial<Rational<E>>> FracGB<E>(
            GroebnerAlgorithm<Monomial<E>, MultivariatePolynomial<E>> algorithm)
        {
            return (gens, ord, hilb) =>
            {
                GBResult<Monomial<E>, MultivariatePolynomial<E>> r =
                    algorithm.GroebnerBasis(ToIntegral(gens), ord, hilb);
                return new GBResult(ToFractions(r), r);
            };
        }


        interface GroebnerAlgorithm<Term, Poly>
        {
            GBResult<Term, Poly> GroebnerBasis(IList<Poly> ideal, Comparator<DegreeVector> monomialOrder,
                HilbertSeries hps);
        }


        sealed class GBResult<Term, Poly> : ListWrapper<Poly>
        {
            readonly int nProcessedPolynomials;


            readonly int nZeroReductions;


            readonly int nHilbertRemoved;


            GBResult(IList<Poly> list, int nProcessedPolynomials, int nZeroReductions, int nHilbertRemoved) : base(list)
            {
                this.nProcessedPolynomials = nProcessedPolynomials;
                this.nZeroReductions = nZeroReductions;
                this.nHilbertRemoved = nHilbertRemoved;
            }


            GBResult(IList<Poly> list, GBResult r) : base(list)
            {
                this.nProcessedPolynomials = r.nProcessedPolynomials;
                this.nZeroReductions = r.nZeroReductions;
                this.nHilbertRemoved = r.nHilbertRemoved;
            }


            bool IsBuchbergerType()
            {
                return nProcessedPolynomials != -1;
            }


            static GBResult<T, P> NotBuchberger<T extends AMonomial<T>, P extends AMultivariatePolynomial<T, P>>(
                IList<P> basis)
            {
                return new GBResult(basis, -1, -1, -1);
            }


            static GBResult<T, P> Trivial<T extends AMonomial<T>, P extends AMultivariatePolynomial<T, P>>(IList<P>
                basis)
            {
                return new GBResult(basis, 0, 0, 0);
            }
        }

        /* ************************************** Buchberger algorithm ********************************************** */


        public static GBResult<Term, Poly> BuchbergerGB<Term extends AMonomial<Term>, Poly
            extends AMultivariatePolynomial<Term, Poly>>(IList<Poly> generators, Comparator<DegreeVector> monomialOrder)
        {
            return BuchbergerGB(generators, monomialOrder, (HilbertSeries)null);
        }


        public static GBResult<Term, Poly> BuchbergerGB<Term extends AMonomial<Term>, Poly
            extends AMultivariatePolynomial<Term, Poly>>(IList<Poly> generators, Comparator<DegreeVector> monomialOrder,
            Comparator<SyzygyPair> strategy)
        {
            return BuchbergerGB(generators, monomialOrder, () => new SyzygyTreeSet(new TreeSet(strategy)), null);
        }


        static GBResult<Term, Poly> BuchbergerGB<Term extends AMonomial<Term>, Poly
            extends AMultivariatePolynomial<Term, Poly>>(IList<Poly> generators, Comparator<DegreeVector> monomialOrder,
            HilbertSeries hilbertSeries)
        {
            if (Util.IsOverRationals(generators[0]))
                return (GBResult<Term, Poly>)FracGB((IList)generators, monomialOrder, hilbertSeries,
                    GroebnerBases.BuchbergerGB());
            Comparator<SyzygyPair> strategy = DefaultSelectionStrategy(monomialOrder);
            return BuchbergerGB(generators, monomialOrder,
                hilbertSeries == null
                    ? () => new SyzygyTreeSet(new TreeSet(strategy))
                    : () => new GradedSyzygyTreeSet(new TreeMap(), strategy, SyzygyPair.Degree()), hilbertSeries);
        }


        static GBResult<Term, Poly> BuchbergerGB<Term extends AMonomial<Term>, Poly
            extends AMultivariatePolynomial<Term, Poly>>(IList<Poly> generators, Comparator<DegreeVector> monomialOrder,
            Supplier<SyzygySet<Term, Poly>> syzygySetSupplier, HilbertSeries hilbertSeries)
        {
            return BuchbergerGB(generators, monomialOrder, NO_MINIMIZATION, syzygySetSupplier, hilbertSeries);
        }


        static GBResult<Term, Poly> BuchbergerGB<Term extends AMonomial<Term>, Poly
            extends AMultivariatePolynomial<Term, Poly>>(IList<Poly> generators, Comparator<DegreeVector> monomialOrder,
            MinimizationStrategy minimizationStrategy, Supplier<SyzygySet<Term, Poly>> syzygySetSupplier, HilbertSeries
            hilbertSeries)
        {
            // simplify generators as much as possible
            generators = PrepareGenerators(generators, monomialOrder);
            if (generators.Count == 1)
                return GBResult.Trivial(Canonicalize(generators));
            Poly factory = generators[0];

            // sort polynomials in the basis to achieve faster divisions
            Comparator<Poly> polyOrder = IsGradedOrder(monomialOrder)
                ? (a, b) => monomialOrder.Compare(a.Lt(), b.Lt())
                : new EcartComparator();
            generators.Sort(polyOrder);

            // set of syzygies
            SyzygySet<Term, Poly> sPairs = syzygySetSupplier.Get();

            // Groebner basis that will be computed
            IList<Poly> groebner = new List();

            // update Groebner basis with initial generators
            generators.ForEach((g) => UpdateBasis(groebner, sPairs, g));

            // cache array used in divisions (little performance improvement actually)
            Poly[] reducersArray = groebner.Stream().Filter(Objects.NonNull()).ToArray(factory.CreateArray());

            // syzygies degree to pickup for Hilbert-driven algorithm (used only when HPS is supplied)
            int hilbertDegree = 0;

            // number of linearly independent basis elements needed in fixed degree (used only when HPS is supplied)
            int hilbertDelta = Integer.MAX_VALUE;

            // number of pairs removed with Hilbert criteria)
            int nHilbertRemoved = 0;

            // cache size of basis after each minimization
            int sizeAfterMinimization = groebner.Count;

            // number of processed polynomials
            int nProcessedSyzygies = 0;

            // number of zero-reducible syzygies
            int nRedundantSyzygies = 0;
            while (!sPairs.IsEmpty())
            {
                // pick up (and remove) a bunch of critical pairs
                Collection<SyzygyPair<Term, Poly>> subset = hilbertSeries == null
                    ? sPairs.GetAndRemoveNextBunch()
                    : ((GradedSyzygyTreeSet<Term, Poly>)sPairs).sPairs.Remove(hilbertDegree);
                if (subset != null)
                    foreach (SyzygyPair<Term, Poly> pair in subset)
                    {
                        if (hilbertDelta == 0)
                            break;

                        // compute actual syzygy
                        Poly syzygy = MultivariateDivision.PseudoRemainder(Syzygy(pair), reducersArray);
                        ++nProcessedSyzygies;
                        if (syzygy.IsZero())
                        {
                            ++nRedundantSyzygies;
                            continue;
                        }

                        if (syzygy.IsConstant())

                            // ideal = ring
                            return new GBResult(Collections.SingletonList(factory.CreateOne()), 2 * nProcessedSyzygies,
                                2 * nRedundantSyzygies, nHilbertRemoved);

                        // add syzygy to basis
                        UpdateBasis(groebner, sPairs, syzygy);

                        // decrement current delta
                        if (hilbertSeries != null)
                            --hilbertDelta;

                        // recompute array
                        reducersArray = groebner.Stream().Filter(Objects.NonNull()).ToArray(factory.CreateArray());

                        // don't sort here, not practical actually
                        // Arrays.sort(groebnerArray, polyOrder);
                        if (minimizationStrategy.DoMinimize(sizeAfterMinimization, groebner.Count))
                        {
                            ReduceAndMinimizeGroebnerBases(groebner, sPairs, sizeAfterMinimization);
                            sizeAfterMinimization = groebner.Count;
                        }
                    }

                if (hilbertSeries != null && (subset != null || hilbertDegree == 0))
                {
                    //compute Hilbert series for LT(ideal) obtained so far
                    HilbertSeries currentHPS = HilbertSeriesOfLeadingTermsSet(groebner);
                    HilbertUpdate update =
                        UpdateWithHPS(hilbertSeries, currentHPS, (GradedSyzygyTreeSet<Term, Poly>)sPairs);
                    if (update.finished)

                        // we are done
                        break;
                    nHilbertRemoved += update.nRemovedPairs;
                    hilbertDegree = update.currentDegree;
                    hilbertDelta = update.hilbertDelta;
                }
            }


            // batch remove all nulls
            groebner.RemoveAll(Collections.Singleton(null));

            // minimize Groebner basis
            MinimizeGroebnerBases(groebner);

            // speed up final reduction
            groebner.Sort(polyOrder);

            // reduce Groebner basis
            RemoveRedundant(groebner);

            // canonicalize Groebner basis
            Canonicalize(groebner);
            return new GBResult(groebner, 2 * nProcessedSyzygies, 2 * nRedundantSyzygies, nHilbertRemoved);
        }


        private static HilbertUpdate UpdateWithHPS<Term extends AMonomial<Term>, Poly extends MonomialSetView<Term>>(
            HilbertSeries hilbertSeries, HilbertSeries currentHPS, GradedSyzygyTreeSet<Term, Poly> sPairs)
        {
            if (currentHPS.Equals(hilbertSeries))

                // we are done
                return new HilbertUpdate();

            // since Mon(current) ⊆ Mon(ideal) we can use only numerators of HPS
            int currentDegree = 0;
            for (;; ++currentDegree)
                if (!hilbertSeries.initialNumerator[currentDegree].Equals(currentHPS.initialNumerator[currentDegree]))
                    break;
            Rational<BigInteger> delta = currentHPS.initialNumerator[currentDegree]
                .Subtract(hilbertSeries.initialNumerator[currentDegree]);
            int hilbertDelta = delta.Numerator().IntValueExact();

            // remove redundant S-pairs
            int mDegree = currentDegree;
            int sizeBefore = sPairs.Count;
            sPairs.AllSets().ForEach((set) => set.RemoveIf((s) => s.Degree() < mDegree));
            return new HilbertUpdate(currentDegree, hilbertDelta, sizeBefore - sPairs.Count);
        }


        private sealed class HilbertUpdate
        {
            readonly int currentDegree;
            readonly int hilbertDelta;
            readonly int nRemovedPairs;
            readonly bool finished;

            HilbertUpdate()
            {
                this.finished = true;
                this.nRemovedPairs = -1;
                this.currentDegree = -1;
                this.hilbertDelta = -1;
            }

            HilbertUpdate(int currentDegree, int hilbertDelta, int nRemovedPairs)
            {
                this.currentDegree = currentDegree;
                this.hilbertDelta = hilbertDelta;
                this.nRemovedPairs = nRemovedPairs;
                this.finished = false;
            }
        }


        public interface MinimizationStrategy
        {
            bool DoMinimize(int previousSize, int currentSize);
        }


        public static MinimizationStrategy NO_MINIMIZATION = (prev, curr) => false;


        private static void ReduceAndMinimizeGroebnerBases<Term extends AMonomial<Term>, Poly
            extends AMultivariatePolynomial<Term, Poly>>(IList<Poly> generators, SyzygySet<Term, Poly> sPairs, int from)
        {
            for (int i = from; i < generators.Count; i++)
            {
                // this are newly added elements of Groebner basis
                Poly fi = generators[i];
                if (fi == null)
                    continue;
                for (int j = 0; j < i; ++j)
                {
                    // search for fj which is divisible by fi
                    Poly fj = generators[j];
                    if (fj == null)
                        continue;

                    // proceed only if new syzygy can reduce fk
                    if (!MultivariateDivision.NontrivialQuotientQ(fj, fi))
                        continue;
                    generators.Remove(j);
                    Poly reduced = Remainder0(fj, generators);
                    if (reduced.Equals(fj))
                        continue;
                    if (reduced.IsZero())
                        reduced = null;
                    generators.Add(j, reduced);
                    if (!fj.Equals(reduced))
                    {
                        if (reduced == null)
                        {
                            // remove all pairs with k
                            for (int l = 0; l < generators.Count; l++)
                                if (l != j && generators[l] != null)
                                    sPairs.Remove(new SyzygyPair(l, j, generators));
                        }
                        else

                            // update all pairs with k
                            for (int l = 0; l < generators.Count; l++)
                                if (l != j && generators[l] != null)
                                    sPairs.Add(new SyzygyPair(l, j, generators));
                    }
                }
            }
        }

        private static Poly Remainder0<Term extends AMonomial<Term>, Poly extends AMultivariatePolynomial<Term, Poly>>(
            Poly dividend, IList<Poly> dividers)
        {
            Poly[] dividersArr = dividers.Stream().Filter(Objects.NonNull()).ToArray(dividend.CreateArray());

            //Arrays.sort(dividersArr, (a, b) -> a.ordering.compare(a.lt(), b.lt()));
            return MultivariateDivision.PseudoRemainder(dividend, dividersArr);
        }


        public static GBResult<Term, Poly> HilbertGB<Term extends AMonomial<Term>, Poly
            extends AMultivariatePolynomial<Term, Poly>>(IList<Poly> generators, Comparator<DegreeVector> monomialOrder,
            HilbertSeries hilbertSeries)
        {
            if (hilbertSeries == null)
                throw new NullReferenceException();
            return BuchbergerGB(generators, monomialOrder, hilbertSeries);
        }


        public static GBResult<Term, Poly> HilbertConvertBasis<Term extends AMonomial<Term>, Poly
            extends AMultivariatePolynomial<Term, Poly>>(IList<Poly> groebnerBasis, Comparator<DegreeVector>
            desiredOrdering)
        {
            Comparator<DegreeVector> ordering = groebnerBasis[0].ordering;
            if (ordering == desiredOrdering)
                return GBResult.Trivial(new List(groebnerBasis));
            if (IsHomogeneousIdeal(groebnerBasis) || (IsGradedOrder(desiredOrdering) && IsGradedOrder(ordering)))
                return HilbertGB(groebnerBasis, desiredOrdering, HilbertSeriesOfLeadingTermsSet(groebnerBasis));
            else if (IsHomogenizationCompatibleOrder(desiredOrdering))

                // nothing to do
                return HilbertGB(groebnerBasis, desiredOrdering);
            else
                throw new Exception("Hilbert conversion is not supported for specified ordering: " + desiredOrdering);
        }


        public static GBResult<Term, Poly> HilbertGB<Term extends AMonomial<Term>, Poly
            extends AMultivariatePolynomial<Term, Poly>>(IList<Poly> generators, Comparator<DegreeVector> monomialOrder)
        {
            return HilbertGB(generators, monomialOrder, (p, o, s) => GBResult.NotBuchberger(GroebnerBasis(p, o)));
        }


        public static GBResult<Term, Poly> HilbertGB<Term extends AMonomial<Term>, Poly
            extends AMultivariatePolynomial<Term, Poly>>(IList<Poly> generators, Comparator<DegreeVector> monomialOrder,
            GroebnerAlgorithm<Term, Poly> baseAlgorithm)
        {
            if (IsEasyOrder(monomialOrder))
                return baseAlgorithm.GroebnerBasis(generators, monomialOrder, null);
            if (IsHomogeneousIdeal(generators))
                return HilbertConvertBasis(GroebnerBasisWithOptimizedGradedOrder(generators, baseAlgorithm),
                    monomialOrder);

            // we don't check whether we are in homogenization-compatible order
            GBResult<Term, Poly> r = HilbertGB(Homogenize(generators), monomialOrder, baseAlgorithm);
            IList<Poly> l = Dehomogenize(r);
            RemoveRedundant(l);
            Canonicalize(l);
            return new GBResult(l, r);
        }


        public static IList<Poly> GroebnerBasisWithOptimizedGradedOrder<Term extends AMonomial<Term>, Poly
            extends AMultivariatePolynomial<Term, Poly>>(IList<Poly> ideal)
        {
            return GroebnerBasisWithOptimizedGradedOrder(ideal,
                (l, o, h) => GBResult.NotBuchberger(GroebnerBasis(l, o)));
        }


        public static IList<Poly> GroebnerBasisWithOptimizedGradedOrder<Term extends AMonomial<Term>, Poly
            extends AMultivariatePolynomial<Term, Poly>>(IList<Poly> ideal, GroebnerAlgorithm<Term, Poly> baseAlgorithm)
        {
            Comparator<DegreeVector> ord = OptimalOrder(ideal);
            if (ord == GREVLEX)

                // all variables have the same degree
                return baseAlgorithm.GroebnerBasis(ideal, GREVLEX, null);

            // the ordering for which the Groebner basis will be obtained
            GrevLexWithPermutation order = (GrevLexWithPermutation)ord;
            return GroebnerBasisRegardingGrevLexWithPermutation(ideal, baseAlgorithm, order);
        }


        public static IList<Poly> GroebnerBasisRegardingGrevLexWithPermutation<Term extends AMonomial<Term>, Poly
            extends AMultivariatePolynomial<Term, Poly>>(IList<Poly> ideal, GroebnerAlgorithm<Term, Poly> baseAlgorithm,
            GrevLexWithPermutation order)
        {
            int[] inversePermutation = MultivariateGCD.InversePermutation(order.permutation);
            return baseAlgorithm
                .GroebnerBasis(
                    ideal.Stream().Map((p) => AMultivariatePolynomial.RenameVariables(p, order.permutation))
                        .Collect(Collectors.ToList()), GREVLEX, null).Stream()
                .Map((p) => AMultivariatePolynomial.RenameVariables(p, inversePermutation))
                .Map((p) => p.SetOrdering(order)).Collect(Collectors.ToList());
        }


        public static Comparator<DegreeVector> OptimalOrder<Term extends AMonomial<Term>, Poly
            extends AMultivariatePolynomial<Term, Poly>>(IList<Poly> ideal)
        {
            if (ideal.IsEmpty())
                return GREVLEX;
            Poly factory = ideal[0];
            int[] degrees = ideal.Stream().Map(AMultivariatePolynomial.Degrees())
                .Reduce(new int[factory.nVariables], ArraysUtil.Sum());
            if (Arrays.Stream(degrees).AllMatch((i) => i == degrees[0]))

                // all variables have the same degree
                return GREVLEX;
            int[] permutation = ArraysUtil.Sequence(0, factory.nVariables);
            ArraysUtil.QuickSort(degrees, permutation);

            // the ordering for which the Groebner basis will be obtained
            return new GrevLexWithPermutation(permutation);
        }

        /* ************************************************** F4 ******************************************************* */


        public static GBResult<Term, Poly> F4GB<Term extends AMonomial<Term>, Poly
            extends AMultivariatePolynomial<Term, Poly>>(IList<Poly> generators, Comparator<DegreeVector> monomialOrder)
        {
            return F4GB(generators, monomialOrder, null);
        }


        static GBResult<Term, Poly> F4GB<Term extends AMonomial<Term>, Poly
            extends AMultivariatePolynomial<Term, Poly>>(IList<Poly> generators, Comparator<DegreeVector> monomialOrder,
            HilbertSeries hilbertSeries)
        {
            if (!IsGradedOrder(monomialOrder))
                throw new NotSupportedException("F4 works only with graded orders");
            if (Util.IsOverRationals(generators[0]))
                return (GBResult<Term, Poly>)FracGB((IList)generators, monomialOrder, hilbertSeries,
                    GroebnerBases.F4GB());
            return F4GB(generators, monomialOrder,
                () => new GradedSyzygyTreeSet(new TreeMap(), DefaultSelectionStrategy(monomialOrder),
                    SyzygyPair.Degree()), hilbertSeries);
        }

        /*
         * In F4 algorithm we don't need tree-based structure for multivariate polynomials, since no any additions/divisions
         * performed (all operations are reduced to matrix operations). The only arithmetic operation used under the hood
         * of F4 is multiplication by monomial, which still preserves the order of terms. So, we switch here to array-based
         * (with sorted array) representation of polynomials which is much faster than tree-based.
         */


        private sealed class ArrayBasedPoly<Term> : MonomialSetView<Term>
        {
            readonly IMonomialAlgebra<Term> mAlgebra;


            readonly Term[] data;


            readonly int[] degrees;


            ArrayBasedPoly(AMultivariatePolynomial<Term, TWildcardTodo> poly)
            {
                // retrieve data array from poly in descending order
                this.mAlgebra = poly.monomialAlgebra;
                this.data = poly.terms.DescendingMap().Values().ToArray(poly.monomialAlgebra.CreateArray(poly.Count));
                this.degrees = poly.Degrees();
            }


            // retrieve data array from poly in descending order
            ArrayBasedPoly(IMonomialAlgebra<Term> mAlgebra, Term[] data, int[] degrees)
            {
                this.mAlgebra = mAlgebra;
                this.data = data;
                this.degrees = degrees;
            }


            // retrieve data array from poly in descending order
            ArrayBasedPoly(IMonomialAlgebra<Term> mAlgebra, Term[] data, int nVariables)
            {
                this.mAlgebra = mAlgebra;
                this.data = data;
                this.degrees = new int[nVariables];
                foreach (Term db in data)
                    for (int i = 0; i < nVariables; i++)
                        if (db.exponents[i] > degrees[i])
                            degrees[i] = db.exponents[i];
            }


            // retrieve data array from poly in descending order
            public IEnumerator<Term> AscendingIterator()
            {
                throw new NotSupportedException();
            }


            // retrieve data array from poly in descending order
            public IEnumerator<Term> DescendingIterator()
            {
                return Arrays.AsList(data).Iterator();
            }


            // retrieve data array from poly in descending order
            public IEnumerator<Term> Iterator()
            {
                return DescendingIterator();
            }


            // retrieve data array from poly in descending order
            public int[] Degrees()
            {
                return degrees;
            }


            // retrieve data array from poly in descending order
            public Term First()
            {
                return data[data.Length - 1];
            }


            // retrieve data array from poly in descending order
            public Term Last()
            {
                return data[0];
            }


            // retrieve data array from poly in descending order
            public int Size()
            {
                return data.Length;
            }


            // retrieve data array from poly in descending order
            Term Get(int i)
            {
                return data[i];
            }


            // retrieve data array from poly in descending order
            Term Find(Term dv, Comparator<DegreeVector> ordering)
            {
                int i = Arrays.BinarySearch(data, dv, (a, b) => ordering.Compare(b, a));
                if (i < 0)
                    return null;
                return data[i];
            }


            // retrieve data array from poly in descending order
            public Collection<Term> Collection()
            {
                return Arrays.AsList(data);
            }


            // retrieve data array from poly in descending order
            public ArrayBasedPoly<Term> Clone()
            {
                return new ArrayBasedPoly(mAlgebra, data.Clone(), degrees.Clone());
            }


            // retrieve data array from poly in descending order
            public string ToString()
            {
                return Arrays.ToString(data);
            }


            // retrieve data array from poly in descending order
            void Multiply(Term term)
            {
                for (int i = data.length - 1; i >= 0; --i)
                    data[i] = mAlgebra.Multiply(data[i], term);
                for (int i = 0; i < degrees.Length; ++i)
                    degrees[i] += term.exponents[i];
            }


            // retrieve data array from poly in descending order
            void Multiply(DegreeVector term)
            {
                for (int i = data.length - 1; i >= 0; --i)
                    data[i] = data[i].Multiply(term);
                for (int i = 0; i < degrees.Length; ++i)
                    degrees[i] += term.exponents[i];
            }


            // retrieve data array from poly in descending order
            void DivideExact(Term term)
            {
                for (int i = data.length - 1; i >= 0; --i)
                    data[i] = mAlgebra.DivideExact(data[i], term);
                for (int i = 0; i < degrees.Length; ++i)
                    degrees[i] -= term.exponents[i];
            }


            // retrieve data array from poly in descending order


            void Monic()
            {
                Term unit = mAlgebra.GetUnitTerm(Lt().exponents.Length);
                Multiply(mAlgebra.DivideExact(unit, Lt().SetDegreeVector(unit)));
            }


            // retrieve data array from poly in descending order


            void PrimitivePart()
            {
                if (mAlgebra is IMonomialAlgebra.MonomialAlgebra)
                {
                    Ring ring = ((IMonomialAlgebra.MonomialAlgebra)mAlgebra).ring;
                    object gcd = ring.Gcd(Arrays.Stream(data).Map((t) => ((Monomial)t).coefficient)
                        .Collect(Collectors.ToList()));
                    this.DivideExact((Term)new Monomial(data[0].exponents.Length, gcd));
                }
            }


            // retrieve data array from poly in descending order


            void Canonical()
            {
                if (mAlgebra is IMonomialAlgebra.MonomialAlgebra &&
                    !((IMonomialAlgebra.MonomialAlgebra)mAlgebra).ring.IsField())
                    PrimitivePart();
                else
                    Monic();
            }


            // retrieve data array from poly in descending order


            ArrayBasedPoly<Term> SetLeadCoeffFrom(Term term)
            {
                if (mAlgebra.HaveSameCoefficients(Lt(), term))
                    return this;
                ArrayBasedPoly<Term> poly = Clone();
                poly.Multiply(mAlgebra.DivideExact(term.ToZero(), poly.Lt().ToZero()));
                return poly;
            }
        }


        private static readonly int F4_MIN_SELECTION_SIZE = 0; // not used actually


        private static readonly int F4_OVER_FIELD_LINALG_THRESHOLD = 16, F4_OVER_EUCLID_LINALG_THRESHOLD = 6;


        static GBResult<Term, Poly> F4GB<Term extends AMonomial<Term>, Poly
            extends AMultivariatePolynomial<Term, Poly>>(IList<Poly> generators, Comparator<DegreeVector> monomialOrder,
            Supplier<GradedSyzygyTreeSet<Term, ArrayBasedPoly<Term>>> syzygySetSupplier, HilbertSeries hilbertSeries)
        {
            // simplify generators as much as possible
            generators = PrepareGenerators(generators, monomialOrder);
            if (generators.Count == 1)
                return GBResult.Trivial(Canonicalize(generators));
            Poly factory = generators[0];

            // sort polynomials in the basis to achieve in general faster inital reductions
            Comparator<Poly> polyOrder = IsGradedOrder(monomialOrder)
                ? (a, b) => monomialOrder.Compare(a.Lt(), b.Lt())
                : new EcartComparator();
            generators.Sort(polyOrder);

            // set of all syzygies
            GradedSyzygyTreeSet<Term, ArrayBasedPoly<Term>> sPairs = syzygySetSupplier.Get();

            // Groebner basis that will be computed
            IList<ArrayBasedPoly<Term>> groebner = new List();

            // a history of performed reductions: index of generator -> list of available reductions
            // this is extensively used in simplify routine; the data is updated each time when a new row-echelon
            // form is calculated; for details see Joux & Vitse, "A variant of F4 algorithm" (TabSimplify structure)
            IList<IList<ArrayBasedPoly<Term>>> f4reductions = new List();

            // update Groebner basis with initial generators
            foreach (Poly generator in generators)
            {
                ArrayBasedPoly<Term> g = new ArrayBasedPoly(generator);
                UpdateBasis(groebner, sPairs, g);
                f4reductions.Add(new List(Collections.SingletonList(g)));
            }


            // a list of reducers that will be used for a full  direct (without linear algebra) reduction of syzygies
            IList<Poly> reducers = new List(generators);

            // cache array used in divisions (little performance improvement actually)
            Poly[] reducersArray = reducers.Stream().Filter(Objects.NonNull()).ToArray(factory.CreateArray());

            // syzygies degree to pickup for Hilbert-driven algorithm (used only when HPS is supplied)
            int hilbertDegree = 0;

            // number of linearly independent basis elements needed in fixed degree (used only when HPS is supplied)
            int hilbertDelta = Integer.MAX_VALUE;

            // number of pairs removed with Hilbert criteria)
            int nHilbertRemoved = 0;

            // number of processed polynomials
            int nProcessedPolynomials = 0;

            // number of zero-reducible syzygies
            int nRedundantSyzygies = 0;
            main:
            while (!sPairs.IsEmpty())
            {
                // pick up and remove a bunch of pairs (select at least F4_MIN_SELECTION_SIZE pairs)
                Collection<SyzygyPair<Term, ArrayBasedPoly<Term>>> subset;
                if (hilbertSeries != null)
                    subset = sPairs.sPairs.Remove(hilbertDegree);
                else
                {
                    subset = sPairs.GetAndRemoveNextBunch();
                    while (!sPairs.IsEmpty() && subset.Count < F4_MIN_SELECTION_SIZE)
                        subset.AddAll(sPairs.GetAndRemoveNextBunch());
                }

                if (subset != null)
                {
                    if ((factory.IsOverField() && subset.Count <= F4_OVER_FIELD_LINALG_THRESHOLD) ||
                        (!factory.IsOverField() && subset.Count <= F4_OVER_EUCLID_LINALG_THRESHOLD))
                    {
                        // normal Buchberger case, don't use linear algebra, just reduce
                        foreach (SyzygyPair<Term, ArrayBasedPoly<Term>> sPair in subset)
                        {
                            ++nProcessedPolynomials;
                            if (hilbertDelta == 0)
                                break;

                            // compute actual syzygy
                            Poly syzygy = Syzygy(sPair.syzygyGamma, factory.Create(sPair.fi), factory.Create(sPair.fj));
                            syzygy = MultivariateDivision.PseudoRemainder(syzygy, reducersArray);
                            if (syzygy.IsZero())
                            {
                                ++nRedundantSyzygies;
                                continue;
                            }

                            if (syzygy.IsConstant())

                                // ideal = ring
                                return new GBResult(Collections.SingletonList(factory.CreateOne()),
                                    nProcessedPolynomials, nRedundantSyzygies, nHilbertRemoved);

                            // add syzygy to basis
                            ArrayBasedPoly<Term> _syzygy_ = new ArrayBasedPoly(syzygy);
                            UpdateBasis(groebner, sPairs, _syzygy_);

                            // decrement current delta
                            if (hilbertSeries != null)
                                --hilbertDelta;

                            // update F4 reductions
                            f4reductions.Add(new List(Collections.SingletonList(_syzygy_)));

                            // add syzygy to a list of reducers
                            reducers.Add(syzygy);

                            // recompute array
                            reducersArray = reducers.Stream().Filter(Objects.NonNull()).ToArray(factory.CreateArray());
                        }
                    }
                    else
                    {
                        // F4 case, use linear algebra
                        nProcessedPolynomials += subset.Count;

                        // the list of polynomials to reduce (H-polynomials)
                        IList<HPolynomial<Term>> hPolynomials = new List();

                        // all monomials that occur in H-polynomials
                        TreeSet<DegreeVector> hMonomials = new TreeSet(monomialOrder);

                        // monomials that were annihilated
                        TreeSet<DegreeVector> hAnnihilated = new TreeSet(monomialOrder);

                        // form initial set of H-polynomials from the current set of critical pairs
                        foreach (SyzygyPair<Term, ArrayBasedPoly<Term>> syzygy in subset)
                        {
                            // correction factors
                            DegreeVector fiQuot = syzygy.syzygyGamma.DvDivideExact(syzygy.fi.Lt()),
                                fjQuot = syzygy.syzygyGamma.DvDivideExact(syzygy.fj.Lt());

                            // a pair of H-polynomials
                            ArrayBasedPoly<Term> fiHPoly = Simplify(syzygy.fi, fiQuot, f4reductions[syzygy.i]),
                                fjHPoly = Simplify(syzygy.fj, fjQuot, f4reductions[syzygy.j]);

                            // add to set of H-polynomials
                            hPolynomials.Add(new HPolynomial(fiHPoly, syzygy.i));
                            hPolynomials.Add(new HPolynomial(fjHPoly, syzygy.j));

                            // store all monomials that we have in H
                            hMonomials.AddAll(fiHPoly.Collection());
                            hMonomials.AddAll(fjHPoly.Collection());

                            // lts will be annihilated
                            hAnnihilated.Add(fiHPoly.Lt());
                        }


                        // the diff = Mon(H-polynomials) / annihilated
                        TreeSet<DegreeVector> diff = new TreeSet(monomialOrder);
                        diff.AddAll(hMonomials);
                        diff.RemoveAll(hAnnihilated);

                        // compute a whole set of required H-polynomials for reductions
                        // ("Symbolic Preprocessing" routine from Faugère's original paper)
                        while (!diff.IsEmpty())
                        {
                            // pick the "highest" term from diff
                            DegreeVector dv = diff.PollLast();
                            hAnnihilated.Add(dv);

                            // select some polynomial from basis which can reduce the "highest" term (lt-divisor)
                            OptionalInt divisorOpt = IntStream.Range(0, groebner.Count)
                                .Filter((i) => groebner[i] != null).Filter((i) => dv.DvDivisibleBy(groebner[i].Lt()))
                                .FindAny();
                            if (divisorOpt.IsPresent())
                            {
                                // index in the basis
                                int iIndex = divisorOpt.GetAsInt();

                                // correction factor
                                DegreeVector quot = dv.DvDivideExact(groebner[iIndex].Lt());

                                // new H-polynomial to add
                                ArrayBasedPoly<Term> newH = Simplify(groebner[iIndex], quot, f4reductions[iIndex]);

                                // append newH to H-polynomials
                                hPolynomials.Add(new HPolynomial(newH, iIndex));

                                // update monomials set
                                TreeSet<DegreeVector> newMonomials = new TreeSet(monomialOrder);
                                newMonomials.AddAll(newH.Collection());
                                hMonomials.AddAll(newMonomials);

                                // update the diff
                                newMonomials.RemoveAll(hAnnihilated);
                                diff.AddAll(newMonomials);
                            }
                        }


                        // all monomials occurring in H sorted in descending order, i.e. lead term first
                        DegreeVector[] hMonomialsArray =
                            hMonomials.DescendingSet().ToArray(new DegreeVector[hMonomials.Count]);

                        // sort rows to make the initial matrix upper-right-triangular as max as possible
                        hPolynomials.Sort((a, b) =>
                        {
                            int c = monomialOrder.Compare(b.hPoly.Lt(), a.hPoly.Lt());
                            if (c != 0)
                                return c;
                            return Integer.Compare(a.hPoly.Count, b.hPoly.Count);
                        });

                        // reduce all H-polynomials with linear algebra and compute new basis elements (N+)
                        IList<ArrayBasedPoly<Term>> nPlus = ReduceMatrix(factory, groebner, hPolynomials,
                            hMonomialsArray, f4reductions);
                        nRedundantSyzygies += subset.Count - nPlus.Count;

                        // enlarge the basis
                        nPlus.ForEach((g) => UpdateBasis(groebner, sPairs, g));

                        // add reducers to a list of reducers
                        nPlus.ForEach((g) => reducers.Add(factory.Create(g)));
                        for (int i = 0; i < groebner.Count; ++i)
                            if (groebner[i] == null)
                                reducers[i] = null;

                        // recompute array
                        reducersArray = reducers.Stream().Filter(Objects.NonNull()).ToArray(factory.CreateArray());
                    }
                }

                if (hilbertSeries != null && (subset != null || hilbertDegree == 0))
                {
                    //compute Hilbert series for LT(ideal) obtained so far
                    HilbertSeries currentHPS =
                        HilbertSeries(groebner.Stream().Map(MonomialSetView.Lt()).Collect(Collectors.ToList()));
                    HilbertUpdate update = UpdateWithHPS(hilbertSeries, currentHPS, sPairs);
                    if (update.finished)

                        // we are done
                        break;
                    nHilbertRemoved += update.nRemovedPairs;
                    hilbertDegree = update.currentDegree;
                    hilbertDelta = update.hilbertDelta;
                }
            }


            // batch remove all nulls
            groebner.RemoveAll(Collections.Singleton(null));

            // convert from array-based polynomials to normal
            IList<Poly> result = groebner.Stream().Map(factory.Create()).Collect(Collectors.ToList());

            // minimize Groebner basis
            MinimizeGroebnerBases(result);

            // speed up final reduction
            result.Sort(polyOrder);

            // reduce Groebner basis
            RemoveRedundant(result);

            // canonicalize Groebner basis
            Canonicalize(result);
            return new GBResult(result, nProcessedPolynomials, nRedundantSyzygies, nHilbertRemoved);
        }


        sealed class HPolynomial<Term>
        {
            readonly ArrayBasedPoly<Term> hPoly;


            readonly int indexOfOrigin;


            HPolynomial(ArrayBasedPoly<Term> hPoly, int indexOfOrigin)
            {
                this.hPoly = hPoly;
                this.indexOfOrigin = indexOfOrigin;
            }
        }


        static ArrayBasedPoly<Term> Simplify<Term extends AMonomial<Term>>(ArrayBasedPoly<Term> generator,
            DegreeVector factor, IList<ArrayBasedPoly<Term>> reductions)
        {
            // the desired leading term of H-polynomial
            DegreeVector desiredLeadTerm = generator.Lt().DvMultiply(factor);

            // iterate from the last (most recently added, thus most simplified) to the first (the initial generator) reductions
            for (int i = reductions.size() - 1; i >= 0; --i)
            {
                ArrayBasedPoly<Term> reduction = reductions[i];

                // leading term of previously computed reduction
                Term rLeadTerm = reduction.Lt();
                if (rLeadTerm.DvEquals(desiredLeadTerm))

                    // we just have the same calculation in the map (no any appropriate reductions were calculated)
                    return reduction;
                if (desiredLeadTerm.DvDivisibleBy(rLeadTerm))
                {
                    // <- nontrivial appropriate reduction
                    DegreeVector quot = desiredLeadTerm.DvDivideExact(rLeadTerm);
                    ArrayBasedPoly<Term> g = reduction.Clone();
                    g.Multiply(quot);

                    // cache this reduction too
                    reductions.Add(g);
                    return g;
                }
            }


            // <- this point is unreachable since the initial generator is already added to the list (in the begining)
            throw new Exception();
        }


        static IList<ArrayBasedPoly<Term>> ReduceMatrix<Term extends AMonomial<Term>, Poly
            extends AMultivariatePolynomial<Term, Poly>>(Poly factory, IList<ArrayBasedPoly<Term>> basis,
            IList<HPolynomial<Term>> hPolynomials, DegreeVector[] hMonomials, IList<IList<ArrayBasedPoly<Term>>>
            f4reductions)
        {
            if (hPolynomials.IsEmpty())
                return new List();
            if (factory is MultivariatePolynomialZp64)
                return (IList<ArrayBasedPoly<Term>>)ReduceMatrixZp64((MultivariatePolynomialZp64)factory, (IList)basis,
                    (IList)hPolynomials, hMonomials, (IList)f4reductions);
            else
                return (IList<ArrayBasedPoly<Term>>)ReduceMatrixE((MultivariatePolynomial)factory, (IList)basis,
                    (IList)hPolynomials, hMonomials, (IList)f4reductions);
        }

        private static readonly double DENSE_FILLING_THRESHOLD = 0.1;

        /* ******************************************* F4 Zp64 linear algebra ******************************************* */
        private static IList<ArrayBasedPoly<MonomialZp64>> ReduceMatrixZp64(MultivariatePolynomialZp64 factory,
            IList<ArrayBasedPoly<MonomialZp64>> basis, IList<HPolynomial<MonomialZp64>> hPolynomials,
            DegreeVector[] hMonomials, IList<IList<ArrayBasedPoly<MonomialZp64>>> f4reductions)
        {
            IntegersZp64 ring = factory.ring;
            IMonomialAlgebra<MonomialZp64> mAlgebra = factory.monomialAlgebra;

            // We use the structured Gaussian elimination strategy as described in
            // J.-C. Faugere & S. Lachartre, PASCO'10 https://doi.org/10.1145/1837210.1837225
            // "Parallel Gaussian Elimination for Gröbner bases computations in finite fields"
            //
            // By swapping the rows and non-pivoting columns, each matrix in F4
            // can be rewritten in the following form (F4-form):
            //
            //    \x0x00xx000x | x0x00xx0000x0x0x0xx000xx000xx0
            //    0\x0x0x00xx0 | 0xx0000x0xxx0x00xx0x0000xx0000
            //    00\x00x000x0 | 0x0x0000x0x0xx00xx0x00x0x0xx00
            //    000\xx0x0x00 | xx0xxx00x000x0x0xx00x0x0xx000x
            //    0000\xx0x0x0 | x0000xx0x00x0xxx0xx0000x000xx0
            //    00000\x0000x | 00x0000x0x0x0xx0xx0xx000xx0000
            //    000000\xx00x | 0x0x000x00x0xxx0xx00xxx0x0xx00
            //    0000000\x0x0 | xx00xx00xx00x000xx0xx00x0x000x
            //    ............ | ..............................
            //    -------------+-------------------------------
            //    0xx000x0x0xx | xxxxxx0xxxxxxx0xxxxxxxxxxxxxxx
            //    x0xx000x0x00 | xxxx0xxxxxxxxxxxxxx0xxxxxxxxxx
            //    00x00x0000xx | xxxxxxx0xxxxxxxxxxxxxxx0xxxxxx
            //    x0000x00xx0x | xxxxxxxxxxxxxxxxx0xxxxxxx0xxxx
            //    ............ | ..............................
            //
            // We denote:
            //
            // A - upper left  block (very sparse, triangular)         -- pivoting rows
            // B - upper right block (partially sparse, rectangular)   -- pivoting rows
            // C -  down left  block (partially  sparse, rectangular)  -- non-pivoting rows
            // D -  down right block (dense, rectangular)              -- non-pivoting rows
            //
            // The algorithm to reduce the matrix is then very simple:
            //
            // 1) row reduce A (B is still partially sparse)
            // 2) annihilate C (D is now almost certainly dense)
            // 3) row echelon & row reduce D
            // 4) row reduce B
            // reverse order for binary searching
            Comparator<DegreeVector> reverseOrder = (a, b) => factory.ordering.Compare(b, a);
            int nRows = hPolynomials.Count, nColumns = hMonomials.Length, iRow, iColumn;

            // <- STEP 0: bring matrix to F4-form
            // number of lead-terms in each column (columns with no any lead
            // terms are non-pivoting and can be rearranged)
            int[] columnsLeadTermsFilling = new int[nColumns];

            // detect non-pivoting columns
            int iOldColumnPrev = 0;
            foreach (HPolynomial<MonomialZp64> hPoly in hPolynomials)
            {
                iColumn = Arrays.BinarySearch(hMonomials, iOldColumnPrev, hMonomials.Length, hPoly.hPoly.Lt(),
                    reverseOrder);
                iOldColumnPrev = iColumn;
                ++columnsLeadTermsFilling[iColumn];
            }


            // find non pivoting columns
            TIntArrayList nonPivotColumns = new TIntArrayList();
            for (iColumn = 0; iColumn < nRows + nonPivotColumns.Count && iColumn < nColumns; ++iColumn)
                if (columnsLeadTermsFilling[iColumn] == 0)
                    nonPivotColumns.Add(iColumn);

            // now we move non-pivoting columns to the right
            // mapping between old and new columns numeration
            int[] nonPivotColumnsArr = nonPivotColumns.ToArray();
            int[] columnsRearrangement =
                ArraysUtil.AddAll(ArraysUtil.IntSetDifference(ArraysUtil.Sequence(0, nColumns), nonPivotColumnsArr),
                    nonPivotColumnsArr);

            // back mapping between new and old columns numeration
            int[] columnsBackRearrangement = new int[nColumns];
            for (int i = 0; i < nColumns; ++i)
                columnsBackRearrangement[columnsRearrangement[i]] = i;

            // number of non-zero entries in each column
            int[] columnsFilling = new int[nColumns];

            // index of each term in hPolynomials in the hMonomials array
            int[,] mapping = new int[nRows];

            // estimated row filling of B matrix (number of nonzero elements in each row)
            int[] bRowsFilling = new int[nRows];

            // first iteration: gather info about matrix pattern
            for (iRow = 0; iRow < nRows; ++iRow)
            {
                ArrayBasedPoly<MonomialZp64> hPoly = hPolynomials[iRow].hPoly;
                mapping[iRow] = new int[hPoly.Count];
                iOldColumnPrev = 0;
                for (int i = 0; i < hPoly.Count; ++i)
                {
                    MonomialZp64 term = hPoly[i];

                    // column in old numeration
                    int iOldColumn = Arrays.BinarySearch(hMonomials, iOldColumnPrev, hMonomials.Length, term,
                        reverseOrder);
                    iOldColumnPrev = iOldColumn;

                    // column in new numeration
                    iColumn = columnsBackRearrangement[iOldColumn];
                    mapping[iRow][i] = iColumn;
                    ++columnsFilling[iColumn];
                    if (iColumn >= nRows)
                        ++bRowsFilling[iRow];
                }
            }


            // choose pivoting rows, so that B matrix is maximally sparse (rows with minimal bFillIns)
            TIntArrayList pivots = new TIntArrayList();
            for (iColumn = 0; iColumn < nRows; ++iColumn)
            {
                int minFillIn = Integer.MAX_VALUE;
                int pivot = -1;
                for (iRow = iColumn; iRow < nRows; ++iRow)
                    if (mapping[iRow][0] == iColumn && bRowsFilling[iRow] < minFillIn)
                    {
                        minFillIn = bRowsFilling[iRow];
                        pivot = iRow;
                    }
                    else if (pivot != -1 && mapping[iRow][0] != iColumn)
                        break;

                if (pivot == -1)
                    break;
                pivots.Add(pivot);
            }

            bRowsFilling = null; // prevent further use

            // rearrange rows: move pivots up and non-pivots down
            int nPivotRows = pivots.Count;
            for (int i = 0; i < nPivotRows; ++i)
            {
                int pivot = pivots[i];
                Collections.Swap(hPolynomials, i, pivot);
                ArraysUtil.Swap(mapping, i, pivot);
            }

            for (int i = 0; i < nPivotRows; ++i)
                hPolynomials[i].hPoly.Monic();

            // the matrix is now is in the desired F4-form
            // <- STEP 1: prepare data structures
            // dense columns in matrices B & D
            int[] bDenseColumns = IntStream.Range(nPivotRows, nColumns)
                .Filter((i) => 1 * columnsFilling[i] / nRows > DENSE_FILLING_THRESHOLD).Map((i) => i - nPivotRows)
                .ToArray(); // <- it is sorted (for below binary searching)

            // A & C are very sparse
            SparseRowMatrixZp64 aMatrix = new SparseRowMatrixZp64(ring, nPivotRows, nPivotRows, new int[0]),
                cMatrix = new SparseRowMatrixZp64(ring, nRows - nPivotRows, nPivotRows, new int[0]);

            // sparse matrices B & D
            SparseRowMatrixZp64 bMatrix =
                    new SparseRowMatrixZp64(ring, nPivotRows, nColumns - nPivotRows, bDenseColumns),
                dMatrix = new SparseRowMatrixZp64(ring, nRows - nPivotRows, nColumns - nPivotRows, bDenseColumns);
            for (iRow = 0; iRow < nRows; ++iRow)
            {
                ArrayBasedPoly<MonomialZp64> hPoly = hPolynomials[iRow].hPoly;
                TIntArrayList acSparseCols = new TIntArrayList(), bdSparseCols = new TIntArrayList();
                TLongArrayList acSparseVals = new TLongArrayList(), bdSparseVals = new TLongArrayList();
                long[] bdDenseVals = new long[bDenseColumns.Length];
                for (int i = 0; i < hPoly.Count; i++)
                {
                    iColumn = mapping[iRow][i];
                    long coefficient = hPoly[i].coefficient;
                    if (iColumn < nPivotRows)
                    {
                        // element of matrix A or C
                        acSparseCols.Add(iColumn);
                        acSparseVals.Add(coefficient);
                    }
                    else
                    {
                        // element of matrix B or D
                        iColumn -= nPivotRows;
                        int iDense;
                        if ((iDense = Arrays.BinarySearch(bDenseColumns, iColumn)) >= 0)

                            // this is dense column (in B or D)
                            bdDenseVals[iDense] = coefficient;
                        else
                        {
                            bdSparseCols.Add(iColumn);
                            bdSparseVals.Add(coefficient);
                        }
                    }
                }

                int[] bdSparseColumns = bdSparseCols.ToArray();
                long[] bdSparseValues = bdSparseVals.ToArray();
                ArraysUtil.QuickSort(bdSparseColumns, bdSparseValues);
                int[] acSparseColumns = acSparseCols.ToArray();
                long[] acSparseValues = acSparseVals.ToArray();
                ArraysUtil.QuickSort(acSparseColumns, acSparseValues);
                if (iRow < nPivotRows)
                {
                    aMatrix.rows[iRow] = new SparseArrayZp64(ring, nPivotRows, new int[0], acSparseColumns, new long[0],
                        acSparseValues);
                    bMatrix.rows[iRow] = new SparseArrayZp64(ring, nColumns - nPivotRows, bDenseColumns,
                        bdSparseColumns, bdDenseVals, bdSparseValues);
                }
                else
                {
                    cMatrix.rows[iRow - nPivotRows] = new SparseArrayZp64(ring, nPivotRows, new int[0], acSparseColumns,
                        new long[0], acSparseValues);
                    dMatrix.rows[iRow - nPivotRows] = new SparseArrayZp64(ring, nColumns - nPivotRows, bDenseColumns,
                        bdSparseColumns, bdDenseVals, bdSparseValues);
                }
            }


            // <- STEP 2: row reduce matrix A
            // we start from the last column in matrix A
            for (iRow = nPivotRows - 2; iRow >= 0; --iRow)
            {
                SparseArrayZp64 aRow = aMatrix.rows[iRow];
                long[] bRow = bMatrix.rows[iRow].ToDense();
                for (int i = 0; i < aRow.sparsePositions.Length; ++i)
                {
                    iColumn = aRow.sparsePositions[i];
                    if (iColumn == iRow)
                        continue;
                    SubtractSparseFromDense(ring, bRow, bMatrix.rows[iColumn], aRow.sparseValues[i]);
                }

                bMatrix.rows[iRow] = new SparseArrayZp64(ring, bMatrix.densePositions, bRow);
            }


            // <-  STEP 3: annihilate matrix C
            for (iRow = 0; iRow < nRows - nPivotRows; ++iRow)
            {
                SparseArrayZp64 cRow = cMatrix.rows[iRow];
                long[] dRow = dMatrix.rows[iRow].ToDense();
                for (int i = 0; i < cRow.sparsePositions.Length; ++i)
                {
                    iColumn = cRow.sparsePositions[i];
                    SubtractSparseFromDense(ring, dRow, bMatrix.rows[iColumn], cRow.sparseValues[i]);
                }

                dMatrix.rows[iRow] = new SparseArrayZp64(ring, dMatrix.densePositions, dRow);
            }


            // <-  STEP 4: compute row reduced echelon form of matrix D
            // this can be optimized (use dense structures, use same structured elimination), but actually
            // it doesn't take too much time
            // make D maximally triangular
            Arrays.Sort(dMatrix.rows, Comparator.ComparingInt(SparseArrayZp64.FirstNonZeroPosition()));
            dMatrix.RowReduce();

            // <-  STEP 5: row reduce B
            int dShift = 0;
            for (iRow = 0; iRow < dMatrix.nRows; iRow++)
            {
                SparseArrayZp64 dRow = dMatrix.rows[iRow];
                iColumn = iRow + dShift;
                if (iColumn >= dMatrix.nColumns)
                    break;
                if (dRow.Coefficient(iColumn) == 0)
                {
                    --iRow;
                    ++dShift;
                    continue;
                }

                for (int i = 0; i < bMatrix.nRows; ++i)
                    if (bMatrix.rows[i].Coefficient(iColumn) != 0)
                        bMatrix.rows[i].Subtract(dRow, bMatrix.rows[i].Coefficient(iColumn));
            }


            // <- STEP 6: finally form N+ polynomials
            // leading monomials of H-polynomials
            TreeSet<DegreeVector> hLeadMonomials = hPolynomials.Stream().Map((p) => p.hPoly.Lt())
                .Collect(Collectors.ToCollection(() => new TreeSet(factory.ordering)));
            IList<ArrayBasedPoly<MonomialZp64>> nPolynomials = new List();
            for (iRow = 0; iRow < nRows; ++iRow)
            {
                List<MonomialZp64> candidateList = new List();
                if (iRow < nPivotRows)
                    candidateList.Add(new MonomialZp64(hMonomials[columnsRearrangement[iRow]], 1));
                SparseArrayZp64 row = iRow < nPivotRows ? bMatrix.rows[iRow] : dMatrix.rows[iRow - nPivotRows];
                for (int i = 0; i < row.sparsePositions.Length; i++)
                {
                    long val = row.sparseValues[i];
                    if (val != 0)
                        candidateList.Add(
                            new MonomialZp64(hMonomials[columnsRearrangement[nPivotRows + row.sparsePositions[i]]],
                                val));
                }

                for (int i = 0; i < bDenseColumns.Length; i++)
                {
                    long val = row.denseValues[i];
                    if (val != 0)
                        candidateList.Add(
                            new MonomialZp64(hMonomials[columnsRearrangement[nPivotRows + bDenseColumns[i]]], val));
                }

                candidateList.Sort(reverseOrder);
                if (!candidateList.IsEmpty())
                {
                    ArrayBasedPoly<MonomialZp64> poly = new ArrayBasedPoly(mAlgebra,
                        candidateList.ToArray(mAlgebra.CreateArray(candidateList.Count)), factory.nVariables);
                    if (poly.Lt().coefficient != 1)
                    {
                        long factor = ring.Reciprocal(poly.Lt().coefficient);
                        for (int i = 0; i < poly.data.Length; i++)
                            poly.data[i] = poly.data[i].SetCoefficient(ring.Multiply(factor, poly.data[i].coefficient));
                    }

                    nPolynomials.Add(poly);
                }
            }

            nPolynomials.Sort((a, b) => reverseOrder.Compare(a.Lt(), b.Lt()));

            // resulting N+ set
            IList<ArrayBasedPoly<MonomialZp64>> nPlusPolynomials = new List();
            foreach (ArrayBasedPoly<MonomialZp64> candidate in nPolynomials)
            {
                if (!hLeadMonomials.Contains(candidate.Lt()))
                {
                    // lt is new -> just add
                    nPlusPolynomials.Add(candidate);
                    f4reductions.Add(new List(Collections.SingletonList(candidate)));
                }
                else

                    // update f4reductions
                    for (int iIndex = 0; iIndex < basis.Count; ++iIndex)
                    {
                        ArrayBasedPoly<MonomialZp64> g = basis[iIndex];
                        if (g == null)
                            continue;
                        if (candidate.Lt().DvDivisibleBy(g.Lt()))
                        {
                            IList<ArrayBasedPoly<MonomialZp64>> reductions = f4reductions[iIndex];
                            bool reduced = false;
                            for (int i = 0; i < reductions.Count; ++i)
                            {
                                ArrayBasedPoly<MonomialZp64> red = reductions[i];
                                if (red.Lt().DvEquals(candidate.Lt()))
                                {
                                    reductions[i] = candidate;
                                    reduced = true;
                                    break;
                                }
                            }

                            if (!reduced)
                                reductions.Add(candidate);
                        }
                    }
            }

            return nPlusPolynomials;
        }


        sealed class SparseArrayZp64
        {
            readonly IntegersZp64 ring;
            readonly int length;
            readonly int[] densePositions;
            readonly long[] denseValues;
            int[] sparsePositions;
            long[] sparseValues;

            SparseArrayZp64(IntegersZp64 ring, int length, int[] densePositions, int[] sparsePositions,
                long[] denseValues, long[] sparseValues)
            {
                this.ring = ring;
                this.Length = length;
                this.densePositions = densePositions;
                this.sparsePositions = sparsePositions;
                this.denseValues = denseValues;
                this.sparseValues = sparseValues;
            }

            SparseArrayZp64(IntegersZp64 ring, int[] densePositions, long[] denseArray)
            {
                this.ring = ring;
                this.densePositions = densePositions;
                this.Length = denseArray.Length;
                this.denseValues = new long[densePositions.Length];
                for (int i = 0; i < densePositions.Length; i++)
                    denseValues[i] = denseArray[densePositions[i]];
                TIntArrayList sparsePositions = new TIntArrayList();
                TLongArrayList sparseValues = new TLongArrayList();
                for (int i = 0; i < denseArray.Length; ++i)
                {
                    if (denseArray[i] == 0)
                        continue;
                    if (Arrays.BinarySearch(densePositions, i) >= 0)
                        continue;
                    sparsePositions.Add(i);
                    sparseValues.Add(denseArray[i]);
                }

                this.sparsePositions = sparsePositions.ToArray();
                this.sparseValues = sparseValues.ToArray();
            }

            long[] ToDense()
            {
                long[] result = new long[length];
                for (int i = 0; i < densePositions.Length; ++i)
                    result[densePositions[i]] = denseValues[i];
                for (int i = 0; i < sparsePositions.Length; ++i)
                    result[sparsePositions[i]] = sparseValues[i];
                return result;
            }

            void Subtract(SparseArrayZp64 pivot, long factor, int iColumn, int firstDense)
            {
                if (factor == 0)
                    return;
                long negFactor = ring.Negate(factor);

                // adding dense parts
                for (int i = firstDense; i < denseValues.Length; i++)
                    denseValues[i] = ring.Subtract(denseValues[i], ring.Multiply(factor, pivot.denseValues[i]));

                // subtracting sparse parts
                int[] pivCols = pivot.sparsePositions;
                long[] pivVals = pivot.sparseValues;
                int firstSparse = ArraysUtil.BinarySearch1(sparsePositions, iColumn);

                // resulting non-zero columns
                TIntArrayList resCols = new TIntArrayList(sparsePositions.Length + pivCols.Length);
                TLongArrayList resVals = new TLongArrayList(sparsePositions.Length + pivCols.Length);
                resCols.Add(sparsePositions, 0, firstSparse);
                resVals.Add(sparseValues, 0, firstSparse);
                int iSel = firstSparse, iPiv = 0;
                while (iSel < sparsePositions.Length && iPiv < pivCols.Length)
                {
                    int selCol = sparsePositions[iSel], othCol = pivCols[iPiv];
                    if (selCol == othCol)
                    {
                        long subtract = ring.Subtract(sparseValues[iSel], ring.Multiply(factor, pivVals[iPiv]));
                        if (subtract != 0)
                        {
                            resCols.Add(selCol);
                            resVals.Add(subtract);
                        }

                        ++iSel;
                        ++iPiv;
                    }
                    else if (selCol < othCol)
                    {
                        resCols.Add(selCol);
                        resVals.Add(sparseValues[iSel]);
                        ++iSel;
                    }
                    else if (selCol > othCol)
                    {
                        resCols.Add(othCol);
                        resVals.Add(ring.Multiply(negFactor, pivVals[iPiv]));
                        ++iPiv;
                    }
                }

                if (iSel < sparsePositions.Length)
                    for (; iSel < sparsePositions.Length; ++iSel)
                    {
                        resCols.Add(sparsePositions[iSel]);
                        resVals.Add(sparseValues[iSel]);
                    }

                if (iPiv < pivCols.Length)
                    for (; iPiv < pivCols.Length; ++iPiv)
                    {
                        resCols.Add(pivCols[iPiv]);
                        resVals.Add(ring.Multiply(negFactor, pivVals[iPiv]));
                    }

                sparsePositions = resCols.ToArray();
                sparseValues = resVals.ToArray();
            }

            void Subtract(SparseArrayZp64 pivot, long factor)
            {
                Subtract(pivot, factor, 0, 0);
            }

            void Multiply(long factor)
            {
                if (factor == 1)
                    return;
                for (int i = 0; i < sparseValues.Length; ++i)
                    sparseValues[i] = ring.Multiply(sparseValues[i], factor);
                for (int i = 0; i < denseValues.Length; ++i)
                    denseValues[i] = ring.Multiply(denseValues[i], factor);
            }

            long Coefficient(int iRow)
            {
                int index = Arrays.BinarySearch(sparsePositions, iRow);
                if (index >= 0)
                    return sparseValues[index];
                index = Arrays.BinarySearch(densePositions, iRow);
                if (index >= 0)
                    return denseValues[index];
                return 0;
            }

            void Multiply(int iRow, long value)
            {
                int index = Arrays.BinarySearch(sparsePositions, iRow);
                if (index >= 0)
                    sparseValues[index] = ring.Multiply(sparseValues[index], value);
                else
                {
                    index = Arrays.BinarySearch(densePositions, iRow);
                    if (index >= 0)
                        denseValues[index] = ring.Multiply(denseValues[index], value);
                }
            }

            void Subtract(int iRow, long value)
            {
                if (value == 0)
                    return;
                int index = Arrays.BinarySearch(densePositions, iRow);
                if (index >= 0)
                    denseValues[index] = ring.Subtract(denseValues[index], value);
                else
                {
                    index = Arrays.BinarySearch(sparsePositions, iRow);
                    if (index >= 0)
                    {
                        sparseValues[index] = ring.Subtract(sparseValues[index], value);
                        if (sparseValues[index] == 0)
                        {
                            sparsePositions = ArraysUtil.Remove(sparsePositions, index);
                            sparseValues = ArraysUtil.Remove(sparseValues, index);
                        }
                    }
                    else
                    {
                        index = ~index;
                        sparsePositions = ArraysUtil.Insert(sparsePositions, index, iRow);
                        sparseValues = ArraysUtil.Insert(sparseValues, index, ring.Negate(value));
                    }
                }
            }

            int FirstNonZeroPosition()
            {
                int firstSparse = sparsePositions.Length != 0 ? sparsePositions[0] : Integer.MAX_VALUE;
                for (int i = 0; i < densePositions.Length; ++i)
                    if (denseValues[i] != 0)
                        return Math.Min(densePositions[i], firstSparse);
                return firstSparse;
            }
        }

        // denseArray - factor * sparseArray
        static void SubtractSparseFromDense(IntegersZp64 ring, long[] denseArray, SparseArrayZp64 sparseArray,
            long factor)
        {
            for (int i = 0; i < sparseArray.densePositions.Length; i++)
                denseArray[sparseArray.densePositions[i]] = ring.Subtract(denseArray[sparseArray.densePositions[i]],
                    ring.Multiply(factor, sparseArray.denseValues[i]));
            for (int i = 0; i < sparseArray.sparsePositions.Length; i++)
                denseArray[sparseArray.sparsePositions[i]] = ring.Subtract(denseArray[sparseArray.sparsePositions[i]],
                    ring.Multiply(factor, sparseArray.sparseValues[i]));
        }

        sealed class SparseColumnMatrixZp64
        {
            readonly IntegersZp64 ring;
            readonly int nRows, nColumns;
            readonly int[] densePositions;
            readonly SparseArrayZp64[] columns;

            SparseColumnMatrixZp64(IntegersZp64 ring, int nRows, int nColumns, int[] densePositions)
            {
                this.ring = ring;
                this.nRows = nRows;
                this.nColumns = nColumns;
                this.densePositions = densePositions;
                this.columns = new SparseArrayZp64[nColumns];
            }

            void MultiplyRow(int iRow, long value)
            {
                if (value != 1)
                    for (int i = 0; i < nColumns; ++i)
                        columns[i].Multiply(iRow, value);
            }

            SparseRowMatrixZp64 ToRowMatrix(int[] densePositions)
            {
                SparseRowMatrixZp64 rowMatrix = new SparseRowMatrixZp64(ring, nRows, nColumns, densePositions);
                TIntArrayList[] sparseColumns = new TIntArrayList[nRows];
                TLongArrayList[] sparseValues = new TLongArrayList[nRows];
                long[,] denseValues = new long[nRows, densePositions.Length];
                for (int iRow = 0; iRow < nRows; ++iRow)
                {
                    sparseColumns[iRow] = new TIntArrayList();
                    sparseValues[iRow] = new TLongArrayList();
                }

                for (int iColumn = 0; iColumn < nColumns; ++iColumn)
                {
                    SparseArrayZp64 column = columns[iColumn];
                    int iDenseColumn = Arrays.BinarySearch(densePositions, iColumn);
                    if (iDenseColumn >= 0)
                    {
                        for (int i = 0; i < column.densePositions.Length; i++)
                            denseValues[column.densePositions[i]][iDenseColumn] = column.denseValues[i];
                        for (int i = 0; i < column.sparsePositions.Length; ++i)
                            denseValues[column.sparsePositions[i]][iDenseColumn] = column.sparseValues[i];
                    }
                    else
                    {
                        for (int i = 0; i < column.densePositions.Length; i++)
                        {
                            sparseColumns[column.densePositions[i]].Add(iColumn);
                            sparseValues[column.densePositions[i]].Add(column.denseValues[i]);
                        }

                        for (int i = 0; i < column.sparsePositions.Length; ++i)
                        {
                            sparseColumns[column.sparsePositions[i]].Add(iColumn);
                            sparseValues[column.sparsePositions[i]].Add(column.sparseValues[i]);
                        }
                    }
                }

                for (int iRow = 0; iRow < nRows; ++iRow)
                    rowMatrix.rows[iRow] = new SparseArrayZp64(ring, nColumns, densePositions,
                        sparseColumns[iRow].ToArray(), denseValues[iRow], sparseValues[iRow].ToArray());
                return rowMatrix;
            }
        }

        sealed class SparseRowMatrixZp64
        {
            readonly IntegersZp64 ring;
            readonly int nRows, nColumns;
            readonly int[] densePositions;
            readonly SparseArrayZp64[] rows;

            SparseRowMatrixZp64(IntegersZp64 ring, int nRows, int nColumns, int[] densePositions)
            {
                this.ring = ring;
                this.nRows = nRows;
                this.nColumns = nColumns;
                this.densePositions = densePositions;
                this.rows = new SparseArrayZp64[nRows];
            }

            SparseRowMatrixZp64(IntegersZp64 ring, int nRows, int nColumns, int[] densePositions,
                SparseArrayZp64[] rows)
            {
                this.ring = ring;
                this.nRows = nRows;
                this.nColumns = nColumns;
                this.densePositions = densePositions;
                this.rows = rows;
            }

            SparseRowMatrixZp64 Range(int from, int to)
            {
                return new SparseRowMatrixZp64(ring, to - from, nColumns, densePositions,
                    Arrays.CopyOfRange(rows, from, to));
            }

            long[][] DenseMatrix()
            {
                long[,] result = new long[rows.Length, nColumns];
                for (int iRow = 0; iRow < rows.Length; ++iRow)
                {
                    SparseArrayZp64 row = rows[iRow];
                    for (int i = 0; i < row.sparsePositions.Length; i++)
                        result[iRow][row.sparsePositions[i]] = row.sparseValues[i];
                    for (int i = 0; i < densePositions.Length; i++)
                        result[iRow][densePositions[i]] = row.denseValues[i];
                }

                return result;
            }

            // return rank
            int RowReduce()
            {
                // Gaussian elimination
                int nZeroRows = 0;
                for (int iCol = 0, to = rows.length; iCol < to; ++iCol)
                {
                    int iRow = iCol - nZeroRows;
                    int pivotIndex = -1;
                    for (int i = iRow; i < rows.Length; ++i)
                    {
                        if (rows[i].Coefficient(iCol) == 0)
                            continue;
                        if (pivotIndex == -1)
                        {
                            pivotIndex = i;
                            continue;
                        }

                        if (rows[i].sparsePositions.Length < rows[pivotIndex].sparsePositions.Length)
                            pivotIndex = i;
                    }

                    if (pivotIndex == -1)
                    {
                        ++nZeroRows;
                        to = Math.Min(nColumns, rows.Length + nZeroRows);
                        continue;
                    }

                    ArraysUtil.Swap(rows, pivotIndex, iRow);
                    SparseArrayZp64 pivot = rows[iRow];
                    long diagonalValue = pivot.Coefficient(iCol);

                    // row-reduction
                    pivot.Multiply(ring.Reciprocal(diagonalValue));
                    int firstDense = ArraysUtil.BinarySearch1(densePositions, iCol);
                    for (int row = 0; row < rows.Length; ++row)
                    {
                        if (row == iRow)
                            continue;
                        long value = rows[row].Coefficient(iCol);
                        if (value == 0)
                            continue;
                        rows[row].Subtract(pivot, value, iCol, firstDense);
                    }
                }

                return Math.Min(nRows, nColumns) - nZeroRows;
            }
        }

        /* ******************************************* F4 generic linear algebra ******************************************* */
        private static IList<ArrayBasedPoly<Monomial<E>>> ReduceMatrixE<E>(MultivariatePolynomial<E> factory,
            IList<ArrayBasedPoly<Monomial<E>>> basis, IList<HPolynomial<Monomial<E>>> hPolynomials,
            DegreeVector[] hMonomials, IList<IList<ArrayBasedPoly<Monomial<E>>>> f4reductions)
        {
            Ring<E> ring = factory.ring;
            IMonomialAlgebra<Monomial<E>> mAlgebra = factory.monomialAlgebra;

            // We use the structured Gaussian elimination strategy as described in
            // J.-C. Faugere & S. Lachartre, PASCO'10 https://doi.org/10.1145/1837210.1837225
            // "Parallel Gaussian Elimination for Gröbner bases computations in finite fields"
            //
            // By swapping the rows and non-pivoting columns, each matrix in F4
            // can be rewritten in the following form (F4-form):
            //
            //    \x0x00xx000x | x0x00xx0000x0x0x0xx000xx000xx0
            //    0\x0x0x00xx0 | 0xx0000x0xxx0x00xx0x0000xx0000
            //    00\x00x000x0 | 0x0x0000x0x0xx00xx0x00x0x0xx00
            //    000\xx0x0x00 | xx0xxx00x000x0x0xx00x0x0xx000x
            //    0000\xx0x0x0 | x0000xx0x00x0xxx0xx0000x000xx0
            //    00000\x0000x | 00x0000x0x0x0xx0xx0xx000xx0000
            //    000000\xx00x | 0x0x000x00x0xxx0xx00xxx0x0xx00
            //    0000000\x0x0 | xx00xx00xx00x000xx0xx00x0x000x
            //    ............ | ..............................
            //    -------------+-------------------------------
            //    0xx000x0x0xx | xxxxxx0xxxxxxx0xxxxxxxxxxxxxxx
            //    x0xx000x0x00 | xxxx0xxxxxxxxxxxxxx0xxxxxxxxxx
            //    00x00x0000xx | xxxxxxx0xxxxxxxxxxxxxxx0xxxxxx
            //    x0000x00xx0x | xxxxxxxxxxxxxxxxx0xxxxxxx0xxxx
            //    ............ | ..............................
            //
            // We denote:
            //
            // A - upper left  block (very sparse, triangular)         -- pivoting rows
            // B - upper right block (partially sparse, rectangular)   -- pivoting rows
            // C -  down left  block (partially  sparse, rectangular)  -- non-pivoting rows
            // D -  down right block (dense, rectangular)              -- non-pivoting rows
            //
            // The algorithm to reduce the matrix is then very simple:
            //
            // 1) row reduce A (B is still partially sparse)
            // 2) annihilate C (D is now almost certainly dense)
            // 3) row echelon & row reduce D
            // 4) row reduce B
            // reverse order for binary searching
            Comparator<DegreeVector> reverseOrder = (a, b) => factory.ordering.Compare(b, a);
            int nRows = hPolynomials.Count, nColumns = hMonomials.Length, iRow, iColumn;

            // <- STEP 0: bring matrix to F4-form
            // bring each poly to canonical form (either monic or primitive)
            hPolynomials.ForEach((h) => h.hPoly.Canonical());

            // number of lead-terms in each column (columns with no any lead
            // terms are non-pivoting and can be rearranged)
            int[] columnsLeadTermsFilling = new int[nColumns];

            // detect non-pivoting columns
            int iOldColumnPrev = 0;
            foreach (HPolynomial<Monomial<E>> hPoly in hPolynomials)
            {
                iColumn = Arrays.BinarySearch(hMonomials, iOldColumnPrev, hMonomials.Length, hPoly.hPoly.Lt(),
                    reverseOrder);
                iOldColumnPrev = iColumn;
                ++columnsLeadTermsFilling[iColumn];
            }


            // find non pivoting columns
            TIntArrayList nonPivotColumns = new TIntArrayList();
            for (iColumn = 0; iColumn < nRows + nonPivotColumns.Count && iColumn < nColumns; ++iColumn)
                if (columnsLeadTermsFilling[iColumn] == 0)
                    nonPivotColumns.Add(iColumn);

            // now we move non-pivoting columns to the right
            // mapping between old and new columns numeration
            int[] nonPivotColumnsArr = nonPivotColumns.ToArray();
            int[] columnsRearrangement =
                ArraysUtil.AddAll(ArraysUtil.IntSetDifference(ArraysUtil.Sequence(0, nColumns), nonPivotColumnsArr),
                    nonPivotColumnsArr);

            // back mapping between new and old columns numeration
            int[] columnsBackRearrangement = new int[nColumns];
            for (int i = 0; i < nColumns; ++i)
                columnsBackRearrangement[columnsRearrangement[i]] = i;

            // number of non-zero entries in each column
            int[] columnsFilling = new int[nColumns];

            // index of each term in hPolynomials in the hMonomials array
            int[,] mapping = new int[nRows];

            // estimated row filling of B matrix (number of nonzero elements in each row)
            int[] bRowsFilling = new int[nRows];

            // first iteration: gather info about matrix pattern
            for (iRow = 0; iRow < nRows; ++iRow)
            {
                ArrayBasedPoly<Monomial<E>> hPoly = hPolynomials[iRow].hPoly;
                mapping[iRow] = new int[hPoly.Count];
                iOldColumnPrev = 0;
                for (int i = 0; i < hPoly.Count; ++i)
                {
                    Monomial<E> term = hPoly[i];

                    // column in old numeration
                    int iOldColumn = Arrays.BinarySearch(hMonomials, iOldColumnPrev, hMonomials.Length, term,
                        reverseOrder);
                    iOldColumnPrev = iOldColumn;

                    // column in new numeration
                    iColumn = columnsBackRearrangement[iOldColumn];
                    mapping[iRow][i] = iColumn;
                    ++columnsFilling[iColumn];
                    if (iColumn >= nRows)
                        ++bRowsFilling[iRow];
                }
            }


            // choose pivoting rows, so that B matrix is maximally sparse (rows with minimal bFillIns)
            // and lead terms of pivoting polys are units (if possible)
            TIntArrayList pivots = new TIntArrayList();
            for (iColumn = 0; iColumn < nRows; ++iColumn)
            {
                int minFillIn = Integer.MAX_VALUE;
                int pivot = -1;
                bool unitLeadTerm = false;
                for (iRow = iColumn; iRow < nRows; ++iRow)
                    if (mapping[iRow][0] == iColumn)
                    {
                        // try to find pivot with unit lead term
                        bool ult = ring.IsUnit(hPolynomials[iRow].hPoly.Lt().coefficient);
                        if (!unitLeadTerm && ult)
                        {
                            minFillIn = bRowsFilling[iRow];
                            pivot = iRow;
                            unitLeadTerm = true;
                        }
                        else if ((unitLeadTerm == ult) && bRowsFilling[iRow] < minFillIn)
                        {
                            minFillIn = bRowsFilling[iRow];
                            pivot = iRow;
                        }
                    }
                    else if (pivot != -1 && mapping[iRow][0] != iColumn)
                        break;

                if (pivot == -1)
                    break;
                pivots.Add(pivot);
            }

            bRowsFilling = null; // prevent further use

            // rearrange rows: move pivots up and non-pivots down
            int nPivotRows = pivots.Count;
            for (int i = 0; i < nPivotRows; ++i)
            {
                int pivot = pivots[i];
                Collections.Swap(hPolynomials, i, pivot);
                ArraysUtil.Swap(mapping, i, pivot);
            }


            // the matrix is in the desired F4-form
            // <- STEP 1: prepare data structures
            // dense columns in matrices B & D
            int[] bDenseColumns = IntStream.Range(nPivotRows, nColumns)
                .Filter((i) => 1 * columnsFilling[i] / nRows > DENSE_FILLING_THRESHOLD).Map((i) => i - nPivotRows)
                .ToArray(); // <- it is sorted (for below binary searching)

            // A & C are very sparse
            SparseRowMatrix<E> aMatrix = new SparseRowMatrix(ring, nPivotRows, nPivotRows, new int[0]),
                cMatrix = new SparseRowMatrix(ring, nRows - nPivotRows, nPivotRows, aMatrix.densePositions);

            // sparse matrices B & D
            SparseRowMatrix<E> bMatrix = new SparseRowMatrix(ring, nPivotRows, nColumns - nPivotRows, bDenseColumns),
                dMatrix = new SparseRowMatrix(ring, nRows - nPivotRows, nColumns - nPivotRows, bDenseColumns);
            for (iRow = 0; iRow < nRows; ++iRow)
            {
                ArrayBasedPoly<Monomial<E>> hPoly = hPolynomials[iRow].hPoly;
                TIntArrayList acSparseCols = new TIntArrayList(), bdSparseCols = new TIntArrayList();
                List<E> acSparseVals = new List(), bdSparseVals = new List();
                E[] bdDenseVals = ring.CreateZeroesArray(bDenseColumns.Length);
                for (int i = 0; i < hPoly.Count; i++)
                {
                    iColumn = mapping[iRow][i];
                    E coefficient = hPoly[i].coefficient;
                    if (iColumn < nPivotRows)
                    {
                        // element of matrix A or C
                        acSparseCols.Add(iColumn);
                        acSparseVals.Add(coefficient);
                    }
                    else
                    {
                        // element of matrix B or D
                        iColumn -= nPivotRows;
                        int iDense;
                        if ((iDense = Arrays.BinarySearch(bDenseColumns, iColumn)) >= 0)

                            // this is dense column (in B or D)
                            bdDenseVals[iDense] = coefficient;
                        else
                        {
                            bdSparseCols.Add(iColumn);
                            bdSparseVals.Add(coefficient);
                        }
                    }
                }

                int[] bdSparseColumns = bdSparseCols.ToArray();
                E[] bdSparseValues = bdSparseVals.ToArray(ring.CreateArray(bdSparseVals.Count));
                ArraysUtil.QuickSort(bdSparseColumns, bdSparseValues);
                int[] acSparseColumns = acSparseCols.ToArray();
                E[] acSparseValues = acSparseVals.ToArray(ring.CreateArray(acSparseVals.Count));
                ArraysUtil.QuickSort(acSparseColumns, acSparseValues);
                if (iRow < nPivotRows)
                {
                    aMatrix.rows[iRow] = new SparseArray(ring, nPivotRows, aMatrix.densePositions, acSparseColumns,
                        ring.CreateArray(0), acSparseValues);
                    bMatrix.rows[iRow] = new SparseArray(ring, nColumns - nPivotRows, bDenseColumns, bdSparseColumns,
                        bdDenseVals, bdSparseValues);
                }
                else
                {
                    cMatrix.rows[iRow - nPivotRows] = new SparseArray(ring, nPivotRows, cMatrix.densePositions,
                        acSparseColumns, ring.CreateArray(0), acSparseValues);
                    dMatrix.rows[iRow - nPivotRows] = new SparseArray(ring, nColumns - nPivotRows, bDenseColumns,
                        bdSparseColumns, bdDenseVals, bdSparseValues);
                }
            }


            // <- STEP 2: row reduce matrix A
            // we start from the last column in matrix A
            for (iRow = nPivotRows - 2; iRow >= 0; --iRow)
            {
                SparseArray<E> aRow = aMatrix.rows[iRow];
                E[] bRow = bMatrix.rows[iRow].ToDense();
                for (int i = aRow.sparsePositions.length - 1; i >= 0; --i)
                {
                    iColumn = aRow.sparsePositions[i];
                    if (iColumn == iRow)
                        continue; // diagonal
                    SparseArray<E> aPivotRow = aMatrix.rows[iColumn], bPivotRow = bMatrix.rows[iColumn];

                    // non-diagonal value in A (iRow, iColumn)
                    E aNonDiagonal = aRow.sparseValues[i];

                    // corresponding pivoting value (iColumn, iColumn) in A
                    E aPivot = aPivotRow.Coefficient(iColumn);
                    E lcm = ring.Lcm(aNonDiagonal, aPivot),
                        rowFactor = ring.DivideExact(lcm, aNonDiagonal),
                        pivotFactor = ring.DivideExact(lcm, aPivot);

                    // bRow = rowFactor * bRow -  pivotFactor * bPivotRow
                    SubtractSparseFromDense(ring, bRow, rowFactor, bPivotRow, pivotFactor);

                    // correct aRow
                    aRow.Multiply(rowFactor);
                }


                // set row in B
                bMatrix.rows[iRow] = new SparseArray(ring, bMatrix.densePositions, bRow);

                // diagonal value in A row
                E aRowDiagonalValue = aRow.Coefficient(iRow);

                // common content in the AB row
                E abRowContent = bMatrix.rows[iRow].Content(aRowDiagonalValue);
                bMatrix.rows[iRow].DivideExact(abRowContent);

                // recreate row of A (with single diagonal element)
                aRow.sparsePositions = new int[]
                {
                    iRow
                };
                aRow.sparseValues = ring.CreateArray(ring.DivideExact(aRowDiagonalValue, abRowContent));
            }


            // <-  STEP 3: annihilate matrix C
            for (iRow = 0; iRow < nRows - nPivotRows; ++iRow)
            {
                SparseArray<E> cRow = cMatrix.rows[iRow];
                E[] dRow = dMatrix.rows[iRow].ToDense();
                for (int i = cRow.sparsePositions.length - 1; i >= 0; --i)
                {
                    iColumn = cRow.sparsePositions[i];
                    SparseArray<E> aPivotRow = aMatrix.rows[iColumn], bPivotRow = bMatrix.rows[iColumn];

                    // value in C (iRow, iColumn)
                    E cElement = cRow.sparseValues[i];

                    // corresponding pivoting value (iColumn, iColumn)
                    E aPivot = aPivotRow.Coefficient(iColumn);
                    E lcm = ring.Lcm(cElement, aPivot),
                        dFactor = ring.DivideExact(lcm, cElement),
                        bFactor = ring.DivideExact(lcm, aPivot);

                    // bRow = rowFactor * bRow -  pivotFactor * bPivotRow
                    SubtractSparseFromDense(ring, dRow, dFactor, bPivotRow, bFactor);

                    // correct cRow
                    cRow.Multiply(dFactor);
                }

                dMatrix.rows[iRow] = new SparseArray(ring, dMatrix.densePositions, dRow);
                dMatrix.rows[iRow].PrimitivePart();
            }


            // <-  STEP 4: compute row reduced echelon form of matrix D
            // this can be optimized (use dense structures, use same structured elimination), but actually
            // it doesn't take too much time
            // make D maximally triangular
            Arrays.Sort(dMatrix.rows, Comparator.ComparingInt(SparseArray.FirstNonZeroPosition()));
            dMatrix.RowReduce();

            // <-  STEP 5: row reduce B
            int dShift = 0;
            for (iRow = 0; iRow < dMatrix.nRows; iRow++)
            {
                SparseArray<E> dPivotRow = dMatrix.rows[iRow];
                iColumn = iRow + dShift;
                if (iColumn >= dMatrix.nColumns)
                    break;
                if (ring.IsZero(dPivotRow.Coefficient(iColumn)))
                {
                    --iRow;
                    ++dShift;
                    continue;
                }

                for (int i = 0; i < bMatrix.nRows; ++i)
                    if (!ring.IsZero(bMatrix.rows[i].Coefficient(iColumn)))
                    {
                        SparseArray<E> bRow = bMatrix.rows[i], aRow = aMatrix.rows[i];
                        E dPivot = dPivotRow.Coefficient(iColumn),
                            bElement = bRow.Coefficient(iColumn),
                            lcm = ring.Lcm(bElement, dPivot),
                            bFactor = ring.DivideExact(lcm, bElement),
                            dFactor = ring.DivideExact(lcm, dPivot);

                        // reduce B row
                        bRow.Multiply(bFactor);
                        bRow.Subtract(dPivotRow, dFactor);

                        // correct corresponding A row
                        aRow.Multiply(bFactor);

                        // make that AB row primitive (remove common content)
                        E abContent = bRow.Content(aMatrix.rows[i].Coefficient(i));
                        bRow.DivideExact(abContent);
                        aRow.DivideExact(abContent);
                    }
            }


            // <- STEP 6: finally form N+ polynomials
            // leading monomials of H-polynomials
            TreeSet<DegreeVector> hLeadMonomials = hPolynomials.Stream().Map((p) => p.hPoly.Lt())
                .Collect(Collectors.ToCollection(() => new TreeSet(factory.ordering)));
            IList<ArrayBasedPoly<Monomial<E>>> nPolynomials = new List();
            for (iRow = 0; iRow < nRows; ++iRow)
            {
                List<Monomial<E>> candidateList = new List();
                if (iRow < nPivotRows)
                {
                    E cf = aMatrix.rows[iRow].Coefficient(iRow);
                    candidateList.Add(new Monomial(hMonomials[columnsRearrangement[iRow]], cf));
                }

                SparseArray<E> row = iRow < nPivotRows ? bMatrix.rows[iRow] : dMatrix.rows[iRow - nPivotRows];
                for (int i = 0; i < row.sparsePositions.Length; i++)
                {
                    E val = row.sparseValues[i];
                    if (!ring.IsZero(val))
                        candidateList.Add(
                            new Monomial(hMonomials[columnsRearrangement[nPivotRows + row.sparsePositions[i]]], val));
                }

                for (int i = 0; i < bDenseColumns.Length; i++)
                {
                    E val = row.denseValues[i];
                    if (!ring.IsZero(val))
                        candidateList.Add(new Monomial(hMonomials[columnsRearrangement[nPivotRows + bDenseColumns[i]]],
                            val));
                }

                candidateList.Sort(reverseOrder);
                if (!candidateList.IsEmpty())
                {
                    ArrayBasedPoly<Monomial<E>> poly = new ArrayBasedPoly(mAlgebra,
                        candidateList.ToArray(mAlgebra.CreateArray(candidateList.Count)), factory.nVariables);
                    poly.Canonical();
                    nPolynomials.Add(poly);
                }
            }

            nPolynomials.Sort((a, b) => reverseOrder.Compare(a.Lt(), b.Lt()));

            // resulting N+ set
            IList<ArrayBasedPoly<Monomial<E>>> nPlusPolynomials = new List();
            foreach (ArrayBasedPoly<Monomial<E>> candidate in nPolynomials)
            {
                if (!hLeadMonomials.Contains(candidate.Lt()))
                {
                    // lt is new -> just add
                    nPlusPolynomials.Add(candidate);
                    f4reductions.Add(new List(Collections.SingletonList(candidate)));
                }
                else

                    // update f4reductions
                    for (int iIndex = 0; iIndex < basis.Count; ++iIndex)
                    {
                        ArrayBasedPoly<Monomial<E>> g = basis[iIndex];
                        if (g == null)
                            continue;
                        if (candidate.Lt().DvDivisibleBy(g.Lt()))
                        {
                            IList<ArrayBasedPoly<Monomial<E>>> reductions = f4reductions[iIndex];
                            bool reduced = false;
                            for (int i = 0; i < reductions.Count; ++i)
                            {
                                ArrayBasedPoly<Monomial<E>> red = reductions[i];
                                if (red.Lt().DvEquals(candidate.Lt()))
                                {
                                    reductions[i] = candidate;
                                    reduced = true;
                                    break;
                                }
                            }

                            if (!reduced)
                                reductions.Add(candidate);
                        }
                    }
            }

            return nPlusPolynomials;
        }


        sealed class SparseArray<E>
        {
            readonly Ring<E> ring;
            readonly int length;
            readonly int[] densePositions;
            readonly E[] denseValues;
            int[] sparsePositions;
            E[] sparseValues;

            SparseArray(Ring<E> ring, int length, int[] densePositions, int[] sparsePositions, E[] denseValues,
                E[] sparseValues)
            {
                this.ring = ring;
                this.Length = length;
                this.densePositions = densePositions;
                this.sparsePositions = sparsePositions;
                this.denseValues = denseValues;
                this.sparseValues = sparseValues;
            }

            SparseArray(Ring<E> ring, int[] densePositions, E[] denseArray)
            {
                this.ring = ring;
                this.densePositions = densePositions;
                this.Length = denseArray.Length;
                this.denseValues = ring.CreateArray(densePositions.Length);
                for (int i = 0; i < densePositions.Length; i++)
                    denseValues[i] = denseArray[densePositions[i]];
                TIntArrayList sparsePositions = new TIntArrayList();
                List<E> sparseValues = new List();
                for (int i = 0; i < denseArray.Length; ++i)
                {
                    if (ring.IsZero(denseArray[i]))
                        continue;
                    if (Arrays.BinarySearch(densePositions, i) >= 0)
                        continue;
                    sparsePositions.Add(i);
                    sparseValues.Add(denseArray[i]);
                }

                this.sparsePositions = sparsePositions.ToArray();
                this.sparseValues = sparseValues.ToArray(ring.CreateArray(sparseValues.Count));
            }

            E[] ToDense()
            {
                E[] result = ring.CreateZeroesArray(length);
                for (int i = 0; i < densePositions.Length; ++i)
                    result[densePositions[i]] = denseValues[i];
                for (int i = 0; i < sparsePositions.Length; ++i)
                    result[sparsePositions[i]] = sparseValues[i];
                return result;
            }

            E NormMax()
            {
                E el = null;
                foreach (E v in denseValues)
                {
                    v = ring.Abs(v);
                    if (el == null || ring.Compare(v, el) > 0)
                        el = v;
                }

                foreach (E v in sparseValues)
                {
                    v = ring.Abs(v);
                    if (el == null || ring.Compare(v, el) > 0)
                        el = v;
                }

                return el;
            }

            void Subtract(SparseArray<E> pivot, E factor, int iColumn, int firstDense)
            {
                if (ring.IsZero(factor))
                    return;
                E negFactor = ring.Negate(factor);

                // adding dense parts
                for (int i = firstDense; i < denseValues.Length; i++)
                    denseValues[i] = ring.Subtract(denseValues[i], ring.Multiply(factor, pivot.denseValues[i]));

                // subtracting sparse parts
                int[] pivCols = pivot.sparsePositions;
                E[] pivVals = pivot.sparseValues;
                int firstSparse = ArraysUtil.BinarySearch1(sparsePositions, iColumn);

                // resulting non-zero columns
                TIntArrayList resCols = new TIntArrayList(sparsePositions.Length + pivCols.Length);
                List<E> resVals = new List(sparsePositions.Length + pivCols.Length);
                resCols.Add(sparsePositions, 0, firstSparse);
                resVals.AddAll(Arrays.AsList(sparseValues).SubList(0, firstSparse));
                int iSel = firstSparse, iPiv = 0;
                while (iSel < sparsePositions.Length && iPiv < pivCols.Length)
                {
                    int selCol = sparsePositions[iSel], othCol = pivCols[iPiv];
                    if (selCol == othCol)
                    {
                        E subtract = ring.Subtract(sparseValues[iSel], ring.Multiply(factor, pivVals[iPiv]));
                        if (!ring.IsZero(subtract))
                        {
                            resCols.Add(selCol);
                            resVals.Add(subtract);
                        }

                        ++iSel;
                        ++iPiv;
                    }
                    else if (selCol < othCol)
                    {
                        resCols.Add(selCol);
                        resVals.Add(sparseValues[iSel]);
                        ++iSel;
                    }
                    else if (selCol > othCol)
                    {
                        resCols.Add(othCol);
                        resVals.Add(ring.Multiply(negFactor, pivVals[iPiv]));
                        ++iPiv;
                    }
                }

                if (iSel < sparsePositions.Length)
                    for (; iSel < sparsePositions.Length; ++iSel)
                    {
                        resCols.Add(sparsePositions[iSel]);
                        resVals.Add(sparseValues[iSel]);
                    }

                if (iPiv < pivCols.Length)
                    for (; iPiv < pivCols.Length; ++iPiv)
                    {
                        resCols.Add(pivCols[iPiv]);
                        resVals.Add(ring.Multiply(negFactor, pivVals[iPiv]));
                    }

                sparsePositions = resCols.ToArray();
                sparseValues = resVals.ToArray(ring.CreateArray(resVals.Count));
            }

            // adding dense parts
            // subtracting sparse parts
            // resulting non-zero columns
            void Subtract(SparseArray<E> pivot, E factor)
            {
                Subtract(pivot, factor, 0, 0);
            }

            // adding dense parts
            // subtracting sparse parts
            // resulting non-zero columns
            void Multiply(E factor)
            {
                if (ring.IsOne(factor))
                    return;
                for (int i = 0; i < sparseValues.Length; ++i)
                    sparseValues[i] = ring.Multiply(sparseValues[i], factor);
                for (int i = 0; i < denseValues.Length; ++i)
                    denseValues[i] = ring.Multiply(denseValues[i], factor);
            }

            // adding dense parts
            // subtracting sparse parts
            // resulting non-zero columns
            void DivideExact(E factor)
            {
                if (ring.IsOne(factor))
                    return;
                for (int i = 0; i < sparseValues.Length; ++i)
                    sparseValues[i] = ring.DivideExact(sparseValues[i], factor);
                for (int i = 0; i < denseValues.Length; ++i)
                    denseValues[i] = ring.DivideExact(denseValues[i], factor);
            }

            // adding dense parts
            // subtracting sparse parts
            // resulting non-zero columns
            E Content(E el)
            {
                if (ring.IsField())
                    return ring.GetOne();
                List<E> els = new List();
                els.Add(el);
                els.AddAll(Arrays.AsList(sparseValues));
                els.AddAll(Arrays.AsList(denseValues));
                E gcd = ring.Gcd(els);
                if (ring.IsZero(gcd))
                    return ring.GetOne();
                return gcd;
            }

            // adding dense parts
            // subtracting sparse parts
            // resulting non-zero columns
            E Content()
            {
                return Content(ring.GetZero());
            }

            // adding dense parts
            // subtracting sparse parts
            // resulting non-zero columns
            void PrimitivePart()
            {
                DivideExact(Content());
            }

            // adding dense parts
            // subtracting sparse parts
            // resulting non-zero columns
            E Coefficient(int iRow)
            {
                int index = Arrays.BinarySearch(sparsePositions, iRow);
                if (index >= 0)
                    return sparseValues[index];
                index = Arrays.BinarySearch(densePositions, iRow);
                if (index >= 0)
                    return denseValues[index];
                return ring.GetZero();
            }

            // adding dense parts
            // subtracting sparse parts
            // resulting non-zero columns
            void Multiply(int iRow, E value)
            {
                int index = Arrays.BinarySearch(sparsePositions, iRow);
                if (index >= 0)
                    sparseValues[index] = ring.Multiply(sparseValues[index], value);
                else
                {
                    index = Arrays.BinarySearch(densePositions, iRow);
                    if (index >= 0)
                        denseValues[index] = ring.Multiply(denseValues[index], value);
                }
            }

            // adding dense parts
            // subtracting sparse parts
            // resulting non-zero columns
            void Subtract(int iRow, E value)
            {
                if (ring.IsZero(value))
                    return;
                int index = Arrays.BinarySearch(densePositions, iRow);
                if (index >= 0)
                    denseValues[index] = ring.Subtract(denseValues[index], value);
                else
                {
                    index = Arrays.BinarySearch(sparsePositions, iRow);
                    if (index >= 0)
                    {
                        sparseValues[index] = ring.Subtract(sparseValues[index], value);
                        if (ring.IsZero(sparseValues[index]))
                        {
                            sparsePositions = ArraysUtil.Remove(sparsePositions, index);
                            sparseValues = ArraysUtil.Remove(sparseValues, index);
                        }
                    }
                    else
                    {
                        index = ~index;
                        sparsePositions = ArraysUtil.Insert(sparsePositions, index, iRow);
                        sparseValues = ArraysUtil.Insert(sparseValues, index, ring.Negate(value));
                    }
                }
            }

            // adding dense parts
            // subtracting sparse parts
            // resulting non-zero columns
            int FirstNonZeroPosition()
            {
                int firstSparse = sparsePositions.Length != 0 ? sparsePositions[0] : Integer.MAX_VALUE;
                for (int i = 0; i < densePositions.Length; ++i)
                    if (!ring.IsZero(denseValues[i]))
                        return Math.Min(densePositions[i], firstSparse);
                return firstSparse;
            }
        }

        // denseArray - factor * sparseArray
        static void SubtractSparseFromDense<E>(Ring<E> ring, E[] denseArray, SparseArray<E> sparseArray, E factor)
        {
            for (int i = 0; i < sparseArray.densePositions.Length; i++)
                denseArray[sparseArray.densePositions[i]] = ring.Subtract(denseArray[sparseArray.densePositions[i]],
                    ring.Multiply(factor, sparseArray.denseValues[i]));
            for (int i = 0; i < sparseArray.sparsePositions.Length; i++)
                denseArray[sparseArray.sparsePositions[i]] = ring.Subtract(denseArray[sparseArray.sparsePositions[i]],
                    ring.Multiply(factor, sparseArray.sparseValues[i]));
        }

        // denseFactor * denseArray - sparseFactor * sparseArray
        static void SubtractSparseFromDense<E>(Ring<E> ring, E[] denseArray, E denseFactor, SparseArray<E> sparseArray,
            E sparseFactor)
        {
            if (!ring.IsOne(denseFactor))
                for (int i = 0; i < denseArray.Length; i++)
                    denseArray[i] = ring.Multiply(denseArray[i], denseFactor);
            for (int i = 0; i < sparseArray.densePositions.Length; i++)
            {
                int dPosition = sparseArray.densePositions[i];
                denseArray[dPosition] = ring.Subtract(denseArray[dPosition],
                    ring.Multiply(sparseFactor, sparseArray.denseValues[i]));
            }

            for (int i = 0; i < sparseArray.sparsePositions.Length; i++)
            {
                int sPosition = sparseArray.sparsePositions[i];
                denseArray[sPosition] = ring.Subtract(denseArray[sPosition],
                    ring.Multiply(sparseFactor, sparseArray.sparseValues[i]));
            }
        }

        sealed class SparseColumnMatrix<E>
        {
            readonly Ring<E> ring;
            readonly int nRows, nColumns;
            readonly int[] densePositions;
            readonly SparseArray<E>[] columns;

            SparseColumnMatrix(Ring<E> ring, int nRows, int nColumns, int[] densePositions)
            {
                this.ring = ring;
                this.nRows = nRows;
                this.nColumns = nColumns;
                this.densePositions = densePositions;
                this.columns = new SparseArray[nColumns];
            }

            void MultiplyRow(int iRow, E value)
            {
                if (!ring.IsOne(value))
                    for (int i = 0; i < nColumns; ++i)
                        columns[i].Multiply(iRow, value);
            }

            SparseRowMatrix<E> ToRowMatrix(int[] densePositions)
            {
                SparseRowMatrix<E> rowMatrix = new SparseRowMatrix(ring, nRows, nColumns, densePositions);
                TIntArrayList[] sparseColumns = new TIntArrayList[nRows];
                List<E>[] sparseValues = new List[nRows];
                E[,] denseValues = ring.CreateArray2d(nRows, densePositions.Length);
                for (int iRow = 0; iRow < nRows; ++iRow)
                {
                    sparseColumns[iRow] = new TIntArrayList();
                    sparseValues[iRow] = new List();
                }

                for (int iColumn = 0; iColumn < nColumns; ++iColumn)
                {
                    SparseArray<E> column = columns[iColumn];
                    int iDenseColumn = Arrays.BinarySearch(densePositions, iColumn);
                    if (iDenseColumn >= 0)
                    {
                        for (int i = 0; i < column.densePositions.Length; i++)
                            denseValues[column.densePositions[i]][iDenseColumn] = column.denseValues[i];
                        for (int i = 0; i < column.sparsePositions.Length; ++i)
                            denseValues[column.sparsePositions[i]][iDenseColumn] = column.sparseValues[i];
                    }
                    else
                    {
                        for (int i = 0; i < column.densePositions.Length; i++)
                        {
                            sparseColumns[column.densePositions[i]].Add(iColumn);
                            sparseValues[column.densePositions[i]].Add(column.denseValues[i]);
                        }

                        for (int i = 0; i < column.sparsePositions.Length; ++i)
                        {
                            sparseColumns[column.sparsePositions[i]].Add(iColumn);
                            sparseValues[column.sparsePositions[i]].Add(column.sparseValues[i]);
                        }
                    }
                }

                for (int iRow = 0; iRow < nRows; ++iRow)
                    rowMatrix.rows[iRow] = new SparseArray(ring, nColumns, densePositions,
                        sparseColumns[iRow].ToArray(), denseValues[iRow],
                        sparseValues[iRow].ToArray(ring.CreateArray(sparseValues[iRow].Count)));
                return rowMatrix;
            }
        }

        sealed class SparseRowMatrix<E>
        {
            readonly Ring<E> ring;
            readonly int nRows, nColumns;
            readonly int[] densePositions;
            readonly SparseArray<E>[] rows;

            SparseRowMatrix(Ring<E> ring, int nRows, int nColumns, int[] densePositions)
            {
                this.ring = ring;
                this.nRows = nRows;
                this.nColumns = nColumns;
                this.densePositions = densePositions;
                this.rows = new SparseArray[nRows];
            }

            SparseRowMatrix(Ring<E> ring, int nRows, int nColumns, int[] densePositions, SparseArray<E>[] rows)
            {
                this.ring = ring;
                this.nRows = nRows;
                this.nColumns = nColumns;
                this.densePositions = densePositions;
                this.rows = rows;
            }

            SparseRowMatrix<E> Range(int from, int to)
            {
                return new SparseRowMatrix(ring, to - from, nColumns, densePositions,
                    Arrays.CopyOfRange(rows, from, to));
            }

            E[][] DenseMatrix()
            {
                E[,] result = ring.CreateZeroesArray2d(rows.Length, nColumns);
                for (int iRow = 0; iRow < rows.Length; ++iRow)
                {
                    SparseArray<E> row = rows[iRow];
                    for (int i = 0; i < row.sparsePositions.Length; i++)
                        result[iRow][row.sparsePositions[i]] = row.sparseValues[i];
                    for (int i = 0; i < densePositions.Length; i++)
                        result[iRow][densePositions[i]] = row.denseValues[i];
                }

                return result;
            }

            // return rank
            int RowReduce()
            {
                // Gaussian elimination
                int nZeroRows = 0;
                for (int iCol = 0, to = rows.length; iCol < to; ++iCol)
                {
                    int iRow = iCol - nZeroRows;
                    int pivotIndex = -1;
                    bool isUnit = false;
                    for (int i = iRow; i < rows.Length; ++i)
                    {
                        if (ring.IsZero(rows[i].Coefficient(iCol)))
                            continue;
                        bool iu = ring.IsUnit(rows[i].Coefficient(iRow));
                        if (pivotIndex == -1)
                        {
                            pivotIndex = i;
                            continue;
                        }

                        if (!isUnit && iu)
                        {
                            pivotIndex = i;
                            isUnit = true;
                        }
                        else if ((isUnit == iu) &&
                                 rows[i].sparsePositions.Length < rows[pivotIndex].sparsePositions.Length)
                            pivotIndex = i;
                    }

                    if (pivotIndex == -1)
                    {
                        ++nZeroRows;
                        to = Math.Min(nColumns, rows.Length + nZeroRows);
                        continue;
                    }

                    ArraysUtil.Swap(rows, pivotIndex, iRow);
                    SparseArray<E> pivot = rows[iRow];
                    E diagonalValue = pivot.Coefficient(iCol);

                    // row-reduction
                    if (ring.IsField())
                        pivot.Multiply(ring.Reciprocal(diagonalValue));
                    else
                        pivot.PrimitivePart();
                    int firstDense = ArraysUtil.BinarySearch1(densePositions, iCol);
                    for (int row = 0; row < rows.Length; ++row)
                    {
                        if (row == iRow)
                            continue;
                        E value = rows[row].Coefficient(iCol);
                        if (ring.IsZero(value))
                            continue;
                        if (ring.IsField())
                            rows[row].Subtract(pivot, value, iCol, firstDense);
                        else
                        {
                            E lcm = ring.Lcm(diagonalValue, value);
                            rows[row].Multiply(ring.DivideExact(lcm, value));
                            rows[row].Subtract(pivot, ring.DivideExact(lcm, diagonalValue), iCol, firstDense);
                            rows[row].PrimitivePart();
                        }
                    }
                }

                return Math.Min(nRows, nColumns) - nZeroRows;
            }
        }

        private static bool IsSorted(int[] arr)
        {
            for (int i = 1; i < arr.Length; ++i)
                if (arr[i - 1] >= arr[i])
                    return false;
            return true;
        }

        /* ************************************** Hilbert-Poincare series ********************************************** */


        public static bool IsMonomialIdeal(IList<TWildcardTodoAMultivariatePolynomial> ideal)
        {
            return ideal.Stream().AllMatch(AMultivariatePolynomial.IsMonomial());
        }


        public static bool IsHomogeneousIdeal(IList<TWildcardTodoAMultivariatePolynomial> ideal)
        {
            return ideal.Stream().AllMatch(AMultivariatePolynomial.IsHomogeneous());
        }


        public static IList<DegreeVector> LeadTermsSet(IList<TWildcardTodoAMultivariatePolynomial> ideal)
        {
            return ideal.Stream().Map(AMultivariatePolynomial.Lt()).Collect(Collectors.ToList());
        }


        public static HilbertSeries HilbertSeriesOfLeadingTermsSet(IList<TWildcardTodoAMultivariatePolynomial> ideal)
        {
            if (!IsHomogeneousIdeal(ideal) && !IsGradedOrder(ideal[0].ordering))
                throw new ArgumentException(
                    "Basis should be homogeneous or use graded (degree compatible) monomial order");
            if (ideal.Stream().AnyMatch(IPolynomial.IsZero()))
                return new HilbertSeries(UnivariatePolynomial.One(Q), ideal[0].nVariables);
            return HilbertSeries(LeadTermsSet(ideal));
        }


        static void ReduceMonomialIdeal(IList<DegreeVector> basis)
        {
            outer:
            for (int i = basis.size() - 1; i >= 1; --i)
            {
                for (int j = i - 1; j >= 0; --j)
                {
                    DegreeVector pi = basis[i], pj = basis[j];
                    if (pi.DvDivisibleBy(pj))
                    {
                        basis.Remove(i);
                        continue;
                    }

                    if (pj.DvDivisibleBy(pi))
                    {
                        basis.Remove(j);
                        --i;
                        continue;
                    }
                }
            }
        }


        public static HilbertSeries HilbertSeries(IList<DegreeVector> ideal)
        {
            ideal = new List(ideal);
            ReduceMonomialIdeal(ideal);
            UnivariatePolynomial<Rational<BigInteger>> initialNumerator =
                HilbertSeriesNumerator(ideal).MapCoefficients(Q, (c) => new Rational(Z, c));
            return new HilbertSeries(initialNumerator, ideal[0].NVariables());
        }


        private static UnivariatePolynomial<BigInteger> HilbertSeriesNumerator(IList<DegreeVector> ideal)
        {
            UnivariateRing<UnivariatePolynomial<BigInteger>> uniRing = Rings.UnivariateRing(Z);
            if (ideal.IsEmpty())

                // zero ideal
                return uniRing.GetOne();
            if (ideal.Count == 1 && ideal[0].IsZeroVector())

                // ideal = ring
                return uniRing.GetZero();
            if (ideal.Stream().AllMatch((m) => m.totalDegree == 1))

                // plain variables
                return uniRing.Pow(uniRing.GetOne().Subtract(uniRing.Variable(0)), ideal.Count);

            // pick monomial
            DegreeVector pivotMonomial = ideal.Stream().Filter((m) => m.totalDegree > 1).FindAny().Get();
            int nVariables = pivotMonomial.exponents.Length, var;
            for (var = 0; var < nVariables; ++var)
                if (pivotMonomial.exponents[var] != 0)
                    break;
            int variable = var;
            int[] varExponents = new int[nVariables];
            varExponents[variable] = 1;
            DegreeVector varMonomial = new DegreeVector(varExponents, 1);

            // sum J + <x_i>
            IList<DegreeVector> sum = ideal.Stream().Filter((m) => m.exponents[variable] == 0)
                .Collect(Collectors.ToList());
            sum.Add(varMonomial);

            // quotient J : <x_i>
            IList<DegreeVector> quot = ideal.Stream()
                .Map((m) => m.exponents[variable] > 0 ? m.DvDivideOrNull(variable, 1) : m).Collect(Collectors.ToList());
            ReduceMonomialIdeal(quot);
            return HilbertSeriesNumerator(sum).Add(HilbertSeriesNumerator(quot).ShiftRight(1));
        }


        public sealed class HilbertSeries
        {
            private static readonly UnivariatePolynomial<Rational<BigInteger>> DENOMINATOR =
                UnivariatePolynomial.Create(1, -1).MapCoefficients(Q, (c) => new Rational(Z, c));


            public readonly UnivariatePolynomial<Rational<BigInteger>> initialNumerator;


            public readonly int initialDenominatorExponent;


            public readonly UnivariatePolynomial<Rational<BigInteger>> numerator;


            public readonly int denominatorExponent;

            private HilbertSeries(UnivariatePolynomial<Rational<BigInteger>> initialNumerator,
                int initialDenominatorExponent)
            {
                this.initialNumerator = initialNumerator;
                this.initialDenominatorExponent = initialDenominatorExponent;
                UnivariatePolynomial<Rational<BigInteger>> reducedNumerator = initialNumerator;
                int reducedDenominatorDegree = initialDenominatorExponent;
                while (!initialNumerator.IsZero())
                {
                    UnivariatePolynomial<Rational<BigInteger>> div =
                        UnivariateDivision.DivideOrNull(reducedNumerator, DENOMINATOR, true);
                    if (div == null)
                        break;
                    reducedNumerator = div;
                    --reducedDenominatorDegree;
                }

                this.numerator = reducedNumerator;
                this.denominatorExponent = reducedDenominatorDegree;
            }


            public int Dimension()
            {
                return denominatorExponent;
            }


            private int idealDegree = -1;


            public int Degree()
            {
                lock (this)
                {
                    if (idealDegree == -1)
                    {
                        Rational<BigInteger> degree = numerator.Evaluate(1);
                        idealDegree = degree.Numerator().IntValueExact();
                    }

                    return idealDegree;
                }
            }


            private UnivariatePolynomial<Rational<BigInteger>> integralPart = null;


            private UnivariatePolynomial<Rational<BigInteger>> remainderNumerator = null;


            public UnivariatePolynomial<Rational<BigInteger>> IntegralPart()
            {
                ComputeIntegralAndRemainder();
                return integralPart;
            }


            public UnivariatePolynomial<Rational<BigInteger>> RemainderNumerator()
            {
                ComputeIntegralAndRemainder();
                return remainderNumerator;
            }

            private void ComputeIntegralAndRemainder()
            {
                lock (this)
                {
                    if (integralPart == null)
                    {
                        UnivariatePolynomial<Rational<BigInteger>>[] divRem =
                            UnivariateDivision.DivideAndRemainder(numerator,
                                UnivariatePolynomialArithmetic.PolyPow(DENOMINATOR, denominatorExponent, true), true);
                        this.integralPart = divRem[0];
                        this.remainderNumerator = divRem[1];
                    }
                }
            }


            private UnivariatePolynomial<Rational<BigInteger>> hilbertPolynomialZ = null;


            public UnivariatePolynomial<Rational<BigInteger>> HilbertPolynomialZ()
            {
                lock (this)
                {
                    if (hilbertPolynomialZ == null)
                    {
                        hilbertPolynomialZ = UnivariatePolynomial.Zero(Q);
                        UnivariatePolynomial<Rational<BigInteger>> var = UnivariatePolynomial.One(Q).ShiftRight(1);
                        for (int i = 0; i <= numerator.Degree(); ++i)
                        {
                            UnivariatePolynomial<Rational<BigInteger>> term = UnivariatePolynomial.One(Q);
                            for (int j = 0; j < (denominatorExponent - 1); ++j)
                                term.Multiply(var.Clone().Add(Q.ValueOf(-i + 1 + j)));
                            term.Multiply(numerator[i]);
                            hilbertPolynomialZ.Add(term);
                        }
                    }

                    return hilbertPolynomialZ;
                }
            }


            private UnivariatePolynomial<Rational<BigInteger>> hilbertPolynomial = null;


            public UnivariatePolynomial<Rational<BigInteger>> HilbertPolynomial()
            {
                lock (this)
                {
                    if (hilbertPolynomial == null)
                    {
                        Rational<BigInteger> facCf = new Rational(Z, BigIntegerUtil.Factorial(denominatorExponent - 1));
                        hilbertPolynomial = HilbertPolynomialZ().Clone().DivideExact(facCf);
                    }

                    return hilbertPolynomial;
                }
            }

            public bool Equals(object o)
            {
                if (this == o)
                    return true;
                if (o == null || GetType() != o.GetType())
                    return false;
                HilbertSeries that = (HilbertSeries)o;
                if (denominatorExponent != that.denominatorExponent)
                    return false;
                return numerator.Equals(that.numerator);
            }

            public int GetHashCode()
            {
                int result = numerator.GetHashCode();
                result = 31 * result + denominatorExponent;
                return result;
            }

            public string ToString()
            {
                return String.Format("(%s) / (%s)^%s", numerator, DENOMINATOR, denominatorExponent);
            }
        }

        /* ********************************** Modular & Sparse Groebner basis ****************************************** */


        public static GBResult<Monomial<BigInteger>, MultivariatePolynomial<BigInteger>> ModularGB(
            IList<MultivariatePolynomial<BigInteger>> ideal, Comparator<DegreeVector> monomialOrder)
        {
            return ModularGB(ideal, monomialOrder, null, false);
        }


        public static GBResult<Monomial<BigInteger>, MultivariatePolynomial<BigInteger>> ModularGB(
            IList<MultivariatePolynomial<BigInteger>> ideal, Comparator<DegreeVector> monomialOrder,
            HilbertSeries hilbertSeries)
        {
            return ModularGB(ideal, monomialOrder, hilbertSeries, false);
        }


        public static GBResult<Monomial<BigInteger>, MultivariatePolynomial<BigInteger>> ModularGB(
            IList<MultivariatePolynomial<BigInteger>> ideal, Comparator<DegreeVector> monomialOrder,
            HilbertSeries hilbertSeries, bool trySparse)
        {
            return ModularGB(ideal, monomialOrder, GroebnerBases.GroebnerBasisInGF(),
                (p, o, s) => GroebnerBasisInZ(p, o, s, false), BigInteger.ONE.ShiftLeft(59), hilbertSeries, trySparse);
        }


        private static readonly int N_UNKNOWNS_THRESHOLD = 32, N_BUCHBERGER_STEPS_THRESHOLD = 2 * 32;


        private static readonly int N_MOD_STEPS_THRESHOLD = 4, N_BUCHBERGER_STEPS_REDUNDANCY_DELTA = 9;

        static IList<MultivariatePolynomialZp64> Mod(IList<MultivariatePolynomial<BigInteger>> polys, long modulus)
        {
            IntegersZp64 ring = Zp64(modulus);
            IntegersZp gRing = ring.AsGenericRing();
            return polys.Stream().Map((p) => MultivariatePolynomial.AsOverZp64(p.SetRing(gRing)))
                .Collect(Collectors.ToList());
        }


        public static GBResult<Monomial<BigInteger>, MultivariatePolynomial<BigInteger>> ModularGB(
            IList<MultivariatePolynomial<BigInteger>> ideal, Comparator<DegreeVector> monomialOrder,
            GroebnerAlgorithm modularAlgorithm, GroebnerAlgorithm defaultAlgorithm, BigInteger firstPrime,
            HilbertSeries hilbertSeries, bool trySparse)
        {
            // simplify generators as much as possible
            ideal = PrepareGenerators(ideal, monomialOrder);
            if (ideal.Count == 1)
                return GBResult.Trivial(Canonicalize(ideal));
            GBResult baseResult = null;
            BigInteger basePrime = null;
            HilbertSeries baseSeries = hilbertSeries;
            IList<MultivariatePolynomial<BigInteger>> baseBasis = null;
            PrimesIterator0 primes = new PrimesIterator0(firstPrime); // start with a large enough prime
            IList<MultivariatePolynomial<BigInteger>> previousGBCandidate = null;

            // number of CRT liftings
            int nModIterations = 0;
            main:
            while (true)
            {
                // check whether we dont' take too long
                if (baseResult != null && baseResult.IsBuchbergerType() && nModIterations > N_MOD_STEPS_THRESHOLD &&
                    Math.Abs(baseResult.nProcessedPolynomials - baseResult.nZeroReductions - baseBasis.Count) <
                    N_BUCHBERGER_STEPS_REDUNDANCY_DELTA)
                {
                    // modular reconstruction is too hard in this case, switch to the non-modular algorithm
                    return defaultAlgorithm.GroebnerBasis(ideal, monomialOrder, hilbertSeries);
                }


                // pick up next prime number
                BigInteger prime = primes.Next();
                IntegersZp ring = Zp(prime);

                // number of traditional "Buchberger" steps
                IList<MultivariatePolynomial<BigInteger>> bModBasis;
                GBResult modResult;
                if (prime.IsLong())
                {
                    // generators mod prime
                    IList<MultivariatePolynomialZp64> modGenerators = Mod(ideal, prime.LongValueExact());

                    // Groebner basis mod prime
                    GBResult<MonomialZp64, MultivariatePolynomialZp64> r =
                        modularAlgorithm.GroebnerBasis((IList)modGenerators, monomialOrder, null);
                    bModBasis = r.Stream().Map(MultivariatePolynomialZp64.ToBigPoly()).Collect(Collectors.ToList());
                    modResult = r;
                }
                else
                {
                    IList<MultivariatePolynomial<BigInteger>> modGenerators =
                        ideal.Stream().Map((p) => p.SetRing(ring)).Collect(Collectors.ToList());
                    GBResult<Monomial<BigInteger>, MultivariatePolynomial<BigInteger>> r =
                        modularAlgorithm.GroebnerBasis((IList)modGenerators, monomialOrder, null);
                    bModBasis = r.list;
                    modResult = r;
                }


                // sort generators by increasing lead terms
                bModBasis.Sort((a, b) => monomialOrder.Compare(a.Lt(), b.Lt()));
                HilbertSeries modSeries = HilbertSeries(LeadTermsSet(bModBasis));

                // first iteration
                if (baseBasis == null)
                {
                    if (trySparse)
                    {
                        // try to solve sparse GB on the first iteration
                        // number of unknowns
                        int nSparseUnknowns = bModBasis.Stream().MapToInt((p) => p.Count - 1).Sum();

                        // try to solve on in most simple cases
                        if (nSparseUnknowns < N_UNKNOWNS_THRESHOLD &&
                            modResult.nProcessedPolynomials > N_BUCHBERGER_STEPS_THRESHOLD)
                        {
                            IList<MultivariatePolynomial<BigInteger>> solvedGB = SolveGB(ideal,
                                bModBasis.Stream().Map(AMultivariatePolynomial.GetSkeleton())
                                    .Collect(Collectors.ToList()), monomialOrder);
                            if (solvedGB != null && IsGroebnerBasis(ideal, solvedGB, monomialOrder))
                                return GBResult.NotBuchberger(solvedGB);
                        }
                    }

                    baseBasis = bModBasis;
                    basePrime = prime;
                    baseSeries = modSeries;
                    baseResult = modResult;
                    nModIterations = 0;
                    continue;
                }


                // check for Hilbert luckiness
                int c = ModHilbertSeriesComparator.Compare(baseSeries, modSeries);
                if (c < 0)

                    // current prime is unlucky
                    continue;
                else if (c > 0)
                {
                    // base prime was unlucky
                    baseBasis = bModBasis;
                    basePrime = prime;
                    baseSeries = modSeries;
                    baseResult = modResult;
                    nModIterations = 0;
                    continue;
                }


                // prime is Hilbert prime, so we compare monomial sets
                for (int i = 0, size = Math.min(baseBasis.size(), bModBasis.size()); i < size; ++i)
                {
                    c = monomialOrder.Compare(baseBasis[i].Lt(), bModBasis[i].Lt());
                    if (c > 0)

                        // current prime is unlucky
                        continue;
                    else if (c < 0)
                    {
                        // base prime was unlucky
                        baseBasis = bModBasis;
                        basePrime = ring.modulus;
                        baseSeries = modSeries;
                        baseResult = modResult;
                        nModIterations = 0;
                        continue;
                    }
                }

                if (baseBasis.Count < bModBasis.Count)
                {
                    // base prime was unlucky
                    baseBasis = bModBasis;
                    basePrime = ring.modulus;
                    baseSeries = modSeries;
                    baseResult = modResult;
                    nModIterations = 0;
                    continue;
                }
                else if (baseBasis.Count > bModBasis.Count)

                    // current prime is unlucky
                    continue;

                ++nModIterations;
                ChineseRemaindersMagic<BigInteger> magic = CreateMagic(Z, basePrime, prime);

                // prime is probably lucky, we can do Chinese Remainders
                for (int iGenerator = 0; iGenerator < baseBasis.Count; ++iGenerator)
                {
                    MultivariatePolynomial<BigInteger> baseGenerator = baseBasis[iGenerator];
                    PairedIterator<Monomial<BigInteger>, MultivariatePolynomial<BigInteger>, Monomial<BigInteger>,
                        MultivariatePolynomial<BigInteger>> iterator =
                        new PairedIterator(baseGenerator, bModBasis[iGenerator]);
                    baseGenerator = baseGenerator.CreateZero();
                    while (iterator.HasNext())
                    {
                        iterator.Advance();
                        Monomial<BigInteger> baseTerm = iterator.aTerm;
                        BigInteger crt = ChineseRemainders(Z, magic, baseTerm.coefficient, iterator.bTerm.coefficient);
                        baseGenerator.Add(baseTerm.SetCoefficient(crt));
                    }

                    baseBasis[iGenerator] = baseGenerator;
                }

                basePrime = basePrime.Multiply(prime);
                IList<MultivariatePolynomial<Rational<BigInteger>>> gbCandidateFrac = new List();
                foreach (MultivariatePolynomial<BigInteger> gen in baseBasis)
                {
                    MultivariatePolynomial<Rational<BigInteger>> gbGen = ReconstructPoly(gen, basePrime);
                    if (gbGen == null)
                        continue;
                    gbCandidateFrac.Add(gbGen);
                }

                IList<MultivariatePolynomial<BigInteger>> gbCandidate = ToIntegral(gbCandidateFrac);
                if (gbCandidate.Equals(previousGBCandidate) && IsGroebnerBasis(ideal, gbCandidate, monomialOrder))
                    return GBResult.NotBuchberger(Canonicalize(gbCandidate));
                previousGBCandidate = gbCandidate;
            }
        }

        private sealed class PrimesIterator0
        {
            private BigInteger from;
            private PrimesIterator base;

            PrimesIterator0(BigInteger from)
            {
                this.from = from;
                this.@base = from.IsLong() ? new PrimesIterator(from.LongValueExact()) : null;
            }

            BigInteger Next()
            {
                if (@base == null)
                    return (from = from.NextProbablePrime());
                long l = @base.Take();
                if (l == -1)
                {
                    @base = null;
                    return Next();
                }

                return BigInteger.ValueOf(l);
            }
        }


        private static MultivariatePolynomial<Rational<BigInteger>> ReconstructPoly(
            MultivariatePolynomial<BigInteger> @base, BigInteger prime)
        {
            MultivariatePolynomial<Rational<BigInteger>> result =
                MultivariatePolynomial.Zero(@base.nVariables, Q, @base.ordering);
            foreach (Monomial<BigInteger> term in @base)
            {
                BigInteger[] numDen = RationalReconstruction.ReconstructFareyErrorTolerant(term.coefficient, prime);
                if (numDen == null)
                    return null;
                result.Add(new Monomial(term, new Rational(Rings.Z, numDen[0], numDen[1])));
            }

            return result;
        }

        private static Comparator<HilbertSeries> ModHilbertSeriesComparator = (a, b) =>
        {
            UnivariatePolynomial<Rational<BigInteger>> aHilbert = a.HilbertPolynomial(),
                bHilbert = b.HilbertPolynomial();
            if (aHilbert.Equals(bHilbert))
                return 0;
            for (int n = Math.max(a.numerator.degree(), b.numerator.degree()) + 1;; ++n)
            {
                int c = aHilbert.Evaluate(n).CompareTo(bHilbert.Evaluate(n));
                if (c != 0)
                    return c;
            }
        };


        public static IList<Poly> SolveGB<Term extends AMonomial<Term>, Poly
            extends AMultivariatePolynomial<Term, Poly>>(IList<Poly> generators, IList<Collection<DegreeVector>>
            gbSkeleton, Comparator<DegreeVector> monomialOrder)
        {
            return SolveGB0(generators, gbSkeleton.Stream().Map((c) =>
            {
                TreeSet<DegreeVector> set = new TreeSet(monomialOrder);
                set.AddAll(c);
                return set;
            }).Collect(Collectors.ToList()), monomialOrder);
        }


        private static readonly int DROP_SOLVED_VARIABLES_THRESHOLD = 128;


        private static readonly double DROP_SOLVED_VARIABLES_RELATIVE_THRESHOLD = 0.1;
        private static IList<Poly> SolveGB0<Term extends AMonomial<Term>, Poly
            extends AMultivariatePolynomial<Term, Poly>>(IList<Poly> generators, IList<SortedSet<DegreeVector>>
            gbSkeleton, Comparator<DegreeVector> monomialOrder)
        {
            Poly factory = generators[0].CreateZero();
            if (!factory.IsOverField())
            {
                IList gb = SolveGB0(ToFractions((IList)generators), gbSkeleton, monomialOrder);
                if (gb == null)
                    return null;
                return Canonicalize((IList<Poly>)ToIntegral(gb));
            }


            // total number of unknowns
            int nUnknowns = gbSkeleton.Stream().MapToInt((p) => p.Count - 1).Sum();

            // we consider polynomials as R[u0, u1, ..., uM][x0, ..., xN], where u_i are unknowns
            // ring R[u0, u1, ..., uM]
            MultivariateRing<Poly> cfRing = Rings.MultivariateRing(factory.SetNVariables(nUnknowns));

            // ring R[u0, u1, ..., uM][x0, ..., xN]
            MultivariateRing<MultivariatePolynomial<Poly>> pRing =
                Rings.MultivariateRing(factory.nVariables, cfRing, monomialOrder);
            AtomicInteger varCounter = new AtomicInteger(0);
            MultivariatePolynomial<Poly>[] gbCandidate = gbSkeleton.Stream().Map((p) =>
            {
                MultivariatePolynomial<Poly> head = pRing.Create(p.Last());
                MultivariatePolynomial<Poly> tail = p.HeadSet(p.Last()).Stream()
                    .Map((t) => pRing.Create(t).Multiply(cfRing.Variable(varCounter.GetAndIncrement())))
                    .Reduce(pRing.GetZero(), (a, b) => a.Add(b));
                return head.Add(tail);
            }).ToArray(MultivariatePolynomial[].New());
            Term uTerm = factory.monomialAlgebra.GetUnitTerm(nUnknowns);

            // initial ideal viewed as R[u0, u1, ..., uM][x0, ..., xN]
            IList<MultivariatePolynomial<Poly>> initialIdeal = generators.Stream()
                .Map((p) => pRing.Factory().Create(p.Collection().Stream()
                    .Map((t) => new Monomial(t, cfRing.Factory().Create(uTerm.SetCoefficientFrom(t))))
                    .Collect(Collectors.ToList()))).Collect(Collectors.ToList());

            // build set of all syzygies
            IList<MultivariatePolynomial<Poly>> _tmp_gb_ = new List(initialIdeal);

            // list of all non trivial S-pairs
            SyzygySet<Monomial<Poly>, MultivariatePolynomial<Poly>> sPairsSet =
                new SyzygyTreeSet(new TreeSet(DefaultSelectionStrategy(monomialOrder)));
            Arrays.Stream(gbCandidate).ForEach((gb) => UpdateBasis(_tmp_gb_, sPairsSet, gb));

            // source of equations
            IList<EqSupplier<Term, Poly>> source = new List();

            // initial ideal must reduce to zero
            initialIdeal.ForEach((p) => source.Add(new EqSupplierPoly(p)));

            // S-pairs must reduce to zero
            sPairsSet.AllPairs().ForEach((p) => source.Add(new EqSupplierSyzygy(initialIdeal, gbCandidate, p)));

            // iterate from "simplest" to "hardest" equations
            source.Sort((a, b) => monomialOrder.Compare(a.Signature(), b.Signature()));
            IList<Equation<Term, Poly>> nonLinearEquations = new List();
            EquationSolver<Term, Poly> solver = CreateSolver(factory);
            TIntHashSet solvedVariables = new TIntHashSet();
            foreach (EqSupplier<Term, Poly> eqSupplier in source)
            {
                if (solvedVariables.Count == nUnknowns)

                    // system is solved
                    return GbSolution(factory, gbCandidate);
                MultivariatePolynomial<Poly> next = eqSupplier.Poly();
                SystemInfo result = ReduceAndSolve(next, gbCandidate, solver, nonLinearEquations);
                if (result == Inconsistent)
                    return null;
                solvedVariables.AddAll(solver.solvedVariables);
                if (solver.solvedVariables.Count > 0)

                    // simplify GB candidate
                    solver.SimplifyGB(gbCandidate);

                // clear solutions (this gives some speed up)
                solver.Clear();
                if (solvedVariables.Count > DROP_SOLVED_VARIABLES_THRESHOLD && 1 * solvedVariables.Count / nUnknowns >=
                    DROP_SOLVED_VARIABLES_RELATIVE_THRESHOLD)
                {
                    DropSolvedVariables(solvedVariables, initialIdeal, gbCandidate, nonLinearEquations, solver);
                    nUnknowns -= solvedVariables.Count;
                    solvedVariables.Clear();
                }
            }

            if (solver.NSolved() == nUnknowns)

                // system is solved
                return GbSolution(factory, gbCandidate);
            return null;
        }

        interface EqSupplier<Term, Poly>
        {
            MultivariatePolynomial<Poly> Poly();
            DegreeVector Signature();
        }

        sealed class EqSupplierPoly<Term, Poly> : EqSupplier<Term, Poly>
        {
            readonly MultivariatePolynomial<Poly> poly;

            EqSupplierPoly(MultivariatePolynomial<Poly> poly)
            {
                this.poly = poly;
            }

            public MultivariatePolynomial<Poly> Poly()
            {
                return poly;
            }

            public DegreeVector Signature()
            {
                return poly.Lt();
            }
        }

        sealed class EqSupplierSyzygy<Term, Poly> : EqSupplier<Term, Poly>
        {
            readonly IList<MultivariatePolynomial<Poly>> initialIdeal;
            readonly MultivariatePolynomial<Poly>[] gbCandidate;
            readonly SyzygyPair<Monomial<Poly>, MultivariatePolynomial<Poly>> sPair;

            EqSupplierSyzygy(IList<MultivariatePolynomial<Poly>> initialIdeal,
                MultivariatePolynomial<Poly>[] gbCandidate,
                SyzygyPair<Monomial<Poly>, MultivariatePolynomial<Poly>> sPair)
            {
                this.initialIdeal = initialIdeal;
                this.gbCandidate = gbCandidate;
                this.sPair = sPair;
            }

            public MultivariatePolynomial<Poly> Poly()
            {
                return Syzygy(sPair.syzygyGamma, GetFi(sPair.i, initialIdeal, gbCandidate),
                    GetFi(sPair.j, initialIdeal, gbCandidate));
            }

            public DegreeVector Signature()
            {
                return sPair.syzygyGamma;
            }
        }

        static int[] UsedVars(AMultivariatePolynomial equation)
        {
            int[] degrees = equation.Degrees();
            TIntArrayList usedVars = new TIntArrayList();
            for (int i = 0; i < degrees.Length; ++i)
                if (degrees[i] > 0)
                    usedVars.Add(i);
            return usedVars.ToArray();
        }

        static MultivariatePolynomial<Poly> GetFi<Term extends AMonomial<Term>, Poly
            extends AMultivariatePolynomial<Term, Poly>>(int i, IList<MultivariatePolynomial<Poly>> initialIdeal,
            MultivariatePolynomial<Poly>[] gbCandidate)
        {
            return i < initialIdeal.Count ? initialIdeal[i] : gbCandidate[i - initialIdeal.Count];
        }


        static void DropSolvedVariables<Term extends AMonomial<Term>, Poly extends AMultivariatePolynomial<Term, Poly>>(
            TIntHashSet solvedVariables, IList<MultivariatePolynomial<Poly>> initialIdeal, MultivariatePolynomial<Poly>
            [] gbCandidate, IList<Equation<Term, Poly>> nonLinearEquations, EquationSolver<Term, Poly> solver)
        {
            if (solver.NSolved() != 0)
                throw new ArgumentException();
            int[] solved = solvedVariables.ToArray();
            Arrays.Sort(solved);
            MultivariateRing<Poly> oldPRing = ((MultivariateRing<Poly>)initialIdeal[0].ring);

            // new coefficient ring
            MultivariateRing<Poly> pRing = MultivariateRing(oldPRing.Factory().DropVariables(solved));

            // transform initial ideal
            initialIdeal.ReplaceAll((p) => p.MapCoefficients(pRing, (cf) => cf.DropVariables(solved)));

            // transform groebner basis
            Arrays.AsList(gbCandidate).ReplaceAll((p) => p.MapCoefficients(pRing, (cf) => cf.DropVariables(solved)));
            int[] vMapping = new int[oldPRing.NVariables()];
            int c = 0;
            for (int i = 0; i < vMapping.Length; ++i)
            {
                if (Arrays.BinarySearch(solved, i) >= 0)
                    vMapping[i] = -1;
                else
                    vMapping[i] = c++;
            }


            // transform non-linear
            nonLinearEquations.ForEach((eq) => eq.DropVariables(vMapping));

            // transform linear
            solver.equations.ForEach((eq) => eq.DropVariables(vMapping));
        }

        static IList<Poly> GbSolution<Term extends AMonomial<Term>, Poly extends AMultivariatePolynomial<Term, Poly>>(
            Poly factory, MultivariatePolynomial<Poly>[] gbCandidate)
        {
            IList<Poly> result = new List();
            foreach (MultivariatePolynomial<Poly> gbElement in gbCandidate)
            {
                if (!gbElement.Stream().AllMatch(Poly.IsConstant()))
                    return null;
                result.Add(factory.Create(gbElement.Collection().Stream()
                    .Map((t) => t.coefficient.Lt().SetDegreeVector(t)).Collect(Collectors.ToList())));
            }

            return Canonicalize(result);
        }


        static SystemInfo ReduceAndSolve<Term extends AMonomial<Term>, Poly
            extends AMultivariatePolynomial<Term, Poly>>(MultivariatePolynomial<Poly> toReduce,
            MultivariatePolynomial<Poly>[] gbCandidate, EquationSolver<Term, Poly> solver, IList<Equation<Term, Poly>>
            nonLinearEquations)
        {
            // reduce poly with GB candidate
            MultivariatePolynomial<Poly> remainder = MultivariateDivision.Remainder(toReduce, gbCandidate);
            if (remainder.IsZero())
                return Consistent;

            // previous number of solved variables
            int nSolved = solver.solvedVariables.Count;

            // whether some independent linear equations were generated
            bool newLinearEqsObtained = false;
            foreach (Poly cf in remainder.Coefficients())
            {
                if (cf.IsConstant())

                    // equation is not solvable
                    return Inconsistent;
                cf = solver.Simplify(cf).Monic();
                Equation<Term, Poly> eq = new Equation(cf);
                if (eq.isLinear)
                {
                    // add equation
                    bool isNewEq = solver.AddEquation(eq);
                    if (isNewEq)

                        // equation is new (independent)
                        newLinearEqsObtained = true;
                }
                else if (!nonLinearEquations.Contains(eq))
                    nonLinearEquations.Add(eq);
            }

            if (!newLinearEqsObtained)

                // nothing to do further
                return Consistent;
            if (solver.solvedVariables.Count > nSolved)

                // if some new solutions were already obtained we
                // can simplify non-linear equations so that
                // new linear combinations will be found
                UpdateNonLinear(solver, nonLinearEquations);
            while (true)
            {
                // try to find and solve some subset of linear equations
                SystemInfo result = solver.Solve();
                if (result == Inconsistent)
                    return Inconsistent;
                else if (result == UnderDetermined)
                    return Consistent;

                // update non-linear equations with new solutions
                UpdateNonLinear(solver, nonLinearEquations);
            }
        }

        private static void UpdateNonLinear<Term extends AMonomial<Term>, Poly
            extends AMultivariatePolynomial<Term, Poly>>(EquationSolver<Term, Poly> solver, IList<Equation<Term, Poly>>
            nonLinearEquations)
        {
            while (true)
            {
                bool nlReduced = false;
                for (int i = nonLinearEquations.size() - 1; i >= 0; --i)
                {
                    Equation<Term, Poly> eq = solver.Simplify(nonLinearEquations[i]);
                    if (eq.reducedEquation.IsZero())
                        nonLinearEquations.Remove(i);
                    else if (IsLinear(eq.reducedEquation))
                    {
                        nlReduced = true;
                        solver.AddEquation(eq);
                        nonLinearEquations.Remove(i);
                    }
                    else
                        nonLinearEquations[i] = eq;
                }

                if (!nlReduced)
                    return;
            }
        }


        static bool IsLinear(AMultivariatePolynomial<TWildcardTodoAMonomial, TWildcardTodo> poly)
        {
            return poly.Collection().Stream().AllMatch((t) => t.totalDegree <= 1);
        }


        private class Equation<Term, Poly>
        {
            readonly int[] usedVars;


            readonly TIntIntHashMap mapping;


            readonly Poly reducedEquation;


            readonly bool isLinear;


            Equation(Poly equation)
            {
                int[] degrees = equation.Degrees();
                TIntArrayList usedVars = new TIntArrayList();
                for (int i = 0; i < degrees.Length; ++i)
                    if (degrees[i] > 0)
                        usedVars.Add(i);
                this.usedVars = usedVars.ToArray();
                this.reducedEquation = equation.DropSelectVariables(this.usedVars);
                this.mapping = new TIntIntHashMap(Constants.DEFAULT_CAPACITY, Constants.DEFAULT_LOAD_FACTOR, -1, -1);
                for (int i = 0; i < this.usedVars.Length; ++i)
                    mapping.Put(this.usedVars[i], i);
                this.isLinear = IsLinear(equation);
            }


            Equation(int[] usedVars, TIntIntHashMap mapping, Poly reducedEquation)
            {
                this.usedVars = usedVars;
                this.mapping = mapping;
                this.reducedEquation = reducedEquation;
                this.isLinear = IsLinear(reducedEquation);
            }


            virtual void DropVariables(int[] vMapping)
            {
                for (int i = 0; i < usedVars.Length; ++i)
                    usedVars[i] = vMapping[usedVars[i]];
                mapping.Clear();
                for (int i = 0; i < this.usedVars.Length; ++i)
                    mapping.Put(this.usedVars[i], i);
            }


            virtual bool HasVariable(int variable)
            {
                return Arrays.BinarySearch(usedVars, variable) >= 0;
            }


            public virtual bool Equals(object o)
            {
                if (this == o)
                    return true;
                if (o == null || GetType() != o.GetType())
                    return false;
                Equation < ?, ?> equation1 = (Equation < ?,  ?>)o;
                return Arrays.Equals(usedVars, equation1.usedVars) && reducedEquation.Equals(equation1.reducedEquation);
            }


            public virtual int GetHashCode()
            {
                int result = Arrays.GetHashCode(usedVars);
                result = 31 * result + reducedEquation.GetHashCode();
                return result;
            }
        }

        static EquationSolver<Term, Poly> CreateSolver<Term extends AMonomial<Term>, Poly
            extends AMultivariatePolynomial<Term, Poly>>(Poly factory)
        {
            if (factory is MultivariatePolynomial)
                return (EquationSolver<Term, Poly>)new EquationSolverE(((MultivariatePolynomial)factory).ring);
            else
                throw new Exception();
        }


        private abstract class EquationSolver<Term, Poly>
        {
            readonly IList<Equation<Term, Poly>> equations = new List();


            readonly TIntArrayList solvedVariables = new TIntArrayList();


            EquationSolver()
            {
            }


            int NEquations()
            {
                return equations.Count;
            }


            abstract bool AddEquation(Equation<Term, Poly> eq);


            abstract SystemInfo Solve();


            abstract Equation<Term, Poly> Simplify(Equation<Term, Poly> eq);


            abstract Poly Simplify(Poly poly);


            abstract void Clear();


            MultivariatePolynomial<Poly> SimplifyGB(MultivariatePolynomial<Poly> poly)
            {
                return poly.MapCoefficients(poly.ring, this.Simplify());
            }


            void SimplifyGB(MultivariatePolynomial<Poly>[] gbCandidate)
            {
                for (int i = 0; i < gbCandidate.Length; i++)
                    gbCandidate[i] = SimplifyGB(gbCandidate[i]);
            }


            int NSolved()
            {
                return solvedVariables.Count;
            }
        }

        private sealed class EquationSolverE<E> : EquationSolver<Monomial<E>, MultivariatePolynomial<E>>
        {
            private readonly IList<E> solutions = new List();


            private readonly Ring<E> ring;


            EquationSolverE(Ring<E> ring)
            {
                this.ring = ring;
            }


            override void Clear()
            {
                solvedVariables.Clear();
                solutions.Clear();
            }


            override bool AddEquation(Equation<Monomial<E>, MultivariatePolynomial<E>> equation)
            {
                equation = Simplify(equation);
                MultivariatePolynomial<E> eq = equation.reducedEquation;
                eq.Monic();
                if (eq.IsZero())

                    // redundant equation
                    return false;
                if (equations.Contains(equation))

                    // redundant equation
                    return false;
                if (eq.nVariables == 1)
                {
                    // equation can be solved directly
                    Ring<E> ring = eq.ring;

                    // solved variable
                    int solvedVar = equation.usedVars[0];
                    solvedVariables.Add(solvedVar);
                    solutions.Add(ring.DivideExact(ring.Negate(eq.Cc()), eq.Lc()));

                    // list of equations that can be updated
                    IList<Equation<Monomial<E>, MultivariatePolynomial<E>>> needUpdate = new List();
                    for (int i = equations.size() - 1; i >= 0; --i)
                    {
                        Equation<Monomial<E>, MultivariatePolynomial<E>> oldEq = equations[i];
                        if (!oldEq.HasVariable(solvedVar))
                            continue;
                        equations.Remove(i);
                        needUpdate.Add(oldEq);
                    }

                    needUpdate.ForEach(this.AddEquation());
                    return true;
                }

                equations.Add(equation);
                return true;
            }


            private void SelfUpdate()
            {
                IList<Equation<Monomial<E>, MultivariatePolynomial<E>>> needUpdate = new List();
                for (int i = equations.size() - 1; i >= 0; --i)
                {
                    Equation<Monomial<E>, MultivariatePolynomial<E>> oldEq = equations[i];
                    Equation<Monomial<E>, MultivariatePolynomial<E>> newEq = Simplify(oldEq);
                    if (oldEq.Equals(newEq))
                        continue;
                    equations.Remove(i);
                    needUpdate.Add(newEq);
                }

                needUpdate.ForEach(this.AddEquation());
            }


            override Equation<Monomial<E>, MultivariatePolynomial<E>> Simplify(
                Equation<Monomial<E>, MultivariatePolynomial<E>> eq)
            {
                // eliminated variables
                TIntArrayList eliminated = new TIntArrayList();

                // eliminated variables in eq.reducedEquation
                TIntHashSet rEliminated = new TIntHashSet();

                // reduced equation
                MultivariatePolynomial<E> rPoly = eq.reducedEquation;
                for (int i = 0; i < solutions.Count; ++i)
                {
                    int var = solvedVariables[i];
                    if (!eq.HasVariable(var))
                        continue;
                    int rVar = eq.mapping[var];
                    eliminated.Add(var);
                    rEliminated.Add(rVar);
                    rPoly = rPoly.Evaluate(rVar, solutions[i]);
                }

                if (eliminated.IsEmpty())
                    return eq;
                eliminated.Sort();
                int[] eliminatedArray = eliminated.ToArray();
                int[] usedVars = ArraysUtil.IntSetDifference(eq.usedVars, eliminatedArray);
                TIntIntHashMap mapping = new TIntIntHashMap();
                for (int i = 0; i < usedVars.Length; ++i)
                    mapping.Put(usedVars[i], i);
                return new Equation(usedVars, mapping, rPoly.DropVariables(rEliminated.ToArray()));
            }


            override MultivariatePolynomial<E> Simplify(MultivariatePolynomial<E> poly)
            {
                int[] degs = poly.Degrees();
                TIntArrayList subsVariables = new TIntArrayList();
                IList<E> subsValues = new List();
                for (int i = 0; i < solvedVariables.Count; ++i)
                    if (degs[solvedVariables[i]] > 0)
                    {
                        subsVariables.Add(solvedVariables[i]);
                        subsValues.Add(solutions[i]);
                    }

                if (subsVariables.IsEmpty())
                    return poly;
                return poly.Evaluate(subsVariables.ToArray(), subsValues.ToArray(ring.CreateArray(subsValues.Count)));
            }


            override SystemInfo Solve()
            {
                // sort equations from "simplest" to "hardest"
                equations.Sort(Comparator.ComparingInt((eq) => eq.usedVars.Length));
                for (int i = 0; i < equations.Count; ++i)
                {
                    // some base equation
                    Equation<Monomial<E>, MultivariatePolynomial<E>> baseEq = equations[i];
                    TIntHashSet baseVars = new TIntHashSet(baseEq.usedVars);

                    // search equations compatible with base equation
                    IList<Equation<Monomial<E>, MultivariatePolynomial<E>>> block =
                        new List(Collections.SingletonList(baseEq));
                    for (int j = 0; j < equations.Count; ++j)
                    {
                        if (i == j)
                            continue;
                        Equation<Monomial<E>, MultivariatePolynomial<E>> jEq = equations[j];
                        if (jEq.usedVars.Length > baseVars.Count)
                            break;
                        if (baseVars.ContainsAll(jEq.usedVars))
                            block.Add(jEq);
                    }

                    if (block.Count < baseVars.Count)

                        // block can't be solved
                        continue;
                    SystemInfo solve = Solve(block, baseVars.ToArray());
                    if (solve == Inconsistent)
                        return solve;
                    if (solve == UnderDetermined)
                        continue;
                    equations.RemoveAll(block);
                    SelfUpdate();
                    return Consistent;
                }

                return UnderDetermined;
            }


            SystemInfo Solve(IList<Equation<Monomial<E>, MultivariatePolynomial<E>>> equations, int[] usedVariables)
            {
                int nUsedVariables = usedVariables.Length;
                TIntIntHashMap mapping = new TIntIntHashMap();
                int[] linalgVariables = new int[nUsedVariables];
                int c = 0;
                foreach (int i in usedVariables)
                {
                    mapping.Put(i, c);
                    linalgVariables[c] = i;
                    ++c;
                }

                E[,] lhs = ring.CreateZeroesArray2d(equations.Count, nUsedVariables);
                E[] rhs = ring.CreateArray(equations.Count);
                for (int i = 0; i < equations.Count; ++i)
                {
                    Equation<Monomial<E>, MultivariatePolynomial<E>> eq = equations[i];
                    rhs[i] = ring.Negate(eq.reducedEquation.Cc());
                    foreach (Monomial<E> term in eq.reducedEquation)
                    {
                        if (term.IsZeroVector())
                            continue;
                        lhs[i][mapping[eq.usedVars[term.FirstNonZeroVariable()]]] = term.coefficient;
                    }
                }

                E[] linalgSolution = ring.CreateArray(nUsedVariables);
                SystemInfo solve = LinearSolver.Solve(ring, lhs, rhs, linalgSolution);
                if (solve == SystemInfo.Consistent)
                {
                    this.solvedVariables.AddAll(linalgVariables);
                    this.solutions.AddAll(Arrays.AsList(linalgSolution));
                }

                return solve;
            }
        }
    }
}