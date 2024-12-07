using System.Numerics;
using Cc.Redberry.Rings.Io;
using System.Text;


namespace Cc.Redberry.Rings.Poly
{
    /// <summary>
    /// </summary>
    /// <remarks>@since1.0</remarks>
    public abstract class APolynomialRing<Poly> : ARing<Poly>, IPolynomialRing<Poly> where Poly : IPolynomial<Poly>
    {
        private static readonly long serialVersionUID = 1;
        /// <summary>
        /// the factory polynomial
        /// </summary>
        readonly Poly factory;
        /// <summary>
        /// the factory polynomial
        /// </summary>
        public APolynomialRing(Poly factory)
        {
            this.factory = factory.CreateZero();
        }

        public abstract int NVariables();

        /// <summary>
        /// the factory polynomial
        /// </summary>
        public override Poly Factory()
        {
            return factory;
        }

        public abstract Poly Variable(int variable);

        /// <summary>
        /// the factory polynomial
        /// </summary>
        public override bool IsEuclideanRing()
        {
            return factory.IsOverField();
        }

        /// <summary>
        /// the factory polynomial
        /// </summary>
        public override bool IsField()
        {
            return false;
        }

        /// <summary>
        /// the factory polynomial
        /// </summary>
        public override BigInteger Cardinality()
        {
            return null;
        }

        /// <summary>
        /// the factory polynomial
        /// </summary>
        public override BigInteger Characteristic()
        {
            return factory.CoefficientRingCharacteristic();
        }

        /// <summary>
        /// the factory polynomial
        /// </summary>
        public override Poly Add(Poly a, Poly b)
        {
            return a.Clone().Add(b);
        }

        /// <summary>
        /// the factory polynomial
        /// </summary>
        public override Poly Subtract(Poly a, Poly b)
        {
            return a.Clone().Subtract(b);
        }

        /// <summary>
        /// the factory polynomial
        /// </summary>
        public override Poly Multiply(Poly a, Poly b)
        {
            return a.Clone().Multiply(b);
        }

        /// <summary>
        /// the factory polynomial
        /// </summary>
        public override Poly Negate(Poly element)
        {
            return element.Clone().Negate();
        }

        /// <summary>
        /// the factory polynomial
        /// </summary>
        public Poly Pow(Poly @base, BigInteger exponent)
        {
            return PolynomialMethods.PolyPow(@base, exponent, true);
        }

        /// <summary>
        /// the factory polynomial
        /// </summary>
        public Poly AddMutable(Poly a, Poly b)
        {
            return a.Add(b);
        }

        /// <summary>
        /// the factory polynomial
        /// </summary>
        public Poly SubtractMutable(Poly a, Poly b)
        {
            return a.Subtract(b);
        }

        /// <summary>
        /// the factory polynomial
        /// </summary>
        public Poly MultiplyMutable(Poly a, Poly b)
        {
            return a.Multiply(b);
        }

        /// <summary>
        /// the factory polynomial
        /// </summary>
        public Poly NegateMutable(Poly element)
        {
            return element.Negate();
        }

        /// <summary>
        /// the factory polynomial
        /// </summary>
        public override Poly Reciprocal(Poly element)
        {
            if (element.IsConstant())
                return DivideExact(GetOne(), element);
            throw new ArithmeticException("not divisible: 1 / " + element);
        }

        /// <summary>
        /// the factory polynomial
        /// </summary>
        public override Poly GetZero()
        {
            return factory.CreateZero();
        }

        /// <summary>
        /// the factory polynomial
        /// </summary>
        public override Poly GetOne()
        {
            return factory.CreateOne();
        }

        /// <summary>
        /// the factory polynomial
        /// </summary>
        public override bool IsZero(Poly element)
        {
            return element.IsZero();
        }

        /// <summary>
        /// the factory polynomial
        /// </summary>
        public override bool IsOne(Poly element)
        {
            return element.IsOne();
        }

        /// <summary>
        /// the factory polynomial
        /// </summary>
        public override bool IsUnit(Poly element)
        {
            return element.IsOverField() ? element.IsConstant() : (IsOne(element) || IsMinusOne(element));
        }

        /// <summary>
        /// the factory polynomial
        /// </summary>
        public override Poly ValueOf(long val)
        {
            return factory.CreateOne().Multiply(val);
        }

        /// <summary>
        /// the factory polynomial
        /// </summary>
        public override Poly ValueOfBigInteger(BigInteger val)
        {
            return factory.CreateOne().MultiplyByBigInteger(val);
        }

        /// <summary>
        /// the factory polynomial
        /// </summary>
        public override Poly ValueOf(Poly val)
        {
            if (factory.SameCoefficientRingWith(val))
                return val;
            else
                return val.SetCoefficientRingFrom(factory);
        }

        /// <summary>
        /// the factory polynomial
        /// </summary>
        public override Poly Copy(Poly element)
        {
            return element.Clone();
        }

        /// <summary>
        /// the factory polynomial
        /// </summary>
        public override int Compare(Poly o1, Poly o2)
        {
            return o1.CompareTo(o2);
        }

        /// <summary>
        /// the factory polynomial
        /// </summary>
        public override bool Equals(object o)
        {
            if (this == o)
                return true;
            if (o == null || GetType() != o.GetType())
                return false;
            APolynomialRing<TWildcardTodo> that = (APolynomialRing<TWildcardTodo>)o;
            return that.factory.GetType().Equals(factory.GetType()) && factory.SameCoefficientRingWith((Poly)that.factory);
        }

        /// <summary>
        /// the factory polynomial
        /// </summary>
        public override int GetHashCode()
        {
            return GetOne().GetHashCode();
        }

        /// <summary>
        /// the factory polynomial
        /// </summary>
        public override IEnumerator<Poly> Iterator()
        {
            throw new NotSupportedException("Ring of infinite cardinality.");
        }

        /// <summary>
        /// the factory polynomial
        /// </summary>
        public override Poly Parse(string @string)
        {
            return factory.ParsePoly(@string);
        }

        /// <summary>
        /// the factory polynomial
        /// </summary>
        public string ToString(IStringifier<Poly> stringifier)
        {
            string cfRing = factory.CoefficientRingToString(stringifier);
            if (cfRing.Length > 2)
                cfRing = "(" + cfRing + ")";
            StringBuilder sb = new StringBuilder();
            sb.Append(cfRing);
            sb.Append("[");
            int nVars = NVariables();
            for (int i = 0;; ++i)
            {
                sb.Append(stringifier.GetBinding(Variable(i), IStringifier.DefaultVar(i, nVars)));
                if (i == nVars - 1)
                    break;
                sb.Append(", ");
            }

            sb.Append("]");
            return sb.ToString();
        }

        /// <summary>
        /// the factory polynomial
        /// </summary>
        public virtual string ToString(params string[] variables)
        {
            return ToString(IStringifier.MkPolyStringifier(factory, variables));
        }

        /// <summary>
        /// the factory polynomial
        /// </summary>
        public override string ToString()
        {
            return ToString(IStringifier.DefaultVars(NVariables()));
        }

    }
}