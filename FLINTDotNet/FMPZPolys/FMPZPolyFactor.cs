namespace FLINTDotNet.FMPZPolys;

using System;
using System.Numerics;

public class FmpzPolyFactors
{
    public BigInteger C { get; set; } = 0;
    public List<FmpzPoly> P { get; set; } = new();
    public List<int> Exp { get; set; } = new();
    
    public int Length => P.Count;
    
    public void Insert(FmpzPoly poly, int exp)
    {
        P.Add(poly);
        Exp.Add(exp);
    }
}

public class FmpzPoly
{
    public BigInteger[] Coeffs { get; set; }
    public int Length => Coeffs.Length;
    public int Deg => Coeffs.Length - 1;
    public BigInteger LC => Coeffs[Deg];

    
    public FmpzPoly(BigInteger[] coeffs)
    {
        Coeffs = coeffs;
    }
    
    public FmpzPoly() : this([]) {}

    public BigInteger Content()
    {
        throw new NotImplementedException();
    }
    
    public static void SetCoeffUi(FmpzPoly poly, int index, BigInteger value) => poly.Coeffs[index] = value;
    public FmpzPoly ShiftRight(int k) { /* Implement shift right logic */ }
    public static void Factor(FmpzPolyFactors fac, FmpzPoly g) { /* Implement factor logic */ }
    public static void FactorSquarefree(FmpzPolyFactors fac, FmpzPoly g) { /* Implement squarefree factor logic */ }
    public static void Inflate(FmpzPoly source, FmpzPoly target, int d) { /* Implement inflate logic */ }
    public static int Deflation(FmpzPoly g) { /* Implement deflation logic */ return 1; }
    public FmpzPoly ScalarDivExact(BigInteger c) { /* Implement scalar division logic */ }
    public static int Sgn(BigInteger value) => value.Sign;
}

public class Program
{
    public static FmpzPolyFactors FmpzPolyFactorDeflation(FmpzPoly G, int deflation)
    {
        FmpzPolyFactors fac = new FmpzPolyFactors(); 
        int lenG = G.Length;

        if (lenG <= 1)
        {
            if (lenG < 1)
                fac.C = 0;
            else
                fac.C = G.Coeffs[0];
            return fac;
        }


        if (lenG < 5)
        {
            fac.C = G.Content();
            if (G.LC < 0)
                fac.C = -fac.C;
            FmpzPoly g = G.ScalarDivExact(fac.C);

            if (lenG < 3)
                fac.Insert(g, 1);
            else if (lenG == 3)
                FmpzPoly.FactorQuadratic(fac, g, 1);
            else
                FmpzPoly.FactorCubic(fac, g, 1);
        }
        else
        {
            int k, d;
            FmpzPolyFactors sq_fr_fac;

            for (k = 0; G.Coeffs[k] == 0; k++)
            { }

            if (k != 0)
            {
                FmpzPoly t = new FmpzPoly([0, 1]); // t = 0+1x
                fac.Insert(t, k);
            }

            FmpzPoly g = G.ShiftRight(k);

            if (deflation != 0 && (d = FmpzPoly.Deflation(G)) > 1)
            {
                FmpzPoly.Deflate(g, g, d);
                FmpzPolyFactors gfac = FmpzPolyFactor(g);
                fac.C = gfac.C;

                for (int i = 0; i < gfac.Length; i++)
                {
                    FmpzPoly.Inflate(gfac.P[i], gfac.P[i], d);
                    FmpzPolyFactors hfac = FmpzPolyFactorDeflation(gfac.P[i], 0);

                    for (int j = 0; j < hfac.Length; j++)
                        fac.Insert(hfac.P[j], gfac.Exp[i] * hfac.Exp[j]);
                }
            }
            else
            {
                FmpzPolyFactors sqFrFac = FmpzPolyFactorSquarefree(g);

                fac.C = sqFrFac.C;

                for (int j = 0; j < sqFrFac.Length; j++)
                {
                    FmpzPoly.FactorZassenhaus(fac, sqFrFac.Exp[j], sqFrFac.P[j], 8, 1);
                }

            }
        }

        return fac;
    }

    public static FmpzPolyFactors FmpzPolyFactor(FmpzPoly G)
    {
        return FmpzPolyFactorDeflation(G, 1);
    }
    
