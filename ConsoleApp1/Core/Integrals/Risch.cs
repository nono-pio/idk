using System.Diagnostics;
using System.Numerics;
using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Expressions.Base;
using ConsoleApp1.Core.Expressions.Trigonometrie;
using Polynomials;
using Polynomials.Poly.Multivar;
using Polynomials.Poly.Univar;
using MPoly =
    Polynomials.Poly.Multivar.MultivariatePolynomial<
        Polynomials.Rational<System.Numerics.BigInteger>>; // QQ[x, t0, ...]
using RMPoly =
    Polynomials.Rational<Polynomials.Poly.Multivar.MultivariatePolynomial<
        Polynomials.Rational<System.Numerics.BigInteger>>>; // QQ(x, t0, ...)
using UPoly =
    Polynomials.Poly.Univar.UnivariatePolynomial<Polynomials.Rational<Polynomials.Poly.Multivar.MultivariatePolynomial<
        Polynomials.Rational<System.Numerics.BigInteger>>>>; // QQ(x, t0, ...)[ti]
using RUPoly =
    Polynomials.Rational<Polynomials.Poly.Univar.UnivariatePolynomial<Polynomials.Rational<
        Polynomials.Poly.Multivar.MultivariatePolynomial<
            Polynomials.Rational<System.Numerics.BigInteger>>>>>; // QQ(x, t0, ...)(ti)


namespace ConsoleApp1.Core.Integrals;

public static class Risch
{
    public static Rationals<BigInteger> QQ = Rings.Q;
    public static Ring<BigInteger> ZZ = Rings.Z;

    public enum RischCases
    {
        Base,
        Primitive,
        Exp,
        Tan,
        OtherNonlinear,
        OtherLinear
    }

    public class DiffField
    {
        public Variable[] t;
        public Expr[] T;
        public Expr[] D;
        public RMPoly[] Dtemp;
        public RischCases[] Cases;
        public Variable x => t[0];
        public Expr f;

        public DiffField(Expr f, Variable x)
        {
            List<Variable> t = [x];
            List<Expr> T = [x];
            List<Expr> D = [1];
            this.f = Components(f, ref t, ref T, ref D);

            this.t = t.ToArray();
            this.T = T.ToArray();
            this.D = D.ToArray();
            this.Dtemp = D.Select(ExprToRMPoly).ToArray();

            SetCases();
        }

        public void SetCases()
        {
            RischCases GetCase(int t, RMPoly d)
            {
                if (d.IsConstant(t))
                {
                    if (d.IsOne())
                        return RischCases.Base;
                    return RischCases.Primitive;
                }

                if (!d.IsIntegral())
                    throw new NotImplementedException();

                if ((d.NumeratorExact() % ExprToMPoly(this.t[t])).IsZero())
                    return RischCases.Exp;

                if ((d.NumeratorExact() % ExprToMPoly(Pow(this.t[t], 2) + 1)).IsZero())
                    return RischCases.Tan;

                if (d.NumeratorExact().Degree(t) > 1)
                    return RischCases.OtherNonlinear;
                return RischCases.OtherLinear;
            }

            this.Cases = new RischCases[this.T.Length];
            for (int i = 0; i < Cases.Length; i++)
            {
                Cases[i] = GetCase(i, Dtemp[i]);
            }
        }


        public static Expr Components(Expr f, ref List<Variable> t, ref List<Expr> T, ref List<Expr> D)
        {
            if (f.Constant(t[0]))
                return f;

            if (f.IsVar(t[0]))
                return f;

            var ti = T.IndexOf(f);
            if (ti != -1)
                return t[ti];

            switch (f)
            {
                case Addition:
                case Multiplication:
                    var args = new Expr[f.Args.Length];
                    for (int i = 0; i < args.Length; i++)
                        args[i] = Components(f.Args[i], ref t, ref T, ref D);
                    return f.Eval(args);

                case Power pow:
                    if (pow.Exp is Number n && n.Num.IsInt)
                    {
                        var newBase = Components(pow.Base, ref t, ref T, ref D);
                        return Pow(newBase, n);
                    }

                    break;
            }

            // not Rational Polynomial

            var newt = new Variable("t" + t.Count, dummy: true);

            t.Add(newt);
            T.Add(f);

            var DT = f.Derivee(t[0]);
            D.Add(Components(DT, ref t, ref T, ref D));

            return newt;
        }

