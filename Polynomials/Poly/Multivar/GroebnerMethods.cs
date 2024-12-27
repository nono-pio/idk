using System.Numerics;
using Polynomials;
using Polynomials.Linear;
using Polynomials.Poly.Multivar;
using Polynomials.Utils;

namespace Polynomials.Poly.Multivar;

public sealed class GroebnerMethods
{
    private GroebnerMethods()
    {
    }

    /* *********************************************** Elimination *********************************************** */


    public static List<MultivariatePolynomial<E>> Eliminate<E>(List<MultivariatePolynomial<E>> ideal, int variable)
    {
        return Eliminate0(ideal, variable);
    }

    private static List<MultivariatePolynomial<E>> Eliminate0<E>(List<MultivariatePolynomial<E>> ideal,
        int variable)
    {
        if (ideal.Count == 0)
            return [];
        IComparer<DegreeVector> originalOrder = ideal[0].ordering;
        IComparer<DegreeVector> optimalOrder = OptimalOrder(ideal);
        List<MultivariatePolynomial<E>> eliminationIdeal = ideal;
        if (!(optimalOrder is MonomialOrder.GrevLexWithPermutation))
            eliminationIdeal =
                GroebnerBasis(eliminationIdeal, new MonomialOrder.EliminationOrder(optimalOrder, variable)).Stream()
                    .Filter((p) => p.Degree(variable) == 0).ToList();
        else
        {
            MonomialOrder.GrevLexWithPermutation order = (MonomialOrder.GrevLexWithPermutation)optimalOrder;
            int[] inversePermutation = MultivariateGCD.inversePermutation(order.permutation);
            eliminationIdeal =
                GroebnerBasis(
                        eliminationIdeal.Select((p) =>
                                MultivariatePolynomial<E>.RenameVariables(p, order.permutation))
                            .ToList(),
                        new MonomialOrder.EliminationOrder(MonomialOrder.GREVLEX, inversePermutation[variable]))
                    .Stream().Map((p) => MultivariatePolynomial<E>.RenameVariables(p, inversePermutation))
                    .Filter((p) => p.Degree(variable) == 0).ToList();
        }

        return eliminationIdeal.Select((p) => p.SetOrdering(originalOrder)).ToList();
    }


    public static List<MultivariatePolynomial<E>> Eliminate<E>(List<MultivariatePolynomial<E>> ideal,
        params int[] variables)
    {
        foreach (int variable in variables)
            ideal = Eliminate(ideal, variable);
        return ideal;
    }

    /* ******************************************* Algebraic dependence ******************************************** */


    public static bool ProbablyAlgebraicallyDependentQ<E>(List<MultivariatePolynomial<E>> sys)
    {
        if (sys.Count == 0)
            return false;
        var representative = sys[0];
        if (sys.Count > representative.nVariables)
            return true;

        // give a check for LEX leading terms set
        List<DegreeVector> leadTerms;
        if (sys.All((p) => Equals(p.ordering, MonomialOrder.LEX)))
            leadTerms = sys.Select(m => (DegreeVector)m.Lt()).ToList();
        else
            leadTerms = sys.Select((p) => (DegreeVector)p.Lt(MonomialOrder.LEX)).ToList();
        if (!AlgebraicallyDependentMonomialsQ(leadTerms))
            return false;
        if (IsMonomialIdeal(sys))
            return true;
        if (ProbablyMaximalJacobianRankQ(JacobianMatrix(sys)))
            return false;
        return true;
    }


    public static bool AlgebraicallyDependentQ<E>(List<MultivariatePolynomial<E>> sys)
    {
        return AlgebraicRelations(sys).Count != 0;
    }


    static bool AlgebraicallyDependentMonomialsQ(List<DegreeVector> sys)
    {
        if (sys.Count == 0)
            return false;

        // build a homogeneous linear system
        int nVariables = sys[0].exponents.Length;
        int nUnknowns = sys.Count;

        // fixme use Bareiss in future
        Rational<BigInteger>[,] lhs = new Rational<BigInteger>[nVariables, nUnknowns];
        for (int i = 0; i < nVariables; ++i)
        for (int j = 0; j < nUnknowns; ++j)
            lhs[i, j] = Rings.Q.ValueOfLong(sys[j].exponents[i]);
        Rational<BigInteger>[] rhs = Rings.Q.CreateZeroesArray(nVariables);

        // try to solve the system
        Rational<BigInteger>[] solution = Rings.Q.CreateZeroesArray(nUnknowns);
        LinearSolver.SystemInfo solveResult = LinearSolver.Solve(Rings.Q, lhs, rhs, solution);
        if (solveResult == LinearSolver.SystemInfo.Consistent && solution.All(r => r.IsZero()))
            return false;
        if (solveResult == LinearSolver.SystemInfo.Inconsistent)
            return false;
        return true;
    }