    public void FmpzPolyFactorSquarefree(FmpzPolyFactors fac, FmpzPoly F)
    {
        fac.C = F.Content();

        if (F.Length != 0 && F.LC < 0)
            fac.C = -fac.C;

        if (F.Length > 1)
        {
            FmpzPoly f = F.ScalarDivExact(fac.C);

            FmpzPoly t1 = f.Derivative();
            FmpzPoly d = FmpzPoly.Gcd(f, t1);

            if (d.Length == 1)
            {
                fac.Insert(f, 1);
            }
            else
            {
                FmpzPoly s;
                long i;
                
                FmpzPoly v = f.DivExact(d);
                FmpzPoly w = t1.DivExact(d);

                for (i = 1; ; i++)
                {
                    t1 = v.Derivative();
                    s = w - t1;

                    if (s.Length == 0)
                    {
                        if (v.Length > 1)
                            fac.Insert(v, i);
                        break;
                    }

                    d = FmpzPoly.Gcd(v, s);
                    v = v.DivExact(d);
                    w = s.Divexact(d);

                    if (d.Length > 1)
                        fac.Insert(d, i);
                }
            }
        }
    }

}
public class PolynomialFactorization
{
    public static BigInteger FmpzPolyFactorMignotte(BigInteger[] f, int m)
    {
        BigInteger f2 = 0;
        for (int j = 0; j <= m; j++)
            f2 += f[j] * f[j];
        
        f2 = BigInteger.Sqrt(f2);
        f2 += 1;

        BigInteger lc = BigInteger.Abs(f[m]);

        BigInteger B = BigInteger.Abs(f[0]);

        BigInteger b = m - 1;
        for (int j = 1; j < m; j++)
        {
            var t = b * lc;

            b = b * (m - j) / j;

            var s = b * f2 + t;
            if (B < s)
                B = s;
        }

        if (B < lc)
            B = lc;
        
        return B;
    }

    public static void FmpzPolyFactorZassenhaus(List<Tuple<BigInteger[], int>> finalFac, int exp, BigInteger[] f, int cutoff, bool useVanHoeij)
    {
        int lenF = f.Length;

        if (lenF < 5)
        {
            if (lenF < 3)
                InsertFactor(finalFac, f, exp);
            else if (lenF == 3)
                FactorQuadratic(finalFac, f, exp);
            else
                FactorCubic(finalFac, f, exp);

            return;
        }
        else
        {
            long i, j;
            long r = lenF;
            ulong p = 2;
            NmodPoly d = new NmodPoly(), g = new NmodPoly(), t = new NmodPoly();
            NmodPolyFactors fac = new();
            ZassenhausPrune Z = new ZassenhausPrune();

            t.InitPreinv(1, 0);
            d.InitPreinv(1, 0);
            g.InitPreinv(1, 0);

            Z.SetDegree(lenF - 1);

            for (i = 0; i < 3; i++)
            {
                while (true)
                {
                    Nmod mod = new Nmod(p);
                    d.Mod = mod;
                    g.Mod = mod;
                    t.Mod = mod;

                    t.SetFromFmpzPoly(f);
                    if (t.Length == lenF && t.Coeffs[0] != 0)
                    {
                        d.Derivative(t);
                        g.Gcd(t, d);

                        if (g.IsOne())
                        {
                            NmodPolyFactor temp_fac = new NmodPolyFactor();
                            temp_fac.Init();
                            temp_fac.Factor(t);

                            Z.StartAddFactors();
                            for (j = 0; j < temp_fac.Num; j++)
                                Z.AddFactor(temp_fac.P[j].Length - 1, temp_fac.Exp[j]);
                            Z.EndAddFactors();

                            if (temp_fac.Num <= r)
                            {
                                r = temp_fac.Num;
                                fac.Set(temp_fac);
                            }
                            temp_fac.Clear();
                            break;
                        }
                    }
                    p = NumberTheory.NextPrime(p);
                }
                p = NumberTheory.NextPrime(p);
            }
            d.Clear();
            g.Clear();
            t.Clear();

            p = fac.P[0].Mod.N;

            if (r == 1 && r <= cutoff)
            {
                final_fac.Insert(f, exp);
            }
            else if (r > cutoff && use_van_hoeij)
            {
                final_fac.VanHoeij(fac, f, exp, p);
            }
            else
            {
                long a;
                Fmpz T = new Fmpz();
                FmpzPolyFactor lifted_fac = new FmpzPolyFactor();

                lifted_fac.Init();
                T.Init();

                f.FactorMignotte(T);
                // bound adjustment, we multiply true factors (which might be
                // monic) by the leading coefficient of f in the implementation
                // below
                T.Mul(f.Coeffs[f.Length - 1]);
                T.Abs();
                T.MulUi(2);
                T.AddUi(1);
                a = T.ClogUi(p);

                f.HenselLiftOnce(lifted_fac, fac, a);

                #if TRACE_ZASSENHAUS == 1
                Console.WriteLine($"|p = {p}, a = {a}");
                Console.WriteLine("|Pre hensel lift factorisation (nmod_poly):");
                fac.Print();
                Console.WriteLine("|Post hensel lift factorisation (fmpz_poly):");
                lifted_fac.Print();
                #endif

                T.SetUi(p);
                T.PowUi(a);
                final_fac.ZassenhausRecombinationWithPrune(lifted_fac, f, T, exp, Z);

                lifted_fac.Clear();
                T.Clear();
            }

            fac.Clear();
            Z.Clear();
        }
    }

