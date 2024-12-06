using Cc.Redberry.Rings;
using Cc.Redberry.Rings.Bigint;
using Cc.Redberry.Rings.Io;
using Cc.Redberry.Rings.Poly.Multivar;
using Org.Apache.Commons.Math3.Random;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Cc.Redberry.Rings.Poly.RoundingMode;
using static Cc.Redberry.Rings.Poly.Associativity;
using static Cc.Redberry.Rings.Poly.Operator;
using static Cc.Redberry.Rings.Poly.TokenType;
using static Cc.Redberry.Rings.Poly.SystemInfo;

namespace Cc.Redberry.Rings.Poly
{
    /// <summary>
    /// Multivariate quotient ring
    /// </summary>
    public class QuotientRing<Term, Poly> : ARing<Poly>, IPolynomialRing<Poly>
    {
        private static readonly long serialVersionUID = 1;
        /// <summary>
        /// the base ring
        /// </summary>
        public readonly MultivariateRing<Poly> baseRing;
        /// <summary>
        /// the base ring
        /// </summary>
        /// <summary>
        /// the ideal
        /// </summary>
        public readonly Ideal<Term, Poly> ideal;
        /// <summary>
        /// the base ring
        /// </summary>
        /// <summary>
        /// the ideal
        /// </summary>
        /// <summary>
        /// factory element
        /// </summary>
        private readonly Poly factory;
        /// <summary>
        /// the base ring
        /// </summary>
        /// <summary>
        /// the ideal
        /// </summary>
        /// <summary>
        /// factory element
        /// </summary>
        public QuotientRing(MultivariateRing<Poly> baseRing, Ideal<Term, Poly> ideal)
        {
            this.baseRing = baseRing;
            this.ideal = ideal;
            this.factory = ideal.GetBasisGenerator(0).CreateZero();
        }

        /// <summary>
        /// the base ring
        /// </summary>
        /// <summary>
        /// the ideal
        /// </summary>
        /// <summary>
        /// factory element
        /// </summary>
        public override int NVariables()
        {
            return factory.nVariables;
        }

        /// <summary>
        /// the base ring
        /// </summary>
        /// <summary>
        /// the ideal
        /// </summary>
        /// <summary>
        /// factory element
        /// </summary>
        public override Poly Factory()
        {
            return factory;
        }

        /// <summary>
        /// the base ring
        /// </summary>
        /// <summary>
        /// the ideal
        /// </summary>
        /// <summary>
        /// factory element
        /// </summary>
        public override Poly Variable(int variable)
        {
            return factory.CreateMonomial(variable, 1);
        }

        /// <summary>
        /// the base ring
        /// </summary>
        /// <summary>
        /// the ideal
        /// </summary>
        /// <summary>
        /// factory element
        /// </summary>
        public override bool IsField()
        {
            return factory.IsOverField() && ideal.IsMaximal();
        }

        /// <summary>
        /// the base ring
        /// </summary>
        /// <summary>
        /// the ideal
        /// </summary>
        /// <summary>
        /// factory element
        /// </summary>
        public override bool IsEuclideanRing()
        {
            throw new NotSupportedException("Algebraic structure of ring is unknown");
        }

        /// <summary>
        /// the base ring
        /// </summary>
        /// <summary>
        /// the ideal
        /// </summary>
        /// <summary>
        /// factory element
        /// </summary>
        public override BigInteger Cardinality()
        {
            return factory.CoefficientRingCardinality().IsZero() ? BigInteger.ZERO : ideal.Dimension() != 0 ? null : BigInteger.ValueOf(ideal.Degree()).Multiply(factory.CoefficientRingCardinality());
        }

        /// <summary>
        /// the base ring
        /// </summary>
        /// <summary>
        /// the ideal
        /// </summary>
        /// <summary>
        /// factory element
        /// </summary>
        public override BigInteger Characteristic()
        {
            return factory.CoefficientRingCharacteristic();
        }

        /// <summary>
        /// the base ring
        /// </summary>
        /// <summary>
        /// the ideal
        /// </summary>
        /// <summary>
        /// factory element
        /// </summary>
        public virtual Poly Mod(Poly el)
        {
            return ideal.NormalForm(el);
        }

        /// <summary>
        /// the base ring
        /// </summary>
        /// <summary>
        /// the ideal
        /// </summary>
        /// <summary>
        /// factory element
        /// </summary>
        public virtual Poly NormalForm(Poly el)
        {
            return Mod(el);
        }

        /// <summary>
        /// the base ring
        /// </summary>
        /// <summary>
        /// the ideal
        /// </summary>
        /// <summary>
        /// factory element
        /// </summary>
        public override Poly Add(Poly a, Poly b)
        {
            return Mod(baseRing.Add(a, b));
        }

        /// <summary>
        /// the base ring
        /// </summary>
        /// <summary>
        /// the ideal
        /// </summary>
        /// <summary>
        /// factory element
        /// </summary>
        public override Poly Subtract(Poly a, Poly b)
        {
            return Mod(baseRing.Subtract(a, b));
        }

        /// <summary>
        /// the base ring
        /// </summary>
        /// <summary>
        /// the ideal
        /// </summary>
        /// <summary>
        /// factory element
        /// </summary>
        public override Poly Multiply(Poly a, Poly b)
        {
            return Mod(baseRing.Multiply(a, b));
        }

        /// <summary>
        /// the base ring
        /// </summary>
        /// <summary>
        /// the ideal
        /// </summary>
        /// <summary>
        /// factory element
        /// </summary>
        public override Poly Negate(Poly element)
        {
            return Mod(baseRing.Negate(element));
        }

