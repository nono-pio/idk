using System.Runtime;
using System.Xml;
using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Expressions.Base;
using ConsoleApp1.Core.Expressions.Trigonometrie;
using ConsoleApp1.Core.Models;
using PolynomialTheory;

using MPoly = PolynomialTheory.MultiPolynomial<PolynomialTheory.Rational>; // QQ[x, t0, ...]
using RMPoly = PolynomialTheory.RationalMultiPolynomial<PolynomialTheory.Rational>; // QQ(x, t0, ...)
using UPoly = PolynomialTheory.UniPolynomial<PolynomialTheory.RationalMultiPolynomial<PolynomialTheory.Rational>>; // QQ(x, t0, ...)[ti]
using RUPoly = PolynomialTheory.RationalUniPolynomial<PolynomialTheory.RationalMultiPolynomial<PolynomialTheory.Rational>>; // QQ(x, t0, ...)(ti)


namespace ConsoleApp1.Core.Integrals;

public class Risch
{
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
        public static RationalRing QQ = new RationalRing();
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
            RischCases GetCase(Variable t, Expr d)
            {
                if (d.Constant(t))
                {
                    if (d.IsNumOne)
                        return RischCases.Base;
                    return RischCases.Primitive;
                }
                
                if ((d / t).Constant(t)) // TODO : d.Rem(t).IsZero
                    return RischCases.Exp;
                
                if ((d / (Pow(t, 2) + 1)).Constant(t)) // TODO : d.Rem(1 + Pow(t, 2)).IsZero
                    return RischCases.Tan;
                
                // if (d.Deg(t) > 1)
                //     return RischCases.OtherNonlinear;
                return RischCases.OtherLinear;
            }
            
            this.Cases = new RischCases[this.T.Length];
            for (int i = 0; i < Cases.Length; i++)
            {
                Cases[i] = GetCase(t[i], D[i]);
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
            return new RMPoly(ExprToMPoly(n), ExprToMPoly(d));
        }
        
