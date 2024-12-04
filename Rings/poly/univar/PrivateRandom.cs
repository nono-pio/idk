namespace Rings.poly.univar;

static class PrivateRandom {

    /** thread local instance of random */
    private static ThreadLocal<RandomGenerator> ThreadLocalRandom
            = new ThreadLocal<RandomGenerator>(() => new Well1024a(0x7f67fcad528cfae9L));

    /** Returns random generator associated with current thread */
    static RandomGenerator getRandom() {
        return ThreadLocalRandom.Value;
    }
}
