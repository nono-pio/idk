using System.Collections;
using System.Diagnostics;
using Polynomials.Linear;
using Polynomials.Poly.Univar;
using Polynomials.Utils;
using MultivariatePolynomialZp64 = Polynomials.Poly.Multivar.MultivariatePolynomial<long>;

namespace Polynomials.Poly.Multivar;

public static class HenselLifting
{
    
    private static UnivariatePolynomial<uE>[] monicExtendedEuclid<uE>(UnivariatePolynomial<uE> a, UnivariatePolynomial<uE> b) {
        var xgcd = UnivariateGCD.PolynomialExtendedGCD(a, b);
        if (xgcd[0].IsOne())
            return xgcd;

        // assert xgcd[0].isConstant() : "bad xgcd: " + Arrays.toString(xgcd) + " for xgcd(" + a + ", " + b + ")";

        //normalize: x * a + y * b = 1
        xgcd[2].DivideByLC(xgcd[0]);
        xgcd[1].DivideByLC(xgcd[0]);
        xgcd[0].Monic();

        return xgcd;
    }

    public static MultivariatePolynomial<E> primitivePart<E>(MultivariatePolynomial<E> poly) {
        // multivariate GCDs will be used for calculation of primitive part
        return MultivariatePolynomial<E>.AsMultivariate(poly.AsUnivariate(0).PrimitivePart(), 0);
    }


    static MultivariatePolynomialZp64 modImage(MultivariatePolynomialZp64 poly, int degree) {
        if (degree == 0)
            return poly.CcAsPoly();
        using var it = poly.terms.EntryIterator().GetEnumerator();
        while (it.MoveNext()) {
            var term = it.Current.Value;
            if (term.exponents.Skip(1).Sum() >= degree)
            {
                poly.terms.Remove(it.Current.Key);
                poly.Release();
            }
        }
        poly.Release();
        return poly;
    }


    public interface IEvaluation<E> {


        MultivariatePolynomial<E> evaluateFrom(MultivariatePolynomial<E> poly, int variable);

   
        MultivariatePolynomial<E> evaluateFromExcept(MultivariatePolynomial<E> poly, int from, int except);


        MultivariatePolynomial<E> evaluate(MultivariatePolynomial<E> poly, int variable);


        MultivariatePolynomial<E>[] evaluateFrom(MultivariatePolynomial<E>[] array, int variable) {
            MultivariatePolynomial<E>[] result = new MultivariatePolynomial<E>[array.Length];
            for (int i = 0; i < result.Length; i++)
                result[i] = evaluateFrom(array[i], variable);
            return result;
        }

        bool isZeroSubstitution(int variable);

  
        MultivariatePolynomial<E> taylorCoefficient(MultivariatePolynomial<E> poly, int variable, int order) {
            if (isZeroSubstitution(variable))
                return poly.CoefficientOf(variable, order);

            return evaluate(poly.SeriesCoefficient(variable, order), variable);
        }

        MultivariatePolynomial<E> linearPower(int variable, int exponent);

        MultivariatePolynomial<E> modImage(MultivariatePolynomial<E> poly, int variable, int idealExponent) {
            if (idealExponent == 0)
                return poly.Clone();
            int degree = poly.Degree(variable);
            if (idealExponent < degree - idealExponent) {
                // select terms
                MultivariatePolynomial<E> result = poly.CreateZero();
                for (int i = 0; i < idealExponent; i++) {
                    MultivariatePolynomial<E> term = evaluate(poly.SeriesCoefficient(variable, i), variable).Multiply(linearPower(variable, i));
                    if (term.IsZero())
                        continue;
                    result.Add(term);
                }
                return result;
            } else {
                // drop terms
                poly = poly.Clone();
                for (int i = idealExponent; i <= degree; i++) {
                    MultivariatePolynomial<E> term = evaluate(poly.SeriesCoefficient(variable, i), variable).Multiply(linearPower(variable, i));
                    if (term.IsZero())
                        continue;
                    poly.Subtract(term);
                }
                return poly;
            }
        }

        MultivariatePolynomial<E> modImage(MultivariatePolynomial<E> poly, int[] degreeBounds) {
            for (int i = 1; i < degreeBounds.Length; i++)
                poly = modImage(poly, i, degreeBounds[i] + 1);
            return poly;
        }

        IEvaluation<E> dropVariable(int variable);

        IEvaluation<E> renameVariables(int[] newVariablesExceptFirst);
    }

  
    sealed class Evaluation<E> : IEvaluation<E> {
        readonly E[] values;
        readonly int nVariables;
        readonly Ring<E> ring;
        readonly MultivariatePolynomial<E>.PrecomputedPowersHolder precomputedPowers;
        readonly MultivariatePolynomial<E>.USubstitution[] linearPowers;
        readonly IComparer<DegreeVector> ordering;

        Evaluation(int nVariables, E[] values, Ring<E> ring, IComparer<DegreeVector> ordering) {
            this.nVariables = nVariables;
            this.values = values;
            this.ring = ring;
            this.ordering = ordering;
            this.precomputedPowers = MultivariatePolynomial<E>.MkPrecomputedPowers(nVariables, ring, Utils.Utils.Sequence(1, nVariables), values);
            this.linearPowers = new MultivariatePolynomial<E>.USubstitution[nVariables - 1];
            for (int i = 0; i < nVariables - 1; i++)
                linearPowers[i] = new MultivariatePolynomial<E>.USubstitution(
                        UnivariatePolynomial<E>.CreateUnsafe(ring, [ring.Negate(values[i]), ring.GetOne()]),
                        i + 1, nVariables, ordering);
        }

        Evaluation<E> setRing(Ring<E> ring) {
            return new Evaluation<E>(nVariables, values, ring, ordering);
        }

        public MultivariatePolynomial<E> evaluate(MultivariatePolynomial<E> poly, int variable) {
            return poly.Evaluate(variable, precomputedPowers.powers[variable]);
        }

        public MultivariatePolynomial<E> evaluateFrom(MultivariatePolynomial<E> poly, int variable) {
            if (variable >= poly.nVariables)
                return poly.Clone();
            if (variable == 1 && poly.UnivariateVariable() == 0)
                return poly.Clone();
            return poly.Evaluate(precomputedPowers, Utils.Utils.Sequence(variable, nVariables));
        }

