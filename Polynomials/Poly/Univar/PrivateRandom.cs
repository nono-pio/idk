namespace Polynomials.Poly.Univar;

public static class PrivateRandom
{
    private static readonly ThreadLocal<Random> ThreadLocalRandom =
        new ThreadLocal<Random>(() => new Random(0x7f67fcad));

    public static Random GetRandom()
    {
        return ThreadLocalRandom.Value;
    }
}