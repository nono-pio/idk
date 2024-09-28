using System.Diagnostics;

namespace FLINTDotNet.FMPZPolys;

public struct fmpz_poly_factors
{
    public fmpz cste;
    public List<fmpz_poly> factors;
    public List<int> exp;
        
    public int Length => factors.Count;
        
    public fmpz_poly_factors()
    {
        this.cste = new fmpz(0);
        this.factors = new List<fmpz_poly>();
        this.exp = new List<int>();
    }
        
    public fmpz_poly_factors(fmpz c, List<fmpz_poly> factors, List<int> exponents)
    {
        this.cste = c;
        this.factors = factors;
        this.exp = exponents;
    }

    public void Add(fmpz_poly poly, int exp)
    {
        factors.Add(poly);
        this.exp.Add(exp);
    }
}

public class FMPZPolyFactor
{

    public static fmpz_poly_factors fmpz_poly_factor_deflation(fmpz_poly G, bool deflation)
    {
        int degG = G.Deg;
        fmpz_poly g;
        fmpz_poly_factors fac = new();

        if (degG <= 0) // cste
        {
            if (degG < 0)
                fac.cste = new fmpz(0);
            else
                fac.cste = G.coeffs[0];
            return fac;
        }

        if (degG < 4)
        {
            fac.cste = G.Content();

            if (G.LC < 0)
                fac.cste *= -1;

            g = G.DivExact(fac.cste);

            if (degG == 1) // linear
                fac.Add(g, 1);
            else if (degG == 2)
                _fmpz_poly_factor_quadratic(fac, g, 1);
            else // degG == 3
                _fmpz_poly_factor_cubic(fac, g, 1);

            return fac;
        }
        else
        {
            int i, j, k, d;
            fmpz_poly_factors sq_fr_fac = new();

            for (k = 0; G.coeffs[k] == 0; k++)
            {
            }

            if (k != 0)
            {
                fmpz_poly t = new([(new(1), k)]); // x^k
                fac.Add(t, 1);
            }

            g = G.ShiftRight(k);

            if (deflation && (d = fmpz_poly_deflation(G)) > 1)
            {
                g = g.Deflate(d);
                fmpz_poly_factors gfac = fmpz_poly_factor(g);
                fac.cste = gfac.cste;

                for (i = 0; i < gfac.Length; i++)
                {
                    gfac.factors[i] = gfac.factors[i].Inflate(d);
                    fmpz_poly_factors hfac = fmpz_poly_factor_deflation(gfac.factors[i], false);

                    for (j = 0; j < hfac.Length; j++)
                        fac.Add(hfac.factors[j], gfac.exp[i] * hfac.exp[j]);
                }
            }
            else
            {
                sq_fr_fac = fmpz_poly_factor_squarefree(g); //fmpz_poly_factor_squarefree(sq_fr_fac, g);

                fac.cste = sq_fr_fac.cste;

                /* Factor each square-free part */
                for (j = 0; j < sq_fr_fac.Length; j++)
                {
                    _fmpz_poly_factor_zassenhaus(fac, sq_fr_fac.exp[j], sq_fr_fac.factors[j], 8, 1);
                }
            }
        }

        return fac;
    }

    public static fmpz_poly_factors fmpz_poly_factor(fmpz_poly G)
    {
        return fmpz_poly_factor_deflation(G, true);
    }

    public static fmpz_poly_factors fmpz_poly_factor_squarefree(fmpz_poly F)
    {
        fmpz_poly_factors fac = new();

        fac.cste = F.Content();

        if (F.Length != 0 && F.LC < 0)
            fac.cste *= -1;

        if (F.Deg > 0)
        {
            fmpz_poly f = F.DivExact(fac.cste);

            fmpz_poly t1 = f.Derivative();
            fmpz_poly d = fmpz_poly.Gcd(f, t1);

            if (d.Deg == 0)
            {
                fac.Add(f, 1);
            }
            else
            {
                fmpz_poly s;
                int i;

                fmpz_poly v = f.DivExact(d);
                fmpz_poly w = t1.DivExact(d);

                for (i = 1;; i++)
                {
                    t1 = v.Derivative();
                    s = w - t1;

                    if (s.Length == 0)
                    {
                        if (v.Deg > 0)
                            fac.Add(v, i);
                        break;
                    }

                    d = fmpz_poly.Gcd(d, v);
                    v = v.DivExact(d);
                    w = s.DivExact(d);

                    if (d.Deg > 0)
                        fac.Add(d, i);
                }

            }
        }

        return fac;
    }

    /*
    Let $f$ be a polynomial of degree $m = \deg(f) \geq 2$.
    If another polynomial $g$ divides $f$ then, for all
    $0 \leq j \leq \deg(g)$,
    \begin{equation*}
    \abs{b_j} \leq \binom{n-1}{j} \abs{f} + \binom{n-1}{j-1} \abs{a_m}
    \end{equation*}
    where $\abs{f}$ denotes the $2$-norm of $f$.
    This bound is due to Mignotte, see [Coh1996] page 133.

    This function sets $B$ such that, for all $0 \leq j \leq \deg(g)$,
    $\abs{b_j} \leq B$.

    Consequently, when proceeding with Hensel lifting, we
    proceed to choose an $a$ such that $p^a \geq 2 B + 1$,
    e.g., $a = \ceil{\log_p(2B + 1)}$.

    Note that the formula degenerates for $j = 0$ and $j = n$
    and so in this case we use that the leading (resp.\ constant)
    term of $g$ divides the leading (resp.\ constant) term of $f$.

    [Coh1996] Cohen, Henri : A course in computational algebraic number theory, Springer, 1996
     */
    private static fmpz _fmpz_poly_factor_mignotte(fmpz[] fs, int m)
    {
        int j;

        fmpz f2;
        for (j = 0; j <= m; j++)
            f2 = fmpz_addmul(f2, fs[j], fs[j]);

        f2 = f2.sqrt();
        f2++;

        fmpz lc = fs[m].abs();
        fmpz B = fs[0].abs();

        fmpz b = new(m - 1);
        for (j = 1; j < m; j++)
        {
            fmpz t = b * lc;
            b = b * (m - j);
            b = b.DivExact(j);

            fmpz s = b * f2 + t;
            if (fmpz_cmp(B, s) < 0)
                B = s;
        }

        if (fmpz_cmp(B, lc) < 0)
            B = lc;

        return B;
    }