        public MultivariatePolynomial<E> evaluateFromExcept(MultivariatePolynomial<E> poly, int from, int except) {
            if (from >= poly.nVariables)
                return poly.Clone();
            if (from == 1 && poly.UnivariateVariable() == 0)
                return poly.Clone();

            int[] vars = new int[poly.nVariables - from - 1];
            int c = 0;
            for (int i = from; i < except; i++)
                vars[c++] = i;
            for (int i = except + 1; i < nVariables; i++)
                vars[c++] = i;

            return poly.Evaluate(precomputedPowers, vars);
        }

        public MultivariatePolynomial<E> linearPower(int variable, int exponent) {
            return linearPowers[variable - 1].Pow(exponent);
        }

        public bool isZeroSubstitution(int variable) {
            return ring.IsZero(values[variable - 1]);
        }

        public IEvaluation<E> dropVariable(int variable) {
            return new Evaluation<E>(nVariables - 1, Utils.Utils.Remove(values, variable - 1), ring, ordering);
        }

        public IEvaluation<E> renameVariables(int[] variablesExceptFirst) {
            return new Evaluation<E>(nVariables, map(ring, values, variablesExceptFirst), ring, ordering);
        }

        public string toString() {
            return values.ToString();
        }
    }

    private static  E[] map<E>(Ring<E> ring, E[] oldArray, int[] mapping) {
        E[] newArray = new E[oldArray.Length];
        for (int i = 0; i < oldArray.Length; i++)
            newArray[i] = oldArray[mapping[i]];
        return newArray;
    }
    
    sealed class AllProductsCache<Poly> where Poly : Polynomial<Poly> {
        public readonly Poly[] factors;
        readonly Dictionary<BitArray, Poly> products = new Dictionary<BitArray, Poly>();

        public AllProductsCache(Poly[] factors) {
            Debug.Assert(factors.Length >= 1);
            this.factors = factors;
        }

        private static BitArray clear(BitArray set, int from, int to) {
            set = (BitArray) set.Clone();
            for (int i = from; i < to; i++)
                set[i] = false;
            return set;
        }

        Poly multiply(BitArray selector) {
            int cardinality = selector.Cardinality();
            Debug.Assert(cardinality > 0);
            if (cardinality == 1)
                return factors[selector.NextSetBit(0)];
            
            if (products.TryGetValue(selector, out var cached))
                return cached;
            
            // split BitSet into two ~equal parts:
            int half = cardinality / 2;
            for (int i = 0; ; ++i) {
                if (selector[i])
                    --half;
                if (half == 0) {
                    products.Add(selector, cached =
                            multiply(clear(selector, 0, i + 1)).Clone()
                                    .Multiply(multiply(clear(selector, i + 1, factors.Length))));
                    return cached;
                }
            }
        }

        Poly multiply(int[] selector) {
            BitArray bits = new BitArray(factors.Length);
            for (int i = 0; i < selector.Length; i++)
                bits.Set(selector[i], true);
            return multiply(bits);
        }

        public int size() {
            return factors.Length;
        }

        public Poly get(int var) {
            return factors[var];
        }

        Poly except(int var) {
            BitArray bits = new BitArray(factors.Length);
            bits.SetAll(true);
            bits.Set(var, false);
            return multiply(bits);
        }

        public Poly from(int var) {
            BitArray bits = new BitArray(factors.Length);
            bits.Set(var, factors.Length, true);
            return multiply(bits);
        }

        public Poly[] exceptArray() {
            Poly[] arr = new Poly[factors.Length];
            for (int i = 0; i < arr.Length; i++)
                arr[i] = except(i);
            return arr;
        }

        Poly multiplyAll() {
            BitArray bits = new BitArray(factors.Length);
            bits.SetAll(true);
            return multiply(bits);
        }
    }

    sealed class UDiophantineSolver<E> {
        readonly UnivariatePolynomial<E> a, b;
        readonly UnivariatePolynomial<E> aCoFactor, bCoFactor;

        public UDiophantineSolver(UnivariatePolynomial<E> a, UnivariatePolynomial<E> b) {
            this.a = a;
            this.b = b;
            var xgcd = monicExtendedEuclid(a, b);
            this.aCoFactor = xgcd[1];
            this.bCoFactor = xgcd[2];
        }

        public UnivariatePolynomial<E> x;
        public UnivariatePolynomial<E> y;

        public void solve(UnivariatePolynomial<E> rhs) {
            x = aCoFactor.Clone().Multiply(rhs);
            y = bCoFactor.Clone().Multiply(rhs);

            var qd = UnivariateDivision.DivideAndRemainder(x, b, false);
            x = qd[1];
            y = y.Add(qd[0].Multiply(a));
        }
    }
    
    sealed class UMultiDiophantineSolver<E> {
        /** the given factors */
        readonly AllProductsCache<UnivariatePolynomial<E>> factors;
        readonly UDiophantineSolver<E>[] biSolvers;
        public readonly UnivariatePolynomial<E>[] solution;

        public UMultiDiophantineSolver(AllProductsCache<UnivariatePolynomial<E>> factors) {
            this.factors = factors;
            this.biSolvers = new UDiophantineSolver<E>[factors.size() - 1];
            for (int i = 0; i < biSolvers.Length; i++)
                biSolvers[i] = new UDiophantineSolver<E>(factors.get(i), factors.from(i + 1));
            this.solution = new UnivariatePolynomial<E>[factors.factors.Length];
        }

        public void solve(UnivariatePolynomial<E> rhs) {
            UnivariatePolynomial<E> tmp = rhs.Clone();
            for (int i = 0; i < factors.size() - 1; i++) {
                biSolvers[i].solve(tmp);
                solution[i] = biSolvers[i].y;
                tmp = biSolvers[i].x;
            }
            solution[factors.size() - 1] = tmp;
        }
    }
    
    sealed class MultiDiophantineSolver<E> {
        readonly IEvaluation<E> evaluation;
        readonly UMultiDiophantineSolver<E> uSolver;
        public readonly MultivariatePolynomial<E>[] solution;
        readonly int[] degreeBounds;
        readonly Ring<MultivariatePolynomial<E>> mRing;
        readonly UnivariatePolynomial<MultivariatePolynomial<E>>[, ] imageSeries;

