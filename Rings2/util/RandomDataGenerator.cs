

namespace Cc.Redberry.Rings.Util
{
    /// <summary>
    /// </summary>
    /// <remarks>@since1.0</remarks>
    public class RandomDataGenerator
    {
        public static readonly long serialVersionUID = 1;
        
        private readonly Random rnd;
        public RandomDataGenerator(Random rand)
        {
            this.rnd = rand;
        }

        public int NextInt(int lower, int upper)
        {
            if (lower == upper)
                return lower;
            return rnd.Next(lower, upper);
        }

        public long NextLong(long lower, long upper)
        {
            if (lower == upper)
                return lower;
            return rnd.NextInt64(lower, upper);
        }
    }
}