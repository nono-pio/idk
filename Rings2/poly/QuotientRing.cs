using System.Numerics;
using Cc.Redberry.Rings.Io;
using Cc.Redberry.Rings.Poly.Multivar;

namespace Cc.Redberry.Rings.Poly
{
    public class QuotientRing<Term, Poly> : ARing<Poly>, IPolynomialRing<Poly>
        where Poly : AMultivariatePolynomial<Term, Poly> where Term : AMonomial<Term>
    {
        private static readonly long serialVersionUID = 1;


        public readonly MultivariateRing<Poly> baseRing;


        public readonly Ideal<Term, Poly> ideal;


        private readonly Poly factory;


        public QuotientRing(MultivariateRing<Poly> baseRing, Ideal<Term, Poly> ideal)
        {
            this.baseRing = baseRing;
            this.ideal = ideal;
            this.factory = ideal.GetBasisGenerator(0).CreateZero();
        }


        public override int NVariables()
        {
            return factory.nVariables;
        }


        public override Poly Factory()
        {
            return factory;
        }


        public override Poly Variable(int variable)
        {
            return factory.CreateMonomial(variable, 1);
        }


        public override bool IsField()
        {
            return factory.IsOverField() && ideal.IsMaximal();
        }


        public override bool IsEuclideanRing()
        {
            throw new NotSupportedException("Algebraic structure of ring is unknown");
        }


        public override BigInteger? Cardinality()
        {
            return factory.CoefficientRingCardinality().IsZero() ? BigInteger.Zero :
                ideal.Dimension() != 0 ? null :
                new BigInteger(ideal.Degree()).Multiply(factory.CoefficientRingCardinality());
        }


        public override BigInteger Characteristic()
        {
            return factory.CoefficientRingCharacteristic();
        }


        public virtual Poly Mod(Poly el)
        {
            return ideal.NormalForm(el);
        }


        public virtual Poly NormalForm(Poly el)
        {
            return Mod(el);
        }


        public override Poly Add(Poly a, Poly b)
        {
            return Mod(baseRing.Add(a, b));
        }


        public override Poly Subtract(Poly a, Poly b)
        {
            return Mod(baseRing.Subtract(a, b));
        }


        public override Poly Multiply(Poly a, Poly b)
        {
            return Mod(baseRing.Multiply(a, b));
        }


        public override Poly Negate(Poly element)
        {
            return Mod(baseRing.Negate(element));
        }


        public override Poly Copy(Poly element)
        {
            return baseRing.Copy(element);
        }


        public override Poly[] DivideAndRemainder(Poly dividend, Poly divider)
        {
            if (baseRing.IsUnit(divider))
                return [Multiply(dividend, baseRing.Reciprocal(divider)), GetZero()];
            if (IsField())
                return [Multiply(dividend, Reciprocal(divider)), GetZero()];
            throw new NotSupportedException("Algebraic structure of ring is unknown");
        }


        public override Poly Reciprocal(Poly element)
        {
            if (IsOne(element))
                return element;
            if (IsMinusOne(element))
                return element;
            if (baseRing.IsUnit(element))
                return ValueOf(baseRing.Reciprocal(element));
            if (IsField())
            {
                if (!element.IsConstant())
                    element = Mod(element);
                if (!element.IsConstant())
                    throw new NotSupportedException("Algebraic structure of ring is unknown");
                return baseRing.GetOne().DivideByLC(element);
            }

            throw new NotSupportedException("Algebraic structure of ring is unknown");
        }


        public override Poly GetZero()
        {
            return baseRing.GetZero();
        }


        public override Poly GetOne()
        {
            return baseRing.GetOne();
        }


        public override bool IsZero(Poly element)
        {
            return baseRing.IsZero(element);
        }


        public override bool IsOne(Poly element)
        {
            return baseRing.IsOne(element);
        }


        public override bool IsUnit(Poly element)
        {
            return baseRing.IsUnit(element);
        }


        public override Poly ValueOf(long val)
        {
            return Mod(baseRing.ValueOf(val));
        }


        public override Poly ValueOfBigInteger(BigInteger val)
        {
            return Mod(baseRing.ValueOfBigInteger(val));
        }


        public override Poly ValueOf(Poly val)
        {
            return Mod(baseRing.ValueOf(val));
        }


        public override IEnumerator<Poly> Iterator()
        {
            throw new NotSupportedException("Algebraic structure of ring is unknown");
        }


        public override int Compare(Poly? o1, Poly? o2)
        {
            return baseRing.Compare(o1, o2);
        }


        public override Poly Parse(string @string)
        {
            return ValueOf(baseRing.Parse(@string));
        }


        public override Poly RandomElement(Random rnd)
        {
            return ValueOf(baseRing.RandomElement(rnd));
        }


        public override Poly RandomElementTree(Random rnd)
        {
            return ValueOf(baseRing.RandomElementTree(rnd));
        }


        public override string ToString(IStringifier<Poly> stringifier)
        {
            return baseRing.ToString(stringifier) + "/<" + ideal.ToString(stringifier) + ">";
        }


        public override string ToString()
        {
            return ToString(IStringifier<Poly>.Dummy<Poly>());
        }
    }
}