    private static readonly int N_JACOBIAN_EVALUATIONS_TRIES = 2;


    static bool ProbablyMaximalJacobianRankQ<E>(MultivariatePolynomial<E>[,] jacobian)
    {
        int nRows = jacobian.GetLength(0), nColumns = jacobian.GetLength(1);
        MultivariatePolynomial<E> factory = jacobian[0, 0];
        Ring<E> ring = factory.ring;
        E[,] matrix = new E[nRows, nColumns];
        E[] substitution = new E[nRows];
        Random random = PrivateRandom.GetRandom();
        for (int i = 0; i < N_JACOBIAN_EVALUATIONS_TRIES; ++i)
        {
            for (int var = 0; var < nRows; ++var)
                substitution[var] = ring.RandomNonZeroElement(random);
            for (int iRow = 0; iRow < nRows; ++iRow)
            for (int iColumn = 0; iColumn < nColumns; ++iColumn)
                matrix[iRow, iColumn] = jacobian[iRow, iColumn].Evaluate(substitution);

            // fixme use Bareiss in future
            int nz = LinearSolver.RowEchelonForm(ring, matrix, null, false, true);
            if (nz == 0)
                return true;
        }

        return false;
    }


    public static List<MultivariatePolynomial<E>> AlgebraicRelations<E>(List<MultivariatePolynomial<E>> polys)
    {
        return AlgebraicRelations0(polys);
    }

    private static List<MultivariatePolynomial<E>> AlgebraicRelations0<E>(List<MultivariatePolynomial<E>> polys)
    {
        if (!ProbablyAlgebraicallyDependentQ(polys))
            return [];
        int nInitialVars = polys[0].nVariables;
        int nAdditionalVars = polys.Count;
        var helpPolys = new List<MultivariatePolynomial<E>>();
        for (int i = 0; i < polys.Count; i++)
        {
            var p = polys[i].SetNVariables(nInitialVars + nAdditionalVars);
            helpPolys.Add(p.CreateMonomial(nInitialVars + i, 1).Subtract(p));
        }

        int[] dropVars = Utils.Utils.Sequence(0, nInitialVars);
        return Eliminate(helpPolys, dropVars).Select((p) => p.DropVariables(dropVars))
            .ToList();
    }


    public static MultivariatePolynomial<E>[,] JacobianMatrix<E>(List<MultivariatePolynomial<E>> sys)
    {
        if (sys.Count == 0)
            throw new ArgumentException("Empty list");
        MultivariateRing<E> ring = Rings.MultivariateRing(sys[0]);
        var jacobian = new MultivariatePolynomial<E>[ring.NVariables(), sys.Count];
        for (int i = 0; i < ring.NVariables(); ++i)
        for (int j = 0; j < sys.Count; ++j)
            jacobian[i, j] = sys[j].Derivative(i);
        return jacobian;
    }

    /* **************************************** Nullstellensatz certificate **************************************** */
    private static readonly int NULLSTELLENSATZ_LIN_SYS_THRESHOLD = 1 << 16;


    public static List<MultivariatePolynomial<E>> NullstellensatzCertificate<E>(List<MultivariatePolynomial<E>>
        polynomials)
    {
        return NullstellensatzCertificate(polynomials, true);
    }


    public static List<MultivariatePolynomial<E>> NullstellensatzCertificate<E>(List<MultivariatePolynomial<E>>
        polynomials, bool boundTotalDeg)
    {
        return NullstellensatzSolver(polynomials, polynomials[0].CreateOne(), boundTotalDeg);
    }


    public static List<MultivariatePolynomial<E>> NullstellensatzSolver<E>(List<MultivariatePolynomial<E>> polynomials,
        MultivariatePolynomial<E> rhs, bool boundTotalDeg)
    {
        return NullstellensatzSolver0(polynomials, rhs, boundTotalDeg);
    }