        public MultiDiophantineSolver(IEvaluation<E> evaluation,
                               MultivariatePolynomial<E>[] factors,
                               UMultiDiophantineSolver<E> uSolver,
                               int[] degreeBounds,
                               int from) {
            Debug.Assert(from >= 1);
            this.evaluation = evaluation;
            this.uSolver = uSolver;
            this.degreeBounds = degreeBounds;
            var factory = factors[0];
            this.solution = new MultivariatePolynomial<E>[factors.Length];
            this.mRing = new MultivariateRing<E>(factors[0]);
            this.imageSeries = new UnivariatePolynomial<MultivariatePolynomial<E>>[factory.nVariables, factors.Length];
            for (int i = from - 1; i >= 1; --i)
                for (int j = 0; j < factors.Length; j++)
                    this.imageSeries[i, j] = seriesExpansion(mRing, evaluation.evaluateFrom(factors[j], i + 1), i, evaluation);
        }

        public void updateImageSeries(int liftingVariable, MultivariatePolynomial<E>[] factors) {
            for (int i = 0; i < factors.Length; i++)
                imageSeries[liftingVariable, i] = seriesExpansion(mRing, factors[i], liftingVariable, evaluation);
        }

        public void solve(MultivariatePolynomial<E> rhs, int liftingVariable) {
            if (rhs.IsZero()) {
                for (int i = 0; i < solution.Length; i++)
                    solution[i] = rhs.CreateZero();
                return;
            }
            rhs = evaluation.evaluateFrom(rhs, liftingVariable + 1);
            if (liftingVariable == 0) {
                uSolver.solve( rhs.AsUnivariate());
                for (int i = 0; i < solution.Length; i++)
                    solution[i] = MultivariatePolynomial<E>.AsMultivariate(uSolver.solution[i], rhs.nVariables, 0, rhs.ordering);
                return;
            }

            // solve equation with x_i replaced with b_i:
            // a[x1, ..., x(i-1), b(i), ... b(N)] * x[x1, ..., x(i-1), b(i), ... b(N)]
            //    + b[x1, ..., x(i-1), b(i), ... b(N)] * y[x1, ..., x(i-1), b(i), ... b(N)]
            //         = rhs[x1, ..., x(i-1), b(i), ... b(N)]
            solve(rhs, liftingVariable - 1);

            // <- x and y are now:
            // x = x[x1, ..., x(i-1), b(i), ... b(N)]
            // y = y[x1, ..., x(i-1), b(i), ... b(N)]

            UnivariatePolynomial<MultivariatePolynomial<E>> rhsSeries = seriesExpansion(mRing, rhs, liftingVariable, evaluation);
            var tmpSolution = new UnivariatePolynomial<MultivariatePolynomial<E>>[solution.Length];
            for (int i = 0; i < tmpSolution.Length; i++)
                tmpSolution[i] = seriesExpansion(mRing, solution[i], liftingVariable, evaluation);

            BernardinsTrick<MultivariatePolynomial<E>>[] pProducts = new BernardinsTrick<MultivariatePolynomial<E>>[solution.Length];
            for (int i = 0; i < solution.Length; i++)
                pProducts[i] = createBernardinsTrick(new UnivariatePolynomial<MultivariatePolynomial<E>>[]{tmpSolution[i], imageSeries[liftingVariable, i]}, degreeBounds[liftingVariable]);

            for (int degree = 1; degree <= degreeBounds[liftingVariable]; degree++) {
                // Δ = (rhs - a * x - b * y) mod (x_i - b_i)^degree
                var rhsDelta = rhsSeries.Get(degree);
                for (int i = 0; i < solution.Length; i++)
                    rhsDelta = rhsDelta.Subtract(pProducts[i].fullProduct().Get(degree));

                solve(rhsDelta, liftingVariable - 1);
                //assert x.isZero() || (x.degree(0) < b.degree(0)) : "\na:" + a + "\nb:" + b + "\nx:" + x + "\ny:" + y;

                // (x_i - b_i) ^ degree
                for (int i = 0; i < solution.Length; i++)
                    pProducts[i].update(solution[i], rhs.CreateZero());
            }

            for (int i = 0; i < solution.Length; i++)
                solution[i] = seriesToPoly(rhs, tmpSolution[i], liftingVariable, evaluation);
        }
    }

//    static <Poly extends IPolynomial<Poly>> void correctUnit(Poly poly, Poly[] factors) {
//        Poly lc = poly.lcAsPoly();
//        Poly flc = Arrays.stream(factors)
//                .map(IPolynomial::lcAsPoly)
//                .reduce(poly.createOne(), IPolynomial::multiply);
//        assert lc.isConstant();
//        assert flc.isConstant();
//
//        factors[0].multiplyByLC(lc.divideByLC(flc));
//    }

    static UnivariatePolynomial<UnivariatePolynomial<E>>[] bivariateLiftDense<E>(
            UnivariatePolynomial<UnivariatePolynomial<E>> baseSeries, UnivariatePolynomial<E>[] factors, int degreeBound) {
        AllProductsCache<UnivariatePolynomial<E>> uFactors = new AllProductsCache<UnivariatePolynomial<E>>(factors);
        // univariate multifactor diophantine solver
        UMultiDiophantineSolver<E> uSolver = new UMultiDiophantineSolver<E>(uFactors);

        UnivariatePolynomial<UnivariatePolynomial<E>>[] solution = new UnivariatePolynomial<UnivariatePolynomial<E>>[factors.Length];
        for (int i = 0; i < solution.Length; i++) {
            solution[i] = UnivariatePolynomial<UnivariatePolynomial<E>>.Constant(baseSeries.ring, uFactors.factors[i]);
            solution[i].EnsureInternalCapacity(degreeBound + 1);
        }

        BernardinsTrickWithoutLCCorrection<UnivariatePolynomial<E>> factorsProduct
                = new BernardinsTrickWithoutLCCorrection<UnivariatePolynomial<E>>(solution);
        for (int degree = 1; degree <= degreeBound; ++degree) {
            UnivariatePolynomial<E> rhsDelta = baseSeries.Get(degree).Clone().Subtract(factorsProduct.fullProduct().Get(degree));
            uSolver.solve(rhsDelta);
            factorsProduct.update(uSolver.solution);
        }

        return solution;
    }