    private static void InsertFactor(List<Tuple<BigInteger[], int>> fac, BigInteger[] poly, int exp)
    {
        fac.Add(new Tuple<BigInteger[], int>(poly, exp));
    }

    private static void FactorQuadratic(List<Tuple<BigInteger[], int>> fac, BigInteger[] poly, int exp)
    {
        // Implementation of quadratic factorization
    }

    private static void FactorCubic(List<Tuple<BigInteger[], int>> fac, BigInteger[] poly, int exp)
    {
        // Implementation of cubic factorization
    }
    private static BigInteger PolyContent(BigInteger[] poly)
    {
        // Implementation of polynomial content calculation
        return BigInteger.Zero; // Placeholder
    }

    private static BigInteger[] PolyScalarDivide(BigInteger[] poly, BigInteger scalar)
    {
        // Implementation of polynomial scalar division
        return new BigInteger[0]; // Placeholder
    }
}

public static class FmpzPolyFactorZassenhaus
{
    private static void ProductHelper(
        FmpzPoly res,
        List<FmpzPoly> liftedFac,
        int[] subset,
        int len,
        int P,
        int leadf,
        List<FmpzPoly> stack,
        List<FmpzPoly> tmp)
    {
        int k = 0;
        for (int i = 0; i < len; i++)
        {
            if (subset[i] < 0)
                continue;

            stack[k] = liftedFac[subset[i]];
            k++;

            for (int j = k - 1; j > 0 && stack[j - 1].Length < stack[j].Length; j--)
            {
                var t = stack[j - 1];
                stack[j - 1] = stack[j];
                stack[j] = t;
            }
        }

        while (k > 1)
        {
            for (int j = 1; j < k; j++)
                Debug.Assert(stack[j - 1].Length >= stack[j].Length);

            FmpzPoly.Mul(res, stack[k - 2], stack[k - 1]);
            FmpzPoly.ScalarSmodFmpz(res, res, P);

            k--;
            stack[k - 1] = tmp[k - 1]; // make sure stack[k - 1] is writeable
            FmpzPoly.Swap(res, stack[k - 1]);

            for (int j = k - 1; j > 0 && stack[j - 1].Length < stack[j].Length; j--)
            {
                var t = stack[j - 1];
                stack[j - 1] = stack[j];
                stack[j] = t;
            }
        }

        if (k == 1)
        {
            FmpzPoly.ScalarMulFmpz(res, stack[0], leadf);
            FmpzPoly.ScalarSmodFmpz(res, res, P);
        }
        else
        {
            Debug.Assert(false);
            FmpzPoly.One(res);
        }
    }

