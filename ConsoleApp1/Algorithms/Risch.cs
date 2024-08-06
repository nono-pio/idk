// using ConsoleApp1.Core.Expressions.Atoms;
// using ConsoleApp1.Core.Expressions.Base;
// using ConsoleApp1.Core.Expressions.Trigonometrie;
// using ConsoleApp1.Core.Models;
//
// namespace ConsoleApp1.Algorithms;
//
// public class Risch
// {
// 	public record ThermData(string[] vars, Expr[] therms, MultiRational[] ddx);
// 	
//
// 	public static Expr? RischAlgorithm(Expr f, string x)
// 	{
// 		var therms = GetTherms(f, x);
// 		var ff = Convert(f, therms);
//
// 		var denK = MultiPoly.Lcm( therms.ddx.Select(d => d.Den) ); // lcm des denominateurs des dérivées des therms
// 		var newDdx = therms.ddx.Select(d => (d * denK).Normal().Num ).ToArray();
//
// 		var d_split = Split(ff.Den);
// 		var h_split = Split(denK);
// 		
// 		var ls = GetSpecials(therms);
// 		
// 		var s = h_split.Item2;
// 		for (var i = 1; i <= ls.Length; i++)
// 			if (ls[i].Item2)
// 				s *= ls[i].Item1;
// 		
// 		var cand_den = s * d_split.Item1 * Deflation(d_split.Item2);
// 		
// 		var dg = 1 + s.Deg() + Math.Max(ff.Num.Deg(), ff.Den.Deg());
// 		var monomials = IterMonomials(therms.vars, dg);
// 		
// 		var cand_num = MultiPoly.Zero;
// 		for (var i = 1; i <= monomials.Count; i++)
// 			cand_num += $"_A{i}" * monomials[i];
// 		
// 		var cand = new MultiRational(cand_num, cand_den);
//         
// 		var lunk = new List<string>();
// 		for (var i = 1; i <= monomials.Count; i++)
// 			lunk.Add($"_A{i}");
//         
// 		var sol = TryIntegral(ff, denK, therms.vars, cand, lunk, d_split.Item1, d_split.Item2, h_split.Item1, ls, 0);
// 		if (sol.Item1)
// 			sol = TryIntegral(ff, denK, therms.vars, cand, lunk, d_split.Item1, d_split.Item2, h_split.Item1, ls, 1);
// 		if (sol.Item1)
// 			return null;
// 		
// 		return sol.Item2;
// 	}
// 	
// 	public static (bool IsFail, Expr Result) TryIntegral(MultiPoly f, MultiPoly denK, string[] vars, MultiRational cand, List<string> lunk, MultiPoly l1, MultiPoly l2, MultiPoly l3, (Expr, bool)[] ls, int K)
// 	{
// 		List<MultiPoly> cand_log_part = [ ..MyFactors(l1, K), ..MyFactors(l2, K), ..MyFactors(l3, K) ];
// 		var candidate = cand;
// 		for (int i = 0; i < cand_log_part.Count; i++)
// 			candidate += $"_B{i}" * Ln(cand_log_part[i]);
//         
//         
// 		var sol = solve(coeffs(numer(normal(f - d(candidate) / denK)), vars), lunk.Union($"_B{i}" for i = 1 to candlog.Length));
// 		return (sol == null, subs(sol, candidate));
// 	}
//
// 	public static (MultiPoly, MultiPoly) Split(MultiPoly p)
// 	{
// 		if (vars.Length == 0)
// 			return (1, p);
// 		
// 		var (c, q) = p.ToPoly(0).Primitive();
// 		var (cs, cn) = Split(c);
//
// 		var s = new PolyRational(Poly.Gcd(q, TotalDerivation(q)), Poly.Gcd(q, q.Derivee(x))).Normal().Num;
// 		if (s.Deg() == 0)
// 			return (cs, cn * q);
// 		
// 		var (hs, hn) = Split(q / s);
// 		
// 		return (cs * hs * s, cn * hn);
// 	}
// 	
// 	public static MultiPoly Deflation(MultiPoly p)
// 	{
// 		if (vars.Length == 0)
// 			return p;
// 		
// 		var (c, q) = p.ToPoly(0).Primitive();
// 		return Deflation(c) * Poly.Gcd(q, q.Derivee());
// 	}
//
// 	public static MultiPoly TotalDerivation(string[] vars, MultiPoly[] ddx, MultiPoly f)
// 	{
// 		MultiPoly fp = MultiPoly.Zero;
// 		for (var i = 1; i <= ddx.Length; i++)
// 			fp += ddx[i] * f.Derivee(vars[i]);
// 		return fp;
// 	}
// 	
// 	public static (MultiPoly, bool)[] GetSpecials(ThermData therm) // return known Darboux polys
// 	{
// 		List<(MultiPoly, bool)> special = new();
// 		for (int i = 0; i < therm.therms.Length; i++)
// 		{
// 			var t = therm.therms[i];
// 			var _x = therm.vars[i];
//
// 			if (t is TanExpr)
// 				special.Add((1 + Pow(_x, 2), false));
// 			// if (f is tanh)
// 			//	return [(1 + subs(l,f), false), (1 - subs(l,f), false)];
// 			// if (f is LambertW)
// 			//        return [(subs(l,f), true)];
// 			
// 		}
// 		
// 		return special.ToArray();
// 	}
// 	
// 	public static MultiPoly[] MyFactors(MultiPoly p, int K)
// 	{
// 		var l = K == 0 ? factors(p) : factors(p, K);
// 		return seq(fact[1], fact = l[2]);
// 	}
//
// 	public static MultiRational Convert(Expr f, ThermData therms)
// 	{
// 		return null;
// 	}
// 	
// 	public static Expr Convert(MultiRational f, ThermData therms)
// 	{
// 		return null;
// 	}
// 	
//     public static ThermData GetTherms(Expr f, string x)
//     {
// 	    return null;
//     }
//     
//     private static HashSet<Expr> Components(Expr f, string x)
//     {
// 	    var result = new HashSet<Expr>();
// 	
// 	    if (!f.Constant(x))
// 	    {
// 		    if (f is Variable)
// 			    result.Add(f);
// 		    else if (f is FonctionExpr)
// 		    {
// 			    foreach (var g in f.Args)
// 				    result.UnionWith(Components(g, x));
//
// 			    result.Add(f);
// 		    }
// 		    else if (f is Power p)
// 		    {
// 			    result.UnionWith(Components(p.Base, x));
//
// 			    if (!p.Exp.IsInteger)
// 				    if (p.Exp is Number n && n.Num.IsFraction)
// 					    result.Add(Pow(p.Base, Num(1, n.Num.Denominator)));
// 				    else
// 				    {
// 					    result.UnionWith(Components(p.Exp, x));
// 					    result.Add(f);
// 				    }
// 		    }
// 		    else
// 			    foreach (var g in f.Args)
// 				    result.UnionWith(Components(g, x));
// 	    }
//
// 	    return result;
//     }
//     
//     public static List<Expr> IterMonomials(string[] vars, int max_degree)
//     {
// 	    List<Expr> monomials_list_comm = [];
// 	    for (int i = 0; i < max_degree+1; i++)
// 	    {
// 		    foreach (var item in combinations_with_replacement(vars, i))
// 		    {
// 			    monomials_list_comm.Add(item);
// 		    } 	
// 	    }
// 	    return monomials_list_comm;
//     }
//
//
//     public static IEnumerable<Expr> combinations_with_replacement(string[] vars, int max_degree)
//     {
//
// 	    foreach (var deg in ListTheory.SumAt(max_degree, vars.Length))
// 	    {
// 		    Expr monomial = 1;
// 		    for (int i = 0; i < vars.Length; i++)
// 		    {
// 			    monomial *= Pow(vars[i], deg[i]);
// 		    }
// 		    yield return monomial;	
// 	    }
//     }
//
//     
// }