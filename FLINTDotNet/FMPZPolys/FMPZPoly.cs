namespace FLINTDotNet.FMPZPolys;

public struct fmpz
{
    private int x;
    
    public fmpz(int x)
    {
        this.x = x;
    }
}

public struct fmpz_poly
{
    public fmpz[] coeffs;
    
    public int Length => coeffs.Length;
    public int Deg => coeffs.Length - 1;
    public fmpz LC => coeffs[Deg];
    
    public fmpz_poly(fmpz[] coeffs)
    {
        this.coeffs = coeffs;
    }
    
    public fmpz_poly((fmpz, int)[] monomials)
    {
        coeffs = new fmpz[monomials.Max(m => m.Item2) + 1];
        foreach (var (coef, i) in monomials)
            coeffs[i] = coef;
    }
    
    public fmpz Content()
    {
        throw new NotImplementedException();
    }
    
    public fmpz_poly DivExact(fmpz c)
    {
        throw new NotImplementedException();
    }
    
    public fmpz_poly DivExact(fmpz_poly c)
    {
        throw new NotImplementedException();
    }

    public fmpz_poly ShiftRight(int k)
    {
        throw new NotImplementedException();
    }

}