    public static fmpz fmpz_poly_factor_mignotte(fmpz_poly f)
    {
        return _fmpz_poly_factor_mignotte(f.coeffs, f.Deg);
    }

    private static fmpz_poly_factors _fmpz_poly_factor_zassenhaus(int exp, fmpz_poly f, int cutoff, bool use_van_hoeij)
    {
        fmpz_poly_factors final_fac = new();

        int i, j;
        int r = f.Length;
        mp_limb_t p = 2;
        nmod_poly_t d, g, t;
        nmod_poly_factor_t fac;
        zassenhaus_prune_t Z = new();

        nmod_poly_init_preinv(t, 1, 0);
        nmod_poly_init_preinv(d, 1, 0);
        nmod_poly_init_preinv(g, 1, 0);

        zassenhaus_prune_set_degree(Z, f.Deg);

        for (i = 0; i < 3; i++)
        {
            for (;; p = n_nextprime(p, 0))
            {
                nmod_t mod;

                nmod_init(&mod, p);
                d->mod = mod;
                g->mod = mod;
                t->mod = mod;

                fmpz_poly_get_nmod_poly(t, f);
                if (t.Deg == f.Deg && t.coeffs[0] != 0)
                {
                    nmod_poly_derivative(d, t);
                    nmod_poly_gcd(g, t, d);

                    if (nmod_poly_is_one(g))
                    {
                        nmod_poly_factor_t temp_fac;

                        nmod_poly_factor_init(temp_fac);
                        nmod_poly_factor(temp_fac, t);

                        zassenhaus_prune_start_add_factors(Z);
                        for (j = 0; j < temp_fac->num; j++)
                            zassenhaus_prune_add_factor(Z,
                                temp_fac->p[j].length - 1, temp_fac->exp[j]);
                        zassenhaus_prune_end_add_factors(Z);

                        if (temp_fac->num <= r)
                        {
                            r = temp_fac->num;
                            nmod_poly_factor_set(fac, temp_fac);
                        }

                        break;
                    }
                }
            }

            p = n_nextprime(p, 0);
        }

        p = (fac->p + 0)->mod.n;

        if (r == 1 && r <= cutoff)
        {
            final_fac.Add(f, exp);
        }
        else if (r > cutoff && use_van_hoeij)
        {
            fmpz_poly_factor_van_hoeij(final_fac, fac, f, exp, p);
        }
        else
        {
            int a;
            fmpz_poly_factors lifted_fac;

            fmpz T = fmpz_poly_factor_mignotte(f);

            T = T * f.LC;
            T = 2 * T.abs() + 1;
            a = fmpz_clog_ui(T, p);

            fmpz_poly_hensel_lift_once(lifted_fac, f, fac, a);

            T = p;
            T = T.pow(a);
            final_fac = fmpz_poly_factor_zassenhaus_recombination_with_prune(lifted_fac, f, T, exp, Z);
        }

        return final_fac;
    }
    
    private static fmpz_poly_factors fmpz_poly_factor_zassenhaus_recombination_with_prune(fmpz_poly_factors lifted_fac, fmpz_poly F, fmpz P, int exp, zassenhaus_prune_t Z)
    {
        
        fmpz_poly_factors final_fac = new();
        int r = lifted_fac.Length;
        int[] subset = new int[r];
        int i, k, len, total;
        fmpz_poly Fcopy, Q, tryme;
        fmpz_poly[] tmp = new fmpz_poly[r];
        fmpz_poly** stack;
        fmpz_poly* f;

        for (k = 0; k < r; k++)
            subset[k] = k;

        stack = (fmpz_poly_struct **) flint_malloc(r*sizeof(fmpz_poly_struct *));

        f = (fmpz_poly_struct *) F;

        len = r;
        for (k = 1; k <= len/2; k++)
        {
            zassenhaus_subset_first(subset, len, k);
            while (true)
            {
                total = 0;
                for (i = 0; i < len; i++)
                    if (subset[i] >= 0)
                        total += lifted_fac.factors[subset[i]].Deg;

                if (!zassenhaus_prune_degree_is_possible(Z, total))
                {
                    if (!zassenhaus_subset_next(subset, len))
                        break;
                    continue;
                }

                _fmpz_poly_product(tryme, lifted_fac->p, subset, len, P,
                                                    fmpz_poly_lead(f), stack, tmp);
                fmpz_poly_primitive_part(tryme, tryme);
                if (fmpz_poly_divides(Q, f, tryme))
                {
                    fmpz_poly_factor_insert(final_fac, tryme, exp);
                    f = Fcopy;  /* make sure f is writeable */
                    fmpz_poly_swap(f, Q);
                    len -= k;
                    if (!zassenhaus_subset_next_disjoint(subset, len + k))
                        break;
                }
                else
                {
                    if (!zassenhaus_subset_next(subset, len))
                        break;
                }
            }
        }

        if (f.Deg > 0)
        {
            final_fac.Add(f, exp);
        }
        else
        {
            Debug.Assert(fmpz_poly_is_one(f));
        }

        return final_fac;
    }

}