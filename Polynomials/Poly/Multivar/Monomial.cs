namespace Polynomials.Poly.Multivar;

public class Monomial<E> : DegreeVector
{
    public readonly E coefficient;


    public Monomial(DegreeVector degreeVector, E coefficient) : base(degreeVector.exponents, degreeVector.totalDegree)
    {
        this.coefficient = coefficient;
    }


    public Monomial(int[] exponents, int totalDegree, E coefficient) : base(exponents, totalDegree)
    {
        this.coefficient = coefficient;
    }


    public Monomial(int[] exponents, E coefficient) : base(exponents, exponents.Sum())
    {
        this.coefficient = coefficient;
    }


    public Monomial(int nVariables, E coefficient) : this(new int[nVariables], 0, coefficient)
    {
    }


    public override DegreeVector Dv()
    {
        return new DegreeVector(exponents, totalDegree);
    }
    


    public Monomial<E> SetDegreeVector(int[] newExponents)
    {
        return SetDegreeVector(newExponents, newExponents.Sum());
    }


    public Monomial<E> Multiply(DegreeVector oth)
    {
        return SetDegreeVector(DvMultiply(oth));
    }


    public Monomial<E> Multiply(int[] oth)
    {
        return SetDegreeVector(DvMultiply(oth));
    }


    public Monomial<E> DivideOrNull(DegreeVector divider)
    {
        return SetDegreeVector(DvDivideOrNull(divider));
    }


    public Monomial<E> DivideOrNull(int[] divider)
    {
        return SetDegreeVector(DvDivideOrNull(divider));
    }


    public Monomial<E> JoinNewVariable()
    {
        return SetDegreeVector(DvJoinNewVariable());
    }


    public Monomial<E> JoinNewVariables(int n)
    {
        return SetDegreeVector(DvJoinNewVariables(n));
    }


    public Monomial<E> JoinNewVariables(int newNVariables, int[] mapping)
    {
        return SetDegreeVector(DvJoinNewVariables(newNVariables, mapping));
    }


    public Monomial<E> SetNVariables(int n)
    {
        return SetDegreeVector(DvSetNVariables(n));
    }


    public Monomial<E> Select(int var)
    {
        return SetDegreeVector(DvSelect(var));
    }


    public Monomial<E> Select(int[] variables)
    {
        return SetDegreeVector(DvSelect(variables));
    }


    public Monomial<E> DropSelect(int[] variables)
    {
        return SetDegreeVector(DvDropSelect(variables));
    }


    public Monomial<E> Range(int from, int to)
    {
        return SetDegreeVector(DvRange(from, to));
    }


    public Monomial<E> SetZero(int var)
    {
        return SetDegreeVector(DvSetZero(var));
    }


    public Monomial<E> ToZero()
    {
        if (IsZeroVector())
            return this;
        return SetDegreeVector(new DegreeVector(new int[NVariables()], 0));
    }


    public Monomial<E> SetZero(int[] variables)
    {
        return SetDegreeVector(DvSetZero(variables));
    }


    public Monomial<E> Without(int variable)
    {
        return SetDegreeVector(DvWithout(variable));
    }


    public Monomial<E> Without(int[] variables)
    {
        return SetDegreeVector(DvWithout(variables));
    }


    public Monomial<E> Insert(int variable)
    {
        return SetDegreeVector(DvInsert(variable));
    }


    public Monomial<E> Insert(int variable, int count)
    {
        return SetDegreeVector(DvInsert(variable, count));
    }


    public Monomial<E> Map(int nVariables, int[] mapping)
    {
        return SetDegreeVector(DvMap(nVariables, mapping));
    }


    public Monomial<E> Set(int variable, int exponent)
    {
        return SetDegreeVector(DvSet(variable, exponent));
    }


    public string DvToString(string[] vars)
    {
        return base.ToString(vars);
    }


    public string DvToString()
    {
        return base.ToString();
    }


    public Monomial<E> SetCoefficientFrom(Monomial<E> oth)
    {
        return new Monomial<E>(this, oth.coefficient);
    }


    public Monomial<E> SetDegreeVector(DegreeVector oth)
    {
        if (Equals(this, oth))
            return this;
        if (oth == null)
            return null;
        if (oth.exponents == exponents)
            return this;
        return new Monomial<E>(oth, coefficient);
    }


    public Monomial<E> SetDegreeVector(int[] exponents, int totalDegree)
    {
        if (this.exponents == exponents)
            return this;
        return new Monomial<E>(exponents, totalDegree, coefficient);
    }


    public Monomial<E> ForceSetDegreeVector(int[] exponents, int totalDegree)
    {
        return new Monomial<E>(exponents, totalDegree, coefficient);
    }


    public virtual Monomial<E> SetCoefficient(E c)
    {
        return new Monomial<E>(exponents, totalDegree, c);
    }


    public override bool Equals(object? o)
    {
        if (this == o)
            return true;
        if (o == null || GetType() != o.GetType())
            return false;
        if (!base.Equals(o))
            return false;
        Monomial<E> monomial = (Monomial<E>)o;
        return coefficient.Equals(monomial.coefficient);
    }


    public override int GetHashCode()
    {
        int result = base.GetHashCode();
        result = 31 * result + coefficient.GetHashCode();
        return result;
    }


    public override string ToString()
    {
        string dvString = base.ToString();
        string cfString = coefficient.ToString();
        if (dvString.Length == 0)
            return cfString;
        if (cfString.Equals("1"))
            return dvString;
        return coefficient + "*" + dvString;
    }
}