        public RMPoly ExprToRMPoly(Expr expr)
        {
            var (n, d) = expr.AsFraction();
            return PolynomialFactory.RationalPoly(ExprToMPoly(n), ExprToMPoly(d));
        }

        public MPoly ExprToMPoly(Expr expr)
        {
            var n = t.Length;

            switch (expr)
            {
                case Number num:
                    if (!num.Num.IsFraction)
                        goto default;

                    return PolynomialFactory.Multi(QQ,
                        (new Rational<BigInteger>(ZZ, num.Num.Numerator, num.Num.Denominator), new int[n]));
                case Variable var:

                    var ti = Array.IndexOf(t, var);
                    if (ti == -1)
                        goto default;

                    var degs = new int[n];
                    degs[ti] = 1;

                    return PolynomialFactory.Multi(QQ, (1, degs));
                case Addition add:
                    return add.Args.Select(ExprToMPoly).Aggregate((a, b) => a.Add(b));
                case Multiplication mul:
                    return mul.Args.Select(ExprToMPoly).Aggregate((a, b) => a.Multiply(b));
                case Power pow:
                    var exp = pow.Exp is Number number && number.Num.IsInt
                        ? (int)number.Num.Numerator
                        : throw new NotImplementedException();

                    return ExprToMPoly(pow.Base).Pow(exp);
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public static Expr Derivative(Expr f, DiffField D)
    {
        if (f.Constant())
            return 0;

        if (f is Variable var)
        {
            var ti = Array.IndexOf(D.t, var);
            if (ti != -1)
                return D.D[ti];

            throw new NotImplementedException();
        }

        Expr result = 0;
        for (int i = 0; i < f.Args.Length; i++)
        {
            result += f.fDerivee(i) * Derivative(f.Args[i], D);
        }

        return result;
    }

    public static RMPoly Derivative(RMPoly f, DiffField D)
    {
        return (Derivative(f.Numerator(), D) * f.Denominator().ToRMPoly() -
                Derivative(f.Denominator(), D) * f.Numerator().ToRMPoly()) / (f.Denominator().Square()).ToRMPoly();
    }

    public static RMPoly Derivative(MPoly f, DiffField D)
    {
        var result = RMPoly.Zero(Rings.MultivariateRingQ(D.t.Length));
        for (int i = 0; i < f.nVariables; i++)
        {
            result += f.Derivative(i) * D.Dtemp[i];
        }
        
        return result;
    }

    private static RUPoly Derivative(UPoly a, DiffField D, int i)
    {
        var d = Derivative(a.ToRMPoly(i), D);
        return d.ToRUPoly(i);
    }

    //
    // public static Expr RischIntegrate(Expr f, Variable x)
    // {
    //     var D = new DiffField(f, x);
    //     Expr result = 0;
    //     for (int i = D.T.Length - 1; i >= 0; i--)
    //     {
    //         var @case = D.Cases[i];
    //         switch (@case)
    //         {
    //             default:
    //                 throw new NotImplementedException();
    //         }
    //     }
    //
    //     return result;
    // }

    public static Diff<RMPoly> DefaultDiff(int i, DiffField D)
    {
        return poly => Derivative(poly, D, i);
    }

    public delegate Rational<UnivariatePolynomial<K>>
        Diff<K>(UnivariatePolynomial<K> poly); // Derivative of K[t] on K(t)

    public static (UnivariatePolynomial<K>, UnivariatePolynomial<K>) SplitFactor<K>(UnivariatePolynomial<K> p,
        Diff<K> D)
    {
        var S = UnivariateGCD.PolynomialGCD(p, D(p).NumeratorExact()) / UnivariateGCD.PolynomialGCD(p, p.Derivative());
        if (S.Degree() == 0)
            return (p, p.CreateOne());

        var (qn, qs) = SplitFactor(p / S, D);
        return (qn, S * qs);
    }

    public static (UnivariatePolynomial<K>, UnivariatePolynomial<K>)
        ExtendedEuclidieanDiophantine<K>(UnivariatePolynomial<K> a, UnivariatePolynomial<K> b,
            UnivariatePolynomial<K> c)
    {
        var (g, s) = UnivariateGCD.PolynomialFirstBezoutCoefficient(a, b).ToTuple2();
        var q = UnivariateDivision.DivideExact(c, g);
        s = q * s;

        if (!s.IsZero() && s.Degree() >= b.Degree())
        {
            (q, s) = UnivariateDivision.DivideAndRemainder(s, b)!.ToTuple2();
        }

        return (s, (c - s * a) / b);
    }

    public static (UnivariatePolynomial<K>, Rational<UnivariatePolynomial<K>>, Rational<UnivariatePolynomial<K>>)
        CanonicalRepresentation<K>(Rational<UnivariatePolynomial<K>> f, Diff<K> D)
    {
        if (!f.IsIntegral())
            f = f.MonicDen();

        var (a, d) = (f.Numerator(), f.Denominator());
        var (q, r) = UnivariateDivision.DivideAndRemainder(a, d)!.ToTuple2();
        var (dn, ds) = SplitFactor(d, D);
        var (b, c) = ExtendedEuclidieanDiophantine(dn, ds, r);
        return (q, PolynomialFactory.RationalPoly(b, ds), PolynomialFactory.RationalPoly(c, dn));
    }

    public static UnivariatePolynomial<K>[] SquareFreeList<K>(UnivariatePolynomial<K> poly)
    {
        return SquareFree(poly).List.ToArray();
    }

    public static (K Constant, UnivariatePolynomial<K>[] List) SquareFree<K>(UnivariatePolynomial<K> poly)
    {
        var fac = UnivariateSquareFreeFactorization.SquareFreeFactorization(poly);
        Debug.Assert(fac.Unit.Degree() == 0);
        var cste = fac.Unit.Lc();
        if (fac.Count == 0)
            return (cste, []);

        var result = new UnivariatePolynomial<K>[fac.Exponents.Max() + 1];
        for (int i = 0; i < result.Length; i++)
            result[i] = poly.CreateOne();

        for (int i = 0; i < fac.Count; i++)
            result[fac.Exponents[i]] = fac.Factors[i];

        return (cste, result);
    }

    public static (Rational<UnivariatePolynomial<K>>, Rational<UnivariatePolynomial<K>>,
        Rational<UnivariatePolynomial<K>>)
        HermiteReduce<K>(Rational<UnivariatePolynomial<K>> f, Diff<K> D)
    {
        var (fp, fs, fn) = CanonicalRepresentation(f, D);
        var (a, d) = (fn.Numerator(), fn.Denominator());
        var ds = SquareFreeList(d);
        var g = Rational<UnivariatePolynomial<K>>.Zero(f.ring);
        for (int i = 1; i < ds.Length; i++)
        {
            if (ds[i].Degree() == 0)
                continue;

            var v = ds[i];
            var u = d / v.Pow(i);
            for (int j = i - 1; j >= 1; j--)
            {
                var (b, c) = ExtendedEuclidieanDiophantine(u * D(v).NumeratorExact(), v, -a / j);
                g += PolynomialFactory.RationalPoly(b, v.Pow(j));
                a = c * (-j) - u * D(b).NumeratorExact();
            }

            d = u * v;
        }

        var (q, r) = UnivariateDivision.DivideAndRemainder(a, d)!.ToTuple2();
        return (g, PolynomialFactory.RationalPoly(r, d), PolynomialFactory.RationalPoly(q + fp) + fs);
    }

    public static (UnivariatePolynomial<K>, UnivariatePolynomial<K>)
        PolynomialReduce<K>(UnivariatePolynomial<K> p, Diff<K> D)
    {
        var q = p.CreateZero();
        var ring = p.ring;
        var d = D(p.CreateMonomial(ring.GetOne(), 1)).NumeratorExact(); // Dt
        while (p.Degree() >= d.Degree()) /*deg(p) >= deg_t(Dt)*/
        {
            var m = p.Degree() - d.Degree() + 1; /*deg(p) - deg_t(Dt) + 1*/

            var c = ring.DivideExact(p.Lc(), ring.Multiply(ring.ValueOfLong(m), d.Lc())); /* lc(p) / (m * lc_t(Dt)) */
            var q0 = p.CreateMonomial(c, m);

            q += q0;
            p = p - D(q0).NumeratorExact();
        }

        return (q, p);
    }

    public class Residue<K>
    {
        public UnivariatePolynomial<K>[] s; // K[z]
        public UnivariatePolynomial<UnivariatePolynomial<K>>[] S; // K[z][t]
        public K[][] Alphas;
        public bool IsElementary;

        public Residue(UnivariatePolynomial<K>[] s, UnivariatePolynomial<UnivariatePolynomial<K>>[] S, bool elem)
        {
            this.s = s;
            this.S = S;
            IsElementary = elem;
            SetAlphas();
        }

        public Expr ToExpr(K cste) => throw new NotImplementedException();
        public Expr ToExpr(UnivariatePolynomial<K> poly /*K[t]*/) => throw new NotImplementedException();

        public Expr ToExpression()
        {
            // sum_i sum_|alpha| a log(S_i(a)(t))
            var result = Zero;
            var ringK = s[0].ring;
            for (int i = 0; i < S.Length; i++)
            {
                foreach (var alpha in Alphas[i])
                {
                    var eval = S[i].MapCoefficients(ringK, uni_z => uni_z.Evaluate(alpha));
                    result += ToExpr(alpha) * Ln(ToExpr(eval) );
                }
            }

            return result;
        }

        public Rational<UnivariatePolynomial<K>> Derivative(Diff<K> D)
        {
            // residue = sum_i sum_|alpha| a log(S_i(a)(t))
            // residue' = sum_i sum_|alpha| a D[S_i(a)(t)] / S_i(a)(t)
            var ringK = s[0].ring;
            var result = PolynomialFactory.RationalPoly(PolynomialFactory.Uni(ringK, ringK.GetZero()));
            for (int i = 0; i < S.Length; i++)
            {
                foreach (var alpha in Alphas[i])
                {
                    var eval = S[i].MapCoefficients(ringK, uni_z => uni_z.Evaluate(alpha));
                    var Deval = D(eval);
                    result += PolynomialFactory.Uni(ringK, alpha) * Deval / eval;
                }
            }

            return result;
        }

        public void SetAlphas()
        {
            Alphas = new K[s.Length][];
            for (int i = 0; i < Alphas.Length; i++)
            {
                if (s[i].Degree() == 0)
                {
                    Alphas[i] = [];
                    continue;
                }

                Alphas[i] = Solutions(s[i]);
            }
        }

        private static K[] Solutions(UnivariatePolynomial<K> poly, bool factor = true)
        {
            var ring = poly.ring;
            switch (poly.Degree())
            {
                case 0:
                    return [];
                case 1:
                    return [ring.DivideExact(ring.Negate(poly.Cc()), poly.Lc())];
                case 2: // TODO for Z64, Z, Q, Q64 and Uni, Mul cste
                    break;
            }

            if (!factor)
                throw new Exception();

            var sols = new List<K>();
            var fac = UnivariateFactorization.Factor(poly);
            for (int i = 0; i < fac.Count; i++)
            {
                var fSols = Solutions(fac.Factors[i], false);
                for (int j = 0; j < fac.Exponents[i]; j++)
                    sols.AddRange(fSols);
            }

            return sols.ToArray();
        }
    }


    public static (K, UnivariatePolynomial<K>[]) SubResultant<K>(UnivariatePolynomial<K> a,
        UnivariatePolynomial<K> b)
    {
        var res = UnivariateResultants.SubresultantPRS(a, b);
        return (res.Resultant(), res.remainders.ToArray());
    }

    public static (UnivariatePolynomial<K>[], UnivariatePolynomial<K>[]) SplitSquarefreeFactor<K>(
        UnivariatePolynomial<K> p, Diff<K> D) // p in K[z]
    {
        var k_z_ring = p.AsRing();

        UnivariatePolynomial<K> kD(UnivariatePolynomial<K> poly) // poly in K[z]
            => poly.MapCoefficients<K>(poly.ring,
                c => D(UnivariatePolynomial<K>.Constant(poly.ring, c)).NumeratorExact().Lc());

        var (pc, p_list) = SquareFree(p);
        if (!p.ring.IsOne(pc))
            p_list[1] *= p.CreateConstant(pc);
        if (p.IsZero())
            return ([p], []);

        var S = new UnivariatePolynomial<K>[p_list.Length]; // K[z]
        var N = new UnivariatePolynomial<K>[p_list.Length]; // K[z]
        for (int i = 0; i < p_list.Length; i++)
        {
            var pi = p_list[i];
            if (pi.IsOne())
            {
                S[i] = pi.CreateOne();
                N[i] = pi.CreateOne();
                continue;
            }

            S[i] = UnivariateGCD.PolynomialGCD(pi, kD(pi));
            N[i] = pi / S[i];
        }

        return (N, S);
    }

    public static Residue<K> ResidueReduce<K>(Rational<UnivariatePolynomial<K>> f, Diff<K> D)
    {
        var d = f.Denominator();
        var (p, a) = UnivariateDivision.DivideAndRemainder(f.Numerator(), d)!.ToTuple2();

        UnivariatePolynomial<K> r; // K[z]
        UnivariatePolynomial<UnivariatePolynomial<K>>[] R; // K[z][t]
        var ringK = d.ring;
        var Dd = D(d).NumeratorExact();

        // Convert poly from K[x] to K[z][t]
        var newD = d.MapCoefficients(Rings.UnivariateRing(ringK), c => UnivariatePolynomial<K>.Constant(ringK, c));
        var newA = a.MapCoefficients(Rings.UnivariateRing(ringK), c => UnivariatePolynomial<K>.Constant(ringK, c));
        var newDd = Dd.MapCoefficients(Rings.UnivariateRing(ringK),
            c => UnivariatePolynomial<K>.Constant(ringK, c));
        var newZ = PolynomialFactory.Uni(Rings.UnivariateRing(ringK),
            [PolynomialFactory.Uni(ringK, [ringK.GetZero(), ringK.GetOne()])]); // z in K[z][t]


        if (Dd.Degree() <= d.Degree())
            (r, R) = SubResultant(newD, newA - newZ * newDd);
        else
            (r, R) = SubResultant(newA - newZ * newDd, newD);

        var (n, s) = SplitSquarefreeFactor(r, D);
        var S = new UnivariatePolynomial<UnivariatePolynomial<K>>[s.Length]; // K[z][t]
        for (int i = 0; i < s.Length; i++)
        {
            if (s[i].Degree() == 0)
                continue;

            if (i == d.Degree())
                S[i] = PolynomialFactory.Uni(Rings.UnivariateRing(ringK), d);
            else
            {
                S[i] = R.First(el => el.Degree() == i);
                var A = SquareFree(S[i].Lc()).List;
                for (int j = 0; j < A.Length; j++)
                    S[i] = S[i] / UnivariateGCD.PolynomialGCD(A[j], s[i]).Pow(j);
            }
        }

        var b = n.All(ni => ni.IsConstant());
        return new Residue<K>(s, S, b);
    }

    
    public static (Expr Integral, bool IsElementary) IntegratePrimitive<K>(Rational<UnivariatePolynomial<K>> f, Diff<K> D)
    {
        var (g1, h, r) = HermiteReduce(f, D);
        var residue = ResidueReduce(h, D);
        if (!residue.IsElementary)
            throw new NotImplementedException();
            // return (g1 + g2, false); TODO ToExpr
    
        var (q, IsElem) = IntegratePrimitivePolynomial((h - residue.Derivative(D) + r).NumeratorExact(), D);
        throw new NotImplementedException();
        // return (g1 + g2 + q, IsElem); TODO ToExpr
    }
    
    public static (UnivariatePolynomial<K> Integral, bool IsElementary) IntegratePrimitivePolynomial<K>(UnivariatePolynomial<K> p, Diff<K> D)
    {
        if (p.Degree() == 0)
            return (p.CreateZero(), true);

        var t = p.CreateMonomial(p.ring.GetOne(), 1);
        var a = p.Lc();
        var bc = LimitedIntegrate(a, D(t), D);
    
        if (bc is null)
            return (p.CreateZero(), false);
        
        var (b, c) = bc.Value;
        var m = p.Degree();
        var q0 = p.CreateMonomial(p.ring.DivideExact(c, p.ring.ValueOfLong(m + 1)), m + 1) + p.CreateMonomial(b, m);
        var (q, IsElem) = IntegratePrimitivePolynomial(p - D(q0).NumeratorExact(), D);
        return (q + q0, IsElem);
    }

    // Extensions

    private static bool IsConstant(this RMPoly poly, int variable)
    {
        return poly.Numerator().Degree(variable) == 0 && poly.Denominator().Degree(variable) == 0;
    }

    private static RMPoly ToRMPoly(this MPoly poly)
    {
        return PolynomialFactory.RationalPoly(poly);
    }

    public static RUPoly ToRUPoly(this RMPoly poly, int i)
    {
        var num = poly.Numerator().AsUnivariate(i);
        var ring = Rings.Frac(num.ring);
        var newNum = num.MapCoefficients(ring, PolynomialFactory.RationalPoly);
        var newDen = poly.Denominator().AsUnivariate(i).MapCoefficients(ring, PolynomialFactory.RationalPoly);

        return PolynomialFactory.RationalPoly(newNum, newDen);
    }

    public static RMPoly ToRMPoly(this UPoly poly, int i)
    {
        var fac = poly[0].Numerator();
        var newfac = fac.Clone();
        var result = RMPoly.Zero(Rings.MultivariateRing(newfac));
        for (int j = 0; j < poly.data.Length; j++)
        {
            var coef = poly.data[j];
            if (poly.ring.IsZero(coef))
                continue;

            var newNum = coef.Numerator() * newfac.CreateMonomial(i, j);
            var newDen = coef.Denominator();
            result += PolynomialFactory.RationalPoly(newNum, newDen);
        }

        return result;
    }

    public static RMPoly ToRMPoly(this RUPoly poly, int i)
    {
        return poly.Numerator().ToRMPoly(i) / poly.Denominator().ToRMPoly(i);
    }

    private static Rational<UnivariatePolynomial<E>> MonicDen<E>(this Rational<UnivariatePolynomial<E>> rat)
    {
        var ring = rat.ring;
        var lc = rat.Denominator().Lc();
        return new Rational<UnivariatePolynomial<E>>(ring, rat.Numerator().DivideExact(lc),
            rat.Denominator().DivideExact(lc));
    }

    public static (E, E) ToTuple2<E>(this E[] data) =>
        data.Length == 2 ? (data[0], data[1]) : throw new ArgumentException();

    private static (E, E, E) ToTuple3<E>(this E[] data) =>
        data.Length == 3 ? (data[0], data[1], data[2]) : throw new ArgumentException();
}