    public static void Recombination(
        FmpzPolyFactor finalFac,
        FmpzPolyFactor liftedFac,
        FmpzPoly F,
        int P,
        int exp)
    {
        int r = liftedFac.Num;
        int[] subset = new int[r];
        for (int k = 0; k < r; k++)
            subset[k] = k;

        var stack = new List<FmpzPoly>(r);
        var tmp = new List<FmpzPoly>(r);
        for (int k = 0; k < r; k++)
            tmp.Add(new FmpzPoly());

        var Q = new FmpzPoly();
        var tryme = new FmpzPoly();
        var Fcopy = new FmpzPoly();

        var f = F;

        int len = r;
        for (int k = 1; k <= len / 2; k++)
        {
            ZassenhausSubsetFirst(subset, len, k);
            while (true)
            {
                ProductHelper(tryme, liftedFac.P, subset, len, P, f.Coefficients.Last(), stack, tmp);
                FmpzPoly.PrimitivePart(tryme, tryme);
                if (FmpzPoly.Divides(Q, f, tryme))
                {
                    FmpzPolyFactor.Insert(finalFac, tryme, exp);
                    f = Fcopy;  // make sure f is writeable
                    FmpzPoly.Swap(f, Q);
                    len -= k;
                    if (!ZassenhausSubsetNextDisjoint(subset, len + k))
                        break;
                }
                else
                {
                    if (!ZassenhausSubsetNext(subset, len))
                        break;
                }
            }
        }

        if (FmpzPoly.Degree(f) > 0)
        {
            FmpzPolyFactor.Insert(finalFac, f, exp);
        }
        else
        {
            Debug.Assert(FmpzPoly.IsOne(f));
        }
    }

    // Placeholder methods for Zassenhaus subset operations
    private static void ZassenhausSubsetFirst(int[] subset, int len, int k) { }
    private static bool ZassenhausSubsetNext(int[] subset, int len) { return false; }
    private static bool ZassenhausSubsetNextDisjoint(int[] subset, int len) { return false; }
}

public class ZassenhausPrune
{
    public byte[] PosDegs;
    public long[] NewDegs;
    public long Alloc;
    public long Deg;
    public long NewLength;
    public long NewTotal;

    public void Clear()
    {
        if (Alloc > 0)
        {
            PosDegs = null;
            NewDegs = null;
        }
    }

    public void SetDegree(long d)
    {
        if (d < 1)
        {
            throw new ArgumentException("Invalid degree", nameof(d));
        }

        if (Alloc > 0)
        {
            Array.Resize(ref PosDegs, (int)(d + 1));
            Array.Resize(ref NewDegs, (int)(d + 1));
        }
        else
        {
            PosDegs = new byte[d + 1];
            NewDegs = new long[d + 1];
        }
        Alloc = d + 1;
        Deg = d;

        for (int i = 0; i <= d; i++)
            PosDegs[i] = 1;

        NewLength = 0;
        NewTotal = 0;
    }

    public void AddFactor(long deg, long exp)
    {
        if (exp < 1 || deg < 1)
            return;

        for (int i = 0; i < exp; i++)
        {
            if (NewLength >= Deg)
            {
                throw new InvalidOperationException("Invalid operation in AddFactor");
            }
            NewTotal += deg;
            NewDegs[NewLength] = deg;
            NewLength++;
        }
    }

    public void EndAddFactors()
    {
        if (NewTotal != Deg)
        {
            throw new InvalidOperationException("Invalid operation in EndAddFactors");
        }

        byte[] a = PosDegs;
        byte posMask = 1;
        byte newMask = 2;

        a[0] |= newMask;
        for (long j = 1; j <= Deg; j++)
            a[j] &= (byte)~newMask;

        for (long i = 0; i < NewLength; i++)
        {
            long d = NewDegs[i];

            for (long j = Deg; j >= 0; j--)
            {
                if ((a[j] & newMask) != 0)
                {
                    if (j + d > Deg)
                    {
                        throw new InvalidOperationException("Invalid operation in EndAddFactors");
                    }
                    a[j + d] |= newMask;
                }
            }
        }

        for (long j = 0; j <= Deg; j++)
            a[j] &= (byte)(a[j] >> 1);

        if (a[0] != posMask || a[Deg] != posMask)
        {
            throw new InvalidOperationException("Invalid operation in EndAddFactors");
        }
    }

    public bool MustBeIrreducible()
    {
        for (long i = 1; i < Deg; i++)
        {
            if (PosDegs[i] != 0)
                return false;
        }

        return true;
    }
}

public class ZassenhausSubset
{
    // First subset of size m
    public static void SubsetFirst(long[] s, long r, long m)
    {
        Debug.Assert(0 <= m && m <= r);

        for (long i = 0; i < r; i++)
        {
            if (i >= m)
                s[i] = (s[i] < 0) ? s[i] : -s[i] - 1;
            else
                s[i] = (s[i] >= 0) ? s[i] : -s[i] - 1;
        }
    }

