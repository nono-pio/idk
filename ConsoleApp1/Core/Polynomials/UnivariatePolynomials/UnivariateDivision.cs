using ConsoleApp1.Core.Polynomials.Rings;

namespace ConsoleApp1.Core.Polynomials.UnivariatePolynomials;

public class UnivariateDivision
{

    public static UnivariatePolynomial<TElem>? RemCheck<TElem>(UnivariatePolynomial<TElem> n, UnivariatePolynomial<TElem> d)
    {
        if (n.Deg < d.Deg) {
            return n;
        } else if (d.Deg == 0) {
            return n.Zero;
        } else if (d.Deg == 1) {
            var ring = n.Ring;
            var (pNull, p) = ring.SafeDiv(ring.Neg(d.CC), d.LC);
            return pNull ? null : n.ValueOf(n.Eval(p));
        } else {
            return null;
        }
    }
    
    public static (UnivariatePolynomial<TElem> Quotient, UnivariatePolynomial<TElem> Remainder) DivisonWithRemainder<TElem>(UnivariatePolynomial<TElem> n, UnivariatePolynomial<TElem> d)
    {
        
        if (d.IsZero(d))
            throw new DivideByZeroException("Division by zero");
        
        if (n.IsZero(n))
            return (n.Zero, n.Zero);
        if (n.Deg < d.Deg)
            return (n.Zero, n);
        if (d.Deg == 0) 
            return (n / d.LC, n.Zero);
        
        
        var ring = n.Ring;
        var r = n;
        TElem[] q = new TElem[n.Deg - d.Deg + 1];

        TElem lcMultiplier, lcDivider;
        // if (ring.isField()) {
        //     lcMultiplier = ring.reciprocal(divider.lc());
        //     lcDivider = ring.getOne();
        // } else {
        lcMultiplier = ring.One;
        lcDivider = d.LC;
        // }

        for (int i = n.Deg - d.Deg; i >= 0; --i) {
            if (r.Deg == d.Deg + i) {
                TElem quot = ring.Div(ring.Mul(r.LC, lcMultiplier), lcDivider);

                q[i] = quot;
                r -= r.MonomialToPolynomial(q[i], i) * d;

            } else q[i] = ring.Zero;
        }

        return (new UnivariatePolynomial<TElem>(ring, q), r);
    }
    
}