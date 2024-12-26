using Cc.Redberry.Combinatorics;
using Cc.Redberry;
using Cc.Redberry.Rings.Bigint;
using Cc.Redberry.Rings.Linear;
using Cc.Redberry.Rings.Poly;
using Cc.Redberry.Rings.Util;
using Org.Apache.Commons.Math3.Random;
using Java;
using Java.Util.Stream;
using Cc.Redberry.Rings;
using Cc.Redberry.Rings.Linear.LinearSolver.SystemInfo;
using Cc.Redberry.Rings.Poly.Multivar;
using Cc.Redberry.Rings.Poly.Multivar.MonomialOrder;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Cc.Redberry.Rings.Poly.Multivar
{
    /// <summary>
    /// Utility methods based on Groebner bases
    /// </summary>
    public sealed class GroebnerMethods
    {
        private GroebnerMethods()
        {
        }

        /* *********************************************** Elimination *********************************************** */
        /// <summary>
        /// Eliminates specified variables from the given ideal.
        /// </summary>
        public static IList<Poly> Eliminate<Poly extends AMultivariatePolynomial>(IList<Poly> ideal, int variable)
        {
            return Eliminate0(ideal, variable);
        }

        private static IList<Poly> Eliminate0<Term extends AMonomial<Term>, Poly extends AMultivariatePolynomial<Term, Poly>>(IList<Poly> ideal, int variable)
        {
            if (ideal.IsEmpty())
                return Collections.EmptyList();
            Comparator<DegreeVector> originalOrder = ideal[0].ordering;
            Comparator<DegreeVector> optimalOrder = OptimalOrder(ideal);
            IList<Poly> eliminationIdeal = ideal;
            if (!(optimalOrder is MonomialOrder.GrevLexWithPermutation))
                eliminationIdeal = GroebnerBasis(eliminationIdeal, new EliminationOrder(optimalOrder, variable)).Stream().Filter((p) => p.Degree(variable) == 0).Collect(Collectors.ToList());
            else
            {
                MonomialOrder.GrevLexWithPermutation order = (MonomialOrder.GrevLexWithPermutation)optimalOrder;
                int[] inversePermutation = MultivariateGCD.InversePermutation(order.permutation);
                eliminationIdeal = GroebnerBasis(eliminationIdeal.Stream().Map((p) => AMultivariatePolynomial.RenameVariables(p, order.permutation)).Collect(Collectors.ToList()), new EliminationOrder(GREVLEX, inversePermutation[variable])).Stream().Map((p) => AMultivariatePolynomial.RenameVariables(p, inversePermutation)).Filter((p) => p.Degree(variable) == 0).Collect(Collectors.ToList());
            }

            return eliminationIdeal.Stream().Map((p) => p.SetOrdering(originalOrder)).Collect(Collectors.ToList());
        }

        /// <summary>
        /// Eliminates specified variables from the given ideal.
        /// </summary>
        public static IList<Poly> Eliminate<Poly extends AMultivariatePolynomial>(IList<Poly> ideal, params int[] variables)
        {
            foreach (int variable in variables)
                ideal = Eliminate(ideal, variable);
            return ideal;
        }

        /* ******************************************* Algebraic dependence ******************************************** */
        /// <summary>
        /// Returns true if a given set of polynomials is probably algebraically dependent or false otherwise (which means
        /// that the given set is certainly independent). The method applies two criteria: it tests for lead set (LEX)
        /// independence and does a probabilistic Jacobian test.
        /// </summary>
        public static bool ProbablyAlgebraicallyDependentQ<Poly extends AMultivariatePolynomial>(IList<Poly> sys)
        {
            if (sys.IsEmpty())
                return false;
            Poly representative = sys[0];
            if (sys.Count > representative.nVariables)
                return true;

            // give a check for LEX leading terms set
            IList<DegreeVector> leadTerms;
            if (sys.Stream().AllMatch((p) => p.ordering == LEX))
                leadTerms = sys.Stream().Map(AMultivariatePolynomial.Lt()).Collect(Collectors.ToList());
            else
                leadTerms = sys.Stream().Map((p) => p.Lt(LEX)).Collect(Collectors.ToList());
            if (!AlgebraicallyDependentMonomialsQ(leadTerms))
                return false;
            if (IsMonomialIdeal(sys))
                return true;
            if (ProbablyMaximalJacobianRankQ(JacobianMatrix(sys)))
                return false;
            return true;
        }

        /// <summary>
        /// Returns true if a given set of polynomials is algebraically dependent or false otherwise.
        /// </summary>
        public static bool AlgebraicallyDependentQ<Poly extends AMultivariatePolynomial>(IList<Poly> sys)
        {
            return !AlgebraicRelations(sys).IsEmpty();
        }

        /// <summary>
        /// Tests for algebraic dependence the set of monomials
        /// </summary>
        static bool AlgebraicallyDependentMonomialsQ(IList<DegreeVector> sys)
        {
            if (sys.IsEmpty())
                return false;

            // build a homogeneous linear system
            int nVariables = sys[0].exponents.Length;
            int nUnknowns = sys.Count;

            // fixme use Bareiss in future
            Rational<BigInteger>[, ] lhs = new Rational[nVariables, nUnknowns];
            for (int i = 0; i < nVariables; ++i)
                for (int j = 0; j < nUnknowns; ++j)
                    lhs[i][j] = Q.ValueOf(sys[j].exponents[i]);
            Rational<BigInteger>[] rhs = Q.CreateZeroesArray(nVariables);

            // try to solve the system
            Rational<BigInteger>[] solution = Q.CreateZeroesArray(nUnknowns);
            LinearSolver.SystemInfo solveResult = LinearSolver.Solve(Q, lhs, rhs, solution);
            if (solveResult == Consistent && Arrays.Stream(solution).AllMatch(Rational.IsZero()))
                return false;
            if (solveResult == LinearSolver.SystemInfo.Inconsistent)
                return false;
            return true;
        }

        /// <summary>
        /// Number of random substitutions for polynomial Jacobian to deduce its rank
        /// </summary>
        private static readonly int N_JACOBIAN_EVALUATIONS_TRIES = 2;
        /// <summary>
        /// Probabilistic test for the maximality of the rank of Jacobian matrix
        /// </summary>
        static bool ProbablyMaximalJacobianRankQ<Poly extends AMultivariatePolynomial>(Poly[, ] jacobian)
        {
            if (jacobian[0][0] is MultivariatePolynomialZp64)
                return ProbablyMaximalJacobianRankQ((MultivariatePolynomialZp64[][])jacobian);
            else
                return ProbablyMaximalJacobianRankQ((MultivariatePolynomial[][])jacobian);
        }

        /// <summary>
        /// Probabilistic test for the maximality of the rank of Jacobian matrix
        /// </summary>
        static bool ProbablyMaximalJacobianRankQ(MultivariatePolynomialZp64[, ] jacobian)
        {
            int nRows = jacobian.Length, nColumns = jacobian[0].Length;
            MultivariatePolynomialZp64 factory = jacobian[0][0];
            IntegersZp64 ring = factory.ring;
            long[, ] matrix = new long[nRows, nColumns];
            long[] substitution = new long[nRows];
            RandomGenerator random = PrivateRandom.GetRandom();
            for (int i = 0; i < N_JACOBIAN_EVALUATIONS_TRIES; ++i)
            {
                for (int var = 0; var < nRows; ++var)
                    substitution[var] = ring.RandomNonZeroElement(random);
                for (int iRow = 0; iRow < nRows; ++iRow)
                    for (int iColumn = 0; iColumn < nColumns; ++iColumn)
                        matrix[iRow][iColumn] = jacobian[iRow][iColumn].Evaluate(substitution);
                int nz = LinearSolver.RowEchelonForm(ring, matrix, null, false, true);
                if (nz == 0)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Probabilistic test for the maximality of the rank of Jacobian matrix
        /// </summary>
        static bool ProbablyMaximalJacobianRankQ<E>(MultivariatePolynomial<E>[, ] jacobian)
        {
            int nRows = jacobian.Length, nColumns = jacobian[0].Length;
            MultivariatePolynomial<E> factory = jacobian[0][0];
            Ring<E> ring = factory.ring;
            E[, ] matrix = ring.CreateArray2d(nRows, nColumns);
            E[] substitution = ring.CreateArray(nRows);
            RandomGenerator random = PrivateRandom.GetRandom();
            for (int i = 0; i < N_JACOBIAN_EVALUATIONS_TRIES; ++i)
            {
                for (int var = 0; var < nRows; ++var)
                    substitution[var] = ring.RandomNonZeroElement(random);
                for (int iRow = 0; iRow < nRows; ++iRow)
                    for (int iColumn = 0; iColumn < nColumns; ++iColumn)
                        matrix[iRow][iColumn] = jacobian[iRow][iColumn].Evaluate(substitution);

                // fixme use Bareiss in future
                int nz = LinearSolver.RowEchelonForm(ring, matrix, null, false, true);
                if (nz == 0)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Gives a list of algebraic relations (annihilating polynomials) for the given list of polynomials
        /// </summary>
        public static IList<Poly> AlgebraicRelations<Poly extends AMultivariatePolynomial>(IList<Poly> polys)
        {
            return AlgebraicRelations0(polys);
        }

        private static IList<Poly> AlgebraicRelations0<Term extends AMonomial<Term>, Poly extends AMultivariatePolynomial<Term, Poly>>(IList<Poly> polys)
        {
            if (!ProbablyAlgebraicallyDependentQ(polys))
                return Collections.EmptyList();
            int nInitialVars = polys[0].nVariables;
            int nAdditionalVars = polys.Count;
            IList<Poly> helpPolys = new List();
            for (int i = 0; i < polys.Count; i++)
            {
                Poly p = polys[i].SetNVariables(nInitialVars + nAdditionalVars);
                helpPolys.Add(p.CreateMonomial(nInitialVars + i, 1).Subtract(p));
            }

            int[] dropVars = ArraysUtil.Sequence(0, nInitialVars);
            return Eliminate(helpPolys, dropVars).Stream().Map((p) => p.DropVariables(dropVars)).Collect(Collectors.ToList());
        }

        /// <summary>
        /// Creates a Jacobian matrix of a given list of polynomials
        /// </summary>
        public static Poly[][] JacobianMatrix<Term extends AMonomial<Term>, Poly extends AMultivariatePolynomial<Term, Poly>>(IList<Poly> sys)
        {
            if (sys.IsEmpty())
                throw new ArgumentException("Empty list");
            MultivariateRing<Poly> ring = MultivariateRing(sys[0]);
            Poly[, ] jacobian = ring.CreateArray2d(ring.NVariables(), sys.Count);
            for (int i = 0; i < ring.NVariables(); ++i)
                for (int j = 0; j < sys.Count; ++j)
                    jacobian[i][j] = sys[j].Derivative(i);
            return jacobian;
        }

        /* **************************************** Nullstellensatz certificate **************************************** */
        private static readonly int NULLSTELLENSATZ_LIN_SYS_THRESHOLD = 1 << 16;
        /// <summary>
        /// Computes Nullstellensatz certificate for a given list of polynomials assuming that they have no common zeros (or
        /// equivalently assuming that the ideal formed by the list is trivial). The method doesn't perform explicit check
        /// that the {@code polynomials} have no common zero, so if they are the method will fail.
        /// </summary>
        /// <param name="polynomials">list of polynomials</param>
        /// <returns>polynomials {@code S_i} such that {@code S_1 * f_1 + ... + S_n * f_n = 1} or null if no solution with
        ///         moderate degree bounds exist (either since {@code polynomials} have a common root or because the degree
        ///         bound on the solutions is so big that the system is intractable for computer)</returns>
        public static IList<Poly> NullstellensatzCertificate<Poly extends AMultivariatePolynomial>(IList<Poly> polynomials)
        {
            return NullstellensatzCertificate(polynomials, true);
        }

        /// <summary>
        /// Computes Nullstellensatz certificate for a given list of polynomials assuming that they have no common zeros (or
        /// equivalently assuming that the ideal formed by the list is trivial). The method doesn't perform explicit check
        /// that the {@code polynomials} have no common zero, so if they are the method will fail.
        /// </summary>
        /// <param name="polynomials">list of polynomials</param>
        /// <returns>polynomials {@code S_i} such that {@code S_1 * f_1 + ... + S_n * f_n = 1} or null if no solution with
        ///         moderate degree bounds exist (either since {@code polynomials} have a common root or because the degree
        ///         bound on the solutions is so big that the system is intractable for computer)</returns>
        public static IList<Poly> NullstellensatzCertificate<Poly extends AMultivariatePolynomial>(IList<Poly> polynomials, bool boundTotalDeg)
        {
            return NullstellensatzSolver(polynomials, (Poly)polynomials[0].CreateOne(), boundTotalDeg);
        }

        /// <summary>
        /// Tries to find solution of the equation {@code S_1 * f_1 + ... + S_n * f_n = g} for given {@code f_i} and {@code
        /// g} and unknown {@code S_i} by transforming to a system of linear equations with unknown coefficients of {@code
        /// S_i}.
        /// </summary>
        /// <param name="polynomials">list of polynomials</param>
        /// <param name="rhs">right hand side of the equation</param>
        /// <param name="boundTotalDeg">whether to perform evaluations by increasing total degree of unknown polys or by increasing
        ///                      individual degrees of vars</param>
        /// <returns>polynomials {@code S_i} such that {@code S_1 * f_1 + ... + S_n * f_n = g} or null if no solution with
        ///         moderate degree bounds exists</returns>
        public static IList<Poly> NullstellensatzSolver<Poly extends AMultivariatePolynomial>(IList<Poly> polynomials, Poly rhs, bool boundTotalDeg)
        {
            return NullstellensatzSolver0(polynomials, rhs, boundTotalDeg);
        }

        private static IList<Poly> NullstellensatzSolver0<Term extends AMonomial<Term>, Poly extends AMultivariatePolynomial<Term, Poly>>(IList<Poly> polynomials, Poly rhs, bool boundTotalDeg)
        {
            if (rhs.IsOverZ())

                // fixme: improve when Bareiss will be done
                // switch to Q and then to Z
                return NullstellensatzSolverZ((IList)polynomials, (MultivariatePolynomial)rhs, boundTotalDeg);
            Poly factory = polynomials[0];
            for (int degreeBound = 1;; ++degreeBound)
            {

                // number of coefficients in a single unknown poly
                BigInteger _maxCfSize;
                if (boundTotalDeg)
                    _maxCfSize = IntStream.RangeClosed(0, degreeBound).MapToObj((d) => Z.Binomial(d + factory.nVariables - 1, factory.nVariables - 1)).Reduce(Z.GetZero(), Z.Add());
                else
                    _maxCfSize = Z.Pow(Z.ValueOf(degreeBound), factory.nVariables);

                // total number of unknown coefficients
                BigInteger _nUnknowns = _maxCfSize.Multiply(Z.ValueOf(polynomials.Count));
                if (!_nUnknowns.IsInt())
                    return null;
                int nUnknowns = _nUnknowns.IntValue();
                if (nUnknowns > NULLSTELLENSATZ_LIN_SYS_THRESHOLD)
                    return null;
                int maxCfSize = _maxCfSize.IntValueExact();

                // factory polynomial and ring for the coefficients R[u1, ..., uM]
                Poly cfFactory = factory.CreateZero().SetNVariables(nUnknowns);
                MultivariateRing<Poly> cfRing = MultivariateRing(cfFactory);

                // ring used to build the system R[u1, ..., uM][x1, ..., xN]
                MultivariateRing<MultivariatePolynomial<Poly>> linSysRing = Rings.MultivariateRing(factory.nVariables, cfRing);

                // initial system as R[u1, ..., uM][x1, ..., xN]
                IList<MultivariatePolynomial<Poly>> convertedPolynomials = polynomials.Stream().Map((p) => p.AsOverPoly(cfFactory)).Collect(Collectors.ToList());

                // solution
                IList<MultivariatePolynomial<Poly>> certificate = new List();

                // building the lhs of the equation \sum_i C_i * f_i = rhs
                MultivariatePolynomial<Poly> eq = linSysRing.GetZero();
                for (int i = 0; i < polynomials.Count; ++i)
                {
                    MultivariatePolynomial<Poly> unknownPoly = Generate(cfRing, linSysRing, degreeBound, i * maxCfSize, boundTotalDeg);
                    certificate.Add(unknownPoly);
                    eq.Add(convertedPolynomials[i].Multiply(unknownPoly));
                }


                // if still not compatible
                if (!eq.GetSkeleton().ContainsAll(rhs.GetSkeleton()))
                    continue;

                // solving linear system
                IList<Poly> cert = FindCertificateFromLinearSystem(eq, certificate, rhs, nUnknowns);
                if (cert != null)
                    return cert;
            }
        }

        private static IList<MultivariatePolynomial<BigInteger>> NullstellensatzSolverZ(IList<MultivariatePolynomial<BigInteger>> polynomials, MultivariatePolynomial<BigInteger> rhs, bool boundTotalDeg)
        {

            // fixme: a crutch
            IList<MultivariatePolynomial<Rational<BigInteger>>> result = NullstellensatzSolver(polynomials.Stream().Map((p) => p.MapCoefficients(Q, Q.MkNumerator())).Collect(Collectors.ToList()), rhs.MapCoefficients(Q, Q.MkNumerator()), boundTotalDeg);
            if (result.Stream().AnyMatch((p) => !p.Stream().AllMatch(Rational.IsIntegral())))
                return null;
            return result.Stream().Map((p) => p.MapCoefficients(Z, Rational.Numerator())).Collect(Collectors.ToList());
        }

        /// <summary>
        /// </summary>
        /// <param name="eq">the equation</param>
        /// <param name="certificate">unknown polynomials (certificate)</param>
        /// <param name="rhs">rhs poly</param>
        /// <param name="nUnknowns">number of unknown coefficients</param>
        private static IList<Poly> FindCertificateFromLinearSystem<Term extends AMonomial<Term>, Poly extends AMultivariatePolynomial<Term, Poly>>(MultivariatePolynomial<Poly> eq, IList<MultivariatePolynomial<Poly>> certificate, Poly rhs, int nUnknowns)
        {
            if (eq.Cc() is MultivariatePolynomialZp64)
                return FindCertificateZp64((MultivariatePolynomial)eq, (IList)certificate, (MultivariatePolynomialZp64)rhs, nUnknowns);
            else
                return FindCertificateE((MultivariatePolynomial)eq, (IList)certificate, (MultivariatePolynomial)rhs, nUnknowns);
        }

        /// <summary>
        /// Solve in Zp64
        /// </summary>
        private static IList<MultivariatePolynomialZp64> FindCertificateZp64(MultivariatePolynomial<MultivariatePolynomialZp64> eq, IList<MultivariatePolynomial<MultivariatePolynomialZp64>> certificate, MultivariatePolynomialZp64 rhsPoly, int nUnknowns)
        {

            // lhs of the system
            long[, ] lhs = new long[eq.Count, nUnknowns];

            // rhs of the system
            long[] rhs = new long[eq.Count];
            int iEq = 0;
            foreach (Monomial<MultivariatePolynomialZp64> term in eq)
            {
                MonomialZp64 rhsTerm = rhsPoly.terms[term];
                if (rhsTerm != null)
                    rhs[iEq] = rhsTerm.coefficient;
                else
                    rhs[iEq] = 0;
                foreach (MonomialZp64 cfTerm in term.coefficient)
                {
                    lhs[iEq][cfTerm.FirstNonZeroVariable()] = cfTerm.coefficient;
                }

                ++iEq;
            }

            IntegersZp64 ring = eq.Lc().ring;
            long[] result = new long[nUnknowns];
            LinearSolver.SystemInfo solve = LinearSolver.Solve(ring, lhs, rhs, result, true);
            if (solve == Inconsistent)
                return null;
            return certificate.Stream().Map((p) => p.MapCoefficientsZp64(ring, (m) => m.Evaluate(result))).Collect(Collectors.ToList());
        }

        /// <summary>
        /// Solve in genetic ring
        /// </summary>
        private static IList<MultivariatePolynomial<E>> FindCertificateE<E>(MultivariatePolynomial<MultivariatePolynomial<E>> eq, IList<MultivariatePolynomial<MultivariatePolynomial<E>>> certificate, MultivariatePolynomial<E> rhsPoly, int nUnknowns)
        {
            Ring<E> ring = eq.Lc().ring;
            E[, ] lhs = ring.CreateZeroesArray2d(eq.Count, nUnknowns);

            // rhs of the system
            E[] rhs = ring.CreateZeroesArray(eq.Count);
            int iEq = 0;
            foreach (Monomial<MultivariatePolynomial<E>> term in eq)
            {
                Monomial<E> rhsTerm = rhsPoly.terms[term];
                if (rhsTerm != null)
                    rhs[iEq] = rhsTerm.coefficient;
                else
                    rhs[iEq] = ring.GetZero();
                foreach (Monomial<E> cfTerm in term.coefficient)
                {
                    lhs[iEq][cfTerm.FirstNonZeroVariable()] = cfTerm.coefficient;
                }

                ++iEq;
            }

            E[] result = ring.CreateArray(nUnknowns);
            LinearSolver.SystemInfo solve = LinearSolver.Solve(ring, lhs, rhs, result, true);
            if (solve == Inconsistent)
                return null;
            return certificate.Stream().Map((p) => p.MapCoefficients(ring, (m) => m.Evaluate(result))).Collect(Collectors.ToList());
        }

        /// <summary>
        /// Generates a poly of specified degree (in each variable) with unknown coefficients.
        /// </summary>
        /// <param name="cfRing">coefficient ring</param>
        /// <param name="ring">ring which result belongs to</param>
        /// <param name="degree">degree bound</param>
        /// <param name="startingVar">the starting coefficient</param>
        static MultivariatePolynomial<Poly> Generate<Term extends AMonomial<Term>, Poly extends AMultivariatePolynomial<Term, Poly>>(MultivariateRing<Poly> cfRing, MultivariateRing<MultivariatePolynomial<Poly>> ring, int degree, int startingVar, bool boundTotalDeg)
        {
            MultivariatePolynomial<Poly> result = ring.GetZero();
            if (boundTotalDeg)
                for (int d = 0; d <= degree; ++d)
                    foreach (int[] tuple in new IEnumerator(new IntCompositions(d, ring.NVariables())))
                        result.Add(new Monomial(tuple, cfRing.Variable(startingVar++)));
            else
                foreach (int[] tuple in Combinatorics.Tuples(ArraysUtil.ArrayOf(degree, ring.NVariables())))
                    result.Add(new Monomial(tuple, cfRing.Variable(startingVar++)));
            return result;
        }

        /* **************************************** Partial fractions **************************************** */
        /// <summary>
        /// Computes Leinartas's decomposition of given rational expression (see https://arxiv.org/abs/1206.4740)
        /// </summary>
        public static IList<Rational<Poly>> LeinartasDecomposition<Poly extends AMultivariatePolynomial>(Rational<Poly> fraction)
        {
            return LeinartasDecomposition0(fraction);
        }

        /// <summary>
        /// Computes Leinartas's decomposition of given rational expression (see https://arxiv.org/abs/1206.4740)
        /// </summary>
        private static IList<Rational<Poly>> LeinartasDecomposition0<Term extends AMonomial<Term>, Poly extends AMultivariatePolynomial<Term, Poly>>(Rational<Poly> fraction)
        {
            FactorDecomposition<Poly> denDecomposition = fraction.FactorDenominator();
            IList<Factor<Term, Poly>> denominator = IntStream.Range(0, denDecomposition.Count).MapToObj((i) => new Factor(denDecomposition[i], denDecomposition.GetExponent(i))).Collect(Collectors.ToList());
            return NullstellensatzDecomposition(new Fraction(fraction.Numerator(), denominator, denDecomposition.unit)).Stream().FlatMap((p) => AlgebraicDecomposition(p).Stream()).Map((f) => f.ToRational(fraction.ring)).Collect(Collectors.ToList());
        }

        static IList<Fraction<Term, Poly>> NullstellensatzDecomposition<Term extends AMonomial<Term>, Poly extends AMultivariatePolynomial<Term, Poly>>(Fraction<Term, Poly> fraction)
        {
            if (!Ideal.Create(fraction.BareDenominatorNoUnits()).IsEmpty())
                return Collections.SingletonList(fraction);

            // denominators have not common zeros
            // apply Nullstellensatz decomposition
            IList<Poly> certificate = NullstellensatzCertificate(fraction.RaisedDenominator());
            return IntStream.Range(0, certificate.Count).MapToObj((i) => new Fraction(certificate[i].Multiply(fraction.numerator), Remove(fraction.denominator, i), fraction.denominatorConstantFactor)).FlatMap((f) => NullstellensatzDecomposition(f).Stream()).Collect(Collectors.ToList());
        }

        static IList<Fraction<Term, Poly>> AlgebraicDecomposition<Term extends AMonomial<Term>, Poly extends AMultivariatePolynomial<Term, Poly>>(Fraction<Term, Poly> fraction)
        {
            if (!ProbablyAlgebraicallyDependentQ(fraction.BareDenominatorNoUnits()))
                return Collections.SingletonList(fraction);
            IList<Poly> raisedDenominator = fraction.RaisedDenominator();
            IList<Poly> annihilators = AlgebraicRelations(raisedDenominator);
            if (annihilators.IsEmpty())
                return Collections.SingletonList(fraction);

            // denominators are algebraically dependent
            // choose the simplest annihilator
            Poly annihilator = annihilators.Stream().Min(Comparator.ComparingInt((p) => p.Mt().totalDegree)).Get().SetOrderingUnsafe(GREVLEX);

            // choose the simplest monomial in annihilator
            Term minNormTerm = annihilator.Mt();
            annihilator.Subtract(minNormTerm).Negate();
            Poly numerator = fraction.numerator;
            IList<Factor<Term, Poly>> denominator = fraction.denominator;
            int[] denominatorExponents = denominator.Stream().MapToInt((f) => f.exponent).ToArray();
            IList<Fraction<Term, Poly>> result = new List();
            foreach (Term numFactor in annihilator)
            {

                // numFactor / minNormTerm / denominator
                int[] numExponents = ArraysUtil.Multiply(denominatorExponents, numFactor.exponents);
                int[] denExponents = ArraysUtil.Sum(denominatorExponents, ArraysUtil.Multiply(denominatorExponents, minNormTerm.exponents));
                for (int i = 0; i < numExponents.Length; ++i)
                {
                    if (numExponents[i] >= denExponents[i])
                    {
                        numExponents[i] -= denExponents[i];
                        denExponents[i] = 0;
                    }
                    else
                    {
                        denExponents[i] -= numExponents[i];
                        numExponents[i] = 0;
                    }
                }

                Poly num = IntStream.Range(0, numExponents.Length).MapToObj((i) => denominator[i].SetExponent(numExponents[i]).raised).Reduce(numerator.Clone(), (a, b) => a.Multiply(b)).Multiply(numerator.CreateConstantFromTerm(numFactor));
                IList<Factor<Term, Poly>> den = IntStream.Range(0, numExponents.Length).MapToObj((i) => denominator[i].SetExponent(denExponents[i])).Collect(Collectors.ToList());
                Poly denConstant = fraction.denominatorConstantFactor.Clone().Multiply(numerator.CreateConstantFromTerm(minNormTerm));
                result.AddAll(AlgebraicDecomposition(new Fraction(num, den, denConstant)));
            }

            return result;
        }

        private static IList<E> Remove<E>(IList<E> list, int i)
        {
            list.Remove(i);
            return list;
        }

        private sealed class Fraction<Term, Poly>
        {
            readonly Poly numerator;
            readonly IList<Factor<Term, Poly>> denominator;
            readonly Poly denominatorConstantFactor;
            Fraction(Poly numerator, IList<Factor<Term, Poly>> denominator) : this(numerator, denominator, numerator.CreateOne())
            {
            }

            Fraction(Poly numerator, IList<Factor<Term, Poly>> denominator, Poly denominatorConstantFactor)
            {
                denominator = new List(denominator);
                denominatorConstantFactor = denominatorConstantFactor.Clone();
                for (int i = denominator.size() - 1; i >= 0; --i)
                    if (denominator[i].IsConstant())
                    {
                        denominatorConstantFactor = denominatorConstantFactor.Multiply(denominator[i].raised);
                        denominator.Remove(i);
                    }

                Poly cGcd = PolynomialMethods.PolynomialGCD(numerator, denominatorConstantFactor);
                this.numerator = numerator.DivideByLC(cGcd);
                this.denominator = denominator;
                this.denominatorConstantFactor = denominatorConstantFactor.DivideByLC(cGcd);
            }

            IList<Poly> RaisedDenominator()
            {
                return denominator.Stream().Map((p) => p.raised).Collect(Collectors.ToList());
            }

            IList<Poly> BareDenominator()
            {
                return denominator.Stream().Map((p) => p.factor).Collect(Collectors.ToList());
            }

            IList<Poly> BareDenominatorNoUnits()
            {
                return BareDenominator().Stream().Filter((p) => !p.IsConstant()).Collect(Collectors.ToList());
            }

            Rational<Poly> ToRational(Ring<Poly> polyRing)
            {
                Rational<Poly> r = new Rational(polyRing, numerator);
                r = r.Divide(denominatorConstantFactor);
                foreach (Factor<Term, Poly> den in denominator)
                    r = r.Divide(den.raised);
                return r;
            }
        }

        private sealed class Factor<Term, Poly>
        {
            readonly Poly factor;
            readonly int exponent;
            readonly Poly raised;
            Factor(Poly factor, int exponent, Poly raised)
            {
                this.factor = exponent == 0 ? factor.CreateOne() : factor;
                this.exponent = exponent;
                this.raised = raised;
            }

            Factor(Poly factor, int exponent)
            {
                this.factor = factor;
                this.exponent = exponent;
                this.raised = PolynomialMethods.PolyPow(factor, exponent, true);
            }

            Factor<Term, Poly> SetExponent(int newExponent)
            {
                if (exponent == newExponent)
                    return this;
                if (exponent == 0)
                    return new Factor(factor.CreateOne(), 0, factor.CreateOne());
                if (newExponent % exponent == 0)
                    return new Factor(factor, newExponent, PolynomialMethods.PolyPow(raised, newExponent / exponent));
                return new Factor(factor, newExponent);
            }

            bool IsConstant()
            {
                return factor.IsConstant();
            }
        }
    }
}