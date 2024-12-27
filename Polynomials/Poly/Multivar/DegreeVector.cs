namespace Polynomials.Poly.Multivar;

public class DegreeVector
{

    public readonly int[] exponents;


    public readonly int totalDegree;


    public DegreeVector(int[] exponents, int totalDegree)
    {
        this.exponents = exponents;
        this.totalDegree = totalDegree; // assert ArraysUtil.sum(exponents) == totalDegree;
    }


    public DegreeVector(int[] exponents) : this(exponents, exponents.Sum())
    {
    }


    public int NVariables()
    {
        return exponents.Length;
    }


    public bool IsZeroVector()
    {
        return totalDegree == 0;
    }


    public virtual DegreeVector Dv()
    {
        return this;
    }


    public int DvTotalDegree(params int[] variables)
    {
        int d = 0;
        foreach (int v in variables)
            d += exponents[v];
        return d;
    }


    public DegreeVector DvMultiply(DegreeVector oth)
    {
        if (oth.IsZeroVector())
            return this;
        int[] res = new int[exponents.Length];
        for (int i = 0; i < exponents.Length; i++)
            res[i] = exponents[i] + oth.exponents[i];
        return new DegreeVector(res, totalDegree + oth.totalDegree);
    }


    public DegreeVector DvMultiply(int[] oth)
    {
        int deg = totalDegree;
        int[] res = new int[exponents.Length];
        for (int i = 0; i < exponents.Length; i++)
        {
            res[i] = exponents[i] + oth[i];
            deg += oth[i];
        }

        if (deg == 0)
            return this; // avoid copying
        return new DegreeVector(res, deg);
    }


    public DegreeVector DvMultiply(int variable, int exponent)
    {
        int[] res = (int[])exponents.Clone();
        res[variable] += exponent;
        if (res[variable] < 0)
            return null;
        return new DegreeVector(res, totalDegree + exponent);
    }


    public DegreeVector DvDivideOrNull(int variable, int exponent)
    {
        return DvMultiply(variable, -exponent);
    }


    public DegreeVector? DvDivideOrNull(DegreeVector divider)
    {
        if (divider.IsZeroVector())
            return this;
        int[] res = new int[exponents.Length];
        for (int i = 0; i < exponents.Length; i++)
        {
            res[i] = exponents[i] - divider.exponents[i];
            if (res[i] < 0)
                return null;
        }

        return new DegreeVector(res, totalDegree - divider.totalDegree);
    }


    public DegreeVector DvDivideOrNull(int[] divider)
    {
        int deg = totalDegree;
        int[] res = new int[exponents.Length];
        for (int i = 0; i < exponents.Length; i++)
        {
            res[i] = exponents[i] - divider[i];
            if (res[i] < 0)
                return null;
            deg -= divider[i];
        }

        if (deg == 0)
            return this; // avoid copying
        return new DegreeVector(res, deg);
    }


    public DegreeVector DvDivideExact(DegreeVector divider)
    {
        DegreeVector quot = DvDivideOrNull(divider);
        if (quot == null)
            throw new ArithmeticException("not divisible");
        return quot;
    }


    public DegreeVector DvDivideExact(int[] divider)
    {
        DegreeVector quot = DvDivideOrNull(divider);
        if (quot == null)
            throw new ArithmeticException("not divisible");
        return quot;
    }


    public bool DvDivisibleBy(int[] oth)
    {
        for (int i = 0; i < exponents.Length; i++)
            if (exponents[i] < oth[i])
                return false;
        return true;
    }


    public bool DvDivisibleBy(DegreeVector oth)
    {
        return DvDivisibleBy(oth.exponents);
    }


    public DegreeVector DvJoinNewVariable()
    {
        return DvJoinNewVariables(1);
    }


    public DegreeVector DvJoinNewVariables(int n)
    {
        var newDegs = new int[exponents.Length + n];
        Array.Copy(exponents, 0, newDegs, 0, exponents.Length);
        
        return new DegreeVector(newDegs, totalDegree);
    }


    public DegreeVector DvJoinNewVariables(int newNVariables, int[] mapping)
    {
        int[] res = new int[newNVariables];
        int c = 0;
        foreach (int i in mapping)
            res[i] = exponents[c++];
        return new DegreeVector(res, totalDegree);
    }


    public DegreeVector DvSetNVariables(int n)
    {
        if (n == exponents.Length)
            return this;
        if (n > exponents.Length)
        {
            var newExponents = new int[n];
            for (int i = 0; i < exponents.Length; i++)
                newExponents[i] = exponents[i];
            return new DegreeVector(newExponents, totalDegree);
        }
        else
            return new DegreeVector(exponents[..n]);
    }