    public static void bivariateLiftNoLCCorrection0<E>(MultivariatePolynomial<E> @base, MultivariatePolynomial<E>[] factors,
                                      IEvaluation<E> evaluation, int degreeBound) {
        Ring<UnivariatePolynomial<E>> uRing = new UnivariateRing<E>( factors[0].AsUnivariate());
        UnivariatePolynomial<UnivariatePolynomial<E>>[] res =
                bivariateLiftDense(seriesExpansionDense(uRing, @base, 1, evaluation),
                        asUnivariate(factors, evaluation), degreeBound);
        for (int i = 0; i < res.Length; i++)
            factors[i].Set(denseSeriesToPoly(@base, res[i], 1, evaluation));
    }

    private static void imposeLeadingCoefficients<E>(MultivariatePolynomial<E>[] factors, MultivariatePolynomial<E>[] factorsLC) {
        if (factorsLC != null)
            for (int i = 0; i < factors.Length; i++)
                if (factorsLC[i] != null)
                    factors[i].SetLC(0, factorsLC[i]);
    }

    static void bivariateLift0<E>(MultivariatePolynomial<E> @base, MultivariatePolynomial<E>[] factors, MultivariatePolynomial<E>[] factorsLC,
                        IEvaluation<E> evaluation, int degreeBound) {
        imposeLeadingCoefficients(factors, factorsLC);

        AllProductsCache<UnivariatePolynomial<E>> uFactors = new AllProductsCache<UnivariatePolynomial<E>>(asUnivariate(factors, evaluation));
        // univariate multifactor diophantine solver
        UMultiDiophantineSolver<E> uSolver = new UMultiDiophantineSolver<E>(uFactors);

        Ring<UnivariatePolynomial<E>> uRing = new UnivariateRing<E>(uFactors.get(0));
        UnivariatePolynomial<UnivariatePolynomial<E>> baseSeries = seriesExpansionDense(uRing, @base, 1, evaluation);
        UnivariatePolynomial<UnivariatePolynomial<E>>[] solution = new UnivariatePolynomial<UnivariatePolynomial<E>>[factors.Length];
        for (int i = 0; i < solution.Length; i++) {
            solution[i] = seriesExpansionDense(uRing, factors[i], 1, evaluation);
            solution[i].EnsureInternalCapacity(degreeBound + 1);
        }

        BernardinsTrick<UnivariatePolynomial<E>> product = createBernardinsTrick(solution, degreeBound);
        for (int degree = 1; degree <= degreeBound; ++degree) {
            var rhsDelta = baseSeries.Get(degree).Clone().Subtract(product.fullProduct().Get(degree));
            uSolver.solve(rhsDelta);
            product.update(uSolver.solution);
        }

        for (int i = 0; i < solution.Length; i++)
            factors[i].Set(denseSeriesToPoly(@base, solution[i], 1, evaluation));
    }


    private static UnivariatePolynomial<E>[] asUnivariate<E>(MultivariatePolynomial<E>[] array, IEvaluation<E> evaluation) {
        var u0 =  evaluation.evaluateFrom(array[0], 1).AsUnivariate();
        var res = new UnivariatePolynomial<E>[array.Length];
        res[0] = u0;
        for (int i = 1; i < array.Length; i++)
            res[i] =  evaluation.evaluateFrom(array[i], 1).AsUnivariate();
        return res;
    }

    private static MultivariatePolynomial<E>[] asMultivariate<E>(UnivariatePolynomial<E>[] array, int nVariables, int variable, IComparer<DegreeVector> ordering) {
        var u0 = MultivariatePolynomial<E>.AsMultivariate(array[0], nVariables, variable, ordering);
        var res = u0.CreateArray(array.Length);
        res[0] = u0;
        for (int i = 1; i < array.Length; i++)
            res[i] = MultivariatePolynomial<E>.AsMultivariate(array[i], nVariables, variable, ordering);
        return res;
    }

    public static UnivariatePolynomial<UnivariatePolynomial<E>> seriesExpansionDense<E>(Ring<UnivariatePolynomial<E>> ring, MultivariatePolynomial<E> poly, int variable, IEvaluation<E> evaluate) {
        int degree = poly.Degree(variable);
        var coefficients = new UnivariatePolynomial<E>[degree + 1];
        for (int i = 0; i <= degree; i++)
            coefficients[i] =  evaluate.taylorCoefficient(poly, variable, i).AsUnivariate();
        return UnivariatePolynomial<UnivariatePolynomial<E>>.CreateUnsafe(ring, coefficients);
    }
    
    static MultivariatePolynomial<E> denseSeriesToPoly<E>(MultivariatePolynomial<E> factory, UnivariatePolynomial<UnivariatePolynomial<E>> series, int seriesVariable, IEvaluation<E> evaluation) {
        var result = factory.CreateZero();
        for (int i = 0; i <= series.Degree(); i++) {
            var mPoly = MultivariatePolynomial<E>.AsMultivariate(series.Get(i), factory.nVariables, 0, factory.ordering);
            result = result.Add(mPoly.Multiply(evaluation.linearPower(seriesVariable, i)));
        }
        return result;
    }

    public static void multivariateLiftAutomaticLC<E>(MultivariatePolynomial<E> @base,
                                     MultivariatePolynomial<E>[] factors,
                                     IEvaluation<E> evaluation) {
        var lc = @base.Lc(0);

        if (lc.IsConstant())
            multivariateLift0(@base, factors, null, evaluation, @base.Degrees());
        else {
            // imposing leading coefficients
            var lcCorrection = evaluation.evaluateFrom(lc, 1);
            Debug.Assert(lcCorrection.IsConstant());

            foreach (var factor in factors) {
                Debug.Assert(factor.Lt().exponents[0] == factor.Degree(0));
                factor.MonicWithLC(lcCorrection.LcAsPoly());
            }

            @base = @base.Clone().Multiply(PolynomialMethods.PolyPow(lc, factors.Length - 1, true));

            multivariateLift0(@base, factors, Utils.Utils.ArrayOf(lc, factors.Length), evaluation, @base.Degrees());

            foreach (var factor in factors)
                factor.Set(primitivePart(factor));
        }
    }