    // Next subset of the same size, returns true for success, false for failure
    public static bool SubsetNext(long[] s, long r)
    {
        long i = 0, j, k;

        while (i < r && s[i] < 0)
            i++;
        j = i;

        while (i < r && s[i] >= 0)
            i++;
        k = i;

        if (k == 0 || k >= r)
            return false;

        s[k] = -s[k] - 1;
        s[k - 1] = -s[k - 1] - 1;

        if (j > 0)
        {
            for (i = 0; i < k - j - 1; i++)
                if (s[i] < 0)
                    s[i] = -s[i] - 1;

            for (i = k - j - 1; i < k - 1; i++)
                if (s[i] >= 0)
                    s[i] = -s[i] - 1;
        }
        return true;
    }

    // Next subset of same size and disjoint from current, delete current indices
    public static long SubsetNextDisjoint(long[] s, long r)
    {
        long i, j, last = r - 1, total = 0;

        for (i = 0; i < r; i++)
        {
            if (s[i] >= 0)
            {
                total++;
                last = i;
            }
        }

        j = 0;
        for (i = 0; i < r; i++)
            if (s[i] < 0)
                s[j++] = s[i];

        if (r - total < total || total < 1 || last == r - 1)
            return 0;

        long min = Math.Min(total - 1, last - total + 1);

        for (i = 0; i < min; i++)
            s[i] = -s[i] - 1;

        for (i = last - total + 1; i < last - min + 1; i++)
            s[i] = -s[i] - 1;

        return 1;
    }
}

public class FmpzPolyFactor
{
    public static long HeuristicVanHoeijStartingPrecision(FmpzPoly f, long r, ulong p)
    {
        BigInteger leadB = new BigInteger();
        BigInteger trailB = new BigInteger();

        FmpzPoly.CLDBound(out leadB, f, f.Length - 2);
        FmpzPoly.CLDBound(out trailB, f, 0);

        long minB = Math.Min(leadB.GetBitLength(), trailB.GetBitLength());

        long a = (long)((2.5 * r + minB) * Math.Log(2) + Math.Log(f.Length) / 2.0) / Math.Log(p);

        return a;
    }

    public static void VanHoeijResizeMatrix(FmpzMat M, long numRows)
    {
        if (M.RowCount == numRows)
            return; // nothing to be done

        List<BigInteger[]> emptyRows = new List<BigInteger[]>();

        // Clear rows that aren't needed
        for (long i = numRows; i < M.RowCount; i++)
        {
            Array.Clear(M.Rows[i], 0, M.ColumnCount);

            // This row can be repopulated
            if (i < M.Rows.Length)
                emptyRows.Add(M.Rows[i]);
        }

        for (long i = 0; i < numRows; i++)
        {
            if (i >= M.Rows.Length) // This row must be moved back to empty spot
            {
                BigInteger[] oldRow = M.Rows[i];
                BigInteger[] newRow = emptyRows[emptyRows.Count - 1];
                emptyRows.RemoveAt(emptyRows.Count - 1);

                for (long j = 0; j < M.ColumnCount; j++)
                {
                    BigInteger temp = oldRow[j];
                    oldRow[j] = newRow[j];
                    newRow[j] = temp;
                }

                M.Rows[i] = newRow;
            }
        }

        M.RowCount = numRows;
    }

