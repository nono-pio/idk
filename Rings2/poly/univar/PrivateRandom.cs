using Org.Apache.Commons.Math3.Random;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Cc.Redberry.Rings.Poly.Univar.RoundingMode;
using static Cc.Redberry.Rings.Poly.Univar.Associativity;
using static Cc.Redberry.Rings.Poly.Univar.Operator;
using static Cc.Redberry.Rings.Poly.Univar.TokenType;
using static Cc.Redberry.Rings.Poly.Univar.SystemInfo;

namespace Cc.Redberry.Rings.Poly.Univar
{
    /// <summary>
    /// </summary>
    /// <remarks>@since1.0</remarks>
    sealed class PrivateRandom
    {
        private PrivateRandom()
        {
        }

        /// <summary>
        /// thread local instance of random
        /// </summary>
        private static readonly ThreadLocal<RandomGenerator> ThreadLocalRandom = ThreadLocal.WithInitial(() => new Well1024a(0x7f67fcad528cfae9));
        /// <summary>
        /// Returns random generator associated with current thread
        /// </summary>
        static RandomGenerator GetRandom()
        {
            return ThreadLocalRandom.Get();
        }
    }
}