    public static void multivariateLiftAutomaticLC<E>(MultivariatePolynomial<E> @base,
        MultivariatePolynomial<E>[] factors,
                                     IEvaluation<E> evaluation,
                                     int from) {
        var lc = @base.Lc(0);

        if (lc.IsConstant())
            multivariateLift0(@base, factors, null, evaluation, @base.Degrees());
        else {
            // imposing leading coefficients
            var lcCorrection = evaluation.evaluateFrom(lc, from);

            foreach (var factor in factors) {
                Debug.Assert(factor.Lt().exponents[0] == factor.Degree(0));
                factor.Multiply(MultivariateDivision.DivideExact(lcCorrection, factor.Lc(0)));
            }

            @base = @base.Clone().Multiply(PolynomialMethods.PolyPow(lc, factors.Length - 1, true));

            multivariateLift0(@base, factors, Utils.Utils.ArrayOf(lc, factors.Length), evaluation, @base.Degrees(), from);

            foreach (var factor in factors)
                factor.Set(primitivePart(factor));
        }
    }


    public static void multivariateLift0<E>(MultivariatePolynomial<E> @base,
        MultivariatePolynomial<E>[] factors, MultivariatePolynomial<E>[] factorsLC,
                           IEvaluation<E> evaluation,
                           int[] degreeBounds) {
        multivariateLift0(@base, factors, factorsLC, evaluation, degreeBounds, 1);
    }

    private const double SPARSITY_THRESHOLD = 0.1;

    public static void multivariateLift0<E>(MultivariatePolynomial<E> @base,
        MultivariatePolynomial<E>[] factors, MultivariatePolynomial<E>[] factorsLC,
                           IEvaluation<E> evaluation,
                           int[] degreeBounds,
                           int from) {
        if (@base.nVariables == 2) {
            bivariateLift0(@base, factors, factorsLC, evaluation, degreeBounds[1]);
            return;
        }

        if (from == 2 && @base.Sparsity() < SPARSITY_THRESHOLD) {
            MultivariatePolynomial<E>[]? sparseFactors = null;

            try {
                // ArithmeticException may raise in rare cases in Zp[X] where p is not a prime
                // (exception actually arises when calculating some GCDs, since some non-unit elements
                //  may be picked up from at random from Zp in randomized methods)
                sparseFactors = sparseLifting(@base, factors, factorsLC);
            } catch (ArithmeticException ignore) { }

            if (sparseFactors != null) {
                Array.Copy(sparseFactors, 0, factors, 0, factors.Length);
                return;
            }
        }

        imposeLeadingCoefficients(factors, factorsLC);

        AllProductsCache<UnivariatePolynomial<E>> uFactors = new AllProductsCache<UnivariatePolynomial<E>>(asUnivariate(factors, evaluation));
        // univariate multifactor diophantine solver
        UMultiDiophantineSolver<E> uSolver = new UMultiDiophantineSolver<E>(uFactors);

        // initialize multivariate multifactor diophantine solver
        MultiDiophantineSolver<E> dSolver = new MultiDiophantineSolver<E>(
                evaluation,
                from == 1
                        ? asMultivariate(uFactors.exceptArray(), @base.nVariables, 0, @base.ordering)
                        : new AllProductsCache<MultivariatePolynomial<E>>(factors).exceptArray(),
                uSolver, degreeBounds, from);

        Ring<MultivariatePolynomial<E>> mRing = new MultivariateRing<E>(factors[0]);
        for (int liftingVariable = from; liftingVariable < @base.nVariables; ++liftingVariable) {
            var baseImage = evaluation.evaluateFrom(@base, liftingVariable + 1);
           var baseSeries = seriesExpansion(mRing, baseImage, liftingVariable, evaluation);
           var solution = new UnivariatePolynomial<MultivariatePolynomial<E>>[factors.Length];
            for (int i = 0; i < solution.Length; i++) {
                solution[i] = seriesExpansion(mRing, factors[i], liftingVariable, evaluation);
                solution[i].EnsureInternalCapacity(degreeBounds[liftingVariable] + 1);
            }

            BernardinsTrick<MultivariatePolynomial<E>> product = createBernardinsTrick(solution, degreeBounds[liftingVariable]);
            for (int degree = 1; degree <= degreeBounds[liftingVariable]; ++degree) {
                var rhsDelta = baseSeries.Get(degree).Clone().Subtract(product.fullProduct().Get(degree));
                dSolver.solve(rhsDelta, liftingVariable - 1);
                product.update(dSolver.solution);
            }

            for (int i = 0; i < solution.Length; i++)
                factors[i].Set(seriesToPoly(@base, solution[i], liftingVariable, evaluation));

            if (liftingVariable < @base.nVariables) // don't perform on the last step
                dSolver.updateImageSeries(liftingVariable, new AllProductsCache<MultivariatePolynomial<E>>(
                        evaluation.evaluateFrom(factors, liftingVariable + 1))
                        .exceptArray());
        }
    }


    static UnivariatePolynomial<MultivariatePolynomial<E>> seriesExpansion<E>(Ring<MultivariatePolynomial<E>> ring, MultivariatePolynomial<E> poly, int variable, IEvaluation<E> evaluate) {
        int degree = poly.Degree(variable);
        var coefficients = new MultivariatePolynomial<E>[degree + 1];
        for (int i = 0; i <= degree; i++)
            coefficients[i] = evaluate.taylorCoefficient(poly, variable, i);
        return UnivariatePolynomial<MultivariatePolynomial<E>>.CreateUnsafe(ring, coefficients);
    }

    static MultivariatePolynomial<E> seriesToPoly<E>(MultivariatePolynomial<E> factory, UnivariatePolynomial<MultivariatePolynomial<E>> series, int seriesVariable, IEvaluation<E> evaluation) {
        var result = factory.CreateZero();
        for (int i = 0; i <= series.Degree(); i++) {
            var mPoly = series.Get(i);
            result = result.Add(mPoly.Multiply(evaluation.linearPower(seriesVariable, i)));
        }
        return result;
    }

    /*=========================== Bernardin's trick for fast error computation in lifting =============================*/

    static BernardinsTrick<Poly> createBernardinsTrick<Poly>(UnivariatePolynomial<Poly>[] factors, int degreeBound) where Poly : Polynomial<Poly> {
        if (factors.All(fac => fac.IsConstant()))
            return new BernardinsTrickWithoutLCCorrection<Poly>(factors);
        else
            return new BernardinsTrickWithLCCorrection<Poly>(factors, degreeBound);
    }

    public abstract class BernardinsTrick<Poly> {
        public readonly UnivariatePolynomial<Poly>[] factors;
        public readonly UnivariatePolynomial<Poly>[] partialProducts;
        public readonly Ring<Poly> ring;