    public static void VanHoeij(FmpzPolyFactor finalFac, NmodPolyFactor fac, FmpzPoly f, long exp, ulong p)
    {
        FmpzPolyFactor liftedFac = new FmpzPolyFactor();
        FmpzMat M = new FmpzMat(fac.Count, fac.Count);
        BigInteger B = new BigInteger();
        BigInteger lc = new BigInteger();
        long r = fac.Count;
        long bitR = Math.Max(r, 20);
        long UExp = (long)BigInteger.Log2(bitR);

        // Set M to identity matrix
        for (long i = 0; i < r; i++)
            M[i, i] = BigInteger.One;

        // Prescale the identity matrix by 2^U_exp
        M = M * BigInteger.Pow(2, UExp);

        // Compute Mignotte bound
        FmpzPoly.FactorMignotte(out B, f);
        B *= f[f.Length - 1];
        B = BigInteger.Abs(B);
        B = B * 2 + 1;
        long a = (long)BigInteger.Log(B, p);

        // Compute heuristic starting precision
        a = Math.Min(a, HeuristicVanHoeijStartingPrecision(f, r, p));

        // Start Hensel lift
        List<FmpzPoly> v = new List<FmpzPoly>(2 * r - 2);
        List<FmpzPoly> w = new List<FmpzPoly>(2 * r - 2);
        List<long> link = new List<long>(2 * r - 2);

        for (int i = 0; i < 2 * r - 2; i++)
        {
            v.Add(new FmpzPoly());
            w.Add(new FmpzPoly());
        }

        long prevExp = FmpzPoly.HenselStartLift(liftedFac, link, v, w, f, fac, a);

        // Compute bound
        B = (r + 1) * BigInteger.Pow(2, 2 * UExp);

        // Compute leading coefficient
        long N = f.Length - 1;
        ulong sqN = (ulong)Math.Sqrt(N);
        lc = f[N];

        // Main Hensel loop
        int henselLoops = 0;
        BigInteger P = BigInteger.Pow(p, a);
        BigInteger boundSum = new BigInteger();
        FmpzMat col = new FmpzMat(r, 1);
        FmpzLLL fl = new FmpzLLL();

        while (!CheckIfSolved(M, finalFac, liftedFac, f, P, exp, lc))
        {
            long numCoeffs = (henselLoops < 3 && 3 * r > N + 1) ? (r > 200 ? 50 : 30) : 10;
            numCoeffs = Math.Min(numCoeffs, (N + 1) / 2);
            long prevNumCoeffs = 0;

            do
            {
                FmpzMat data = new FmpzMat(r + 1, 2 * numCoeffs);
                long numDataCols = FmpzPoly.FactorCLDMat(data, f, liftedFac, P, numCoeffs);

                for (long nextCol = prevNumCoeffs; nextCol < numDataCols - prevNumCoeffs; nextCol++)
                {
                    long altCol, diff = nextCol - prevNumCoeffs;

                    if ((diff % 2) == 0)
                        altCol = prevNumCoeffs + diff / 2;
                    else
                        altCol = numDataCols - prevNumCoeffs - (diff + 1) / 2;

                    boundSum = data[r, altCol] * sqN;
                    long worstExp = boundSum.GetBitLength();

                    for (long i = 0; i < r; i++)
                        col[i, 0] = data[i, altCol];

                    bool doLLL = NextColVanHoeij(M, P, col, worstExp, UExp);

                    if (doLLL)
                    {
                        long numRows = fl.WrapperWithRemovalKnapsack(M, null, B);

                        VanHoeijResizeMatrix(M, numRows);

                        if (CheckIfSolved(M, finalFac, liftedFac, f, P, exp, lc))
                            return;
                    }
                }

                prevNumCoeffs = numCoeffs;
                numCoeffs = Math.Min(2 * numCoeffs, (N + 1) / 2);

            } while (numCoeffs != prevNumCoeffs);

            henselLoops++;

            prevExp = FmpzPoly.HenselContinueLift(liftedFac, link, v, w, f, prevExp, a, 2 * a, p);

            a = 2 * a;
            P = BigInteger.Pow(p, a);
        }
    }
}

public class FmpzPolyFactor
{
    private static int ComparePolyLengths(FmpzPoly a, FmpzPoly b)
    {
        return a.Length - b.Length;
    }

    public static bool CheckIfSolved(FmpzMat M, FmpzPolyFactor finalFac, FmpzPolyFactor liftedFac,
                                     FmpzPoly f, BigInteger P, long exp, BigInteger lc)
    {
        var trialFactors = new FmpzPolyFactor();
        var prod = new FmpzPoly();
        var q = new FmpzPoly();
        var fCopy = new FmpzPoly(f);
        var tempLc = new BigInteger();
        var U = new FmpzMat(M, 0, 0, M.Rows, liftedFac.Num);
        var f2 = new NmodPoly(2);
        var g2 = new NmodPoly(2);
        var rem = new NmodPoly(2);
        int numFacs, res = 0;
        long r = liftedFac.Num;

        var part = new long[r];

        numFacs = U.ColPartition(part, 1);
        if (numFacs == 0 || numFacs > r)
            return false;

        if (numFacs == 1)
        {
            finalFac.Insert(f, exp);
            return true;
        }

        tempLc = lc;

        for (int i = 1; i <= numFacs; i++)
        {
            prod.SetCoefficient(tempLc);

            for (int j = 0; j < r; j++)
            {
                if (part[j] == i)
                {
                    prod.Mul(liftedFac.P[j]);
                    prod.ScalarSmodFmpz(P);
                }
            }

            tempLc = prod.Content().Abs();
            prod.ScalarDivexactFmpz(tempLc);

            trialFactors.Insert(prod, 1);
        }

        trialFactors.P = trialFactors.P.OrderBy(p => p.Length).ToArray();

        for (int i = 0; i < trialFactors.Num && numFacs > 1; i++)
        {
            f2.Set(fCopy);
            g2.Set(trialFactors.P[i]);

            rem.Rem(f2, g2);

            if (rem.IsZero() && fCopy.Divides(q, trialFactors.P[i]))
            {
                FmpzPoly.Swap(ref q, ref fCopy);
                numFacs--;
            }
            else
                return false;
        }

        if (numFacs == 1)
        {
            for (int j = 0; j < i; j++)
                finalFac.Insert(trialFactors.P[j], exp);

            finalFac.Insert(fCopy, exp);

            res = 1;
        }

        return res == 1;
    }
}