    public DegreeVector DvSelect(int var)
    {
        int[] res = new int[exponents.Length];
        res[var] = exponents[var];
        return new DegreeVector(res, exponents[var]);
    }


    public DegreeVector DvSelect(int[] variables)
    {
        int[] res = new int[exponents.Length];
        int deg = 0;
        foreach (int i in variables)
        {
            res[i] = exponents[i];
            deg += exponents[i];
        }

        return new DegreeVector(res, deg);
    }


    public DegreeVector DvDropSelect(int[] variables)
    {
        int[] res = new int[variables.Length];
        int deg = 0;
        int c = 0;
        foreach (int i in variables)
        {
            res[c++] = exponents[i];
            deg += exponents[i];
        }

        return new DegreeVector(res, deg);
    }


    public DegreeVector DvRange(int from, int to)
    {
        if (from == 0 && to == exponents.Length)
        {
            return this;
        }

        return new DegreeVector(exponents[from..to]);
    }


    public DegreeVector DvSetZero(int var)
    {
        int[] res = (int[])exponents.Clone();
        res[var] = 0;
        return new DegreeVector(res, totalDegree - exponents[var]);
    }


    public DegreeVector DvSetZero(int[] variables)
    {
        int[] res = (int[])exponents.Clone();
        int deg = totalDegree;
        foreach (int i in variables)
        {
            deg -= exponents[i];
            res[i] = 0;
        }

        return new DegreeVector(res, deg);
    }


    public DegreeVector DvWithout(int variable)
    {
        var newDegs = new int[exponents.Length - 1];
        int c = 0;
        for (int i = 0; i < exponents.Length; i++)
            if (i != variable)
                newDegs[c++] = exponents[i];
        
        
        return new DegreeVector(newDegs, totalDegree - exponents[variable]);
    }


    public DegreeVector DvWithout(int[] variables)
    {
        var newDegs = new int[exponents.Length - variables.Length];
        int c = 0;
        for (int i = 0; i < exponents.Length; i++)
            if (!variables.Contains(i))
                newDegs[c++] = exponents[i];
        
        
        return new DegreeVector(newDegs);
    }


    public DegreeVector DvInsert(int variable)
    {
        int[] newDegs = new int[exponents.Length + 1];
        newDegs[variable] = 0;
        var c = 0;
        for (int i = 0; i < exponents.Length; i++)
            if (i != variable)
                newDegs[i] = exponents[c++];
        
        return new DegreeVector(newDegs, totalDegree);
    }


    public DegreeVector DvInsert(int variable, int count)
    {
        int[] newDegs = new int[exponents.Length + count];
        for (int i = variable; i < variable + count; i++)
            newDegs[i] = 0;
        var c = 0;
        for (int i = 0; i < exponents.Length; i++)
        {
            if (i == variable)
                c += count;
            newDegs[i + c] = exponents[i];
        }
        
        return new DegreeVector(newDegs, totalDegree);
    }


    public DegreeVector DvSet(int variable, int exponent)
    {
        if (exponents[variable] == exponent)
            return this;
        int deg = totalDegree - exponents[variable] + exponent;
        int[] res = (int[])exponents.Clone();
        res[variable] = exponent;
        return new DegreeVector(res, deg);
    }


    public DegreeVector DvMap(int nVariables, int[] mapping)
    {
        int[] newExponents = new int[nVariables];
        for (int i = 0; i < exponents.Length; ++i)
            newExponents[mapping[i]] = exponents[i];
        return new DegreeVector(newExponents, totalDegree);
    }


    public int FirstNonZeroVariable()
    {
        for (int i = 0; i < exponents.Length; ++i)
            if (exponents[i] != 0)
                return i;
        return -1;
    }


    private static string ToString0(string var, int exp)
    {
        return exp == 0 ? "" : var + (exp == 1 ? "" : "^" + exp);
    }


    public string ToString(string[] vars)
    {
        List<string> result = new List<string>();
        for (int i = 0; i < exponents.Length; i++)
            result.Add(ToString0(vars[i], exponents[i]));
        return string.Join('*', result.Where((s) => s.Length != 0));
    }


    public override string ToString()
    {
        return ToString(["x", "y", "z", "w"]);
    }


    public string ToStringArray()
    {
        return exponents.ToString();
    }


    public bool DvEquals(DegreeVector dVector)
    {
        return totalDegree == dVector.totalDegree && exponents.SequenceEqual(dVector.exponents);
    }

    public override bool Equals(object? o)
    {
        if (this == o)
            return true;
        if (o == null || GetType() != o.GetType())
            return false;
        DegreeVector dVector = (DegreeVector)o;
        return DvEquals(dVector);
    }


    public override int GetHashCode()
    {
        return exponents.GetHashCode();
    }
}