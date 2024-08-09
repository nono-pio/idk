using ConsoleApp1.Core.Polynomials.UnivariatePolynomials;

namespace ConsoleApp1.Core.Polynomials.Rings;

public abstract class Ring<TElem>
{
    public abstract TElem Zero { get; }
    public abstract TElem One { get; }
    
    /// Convert int to TElem
    public abstract TElem ValueOf(int value);
    
    public abstract bool IsZero(TElem e);
    public abstract bool IsOne(TElem e);
    
    /// Check if the value is int value
    public abstract bool IsInt(TElem e, int value);
    
    /// Add a and b in the ring
    public abstract TElem Add(TElem a, TElem b);

    /// Add a and b in the ring. And a is mutable.
    /// Implement if help for better performance
    public virtual TElem MAdd(TElem a, TElem b) => Add(a, b);

    /// Add multiple elements
    public TElem Add(params TElem[] elems) => Add(elems.AsEnumerable());
    /// Add multiple elements
    public TElem Add(IEnumerable<TElem> elems)
    {
        var sum = Zero;
        foreach (var elem in elems)
            sum = MAdd(sum, elem);

        return sum;
    }
    
    /// Sub a and b in the ring
    public abstract TElem Sub(TElem a, TElem b);

    /// Sub a and b in the ring. And a is mutable.
    /// Implement if help for better performance
    public virtual TElem MSub(TElem a, TElem b) => Sub(a, b);

    public virtual TElem Neg(TElem e) => MulInt(e, -1);
    
    /// Multiply a and b in the ring
    public abstract TElem Mul(TElem a, TElem b);

    /// Multiply a and b in the ring. And a is mutable.
    /// Implement if help for better performance
    public virtual TElem MMul(TElem a, TElem b) => Mul(a, b);

    /// Multiply a and b in the ring.
    public virtual TElem MulInt(TElem a, int b) => Mul(a, ValueOf(b));

    /// Multiply multiple elements
    public TElem Mul(params TElem[] elems) => Mul(elems.AsEnumerable());
    /// Multiply multiple elements
    public TElem Mul(IEnumerable<TElem> elems)
    {
        var pro = One;
        foreach (var elem in elems)
            pro = MMul(pro, elem);

        return pro;
    }

    /// Divide a and b in the ring
    public abstract TElem Div(TElem a, TElem b);

    /// Remainder of a and b in the ring
    public abstract TElem Rem(TElem a, TElem b);
    
    /// Divide and Remainder of a and b in the ring
    public abstract (TElem Quotient, TElem Remainder) DivRem(TElem a, TElem b);

    /// Gcd of a and b in the ring
    public virtual TElem Gcd(TElem a, TElem b)
    {
        while (!IsZero(b))
        {
            var t = b;
            b = Rem(a, b);
            a = t;
        }

        return a;
    }

    /// Extended Gcd of a and b in the ring
    public virtual (TElem Gcd, TElem s, TElem t) ExtendedGcd(TElem a, TElem b)
    {
        TElem s = Zero, old_s = One;
        TElem t = One, old_t = Zero;
        TElem r = b, old_r = a;

        while (!IsZero(r))
        {
            var quotient = Div(old_r, r);
            (old_r, r) = (r, Sub(old_r, Mul(quotient, r)));
            (old_s, s) = (s, Sub(old_s, Mul(quotient, s)));
            (old_t, t) = (t, Sub(old_t, Mul(quotient, t)));
        }

        return (old_r, old_s, old_t);
    }

    /// Gcd of the elements in the ring
    public TElem Gcd(TElem[] elems)
    {
        if (elems.Length == 0)
            throw new ArgumentException("Array is empty", nameof(elems));
        
        var gcd = elems[0];
        for (int i = 1; i < elems.Length; i++)
        {
            gcd = Gcd(gcd, elems[i]);
        }

        return gcd;
    }

    /// Lcm of a and b in the ring
    public virtual TElem Lcm(TElem a, TElem b) => Div(Mul(a, b), Gcd(a, b));

    /// Lcm of the elements in the ring
    public TElem Lcm(TElem[] elems)
    {
        if (elems.Length == 0)
            throw new ArgumentException("Array is empty", nameof(elems));
        
        var lcm = elems[0];
        for (int i = 1; i < elems.Length; i++)
        {
            lcm = Lcm(lcm, elems[i]);
        }

        return lcm;
    }
}