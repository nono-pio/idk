using ConsoleApp1.Core.Expressions.Atoms;
using ConsoleApp1.Core.Expressions.Base;
using ConsoleApp1.Core.Expressions.Trigonometrie;
using ConsoleApp1.Core.Models;

namespace ConsoleApp1.Algorithms;

public class Risch
{
	public record ThermData(string[] vars, Expr[] therms, MultiRational[] ddx);
	

	public static Expr? RischAlgorithm(Expr f, string x)
	{
		var therms = GetTherms(f, x);
		var ff = Convert(f, therms);

		var denK = MultiPoly.Lcm( therms.ddx.Select(d => d.Den) ); // lcm des denominateurs des dérivées des therms
		var newDdx = therms.ddx.Select(d => (d * denK).Normal().Num ).ToArray();

		var d_split = Split(ff.Den, therms, newDdx);
		var h_split = Split(denK, therms, newDdx);
		
		var ls = GetSpecials(therms);
		
		var s = h_split.Item2;
		for (var i = 1; i <= ls.Length; i++)
			if (ls[i].Item2)
				s *= ls[i].Item1;
		
		var cand_den = s * d_split.Item1 * Deflation(d_split.Item2, therms);
		
		var dg = 1 + s.Deg() + Math.Max(ff.Num.Deg(), ff.Den.Deg());
		var monomials = IterMonomials(therms.vars.Length, dg);

		var cand_num = new MultiPoly(monomials.Select((degs, i) => new MultiNomial($"_A{i}", degs)).ToArray());
		
		var cand = new MultiRational(cand_num, cand_den);
        
		var lunk = new List<string>();
		for (var i = 1; i <= monomials.Count; i++)
			lunk.Add($"_A{i}");
        
		// var sol = TryIntegral(ff, denK, therms.vars, cand, lunk, d_split.Item1, d_split.Item2, h_split.Item1, ls, 0);
		// if (sol.Item1)
		// 	sol = TryIntegral(ff, denK, therms.vars, cand, lunk, d_split.Item1, d_split.Item2, h_split.Item1, ls, 1);
		// if (sol.Item1)
		// 	return null;
		
		// return sol.Item2;
		return null;
	}
	
	public static (bool IsFail, Expr Result) TryIntegral(MultiPoly f, MultiPoly denK, string[] vars, MultiRational cand, List<string> lunk, MultiPoly l1, MultiPoly l2, MultiPoly l3, (Expr, bool)[] ls, int K)
	{
		// List<MultiPoly> cand_log_part = [ ..MyFactors(l1, K), ..MyFactors(l2, K), ..MyFactors(l3, K) ];
		// var candidate = cand;
		// for (int i = 0; i < cand_log_part.Count; i++)
		// 	candidate += $"_B{i}" * Ln(cand_log_part[i]);
		//       
		//       
		// var sol = solve(coeffs(numer(normal(f - d(candidate) / denK)), vars), lunk.Union($"_B{i}" for i = 1 to candlog.Length));
		// return (sol == null, subs(sol, candidate));
		throw new NotImplementedException();
	}

	public static (MultiPoly, MultiPoly) Split(MultiPoly p, ThermData therms, MultiPoly[] newDdx)
	{
		if (therms.vars.Length == 0)
			return (MultiPoly.One, p);
		
		var (c, q) = p.ToPoly(0).Primitive();
		var (cs, cn) = Split(c, therms, newDdx);

		var s = new PolyRational(PolyOfMultiPoly.Gcd(q, TotalDerivation(therms.vars, newDdx, q.ToMultiPoly(0)).ToPoly(0)), PolyOfMultiPoly.Gcd(q, q.Derivee())).Normal().Num;
		if (s.Deg() == 0)
			return (cs, cn * q.ToMultiPoly(0));
		
		var (hs, hn) = Split((q / s).ToMultiPoly(0), therms, newDdx);
		
		return (cs * hs * s.ToMultiPoly(0), cn * hn);
	}
	
	public static MultiPoly Deflation(MultiPoly p, ThermData therms)
	{
		if (therms.vars.Length == 0)
			return p;
		
		var (c, q) = p.ToPoly(0).Primitive();
		return Deflation(c, therms) * PolyOfMultiPoly.Gcd(q, q.Derivee()).ToMultiPoly(0);
	}

	public static MultiPoly TotalDerivation(string[] vars, MultiPoly[] ddx, MultiPoly f)
	{
		MultiPoly fp = MultiPoly.Zero;
		for (var i = 1; i <= ddx.Length; i++)
			fp += ddx[i] * f.Derivee(vars[i]);
		return fp;
	}
	
	public static (MultiPoly, bool)[] GetSpecials(ThermData therm) // return known Darboux polys
	{
		var nVars = therm.vars.Length;
		List<(MultiPoly, bool)> special = new();
		for (int i = 0; i < therm.therms.Length; i++)
		{
			var t = therm.therms[i];
			var _x = therm.vars[i];

			// 1 + Pow(_x, 2)
			if (t is TanExpr)
			{
				var xPow2 = new int[nVars];
				xPow2[i] = 2;
				
				special.Add((
						new MultiPoly(
						new MultiNomial(1, new int[nVars]), 
						new MultiNomial(1, xPow2)
						), false
						));
			}
			// if (f is tanh)
			//	return [(1 + subs(l,f), false), (1 - subs(l,f), false)];
			// if (f is LambertW)
			//        return [(subs(l,f), true)];
			
		}
		
		return special.ToArray();
	}
	
	public static MultiPoly[] MyFactors(MultiPoly p, int K)
	{
		// var l = K == 0 ? factors(p) : factors(p, K);
		// return seq(fact[1], fact = l[2]);
		return [];
	}

	public static MultiRational Convert(Expr f, ThermData therms)
	{
		return null;
	}
	
	public static Expr Convert(MultiRational f, ThermData therms)
	{
		return null;
	}
	
    public static ThermData GetTherms(Expr f, string x)
    {
	    var comps = Components(f, x);
	    var compsDdx = comps.Select(c => c.Derivee(x)).ToArray();
	    comps.UnionWith(comps.SelectMany(c => Components(c, x)));
	    
	    
	    
	    return null;
    }
    
    private static HashSet<Expr> Components(Expr f, string x)
    {
	    var result = new HashSet<Expr>();
	
	    if (!f.Constant(x))
	    {
		    if (f is Variable)
			    result.Add(f);
		    else if (f is FonctionExpr)
		    {
			    foreach (var g in f.Args)
				    result.UnionWith(Components(g, x));

			    result.Add(f);
		    }
		    else if (f is Power p)
		    {
			    result.UnionWith(Components(p.Base, x));

			    if (!p.Exp.IsInteger)
				    if (p.Exp is Number n && n.Num.IsFraction)
					    result.Add(Pow(p.Base, Num(1, n.Num.Denominator)));
				    else
				    {
					    result.UnionWith(Components(p.Exp, x));
					    result.Add(f);
				    }
		    }
		    else
			    foreach (var g in f.Args)
				    result.UnionWith(Components(g, x));
	    }

	    return result;
    }
    
    public static List<int[]> IterMonomials(int nVars, int max_degree)
    {
	    List<int[]> monomials_list_comm = [];
	    for (int i = 0; i < max_degree+1; i++)
	    {
		    monomials_list_comm.AddRange(
			    combinations_with_replacement(nVars, i)
			    );
	    }
	    return monomials_list_comm;
    }


    public static IEnumerable<int[]> combinations_with_replacement(int nVars, int max_degree)
    {
	    return ListTheory.SumAt(max_degree, nVars);
    }

    
}