    private static List<MultivariatePolynomial<E>> NullstellensatzSolver0<E>(
        List<MultivariatePolynomial<E>> polynomials, MultivariatePolynomial<E> rhs, bool boundTotalDeg)
    {
        if (rhs.IsOverZ())

            // fixme: improve when Bareiss will be done
            // switch to Q and then to Z
            return NullstellensatzSolverZ(
                polynomials as List<MultivariatePolynomial<BigInteger>>,
                rhs as MultivariatePolynomial<BigInteger>,
                boundTotalDeg) as List<MultivariatePolynomial<E>>;
        var factory = polynomials[0];
        for (int degreeBound = 1;; ++degreeBound)
        {
            // number of coefficients in a single unknown poly
            BigInteger _maxCfSize;
            if (boundTotalDeg)
                _maxCfSize = Enumerable.Range(0, degreeBound)
                    .Select((d) => Rings.Z.Binomial(d + factory.nVariables - 1, factory.nVariables - 1))
                    .Aggregate(Rings.Z.GetZero(), Rings.Z.Add);
            else
                _maxCfSize = Rings.Z.Pow(Rings.Z.ValueOf(degreeBound), factory.nVariables);

            // total number of unknown coefficients
            BigInteger _nUnknowns = _maxCfSize * (Rings.Z.ValueOf(polynomials.Count));
            if (!_nUnknowns.IsInt())
                return null;
            int nUnknowns = (int)_nUnknowns;
            if (nUnknowns > NULLSTELLENSATZ_LIN_SYS_THRESHOLD)
                return null;
            int maxCfSize = (int)_maxCfSize;

            // factory polynomial and ring for the coefficients R[u1, ..., uM]
            var cfFactory = factory.CreateZero().SetNVariables(nUnknowns);
            var cfRing = Rings.MultivariateRing(cfFactory);

            // ring used to build the system R[u1, ..., uM][x1, ..., xN]
            var linSysRing =
                Rings.MultivariateRing(factory.nVariables, cfRing);

            // initial system as R[u1, ..., uM][x1, ..., xN]
            List<MultivariatePolynomial<MultivariatePolynomial<E>>> convertedPolynomials =
                polynomials.Select((p) => p.AsOverPoly(cfFactory)).ToList();

            // solution
            List<MultivariatePolynomial<MultivariatePolynomial<E>>> certificate =
                new List<MultivariatePolynomial<MultivariatePolynomial<E>>>();

            // building the lhs of the equation \sum_i C_i * f_i = rhs
            MultivariatePolynomial<MultivariatePolynomial<E>> eq = linSysRing.GetZero();
            for (int i = 0; i < polynomials.Count; ++i)
            {
                MultivariatePolynomial<MultivariatePolynomial<E>> unknownPoly =
                    Generate(cfRing, linSysRing, degreeBound, i * maxCfSize, boundTotalDeg);
                certificate.Add(unknownPoly);
                eq.Add(convertedPolynomials[i].Multiply(unknownPoly));
            }


            // if still not compatible
            if (!eq.GetSkeleton().ContainsAll(rhs.GetSkeleton()))
                continue;

            // solving linear system
            List<MultivariatePolynomial<E>> cert = FindCertificateFromLinearSystem(eq, certificate, rhs, nUnknowns);
            if (cert != null)
                return cert;
        }
    }

    private static List<MultivariatePolynomial<BigInteger>> NullstellensatzSolverZ(
        List<MultivariatePolynomial<BigInteger>> polynomials, MultivariatePolynomial<BigInteger> rhs,
        bool boundTotalDeg)
    {
        // fixme: a crutch
        List<MultivariatePolynomial<Rational<BigInteger>>> result =
            NullstellensatzSolver(
                polynomials.Select((p) => p.MapCoefficients(Rings.Q, Rings.Q.MkNumerator)).ToList(),
                rhs.MapCoefficients(Rings.Q, Rings.Q.MkNumerator), boundTotalDeg);
        if (result.Any((p) => !p.Stream().All(r => r.IsIntegral())))
            return null;
        return result.Select((p) => p.MapCoefficients(Rings.Z, r => r.Numerator())).ToList();
    }


