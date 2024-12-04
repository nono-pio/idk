using System;
using System.Collections.Generic;
using System.Numerics;
using Rings;
using Rings.io;
using Rings.poly;

public abstract class APolynomialRing<Poly> : ARing<Poly>, IPolynomialRing<Poly> where Poly : IPolynomial<Poly>
{
    private static readonly long serialVersionUID = 1L;

    protected readonly Poly Factory;

    protected APolynomialRing(Poly factory)
    {           
        this.Factory = factory.createZero();
    }

    public Poly factory() => Factory;

    public new bool isEuclideanRing() => Factory.isOverField();

    public new bool isField() => false;

    public new BigInteger cardinality() => null;

    public new BigInteger characteristic() => Factory.CoefficientRingCharacteristic();

    public override Poly add(Poly a, Poly b) => a.clone().add(b);

    public override Poly subtract(Poly a, Poly b) => a.clone().subtract(b);

    public override Poly multiply(Poly a, Poly b) => a.clone().multiply(b);

    public override Poly negate(Poly element) => element.clone().negate();

    public Poly pow(Poly @base, BigInteger exponent)
    {
        return PolynomialMethods.PolyPow(@base, exponent, true);
    }

    public Poly addMutable(Poly a, Poly b) => a.add(b);

    public Poly subtractMutable(Poly a, Poly b) => a.subtract(b);

    public Poly multiplyMutable(Poly a, Poly b) => a.multiply(b);

    public Poly negateMutable(Poly element) => element.negate();

    public override Poly reciprocal(Poly element)
    {
        if (element.isConstant())
            return divideExact(getOne(), element);
        throw new ArithmeticException($"not divisible: 1 / {element}");
    }

    public override Poly getZero() => Factory.createZero();

    public override Poly getOne() => Factory.createOne();

    public override bool isZero(Poly element) => element.isZero();

    public override bool isOne(Poly element) => element.isOne();

    public override bool isUnit(Poly element)
    {
        return element.isOverField() ? element.isConstant() : (isOne(element) || isMinusOne(element));
    }

    public override Poly valueOf(long val) => Factory.createOne().multiply(val);

    public override Poly valueOfBigInteger(BigInteger val)
    {
        return Factory.createOne().multiplyByBigInteger(val);
    }

    public override Poly valueOf(Poly val)
    {
        if (Factory.SameCoefficientRingWith(val))
            return val;
        else
            return val.SetCoefficientRingFrom(Factory);
    }

    public override Poly copy(Poly element) => element.clone();

    public override int compare(Poly o1, Poly o2) => o1.compare(o2);

    public override bool Equals(object o)
    {
        if (this == o) return true;
        if (o == null || GetType() != o.GetType()) return false;

        var that = (APolynomialRing<Poly>)o;
        return that.Factory.GetType().Equals(Factory.GetType()) && Factory.SameCoefficientRingWith(that.factory);
    }

    public override int GetHashCode() => getOne().GetHashCode();

    public override IEnumerable<Poly> iterator()
    {
        throw new NotSupportedException("Ring of infinite cardinality.");
    }

    public Poly parse(string s) => Factory.ParsePoly(s);

    public string toString(IStringifier<Poly> stringifier)
    {
        string cfRing = Factory.CoefficientRingToString(stringifier);
        if (cfRing.Length > 2)
            cfRing = $"({cfRing})";
        var sb = new System.Text.StringBuilder();
        sb.Append(cfRing);
        sb.Append("[");
        int nVars = NVariables();
        for (int i = 0; ; ++i)
        {
            sb.Append(stringifier.GetBinding(Variable(i), IStringifier.DefaultVar(i, nVars)));
            if (i == nVars - 1)
                break;
            sb.Append(", ");
        }
        sb.Append("]");
        return sb.ToString();
    }

    public string toString(params string[] variables)
    {
        return ToString(IStringifier.MkPolyStringifier(Factory, variables));
    }

    public string toString()
    {
        return ToString(IStringifier.DefaultVars(NVariables()));
    }

    public abstract Poly variable(int variable);
    public abstract int nVariables();
}

