// // using System.Diagnostics;
// // using ConsoleApp1.Core.Exprs.Atoms;
// // using ConsoleApp1.Core.Exprs.Others;
// //
// // namespace ConsoleApp1.Core.Limits;
// //
// // public enum Direction
// // {
// //     Bigger,
// //     Smaller
// // }
// //
// // public class Gruntz
// // {
// //     public static Expr Limit(Expr expr, Variable x, Expr x0, Direction dir)
// //     {
// //         if (x0.IsInfinity)
// //             return LimitInf(expr, x);
// //         if (x0.IsNegativeInfinity)
// //             return LimitInf(expr.Substitue(x, -x), x);
// //
// //         if (dir == Direction.Bigger)
// //             return LimitInf(expr.Substitue(x, x0 + 1 / x), x);
// //         if (dir == Direction.Smaller)
// //             return LimitInf(expr.Substitue(x, x0 - 1 / x), x);
// //         
// //         throw new UnreachableException();
// //     }
// //     
// //     private static Expr LimitInf(Expr expr, Variable x)
// //     {
// //         if (expr.Constant(x))
// //             return expr;
// //         
// //         var (c0, e0) = MRVLeadingTerm(expr, x);
// //         var sign = Sign(e0, x);
// //         if (sign == 1)
// //             return 0;
// //         else if (sign == -1)
// //         {
// //             var s = Sign(c0, x);
// //             return s * Expr.Inf;
// //         }
// //         else // sign == 0
// //         {
// //             if (c0 == expr)
// //                 throw new Exception();
// //             
// //             return LimitInf(c0, x);
// //         }
// //     }
// //
// //     private static int Sign(Expr expr, Variable x)
// //     {
// //         if (expr.IsPositive)
// //             return 1;
// //         if (expr.IsNegative)
// //             return -1;
// //         if (expr.IsZero)
// //             return 0;
// //
// //         if (expr.Constant(x))
// //         {
// //             var sig = SignExpr.Eval(expr);
// //             if (sig.IsNumberInt())
// //             {
// //                 return sig.ToInt();
// //             }
// //         } 
// //         
// //         var (c0, _) = MRVLeadingTerm(expr, x);
// //         return Sign(c0, x);
// //     }
// //
// //     private static (Expr, Expr) MRVLeadingTerm(Expr expr, Variable x)
// //     {
// //         if (expr.Constant(x))
// //             return (expr, 0);
// //
// //         var (Omega, exps) = mrv(expr, x);
// //         if (Omega is null)
// //             return (exps, 0);
// //         
// //         if (x in Omega)
// //         {
// //             var Omega_up = moveup2(Omega, x);
// //             var exps_up = moveup([exps], x)[0];
// //             Omega = Omega_up;
// //             exps = exps_up;
// //         }
// //
// //         var w = Dummy("w", positive = True);
// //         var (f, logw) = rewrite(exps, Omega, x, w);
// //         Expr lt;
// //         try
// //         {
// //             lt = f.leadterm(w, logx = logw);
// //         }
// //         catch
// //         {
// //             var n0 = 1;
// //             var _series = Order(1);
// //             var incr = 1;
// //             while (_series.is_Order)
// //             {
// //                 _series = f._eval_nseries(w, n = n0 + incr, logx = logw);
// //                 incr *= 2;
// //             }
// //
// //             series = _series.expand().removeO();
// //             try
// //             {
// //                 lt = series.leadterm(w, logx = logw);
// //             }
// //             catch
// //             {
// //                 lt = f.as_coeff_exponent(w);
// //                 if (lt[0].has(w))
// //                 {
// //                     base = f.as_base_exp()[0].as_coeff_exponent(w);
// //                     ex = f.as_base_exp()[1];
// //                     lt = (base[0] ** ex, base[1] * ex);
// //                 }
// //             }
// //         }
// //             
// //         return (lt[0].subs(log(w), logw), lt[1])
// //     }
// // }
//
// using ConsoleApp1.Core.Expressions.Atoms;
// using ConsoleApp1.Core.Expressions.Base;
// using ConsoleApp1.Core.Expressions.Others;
//
// public class SubsSet : Dictionary<Expr, Variable>
// {
//     public Dictionary<Variable, Expr> Rewrites { get; set; }
//
//     public SubsSet()
//     {
//         Rewrites = new Dictionary<Variable, Expr>();
//     }
//
//     public override string ToString()
//     {
//         return base.ToString() + ", " + Rewrites.ToString();
//     }
//
//     public new Variable this[Expr key]
//     {
//         get
//         {
//             if (!ContainsKey(key))
//             {
//                 this[key] = Variable.CreateUnique();
//             }
//             return base[key];
//         }
//         set
//         {
//             base[key] = value;
//         }
//     }
//
//     public Expr DoSubs(Expr e)
//     {
//         foreach (var kvp in this)
//         {
//             e = e.Substitue(kvp.Value, kvp.Key);
//         }
//         return e;
//     }
//
//     public bool Meets(SubsSet s2)
//     {
//         return this.Keys.Intersect(s2.Keys).Any();
//     }
//
//     public (SubsSet, Expr) Union(SubsSet s2, Expr? exps = null)
//     {
//         var res = new SubsSet();
//         var tr = new Dictionary<Variable, Expr>();
//
//         foreach (var kvp in s2)
//         {
//             if (this.ContainsKey(kvp.Key))
//             {
//                 if (exps != null)
//                 {
//                     exps = exps.Substitue(kvp.Value, res[kvp.Key]);
//                 }
//                 tr[kvp.Value] = res[kvp.Key];
//             }
//             else
//             {
//                 res[kvp.Key] = kvp.Value;
//             }
//         }
//
//         foreach (var kvp in s2.Rewrites)
//         {
//             res.Rewrites[kvp.Key] = kvp.Value.Substitue(tr);
//         }
//
//         return (res, exps);
//     }
//
//     public new SubsSet Copy()
//     {
//         var r = new SubsSet();
//         r.Rewrites = new Dictionary<Variable, Expr>(Rewrites);
//         foreach (var kvp in this)
//         {
//             r[kvp.Key] = kvp.Value;
//         }
//         return r;
//     }
// }
//
// public static class GruntzAlgorithm
// {
//     public static string Compare(Expr a, Expr b, Variable x)
//     {
//         var la = Log(a);
//         var lb = Log(b);
//
//         if (a is Power powA && powA.Base == Constant.E)
//         {
//             la = powA.Exp;
//         }
//         if (b is Power powB && powB.Base == Constant.E)
//         {
//             lb = powB.Exp;
//         }
//
//         var c = LimitInf(la / lb, x);
//         if (c == 0)
//         {
//             return "<";
//         }
//         else if (c.IsInfinite)
//         {
//             return ">";
//         }
//         else
//         {
//             return "=";
//         }
//     }
//
//     public static (SubsSet, Expr) MRV(Expr e, Variable x)
//     {
//         //e = PowSimp(e, deep: true, combine: "exp");
//         
//         if (e.Constant(x))
//             return (new SubsSet(), e);
//         else if (e == x)
//         {
//             var s = new SubsSet();
//             return (s, s[x]);
//         }
//         else if (e is Multiplication || e is Addition)
//         {
//             var (i, d) = e.AsIndependent(x);
//             if (d.Function != e.Function)
//             {
//                 var (s, expr) = MRV(d, x);
//                 return (s, e.Function(i, expr));
//             }
//             var (a, b) = d.AsTwoTerms();
//             var (s1, e1) = MRV(a, x);
//             var (s2, e2) = MRV(b, x);
//             return MRVMax1(s1, s2, e.Function(i, e1, e2), x);
//         }
//         else if (e is Power pow && pow.Base != Constant.E)
//         {
//             Expr e1 = 1;
//             Expr b1 = 1;
//             while (e is Power pe)
//             {
//                 b1 = pe.Base;
//                 e1 *= pe.Exp;
//                 e = b1;
//             }
//             if (b1 == 1)
//                 return (new SubsSet(), b1);
//             
//             if (!e1.Constant(x))
//             {
//                 var baseLim = LimitInf(b1, x);
//                 if (baseLim == 1)
//                 {
//                     return MRV(Exp(e1 * (b1 - 1)), x);
//                 }
//                 return MRV(Exp(e1 * Log(b1)), x);
//             }
//             else
//             {
//                 var (s, expr) = MRV(b1, x);
//                 return (s, Pow(expr, e1));
//             }
//         }
//         else if (e is Logarithm log)
//         {
//             var (s, expr) = MRV(log.Value, x);
//             return (s, Log(expr));
//         }
//         else if (e is Power pe && pe.Base != Constant.E)
//         {
//             if (pe.Exp is Logarithm loge)
//             {
//                 return MRV(loge.Value, x);
//             }
//             var li = LimitInf(pe.Exp, x);
//             if (li.IsInfinite)
//             {
//                 var s1 = new SubsSet();
//                 var e1 = s1[e];
//                 var (s2, e2) = MRV(pe.Exp, x);
//                 var su = s1.Union(s2).Item1;
//                 su.Rewrites[e1] = Exp(e2);
//                 return MRVMax3(s1, e1, s2, Exp(e2), su, e1, x);
//             }
//             else
//             {
//                 var (s, expr) = MRV(pe.Exp, x);
//                 return (s, Exp(expr));
//             }
//         }
//         else if (e is Expr)
//         {
//             var l = e.Args.Select(a => MRV(a, x)).ToList();
//             var l2 = l.Where(s => s.Item1 != new SubsSet()).Select(s => s.Item1).ToList();
//             if (l2.Count != 1)
//             {
//                 throw new NotImplementedException("MRV set computation for functions in several variables not implemented.");
//             }
//             var s = l2[0];
//             var ss = new SubsSet();
//             var args = l.Select(x => ss.DoSubs(x.Item2)).ToArray();
//             return (s, e.Eval(args));
//         }
//         
//         throw new NotImplementedException($"Don't know how to calculate the mrv of '{e}'");
//     }
//
//     public static (SubsSet, Expr) MRVMax3(SubsSet f, Expr expsf, SubsSet g, Expr expsg, SubsSet union, Expr expsboth, Variable x)
//     {
//         if (f == new SubsSet())
//         {
//             return (g, expsg);
//         }
//         else if (g == new SubsSet())
//         {
//             return (f, expsf);
//         }
//         else if (f.Meets(g))
//         {
//             return (union, expsboth);
//         }
//
//         var c = Compare(f.Keys.First(), g.Keys.First(), x);
//         if (c == ">")
//         {
//             return (f, expsf);
//         }
//         else if (c == "<")
//         {
//             return (g, expsg);
//         }
//         else
//         {
//             if (c != "=")
//             {
//                 throw new ArgumentException("c should be =");
//             }
//             return (union, expsboth);
//         }
//     }
//
//     public static (SubsSet, Expr) MRVMax1(SubsSet f, SubsSet g, Expr exps, Variable x)
//     {
//         var (u, b) = f.Union(g, exps);
//         return MRVMax3(f, g.DoSubs(exps), g, f.DoSubs(exps), u, b, x);
//     }
//
//     public static int Sign(Expr e, Variable x)
//     {
//         if (!(e is Expr))
//         {
//             throw new ArgumentException("e should be an instance of Expr");
//         }
//
//         if (e.IsPositive)
//         {
//             return 1;
//         }
//         else if (e.IsNegative)
//         {
//             return -1;
//         }
//         else if (e.IsZero)
//         {
//             return 0;
//         }
//         else if (e.Constant(x))
//         {
//             //e = LogCombine(e);
//             var sig = SignExpr.Eval(e);
//             if (sig.IsNumberInt())
//             {
//                 return sig.ToInt();
//             }
//         }
//         else if (e == x)
//         {
//             return 1;
//         }
//         else if (e is Multiplication)
//         {
//             var (a, b) = e.AsTwoTerms();
//             var sa = Sign(a, x);
//             if (sa == 0)
//             {
//                 return 0;
//             }
//             return sa * Sign(b, x);
//         }
//         else if (e is Power pow)
//         {
//             if (pow.Base == Constant.E)
//             {
//                 return 1;
//             }
//             var s = Sign(pow.Base, x);
//             if (s == 1)
//             {
//                 return 1;
//             }
//             if (pow.Exp.IsNumberInt())
//             {
//                 return (int)Math.Pow(s, pow.Exp.ToInt());
//             }
//         }
//         else if (e is Logarithm log)
//         {
//             return Sign(log.Value - 1, x);
//         }
//
//         var (c0, e0) = MRVLeadTerm(e, x);
//         return Sign(c0, x);
//     }
//
//     public static Expr LimitInf(Expr e, Variable x)
//     {
//         if (e.Constant(x))
//             return e;
//         
//         if (e.Has(Order))
//             e = e.Expand().RemoveOrder();
//         
//         if (!x.IsPositive || x.IsInteger)
//         {
//             var p = Variable.CreateDummy("p"); // positive: true
//             e = e.Substitue(x, p);
//             x = p;
//         }
//         //e = e.Rewrite("tractable", deep: true, limitvar: x);
//         //e = PowDenest(e);
//         
//         // TODO: AccumBounds
//         var (c0, e0) = MRVLeadTerm(e, x);
//         
//         var sig = Sign(e0, x);
//         if (sig == 1)
//         {
//             return 0;
//         }
//         else if (sig == -1)
//         {
//             var s = Sign(c0, x);
//             if (s == 0)
//                 throw new ArgumentException("Leading term should not be 0");
//             
//             return s * Expr.Inf;
//         }
//         else if (sig == 0)
//         {
//             if (c0 == e)
//                 c0 = c0.Cancel();
//             
//             return LimitInf(c0, x);
//         }
//         else
//         {
//             throw new ArgumentException($"{sig} could not be evaluated");
//         }
//     }
//
//     public static SubsSet MoveUp2(SubsSet s, Variable x)
//     {
//         var r = new SubsSet();
//         foreach (var kvp in s)
//         {
//             r[kvp.Key.Substitue(x, Exp(x))] = kvp.Value;
//         }
//         foreach (var kvp in s.Rewrites)
//         {
//             r.Rewrites[kvp.Key] = s.Rewrites[kvp.Key].Substitue(x, Exp(x));
//         }
//         return r;
//     }
//
//     public static List<Expr> MoveUp(List<Expr> l, Variable x)
//     {
//         return l.Select(e => e.Substitue(x, Exp(x))).ToList();
//     }
//
//     public static (Expr, Expr) MRVLeadTerm(Expr e, Variable x)
//     {
//         var Omega = new SubsSet();
//         if (e.Constant(x))
//             return (e, 0);
//         
//         if (Omega == new SubsSet())
//         {
//             Omega = MRV(e, x).Item1;
//         }
//         if (Omega == null)
//         {
//             return (e, 0);
//         }
//         if (Omega.ContainsKey(x))
//         {
//             var OmegaUp = MoveUp2(Omega, x);
//             var expsUp = MoveUp(new List<Expr> { e }, x)[0];
//             Omega = OmegaUp;
//             e = expsUp;
//         }
//
//         var w = Variable.CreateUnique("w", positive: true);
//         var (f, logw) = Rewrite(e, Omega, x, w);
//         try
//         {
//             var lt = f.LeadTerm(w, logx: logw);
//         }
//         catch (Exception)
//         {
//             var n0 = 1;
//             var _series = Order(1);
//             var incr = 1;
//             while (_series is Order)
//             {
//                 _series = f.EvalNSeries(w, n: n0 + incr, logx: logw);
//                 incr *= 2;
//             }
//             var series = _series.Expand().RemoveOrder();
//             try
//             {
//                 var lt = series.LeadTerm(w, logx: logw);
//             }
//             catch (Exception)
//             {
//                 var lt = f.AsCoeffExponent(w);
//                 if (lt.Item1.Has(w))
//                 {
//                     var @base = f.AsBaseExp().Item1.AsCoeffExponent(w);
//                     var ex = f.AsBaseExp().Item2;
//                     lt = (Pow(@base.Item1, ex), @base.Item2 * ex);
//                 }
//             }
//         }
//         return (lt.Item1.Substitute(Log(w), logw), lt.Item2);
//     }
//
//     public static Dictionary<Variable, Node> BuildExprTree(SubsSet Omega, Dictionary<Variable, Expr> rewrites)
//     {
//         var nodes = new Dictionary<Variable, Node>();
//         foreach (var kvp in Omega)
//         {
//             var n = new Node
//             {
//                 Var = kvp.Value,
//                 Expr = kvp.Key
//             };
//             nodes[kvp.Value] = n;
//         }
//         foreach (var kvp in Omega)
//         {
//             if (rewrites.ContainsKey(kvp.Value))
//             {
//                 var n = nodes[kvp.Value];
//                 var r = rewrites[kvp.Value];
//                 foreach (var kvp2 in Omega)
//                 {
//                     if (!r.Constant(kvp2.Value))
//                     {
//                         n.Before.Add(nodes[kvp2.Value]);
//                     }
//                 }
//             }
//         }
//         return nodes;
//     }
//
//     public static (Expr, Expr) Rewrite(Expr e, SubsSet Omega, Variable x, Expr wsym)
//     {
//         if (Omega.Count == 0)
//             throw new ArgumentException("Length cannot be 0");
//         
//         foreach (var t in Omega.Keys)
//         {
//             if (!(t is Power pow && pow.Base == Constant.E))
//             {
//                 throw new ArgumentException("Value should be exp");
//             }
//         }
//         var rewrites = Omega.Rewrites;
//         var OmegaList = Omega.ToList();
//
//         var nodes = BuildExprTree(Omega, rewrites);
//         OmegaList.Sort((a, b) => nodes[b.Value].Ht().CompareTo(nodes[a.Value].Ht()));
//
//         Power g = null;
//         int sig = -101343;
//         foreach (var (expr, _) in OmegaList)
//         {
//             sig = Sign(((Power) expr).Exp, x);
//             if (sig != 1 && sig != -1) // && !sig.Has<AccumBounds>()
//             {
//                 throw new NotImplementedException("Result depends on the sign of " + sig);
//             }
//             g = (Power) expr;
//         }
//         if (sig == 1)
//         {
//             wsym = 1 / wsym;
//         }
//
//         var O2 = new List<(Variable, Expr)>();
//         var denominators = new List<long>();
//         foreach (var (fp, var) in OmegaList)
//         {
//             var fexp = ((Power)fp).Exp;
//             var c = LimitInf(fexp / g.Exp, x);
//             if (c is Number num && num.Num.IsFraction)
//             {
//                 denominators.Add(num.Num.Denominator);
//             }
//             var arg = fexp;
//             if (rewrites.ContainsKey(var))
//             {
//                 if (!(rewrites[var] is Power && ((Power)rewrites[var]).Base == Constant.E))
//                 {
//                     throw new ArgumentException("Value should be exp");
//                 }
//                 arg = ((Power)rewrites[var]).Exp;
//             }
//             O2.Add((var, Exp((arg - c * g.Exp).Expand()) * Pow(wsym, c)));
//         }
//
//         var f = PowSimp(e, deep: true, combine: "exp");
//         foreach (var (a, b) in O2)
//         {
//             f = f.Substitute(a, b);
//         }
//
//         foreach (var (_, var) in OmegaList)
//         {
//             if (!f.Constant(var))
//             {
//                 throw new ArgumentException("Expr should not have variable " + var.ToString());
//             }
//         }
//
//         var logw = g.Exp;
//         if (sig == 1)
//         {
//             logw = -logw;
//         }
//
//         var exponent = denominators.Aggregate(1, (a, b) => LCM(a, b));
//         f = f.Substitute(wsym, Pow(wsym, exponent));
//         logw /= exponent;
//
//         f = BottomUp(f, w => w.Normal());
//         f = ExpandMul(f);
//
//         return (f, logw);
//     }
// }
//
// public class Node
// {
//     public List<Node> Before { get; set; }
//     public Expr Expr { get; set; }
//     public Variable Var { get; set; }
//
//     public Node()
//     {
//         Before = new List<Node>();
//     }
//
//     public int Ht()
//     {
//         return Before.Sum(x => x.Ht()) + 1;
//     }
// }