    private static List<MultivariatePolynomial<E>> FindCertificateFromLinearSystem<E>(
        MultivariatePolynomial<MultivariatePolynomial<E>> eq,
        List<MultivariatePolynomial<MultivariatePolynomial<E>>> certificate, MultivariatePolynomial<E> rhsPoly,
        int nUnknowns)
    {
        Ring<E> ring = eq.Lc().ring;
        E[,] lhs = new E[eq.Size(), nUnknowns];

        // rhs of the system
        E[] rhs = new E[eq.Size()];
        int iEq = 0;
        foreach (Monomial<MultivariatePolynomial<E>> term in eq.terms)
        {
            Monomial<E> rhsTerm = rhsPoly.terms[term];
            if (rhsTerm != null)
                rhs[iEq] = rhsTerm.coefficient;
            else
                rhs[iEq] = ring.GetZero();
            foreach (Monomial<E> cfTerm in term.coefficient.terms)
            {
                lhs[iEq, cfTerm.FirstNonZeroVariable()] = cfTerm.coefficient;
            }

            ++iEq;
        }

        E[] result = new E[nUnknowns];
        LinearSolver.SystemInfo solve = LinearSolver.Solve(ring, lhs, rhs, result, true);
        if (solve == LinearSolver.SystemInfo.Inconsistent)
            return null;
        return certificate.Select((p) => p.MapCoefficients(ring, (m) => m.Evaluate(result)))
            .ToList();
    }


