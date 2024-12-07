

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
        private static readonly ThreadLocal<Random> ThreadLocalRandom = new ThreadLocal<Random>(() => new Random(0x7f67fcad));
        /// <summary>
        /// Returns random generator associated with current thread
        /// </summary>
        public static Random GetRandom()
        {
            return ThreadLocalRandom.Value;
        }
    }
}