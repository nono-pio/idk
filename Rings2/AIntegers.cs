using Cc.Redberry.Rings.Bigint;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Cc.Redberry.Rings
{
    /// <summary>
    /// </summary>
    /// <remarks>@since1.0</remarks>
    public abstract class AIntegers : ARing<BigInteger>
    {
        public static readonly long serialVersionUID = 1;
        public override BigInteger GetZero()
        {
            return BigInteger.Zero;
        }

        public override BigInteger GetOne()
        {
            return BigInteger.One;
        }

        public override bool IsZero(BigInteger element)
        {
            return element.IsZero;
        }

        public override bool IsOne(BigInteger element)
        {
            return element.IsOne;
        }

        public override BigInteger Parse(string @string)
        {
            return ValueOf(BigInteger.Parse(@string.Trim()));
        }

        public override int Compare(BigInteger o1, BigInteger o2)
        {
            return o1.CompareTo(o2);
        }

        public override BigInteger ValueOfBigInteger(BigInteger val)
        {
            return ValueOf(val);
        }

        public override BigInteger Copy(BigInteger element)
        {
            return element;
        }
    }
}