        public BernardinsTrick(params UnivariatePolynomial<Poly>[] factors) {
            this.factors = factors;
            this.partialProducts = new UnivariatePolynomial<Poly>[factors.Length - 1];
            this.ring = factors[0].ring;

            partialProducts[0] = factors[0].Clone().Multiply(factors[1]);
            for (int i = 1; i < partialProducts.Length; i++)
                partialProducts[i] = partialProducts[i - 1].Clone().Multiply(factors[i + 1]);
        }

        public abstract void update(params Poly[] updates);

        public UnivariatePolynomial<Poly> fullProduct() {return partialProducts[partialProducts.Length - 1];}

    UnivariatePolynomial<Poly> partialProduct(int i) {
            return partialProducts[i];
        }
    }

    /** Bernardin's trick for fast f_0 * f_1 * ... * f_N computing (leading coefficients are discarded) */
    public sealed class BernardinsTrickWithoutLCCorrection<Poly>
            : BernardinsTrick<Poly> where Poly : Polynomial<Poly> {
        public BernardinsTrickWithoutLCCorrection(UnivariatePolynomial<Poly>[] factors) : base(factors) {
        }

        private int pDegree = 0;

        public override void update(params Poly[] updates) {
            ++pDegree;
            // update factors
            for (int i = 0; i < factors.Length; i++)
                factors[i].Set(pDegree, updates[i]);

            // update the first product: factors[0] * factors[1]
            // k-th element is updated by (factors[0]_k * factors[1]_0 + factors[0]_0 * factors[1]_k)
            Poly updateValue = factors[0].Get(pDegree).Clone().Multiply(factors[1].Get(0))
                    .Add(factors[1].Get(pDegree).Clone().Multiply(factors[0].Get(0)));
            partialProducts[0].AddMonomial(updateValue, pDegree);

            // (k+1)-th element is calculated as (factors[0]_1*factors[1]_k + ... + factors[0]_k*factors[1]_1)
            Poly newElement = ring.GetZero();
            for (int i = 1; i <= pDegree; i++)
                newElement.Add(factors[0].Get(i).Clone().Multiply(factors[1].Get(pDegree - i + 1)));
            partialProducts[0].Set(pDegree + 1, newElement);

            // => the first product (factors[0] * factors[1]) is updated
            // update other partial products accordingly
            for (int j = 1; j < partialProducts.Length; j++) {

                // k-th element is updated by (update(p_k) * factors[j+1]_0 + p_0 * factors[j+1]_k),
                // where p is the previous partial product (without factors[j+1]) and
                // update(p_k) is the k-th element update of the previous partial product
                Poly currentUpdate =
                        partialProducts[j - 1].Get(0).Clone().Multiply(factors[j + 1].Get(pDegree))
                                .Add(updateValue.Multiply(factors[j + 1].Get(0)));
                partialProducts[j].AddMonomial(currentUpdate, pDegree);
                // cache current update for the next cycle
                updateValue = currentUpdate;

                // (k+1)-th element is calculated as (p[0]_1*factors[1]_k + ... + p[0]_k*factors[1]_1 + p[0]_(k+1)*factors[1]_0)
                newElement = ring.GetZero();
                for (int i = 1; i <= (pDegree + 1); i++)
                    newElement.Add(partialProducts[j - 1].Get(i).Clone().Multiply(factors[j + 1].Get(pDegree - i + 1)));
                partialProducts[j].Set(pDegree + 1, newElement);
            }
        }
    }

    /** Bernardin's trick for fast f_0 * f_1 * ... * f_N computing (leading coefficients are took into account) */
    sealed class BernardinsTrickWithLCCorrection<Poly>
            : BernardinsTrick<Poly> where Poly : Polynomial<Poly> {
        readonly int degreeBound;

        public BernardinsTrickWithLCCorrection(UnivariatePolynomial<Poly>[] factors, int degreeBound) : base(factors) {
            this.degreeBound = degreeBound;
        }

        // current lift, so that factors are known mod I^pDegree
        private int pDegree = 0;

        private void updatePair(int iFactor, Poly leftUpdate, Poly rightUpdate, int degree) {
            // update factors
            UnivariatePolynomial<Poly>
                    left = iFactor == 0 ? factors[0] : partialProducts[iFactor - 1],
                    right = (iFactor + 1 < factors.Length) ? factors[iFactor + 1] : null;

            if (leftUpdate != null) {
                left.AddMonomial(leftUpdate, degree);
                if (iFactor < (factors.Length - 1))
                    for (int i = degree; i <= Math.Min(degreeBound, degree + right.Degree()); i++)
                        updatePair(iFactor + 1, right.Get(i - degree).Clone().Multiply(leftUpdate), null, i);
            }

            if (rightUpdate != null) {
                right.AddMonomial(rightUpdate, degree);
                if (iFactor < (factors.Length - 1))
                    for (int i = degree; i <= Math.Min(degreeBound, degree + left.Degree()); i++)
                        updatePair(iFactor + 1, left.Get(i - degree).Clone().Multiply(rightUpdate), null, i);
            }
        }

        public override void update(params Poly[] updates) {
            ++pDegree;
            updatePair(0, updates[0], updates[1], pDegree);
            for (int i = 0; i < factors.Length - 2; i++)
                updatePair(i + 1, null, updates[i + 2], pDegree);
        }
    }


    /*=========================== Sparse Hensel lifting from bivariate factors =============================*/

    private const long MAX_TERMS_IN_EXPAND_FORM = 8_388_608L;

