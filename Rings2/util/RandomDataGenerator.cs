using Org.Apache.Commons.Math3.Exception;
using Org.Apache.Commons.Math3.Random;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using static Cc.Redberry.Rings.Util.RoundingMode;
using static Cc.Redberry.Rings.Util.Associativity;
using static Cc.Redberry.Rings.Util.Operator;
using static Cc.Redberry.Rings.Util.TokenType;
using static Cc.Redberry.Rings.Util.SystemInfo;

namespace Cc.Redberry.Rings.Util
{
    /// <summary>
    /// </summary>
    /// <remarks>@since1.0</remarks>
    public class RandomDataGenerator : RandomDataGenerator
    {
        private static readonly long serialVersionUID = 1;
        public RandomDataGenerator(RandomGenerator rand) : base(rand)
        {
        }

        public override int NextInt(int lower, int upper)
        {
            if (lower == upper)
                return lower;
            return base.NextInt(lower, upper);
        }

        public override long NextLong(long lower, long upper)
        {
            if (lower == upper)
                return lower;
            return base.NextLong(lower, upper);
        }
    }
}