using System.Runtime;
using System.Xml;
using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Expressions.Base;
using ConsoleApp1.Core.Expressions.Trigonometrie;
using ConsoleApp1.Core.Models;
using PolynomialTheory;
using MPoly = PolynomialTheory.MultiPolynomial<PolynomialTheory.Rational>;
using RMPoly = PolynomialTheory.RationalMultiPolynomial<PolynomialTheory.Rational>;

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

    public static Poly Euclidean(Expr a, Expr b, Variable t)
    {
        var ap = Poly.ToPoly(a, t);
        var bp = Poly.ToPoly(b, t);

        return Poly.Gcd(ap, bp);
    }

    public static (Expr, Expr) SplitFactor(Expr p, DiffField D, int i)
    {
        var pp = Poly.ToPoly(p, D.t[i]);
        var S = Euclidean(p, Derivative(p, D), D.t[i]) / Euclidean(p, p.Derivee(D.t[i]), D.t[i]);
        if (S.Deg() == 0)
            return (p, 1);
    
        var (qn, qs) = SplitFactor(Poly.Div(pp, S).Item1.Of(D.t[i]), D, i);
        return (qn, (S * qs).Of(D.t[i]));
    }

    // public static (Expr, Expr, Expr) CanonicalRepresentation(Expr f, DiffField D, int i)
    // {
    //     var (a, d) = f.AsFraction();
    //     var (q, r) = Poly.Div(a, d);
    //     var (dn, ds) = SplitFactor(d, D, i);
    //     var (b, c) = ExtendedEuclidean(dn, ds, r);
    //     return (q, b / ds, c / dn);
    // }

    // public static (Poly, Poly, Poly) HermiteReduce(Poly f, DiffField D)
    // {
    //     var (fp, fs, fn) = CanonicalRepresentation(f, D);
    //     var (a, d) = fn.AsNumDen();
    //     var d = SquareFree(d);
    //     var g = 0;
    //     for (int i = 1; i < d.Length; i++)
    //     {
    //         if (d[i].Deg() == 0)
    //             continue;
    //
    //         var v = d[i];
    //         var u = d / Pow(v, i);
    //         for (int j = i - 1; j >= 1; j--) // TODO check range
    //         {
    //             var (b, c) = ExtendedEuclidean(u * Derivative(v), v, -a / j);
    //             g += b / Pow(v, j);
    //             a = -j * c - u * Derivative(b);
    //         }
    //
    //         d = u * v;
    //     }
    //     
    //     var (q, r) = Poly.Div(a, d);
    //     return (g, r / d, q + fp + fs);
    // }
    
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