    static MultivariatePolynomial<E>[] sparseLifting<E>(MultivariatePolynomial<E> @base, MultivariatePolynomial<E>[] biFactors, MultivariatePolynomial<E>[] lc) {
        List<List<Monomial<E>>>
                // terms that are fixed
                determinedTerms = [],
                // terms with unknowns
                undeterminedTerms = [];

        // total number of unknown terms
        int nUnknowns = 0;
        long estimatedExpandedSize = 1;
        for (int i = 0; i < biFactors.Length; ++i) {
            List<Monomial<E>>
                    // fixed terms
                    @fixed = new List<Monomial<E>>(),
                    // unknown terms
                    unknown = new List<Monomial<E>>();

            determinedTerms.Add(@fixed);
            undeterminedTerms.Add(unknown);

            populateUnknownTerms(biFactors[i], lc == null ? @base.CreateOne() : lc[i], @fixed, unknown);
            nUnknowns += unknown.Count;
            estimatedExpandedSize *= unknown.Count;
        }

        if (estimatedExpandedSize > 1024L * @base.Size() || estimatedExpandedSize > MAX_TERMS_IN_EXPAND_FORM)
            // too large problem for sparse lifting -> probably we will run out of memory
            return null;

        // true factors represented as R[x0, x1, x2, ..., xN, u1, ..., uK]
        List<MultivariatePolynomial<E>> trueFactors = new List<MultivariatePolynomial<E>>();
        int unkCounter = @base.nVariables;
        for (int i = 0; i < biFactors.Length; i++) {
            var trueFactor = @base.CreateZero().JoinNewVariables(nUnknowns);
            foreach (var f in determinedTerms[i])
                trueFactor.Add(f.JoinNewVariables(nUnknowns));
            for (int j = 0; j < undeterminedTerms[i].Count; j++) {
                trueFactor.Add(undeterminedTerms[i][j].JoinNewVariables(nUnknowns)
                        .Set(unkCounter, 1));
                ++unkCounter;
            }
            trueFactors.Add(trueFactor);
        }

        // multiply our trueFactors in R[x0, x1, x2, ..., xN, u1, ..., uK]
        var lhsBase = trueFactors.Aggregate(trueFactors[0].CreateOne(), (a, b) => a.Multiply(b));

        // <- matching lhsBase and base in (x0, x1)
        // base as R[x0, x1][x0, x1, x2, ... xN]
        MultivariatePolynomial<MultivariatePolynomial<E>> biBase =
                @base.AsOverMultivariate(Utils.Utils.Sequence(2, @base.nVariables));
        // The main equations to solve
        List<Equation<E>> equations = [];
        foreach (Monomial<MultivariatePolynomial<E>> rhs in biBase.terms) {
            var cf = lhsBase.DropCoefficientOf(new int[]{0, 1}, rhs.exponents[..2]);
            Equation<E> eq = new Equation<E>(cf, rhs.coefficient);

            if (!eq.isConsistent())
                // inconsistent equation -> bad lifting
                return null;

            if (!eq.isIdentity())
                // don't add identities
                equations.Add(eq.canonical());
        }

        if (!lhsBase.IsZero()) {
            Equation<E> eq = new Equation<E>(lhsBase, biBase.ring.GetZero());
            if (!eq.isIdentity())
                equations.Add(eq.canonical());
        }

        // all solutions we obtained so far
        List<BlockSolution<E>> solutions = [];
        while (equations.Count != 0) {
            main: ;

            // canonicalize all equations and rid out identical ones
            equations = new List<Equation<E>>(equations.Select(eq => eq.canonical()));

            // sort equations, so the first has less unknowns
            equations.Sort(new Utils.Utils.ComparerBy<Equation<E>>(eq => eq.nUnknowns));
            
            Debug.Assert( equations.All(eq => eq.isConsistent()));
            // filter linear equations
            List<Equation<E>> linear =
                    equations.Where(eq => eq.isLinear()).ToList();

            if (linear.Count == 0)
                // no any linear solutions more -> can't solve
                return null;

            // search for the block of linear equations
            List<Equation<E>> block = new List<Equation<E>>();
            for (int i = 0; i < linear.Count; i++) {
                // take the base equation
                Equation<E> baseEq = linear[i];
                block.Add(baseEq);
                if (block.Count < baseEq.nUnknowns)
                    for (int j = 0; j < linear.Count; j++) {
                        if (i == j)
                            continue;

                        Equation<E> eq2 = linear[j];
                        if (!eq2.hasOtherUnknownsThan(baseEq))
                            block.Add(eq2);
                    }

                if (block.Count >= baseEq.nUnknowns) {
                    BlockSolution<E> blockSolution = solveBlock(block);
                    if (blockSolution != null) {
                        solutions.Add(blockSolution);
                        // remove all solved equations
                        equations.RemoveAll(block);
                        for (int k = 0; k < equations.Count; k++) {
                            Equation<E> eq = equations[k].substituteSolutions(blockSolution);
                            if (!eq.isConsistent())
                                return null;
                            equations[k] = eq;
                        }

                        Debug.Assert(equations.All(eq => eq.isConsistent()));

                        // remove identity equations
                        equations.RemoveAll(equations.Where(eq => eq.isIdentity()).ToList());
                        goto main;
                    }
                }

                block.Clear();
            }

            // no any solvable blocks
            return null;
        }

        return trueFactors.Select(
                factor => {
                    // factor as R[x0, ..., xN][u1, ..., uK]
                    MultivariatePolynomial<MultivariatePolynomial<E>> result =
                            factor.AsOverMultivariateEliminate(Utils.Utils.Sequence(0, @base.nVariables));
                    foreach (BlockSolution<E> solution in solutions)
                        result = result.Evaluate(solution.unknowns, solution.solutions);

                    Debug.Assert(result.IsConstant());
                    return result.Cc();
                }).ToArray();
    }

    static BlockSolution<E> solveBlock<E>(List<Equation<E>> block) {
        Equation<E> baseEq = block[0];
        // unknown variables (indexed from zero)
        int[] unknowns = baseEq.getUnknowns();

        var factory = baseEq.rhs;
        // polynomial ring
        PolynomialRing<MultivariatePolynomial<E>> polyRing = new MultivariateRing<E>(factory);
        // field of rational functions
        Ring<Rational<MultivariatePolynomial<E>>> fracRing = new Rationals<MultivariatePolynomial<E>>(polyRing);

        // sort so the the first row will have maximal number of unknowns
        // this helps to solve system faster (less GCD's)
        block.Sort(new Utils.Utils.ComparerBy<Equation<E>>(eq => -eq.nUnknowns)); //Comparator.comparing(eq => -eq.nUnknowns));

        // lhs matrix
        var lhs = new List<Rational<MultivariatePolynomial<E>>[]>();
        // rhs column
        var rhs = new List<Rational<MultivariatePolynomial<E>>>();

        // try square system first
        for (int i = 0; i < unknowns.Length; ++i)
            addRow(polyRing, block[i], lhs, rhs, unknowns);

        var rSolution = new Rational<MultivariatePolynomial<E>>[unknowns.Length];
        while (true) {
            // convert to matrix
            Rational<MultivariatePolynomial<E>>[][] lhsMatrix = lhs.ToArray();
            Rational<MultivariatePolynomial<E>>[] rhsColumn = rhs.ToArray();

            LinearSolver.SystemInfo info = LinearSolver.Solve(fracRing, lhsMatrix.AsArray2D(), rhsColumn, rSolution);
            if (info == LinearSolver.SystemInfo.Consistent)
                break;
            if (info == LinearSolver.SystemInfo.Inconsistent)
                return null;
            if (info == LinearSolver.SystemInfo.UnderDetermined) {
                // update matrix data with fresh reduced system
                lhs.Clear();
                lhs.AddRange(lhsMatrix.ToList());
                rhs.Clear();
                rhs.AddRange(rhsColumn.ToList());
                if (block.Count <= rhs.Count)
                    return null;
                addRow(polyRing, block[rhs.Count], lhs, rhs, unknowns);
                continue;
            }
        }

        // real solution
        var solution = new MultivariatePolynomial<E>[rSolution.Length];
        for (int i = 0; i < rSolution.Length; i++) {
            Rational<MultivariatePolynomial<E>> r = rSolution[i];
            if (!r.Denominator().IsOne()) {
                // bad luck
                return null;
            }
            solution[i] = r.Numerator();
        }

        BlockSolution<E> blockSolution = new BlockSolution<E>(unknowns, solution);
        // check the solution
        if (rhs.Count < block.Count)
            for (int i = rhs.Count; i < block.Count; i++)
                if (!block[i].substituteSolutions(blockSolution).isConsistent())
                    return null;

        return blockSolution;
    }

