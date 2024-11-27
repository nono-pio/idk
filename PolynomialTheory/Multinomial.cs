namespace PolynomialTheory;

public class Multinomial<T> where T : IEquatable<T>
{
    public T Coef;
    public int[] Degs;

    public Multinomial(T coef, int[] degs)
    {
        Coef = coef;
        Degs = degs;
    }
    
    
}