        public MPoly ExprToMPoly(Expr expr)
        {
            var n = t.Length;

            switch (expr)
            {
                case Number num:
                    if (!num.Num.IsFraction)
                        goto default;

                    return PolynomialHelper.MultiPolynomial(QQ,
                        new Rational((int)num.Num.Numerator, (int)num.Num.Denominator), n);
                case Variable var:
                    
                    var ti = Array.IndexOf(t, var);
                    if (ti == -1)
                        goto default;

                    var degs = new int[n];
                    degs[ti] = 1;
                    
                    return PolynomialHelper.MultiPolynomial(QQ, (1, degs));
                case Addition add:
                    return add.Args.Select(ExprToMPoly).Aggregate((a, b) => a + b);
                case Multiplication mul:
                    return mul.Args.Select(ExprToMPoly).Aggregate((a, b) => a * b);
                case Power pow:
                    var exp = pow.Exp is Number number && number.Num.IsInt ? (int)number.Num.Numerator : throw new NotImplementedException();
                    
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
        return (Derivative(f.Numerator, D) * f.Denominator.ToRationalPoly() - Derivative(f.Denominator, D) * f.Numerator.ToRationalPoly()) / (f.Denominator * f.Denominator).ToRationalPoly();
    }

    public static RMPoly Derivative(MPoly f, DiffField D)
    {
        RMPoly Derivative(Multinomial<Rational> multinomial)
        {
            var result = RMPoly.Zero(DiffField.QQ, D.t.Length);
            for (int i = 0; i < multinomial.Degs.Length; i++)
            {
                if (multinomial.Degs[i] == 0)
                    continue;
                
                var degs = new int[multinomial.Degs.Length];
                Array.Copy(multinomial.Degs, degs, degs.Length);
                degs[i]--;
                
                var newMul = (multinomial.Degs[i] * multinomial.Coef, degs);
                var newPoly = PolynomialHelper.MultiPolynomial(DiffField.QQ, newMul).ToRationalPoly() * D.Dtemp[i];
                
                result += newPoly;
            }

            return result;
        }
        
        return f.Multinomials.Select(Derivative).Aggregate((a, b) => a + b);
    }
    
    private static UPoly Derivative(UPoly a, DiffField D, int i)
    {
        var d = Derivative(RMPoly.CombineWithMultiPolynomial(a, i), D).ToUniPolynomialOfRational(i);
        return d.Numerator;
    }

    
    public static Expr RischIntegrate(Expr f, Variable x)
    {
        var D = new DiffField(f, x);
        Expr result = 0;
        for (int i = D.T.Length - 1; i >= 0; i--)
        {
            var @case = D.Cases[i];
            switch (@case)
            {
                default:
                    throw new NotImplementedException();
            }
        }

        return result;
    }

    public static (UPoly, UPoly) SplitFactor(UPoly p, DiffField D, int i)
    {
        var S = PolynomialHelper.Gcd(p, Derivative(p, D, i)) / PolynomialHelper.Gcd(p, p.Derivative());
        if (S.Degree == 0)
            return (p, UPoly.One(p.Ring));
    
        var (qn, qs) = SplitFactor(p / S, D, i);
        return (qn, S * qs);
    }

    public static (UPoly, UPoly) ExtendedEuclidieanDiophantine(UPoly a, UPoly b, UPoly c)
    {
        var (s, t, g) = UPoly.ExtendedEuclidean(a.Ring, a, b);
        var (q, r) = UPoly.Divide(a.Ring, c, g);
        if (!r.IsZero())
            throw new ArgumentException("c is not in the ideal generated by a and b");
        s = q * s;
        t = q * t;
        if (!s.IsZero() && s.Degree >= b.Degree)
        {
            (q, r) = UPoly.Divide(s.Ring, s, b);
            s = r;
            t = t + q * a;
        }

        return (s, t);
    }

    public static (UPoly, RUPoly, RUPoly) CanonicalRepresentation(RUPoly f, DiffField D, int i)
    {
        if (!f.IsDenMonic())
            f = f.MonicDen();
        
        var (a, d) = (f.Numerator, f.Denominator);
        var (q, r) = UPoly.Divide(a.Ring, a, d);
        var (dn, ds) = SplitFactor(d, D, i);
        var (b, c) = ExtendedEuclidieanDiophantine(dn, ds, r);
        return (q, new RUPoly(b, ds), new RUPoly(c, dn));
    }

    public static UPoly[] SquareFree(UPoly A)
    {
        // TODO content
        var c = A.Ring.One;
        var S = A / c;
        var Sp = S.Derivative();
        var Sm = UPoly.GCD(S.Ring, S, Sp);
        var Ss = S / Sm;
        var Y = Sp / Sm;
        var k = 1;
        var Z = Y - Ss.Derivative();
        var As = new List<UPoly>();
        As.Add(UPoly.Zero(A.Ring));
        while (!Z.IsZero())
        {
            var newA = UPoly.GCD(Ss.Ring, Ss, Z);
            Ss = Ss / newA;
            Y = Z / newA;
            Z = Y - Ss.Derivative();
            k++;
            As.Add(newA);
        }
        As.Add(Ss);

        return As.ToArray();
    }

    public static (RUPoly, RUPoly, RUPoly) HermiteReduce(RUPoly f, DiffField D, int index)
    {
        var (fp, fs, fn) = CanonicalRepresentation(f, D, index);
        var (a, d) = (fn.Numerator, fn.Denominator);
        var ds = SquareFree(d);
        var g = RUPoly.Zero(f.Ring);
        for (int i = 1; i < ds.Length; i++)
        {
            if (ds[i].Degree == 0)
                continue;
    
            var v = ds[i];
            var u = d / v.Pow(i);
            for (int j = i - 1; j >= 1; j--) // TODO check range
            {
                var (b, c) = ExtendedEuclidieanDiophantine(u * Derivative(v, D, index), v, -a / j);
                g += new RUPoly(b, v.Pow(j));
                a = c * (-j) - u * Derivative(b, D, index);
            }
    
            d = u * v;
        }
        
        var (q, r) = UPoly.Divide(a.Ring, a, d);
        return (g, new RUPoly(r, d), new RUPoly(q + fp) + fs);
    }

    public static (UPoly, UPoly) PolynomialReduce(UPoly p, DiffField D, int i)
    {
        var q = UPoly.Zero(p.Ring);
        var ring = (RationalMultiPolynomialRing<Rational>)p.Ring;
        var d = D.Dtemp[i].Numerator;
        while (p.Degree >= d.Deg(i))
        {
            var m = p.Degree - d.Deg(i) + 1;

            var coefficients = new RMPoly[m + 1];
            Array.Fill(coefficients, RMPoly.Zero(ring.Ring, ring.NVars - 1));
            coefficients[m] = p.LC / (d.LC(i) * m);

            var q0 = new UPoly(ring, coefficients);
            
            q += q0;
            p = p - Derivative(q0, D, i);

        }

        return (q, p);
    }
    
    //
    // public static (Poly Integral, bool IsElementary) ResidueReduce(PolyRational f, DiffField D)
    // {
    //     var d = f.Den;
    //     var (p, a) = Poly.Div(f.Num, d);
    //     var z = new Variable("z", dummy: true);
    //     Poly r;
    //     Poly[] R;
    //     if (Derivative(d, D).Deg() <= d.Deg())
    //         (r, R) = SubResultant_x(d, a - z * Derivative(d, D));
    //     else 
    //         (r, R) = SubResultant_x(d, a - z * Derivative(d, D));
    //
    //     var (n, s) = SplitSquarefreeFactor(r, kD);
    //     for (int i = 0; i < ss.Length; i++)
    //     {
    //         if (i == d.Deg())
    //             S[i] = d;
    //         else
    //         {
    //             S[i] = R.First(el => el.Deg() == i);
    //             var A = SquareFree(lc_t(S[i]));
    //             for (int j = 0; j < A.Length; j++)
    //                 S[i] = S[i] / gcd_x(A[j], s[i]) ^ j;
    //         }
    //     }
    // }
    //
    //
    // public static (Poly Integral, bool IsElementary) IntegratePrimitive(Poly f, DiffField D)
    // {
    //     var (g1, h, r) = HermiteReduce(f, D);
    //     var (g2, IsElem) = ResidueReduce(h, D);
    //     if (!IsElem)
    //         return (g1 + g2, false);
    //
    //     (var q, IsElem) = IntegratePrimitivePolynomial(h - Derivative(g2, D) + r, D);
    //     return (g1 + g2 + q, IsElem);
    // }
    //
    // public static (Poly Integral, bool IsElementary) IntegratePrimitivePolynomial(Poly p, DiffField D)
    // {
    //     if (p.Deg() == 0)
    //         return (0, true);
    //
    //     var a = p.LC();
    //     var bc = LimitedIntegrate(a, Dt, D);
    //
    //     if (bc is null)
    //         return (0, false);
    //     
    //     var (b, c) = bc.Value;
    //     var m = p.Deg();
    //     var q0 = c * t ^ (m + 1) / (m + 1) + b * t ^ m;
    //     var (q, IsElem) = IntegratePrimitivePolynomial(p - Derivative(q0, D), D);
    //     return (q + q0, IsElem);
    // }
}