    static void addRow<E>(PolynomialRing<MultivariatePolynomial<E>> polyRing, Equation<E> eq,
                List<Rational<MultivariatePolynomial<E>>[]> lhs, List<Rational<MultivariatePolynomial<E>>> rhs, int[] unknowns) {
        var row = new Rational<MultivariatePolynomial<E>>[unknowns.Length];
        for (int j = 0; j < row.Length; j++) {
            // lhs matrix element
            MultivariatePolynomial<MultivariatePolynomial<E>> el = eq.lhs.CoefficientOf(unknowns[j], 1);
            Debug.Assert(el.Size() <= 1);
            row[j] = new Rational<MultivariatePolynomial<E>>(polyRing, el.Cc());
        }

        lhs.Insert(0, row);
        rhs.Insert(0, new Rational<MultivariatePolynomial<E>>(polyRing, eq.rhs));
    }

    sealed class BlockSolution<E> {
        public readonly int[] unknowns;
        public readonly MultivariatePolynomial<E>[] solutions;

        public BlockSolution(int[] unknowns, MultivariatePolynomial<E>[] solutions) {
            this.unknowns = unknowns;
            this.solutions = solutions;
        }

        public String toString() {
            return "solution: " + solutions.ToString();
        }
    }

    sealed class Equation<E> {
        public readonly MultivariatePolynomial<MultivariatePolynomial<E>> lhs;
        public readonly MultivariatePolynomial<E> rhs;
        readonly int[] lhsDegrees;
        readonly bool isLinear_;
        public readonly int nUnknowns;

        public Equation(MultivariatePolynomial<E> lhs, MultivariatePolynomial<E> rhs) :this(lhs.AsOverMultivariateEliminate(Utils.Utils.Sequence(0, rhs.nVariables)), rhs) {
        }

        public Equation(MultivariatePolynomial<MultivariatePolynomial<E>> lhs, MultivariatePolynomial<E> rhs) {
            rhs.Subtract(lhs.Cc());
            lhs.Subtract(lhs.Cc());
            this.lhs = lhs;
            this.rhs = rhs;
            this.lhsDegrees = this.lhs.Degrees();
            this.isLinear_ = lhs.GetSkeleton().All(s => s.totalDegree <= 1);
            int nUnknowns = 0;
            foreach (int lhsDegree in lhsDegrees)
                nUnknowns += lhsDegree == 0 ? 0 : 1;
            this.nUnknowns = nUnknowns;
        }

        public bool isConsistent() {
            return !isIdentity() || lhs.Cc().Equals(rhs);
        }

        public bool hasOtherUnknownsThan(Equation<E> other) {
            for (int i = 0; i < lhsDegrees.Length; i++)
                if (lhsDegrees[i] > other.lhsDegrees[i])
                    return true;
            return false;
        }

        public bool isIdentity() { return lhsDegrees.Sum() == 0;}

        public bool isLinear() { return isLinear_;}

        public int[] getUnknowns() {
            int[] unknowns = new int[nUnknowns];
            int i = -1;
            for (int j = 0; j < lhsDegrees.Length; j++)
                if (lhsDegrees[j] != 0)
                    unknowns[++i] = j;
            return unknowns;
        }

        public Equation<E> substituteSolutions(BlockSolution<E> solution) {
            // IMPORTANT: invocation of clone() is very important since rhs is modified in constructor!
            return new Equation<E>(lhs.Evaluate(solution.unknowns, solution.solutions), rhs.Clone());
        }

        public Equation<E> canonical() {
            var gcd = MultivariateGCD.PolynomialGCD(lhs.Content(), rhs);
            if (rhs.IsOverField() && !rhs.IsZero()) {
                lhs.DivideExact(rhs.LcAsPoly());
                rhs.Monic();
            }
            return new Equation<E>(lhs.DivideExact(gcd), MultivariateDivision.DivideExact(rhs, gcd));
        }

        public String toString() {
            return lhs.ToString() + " = " + rhs;
        }

        public bool equals(Object o) {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;

            Equation<E> equation = (Equation<E>) o;

            if (!lhs.Equals(equation.lhs)) return false;
            return rhs.Equals(equation.rhs);
        }

        public override int GetHashCode() {
            int result = lhs.GetHashCode();
            result = 31 * result + rhs.GetHashCode();
            return result;
        }
    }

    static void populateUnknownTerms<E>(MultivariatePolynomial<E> biPoly, MultivariatePolynomial<E> lc, List<Monomial<E>> @fixed, List<Monomial<E>> unknown) {
        // degree in x0
        int xDeg = biPoly.Degree(0);
        foreach (var term in biPoly.terms) {
            if (term.exponents[0] == xDeg) {
                var cf = lc.CoefficientOf(1, term.exponents[1]);
                biPoly.terms[term] = term.SetCoefficientFrom(cf.monomialAlgebra.GetUnitTerm(cf.nVariables));
                @fixed.AddRange(cf.Multiply(term).Collection());
            } else
                unknown.Add(term);
        }
    }
}

