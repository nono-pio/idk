// using System.Diagnostics;
// using Polynomials;
// using Polynomials.Poly.Univar;
//
// namespace ConsoleApp1.Core.Integrals;
//
// public static class RischDE
// {
//     public delegate UnivariatePolynomial<K> DiffPoly<K>(UnivariatePolynomial<K> poly);
//     
//     public static UnivariatePolynomial<K> WeakNormalizer<K>(Rational<UnivariatePolynomial<K>> f, DiffPoly<K> D)
//     {
//         var (dn, ds) = Risch.SplitFactor(f.Denominator(), D);
//         var g = UnivariateGCD.PolynomialGCD(dn, dn.Derivative());
//         var d_s = dn / g;
//         var d1 = d_s / UnivariateGCD.PolynomialGCD(d_s, g);
//         var (a, b) = Risch.ExtendedEuclidieanDiophantine(f.Denominator() / d1, d1, f.Numerator());
//
//         var Dd1 = D(d1);
//         // To K[z][t]
//         var ringK = a.ring;
//         var ringZOverK = Rings.UnivariateRing(ringK);
//         var newA = a.MapCoefficients(ringZOverK, cf => PolynomialFactory.Uni(ringK, cf));
//         var newD1 = a.MapCoefficients(ringZOverK, cf => PolynomialFactory.Uni(ringK, cf));
//         var newDd1 = a.MapCoefficients(ringZOverK, cf => PolynomialFactory.Uni(ringK, cf));
//         var newZ = PolynomialFactory.Uni(ringZOverK, PolynomialFactory.Uni(ringK, [ringK.GetZero(), ringK.GetOne()]));
//         
//         var r = UnivariateResultants.Resultant(newA - newZ * newDd1, newD1); // K[z]
//         int[] N = PositiveIntegerRoots(r);
//         var result = a.CreateOne();
//         foreach (var n in N)
//         {
//             result *= UnivariateGCD.PolynomialGCD(a - n * D(d1), d1).Pow(n);
//         }
//
//         return result;
//     }
//
//     public static int OrderAt<K>(UnivariatePolynomial<K> a, UnivariatePolynomial<K> p)
//     {
//         if (a.IsZero())
//             return int.MaxValue;
//         
//         var order = 0;
//         while ((a = UnivariateDivision.DivideOrNull(a, p)) is not null)
//         {
//             order++;
//         }
//
//         return order;
//     }
//
//     public static int OrderAt<K>(Rational<UnivariatePolynomial<K>> a, UnivariatePolynomial<K> p)
//     {
//         if (a.IsZero())
//             return int.MaxValue;
//         return OrderAt(a.Numerator(), p) - OrderAt(a.Denominator(), p);
//     }
//     
//     public static (UnivariatePolynomial<K> a, Rational<UnivariatePolynomial<K>> b, Rational<UnivariatePolynomial<K>> c, UnivariatePolynomial<K> h)? 
//         RdeNormalDenominator<K>(Rational<UnivariatePolynomial<K>> f, Rational<UnivariatePolynomial<K>> g, DiffPoly<K> D)
//     {
//         var (dn, ds) = Risch.SplitFactor(f.Denominator(), D);
//         var (en, es) = Risch.SplitFactor(g.Denominator(), D);
//         var p = UnivariateGCD.PolynomialGCD(dn, en);
//         var h = UnivariateGCD.PolynomialGCD(en, en.Derivative()) / UnivariateGCD.PolynomialGCD(p, p.Derivative());
//         var d_hsquare = dn * h.Clone().Square();
//         if (UnivariateDivision.DivideOrNull(d_hsquare, en) is null)
//             return null;
//         
//         return (dn * h, dn * h * f - dn * D(h), d_hsquare * g, h);
//     }
//
//
//     public static (UnivariatePolynomial<K> a, UnivariatePolynomial<K> b, UnivariatePolynomial<K> c, UnivariatePolynomial<K> h) RdeSpecialDenomExp<K>(UnivariatePolynomial<K> a, Rational<UnivariatePolynomial<K>> b,
//         Rational<UnivariatePolynomial<K>> c, DiffPoly<K> D)
//     {
//         var t = a.CreateMonomial(a.ring.GetOne(), 1);
//         var p = t;
//         var nb = OrderAt(b, p);
//         var nc = OrderAt(c, p);
//         var n = Math.Min(0, nc - Math.Min(0, nb));
//         if (nb == 0)
//         {
//             // var ring = a.ring;
//             // var alpha = ring.Negate(ring.DivideExact(b.Numerator().Evaluate(0), ring.Multiply(b.Denominator().Evaluate(0), a.Evaluate(0))));
//             // var eta = D(t) / t;
//             // var log = ParamDERisch.ParametricLogarithmicDerivative(alpha, eta, D);
//             // if (log is not null)
//             // {
//             //     n = Math.Min(log.Value.m, n);
//             // }
//             throw new NotImplementedException();
//         }
//
//         var N = Math.Max(0, Math.Max(-nb, n - nc));
//         return (a * p.Pow(N), ((b + n * a * D(p) / p) * p.Pow(N)).NumeratorExact(), (c * p.Pow(N - n)).NumeratorExact(), p.Pow(-n));
//     }
//
//
//     public static (UnivariatePolynomial<K> a, UnivariatePolynomial<K> b, UnivariatePolynomial<K> c,
//         UnivariatePolynomial<K> h) RdeSpecielDenomTan<K>(UnivariatePolynomial<K> a, Rational<UnivariatePolynomial<K>> b,
//             Rational<UnivariatePolynomial<K>> c, DiffPoly<K> D)
//     {
//         var ring = a.ring;
//         var t = a.CreateMonomial(ring.GetOne(), 1);
//         var p = a.CreateFromArray([ring.GetOne(), ring.GetZero(), ring.GetOne()]); // t^2+1
//         var nb = OrderAt(b, p);
//         var nc = OrderAt(c, p);
//         var n = Math.Min(0, nc - Math.Min(0, nb));
//         if (nb == 0)
//         {
//             // var alpha;
//             // var beta;
//             // var eta = D(t) / p;
//             throw new NotImplementedException();
//         }
//
//         var N = Math.Max(0, Math.Max(-nb, n - nc));
//         return (a * p.Pow(N), ((b + n * a * D(p) / p) * p.Pow(N)).NumeratorExact(), (c * p.Pow(N - n)).NumeratorExact(), p.Pow(-n));
//     }
//
//     public static int RdeBoundDegreePrim<K>(UnivariatePolynomial<K> a, UnivariatePolynomial<K> b,
//         UnivariatePolynomial<K> c, DiffPoly<K> D)
//     {
//         var da = a.Degree();
//         var db = b.Degree();
//         var dc = c.Degree();
//         var n = db > da ? Math.Max(0, dc - db) : Math.Max(0, dc - da + 1);
//         var ring = a.ring;
//         if (db == da - 1)
//         {
//             var alpha = ring.Negate(ring.DivideExact(b.Lc(), a.Lc()));
//             var integ = LimitedIntegrate(alpha, [eta], D);
//             if (integ is not null)
//                 n = Math.Max(n, integ.Value.m[0]);
//         }
//
//         if (db == da)
//         {
//             var alpha = ring.Negate(ring.DivideExact(b.Lc(), a.Lc()));
//             if ()
//             {
//                 var beta;
//                 if ()
//                     n = Math.Max(n, m);
//             }
//         }
//
//         return n;
//     }
//
//     public static int RdeBoundDegreeBase<K>(UnivariatePolynomial<K> a, UnivariatePolynomial<K> b,
//         UnivariatePolynomial<K> c)
//     {
//         var da = a.Degree();
//         var db = b.Degree();
//         var dc = c.Degree();
//         var n = Math.Max(0, dc - Math.Max(db, da - 1));
//         if (db == da - 1)
//         {
//             var ring = a.ring;
//             var m = ring.Negate(ring.DivideExact(b.Lc(), a.Lc()));
//             if ()
//                 n = Math.Max(0, m, dc - db);
//         }
//
//         return n;
//     }
//
//     public static int RdeBoundDegreeExp<K>(UnivariatePolynomial<K> a, UnivariatePolynomial<K> b,
//         UnivariatePolynomial<K> c, DiffPoly<K> D)
//     {
//         var da = a.Degree();
//         var db = b.Degree();
//         var dc = c.Degree();
//         var n = Math.Max(0, dc - Math.Max(db, da));
//         if (da == db)
//         {
//             var ring = a.ring;
//             var alpha = ring.Negate(ring.DivideExact(b.Lc(), a.Lc()));
//             if ()
//                 n = Math.Max(n, m);
//         }
//
//         return n;
//     }
//
//     public static int RdeBoundDegreeNonLinear<K>(UnivariatePolynomial<K> a, UnivariatePolynomial<K> b,
//         UnivariatePolynomial<K> c, DiffPoly<K> D)
//     {
//         var da = a.Degree();
//         var db = b.Degree();
//         var dc = c.Degree();
//         var delta = D(t).Degree();
//         var lambda = D(t).Lc();
//         var n = Math.Max(0, dc - Math.Max(da + delta - 1, db));
//         if (db == da + delta - 1)
//         {
//             var ring = a.ring;
//             var m = ring.Negate(ring.DivideExact(b.Lc(), ring.Multiply(lambda, a.Lc())));
//             if ()
//                 n = Math.Max(0, m, dc - db);
//         }
//
//         return n;
//     }
//
//     public static (UnivariatePolynomial<K> b, UnivariatePolynomial<K> c, int m, UnivariatePolynomial<K> alpha, UnivariatePolynomial<K> beta)? SPDE<K>(UnivariatePolynomial<K> a, UnivariatePolynomial<K> b, UnivariatePolynomial<K> c,
//         DiffPoly<K> D, int n)
//     {
//         if (n < 0)
//             return c.IsZero() ? (a.CreateZero(), a.CreateZero(), 0, a.CreateZero(), a.CreateZero()) : null;
//         var g = UnivariateGCD.PolynomialGCD(a, b);
//         if (UnivariateDivision.DivideOrNull(c, g) is null)
//             return null;
//         a = a / g;
//         b = b / g;
//         c = c / g;
//         if (a.Degree() == 0)
//             return (b / a, c / a, n, a.CreateOne(), a.CreateZero());
//         var (r, z) = Risch.ExtendedEuclidieanDiophantine(b, a, c);
//         var u = SPDE(a, b + D(a), z - D(r), D, n - a.Degree());
//         if (u is null)
//             return null;
//
//         var sol = u.Value;
//         return (sol.b, sol.c, sol.m, a * sol.alpha, a * sol.beta + r);
//     }
//
//     public static UnivariatePolynomial<K>? PolyRischDENoCancel1<K>(UnivariatePolynomial<K> b, UnivariatePolynomial<K> c,
//         DiffPoly<K> D, int n)
//     {
//         var q = b.CreateZero();
//         while (!c.IsZero())
//         {
//             var m = c.Degree() - b.Degree();
//             if (n < 0 || m < 0 || m > n)
//                 return null;
//
//             var p = b.CreateMonomial(b.ring.DivideExact(c.Lc(), b.Lc()), m);
//             q += p;
//             n = m - 1;
//             c = c - D(p) - b * p;
//         }
//
//         return q;
//     }
//
//     public static UnivariatePolynomial<K>? PolyRischDENoCancel2<K>(UnivariatePolynomial<K> b, UnivariatePolynomial<K> c,
//         DiffPoly<K> D, int n)
//     {
//         var q = b.CreateZero();
//         while (!c.IsZero())
//         {
//             var m = n == 0 ? 0 : c.Degree() - delta(t) + 1;
//             if (n < 0 || m < 0 || m > n)
//                 return null;
//
//             UnivariatePolynomial<K> p;
//             if (m > 0)
//                 p = b.CreateMonomial(b.ring.DivideExact(c.Lc(), m * lambda(t)), m);
//             else
//             {
//                 if (b.Degree() != c.Degree())
//                     return null;
//                 if (b.Degree() == 0)
//                     return (q, b, c)
//                 p = b.CreateConstant(b.ring.DivideExact(c.Lc(), b.Lc()));
//             }
//                 
//             q += p;
//             n = m - 1;
//             c = c - D(p) - b * p;
//         }
//
//         return q;
//     }
//     
//     public static UnivariatePolynomial<K>? PolyRischDENoCancel3<K>(UnivariatePolynomial<K> b, UnivariatePolynomial<K> c,
//         DiffPoly<K> D, int n)
//     {
//         var q = b.CreateZero();
//         var ring = q.ring;
//         var M = () ? ring.Negate(ring.DivideExact(b.Lc(), lambda(t))) : -1;
//         
//         while (!c.IsZero())
//         {
//             var m = Math.Max(M, c.Degree() - delta(t) + 1);
//             if (n < 0 || m < 0 || m > n)
//                 return null;
//
//             var u = m * lambda(t) + b.Lc();
//             if (u == 0)
//                 return (q, m, c);
//             UnivariatePolynomial<K> p;
//             if (m > 0)
//                 p = b.CreateMonomial(b.ring.DivideExact(c.Lc(), u), m);
//             else
//             {
//                 if (c.Degree() != delta(t) - 1)
//                     return null;
//                 p = b.CreateConstant(b.ring.DivideExact(c.Lc(), b.Lc()));
//             }
//                 
//             q += p;
//             n = m - 1;
//             c = c - D(p) - b * p;
//         }
//
//         return q;
//     }
//
//     public static UnivariatePolynomial<K>? PolyRischDECancelPrim<K>(K b, UnivariatePolynomial<K> c,
//         DiffPoly<K> D, int n)
//     {
//         if ()
//         {
//             if ()
//                 return p / z;
//             return null;
//         }
//
//         if (c.IsZero())
//             return 0;
//         if (n < c.Degree())
//             return null;
//         var q = c.CreateZero();
//         var ring = c.ring;
//         while (!c.IsZero())
//         {
//             var m = c.Degree();
//             if (n < m)
//                 return null;
//             var s = RischDE(b, c.Lc());
//             if (s is null)
//                 return null;
//
//             q = q.AddMonomial(s, m);
//             n = m - 1;
//             c = c - c.CreateMonomial(ring.Multiply(b, s), m) - D(c.CreateMonomial(s, m));
//         }
//
//         return q;
//     }
//     
//     public static UnivariatePolynomial<K>? PolyRischDECancelExp<K>(K b, UnivariatePolynomial<K> c,
//         DiffPoly<K> D, int n)
//     {
//         if ()
//         {
//             if ()
//                 return q;
//             return null;
//         }
//
//         if (c.IsZero())
//             return 0;
//         if (n < c.Degree())
//             return null;
//         var q = c.CreateZero();
//         var ring = c.ring;
//         while (!c.IsZero())
//         {
//             var m = c.Degree();
//             if (n < m)
//                 return null;
//             var s = RischDE(b + m * D(t)/t, c.Lc());
//             if (s is null)
//                 return null;
//
//             q = q.AddMonomial(s, m);
//             n = m - 1;
//             c = c - c.CreateMonomial(ring.Multiply(b, s), m) - D(c.CreateMonomial(s, m));
//         }
//
//         return q;
//     }
//
//
//     public static UnivariatePolynomial<K>? PolyRischDECancelTan<K>(K b0, UnivariatePolynomial<K> c,
//         DiffPoly<K> D, int n)
//     {
//         var ring = c.ring;
//         if (n == 0)
//         {
//             if (c.IsConstant())
//             {
//                 if (!ring.IsZero(b0))
//                     return RischDE(b0, c);
//                 else if ()
//                     return q;
//                 return null;
//             }
//
//             return null;
//         }
//
//         var p = t ^ 2 + 1;
//         var eta = D(t) / p;
//         var (c_, rem) = UnivariateDivision.DivideAndRemainder(c, p).ToTuple2();
//         var c0 = rem.Cc();
//         var c1 = rem.Lc();
//         var uv = CoupledDESystem(b0, -n * eta, c0, c1);
//         if (uv is null)
//             return null;
//         var (u, v) = uv.Value;
//         if (n == 1)
//             return u * t + v;
//         var r = u + v * t;
//         c = (c - D(r) - (b0 - n * eta) * r) / p;
//         var h = PolyRischDECancelTan(b0, c, D, n - 2);
//         return h is null ? null : p * h + r;
//     }
// }