        /// <summary>
        /// the base ring
        /// </summary>
        /// <summary>
        /// the ideal
        /// </summary>
        /// <summary>
        /// factory element
        /// </summary>
        public override Poly Copy(Poly element)
        {
            return baseRing.Copy(element);
        }

        /// <summary>
        /// the base ring
        /// </summary>
        /// <summary>
        /// the ideal
        /// </summary>
        /// <summary>
        /// factory element
        /// </summary>
        public override Poly[] DivideAndRemainder(Poly dividend, Poly divider)
        {
            if (baseRing.IsUnit(divider))
                return CreateArray(Multiply(dividend, baseRing.Reciprocal(divider)), GetZero());
            if (IsField())
                return CreateArray(Multiply(dividend, Reciprocal(divider)), GetZero());
            throw new NotSupportedException("Algebraic structure of ring is unknown");
        }

        /// <summary>
        /// the base ring
        /// </summary>
        /// <summary>
        /// the ideal
        /// </summary>
        /// <summary>
        /// factory element
        /// </summary>
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

        /// <summary>
        /// the base ring
        /// </summary>
        /// <summary>
        /// the ideal
        /// </summary>
        /// <summary>
        /// factory element
        /// </summary>
        public override Poly GetZero()
        {
            return baseRing.GetZero();
        }

        /// <summary>
        /// the base ring
        /// </summary>
        /// <summary>
        /// the ideal
        /// </summary>
        /// <summary>
        /// factory element
        /// </summary>
        public override Poly GetOne()
        {
            return baseRing.GetOne();
        }

        /// <summary>
        /// the base ring
        /// </summary>
        /// <summary>
        /// the ideal
        /// </summary>
        /// <summary>
        /// factory element
        /// </summary>
        public override bool IsZero(Poly element)
        {
            return baseRing.IsZero(element);
        }

        /// <summary>
        /// the base ring
        /// </summary>
        /// <summary>
        /// the ideal
        /// </summary>
        /// <summary>
        /// factory element
        /// </summary>
        public override bool IsOne(Poly element)
        {
            return baseRing.IsOne(element);
        }

        /// <summary>
        /// the base ring
        /// </summary>
        /// <summary>
        /// the ideal
        /// </summary>
        /// <summary>
        /// factory element
        /// </summary>
        public override bool IsUnit(Poly element)
        {
            return baseRing.IsUnit(element);
        }

        /// <summary>
        /// the base ring
        /// </summary>
        /// <summary>
        /// the ideal
        /// </summary>
        /// <summary>
        /// factory element
        /// </summary>
        public override Poly ValueOf(long val)
        {
            return Mod(baseRing.ValueOf(val));
        }

        /// <summary>
        /// the base ring
        /// </summary>
        /// <summary>
        /// the ideal
        /// </summary>
        /// <summary>
        /// factory element
        /// </summary>
        public override Poly ValueOfBigInteger(BigInteger val)
        {
            return Mod(baseRing.ValueOfBigInteger(val));
        }

        /// <summary>
        /// the base ring
        /// </summary>
        /// <summary>
        /// the ideal
        /// </summary>
        /// <summary>
        /// factory element
        /// </summary>
        public override Poly ValueOf(Poly val)
        {
            return Mod(baseRing.ValueOf(val));
        }

        /// <summary>
        /// the base ring
        /// </summary>
        /// <summary>
        /// the ideal
        /// </summary>
        /// <summary>
        /// factory element
        /// </summary>
        public override IEnumerator<Poly> Iterator()
        {
            throw new NotSupportedException("Algebraic structure of ring is unknown");
        }

        /// <summary>
        /// the base ring
        /// </summary>
        /// <summary>
        /// the ideal
        /// </summary>
        /// <summary>
        /// factory element
        /// </summary>
        public override int Compare(Poly o1, Poly o2)
        {
            return baseRing.Compare(o1, o2);
        }

        /// <summary>
        /// the base ring
        /// </summary>
        /// <summary>
        /// the ideal
        /// </summary>
        /// <summary>
        /// factory element
        /// </summary>
        public override Poly Parse(string @string)
        {
            return ValueOf(baseRing.Parse(@string));
        }

        /// <summary>
        /// the base ring
        /// </summary>
        /// <summary>
        /// the ideal
        /// </summary>
        /// <summary>
        /// factory element
        /// </summary>
        public override Poly RandomElement(RandomGenerator rnd)
        {
            return ValueOf(baseRing.RandomElement(rnd));
        }

        /// <summary>
        /// the base ring
        /// </summary>
        /// <summary>
        /// the ideal
        /// </summary>
        /// <summary>
        /// factory element
        /// </summary>
        public override Poly RandomElementTree(RandomGenerator rnd)
        {
            return ValueOf(baseRing.RandomElementTree(rnd));
        }

        /// <summary>
        /// the base ring
        /// </summary>
        /// <summary>
        /// the ideal
        /// </summary>
        /// <summary>
        /// factory element
        /// </summary>
        public override string ToString(IStringifier<Poly> stringifier)
        {
            return baseRing.ToString(stringifier) + "/<" + ideal.ToString(stringifier) + ">";
        }

        /// <summary>
        /// the base ring
        /// </summary>
        /// <summary>
        /// the ideal
        /// </summary>
        /// <summary>
        /// factory element
        /// </summary>
        public override string ToString()
        {
            return ToString(IStringifier.Dummy());
        }
    }
}