public class FmpzPolyFactor
{
    public static long FmpzPolyFactorCLDMat(FmpzMat res, FmpzPoly f,
                                            FmpzPolyFactor liftedFac, BigInteger P, ulong k)
    {
        long i, zeroes, bound, loN, hiN, r = liftedFac.Num;
        long bitR = Math.Max(r, 20);
        FmpzPoly gd = new FmpzPoly(), gcld = new FmpzPoly(), temp = new FmpzPoly();
        FmpzPoly truncF, truncFac; // don't initialize truncF, truncFac

        // Insert CLD bounds in last row of matrix
        for (i = 0; i < k; i++)
        {
            res.Rows[r][i] = FmpzPoly.CLDBound(f, i);
            res.Rows[r][2 * k - i - 1] = FmpzPoly.CLDBound(f, f.Length - i - 2);
        }

        // We exclude columns in the middle for which CLD bounds are too large
        BigInteger t = new BigInteger();

        bound = (int)(BigInteger.Log(P, 2) - bitR - bitR / 2); // log_2(p^a / 2^{1.5r})

        for (loN = 0; loN < k; loN++)
        {
            t = res.Rows[r][loN] * (long)Math.Sqrt(f.Length);

            if (t.GetBitLength() > bound)
                break;
        }

        t = BigInteger.Zero;

        for (hiN = 0; hiN < k; hiN++)
        {
            t = res.Rows[r][2 * k - hiN - 1] * (long)Math.Sqrt(f.Length);

            if (t.GetBitLength() > bound)
                break;
        }

        // Now insert data into matrix
        if (loN > 0)
        {
            for (i = 0; i < r; i++)
            {
                zeroes = 0;
                while (liftedFac.P[i].Coeffs[zeroes].IsZero)
                    zeroes++;

                truncFac = FmpzPoly.AttachTruncate(liftedFac.P[i], loN + zeroes + 1);
                gd = FmpzPoly.Derivative(truncFac);
                gcld = FmpzPoly.MulLow(f, gd, loN + zeroes);
                res.Rows[i] = FmpzPoly.DivLowSmodp(gcld, truncFac, P, loN);
            }
        }

        if (hiN > 0)
        {
            truncF = FmpzPoly.AttachShift(f, f.Length - hiN);

            for (i = 0; i < r; i++)
            {
                long len = liftedFac.P[i].Length - hiN - 1;

                if (len < 0)
                {
                    temp = FmpzPoly.ShiftLeft(liftedFac.P[i], -len);
                    gd = FmpzPoly.Derivative(temp);
                    gcld = FmpzPoly.MulHighN(truncF, gd, hiN);
                    Array.Copy(FmpzPoly.DivHighSmodp(gcld, temp, P, hiN), 0, res.Rows[i], loN, hiN);
                }
                else
                {
                    truncFac = FmpzPoly.AttachShift(liftedFac.P[i], len);
                    gd = FmpzPoly.Derivative(truncFac);
                    gcld = FmpzPoly.MulHighN(truncF, gd, hiN);
                    Array.Copy(FmpzPoly.DivHighSmodp(gcld, truncFac, P, hiN), 0, res.Rows[i], loN, hiN);
                }
            }
        }

        if (hiN > 0)
        {
            // Move bounds into correct columns
            for (i = 0; i < hiN; i++)
                res.Rows[r][loN + i] = res.Rows[r][2 * k - hiN + i];
        }

        return loN + hiN;
    }
}