    static MultivariatePolynomial<MultivariatePolynomial<E>> Generate<E>(MultivariateRing<E> cfRing,
        MultivariateRing<MultivariatePolynomial<E>> ring, int degree, int startingVar, bool boundTotalDeg)
    {
        MultivariatePolynomial<MultivariatePolynomial<E>> result = ring.GetZero();
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


    public static List<Rational<MultivariatePolynomial<E>>> LeinartasDecomposition<E>(
        Rational<MultivariatePolynomial<E>>
            fraction)
    {
        return LeinartasDecomposition0(fraction);
    }


    private static List<Rational<MultivariatePolynomial<E>>> LeinartasDecomposition0<E>(
        Rational<MultivariatePolynomial<E>> fraction)
    {
        FactorDecomposition<MultivariatePolynomial<E>> denDecomposition = fraction.FactorDenominator();
        List<Factor<E>> denominator = Enumerable.Range(0, denDecomposition.Factors.Count)
            .Select((i) => new Factor<E>(denDecomposition.Factors[i], denDecomposition.Exponents[i]))
            .ToList();
        return NullstellensatzDecomposition(new Fraction<E>(fraction.Numerator(), denominator, denDecomposition.Unit))
            .SelectMany((p) => AlgebraicDecomposition(p).Select((f) => f.ToRational(fraction.ring)))
            .ToList();
    }

    static List<Fraction<E>> NullstellensatzDecomposition<E>(Fraction<E> fraction)
    {
        if (!Ideal<E>.Create(fraction.BareDenominatorNoUnits()).IsEmpty())
            return [fraction];

        // denominators have not common zeros
        // apply Nullstellensatz decomposition
        List<MultivariatePolynomial<E>> certificate = NullstellensatzCertificate(fraction.RaisedDenominator());
        return Enumerable.Range(0, certificate.Count)
            .Select((i) => new Fraction<E>(certificate[i].Multiply(fraction.numerator),
                Remove(fraction.denominator, i), fraction.denominatorConstantFactor))
            .SelectMany(NullstellensatzDecomposition).ToList();
    }

    static List<Fraction<E>> AlgebraicDecomposition<E>(Fraction<E> fraction)
    {
        if (!ProbablyAlgebraicallyDependentQ(fraction.BareDenominatorNoUnits()))
            return [fraction];
        List<MultivariatePolynomial<E>> raisedDenominator = fraction.RaisedDenominator();
        List<MultivariatePolynomial<E>> annihilators = AlgebraicRelations(raisedDenominator);
        if (annihilators.Count == 0)
            return [fraction];

        // denominators are algebraically dependent
        // choose the simplest annihilator
        var annihilator = annihilators.Min(Comparator.ComparingInt((p) => p.Mt().totalDegree)).Get()
            .SetOrderingUnsafe(MonomialOrder.GREVLEX);

        // choose the simplest monomial in annihilator
        var minNormTerm = annihilator.Mt();
        annihilator.Subtract(minNormTerm).Negate();
        var numerator = fraction.numerator;
        List<Factor<E>> denominator = fraction.denominator;
        int[] denominatorExponents = denominator.Select((f) => f.exponent).ToArray();
        List<Fraction<E>> result = new List<Fraction<E>>();
        foreach (var numFactor in annihilator.terms)
        {
            // numFactor / minNormTerm / denominator
            int[] numExponents = ArraysUtil.Multiply(denominatorExponents, numFactor.exponents);
            int[] denExponents = ArraysUtil.Sum(denominatorExponents,
                ArraysUtil.Multiply(denominatorExponents, minNormTerm.exponents));
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

            var num = Enumerable.Range(0, numExponents.Length)
                .Select((i) => denominator[i].SetExponent(numExponents[i]).raised)
                .Aggregate(numerator.Clone(), (a, b) => a.Multiply(b))
                .Multiply(numerator.CreateConstantFromTerm(numFactor));
            List<Factor<E>> den = Enumerable.Range(0, numExponents.Length)
                .Select((i) => denominator[i].SetExponent(denExponents[i])).ToList();
            var denConstant = fraction.denominatorConstantFactor.Clone()
                .Multiply(numerator.CreateConstantFromTerm(minNormTerm));
            result.AddRange(AlgebraicDecomposition(new Fraction<E>(num, den, denConstant)));
        }

        return result;
    }

    private static List<E> Remove<E>(List<E> list, int i)
    {
        list.RemoveAt(i);
        return list;
    }

    private sealed class Fraction<E>
    {
        public readonly MultivariatePolynomial<E> numerator;
        public readonly List<Factor<E>> denominator;
        public readonly MultivariatePolynomial<E> denominatorConstantFactor;

        public Fraction(MultivariatePolynomial<E> numerator, List<Factor<E>> denominator) : this(numerator, denominator,
            numerator.CreateOne())
        {
        }

        public Fraction(MultivariatePolynomial<E> numerator, List<Factor<E>> denominator,
            MultivariatePolynomial<E> denominatorConstantFactor)
        {
            denominator = new List<Factor<E>>(denominator);
            denominatorConstantFactor = denominatorConstantFactor.Clone();
            for (int i = denominator.Count - 1; i >= 0; --i)
                if (denominator[i].IsConstant())
                {
                    denominatorConstantFactor = denominatorConstantFactor.Multiply(denominator[i].raised);
                    denominator.RemoveAt(i);
                }

            var cGcd = PolynomialMethods.PolynomialGCD(numerator, denominatorConstantFactor);
            this.numerator = numerator.DivideByLC(cGcd);
            this.denominator = denominator;
            this.denominatorConstantFactor = denominatorConstantFactor.DivideByLC(cGcd);
        }

        public List<MultivariatePolynomial<E>> RaisedDenominator()
        {
            return denominator.Select((p) => p.raised).ToList();
        }

        List<MultivariatePolynomial<E>> BareDenominator()
        {
            return denominator.Select((p) => p.factor).ToList();
        }

        public List<MultivariatePolynomial<E>> BareDenominatorNoUnits()
        {
            return BareDenominator().Where((p) => !p.IsConstant()).ToList();
        }

        public Rational<MultivariatePolynomial<E>> ToRational(Ring<MultivariatePolynomial<E>> polyRing)
        {
            Rational<MultivariatePolynomial<E>> r = new Rational<MultivariatePolynomial<E>>(polyRing, numerator);
            r = r.Divide(denominatorConstantFactor);
            foreach (Factor<E> den in denominator)
                r = r.Divide(den.raised);
            return r;
        }
    }

    private sealed class Factor<E>
    {
        public readonly MultivariatePolynomial<E> factor;
        public readonly int exponent;
        public readonly MultivariatePolynomial<E> raised;

        public Factor(MultivariatePolynomial<E> factor, int exponent, MultivariatePolynomial<E> raised)
        {
            this.factor = exponent == 0 ? factor.CreateOne() : factor;
            this.exponent = exponent;
            this.raised = raised;
        }

        public Factor(MultivariatePolynomial<E> factor, int exponent)
        {
            this.factor = factor;
            this.exponent = exponent;
            this.raised = PolynomialMethods.PolyPow(factor, exponent, true);
        }

        public Factor<E> SetExponent(int newExponent)
        {
            if (exponent == newExponent)
                return this;
            if (exponent == 0)
                return new Factor<E>(factor.CreateOne(), 0, factor.CreateOne());
            if (newExponent % exponent == 0)
                return new Factor<E>(factor, newExponent, PolynomialMethods.PolyPow(raised, newExponent / exponent));
            return new Factor<E>(factor, newExponent);
        }

        public bool IsConstant()
        {
            return factor.IsConstant();
        }
    }
}