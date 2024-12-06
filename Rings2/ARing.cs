using Cc.Redberry.Rings.Bigint;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Cc.Redberry.Rings
{
    /// <summary>
    /// Abstract ring which holds perfect power decomposition of its cardinality.
    /// </summary>
    /// <remarks>@since1.0</remarks>
    public abstract class ARing<E> : Ring<E>
    {
        private static readonly long serialVersionUID = 1;
        /// <summary>
        /// if modulus = a^b, a and b are stored in this array
        /// </summary>
        private readonly BigInteger?[] perfectPowerDecomposition = new BigInteger?[2];

        private volatile bool initialized = false;
        private Ring<E> _ringImplementation;

        private void CheckPerfectPower()
        {

            // lazy initialization
            if (!initialized)
            {
                lock (perfectPowerDecomposition)
                {
                    if (initialized)
                        return;
                    initialized = true;
                    if (Cardinality() == null)
                    {
                        perfectPowerDecomposition[0] = null;
                        perfectPowerDecomposition[1] = null;
                        return;
                    }

                    BigInteger[] ipp = BigIntegerUtil.PerfectPowerDecomposition(Cardinality());
                    if (ipp == null)
                    {

                        // not a perfect power
                        perfectPowerDecomposition[0] = Cardinality();
                        perfectPowerDecomposition[1] = BigInteger.One;
                        return;
                    }

                    perfectPowerDecomposition[0] = ipp[0];
                    perfectPowerDecomposition[1] = ipp[1];
                }
            }
        }

        public abstract bool IsField();
        public abstract bool IsEuclideanRing();

        public abstract BigInteger Cardinality();
        public abstract BigInteger Characteristic();

        /// <summary>
        /// if modulus = a^b, a and b are stored in this array
        /// </summary>
        // lazy initialization
        // not a perfect power
        public virtual bool IsPerfectPower()
        {
            CheckPerfectPower();
            return perfectPowerDecomposition[1] is not null && !perfectPowerDecomposition[1].Value.IsOne;
        }

        BigInteger Ring<E>.PerfectPowerBase()
        {
            return PerfectPowerBase();
        }

        BigInteger Ring<E>.PerfectPowerExponent()
        {
            return PerfectPowerExponent();
        }

        public abstract E Add(E a, E b);
        public abstract E Subtract(E a, E b);
        public abstract E Multiply(E a, E b);
        public abstract E Negate(E element);
        public abstract E Copy(E element);
        public abstract E[] DivideAndRemainder(E dividend, E divider);
        public abstract E Reciprocal(E element);
        public abstract E GetZero();
        public abstract E GetOne();
        public abstract bool IsZero(E element);
        public abstract bool IsOne(E element);
        public abstract bool IsUnit(E element);
        public abstract E ValueOf(long val);
        public abstract E ValueOf(E val);
        public abstract E ValueOfBigInteger(BigInteger val);

        /// <summary>
        /// if modulus = a^b, a and b are stored in this array
        /// </summary>
        // lazy initialization
        // not a perfect power
        public virtual BigInteger PerfectPowerBase()
        {
            CheckPerfectPower();
            return perfectPowerDecomposition[0].Value;
        }

        /// <summary>
        /// if modulus = a^b, a and b are stored in this array
        /// </summary>
        // lazy initialization
        // not a perfect power
        public virtual BigInteger PerfectPowerExponent()
        {
            CheckPerfectPower();
            return perfectPowerDecomposition[1].Value;
        }

        public abstract IEnumerator<E> Iterator();
        public abstract E Parse(string @string);
        public IEnumerator<E> GetEnumerator() => Iterator();
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract int Compare(E? x, E? y);
    }
}